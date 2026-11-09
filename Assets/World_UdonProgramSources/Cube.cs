using UdonSharp;
using UnityEngine;

public class Cube : UdonSharpBehaviour
{
    [SerializeField] private Collider[] placementZones;

    private bool canSnap = false;
    private Transform currentSnapPoint;
    private int currentZoneIndex = -1;
    [SerializeField] private SequencePuzzle sp;

    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < placementZones.Length; i++)
        {
            if (other == placementZones[i])
            {
                canSnap = true;
                currentSnapPoint = other.transform;
                currentZoneIndex = i;
                return;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == currentSnapPoint)
        {
            canSnap = false;
            currentSnapPoint = null;
            currentZoneIndex = -1;
        }
    }

    public override void OnDrop()
    {
        if (!canSnap || currentSnapPoint == null)
        {
            return;
        }

        transform.position = currentSnapPoint.position;
        transform.rotation = currentSnapPoint.rotation;
        sp.SetPlacedObject(currentZoneIndex, gameObject);
    }
}