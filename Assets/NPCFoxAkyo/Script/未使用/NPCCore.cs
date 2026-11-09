
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class NPCCore : UdonSharpBehaviour
{
    // NPC関連のコンポーネントを格納
    //コンポーネントはプロパティで指定して取得
    [SerializeField] private NPCStateHandler npcStateHandler;   //ステート駆動による状態管理

    [SerializeField] private NPCEventHandler npcEventHandler;   //攻撃を受けたり何かを発見するなど、主にコライダーをトリガーにしたイベントなど
    [SerializeField] private NPCSearch npcSearch;               //コライダーをトリガーとしたプレイヤーや指定のオブジェクトを見つける判定
    [SerializeField] private DamageHandler damageHandler;       //コライダーをトリガーとした攻撃を受けた時の判定
    [SerializeField] private NPCCombatAI npcCombatAI;           //戦闘時の行動AI
    [SerializeField] private NavMeshHandler navMeshHandler;     //NavMesh関係の処理
    [SerializeField] private NPCAnimation npcAnimation;         //現在のステートに対応したアニメーションを再生する
    [SerializeField] private NPCSync npcSync;                   //現在のTransform座標、NavMesh用の目的地、現在のステート、移動速度などをオーナーから他のプレイヤー同期する



    // OnStart()で初期化
    private void Start()
    {

    }

    // OnUpdate()で毎フレームの処理
    private void Update()
    {


        // 状態を管理
        if (npcStateHandler != null)
        {
            npcStateHandler.UpdateState();
        }
        else {
            Debug.Log("npcStateHandlerがないよ");
        }

    }

    // 外部からダメージを受けたときの処理
    public void HandleDamage()
    {
        // ダメージ時に戦闘状態に切り替え
        npcStateHandler.SetState(NPCState.COMBAT);
        npcStateHandler.SetIsCombat(true); // 戦闘フラグ
    }

    // ダメージ受けたときのイベント
    public void OnDamageEvent()
    {
        HandleDamage();
    }
}
