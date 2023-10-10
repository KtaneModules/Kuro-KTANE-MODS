using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using UnityEngine.UI;

public class Kuro : MonoBehaviour {

    //x todo make text on module a bit bigger
    //x todo calcuate the correct time
    //todo fix pfps looking the opposite way
    //todo based on the correct time, make them do the correct thing
    //todo maintaining the repo
    //x todo -get all modules from the repo
    //x todo --if json can't be gotten, have the people say to choose anyone to solve the module
    //x todo -add button interaction to profile pictures
    //x todo -test to see if the module solves if the loading fails and any pfp is pressed
    //todo -test in game if a module appears, the tolerance multiplies itself by 2
    //x todo fix the bug of the time not being displayed properly in the log
    //x todo figure out why you got an out of range error from just loading the module 
    //x todo have a set up module method that will deal with what is shown and the buttons. (Call it in start before module loading starts)
    //x todo have the custom highlighting work with all voice / text channels
    //x todo have a loading state (that doesn't break all the kms)
    //todo when a channel is active, deactivate the other one (fix a bug where the gray highlighting disappears when the highlight event ends)
    //todo have a solved state where it shows people's game activity

    //todo beta testing
    //todo -maintaining the repo
    private static RepoJSONGetter jsonData;

    #region Module States
    private GameObject moduleActiveState, loadingState, solvedState;
    #endregion

    #region Repo Request
    private GameObject repoRequestGameObject;

    private int[] repoRequestValue = { 0, 0, 0 };

    private bool repoRequestCalculatedValues = false; //tells if we are done calculating values'

    private KMSelectable[] repoRequestPfpButtons;

    private Person[] repoRequestPeople;
    #endregion


    #region voice/text channels
    private List<VoiceChannel> voiceChannelList; //chillZoneAlfa, chillZoneBravo, chillZoneCharlie

    private GameObject chillZoneAlfaGameObject;
    public GameObject chillZoneBravoGameObject;
    public GameObject chillZoneCharlieGameObject;

    private KMSelectable modIdeasButton;
    public KMSelectable repoRequestButton;
    public KMSelectable voiceTextModdedButton;
    public KMSelectable moddedAlfaButton;
    public KMSelectable chillZoneAlfaButton;
    public KMSelectable chillZoneBravoButton;
    public KMSelectable chillZoneCharlieButton;

    private TextChannel generalTextChannel;
    private TextChannel modIdeasTextChannel;
    private TextChannel repoRequestTextChannel;
    private TextChannel voiceTextModdedTextChannel;
    private List<TextChannel> textChannelList;

    public Material[] kuroMoods;


    public Material acerPfp;
    public Material blaisePfp;
    public Material camiaPfp;
    public Material cielPfp;
    public Material curlPfp;
    public Material goodhoodPfp;
    public Material hawkerPfp;
    public Material hazelPfp;
    public Material kitPfp;
    public Material marPfp;
    public Material piccoloPfp;
    public Material playPfp;

    private List<Person> people; //acer, blaise, camia, ciel, curl, goodhood, hawker, hazel, kit, mar, piccolo, play

    private Material currentKuroMood; //kuro's mood
    #endregion


    private Enums.TextLocation currentTextLocation;
    private Enums.VoiceLocation currentVoiceLocation;
    private DateTime currentTime; //the time the bomb was activated
    private DateTime desiredTime; //th time used to figure out what to do
    private Enums.Task desiredTask; //the task needed to get done

    private KMBombInfo BombInfo;
    private KMAudio Audio;
    private KMBombModule BombModule;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved, moduleActivated = false;
    private bool debug = true;


    void Awake()
    {
        BombInfo = GetComponent<KMBombInfo>();
        Audio = GetComponent<KMAudio>();
        BombModule = GetComponent<KMBombModule>();
        ModuleId = ModuleIdCounter++;
    }

