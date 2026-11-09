using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]

//このクラスがアタッチされているオブジェクトは
//基本的にオーナーを切り替えない。
//オーナーが退室するまで同期の値変更は一人だけが行う

public class NPCSync : UdonSharpBehaviour
{
    //[SerializeField] private ParticleLaunch particle; // 同期タイミング確認用パーティクル

    [Header("NPC本体とステータス参照")]
    [SerializeField] private GameObject npc;          // 同期対象のNPC
    [SerializeField] private NPCStatus npcStatus;     // 必要に応じて参照

    [Header("同期間隔（秒）")]
    [SerializeField] private float syncInterval = 10f; // データ送信間隔（秒）
    private float syncTimer = 0f;                      // タイマー用

    [Header("非オーナー：同期反映の閾値")]
    [SerializeField] private float positionThreshold = 10f; // これ以上離れていたら反映

    [UdonSynced(UdonSyncMode.None)] private Vector3 syncedPosition;

    void Update()
    {
        if (Networking.IsOwner(gameObject))
        {
            syncTimer += Time.deltaTime;
            if (syncTimer >= syncInterval)
            {
                syncTimer = 0f;

                syncedPosition = npc.transform.position;
                RequestSerialization();

                // パーティクル再生（送信タイミング確認用）
                //if (particle != null) particle.LaunchParticle2();
            }
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        // オーナーのみ：JOINしてきたプレイヤーに現在位置を即時送信
        if (Networking.IsOwner(gameObject)) 
        {
            syncedPosition = npc.transform.position;
            RequestSerialization(); // 最新位置を同期

            //if (particle != null) particle.LaunchParticle2(); // パーティクル再生（送信確認）
        }
    }

    public override void OnDeserialization()
    {
        if (npc == null) return;

        float distance = Vector3.Distance(npc.transform.position, syncedPosition);
        if (distance > positionThreshold)
        {
            npc.transform.position = syncedPosition;

            // パーティクル再生（同期受信確認用）
            //if (particle != null) particle.NetworkEventLaunchParticle2();
        }
    }


}