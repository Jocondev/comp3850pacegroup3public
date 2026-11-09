using VRC.SDK3.Data;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GlobalPetManager : UdonSharpBehaviour
{
    private DataList petList = new DataList();
    [SerializeField] private GameObject stage;

    void Start()
    {
        
    }

    public void addPet(PetObjectV2 pet)
    {
        petList.Add(pet);
        Debug.Log("pet added" + pet);
    }

    public void removePet(PetObjectV2 pet)
    {
        petList.Remove(pet);
    }

    public void addStage()
    {
        
    }
}
