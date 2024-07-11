using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using UnityEngine.UI;
using HarmonyLib;

public class Kuro : MonoBehaviour {

    //x todo make text on module a bit bigger
    //x todo calcuate the correct time
    //todo fix pfps looking the opposite way
    //todo based on the correct time, make them do the correct thing
    //todo - maintaining the repo
    //todo - creating a module
    //todo - eating
    //todo - playing KTANE
    //todo - getting ready for bed

    //todo maintaining the repo
    //x todo -get all modules from the repo
    //x todo --if json can't be gotten, have the people say to choose anyone to solve the module
    //x todo -add button interaction to profile pictures
    //x todo -test to see if the module solves if the loading fails and any pfp is pressed
    //todo -test in game if a module appears, the tolerance multiplies itself by 2
    //todo create a module
    //x todo - logging: if there are duplicate mods, put numbers in parentheses
    //todo -test when there are modules kuro made on the bomb
    //todo --make code that will put kuro into chill zone alfa
    //todo --test to make sure on a strike, kuro will be moved to chill zone alfa on his own
    //todo --test if loading fails, the hard coded list will be used instead
    //todo --add a button where kuro can disconnect from the call
    //todo ---if button is pressed before all modules are solved, strike
    //todo ---if button is pressed after all modules are solved, solve
    //todo -test when there aren't modules kuro made on the bomb
    //todo --create ideas for mod ideas
    //x todo -use souv's warning triangle to show that the loading failed
    //todo eat
    //todo play ktane
    //x todo fix the bug of the time not being displayed properly in the log
    //x todo figure out why you got an out of range error from just loading the module 
    //x todo have a set up module method that will deal with what is shown and the buttons. (Call it in start before module loading starts)
    //x todo have the custom highlighting work with all voice / text channels
    //x todo have a loading state (that doesn't break all the kms)
    //x todo when a channel is active, deactivate the other one (fix a bug where the gray highlighting disappears when the highlight event ends)
    //todo have a solved state where it shows people's game activity

    //todo beta testing
    //todo -maintaining the repo

    [SerializeField]
    private AudioClip[] audioClips; //join, leave
    [SerializeField]
    private GameObject wariningSign;
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
    private const float channelOffset = -0.0081f; //the amount of space something will move down in the list of voice channels
    private List<VoiceChannel> voiceChannelList; //chillZoneAlfa, chillZoneBravo, chillZoneCharlie

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

    private List<string> onBombKuroModules; //all the modules on the bomb made by Kuro
    private List<string> currentSolvedModules; //modules that have been solved on the bomb

    private bool pause = false; //if this is true, something must finish before any interactions can be done
    List<Person> notInVcsPeople;

    void Awake()
    {
        BombInfo = GetComponent<KMBombInfo>();
        Audio = GetComponent<KMAudio>();
        BombModule = GetComponent<KMBombModule>();
        ModuleId = ModuleIdCounter++;
        wariningSign.SetActive(false);
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

        //get all the modules made by kuro
        List<string> allModules = BombInfo.GetSolvableModuleNames(); //new List<string>();
        List<string> kuroModules;
        if (!RepoJSONGetter.Success)
        {
            kuroModules = "Technical Keypad|Procedural Maze|Blank Slate|Orientation Hypercube|Shy Guy Says|Samuel Says|Coloured Cubes".Split('|').ToList();
            Log("Data failed to load. List of Kuro modules: " + string.Join(", ", kuroModules.ToArray()));
            wariningSign.SetActive(true);
        }
        else
        {
            Log("Got data successfully");
            wariningSign.SetActive(false);
            kuroModules = RepoJSONGetter.kuroModules;
        }

        onBombKuroModules = allModules.Where(mod => kuroModules.Contains(mod)).OrderBy(q => q).ToList();
        Dictionary<string, int> dictionary = new Dictionary<string, int>();
        //this is the grossest for each loop I have ever made
        foreach (KeyValuePair<string, int> kv in onBombKuroModules.GroupBy(name => name).Select(g => new KeyValuePair<string, int>(g.Key, g.Count())))
        {
            dictionary[kv.Key] = kv.Value;
        }
        

        currentSolvedModules = new List<string>();

        if (desiredTask == Enums.Task.CreateModule)
        {
            if (onBombKuroModules.Count > 0)
            {

                Log($"You must just Chill Zone Alfa. Then solve the following modules: {dictionary.Select(kv => $"{kv.Key} ({kv.Value})").Join(", ")}");
            }
            else
            {
                Log($"You must join #mod-ideas");
            }
        }

        loadingState.SetActive(false);
        moduleActiveState.SetActive(true);
    }

