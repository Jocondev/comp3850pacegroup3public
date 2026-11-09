using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerQuestionaireObject : UdonSharpBehaviour
{
    [SerializeField] private Collider placementZone;
    [SerializeField] private string fieldnName;
    [SerializeField] private PetObjectV2 pet;
    private bool canSnap;

    void Start()
    {
        canSnap = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Networking.IsOwner(gameObject))
        {
            return;
        }

        if(other == placementZone)
        {
            canSnap = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!Networking.IsOwner(gameObject))
        {
            return;
        }

        if (other == placementZone)
        {
            canSnap = false;
        }
    }

    public override void OnDrop()
    {
        if (!Networking.IsOwner(gameObject) || !canSnap)
        {
            return;
        }

        transform.position = placementZone.gameObject.transform.position;
        transform.rotation = placementZone.gameObject.transform.rotation;

        pet.changeColour(fieldnName);
    }
}