    IEnumerator Start()
    {
        BombModule.OnActivate += OnActivate;

        SetUpModule();

        jsonData = gameObject.GetComponent<RepoJSONGetter>();

        //if data is not done loaded
        if (!RepoJSONGetter.LoadingDone)
        {
            //if not already loading, load
            if (!RepoJSONGetter.Loading)
                yield return jsonData.LoadData();

            //if aleady loading, wait until loading is done
            else
            {
                do
                {
                    yield return new WaitForSeconds(0.1f);

                } while (!RepoJSONGetter.LoadingDone);
            }
        }

        loadingState.SetActive(false);
        moduleActiveState.SetActive(true);
    }

    void SetUpModule()
    {
        //get gameobjects
        moduleActiveState = transform.Find("Module Active State").gameObject;
        loadingState = transform.Find("Loading State").gameObject;
        solvedState = transform.Find("Solved State").gameObject;
        repoRequestGameObject = moduleActiveState.transform.Find("Repo Request").gameObject;

        Transform voiceChannelTransform = moduleActiveState.transform.Find("Voice Channels");

        chillZoneAlfaGameObject = voiceChannelTransform.Find("Chill Zone Alfa").gameObject;
        chillZoneBravoGameObject = voiceChannelTransform.Find("Chill Zone Bravo").gameObject;
        chillZoneCharlieGameObject = voiceChannelTransform.Find("Chill Zone Charlie").gameObject;


        //creating people
        people = new List<Person>()
        {
            new Person(acerPfp),
            new Person(blaisePfp),
            new Person(camiaPfp),
            new Person(cielPfp),
            new Person(curlPfp),
            new Person(goodhoodPfp),
            new Person(hawkerPfp),
            new Person(hazelPfp),
            new Person(kitPfp),
            new Person(marPfp),
            new Person(piccoloPfp),
            new Person(playPfp),
        };

        //create the vcs
        voiceChannelList = new List<VoiceChannel>() { new VoiceChannel(chillZoneAlfaGameObject), new VoiceChannel(chillZoneBravoGameObject), new VoiceChannel(chillZoneCharlieGameObject) };

        voiceChannelList.ForEach(t => t.Deactivate());

        //show the loading state
        solvedState.SetActive(false);
        moduleActiveState.SetActive(false);

        //hide reqo request
        repoRequestGameObject.SetActive(false);

        //set people in vcs

        int[] vcCount = new int[3];

        do
        {
            for (int i = 0; i < 3; i++)
            {
                vcCount[i] = Rnd.Range(1, 4);
            }
        } while (vcCount.Distinct().Count() != 3);

        List<Person> notInVcsPeople = people.Select(x => x).ToList();

        for (int i = 0; i < 3; i++)
        {
            VoiceChannel vc = voiceChannelList[i];
            for (int j = 0; j < vcCount[i]; j++)
            {
                Person p = notInVcsPeople.PickRandom();
                notInVcsPeople.Remove(p);
                vc.AddPerson(p);
            }
        }

        voiceChannelList.ForEach(x => { x.DisplayPeople(); });

        const float offset = -0.0081f;

        int alfaPeople = voiceChannelList[0].PeopleCount;
        int bravoPeople = voiceChannelList[1].PeopleCount;

        Vector3 bravoVector = chillZoneBravoGameObject.transform.localPosition;
        chillZoneBravoGameObject.transform.localPosition = new Vector3(bravoVector.x, bravoVector.y, bravoVector.z + (offset * alfaPeople));
        Vector3 charlieVector = chillZoneCharlieGameObject.transform.localPosition;
        chillZoneCharlieGameObject.transform.localPosition = new Vector3(charlieVector.x, charlieVector.y, charlieVector.z + (offset * (alfaPeople + bravoPeople)));


        //changing kuro pfp
        GameObject textChannels = moduleActiveState.transform.Find("Text Channels").gameObject;
        MeshRenderer kuroPfp = moduleActiveState.transform.Find("Profile").Find("PFP").GetComponent<MeshRenderer>();
        currentKuroMood = kuroMoods.PickRandom();
        kuroPfp.material = currentKuroMood;

        //set locations
        currentTextLocation = Enums.TextLocation.None;
        currentVoiceLocation = Enums.VoiceLocation.None;

        //setting text channels
        generalTextChannel = CreateTextChannel(textChannels.transform.Find("general").gameObject);
        modIdeasTextChannel = CreateTextChannel(textChannels.transform.Find("mod ideas").gameObject);
        repoRequestTextChannel = CreateTextChannel(textChannels.transform.Find("repo request").gameObject);
        voiceTextModdedTextChannel = CreateTextChannel(textChannels.transform.Find("voice text modded").gameObject);

        textChannelList = new List<TextChannel>() { generalTextChannel, modIdeasTextChannel, repoRequestTextChannel, voiceTextModdedTextChannel };
        textChannelList.ForEach(t => t.Deactivate());
        generalTextChannel.Activate();


    //setting buttons
        modIdeasButton = textChannels.transform.Find("mod ideas").GetComponent<KMSelectable>();
        modIdeasButton.OnInteract += delegate () { if (moduleActivated) { OnModIdeas(); } return false; };
        
        repoRequestButton.OnInteract += delegate () { if (moduleActivated) { OnRepoRequest(); } return false; }; ;
        voiceTextModdedButton.OnInteract += delegate () { if (moduleActivated) { OnVoiceTextModded(); } return false; }; ;
        moddedAlfaButton.OnInteract += delegate () { if (moduleActivated) { OnModdedAlfa(); } return false; }; ;
        chillZoneAlfaButton.OnInteract += delegate () { if (moduleActivated) { OnChillZoneAlfa(); } return false; }; ;
        chillZoneBravoButton.OnInteract += delegate () { if (moduleActivated) { OnChillZoneBravo(); } return false; };
        chillZoneCharlieButton.OnInteract += delegate () { if (moduleActivated) { OnChillZoneCharlie(); } return false; };

        //repo request buttons
        repoRequestPfpButtons = Enumerable.Range(1, 3).Select(i => repoRequestGameObject.transform.Find($"Person {i}").Find("PFP").GetComponent<KMSelectable>()).ToArray();

        for (int i = 0; i < 3; i++)
        {
            int dummy = i;
            repoRequestPfpButtons[dummy].OnInteract += delegate () { OnRepoRequestProfilePic(dummy); return false; };
        }
    }

