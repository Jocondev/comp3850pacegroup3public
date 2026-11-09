
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


/// <summary>
/// NPC の戦闘時行動ステート
/// </summary>
public class NPCCombatAI : UdonSharpBehaviour
{
    public float attackRange = 5f; // 攻撃範囲
    public float attackCooldown = 2f; // 攻撃クールダウン時間
    private float lastAttackTime = 0f; // 最後の攻撃時間

    public Transform target; // ターゲット（プレイヤーや敵NPC）

    // 戦闘AIの思考と行動
    public void ThinkAndAct()
    {
        // ターゲットが存在しない場合は行動しない
        if (target == null)
        {
            return;
        }

        // 攻撃範囲に入った場合、攻撃を実行
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            Attack();
        }
        else
        {
            MoveToTarget();
        }
    }

    // 攻撃の処理
    private void Attack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            // 攻撃アニメーションなどをここに追加
            //Debug.Log("NPC is attacking the target");

            // 攻撃後にクールダウンを設定
            lastAttackTime = Time.time;
        }
    }

    // ターゲットに向かって移動
    private void MoveToTarget()
    {
        // ターゲットに向かって移動
        //Debug.Log("NPC is moving towards the target");
        // ここにNavMeshを使った移動処理を追加
    }
}
