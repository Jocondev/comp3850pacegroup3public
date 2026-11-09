using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class NPCSearch : UdonSharpBehaviour
{
    [SerializeField] NPCStatus npcStatus;

    public LayerMask playerLayer; // プレイヤーのレイヤー
    public LayerMask objectLayer; // オブジェクトのレイヤー

    // プレイヤーが範囲内に入ってきた場合
    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {

       
        // プレイヤーがローカルプレイヤーであるかを確認
        if (player != null && player.isLocal)
        {



            // 視界内であればターゲットとして設定
            if (npcStatus.GetCurrentState() == 0 || npcStatus.GetCurrentState() == 1)
            {


                npcStatus.SetTargetPlayer(player);
                npcStatus.SetCurrentState(3);
               // }
            }
        }
    }

    // オブジェクトが範囲内に入ってきた場合
    private void OnTriggerEnter(Collider other)
    {
        // オブジェクトが範囲内に入ってきた場合
        if ((objectLayer.value & (1 << other.gameObject.layer)) > 0)
        {

                // オブジェクトのIDを取得して設定
                int objectID = other.gameObject.GetInstanceID();
                npcStatus.SetCurrentTargetObjectID(objectID);

        }
    }

}