    void OnActivate()
    {
        currentTime = DateTime.Now;

        //Take the highest out of batteries, indicators and ports
        int batteryCount = BombInfo.GetBatteryCount();
        int indicatorCount = BombInfo.GetIndicators().Count();
        int portCount = BombInfo.GetPortCount();

        int minuteOffset = 0;

        Log("Current Time: " + FormatHourMinute(currentTime));
        Log("Battery Count: " + batteryCount);
        Log("Indicator Count: " + indicatorCount);
        Log("Port Count: " + portCount);

        //If the number of batteries is the highest, add 1 hour for each batteries
        if (batteryCount > indicatorCount && batteryCount > portCount)
        { 
            minuteOffset = 60 * batteryCount;
            Log("Battery count is the highest. Minute offset is now " + minuteOffset);
        }

        //Otherwise, if the number of indicators is the highest, add 30 minutes for each indicators
        else if (indicatorCount > batteryCount && indicatorCount > portCount)
        {
            minuteOffset = 30 * indicatorCount;
            Log("Indicator count is the highest. Minute offset is now " + minuteOffset);
        }

        //Otherwise, add 15 minutes for each port
        else
        {
            minuteOffset = 15 * portCount;
            Log("Port count is the highest. Minute offset is now " + minuteOffset);
        }

        string serialNumber = BombInfo.GetSerialNumber().ToUpper();

        foreach (char c in serialNumber)
        {
            if (Char.IsLetter(c))
            {
                if ("AEIOU".Contains(c))
                {
                    minuteOffset -= 60;
                    Log($"{c} is a vowel. Minute offset is now {minuteOffset}");
                }

                else
                { 
                    minuteOffset -= 30;
                    Log($"{c} is a consonant. Minute offset is now {minuteOffset}");
                }
            }
        }

        desiredTime = currentTime.AddMinutes(minuteOffset);
        
        int fullMiutes = desiredTime.Hour * 60 + desiredTime.Minute;

        if (fullMiutes >= 9 * 60 && fullMiutes <= 12 * 60 + 59)
            desiredTask = Enums.Task.MaintainRepo;
        else if (fullMiutes >= 13 * 60 && fullMiutes <= 15 * 60 + 59)
            desiredTask = Enums.Task.CreateModule;
        else if (fullMiutes >= 16 * 60 && fullMiutes <= 18 * 60 + 59)
            desiredTask = Enums.Task.CreateModule;
        else if (fullMiutes >= 19 * 60 && fullMiutes <= 21 * 60 + 59)
            desiredTask = Enums.Task.PlayKTANE;
        else
            desiredTask = Enums.Task.Bed;

        if (debug)
            desiredTask = Enums.Task.MaintainRepo;

        people.ForEach(person => person.SetTolerance(desiredTime.DayOfWeek));

        Log($"It's {FormatHourMinute(desiredTime)}. You should be {GetTask(desiredTask)}");
        moduleActivated = true;
    }



