
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerObjectVisibility : UdonSharpBehaviour
{
    [SerializeField] private GameObject[] objects;

    void Start()
    {
        VRCPlayerApi owner = Networking.GetOwner(gameObject);

        if (!Utilities.IsValid(owner))
        {
            return;
        }

        bool isLocalOwner = owner.playerId == Networking.LocalPlayer.playerId;

        for(int i=0; i<objects.Length; i++)
        {
            if (Utilities.IsValid(objects[i]))
            {
                objects[i].SetActive(isLocalOwner);
            }
        }
    }
}
