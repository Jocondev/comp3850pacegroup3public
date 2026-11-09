
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SequencePuzzle : UdonSharpBehaviour
{
    [SerializeField] private GameObject[] placedObjects;
    [SerializeField] private GameObject[] correctObjects;


    public void SetPlacedObject(int index, GameObject obj)
    {
        placedObjects[index] = obj;
        CheckPuzzle();
    }

    private void CheckPuzzle()
    {
        if (placedObjects.Length != correctObjects.Length)
        {
            return;
        }

        for (int i = 0; i < correctObjects.Length; i++)
        {
            if (placedObjects[i] != correctObjects[i])
            {
                Debug.Log("Nope!");
                return;
            }
        }
        Debug.Log("Correct!");

    }
}
