using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System.Collections;

/// <summary>
/// NPC の行動ステート
/// </summary>
/// 
/*
public enum NPCState
{
    Idle,           // 待機状態（何もしない）
    Patrolling,     // 巡回（エリアを移動）
    Chasing,        // 追跡（敵を追いかける）
    Accompanying,   // 同行 (特定の味方の近くへ移動する)
    Fleeing,        // 逃走（敵から離れる）
    Combat,         // 戦闘（攻撃を行う）
    Stunned,        // スタン（一定時間行動不能）
    Dead,           // 死亡（行動不能＆削除される）
    Damaged,        // ダメージ（ダメージを食らった）
    KnockedBack     // 爆発などで吹っ飛ばされた状態
}
*/

public static class NPCState
{
    public const int IDLE = 0;
    public const int PATROLLING = 1;
    public const int CHASING = 2;
    public const int ACCOMPANYING = 3;
    public const int FLEEING = 4;
    public const int COMBAT = 5;
    public const int STUNNED = 6;
    public const int DEAD = 7;
    public const int DAMAGED = 8;
    public const int KNOCKED_BACK = 9;
    public const int PET = 10;
}


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]

/// <summary>
/// NPC の行動を管理するクラス
/// </summary>
public class NPCStateHandler : UdonSharpBehaviour
{
    [SerializeField] private NPCCombatAI npcCombatAI;           // 戦闘時の行動AI
    [SerializeField] private NPCStatus npcStatus;               //各種情報
    [SerializeField] private NPCSearch npcSearch;               //
    [SerializeField] private NavMeshHandler navMeshHandler;               //
    [SerializeField] private NPCLookAtTarget npcLookatTarget;


[Tooltip("NPCの現在の状態")]
    private bool isDead = false;
    private bool isCombat = false;  // trueの間、戦闘状態を維持する。
    private float stunDuration = 5f; // スタン時間

    private float waitTime = 0.0f;

    private float accompanyDuration = 0.0f;  // 同行状態の経過時間
    private float maxDistance = 25.0f;       // 最大距離（25メートル）


    /// <summary>
    /// 初期状態の設定
    /// </summary>
    public void Start()
    {
        // 初期状態は待機
        SetState(NPCState.IDLE);
    }

    /// <summary>
    /// NPC の状態を変更する
    /// </summary>
    /// <param name="newState">変更したい状態</param>
    public void SetState(int newState)
    {
        // 死亡している、またはすでに同じ状態なら処理しない
        if (isDead || npcStatus.GetCurrentState() == newState) return;

        // 状態を更新
        npcStatus.SetCurrentState(newState);
        //Debug.Log($"NPC state changed to {currentState}");

        OnStateEnter(newState); // 状態変更時の処理をまとめる

        // 変更後の状態に応じた動作を実行
        UpdateState();


    }

    private void OnStateEnter(int state)
    {
        if (state == NPCState.COMBAT)
        {
            isCombat = true;
        }
        else if (state == NPCState.DEAD)
        {
            isDead = true;
        }
    }



    private void Update()
    {
        UpdateState();
    }

    /// <summary>
    /// NPC の行動を状態に応じて更新（NPCCore からも呼び出し可能）
    /// </summary>
    public void UpdateState()
    {
        switch (npcStatus.GetCurrentState())
        {
            case 0://Idle
                Idle();
                break;
            case 1://Patrolling
                Patrolling();

                break;
            case 2://Chasing
                Chasing();
                break;
            case 3://Accompanying
                Accompanying();
                break;
            case 4://Fleeing
                Fleeing();
                break;
            case 5://Combat
                Combat();
                break;
            case 6://Stunned
                Stunned();
                break;
            case 7://Dead
                Dead();
                break;
            case 8://Damaged:
                Damaged();
                break;
            case 9://KnockedBack:
                KnockedBack();
                break;
            case 10://Pet
                Pet(); 
                break;
        }




    }







    /// <summary>
    /// 待機状態
    /// </summary>
    private void Idle()
    {
        // Debug.Log("NPC is idle (waiting)");




        waitTime -= 0.01666f;
        if (Networking.IsOwner(gameObject))
        {
            if (waitTime <= 0.0f)
            {
                waitTime = Random.Range(5f, 20f);
                SetState(NPCState.PATROLLING);
            }

        }

    }

    /// <summary>
    /// 巡回状態
    /// </summary>
    private void Patrolling()
    {
        // Debug.Log("NPC is patrolling");

        navMeshHandler.Patrolling();

        waitTime -= 0.01666f;
        if (Networking.IsOwner(gameObject))
        {
            if (waitTime <= 0.0f)
            {
                waitTime = Random.Range(5f, 20f);
                SetState(NPCState.IDLE);
            }
        }
    }

