using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// NPCの移動を制御するためのスクリプトです。
/// 巡回・追跡・付き添い・スタック検出などを行います。
/// </summary>
public class NavMeshHandler : UdonSharpBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent; // UnityのNavMesh移動用コンポーネント
    [SerializeField] private NPCStatus npcStatus;       // NPCの状態管理クラス

    // スタック検出用パラメータ
    [SerializeField] private float stuckCheckInterval = 3.0f;       // スタック検出の間隔（秒）
    [SerializeField] private float stopCheckInterval = 20.0f;       // 長時間停止チェックの間隔（秒）
    [SerializeField] private float stuckDistanceThreshold = 0.2f;   // 「ほとんど動いていない」と判断する距離

    private float currentSpeed = 0f;               // 現在の移動速度（補間中）
    [SerializeField] private float speedLerpTime = 5.0f; // スピード補間の速さ（大きいほどゆっくり）

    private Vector3 lastPosition;     // 最後に記録した位置
    private float lastCheckTime;      // 最後にチェックした時間
    private int playerID = 0;         // ターゲットプレイヤーID
    private VRCPlayerApi targetPlayer = null; // ターゲットプレイヤー
    private Vector3 targetPosition;   // 現在のターゲット位置

    // 初期化処理
    public void Start()
    {
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgentが設定されていません。");
        }
        if (npcStatus == null)
        {
            Debug.LogError("NPCStatusが設定されていません。");
        }
    }

    /// <summary>
    /// 指定された位置に向かって移動を開始します。
    /// </summary>
    public void MoveTo(Vector3 targetPosition)
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(targetPosition);
        }
    }

    /// <summary>
    /// 現在の移動を停止します。
    /// </summary>
    public void StopMoving()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
        }
    }

    /// <summary>
    /// 現在移動中かどうかを判定します。
    /// </summary>
    public bool IsMoving()
    {
        return navMeshAgent.velocity.sqrMagnitude > 0f;
    }

    /// <summary>
    /// 現在の目的地に到達したかどうかを判定します。
    /// </summary>
    public bool HasArrived()
    {
        return !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
    }

    /// <summary>
    /// NPCの巡回行動を処理します。
    /// </summary>
    public void Patrolling()
    {
        if (npcStatus == null || navMeshAgent == null) return;

        // 巡回速度に設定
        navMeshAgent.speed = npcStatus.GetSpeed();

        // ターゲット位置が変わったら再設定
        if (targetPosition != npcStatus.GetCurrentTargetPoint())
        {
            targetPosition = npcStatus.GetCurrentTargetPoint();
            MoveTo(targetPosition);
        }

        // 到着したら新しい目的地へ
        if (Networking.IsOwner(gameObject) && HasArrived())
        {
            SetNewRandomDestination();
        }

        // スタックチェック
        StuckCheck();
    }

    /// <summary>
    /// ターゲットプレイヤーを追跡します。
    /// </summary>
    public void Chasing()
    {
        if (npcStatus == null || navMeshAgent == null) return;

        // ターゲットプレイヤーの更新
        if (playerID != npcStatus.GetTargetPlayerID())
        {
            playerID = npcStatus.GetTargetPlayerID();
            targetPlayer = VRCPlayerApi.GetPlayerById(playerID);
        }

        if (targetPlayer != null)
        {
            // プレイヤーの頭の位置を取得
            targetPosition = targetPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        }

        // 有効なターゲットがある場合のみ移動
        if (targetPosition != Vector3.zero)
        {
            MoveTo(targetPosition);
        }
        else
        {
            Debug.LogWarning("追跡対象が無効です。");
        }

        // スタックチェック
        StuckCheck();
    }

    /// <summary>
    /// プレイヤーに付き添う行動です。
    /// </summary>
    public void Accompanying()
    {
        if (npcStatus == null || navMeshAgent == null) return;

        // ターゲットプレイヤーを取得
        if (playerID != npcStatus.GetTargetPlayerID())
        {
            playerID = npcStatus.GetTargetPlayerID();
            targetPlayer = VRCPlayerApi.GetPlayerById(playerID);
        }

        if (targetPlayer != null)
        {
            // ターゲットの位置取得
            targetPosition = targetPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        }
        else
        {
            // ターゲットが無効なら巡回状態に戻す
            if (Networking.IsOwner(gameObject))
            {
                npcStatus.SetCurrentState(0); // IDLE状態に戻す
            }
            Debug.Log("ターゲットプレイヤーがいないので待機状態に移行します。");
            return;
        }

        // ターゲットとの距離に応じて速度を変える（近くでは歩き、遠ければ走る）
        if (targetPosition != Vector3.zero)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            // 距離に応じた目標速度を計算（歩き〜走りの間で補間）
            float speedFactor = Mathf.InverseLerp(2.0f, 6.0f, distanceToTarget);
            float targetSpeed = Mathf.Lerp(npcStatus.GetSpeed(), npcStatus.GetRunningSpeed(), speedFactor);

            // 現在の速度を滑らかに目標に近づける
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedLerpTime);

            // NavMeshAgent に反映
            navMeshAgent.speed = currentSpeed;
            MoveTo(targetPosition);
        }
        else
        {
            Debug.LogWarning("付き添い対象が無効です。");
        }

        // 一定時間停止していたら巡回に戻す
        StopCheck();
    }

    /// <summary>
    /// NavMesh上のランダムな地点を目的地に設定します。
    /// </summary>
    private void SetNewRandomDestination()
    {
        if (Networking.IsOwner(gameObject))
        {
            npcStatus.SetCurrentTargetPoint(GetRandomNavMeshPoint());
        }

        navMeshAgent.speed = npcStatus.GetSpeed();
    }

    /// <summary>
    /// NavMesh上のランダムな有効地点を取得します。
    /// </summary>
    private Vector3 GetRandomNavMeshPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// NPCがスタックしているかを一定時間ごとに確認します。
    /// </summary>
    private void StuckCheck()
    {
        float currentTime = Time.time;

        if (currentTime - lastCheckTime > stuckCheckInterval)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (distanceMoved < stuckDistanceThreshold)
            {
                if (Networking.IsOwner(gameObject))
                {
                    SetNewRandomDestination();
                    Debug.Log("スタックしたので別の経路を探します。");
                }
            }

            lastPosition = transform.position;
            lastCheckTime = currentTime;
        }
    }

    /// <summary>
    /// NPCが長時間動かないときに巡回に戻す処理です。
    /// </summary>
    private void StopCheck()
    {
        float currentTime = Time.time;

        if (currentTime - lastCheckTime > stopCheckInterval)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (distanceMoved < stuckDistanceThreshold)
            {
                if (Networking.IsOwner(gameObject))
                {
                    npcStatus.SetCurrentState(1); // 巡回状態へ
                }
                Debug.Log("動かないので巡回に移行します。");
            }

            lastPosition = transform.position;
            lastCheckTime = currentTime;
        }
    }
}