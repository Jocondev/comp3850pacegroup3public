using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TestPet : UdonSharpBehaviour
{
    [SerializeField] int colour;

    public int Colour
    {
        get
        {
            return colour;
        }
    }

    void Start()
    {
        
    }
}
