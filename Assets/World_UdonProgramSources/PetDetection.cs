using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using TMPro;

public class PetDetection : UdonSharpBehaviour
{
    [SerializeField] private PetObjectV2 petObject;
    [SerializeField] private TMP_Text errorText;

    [Header("Scan Settings")]
    [SerializeField] private Collider scanCollider;
    [SerializeField] private float scanInterval;
    [SerializeField] private float scanDuration;

    [Header("Match History")]
    [SerializeField] private int matchMemory;
    private DataList matchedPets = new DataList();

    private DataList foundPets = new DataList();
    private bool isScanning = false;

    private int matchMode = 0;

    void Start()
    {
        if (Utilities.IsValid(errorText))
        {
            errorText.text = "";
        }

        // ShowDebug("[PetDetection] Script started.");

        scanCollider.enabled = false;

        SendCustomEventDelayedSeconds(nameof(StartScan), scanInterval);
    }

    public void SetMatchMode(int mode)
    {
        matchMode = mode;

        if (matchMode == 0)
        {
            // ShowDebug("Match mode: most shared interests");
        }
        else
        {
            // ShowDebug("Match mode: closest");
        }
    }

    public void StartScan()
    {
        if (!Utilities.IsValid(petObject) || !Networking.IsOwner(petObject.gameObject) || petObject.IsBusy)
        {
            SendCustomEventDelayedSeconds(nameof(StartScan), scanInterval);
            return;
        }

        foundPets.Clear();

        isScanning = true;
        scanCollider.enabled = true;
        // ShowDebug("Scan started.");

        SendCustomEventDelayedSeconds(nameof(FinishScan), scanDuration);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isScanning)
        {
            return;
        }

        if (other.gameObject.layer != 23)
        {
            return;
        }

        Transform current = other.transform;
        PetObjectV2 otherPet = null;

        while (Utilities.IsValid(current))
        {
            otherPet = current.GetComponent<PetObjectV2>();

            if (Utilities.IsValid(otherPet))
            {
                break;
            }

            current = current.parent;
        }

        if (!Utilities.IsValid(otherPet))
        {
            return;
        }

        if (otherPet == petObject)
        {
            return;
        }

        if (foundPets.Contains(otherPet))
        {
            return;
        }

        foundPets.Add(otherPet);
    }

    public void FinishScan()
    {
        isScanning = false;
        scanCollider.enabled = false;

        if (petObject.IsBusy)
        {
            foundPets.Clear();
            SendCustomEventDelayedSeconds(nameof(StartScan), scanInterval);
            return;
        }

        int bestIndex = -1;
        int bestScore = 0;
        float bestDistance = 0f;

        for (int i = 0; i < foundPets.Count; i++)
        {
            PetObjectV2 pet = (PetObjectV2)foundPets[i].Reference;

            if (!Utilities.IsValid(pet))
            {
                continue;
            }

            if (pet.IsBusy)
            {
                continue;
            }

            if (matchedPets.Contains(pet))
            {
                continue;
            }

            int score = CountSharedInterests(pet);

            if (score == 0)
            {
                continue;
            }

            float distance = GetDistanceTo(pet);

            bool isBetter;

            if (matchMode == 0)
            {
                isBetter = score > bestScore || (score == bestScore && distance < bestDistance);
            }
            else
            {
                isBetter = bestIndex == -1 || distance < bestDistance;
            }

            if (isBetter)
            {
                bestScore = score;
                bestDistance = distance;
                bestIndex = i;
            }
        }

        if (bestIndex == -1)
        {
            // ShowDebug("No shared interests.");
        }
        else
        {
            PetObjectV2 bestPet = (PetObjectV2)foundPets[bestIndex].Reference;

            RememberMatch(bestPet);
            petObject.dancePet(bestPet.PetModel);
        }

        SendCustomEventDelayedSeconds(nameof(StartScan), scanInterval);
    }

    private void RememberMatch(PetObjectV2 pet)
    {
        matchedPets.Add(pet);

        // Forget the oldest match once the list is full.
        while (matchedPets.Count > matchMemory)
        {
            matchedPets.RemoveAt(0);
        }
    }

    private float GetDistanceTo(PetObjectV2 otherPet)
    {
        return Vector3.Distance(petObject.PetModel.transform.position, otherPet.PetModel.transform.position);
    }

    private int CountSharedInterests(PetObjectV2 otherPet)
    {
        int count = 0;

        if (petObject.Interest1 && otherPet.Interest1) count++;
        if (petObject.Interest2 && otherPet.Interest2) count++;
        if (petObject.Interest3 && otherPet.Interest3) count++;
        if (petObject.Interest4 && otherPet.Interest4) count++;
        if (petObject.Interest5 && otherPet.Interest5) count++;
        if (petObject.Interest6 && otherPet.Interest6) count++;
        if (petObject.Interest7 && otherPet.Interest7) count++;
        if (petObject.Interest8 && otherPet.Interest8) count++;
        if (petObject.Interest9 && otherPet.Interest9) count++;
        if (petObject.Interest10 && otherPet.Interest10) count++;
        if (petObject.Interest11 && otherPet.Interest11) count++;
        if (petObject.Interest12 && otherPet.Interest12) count++;
        if (petObject.Interest13 && otherPet.Interest13) count++;
        if (petObject.Interest14 && otherPet.Interest14) count++;
        if (petObject.Interest15 && otherPet.Interest15) count++;
        if (petObject.Interest16 && otherPet.Interest16) count++;
        if (petObject.Interest17 && otherPet.Interest17) count++;
        if (petObject.Interest18 && otherPet.Interest18) count++;
        if (petObject.Interest19 && otherPet.Interest19) count++;
        if (petObject.Interest20 && otherPet.Interest20) count++;

        return count;
    }

    private void OnTriggerExit(Collider other)
    {
        
    }

    private void ShowDebug(string message)
    {
        Debug.Log(message);

        if (Utilities.IsValid(errorText))
        {
            errorText.text += message + "\n";
        }
    }

    public void ClearMatchHistory()
    {
        matchedPets.Clear();
        ShowDebug("Match history cleared.");
    }

    public void ToggleBusy()
    {
        petObject.ToggleBusy();
    }
}