    void Update()
    {
        if (!RepoJSONGetter.LoadingDone || ModuleSolved || desiredTask != Enums.Task.CreateModule)
            return;
        List<string> solvedModules = BombInfo.GetSolvedModuleNames();

        if (currentSolvedModules.Count != solvedModules.Count)
        { 
            string solvedModule = GetLatestSolve(solvedModules, currentSolvedModules);

            if (onBombKuroModules.Contains(solvedModule))
            {
                if (currentVoiceLocation != Enums.VoiceLocation.ChillZoneAlfa)
                {
                    Strike($"You solved {solvedModule} before moving to Chill Zone Alfa. Strike!");
                    MoveToChillZoneAlfa();
                }

                else
                {
                    onBombKuroModules.Remove(solvedModule);
                    Log($"Solved {solvedModule}");
                }
            }
        }
    }

    private string GetLatestSolve(List<string> solvedModules, List<string> currentSolves)
    {
        for (int i = 0; i < currentSolves.Count; i++)
        {
            solvedModules.Remove(currentSolves.ElementAt(i));
        }
        for (int i = 0; i < solvedModules.Count; i++)
        {
            currentSolvedModules.Add(solvedModules.ElementAt(i));
        }
        return solvedModules.ElementAt(0);
    }

    void SetUpModule()
    {

        //get gameobjects
        moduleActiveState = transform.Find("Module Active State").gameObject;
        loadingState = transform.Find("Loading State").gameObject;
        solvedState = transform.Find("Solved State").gameObject;
        repoRequestGameObject = moduleActiveState.transform.Find("Repo Request").gameObject;

        Transform voiceChannelTransform = moduleActiveState.transform.Find("Voice Channels");
        Transform chillZoneAlfaTransform = voiceChannelTransform.Find("Chill Zone Alfa");
        Transform chillZoneBravoTransform = voiceChannelTransform.Find("Chill Zone Bravo");
        Transform chillZoneCharlieTransform = voiceChannelTransform.Find("Chill Zone Charlie");
        Transform moddedAlfaTransform = voiceChannelTransform.Find("Modded Alfa");

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
        voiceChannelList = new List<VoiceChannel>() { new VoiceChannel(chillZoneAlfaTransform.gameObject), new VoiceChannel(chillZoneBravoTransform.gameObject), new VoiceChannel(chillZoneCharlieTransform.gameObject), new VoiceChannel(voiceChannelTransform.Find("Modded Alfa").gameObject) };

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

        notInVcsPeople = people.Select(x => x).ToList();

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

        int alfaPeople = voiceChannelList[0].PeopleCount;
        int bravoPeople = voiceChannelList[1].PeopleCount;

        Vector3 bravoVector = chillZoneBravoTransform.localPosition;
        chillZoneBravoTransform.localPosition = new Vector3(bravoVector.x, bravoVector.y, bravoVector.z + (channelOffset * alfaPeople));
        Vector3 charlieVector = chillZoneCharlieTransform.localPosition;
        chillZoneCharlieTransform.localPosition = new Vector3(charlieVector.x, charlieVector.y, charlieVector.z + (channelOffset * (alfaPeople + bravoPeople)));


        //changing kuro pfp
        Transform textChannelsTransform = moduleActiveState.transform.Find("Text Channels");
        MeshRenderer kuroPfp = moduleActiveState.transform.Find("Profile").Find("PFP").GetComponent<MeshRenderer>();
        currentKuroMood = kuroMoods.PickRandom();
        kuroPfp.material = currentKuroMood;

        //set locations
        currentTextLocation = Enums.TextLocation.None;
        currentVoiceLocation = Enums.VoiceLocation.None;

        //setting text channels
        generalTextChannel = CreateTextChannel(textChannelsTransform.Find("general").gameObject);
        modIdeasTextChannel = CreateTextChannel(textChannelsTransform.Find("mod ideas").gameObject);
        repoRequestTextChannel = CreateTextChannel(textChannelsTransform.Find("repo request").gameObject);
        voiceTextModdedTextChannel = CreateTextChannel(textChannelsTransform.Find("voice text modded").gameObject);

        textChannelList = new List<TextChannel>() { generalTextChannel, modIdeasTextChannel, repoRequestTextChannel, voiceTextModdedTextChannel };
        textChannelList.ForEach(t => t.Deactivate());
        generalTextChannel.Activate();


        //setting buttons
        Transform modIdeasTransform = textChannelsTransform.Find("mod ideas");
        modIdeasTransform.GetComponent<CustomSelectable>().SetTextChannel(modIdeasTextChannel);
        modIdeasTransform.GetComponent<KMSelectable>().OnInteract += delegate () { if (moduleActivated && !pause) { OnModIdeas(); } return false; };

        Transform repoRequestTransform = textChannelsTransform.Find("repo request");
        repoRequestTransform.GetComponent<CustomSelectable>().SetTextChannel(repoRequestTextChannel);
        repoRequestTransform.GetComponent<KMSelectable>().OnInteract += delegate () { if (moduleActivated && !pause) { OnRepoRequest(); } return false; };

        Transform voiceTextModdedTransform = textChannelsTransform.Find("voice text modded");
        voiceTextModdedTransform.GetComponent<CustomSelectable>().SetTextChannel(voiceTextModdedTextChannel);
        voiceTextModdedTransform.GetComponent<KMSelectable>().OnInteract += delegate () { if (moduleActivated && !pause) { OnVoiceTextModded(); } return false; };

        chillZoneAlfaTransform.GetComponent<CustomSelectable>().SetVoiceChannel(voiceChannelList[0]);
        chillZoneAlfaTransform.GetComponent<KMSelectable>().OnInteract += delegate () { if (moduleActivated && !pause) { OnChillZoneAlfa(); } return false; };

        chillZoneBravoTransform.GetComponent<CustomSelectable>().SetVoiceChannel(voiceChannelList[1]);
        chillZoneBravoTransform.GetComponent<KMSelectable>().OnInteract += delegate () { if (moduleActivated && !pause) { OnChillZoneBravo(); } return false; };

        chillZoneCharlieTransform.GetComponent<CustomSelectable>().SetVoiceChannel(voiceChannelList[2]);
        chillZoneCharlieTransform.GetComponent<KMSelectable>().OnInteract += delegate () { if (moduleActivated && !pause) { OnChillZoneCharlie(); } return false; };

        moddedAlfaTransform.GetComponent<CustomSelectable>().SetVoiceChannel(voiceChannelList[3]);
        moddedAlfaTransform.GetComponent<KMSelectable>().OnInteract += delegate () { if (moduleActivated && !pause) { OnModdedAlfa(); } return false; };

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

        desiredTime = new DateTime(2024, 1, 1, 9, 0, 0);
        
        int fullMiutes = desiredTime.Hour * 60 + desiredTime.Minute;

        if (fullMiutes >= 540 && fullMiutes <= 779)
            desiredTask = Enums.Task.MaintainRepo;
        else if (fullMiutes >= 780 && fullMiutes <= 959)
            desiredTask = Enums.Task.CreateModule;
        else if (fullMiutes >= 960 && fullMiutes <= 1139)
            desiredTask = Enums.Task.Eat;
        else if (fullMiutes >= 1140 && fullMiutes <= 1319)
            desiredTask = Enums.Task.PlayKTANE;
        else
            desiredTask = Enums.Task.Bed;

        if (debug)
            desiredTask = Enums.Task.CreateModule;

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
        Log("You pressed #mod-ideas");

        if (desiredTask != Enums.Task.CreateModule || onBombKuroModules.Count > 0)
        {
            WrongChannel("#mod-ideas");
            return;
        }

        if (currentTextLocation == Enums.TextLocation.None)
        {
            currentTextLocation = Enums.TextLocation.ModIdeas;
        }
    }