    /// <summary>
    /// 追跡状態
    /// </summary>
    public void Chasing()
    {
        //Debug.Log("NPC is chasing the target");
        navMeshHandler.Chasing();
        // ここでプレイヤーに追いついた場合、戦闘を再開する
        if (isCombat)
        {
            //Debug.Log("NPC has caught up and resumes combat");
            npcStatus.SetCurrentState(NPCState.COMBAT); // 状態を直接変更（SetStateを呼ばない
            Combat();
        }
    }



    /// <summary>
    /// 同行状態
    /// </summary>
    public void Accompanying()
    {
        npcLookatTarget.NPCLookAtTargetUpdate();
        navMeshHandler.Accompanying();



    }

    /// <summary>
    /// 逃走状態
    /// </summary>
    private void Fleeing()
    {
        //Debug.Log("NPC is fleeing from danger");

    }




    /// <summary>
    /// スタン状態
    /// </summary>
    public void Stunned()
    {
        //Debug.Log("NPC is stunned and unable to move");

        // stunDuration秒後にStunEndを実行する
        SendCustomEventDelayedSeconds(nameof(StunEnd), stunDuration);
    }

    /// <summary>
    /// スタン終了
    /// </summary>
    public void StunEnd()
    {
        // isCombatがtrueなら戦闘を続行する
        if (isCombat)
        {
            //Debug.Log("NPC resumes combat after stun");
            npcStatus.SetCurrentState(NPCState.COMBAT); // 状態を直接変更（SetStateを呼ばない）
            Combat();
        }
        else
        {
            SetState(NPCState.IDLE);
        }
    }

    /// <summary>
    /// 死亡状態
    /// </summary>
    public void Dead()
    {
        //Debug.Log("NPC has died");
        isDead = true;
        SetState(NPCState.DEAD);
    }

    /// <summary>
    /// ダメージを食らった状態
    /// </summary>
    public void Damaged()
    {
        //Debug.Log("NPC has taken damage");

        // ダメージ後、戦闘状態がtrueなら戦闘状態に戻る
        if (isCombat)
        {
            //Debug.Log("NPC resumes combat after taking damage");
            SetState(NPCState.COMBAT);
            return;
        }

        // ダメージ後、少し待ってから元の状態に戻る（任意で待機状態に戻るなど）
        SetState(NPCState.IDLE);
    }

    /// <summary>
    /// 吹っ飛ばされた状態
    /// </summary>
    public void KnockedBack()
    {
        //Debug.Log("NPC is knocked back");

        // 吹っ飛ばされた後、戦闘状態がtrueなら戦闘状態に戻る
        if (isCombat)
        {
            //Debug.Log("NPC resumes combat after being knocked back");
            SetState(NPCState.COMBAT);
            return;
        }

        // 吹っ飛ばされた後、少し待ってから元の状態に戻る
        SendCustomEventDelayedSeconds(nameof(RecoverFromKnockback), 2f); // 2秒後に元の状態に戻る
    }

    /// <summary>
    /// 吹っ飛ばされた後の回復
    /// </summary>
    public void RecoverFromKnockback()
    {
        SetState(NPCState.IDLE);  // 吹っ飛ばされた後、待機状態に戻る
    }

    /// <summary>
    /// 戦闘状態
    /// </summary>
    private void Combat()
    {
        //Debug.Log("NPC enters combat mode");

        // 戦闘状態フラグをONにする
        isCombat = true;

        // 戦闘状態 → NPCCombatAI に制御を任せる
        if (npcCombatAI != null)
        {
            npcCombatAI.ThinkAndAct();
        }
    }

    private void Pet() 
    {
       
    }

    /// <summary>
    /// プレイヤーを見失った場合の処理
    /// </summary>
    public void LosePlayer()
    {
        //Debug.Log("NPC lost the player");

        // ここで戦闘継続条件を満たさなければ戦闘終了
        isCombat = false;
        SetState(NPCState.IDLE);
    }


    //===Getter Setter====================================================

    /// <summary>
    /// isCombatの状態を取得する
    /// </summary>
    public bool GetIsCombat() { return isCombat; }
    /// <summary>
    /// isCombatの状態を変更
    /// </summary>
    public void SetIsCombat(bool isCombat)
    {

        this.isCombat = isCombat;

        if (isCombat)
        {
            // 戦闘に移行する処理があればここに追加
        }
        else
        {
            // 戦闘終了時の処理があればここに追加
        }
    }
}
