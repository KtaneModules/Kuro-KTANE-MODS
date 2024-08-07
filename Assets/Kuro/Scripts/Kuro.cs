using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEditor;

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

    //x todo maintaining the repo
    //x todo -get all modules from the repo
    //x todo --if json can't be gotten, have the people say to choose anyone to solve the module
    //x todo -add button interaction to profile pictures
    //x todo -test to see if the module solves if the loading fails and any pfp is pressed
    //x todo -test in game if a module appears, the tolerance multiplies itself by 2
    //x todo create a module
    //x todo - logging: if there are duplicate mods, put numbers in parentheses
    //x todo -test when there are modules kuro made on the bomb
    //x todo --make code that will put kuro into chill zone alfa
    //x todo --test to make sure on a strike, kuro will be moved to chill zone alfa on his own
    //x todo --test if loading fails, the hard coded list will be used instead
    //x todo --add a button where kuro can disconnect from the call
    //x todo ---if button is pressed before all modules are solved, strike
    //x todo ---if button is pressed after all modules are solved, solve
    //x todo -test when there aren't modules kuro made on the bomb
    //x todo --create ideas for mod ideas
    //x todo -use souv's warning triangle to show that the loading failed
    //todo eat
    //todo fix the logging with there being numbers for the food (stretch goal)
    //x todo- make it so a green circle appears when kuro is speaking
    //todo - make it so when someone clcks on kuro, he will repeat what he said (stretch goal)
    //todo - add kuro voice lines (stretch goal)
    //x todo - put british flags on fab lollies
    //todo play ktane
    //todo - strike if voice-text-modded is pressed first
    //todo - kuro joins call
    //todo - two people (who are not in other voice calls) join call
    //todo - strike when trying to leave
    //todo bed
    //x todo fix the bug of the time not being displayed properly in the log
    //x todo figure out why you got an out of range error from just loading the module 
    //x todo have a set up module method that will deal with what is shown and the buttons. (Call it in start before module loading starts)
    //x todo have the custom highlighting work with all voice / text channels
    //x todo have a loading state (that doesn't break all the kms)
    //x todo when a channel is active, deactivate the other one (fix a bug where the gray highlighting disappears when the highlight event ends)
    //todo have a solved state where it shows people's game activity (stretch goal)
    //todo change the discord leaving sound (stretch goal)
    //todo have people in vcs be in alphabetical order (stretch goal)
    //todo have the highlight/on hover work for all the voice/text channels (stretch goal)

    [SerializeField]
    private AudioClip[] audioClips; //join, leave, eggs, fab lolly, pasta
    [SerializeField]
    private GameObject wariningSign;
    private static RepoJSONGetter jsonData;

    #region Module States
    private GameObject loadingState, solvedState;
    #endregion

    #region Food
    [SerializeField]
    private Material[] eggs, fabLollies, pasta, others;
    #endregion
    #region voice/text channels
    private const float channelOffset = -0.0081f; //the amount of space something will move down in the list of voice channels
    private List<VoiceChannel> voiceChannelList; //chillZoneAlfa, chillZoneBravo, chillZoneCharlie, modded alfa

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

    private Enums.Mood currentMood;
    #endregion

    VoiceChannel[] desiredVCs;

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
        List<string> allModules = BombInfo.GetSolvableModuleNames();
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
        currentSolvedModules = new List<string>();
        Debug.Log(voiceChannelList.Select(x => x.Name).ToArray().Join(", "));
        switch (desiredTask)
        {
            case Enums.Task.Eat:
                desiredVCs = new VoiceChannel[1];
                VoiceChannel[] chillZones = voiceChannelList.Where(vc => vc.Name != "Modded Alfa").ToArray();
                List<VoiceChannel> voiceChannelByOrder = voiceChannelList.Where(vc => vc.Name != "Modded Alfa").OrderByDescending(vc => vc.people.Count).ToList();

                if (currentMood == Enums.Mood.Happy)
                {
                    desiredVCs = new VoiceChannel[] { voiceChannelByOrder[0] };
                }
                else if (currentMood == Enums.Mood.Neutral)
                {
                    desiredVCs = new VoiceChannel[] { voiceChannelByOrder[1] };
                }
                else if (currentMood == Enums.Mood.Angry)
                {
                    desiredVCs = new VoiceChannel[] { voiceChannelByOrder[2] };
                }
                else if (currentMood == Enums.Mood.Devious)
                {
                    desiredVCs = new VoiceChannel[] { voiceChannelByOrder.First(vc => vc.people.Any(p => p.Name == "CurlBot")) };
                }
                else if (currentMood == Enums.Mood.Curious)
                {
                    desiredVCs = voiceChannelByOrder.ToArray();
                }

                if (currentMood != Enums.Mood.Devious && currentMood != Enums.Mood.Curious && desiredVCs[0].people.Any(p => p.Name == "CurlBot"))
                { 
                    desiredVCs[0] = chillZones[(Array.IndexOf(chillZones, desiredVCs[0]) + 1) % chillZones.Length];
                }
                Log($"You are {currentMood}. You are able to join {desiredVCs.Select(vc => vc.Name).Join(", ")}");

                break;


            case Enums.Task.MaintainRepo:
                Log("You must look at #repo-requests");
                break;
            case Enums.Task.CreateModule:
                if (onBombKuroModules.Count > 0)
                {
                    Log($"You must join Chill Zone Alfa. Then solve the following modules: {GetGroupModuleString()}");
                }
                else
                {
                    Log($"You must look at #mod-ideas");
                }
                break;

            case Enums.Task.PlayKTANE:
                Log("You must play join Modded Alfa and look at #voice-text-modded in that order");
                break;
        }

        loadingState.SetActive(false);
        EnableModuleActive(true);
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
                onBombKuroModules.Remove(solvedModule);
                if (currentVoiceLocation != Enums.VoiceLocation.ChillZoneAlfa)
                {
                    Strike($"You solved {solvedModule} before moving to Chill Zone Alfa. Strike!");
                    MoveToVoiceChannel(voiceChannelList[0]);
                }

                else
                {
                    if (onBombKuroModules.Count == 0)
                    {
                        Log($"Solved {solvedModule}. Leave the call to solve the module");
                    }
                    else
                    {
                        Log($"Solved {solvedModule}. Need to solve: {GetGroupModuleString()}");
                    }
                }
            }
        }
    }

    void SetUpModule()
    {
        //get gameobjects
        loadingState = transform.Find("Loading State").gameObject;
        solvedState = transform.Find("Solved State").gameObject;
        EnableSpeaking(false);
        EnableVoiceGameObject(false);
        EnableModIdeas(false);
        EnableRepoRequest(false);
        EnableFood(false);
        EnableModuleActive(false);

        Transform voiceChannelTransform = transform.Find("Module Active State/Voice Channels");
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
        voiceChannelList = new List<VoiceChannel>() { new VoiceChannel(chillZoneAlfaTransform.gameObject, "Chill Zone Alfa"), new VoiceChannel(chillZoneBravoTransform.gameObject, "Chill Zone Bravo"), new VoiceChannel(chillZoneCharlieTransform.gameObject, "Chill Zone Charlie"), new VoiceChannel(voiceChannelTransform.Find("Modded Alfa").gameObject, "Modded Alfa") };
        //show the loading state
        solvedState.SetActive(false);
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
                vc.people.Add(p);
            }
        }

        voiceChannelList.ForEach(t => { t.DisplayPeople(); t.Deactivate(); });
        voiceChannelList.Where(t => t.Name != "Modded Alfa").ToList().ForEach(t => { Log(t.ToString()); });
        

        ShiftChannels();

        //changing kuro pfp
        Transform textChannelsTransform = transform.Find("Module Active State/Text Channels");
        MeshRenderer kuroPfp = transform.Find("Module Active State/Profile/PFP").GetComponent<MeshRenderer>();

        int num;
        //reroll for devious is Curl is not there
        bool foundCurl = voiceChannelList.Any(vc => vc.people.Any(person => person.Name == "CurlBot"));
        Debug.Log("Found curl: " + foundCurl);
        do
        {
            num = Rnd.Range(0, 5);
            currentMood = (Enums.Mood)num;
        } while (currentMood == Enums.Mood.Devious && !foundCurl);
        kuroPfp.material = kuroMoods[num];

        

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
        moddedAlfaTransform.GetComponent<KMSelectable>().OnInteract += delegate () { if (moduleActivated && !pause) { StartCoroutine(OnModdedAlfa()); } return false; };

        KMSelectable endCallButton = transform.Find("Module Active State/Call/end call").GetComponent<KMSelectable>();
        endCallButton.OnInteract += delegate () { if (moduleActivated && !pause) { CallButtonClicK(); } return false; };
        endCallButton.OnHighlight += () =>
        {
            endCallButton.transform.GetComponent<SpriteRenderer>().color = Color.red;
        };
        endCallButton.OnHighlightEnded += () =>
        {
            endCallButton.transform.GetComponent<SpriteRenderer>().color = Color.white;
        };
    }

    private string GetGroupModuleString()
    {
        return onBombKuroModules.GroupBy(name => name).Select(kv => $"{kv.Key} ({kv.Count()})").Join(", ");
    }

    private void CallButtonClicK()
    {
        if (ModuleSolved) return;

        switch (desiredTask)
        {
            case Enums.Task.CreateModule:
                if (onBombKuroModules.Count > 0)
                {
                    Strike("Tried to leave the call before all modules were solved");
                }

                else
                {
                    Audio.PlaySoundAtTransform(audioClips[1].name, transform);
                    Solve("Left call after solving all required modules. Solving module...");
                }
                break;

            case Enums.Task.Eat:

                break;
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

    private void ChangeVoiceChannelText(string name)
    {
        transform.Find("Module Active State/Call/Status").GetComponent<TextMesh>().text = $"{name} / KTANE";
    }

    private void EnableModuleActive(bool enable)
    {
        transform.Find("Module Active State").gameObject.SetActive(enable);
    }

    private void EnableVoiceGameObject(bool enable)
    {
        transform.Find("Module Active State/Call").gameObject.SetActive(enable);
    }
    private void EnableModIdeas(bool enable)
    {
        transform.Find("Module Active State/Mod Ideas").gameObject.SetActive(enable);
    }

    private void EnableRepoRequest(bool enable)
    {
        transform.Find("Module Active State/Repo Request").gameObject.SetActive(enable);
    }
    private void EnableFood(bool enable) {
        transform.Find("Module Active State/Eat").gameObject.SetActive(enable);
    }

    private IEnumerator GetFood(VoiceChannel vc)
    {
        yield return new WaitForSeconds(audioClips[0].length); //wait for join sound to end
        string[] foods = new string[] { "eggs", "a fab lolly", "pasta" };
        AudioClip[] foodClips = new AudioClip[] { audioClips[2], audioClips[3], audioClips[4] }; //eggs , fab lolly, pasta
        int foodIndex = Rnd.Range(0, 3);
        int correctIndex = Rnd.Range(0, 3);
        Audio.PlaySoundAtTransform(foodClips[foodIndex].name, transform);
        vc.EnableSpeaking(true);
        EnableSpeaking(true);
        yield return new WaitForSeconds(foodClips[foodIndex].length);
        vc.EnableSpeaking(false);
        EnableSpeaking(false);
        KMSelectable[] foodButtons = new KMSelectable[] { transform.GetComponent<KMSelectable>().Children[14], transform.GetComponent<KMSelectable>().Children[15], transform.GetComponent<KMSelectable>().Children[16] };
        Material correctMaterial = null;
        Material[] wrongMaterials = null;
        switch (foodIndex)
        {
            case 0:
                correctMaterial = eggs.PickRandom();
                wrongMaterials = others.Concat(fabLollies).Concat(pasta).ToArray();
                break;
            case 1:
                correctMaterial = fabLollies.PickRandom();
                wrongMaterials = others.Concat(eggs).Concat(pasta).ToArray();

                break;
            case 2:
                correctMaterial = pasta.PickRandom();
                wrongMaterials = others.Concat(eggs).Concat(fabLollies).ToArray();
                break;
        }
        wrongMaterials = wrongMaterials.Shuffle().Take(3).ToArray();
        for (int i = 0; i < 3; i++)
        {
            int dummy = i;
            if (correctIndex == dummy)
            {
                foodButtons[dummy].GetComponent<MeshRenderer>().sharedMaterial = correctMaterial;
                foodButtons[dummy].OnInteract += delegate () { if (moduleActivated && !pause) { Solve($"You chose {foodButtons[dummy].name}. This is correct"); } return false; };

            }
            else
            {
                foodButtons[dummy].GetComponent<MeshRenderer>().sharedMaterial = wrongMaterials[dummy];
                foodButtons[dummy].OnInteract += delegate () { if (moduleActivated && !pause) { Strike($"You chose {foodButtons[dummy].name}. This is incorrect"); } return false; };
            }
        }

        Log($"Kuro wants {foods[foodIndex]}");
        Log($"The displayed foods are {foodButtons.Select(f => f.GetComponent<MeshRenderer>().sharedMaterial.name).Join(", ")}");
        EnableFood(true);
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
        people.ForEach(person => person.SetTolerance(desiredTime.DayOfWeek));

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
        {
            desiredTask = Enums.Task.Eat;
        }


        Log($"It's {FormatHourMinute(desiredTime)}. You should be {GetTask(desiredTask)}");
        moduleActivated = true;
    }


    private void ShiftChannels()
    {
        Transform voiceChannelTransform = transform.Find("Module Active State/Voice Channels");
        Transform chillZoneAlfaTransform = voiceChannelTransform.Find("Chill Zone Alfa");
        Transform chillZoneBravoTransform = voiceChannelTransform.Find("Chill Zone Bravo");
        Transform chillZoneCharlieTransform = voiceChannelTransform.Find("Chill Zone Charlie");
        Transform moddedAlfaTransform = voiceChannelTransform.Find("Modded Alfa");
        int moddedPeople = voiceChannelList[3].people.Count;
        int alfaPeople = voiceChannelList[0].people.Count;
        int bravoPeople = voiceChannelList[1].people.Count;

        Vector3 alfaVector = chillZoneAlfaTransform.localPosition;
        chillZoneAlfaTransform.localPosition = new Vector3(alfaVector.x, alfaVector.y, -0.0531f + (channelOffset * moddedPeople));
        Vector3 bravoVector = chillZoneBravoTransform.localPosition;
        chillZoneBravoTransform.localPosition = new Vector3(bravoVector.x, bravoVector.y, -0.059f + (channelOffset * (moddedPeople + alfaPeople)));
        Vector3 charlieVector = chillZoneCharlieTransform.localPosition;
        chillZoneCharlieTransform.localPosition = new Vector3(charlieVector.x, charlieVector.y, -0.0655f + (channelOffset * (moddedPeople + alfaPeople + bravoPeople)));
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

        currentTextLocation = Enums.TextLocation.ModIdeas;
        Person[] modIdeaPeople = new Person[3];
        string[] loves = new string[] { "Polish music", "skating", "yellow", "programming", "modeling" };

        List<string> requests = new List<string>();
        List<bool> isNeedy = new List<bool>();
        List<bool> isLoved = new List<bool>();
        List<bool> isCurl = new List<bool>();



        do
        {
            //generating requests
            for (int i = 0; i < 3; i++)
            {
                string moduleType = new string[] { "needy", "solvable", "boss" }.PickRandom();
                int index = Rnd.Range(0, 4);
                switch (index)
                {
                    case 0:
                        requests.Add($"A {moduleType} module that is about the color {new string[] { "red", "orange", "yellow", "green", "blue", "purple" }.PickRandom()}");
                        break;
                    case 1:
                        requests.Add($"A {moduleType} module that is about {new string[] { "running", "longboarding", "hockey", "curling", "biking", "skating" }.PickRandom()}");
                        break;
                    case 2:
                        requests.Add($"A {moduleType} module that is about {new string[] { "German", "French", "Italian", "Spanish", "Chinese", "Polish" }.PickRandom()} music");
                        break;
                    case 3:
                        requests.Add($"A {moduleType} module that is about {new string[] { "animating", "programming", "modeling", "writing", "composing" }.PickRandom()}");
                        break;
                }
            }

            if (requests.Distinct().Count() != 3)
            { 
                requests.Clear();
            }

        } while (requests.Count == 0);
        
        do
        {
            modIdeaPeople = new Person[] { people.PickRandom(), people.PickRandom(), people.PickRandom() };
        } while (modIdeaPeople.Distinct().Count() != 3);


        for (int i = 0; i < 3; i++)
        {
            Person p = modIdeaPeople[i];
            string request = requests[i];
            Transform personTransform = transform.Find($"Module Active State/Mod Ideas/Person {i + 1}");
            personTransform.Find("PFP").GetComponent<MeshRenderer>().material = p.ProfilePicture;
            personTransform.Find("Name").GetComponent<TextMesh>().text = p.Name;
            personTransform.Find("Request").GetComponent<TextMesh>().text = request;
            Log($"{p.Name} requested: \"{request}\"");
            isNeedy.Add(request.Contains("needy"));
            isLoved.Add(loves.Any(l => request.Contains(l)));
            isCurl.Add(p.Name == "CurlBot");


        }

        List<int> correctIndex = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            if (!isNeedy[i] && isLoved[i] && !isCurl[i])
            {
                correctIndex.Add(i);
            }
        }

        if (correctIndex.Count > 0)
        {
            if (correctIndex.Count == 1)
            {
                Log($"{modIdeaPeople[correctIndex[0]].Name} suggested an idea that Kuro loves and is not a needy. That is the desired request");

            }

            else
            {
                string[] names = correctIndex.Select(i => modIdeaPeople[i].Name).ToArray();
                Log($"{string.Join(", ", names, 0, names.Length - 1)}, and {names.Last()} suggested ideas that Kuro loves and are not needies. Those are the desired requests");

            }

        }

        else
        {
            for (int i = 0; i < 3; i++)
            {
                if (!isNeedy[i] && !isCurl[i])
                {
                    correctIndex.Add(i);
                }
            }

            if (correctIndex.Count > 0)
            {
                if (correctIndex.Count == 1)
                {
                    Log($"{modIdeaPeople[correctIndex[0]].Name} suggested an idea that is not a needy. That is the desired request");

                }

                else
                {
                    string[] names = correctIndex.Select(i => modIdeaPeople[i].Name).ToArray();
                    Log($"{string.Join(", ", names, 0, names.Length - 1)} and {names.Last()} suggested ideas that are not needies. Those are the desired requests");
                }

            }

            else
            {
                for (int i = 0; i < 3; i++)
                {
                    if (!isCurl[i])
                    {
                        correctIndex.Add(i);
                    }
                }

                string[] names = correctIndex.Select(i => modIdeaPeople[i].Name).ToArray();
                Log($"{string.Join(", ", names, 0, names.Length - 1)} and {names.Last()} are not CurlBot. Choose one of their requests");
            }
        }


        for (int i = 0; i < 3; i++)
        {
            KMSelectable button = transform.Find($"Module Active State/Mod Ideas/Person {i + 1}/PFP").GetComponent<KMSelectable>();
            int dummy = i;
            if (correctIndex.Contains(i))
            {
                button.OnInteract += delegate () { if (moduleActivated && !pause) { Solve($"You chose {modIdeaPeople[dummy].Name}'s idea. This is correct"); } return false; };
            }

            else
            {
                button.OnInteract += delegate () { if (moduleActivated && !pause) { Strike($"You chose {modIdeaPeople[dummy].Name}'s idea. This is incorrect"); } return false; };
            }
        }

        EnableModIdeas(true);
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
            Person[] repoRequestPeople;
            int[] repoRequestValue = new int[3];
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
            Text[] requestsText = new Text[3];
            TextMesh[] nameText = new TextMesh[3];
            for (int i = 0; i < 3; i++)
            {
                Transform personTransform = transform.Find($"Module Active State/Repo Request/Person {i + 1}");
                requestsText[i] = personTransform.Find("Request/Text").GetComponent<Text>();
                pfpMeshRenderers[i] = personTransform.Find("PFP").GetComponent<MeshRenderer>();
                nameText[i] = personTransform.Find("Name").GetComponent<TextMesh>();
            }

            do
            {
                repoRequestPeople = Enumerable.Range(1, 3).Select(i => people.PickRandom()).ToArray();
            }
            while (repoRequestPeople.Distinct().Count() != 3);
            for (int i = 0; i < 3; i++)
            {
                //setting up request and people
                Person p = repoRequestPeople[i];
                pfpMeshRenderers[i].material = p.ProfilePicture;
                nameText[i].text = p.Name;
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
                int maxValue = repoRequestValue.Max();
                List<int> correctIndicies = repoRequestValue.Select((value, index) => value == maxValue ? index : -1).Where(index => index != -1).ToList();
                Debug.Log($"You should choose: {correctIndicies.Select(i => repoRequestPeople[i].Name).ToArray().Join(", ")}");

                for (int i = 0; i < 3; i++)
                {
                    int dummy = i;
                    if (correctIndicies.Contains(i))
                    {
                        transform.Find($"Module Active State/Repo Request/Person {i + 1}/PFP").GetComponent<KMSelectable>().OnInteract += delegate () { if (moduleActivated && !pause) { Solve($"You chose {repoRequestPeople[dummy].Name}. That is correct"); } return false; };
                    }

                    else
                    { 
                        transform.Find($"Module Active State/Repo Request/Person {i + 1}/PFP").GetComponent<KMSelectable>().OnInteract += delegate () { if (moduleActivated && !pause) { Strike($"You chose {repoRequestPeople[dummy].Name}. That is incorrect"); } return false; };
                    }
                }
            }

            EnableRepoRequest(true);
            currentTextLocation = Enums.TextLocation.RepoRequest;
        }
    }

    private void MoveToVoiceChannel(VoiceChannel vc, string name = "Kuro")
    {
        pause = true;
        Audio.PlaySoundAtTransform(audioClips[0].name, transform);
        if (name == "Kuro")
        {
            Person kuro = new Person(kuroMoods[(int)currentMood]);
            kuro.Name = "Kuro";
            vc.people.Add(kuro);
        }

        else
        {
            Person person = people.First(p => p.Name == name);
            people.Remove(person);
            vc.people.Add(person);
        }

        ShiftChannels();
        vc.DisplayPeople();
        EnableVoiceGameObject(true);
        ChangeVoiceChannelText(vc.Name);
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

    private IEnumerator OnModdedAlfa()
    {
        float time = 1f;
        if (desiredTask != Enums.Task.PlayKTANE)
        {
            WrongChannel("Modded Alfa");
            yield break;
        }

        VoiceChannel vc = voiceChannelList[3];
        MoveToVoiceChannel(vc, people.PickRandom().Name);
            for (int i = 0; i < 2; i++)
            {
                yield return new WaitForSeconds(time);
                MoveToVoiceChannel(vc, people.PickRandom().Name);
            }
    }

    private void OnChillZoneAlfa()
    {
        if (!(desiredTask == Enums.Task.Eat && desiredVCs.Contains(voiceChannelList[0])) && !(desiredTask == Enums.Task.CreateModule && onBombKuroModules.Count > 0))
        {
            WrongChannel("Chill Zone Alfa");
            return;
        }

        MoveToVoiceChannel(voiceChannelList[0]);
        if (desiredTask == Enums.Task.Eat)
        {
            StartCoroutine(GetFood(voiceChannelList[0]));
        }
    }

    private void OnChillZoneBravo()
    {
        if (!(desiredTask == Enums.Task.Eat && desiredVCs.Contains(voiceChannelList[1])))
        {
            WrongChannel("Chill Zone Bravo");
            return;
        }
        MoveToVoiceChannel(voiceChannelList[1]);
        StartCoroutine(GetFood(voiceChannelList[1]));
    }

    private void OnChillZoneCharlie()
    {
        if (!(desiredTask == Enums.Task.Eat && desiredVCs.Contains(voiceChannelList[2])))
        {
            WrongChannel("Chill Zone Charlie");
            return;
        }
        MoveToVoiceChannel(voiceChannelList[2]);
        StartCoroutine(GetFood(voiceChannelList[2]));
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

    private void EnableSpeaking(bool enabled)
    {
        transform.Find("Module Active State/Profile/Call Background").gameObject.SetActive(enabled);
    }

    private void WrongChannel(string location)
    {
        Strike($"You don't need to go to {location}. Strike!");
    }

    private void Solve(string s)
    {
        if (s != "")
            Log(s);

        EnableModuleActive(false);
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