    public TextChannel CreateTextChannel(GameObject gameObject)
    {
        GameObject highlight = gameObject.transform.Find("background").gameObject;
        TextMesh textMesh = gameObject.transform.Find("label").GetComponent<TextMesh>();

        return new TextChannel(textMesh, highlight);
    }

    private void OnModIdeas()
    {
        if (desiredTask != Enums.Task.CreateModule)
        {
            WrongChannel("#mod-ideas");
            return;
        }
    }

    private void OnRepoRequest()
    { 
        if(desiredTask != Enums.Task.MaintainRepo) 
        {
            WrongChannel("#repo-request");
            return;
        }

        if (currentTextLocation == Enums.TextLocation.None)
        {
            generalTextChannel.Deactivate();
            repoRequestTextChannel.Activate();

            Dictionary<string, int> dictionary = new Dictionary<string, int>()
            {
                ["Make an interactive for"] = -2,
                ["Upload manual for"] = -1,
                ["Fix grammar mistakes in"] = 0,
                ["Add dark mode support to"] = 1,
                ["Add LFA Support to"] = 2,
                ["Svgify"] = 3,
            };

            Text[] requestsText = Enumerable.Range(1, 3).Select(x => repoRequestGameObject.transform.Find("Canvas").Find($"Person {x} Text").GetComponent<Text>()).ToArray();

            do
            {
                repoRequestPeople = Enumerable.Range(1, 3).Select(i => people.PickRandom()).ToArray();
            }
            while (repoRequestPeople.Distinct().Count() != 3);

            for (int i = 0; i < 3; i++)
            {
                //setting up request and people
                MeshRenderer meshRenderer = repoRequestGameObject.transform.Find($"Person {i + 1}").Find("PFP").GetComponent<MeshRenderer>();
                TextMesh textMesh = repoRequestGameObject.transform.Find($"Person {i + 1}").Find("Name").GetComponent<TextMesh>();
                Person p = repoRequestPeople[i];

                meshRenderer.material = p.ProfilePicture;
                textMesh.text = p.Name;
            }

            if (!RepoJSONGetter.Success)
            {
                requestsText[0].text = requestsText[1].text = requestsText[2].text = "Unable get data. Select any pfp to solve the module";
                Log("Unable get data. Select any pfp to solve the module");
            }

            else
            {
                for (int i = 0; i < 3; i++)
                {
                    Person p = repoRequestPeople[i];

                    KeyValuePair<string, int> kv = dictionary.PickRandom();
                    string moduleName = RepoJSONGetter.ModuleNames.PickRandom();

                    requestsText[i].text = $"{kv.Key} {moduleName}";

                    //calculating values
                    int tolerance = p.Tolerance;

                    Log($"{p.Name}'s request: {requestsText[i].text}");

                    Log($"{p.Name} has an inital tolerance of {GetTolerance(tolerance)}");

                    if (tolerance != int.MinValue)
                    {
                        //modify it based on what the task is
                        tolerance += kv.Value;

                        Log($"Modify it by {kv.Value}");

                        //If the module that is named in the request is on the bomb, double the value
                        if (BombInfo.GetModuleNames().Contains(moduleName))
                        {
                            tolerance *= 2;
                            Log($"{moduleName} is on the bomb. Multiplying tolerance by 2");
                        }
                    }

                    Log($"{p.Name} has a total tolerance of {GetTolerance(tolerance)}");
                    repoRequestValue[i] = tolerance;       
                }
            }
            repoRequestGameObject.SetActive(true);
            currentTextLocation = Enums.TextLocation.RepoRequest;
            repoRequestCalculatedValues = true;
        }
    }

