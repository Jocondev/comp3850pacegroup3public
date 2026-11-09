
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class NPCEventHandler : UdonSharpBehaviour
{
    [SerializeField] private NPCCore npcCore;

    // ダメージイベントを受け取るメソッド
    public void OnDamageEvent()
    {
        // ダメージを受けた場合、NPCCore に連携して戦闘状態にする
        npcCore.HandleDamage(); // NPCCore の HandleDamage を呼び出す
    }

    // NPCが攻撃を受けた場合のイベント発行（例えば、攻撃検出後）
    public void OnAttackReceived()
    {
        // ダメージイベントが発生した場合に OnDamageEvent を呼び出す
        OnDamageEvent();
    }

    // イベントを他のシステム（例えばプレイヤーの攻撃）から受け取る例
    void SomeOtherEvent()
    {
        // 何らかの理由でイベントが発生した場合
        OnDamageEvent();
    }
}
