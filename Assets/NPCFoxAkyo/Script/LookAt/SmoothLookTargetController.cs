using System.Net.NetworkInformation;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;



/// <summary>
/// 視線制御の状態を表す列挙型。
/// UdonSharpではクラス外に置く必要があります。
/// </summary>
public enum LookState
{
    None,
    BeingPetted,
    FollowGaze_Delayed,
    FollowGaze_Instant
}

/// <summary>
/// 状態ごとのターゲット位置に滑らかに遷移させ、
/// LookAt用の位置とウェイトを提供するコントローラ。
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class SmoothLookTargetController : UdonSharpBehaviour
{

    [SerializeField] private NPCStatus npcStatus;

    [Header("補間速度の設定")]
    [Tooltip("ターゲット位置の補間速度（大きいほどすぐに向く）")]
    public float positionLerpSpeed = 4.0f;

    [Tooltip("LookAtWeight の補間速度（視線ON/OFF時の切り替え）")]
    public float weightLerpSpeed = 4.0f;

    [Header("現在の視線状態")]
    [Tooltip("現在の視線制御状態")]
    public LookState currentState = LookState.None;

    // 前のフレームでの状態を記録するための変数
    private LookState previousState = LookState.None;

    // 各状態ごとのターゲット位置をキャッシュしておく変数
    private Vector3 targetPetted;
    private Vector3 targetGazeDelayed;
    private Vector3 targetGazeInstant;

    // 実際にLookAt対象とする補間済みの現在位置
    private Vector3 currentTargetPosition;

    // LookAtの強さ（Animatorなどに使用可能）
    private float currentLookAtWeight;

    //視線追従フラグ
    private bool isGaze = true;


    Vector3 Offset; // 上方向にオフセット

    /// <summary>
    /// 撫で状態用のターゲット位置を設定します。
    /// </summary>
    public void SetGazeFlag(bool flag)
    {
        isGaze = flag;
    }

    /// <summary>
    /// 撫で状態用のターゲット位置を設定します。
    /// </summary>
    public void SetPettedTarget(Vector3 position)
    {
        targetPetted = position;
    }

    /// <summary>
    /// 遅延視線追従用のターゲット位置を設定します。
    /// </summary>
    public void SetGazeDelayedTarget(Vector3 position)
    {
        targetGazeDelayed = position;
    }

    /// <summary>
    /// 即時視線追従用のターゲット位置を設定します。
    /// </summary>
    public void SetGazeInstantTarget(Vector3 position)
    {
        targetGazeInstant = position;
    }

    /// <summary>
    /// 現在補間済みのターゲット位置を取得します。
    /// LookAt処理で使用できます。
    /// </summary>
    public Vector3 GetCurrentTargetPosition()
    {
        return currentTargetPosition;
    }

    /// <summary>
    /// 現在のLookAtWeightを取得します。
    /// Animatorなどで使用する値です。
    /// </summary>
    public float GetCurrentLookAtWeight()
    {
        return currentLookAtWeight;
    }

    /// <summary>
    /// 毎フレーム、ターゲット位置とウェイトの補間処理を行います。
    /// </summary>
    private void Update()
    {
        // 状態に応じた「次のターゲット位置」を決定する
        Vector3 nextTarget;
        switch (currentState)
        {
            case LookState.BeingPetted:
                nextTarget = targetPetted;
                //Debug.Log("撫でられ中");
                break;
            case LookState.FollowGaze_Delayed:
                nextTarget = targetGazeDelayed;
                //Debug.Log("遅延あり");
                break;
            case LookState.FollowGaze_Instant:
                nextTarget = targetGazeInstant;
                //Debug.Log("遅延なし！！！！");
                break;
            default:
                Offset = Vector3.up * 0.8f; // 上方向にオフセット
                // 視線対象がない場合は、正面を見る（回避用）
                nextTarget = transform.position + transform.forward + Offset;
                break;
        }


        if (!(npcStatus.GetCurrentState() == 2 || npcStatus.GetCurrentState() == 3 || npcStatus.GetCurrentState() == 5)) 
        { 
            SetGazeFlag(false);
        }

        // 現在のターゲット位置へなめらかに補間
        currentTargetPosition = Vector3.Lerp(currentTargetPosition, nextTarget, Time.deltaTime * positionLerpSpeed);

        // 視線追従のON/OFFの変化を確認し、必要に応じてLookAtWeightを補間


        float targetWeight = isGaze ? 1.0f : 0.0f;
        currentLookAtWeight = Mathf.Lerp(currentLookAtWeight, targetWeight, Time.deltaTime * weightLerpSpeed);


        // 前回の状態を記録
        previousState = currentState;
    }

    /// <summary>
    /// 指定した状態が視線追従状態かどうかを判定します。
    /// </summary>
    private bool IsGazeState(LookState state)
    {
        return state == LookState.FollowGaze_Delayed || state == LookState.FollowGaze_Instant;
    }
}