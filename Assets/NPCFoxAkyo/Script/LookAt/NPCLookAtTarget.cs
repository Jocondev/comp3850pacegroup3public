using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
/// <summary>
/// NPCが一定の条件下でターゲットに視線追尾する機能
/// </summary>
public class NPCLookAtTarget : UdonSharpBehaviour
{
    // --- 共通パラメータ ---
    [SerializeField] private NPCStatus npcStatus; // NPCのステータス管理用
    [SerializeField] private Animator animator;    // NPCのAnimatorコンポーネント
    [SerializeField] private NavMeshAgent navMeshAgent; // NPCのNavMeshAgentコンポーネント
    [SerializeField] private SmoothLookTargetController smoothLookTargetController;
    [SerializeField] private bool isPeting = true; // 撫で判定が有効かどうか

    // 撫で処理パラメータ
    public float activationDistance = 0.03f; // 撫で判定の距離
    public float lookAtSpeed = 3f;           // 視線追従速度
    public float returnSpeed = 2f;           // 視線を元に戻す速度

    // 内部用
    private bool isHandFollowing = false;    // 撫でているかどうか
                                             // 内部変数の追加
    private int frameCounter = 0;
    private int frameCounter2 = 0;
    // ターゲット追跡用
    private Vector3 delayedTargetPosition;   // 遅延ターゲット位置
    private Vector3 handPosition = Vector3.zero;            // 手の位置
    private Quaternion handRotation;         // 手の回転
    private VRCPlayerApi targetPlayer;       // 現在のターゲットプレイヤー
    private int playerID;                    // プレイヤーID
    private Vector3 target = Vector3.zero;                  // プレイヤーの頭のTransform
    private Transform headTransform;         // NPCの頭のTransform
    private VRCPlayerApi localPlayer;        // ローカルプレイヤー




    // 初期化
    private void Start()
    {
        localPlayer = Networking.LocalPlayer; // ローカルプレイヤーの取得

        if (animator == null)
        {
            animator = GetComponent<Animator>(); // Animatorコンポーネントの取得
            Debug.Log("アニメータがないよ");
        }

        if (npcStatus == null)
        {
            npcStatus = GetComponent<NPCStatus>(); // NPCStatusの取得
            Debug.Log("ステータスがないよ");

        }
        headTransform = animator.GetBoneTransform(HumanBodyBones.Chest); // NPCの頭のTransformを取得
                                                                        // NPCのheadTransformを少し上にオフセット
        Vector3 headOffset = Vector3.up * 0.1f; // 上方向に10cm（0.1m）のオフセット


    }

