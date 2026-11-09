using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

enum State1
{
    Idle,
    Follow,
    curious,
    dance
}

public class PetObject : UdonSharpBehaviour
{
    [SerializeField] GameObject pet;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float followRange;
    [SerializeField] private float stopRange;
    [SerializeField] private float jumpForce;
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private Animator petAnimator;
    private float distance;
    private State1 currentState;
    private GameObject curiosTarget;

    private VRCPlayerApi player;

    private bool hasJumped = false;

    void Start()
    {
        player = Networking.LocalPlayer;

        currentState = State1.Idle;
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case State1.Idle:
                IdleState();
                break;

            case State1.Follow:
                FollowState();
                break;

            case State1.curious:
                CuriousState();
                break;
            case State1.dance:
                DanceState();
                break;
        }
    }

    private void DanceState()
    {
        if(!hasJumped)
        {
            agent.enabled = false;

            rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            hasJumped = true;
        }
        
    }


    private void CuriousState()
    {
        distance = Vector3.Distance(pet.transform.position, curiosTarget.transform.position);
        Debug.Log(distance);

        if (distance <= 1.5f)
        {
            agent.ResetPath();
            currentState = State1.dance;
            return;
        }

        NavMeshHit hit;

        if (NavMesh.SamplePosition(curiosTarget.transform.position, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }


    private void IdleState()
    {
        petAnimator.SetBool("Walking", false);
        
        distance = Vector3.Distance(pet.transform.position,player.GetPosition());

        Vector3 targetPosition = player.GetPosition();
        targetPosition.y = transform.position.y;

        transform.LookAt(targetPosition);

        if (distance > followRange)
        {
            currentState = State1.Follow;
        }
    }

    private void FollowState()
    {
        petAnimator.SetBool("Walking", true);

        distance = Vector3.Distance(pet.transform.position, player.GetPosition());

        if (distance <= stopRange)
        {
            agent.ResetPath();
            currentState = State1.Idle;
            return;
        }

        NavMeshHit hit;

        if (NavMesh.SamplePosition(player.GetPosition(), out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Pet"))
        {
            curiosTarget = other.gameObject;
            currentState = State1.curious;
            Debug.Log("Hi Pet!!!!");
        }
    }
}
