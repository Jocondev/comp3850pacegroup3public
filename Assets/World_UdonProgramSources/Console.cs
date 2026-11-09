using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Console : UdonSharpBehaviour
{
    [SerializeField] private GameObject tv;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material greenMaterial;

    void Start()
    {
        
    }

    public void ButtonPressed(int buttonID)
    {
        Debug.Log("Button pressed: " + buttonID);

        if (buttonID == 0)
        {
            Debug.Log("Blue button");
            tv.GetComponent<MeshRenderer>().material = blueMaterial;
        }
        else if (buttonID == 1)
        {
            Debug.Log("Red button");
            tv.GetComponent<MeshRenderer>().material = redMaterial;
        }
        else if (buttonID == 2)
        {
            Debug.Log("Green button");
            tv.GetComponent<MeshRenderer>().material = greenMaterial;
        }
    }
}
