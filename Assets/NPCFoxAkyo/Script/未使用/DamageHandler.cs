using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DamageHandler : UdonSharpBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    private NPCSync npcSync;

    public void Start()
    {
        currentHealth = maxHealth;
    }

    public void SetNPCSync(UdonSharpBehaviour sync)
    {

        // syncがnullでないことを先に確認
        if (sync == null)
        {
            Debug.LogError("NPCSync is null! Make sure the object has the NPCSync script attached.");
            return;
        }

        if (sync != null)// && sync is NPCSync)
        {
            npcSync = (NPCSync)sync;
            Debug.Log("NPCSync successfully assigned in DamageHandler.");
        }
        else
        {
            Debug.LogError("Failed to assign NPCSync. Make sure the object has the NPCSync script attached.");
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log("Damage taken: " + damage + ", Current Health: " + currentHealth);

        if (npcSync != null)
        {
           
        }
        else
        {
            Debug.LogError("npcSync is null in TakeDamage!");
        }

        // 死亡判定はTakeDamage内で行わない
        if (currentHealth <= 0)
        {
            SendCustomEvent(nameof(Die)); // 死亡イベントの呼び出し
        }
    }

    public void Die()
    {
        if (currentHealth <= 0)
        {
            Debug.Log("NPC has died.");

            if (npcSync != null)
            {
               
            }

            // 死亡時の処理（アニメーションや削除）
            Networking.Destroy(gameObject); // オブジェクトの削除
        }
        else
        {
            Debug.LogWarning("NPC attempted to die with health still above 0.");
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void SetHealth(float health)
    {
        currentHealth = health;
        Debug.Log("HP Synced: " + currentHealth);
    }
}