    // 視線追従の更新
    public void NPCLookAtTargetUpdate()
    {
        bool inCorrectState = npcStatus.GetCurrentState() == 2 || npcStatus.GetCurrentState() == 3 || npcStatus.GetCurrentState() == 5;

        smoothLookTargetController.SetGazeFlag(inCorrectState && npcStatus.GetEnableLookAt());
        
        //撫でられ機能がOFFになっている場合実行しない
        if (isPeting)
        {
            // NPCとローカルプレイヤーの距離を計算
            float distanceToNPC = Vector3.Distance(localPlayer.GetPosition(), transform.position);

            // ローカルプレイヤーがNPCの0.8メートル以内にいる場合のみ処理を実行
            if (distanceToNPC <= 0.8f)
            {
                frameCounter++;
                if (frameCounter >= 10 || handPosition == Vector3.zero)  // 10フレームごとに更新
                {
                    // ローカルプレイヤーの手の位置と手の向きを取得
                    handPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                    handRotation = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;

                    // 手のひら方向（手の向き）を取得
                    Vector3 handPalmDirection = handRotation * new Vector3(0f, -0.5f, 0.1f); // 手のひら方向（手の向き）を取得

                    // 手のひら方向に15cm（0.1m）のオフセットを追加
                    Vector3 offset = handPalmDirection * 0.05f;
                    handPosition += offset; // 手の位置にオフセットを加算
                    frameCounter = 0;
                }

                // 撫で判定の距離計算部分
                float distance = Vector3.Distance(headTransform.position + Vector3.up * 0.35f, handPosition);
                // 撫でているかどうか
                isHandFollowing = distance <= activationDistance;

                // 撫でているかつターゲットプレイヤーがローカルプレイヤーでない場合
                if (isHandFollowing && npcStatus.GetTargetPlayerID() != localPlayer.playerId)
                {
                    npcStatus.SetTargetPlayer(localPlayer); // ターゲットをローカルプレイヤーに設定
                }
            }

            // 撫でられ視線追従状態
            if (inCorrectState && isHandFollowing)
            {
                
                smoothLookTargetController.currentState = LookState.BeingPetted;
                smoothLookTargetController.SetPettedTarget(handPosition); // 視線ターゲットを手の位置に設定

            }
            else
            {
                if (npcStatus.GetDelayedLookAt())
                {
                    smoothLookTargetController.currentState = LookState.FollowGaze_Delayed;
                }
                else
                {
                    smoothLookTargetController.currentState = LookState.FollowGaze_Instant;
                }
            }

        }

        // ターゲット追尾処理
        if (!isHandFollowing)
        {
           
            // プレイヤーIDが変更された場合、ターゲットを更新
            if (playerID != npcStatus.GetTargetPlayerID())
            {
                playerID = npcStatus.GetTargetPlayerID();
                targetPlayer = VRCPlayerApi.GetPlayerById(playerID);
            }

            // ターゲットプレイヤーが有効な場合
            if (targetPlayer != null && targetPlayer.IsValid())
            {


                frameCounter2++;
                if (frameCounter2 >= 20 || target == Vector3.zero)  // 10フレームごとに更新
                {
                    // プレイヤーの頭の位置を取得
                    target = targetPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                    frameCounter2 = 0;
                }
                // 最初にターゲット位置を設定
                if (delayedTargetPosition == Vector3.zero)
                {
                    delayedTargetPosition = target;
                }

                // 遅延視線追従の処理
                //遅延視線追従かつ、走っていない
                if (npcStatus.GetDelayedLookAt() && navMeshAgent.velocity.magnitude < 2.0f )
                {
                   
                    smoothLookTargetController.currentState = LookState.FollowGaze_Delayed;
                    smoothLookTargetController.SetGazeDelayedTarget(target);
                }
                else
                {
                    
                    smoothLookTargetController.currentState = LookState.FollowGaze_Instant;
                    smoothLookTargetController.SetGazeInstantTarget(target);

                }

                // ターゲットとの距離を計算し、範囲内であれば視線追従
                float distanceToTarget = Vector3.Distance(transform.position, delayedTargetPosition);
                bool withinRange = distanceToTarget <= npcStatus.GetmaxLookAtDistance();
                
            }
        }
    }

    // Animator IKを使用して視線を制御
    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;


        animator.SetLookAtWeight(smoothLookTargetController.GetCurrentLookAtWeight(), 0.5f, 0.8f, 1.0f, 0.5f); // 通常時の視線重み
        animator.SetLookAtPosition(smoothLookTargetController.GetCurrentTargetPosition()); // 通常の視線ターゲット位置を設定


        
        // 体の傾き調整
        if (headTransform != null)
        {
            // Chest (胸) ボーンを取得し、そこを起点に体を傾ける
            Transform chestTransform = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (chestTransform != null)
            {
                // 視線ターゲットとの方向を計算
                Vector3 directionToTarget = (smoothLookTargetController.GetCurrentTargetPosition() - chestTransform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
                chestTransform.rotation = Quaternion.Slerp(chestTransform.rotation, lookRotation, Time.deltaTime * npcStatus.GetSmoothTime()); // 体をターゲットに合わせる
            }
        }
        
    }
}