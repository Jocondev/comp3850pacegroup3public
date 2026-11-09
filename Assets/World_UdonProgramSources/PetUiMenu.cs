
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class PetUiMenu : UdonSharpBehaviour
{
    private VRCPlayerApi player;

    [SerializeField] private GameObject menu;
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private Vector3 rotationOffset;

    void Start()
    {
        player = Networking.LocalPlayer;
    }

    void LateUpdate()
    {
        if (player == null || menu == null || !menu.activeSelf)
        {
            return;
        }

        VRCPlayerApi.TrackingData handData = player.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);

        menu.transform.position = handData.position + handData.rotation * positionOffset;

        menu.transform.rotation = handData.rotation * Quaternion.Euler(rotationOffset);
    }

    
    public override void InputJump(bool value, UdonInputEventArgs args)
    {
        if (value)
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        menu.SetActive(!menu.activeSelf);
    }
}
