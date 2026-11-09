using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using TMPro;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PetManager : UdonSharpBehaviour
{
    [SerializeField] private TMP_Dropdown petDropdown;
    [SerializeField] private Slider senioritySlider;
    [SerializeField] private TMP_Dropdown matchModeDropdown;
    [SerializeField] private PetDetection petDetection;
    [SerializeField] private PetObjectV2 pet;
    [SerializeField] private Toggle interest1;
    [SerializeField] private Toggle interest2;
    [SerializeField] private Toggle interest3;
    [SerializeField] private Toggle interest4;
    [SerializeField] private Toggle interest5;
    [SerializeField] private Toggle interest6;
    [SerializeField] private Toggle interest7;
    [SerializeField] private Toggle interest8;
    [SerializeField] private Toggle interest9;
    [SerializeField] private Toggle interest10;
    [SerializeField] private Toggle interest11;
    [SerializeField] private Toggle interest12;
    [SerializeField] private Toggle interest13;
    [SerializeField] private Toggle interest14;
    [SerializeField] private Toggle interest15;
    [SerializeField] private Toggle interest16;
    [SerializeField] private Toggle interest17;
    [SerializeField] private Toggle interest18;
    [SerializeField] private Toggle interest19;
    [SerializeField] private Toggle interest20;

    [SerializeField] private int maxInterests;
    private bool isReverting = false;


    private int selectedEar = 0;
    private int selectedSeniorLevel = 0;

    private int interestCount;

    void Start()
    {
        if (Utilities.IsValid(petDropdown))
        {
            selectedEar = petDropdown.value;
        }
        interestCount = 0;

        if (Utilities.IsValid(matchModeDropdown) && Utilities.IsValid(petDetection))
        {
            petDetection.SetMatchMode(matchModeDropdown.value);
        }
    }

    private bool TryChangeInterest(Toggle toggle)
    {
        if (isReverting)
        {
            return false;
        }

        if (toggle.isOn)
        {
            if (interestCount >= maxInterests)
            {
                isReverting = true;
                toggle.isOn = false;
                isReverting = false;
                return false;
            }

            interestCount++;
        }
        else
        {
            interestCount--;
        }

        return true;
    }

    public void PetDropDown()
    {
        selectedEar = petDropdown.value;
        pet.changeEar(selectedEar);
        // Debug.Log(selectedEar);
    }

    public void PetSeniorSlider()
    {
        selectedSeniorLevel = (int)senioritySlider.value;
        pet.changeRings(selectedSeniorLevel);
        // Debug.Log("using slider " + selectedSeniorLevel);
    }

    public void Interest1Changed()
    {
        if (TryChangeInterest(interest1) == false)
        {
            return;
        }
        pet.SetInterest1(interest1.isOn);
    }

    public void Interest2Changed()
    {
        if (TryChangeInterest(interest2) == false)
        {
            return;
        }
        pet.SetInterest2(interest2.isOn);
    }

    public void Interest3Changed()
    {
        if (TryChangeInterest(interest3) == false)
        {
            return;
        }
        pet.SetInterest3(interest3.isOn);
    }

    public void Interest4Changed()
    {
        if (TryChangeInterest(interest4) == false)
        {
            return;
        }
        pet.SetInterest4(interest4.isOn);
    }

    public void Interest5Changed()
    {
        if (TryChangeInterest(interest5) == false)
        {
            return;
        }
        pet.SetInterest5(interest5.isOn);
    }

    public void Interest6Changed()
    {
        if (TryChangeInterest(interest6) == false)
        {
            return;
        }
        pet.SetInterest6(interest6.isOn);
    }

    public void Interest7Changed()
    {
        if (TryChangeInterest(interest7) == false)
        {
            return;
        }
        pet.SetInterest7(interest7.isOn);
    }

    public void Interest8Changed()
    {
        if (TryChangeInterest(interest8) == false)
        {
            return;
        }
        pet.SetInterest8(interest8.isOn);
    }

    public void Interest9Changed()
    {
        if (TryChangeInterest(interest9) == false)
        {
            return;
        }
        pet.SetInterest9(interest9.isOn);
    }

    public void Interest10Changed()
    {
        if (TryChangeInterest(interest10) == false)
        {
            return;
        }
        pet.SetInterest10(interest10.isOn);
    }

    public void Interest11Changed()
    {
        if (TryChangeInterest(interest11) == false)
        {
            return;
        }
        pet.SetInterest11(interest11.isOn);
    }

    public void Interest12Changed()
    {
        if (TryChangeInterest(interest12) == false)
        {
            return;
        }
        pet.SetInterest12(interest12.isOn);
    }

    public void Interest13Changed()
    {
        if (TryChangeInterest(interest13) == false)
        {
            return;
        }
        pet.SetInterest13(interest13.isOn);
    }

    public void Interest14Changed()
    {
        if (TryChangeInterest(interest14) == false)
        {
            return;
        }
        pet.SetInterest14(interest14.isOn);
    }

    public void Interest15Changed()
    {
        if (TryChangeInterest(interest15) == false)
        {
            return;
        }
        pet.SetInterest15(interest15.isOn);
    }

    public void Interest16Changed()
    {
        if (TryChangeInterest(interest16) == false)
        {
            return;
        }
        pet.SetInterest16(interest16.isOn);
    }

    public void Interest17Changed()
    {
        if (TryChangeInterest(interest17) == false)
        {
            return;
        }
        pet.SetInterest17(interest17.isOn);
    }

    public void Interest18Changed()
    {
        if (TryChangeInterest(interest18) == false)
        {
            return;
        }
        pet.SetInterest18(interest18.isOn);
    }

    public void Interest19Changed()
    {
        if (TryChangeInterest(interest19) == false)
        {
            return;
        }
        pet.SetInterest19(interest19.isOn);
    }

    public void Interest20Changed()
    {
        if (TryChangeInterest(interest20) == false)
        {
            return;
        }
        pet.SetInterest20(interest20.isOn);
    }

    public void SpawnPetButton()
    {
        pet.SpawnPet();
    }

    public void DespawnPetButton()
    {
        pet.DespawnPet();
    }

    public void MatchModeDropDown()
    {
        petDetection.SetMatchMode(matchModeDropdown.value);
    }
}