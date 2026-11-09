using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

enum State
{
    Idle,
    Follow,
    curious,
    dance,
}

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PetObjectV2 : UdonSharpBehaviour
{

    [Header("Pet Model")]
    [SerializeField] private GameObject petModel;
    [SerializeField] private Renderer petRenderer;
    [SerializeField] private Renderer petTailRenderer;
    [SerializeField] private GameObject petSpawnZone;

    [Header("Body Materials")]
    [SerializeField] private Material compSci;
    [SerializeField] private Material chem;
    [SerializeField] private Material astron;
    [SerializeField] private Material geo;
    [SerializeField] private Material engin;
    [SerializeField] private Material math;
    [SerializeField] private Material bio;

    [Header("Glow Body Materials")]
    [SerializeField] private Material glowcompSci;
    [SerializeField] private Material glowchem;
    [SerializeField] private Material glowastron;
    [SerializeField] private Material glowgeo;
    [SerializeField] private Material glowengin;
    [SerializeField] private Material glowmath;
    [SerializeField] private Material glowbio;

    [Header("Tail Materials")]
    [SerializeField] private Material tailCompSci;
    [SerializeField] private Material tailChem;
    [SerializeField] private Material tailAstron;
    [SerializeField] private Material tailGeo;
    [SerializeField] private Material tailEngin;
    [SerializeField] private Material tailMath;
    [SerializeField] private Material tailBio;

    [Header("Navigation")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float stopDistance;

    [Header("Ear Prefabs")]
    [SerializeField] private GameObject bear;
    [SerializeField] private GameObject hare;
    [SerializeField] private GameObject impala;
    [SerializeField] private GameObject llama;
    [SerializeField] private GameObject koala;
    [SerializeField] private GameObject redpanda;

    [Header("Tail Rings")]
    [SerializeField] private GameObject tail1;
    [SerializeField] private GameObject tail2;
    [SerializeField] private GameObject tail3;
    [SerializeField] private GameObject tail4;

    private VRCPlayerApi player;
    private State currentState;

    [UdonSynced] private string petColour;

    [UdonSynced] private string fieldName;

    [UdonSynced] private int petEar;
    [UdonSynced] private int tailSeniorityLevel;

    [UdonSynced] private bool interest1;
    [UdonSynced] private bool interest2;
    [UdonSynced] private bool interest3;
    [UdonSynced] private bool interest4;
    [UdonSynced] private bool interest5;
    [UdonSynced] private bool interest6;
    [UdonSynced] private bool interest7;
    [UdonSynced] private bool interest8;
    [UdonSynced] private bool interest9;
    [UdonSynced] private bool interest10;
    [UdonSynced] private bool interest11;
    [UdonSynced] private bool interest12;
    [UdonSynced] private bool interest13;
    [UdonSynced] private bool interest14;
    [UdonSynced] private bool interest15;
    [UdonSynced] private bool interest16;
    [UdonSynced] private bool interest17;
    [UdonSynced] private bool interest18;
    [UdonSynced] private bool interest19;
    [UdonSynced] private bool interest20;

    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text stemInterestsText;
    [SerializeField] private TMP_Text userTypesText;

    [SerializeField] private TMP_Text friendPlayerNameText;
    [SerializeField] private TMP_Text friendStemInterestsText;
    [SerializeField] private TMP_Text friendUserTypesText;

    [SerializeField] private GlobalPetManager petManager;
    private GameObject target;
    [SerializeField] private TMP_Text errorText;

    [SerializeField] private float danceDuration = 30f;
    private float danceTimer;
    [UdonSynced] private bool isSpawned = false;

    private Material impalaMat;
    private Texture bodyTex;

    [UdonSynced] private bool isBusy = false;
    [UdonSynced] private bool manualBusy = false;

    [SerializeField] private TMP_Text busyText;
    private bool lastShownBusy;
    private bool busyTextInitialised = false;

    [UdonSynced] private bool isDancing;

    public bool Interest1
    {
        get
        {
            return interest1;
        }
    }

    public bool Interest2
    {
        get
        {
            return interest2;
        }
    }

    public bool Interest3
    {
        get
        {
            return interest3;
        }
    }

    public bool Interest4
    {
        get
        {
            return interest4;
        }
    }

    public bool IsBusy
    {
        get
        {
            return isBusy || manualBusy || !isSpawned;
        }
    }

    public bool IsSpawned
    {
        get
        {
            return isSpawned;
        }
    }

    public GameObject PetModel
    {
        get
        {
            return petModel;
        }
    }

    public bool Interest5 { get { return interest5; } }
    public bool Interest6 { get { return interest6; } }
    public bool Interest7 { get { return interest7; } }
    public bool Interest8 { get { return interest8; } }
    public bool Interest9 { get { return interest9; } }
    public bool Interest10 { get { return interest10; } }
    public bool Interest11 { get { return interest11; } }
    public bool Interest12 { get { return interest12; } }
    public bool Interest13 { get { return interest13; } }
    public bool Interest14 { get { return interest14; } }
    public bool Interest15 { get { return interest15; } }
    public bool Interest16 { get { return interest16; } }
    public bool Interest17 { get { return interest17; } }
    public bool Interest18 { get { return interest18; } }
    public bool Interest19 { get { return interest19; } }
    public bool Interest20 { get { return interest20; } }

    void Start()
    {
        currentState = State.Follow;
        player = Networking.GetOwner(transform.parent.gameObject);
        if (player != null)
        {
            petManager.addPet(this);
        }
        petModel.SetActive(isSpawned);
        ApplyPetColour();
        ApplyPetEar();
        ApplyTails();
        UpdatePetInfo();
        agent.updateRotation = false;

        //Get the ear shader material, to change it dynamically with the body texture.
        impalaMat = impala.transform.GetChild(0).GetComponent<Renderer>().material;
        UpdateEarTexture();
        // Debug.Log("body tex hash: " + VRCShader.PropertyToID("_BodyTex")); //1306
    }

    public override void OnPlayerLeft(VRCPlayerApi leftPlayer)
    {
        if (leftPlayer.playerId == player.playerId)
        {
            petManager.removePet(this);
        }
    }

    void Update()
    {
        if (!Networking.IsOwner(transform.parent.gameObject))
        {
            return;
        }

        if (!isSpawned)
        {
            return;
        }

        UpdateBusyText();

        switch (currentState)
        {
            case State.Idle:
                IdleState();
                break;

            case State.Follow:
                FollowState();
                break;

            case State.curious:
                CuriousState();
                break;

            case State.dance:
                DanceState();
                break;
        }
    }

    private void IdleState()
    {
        petModel.transform.LookAt(target.transform.position);
    }

    private void FollowState()
    {

        NavMeshHit hit;

        if (NavMesh.SamplePosition(player.GetPosition(), out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        petModel.transform.LookAt(player.GetPosition());
    }

    private void CuriousState()
    {

    }

    private void DanceState()
    {
        danceTimer += Time.deltaTime;

        if (danceTimer >= danceDuration)
        {
            EndDance();
            return;
        }

        NavMeshHit hit;

        if (NavMesh.SamplePosition(target.transform.position, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        petModel.transform.LookAt(target.transform.position);
    }

    private void EndDance()
    {
        agent.ResetPath();
        target = null;
        danceTimer = 0f;
        isBusy = false;
        currentState = State.Follow;
        isDancing = false;
        ApplyPetColour();
        RequestSerialization();
        ShowDebug("Dance finished, back to following player.");
    }

    public void changeColour(string i)
    {
        if (!Networking.IsOwner(gameObject))
        {
            return;
        }

        petColour = i;
        // Debug.Log(i);

        ApplyPetColour();
        UpdateEarTexture();
        UpdatePetInfo();
        RequestSerialization();
    }

    public void changeEar(int selectedEar)
    {
        if (!Networking.IsOwner(gameObject))
        {
            return;
        }

        petEar = selectedEar;
        ApplyPetEar();
        UpdateEarTexture();
        RequestSerialization();
    }

    public void changeRings(int selectedTail)
    {
        // Debug.Log("changeRings called with " + selectedTail);

        if (!Networking.IsOwner(gameObject))
        {
            // Debug.Log("changeRings blocked: not owner of " + gameObject.name);
            return;
        }

        tailSeniorityLevel = selectedTail;
        ApplyTails();
        RequestSerialization();
    }

    private void ApplyTails()
    {
        // Debug.Log("ApplyTails level " + tailSeniorityLevel + " | tail1: " + tail1.name);
        if(tailSeniorityLevel == 0)
        {
            tail1.SetActive(false);
            tail2.SetActive(false);
            tail3.SetActive(false);
            tail4.SetActive(false);
        }
        else if(tailSeniorityLevel == 1)
        {
            tail1.SetActive(true);
            tail2.SetActive(false);
            tail3.SetActive(false);
            tail4.SetActive(false);
        }
        else if(tailSeniorityLevel == 2)
        {
            tail1.SetActive(true);
            tail2.SetActive(true);
            tail3.SetActive(false);
            tail4.SetActive(false);           
        }
        else if(tailSeniorityLevel == 3)
        {
            tail1.SetActive(true);
            tail2.SetActive(true);
            tail3.SetActive(true);
            tail4.SetActive(false);
        }
        else if(tailSeniorityLevel == 4)
        {
            tail1.SetActive(true);
            tail2.SetActive(true);
            tail3.SetActive(true);
            tail4.SetActive(true);
        }
    }

    private void ApplyPetEar()
    {
        if (petEar == 0)
        {
            bear.SetActive(true);
            hare.SetActive(false);
            impala.SetActive(false);
            llama.SetActive(false);
            koala.SetActive(false);
            redpanda.SetActive(false);
        }
        else if (petEar == 1)
        {
            bear.SetActive(false);
            hare.SetActive(true);
            impala.SetActive(false);
            llama.SetActive(false);
            koala.SetActive(false);
            redpanda.SetActive(false);
        }
        else if (petEar == 2)
        {
            bear.SetActive(false);
            hare.SetActive(false);
            impala.SetActive(true);
            llama.SetActive(false);
            koala.SetActive(false);
            redpanda.SetActive(false);
        }
        else if (petEar == 3)
        {
            bear.SetActive(false);
            hare.SetActive(false);
            impala.SetActive(false);
            llama.SetActive(true);
            koala.SetActive(false);
            redpanda.SetActive(false);
        }
        else if (petEar == 4)
        {
            bear.SetActive(false);
            hare.SetActive(false);
            impala.SetActive(false);
            llama.SetActive(false);
            koala.SetActive(true);
            redpanda.SetActive(false);
        }
        else if (petEar == 5)
        {
            bear.SetActive(false);
            hare.SetActive(false);
            impala.SetActive(false);
            llama.SetActive(false);
            koala.SetActive(false);
            redpanda.SetActive(true);
        }
    }

    // Apply changes to the ear colour, to match the body colour.
    private void UpdateEarTexture()
    {
        bodyTex = petRenderer.material.mainTexture;
        Material currentEarMat = impalaMat;
        switch (petEar)
        {
            case 1:
                currentEarMat = hare.GetComponent<Renderer>().material;
                break;
            case 2:
                currentEarMat = impalaMat;
                break;
            case 3:
                currentEarMat = llama.GetComponent<Renderer>().material;
                break;
            case 4:
                currentEarMat = koala.GetComponent<Renderer>().material;
                break;
            case 5:
                currentEarMat = redpanda.GetComponent<Renderer>().material;
                break;
            default:
                currentEarMat = bear.GetComponent<Renderer>().material;
                break;
        }
        // Debug.Log("current ear " + currentEarMat);
        currentEarMat.SetTexture("_BodyTex", bodyTex);
        // Debug.Log("update ear texture");
    }

    private void ApplyPetColour()
    {
        if (petColour == "CompSci")
        {
            fieldName = "Computer Science";
            petRenderer.material = isDancing ? glowcompSci : compSci;
            petTailRenderer.material = tailCompSci;
        }
        else if (petColour == "Chem")
        {
            fieldName = "Chemistry";
            petRenderer.material = isDancing ? glowchem : chem;
            petTailRenderer.material = tailChem;
        }
        else if (petColour == "Astro")
        {
            fieldName = "Astronomy";
            petRenderer.material = isDancing ? glowastron : astron;
            petTailRenderer.material = tailAstron;
        }
        else if (petColour == "Geo")
        {
            fieldName = "Geology";
            petRenderer.material = isDancing ? glowgeo : geo;
            petTailRenderer.material = tailGeo;
        }
        else if (petColour == "Eng")
        {
            fieldName = "Engineering";
            petRenderer.material = isDancing ? glowengin : engin;
            petTailRenderer.material = tailEngin;
        }
        else if (petColour == "bio")
        {
            fieldName = "Biology";
            petRenderer.material = isDancing ? glowbio : bio;
            petTailRenderer.material = tailBio;
        }
        else if (petColour == "math")
        {
            fieldName = "Mathematics";
            petRenderer.material = isDancing ? glowmath : math;
            petTailRenderer.material = tailMath;
        }
    }

    public override void OnDeserialization()
    {
        ApplySpawned();
        ApplyPetColour();
        ApplyPetEar();
        ApplyTails();
        UpdateEarTexture();
        UpdatePetInfo();
    }

    public void SetInterest1(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest1 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest2(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest2 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest3(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest3 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest4(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest4 = value;
        UpdatePetInfo();
        RequestSerialization();
    }
    public void SetInterest5(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest5 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest6(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest6 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest7(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest7 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest8(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest8 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest9(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest9 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest10(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest10 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest11(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest11 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest12(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest12 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest13(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest13 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest14(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest14 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest15(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest15 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest16(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest16 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest17(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest17 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest18(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest18 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest19(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest19 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    public void SetInterest20(bool value)
    {
        if (!Networking.IsOwner(gameObject)) return;

        interest20 = value;
        UpdatePetInfo();
        RequestSerialization();
    }

    private void UpdatePetInfo()
    {
        VRCPlayerApi owner = Networking.GetOwner(transform.parent.gameObject);

        if (Utilities.IsValid(owner))
        {
            playerNameText.text = owner.displayName;
        }

        stemInterestsText.text = "";

        if (interest1)
        {
            stemInterestsText.text += "Astronomy\n";
        }

        if (interest2)
        {
            stemInterestsText.text += "Engineering\n";
        }

        if (interest3)
        {
            stemInterestsText.text += "Computer Science\n";
        }

        if (interest4)
        {
            stemInterestsText.text += "Geology\n";
        }

        if (interest5)
        {
            stemInterestsText.text += "Chemistry\n";
        }

        if (interest6)
        {
            stemInterestsText.text += "Biology/Life Science\n";
        }

        if (interest7)
        {
            stemInterestsText.text += "Mathematics\n";
        }

        if (interest8)
        {
            stemInterestsText.text += "Physics\n";
        }

        if (interest9)
        {
            stemInterestsText.text += "Photography\n";
        }

        if (interest10)
        {
            stemInterestsText.text += "Research\n";
        }

        if (interest11)
        {
            stemInterestsText.text += "History\n";
        }

        if (interest12)
        {
            stemInterestsText.text += "Video/Tabletop Games\n";
        }

        if (interest13)
        {
            stemInterestsText.text += "Books/Literature\n";
        }

        if (interest14)
        {
            stemInterestsText.text += "Sports/Outdoor Activities\n";
        }

        if (interest15)
        {
            stemInterestsText.text += "Music\n";
        }

        if (interest16)
        {
            stemInterestsText.text += "Arts and Crafts\n";
        }

        if (interest17)
        {
            stemInterestsText.text += "Travel\n";
        }

        if (interest18)
        {
            stemInterestsText.text += "Food and Gastronomy\n";
        }

        if (interest19)
        {
            stemInterestsText.text += "Film/Television\n";
        }

        if (interest20)
        {
            stemInterestsText.text += "Language and Culture\n";
        }

        userTypesText.text = fieldName;

        if (Utilities.IsValid(friendUserTypesText))
        {
            friendUserTypesText.text = userTypesText.text;
        }

        if (Utilities.IsValid(friendPlayerNameText))
        {
            friendPlayerNameText.text = playerNameText.text;
        }

        if (Utilities.IsValid(friendStemInterestsText))
        {
            friendStemInterestsText.text = stemInterestsText.text;
        }
    }

    public void dancePet(GameObject pet)
    {
        isBusy = true;
        target = pet;
        danceTimer = 0f;
        ShowDebug("" + pet.gameObject);
        currentState = State.dance;

        isDancing = true;
        ApplyPetColour();
        RequestSerialization();
    }

    private void ShowDebug(string message)
    {
        Debug.Log(message);

        if (Utilities.IsValid(errorText))
        {
            errorText.text += message + "\n";
        }
    }

    public void SpawnPet()
    {
        if (!Networking.IsOwner(gameObject))
        {
            return;
        }

        isSpawned = true;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(petSpawnZone.transform.position, out hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            ShowDebug("Spawn zone is not near the NavMesh.");
        }

        currentState = State.Follow;
        isDancing = false;
        ApplySpawned();
        ApplyPetColour();
        UpdateEarTexture();
        UpdatePetInfo();
        RequestSerialization();
    }

    public void DespawnPet()
    {
        if (!Networking.IsOwner(gameObject))
        {
            return;
        }

        isSpawned = false;
        isDancing = false;
        agent.ResetPath();
        target = null;
        isBusy = false;
        currentState = State.Follow;
        ApplySpawned();
        RequestSerialization();
    }

    private void ApplySpawned()
    {
        petModel.SetActive(isSpawned);
    }

    public void ToggleBusy()
    {
        if (!Networking.IsOwner(gameObject))
        {
            return;
        }

        manualBusy = !manualBusy;

        if (manualBusy && (currentState == State.dance || currentState == State.Idle))
        {
            EndDance();
        }

        RequestSerialization();
    }

    private void UpdateBusyText()
    {
        if (!Utilities.IsValid(busyText))
        {
            return;
        }

        bool busyNow = IsBusy;

        // Only rewrite the text when the value changes.
        if (busyTextInitialised && busyNow == lastShownBusy)
        {
            return;
        }

        lastShownBusy = busyNow;
        busyTextInitialised = true;
        busyText.text = "Busy: " + busyNow;
    }
}