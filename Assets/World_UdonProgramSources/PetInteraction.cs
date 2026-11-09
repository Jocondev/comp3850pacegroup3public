using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class PetInteraction : UdonSharpBehaviour
{
    [SerializeField] private GameObject petUI;
    [SerializeField] private GameObject visitorUI;
    [SerializeField] private GameObject playerObject;

    void Start()
    {
        petUI.SetActive(false);
        visitorUI.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!visitorUI.activeSelf) return;

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (!Utilities.IsValid(localPlayer)) return;

        Vector3 headPosition = localPlayer.GetTrackingData(
            VRCPlayerApi.TrackingDataType.Head
        ).position;

        Vector3 direction = visitorUI.transform.position - headPosition;

        if (direction.sqrMagnitude > 0.001f)
        {
            visitorUI.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    public override void Interact()
    {
        if (Networking.IsOwner(playerObject))
        {
            petUI.SetActive(!petUI.activeSelf);
        }
        else
        {
            visitorUI.SetActive(!visitorUI.activeSelf);
        }
    }

    public void CloseUI()
    {
        petUI.SetActive(false);
        visitorUI.SetActive(false);
    }
}