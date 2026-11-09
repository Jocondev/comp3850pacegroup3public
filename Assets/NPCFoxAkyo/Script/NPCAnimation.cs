
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

public class NPCAnimation : UdonSharpBehaviour
{
    [SerializeField]private Animator animator;
    [SerializeField] private NPCStatus npcStatus;               //各種情報
    [SerializeField] private NavMeshAgent navMeshAgent;

    // 初期化
    public  void Start()
    {
    }

    private void Update()
    {
        animator.SetFloat("Speed", navMeshAgent.velocity.magnitude);
    }

}