    public void OnRepoRequestProfilePic(int index)
    {
        if (ModuleSolved || !repoRequestCalculatedValues)
            return;

        Log($"You chose {repoRequestPeople[index].Name}");

        //if the data could not be laoded properly
        if (!RepoJSONGetter.Success)
        {
            Solve("Data could not be loaded. Solving module...");
            return;
        }

        int max = repoRequestValue.Max();

        //check to see this index is the max
        if (repoRequestValue[index] == max)
        {
            //all the indicies that have the max value
            List<int> maxIndex = new List<int>();

            for (int i = 0; i < 3; i++)
            {
                if (max == repoRequestValue[i])
                    maxIndex.Add(i);
            }

            if (maxIndex.Count != 1)
            {
                if (maxIndex[0] != index)
                {
                    Strike($"{repoRequestPeople[0]} was higher in the list. Strike!");
                    return;
                }
            }

            else
            {
                Solve("Solving module...");
            }
        }

        else
        {
            Strike("Somoene had a higher value. Strike!");
        }
    }

    private string GetTolerance(int num)
    {
        return num == int.MinValue ? "Skull" : num.ToString();
    }

    private void OnVoiceTextModded()
    {
        if (desiredTask != Enums.Task.PlayKTANE)
        {
            WrongChannel("#voice-text-modded");
            return;
        }
    }

    private void OnModdedAlfa()
    {
        if (desiredTask != Enums.Task.PlayKTANE)
        {
            WrongChannel("Modded Alfa");
            return;
        }
    }

    private void OnChillZoneAlfa()
    {
        if (desiredTask != Enums.Task.Eat)
        {
            WrongChannel("Chill Zone Alfa");
            return;
        }
    }

    private void OnChillZoneBravo()
    {
        if (desiredTask != Enums.Task.Eat)
        {
            WrongChannel("Chill Zone Bravo");
            return;
        }
    }

    private void OnChillZoneCharlie()
    {
        if (desiredTask != Enums.Task.Eat)
        {
            WrongChannel("Chill Zone Charlie");
            return;
        }
    }

    private string FormatHourMinute(DateTime dateTime)
    {
        return $"{dateTime.DayOfWeek}, {dateTime.Hour:00}:{dateTime.Minute:00}";
    }


    private string GetTask(Enums.Task task) 
    {
        switch (task) 
        {
            case Enums.Task.MaintainRepo:
                return "maintaining the repo";
            case Enums.Task.CreateModule:
                return "creating a module";
            case Enums.Task.Eat:
                return "eating";
            case Enums.Task.PlayKTANE:
                return "playing KTANE";
            case Enums.Task.Bed:
                return "getting ready for bed";
        }

        return "ERROR";
    }

    private void WrongChannel(string location)
    {
        Strike($"You don't need to go to {location}. Strike!");
    }

    private void Solve(string s)
    {
        if (s != "")
            Debug.Log($"[Kuro #{ModuleId}] {s}");

        moduleActiveState.SetActive(false);
        solvedState.SetActive(true);
        BombModule.HandlePass();
        ModuleSolved = true;
    }

    private void Strike(string s)
    {
        if (s != "")
            Debug.Log($"[Kuro #{ModuleId}] {s}");
        BombModule.HandleStrike();
    }

    private void Log(string s)
    {
        Debug.Log($"[Kuro #{ModuleId}] {s}");
    }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      yield return null;
   }

   IEnumerator TwitchHandleForcedSolve () {
      yield return null;
   }
}
