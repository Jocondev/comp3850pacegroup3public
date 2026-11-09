using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Button : UdonSharpBehaviour
{
    [SerializeField] private Console console;
    [SerializeField] private int buttonID;

    void Start()
    {
        
    }

    public override void Interact()
    {
        Debug.Log("Button pressed!");
        console.ButtonPressed(buttonID);
    }
}