    private void OnRepoRequest()
    {
        Log("You pressed #repo-request");
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

            MeshRenderer[] pfpMeshRenderers = new MeshRenderer[3];
            TextMesh[] requestsText = new TextMesh[3];
            TextMesh[] nameText = new TextMesh[3];
            for (int i = 0; i < 3; i++)
            {
                Transform personTransform = repoRequestGameObject.transform.Find($"Person {i + 1}");
                requestsText[i] = personTransform.Find("Request").GetComponent<TextMesh>();
                pfpMeshRenderers[i] = personTransform.Find("PFP").GetComponent<MeshRenderer>();
                nameText[i] = personTransform.Find("Name").GetComponent<TextMesh>();
            }

            do
            {
                repoRequestPeople = Enumerable.Range(1, 3).Select(i => people.PickRandom()).ToArray();
            }
            while (repoRequestPeople.Distinct().Count() != 3);

            int maxWidth = 1650;
            int fontSize = requestsText[0].fontSize;

            for (int i = 0; i < 3; i++)
            {
                //setting up request and people
                Person p = repoRequestPeople[i];
                pfpMeshRenderers[i].material = p.ProfilePicture;
                nameText[i].text = p.Name;
            }

            if (!RepoJSONGetter.Success)
            {
                requestsText[0].text = requestsText[1].text = requestsText[2].text = FormatText("Unable get data. Select any pfp to solve the module", fontSize, maxWidth);
                Log("Unable get data. Select any pfp to solve the module");
            }

            else
            {
                for (int i = 0; i < 3; i++)
                {
                    Person p = repoRequestPeople[i];

                    KeyValuePair<string, int> kv = dictionary.PickRandom();
                    string moduleName = RepoJSONGetter.ModuleNames.PickRandom();
                    requestsText[i].text = FormatText($"{kv.Key} {moduleName}", fontSize, maxWidth);
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

    private string FormatText(string request, int fontSize, int maxWidth)
    {
        string[] requestArr = request.Split(' ');
        string text = "";
        int currentSize = 0;

        foreach (string s in requestArr)
        {
            currentSize += (s.Length + 1) * fontSize;

            if (currentSize > maxWidth)
            {
                text += $"\n{s} ";
                currentSize %= maxWidth;
            }

            else
            {
                text += $"{s} ";
            }
        }

        return text;
    }

    private void MoveToChillZoneAlfa()
    {
        pause = true;
        Audio.PlaySoundAtTransform(audioClips[0].name, transform);
        VoiceChannel vc = voiceChannelList[3];
        Person kuro = new Person(currentKuroMood);
        kuro.Name = "Kuro";
        vc.AddPerson(kuro);
        pause = false;
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
        if (desiredTask != Enums.Task.Eat && !(desiredTask == Enums.Task.CreateModule && onBombKuroModules.Count > 0))
        {
            WrongChannel("Chill Zone Alfa");
            return;
        }

        MoveToChillZoneAlfa();
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
            Log(s);

        moduleActiveState.SetActive(false);
        solvedState.SetActive(true);
        BombModule.HandlePass();
        ModuleSolved = true;
    }

    private void Strike(string s)
    {
        if (s != "")
            Log(s);
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
