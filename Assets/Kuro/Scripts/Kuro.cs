using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using UnityEngine.UI;
using static Enums;
using System.Xml.Linq;

public class Kuro : MonoBehaviour {

    //todo fix pfps looking the opposite way
    //x todo make text on module a bit bigger
    //x todo calcuate the correct time
    //x todo based on the correct time, make them do the correct thing
    //x todo - maintaining the repo
    //x todo - creating a module
    //x todo - eating
    //x todo - playing KTANE
    //x todo - getting ready for bed

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
    //x todo eat
    //x todo fix the logging with there being numbers for the food (stretch goal)
    //x todo- make it so a green circle appears when kuro is speaking
    //x todo - make it so when someone clicks on kuro, he will repeat what he said (stretch goal)
    //x todo - add kuro voice lines (stretch goal)
    //x todo - if call button is press prematurely, strike
    //x todo - put british flags on fab lollies
    //x todo play ktane
    //x todo - strike if voice-text-modded is pressed first
    //x todo - generate people wanting to play
    //x todo - fix bug in which people from other vcs go into modded alfa
    //x todo - fix bug where chill zones go overs the call
    //x todo - curl unicorn
    //x todo - kuro wants to defuse
    //x todo - kuro wants to expert
    //x todo - same role, less tolerance
    //x todo - same role, more tolerance
    //x todo - fix bug where kuro can join vc multiple times
    //x todo - kuro joins call
    //x todo - two people (who are not in other voice calls) join call
    //x todo - if call button is press prematurely, strike
    //x todo bed
    //x todo fix the bug of the time not being displayed properly in the log
    //x todo figure out why you got an out of range error from just loading the module 
    //x todo have a set up module method that will deal with what is shown and the buttons. (Call it in start before module loading starts)
    //x todo have the custom highlighting work with all voice / text channels
    //x todo have a loading state (that doesn't break all the kms)
    //x todo when a channel is active, deactivate the other one (fix a bug where the gray highlighting disappears when the highlight event ends)
    //x todo have people in vcs be in alphabetical order (stretch goal)

    //todo have a solved state where it shows people's game activity (stretch goal)
    //! todo - play a game
    //x todo - spotify
    //x todo - add curlbot images for spotify
    //x todo - make it so the spotify and game time shows how many minutes/hours it's been since the mod solved (stretch goal)
    //x todo - vc
    //todo change the discord leaving sound (stretch goal)
    //todo have the highlight/on hover work for all the voice/text channels (stretch goal)
    //todo fix text/voice channel highlights (stretch goal)
    //todo tp (stretch goal)
    //todo - regular (stretch goal)
    //todo - autosolve (stretch goal)

    private Activity[] activities;
    
    [SerializeField]
    private bool debug;
    [SerializeField]
    private Sprite spotifyLogo;

    [SerializeField]
    private Sprite[] songs; //alors, collared, die in a fire, funkytown, glass animals, golden afternoon, i should've known, paint it black, wavetapper

    [SerializeField]
    private AudioClip[] audioClips; //join, leave, eggs, fab lolly, pasta
    [SerializeField]
    private GameObject wariningSign;
    private static RepoJSONGetter jsonData;

    DateTime solveTime;

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

    private Mood currentMood;
    #endregion

    VoiceChannel[] desiredVCs;
    Person[] moddedAlfaPeople;
    Role correctRole = Role.None;
    private TextLocation currentTextLocation;
    private VoiceLocation currentVoiceLocation;
    private DateTime currentTime; //the time the bomb was activated
    private DateTime desiredTime; //th time used to figure out what to do
    private Task desiredTask; //the task needed to get done

    AudioClip[] foodClips;
    int foodIndex = -1; //the index of the food kuro wants

    private KMBombInfo BombInfo;
    private KMAudio Audio;
    private KMBombModule BombModule;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved, moduleActivated = false;
    private List<string> onBombKuroModules = new List<string>(); //all the modules on the bomb made by Kuro
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
        BombModule.OnActivate += delegate { StartCoroutine(OnActivate()); };

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
    }



    void Update()
    {
        if (!RepoJSONGetter.LoadingDone || !moduleActivated)
            return;
        if (ModuleSolved)
        {
            string actiityTime = GetActivityTime();

            for (int i = 0; i < 3; i++)
            {
                switch (activities[i])
                {
                    case Activity.VC:
                        continue;
                    case Activity.Game:
                        continue;
                    case Activity.Song:
                        transform.Find($"Solved State/Activity {i + 1}").Find("Canvas/Activity").GetComponent<Text>().text = $"Spotify - {GetActivityTime()}";
                        break;
                }
            }
        }

        else if (desiredTask == Task.CreateModule)
        {
            List<string> solvedModules = BombInfo.GetSolvedModuleNames();

            if (currentSolvedModules.Count != solvedModules.Count)
            {
                string solvedModule = GetLatestSolve(solvedModules, currentSolvedModules);

                if (onBombKuroModules.Contains(solvedModule))
                {
                    onBombKuroModules.Remove(solvedModule);
                    if (currentVoiceLocation != VoiceLocation.ChillZoneAlfa)
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
        EnablePlayKTANE(false);



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

        //add songs to people
        //acer, blaise, camia, ciel, hazel. kit, mar, piccolo
        people.Where(p => p.Name == "Acer" || p.Name == "Blaise" || p.Name == "Camia" || p.Name == "Ciel" || p.Name == "Hazel" || p.Name == "Kit" || p.Name == "Mar" || p.Name == "Piccolo").ToList().ForEach(p => p.Songs.AddRange(new Song[] { new Song("It Should've Been Me", "Riproducer", songs[8]), new Song("Collared", "Vane Lily", songs[1]), new Song("Golden Afternoon", "CircusP", songs[6]) }));
        people.First(p => p.Name == "Hawker").Songs.AddRange(new Song[] { new Song("Alors on danse", "Stromae", songs[0]), new Song("Poplar St", "Glass Animals", songs[5]), new Song("Pork Soda", "Glass Animals", songs[5]) });
        people.First(p => p.Name == "GoodHood").Songs.AddRange(new Song[] { new Song("Funky Town", "Lipps Inc.", songs[4]) });
        people.First(p => p.Name == "Play").Songs.AddRange(new Song[] { new Song("Paint It, Black", "The Rolling Stones", songs[9]), new Song("Die In A Fire", "Living Tombstone", songs[3]), new Song("Wavetapper", "Frums", songs[11]) });
        people.First(p => p.Name == "CurlBot").Songs.AddRange(new Song[] { new Song("Crazy", "Creo", songs[2]), new Song("Ordinary Day", "The Great Big Sea", songs[7]), new Song("Death By Glamour", "Toby Foxx", songs[10]) });

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

        for (int i = 0; i < 3; i++)
        {
            VoiceChannel vc = voiceChannelList[i];
            for (int j = 0; j < vcCount[i]; j++)
            {
                Person p = people.PickRandom();
                people.Remove(p);
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
        do
        {
            num = Rnd.Range(0, 5);
            currentMood = (Mood)num;
        } while (currentMood == Mood.Devious && !foundCurl);

        if (debug)
        {
            //currentMood = Mood.Curious;
            //num = (int)currentMood;
        }

        kuroPfp.material = kuroMoods[num];



        //set locations
        currentTextLocation = TextLocation.None;
        currentVoiceLocation = VoiceLocation.None;

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

        foodClips = new AudioClip[] { audioClips[2], audioClips[3], audioClips[4] }; //eggs , fab lolly, pasta
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
            case Task.CreateModule:
                if (onBombKuroModules.Count > 0)
                {
                    Strike("Tried to leave the call before all modules were solved");
                }

                else
                {
                    Audio.PlaySoundAtTransform(audioClips[1].name, transform);
                    Solve("Left call after solving all required modules.");
                }
                break;

            case Task.Eat:
                Strike("Can't leave the call yet");
                break;

            case Task.Bed:
                Audio.PlaySoundAtTransform(audioClips[1].name, transform);
                Solve("Left the call.");
                break;

            case Task.PlayKTANE:
                if (currentTextLocation != TextLocation.VoiceTextModded || (currentTextLocation == TextLocation.VoiceTextModded && correctRole != Role.None))
                {
                    Strike("Can't leave the call yet");
                }
                else
                {
                    Audio.PlaySoundAtTransform(audioClips[1].name, transform);
                    Solve("Left the call since CurlBot wanted to play.");
                }
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

    private void EnablePlayKTANE(bool enable)
    {
        transform.Find("Module Active State/Play KTANE").gameObject.SetActive(enable);
    }

    private IEnumerator GetFood(VoiceChannel vc)
    {
        yield return new WaitForSeconds(audioClips[0].length); //wait for join sound to end
        string[] foods = new string[] { "eggs", "a fab lolly", "pasta" };
        foodIndex = Rnd.Range(0, 3);
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
        List<string> newNames = new List<string>();

        for (int i = 0; i < foodButtons.Length; i++)
        {
            string oldName = foodButtons[i].GetComponent<MeshRenderer>().sharedMaterial.name;
            string newName = "";
            foreach (char c in oldName)
            {
                if (!Char.IsDigit(c))
                {
                    newName += c;
                }
            }

            newNames.Add(newName.Trim());
        }
        SetUpPfpButtons();
        Log($"The displayed foods are {newNames.Join(", ")}");
        EnableFood(true);
    }

    private void SetUpPfpButtons()
    {
        KMSelectable[] buttons = GetComponent<KMSelectable>().Children.Where((_, index) => index >= 19 && index <= 30).ToArray();
        foreach (KMSelectable b in buttons)
        {
            b.OnInteract += delegate { StartCoroutine(PfpButton(b)); return false; };
        }

    }


    private IEnumerator PfpButton(KMSelectable b)
    {
        if (pause || b.transform.parent.Find("Name").GetComponent<TextMesh>().text != "Kuro")
            yield break;

        pause = true;
        if (foodIndex < 0 || foodIndex >= foodClips.Length)
        { 
            pause = false;
            yield break;
        }
        EnableSpeaking(true);
        Audio.PlaySoundAtTransform(foodClips[foodIndex].name, transform);
        yield return new WaitForSeconds(foodClips[foodIndex].length);
        EnableSpeaking(false);
        pause = false;
    }

    IEnumerator OnActivate()
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
            Log("Neither indicator nor battery count were the highest. Minute offset is now " + minuteOffset);
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
            desiredTask = Task.MaintainRepo;
        else if (fullMiutes >= 780 && fullMiutes <= 959)
            desiredTask = Task.CreateModule;
        else if (fullMiutes >= 960 && fullMiutes <= 1139)
            desiredTask = Task.Eat;
        else if (fullMiutes >= 1140 && fullMiutes <= 1319)
            desiredTask = Task.PlayKTANE;
        else
            desiredTask = Task.Bed;

        if (debug)
        {
            desiredTask = Task.Bed;
        }


        Log($"It's {FormatHourMinute(desiredTime)}. You should be {GetTask(desiredTask)}");

        //get all the modules made by kuro
        List<string> allModules = BombInfo.GetSolvableModuleNames();
        List<string> kuroModules;


        do
        {
            yield return new WaitForSeconds(0.1f);

        } while (!RepoJSONGetter.LoadingDone);

        if (!RepoJSONGetter.Success)
        {
            kuroModules = "Technical Keypad|Procedural Maze|Blank Slate|Orientation Hypercube|Shy Guy Says|Samuel Says|Coloured Cubes".Split('|').ToList();
            Log("Data failed to load. List of Kuro modules: " + string.Join(", ", kuroModules.ToArray()));
            wariningSign.SetActive(true);
        }
        else
        {
            Log("Got data successfully. Using repositry to get a list of kuro modules");
            wariningSign.SetActive(false);
            kuroModules = RepoJSONGetter.kuroModules;
        }
        onBombKuroModules = allModules.Where(mod => kuroModules.Contains(mod)).OrderBy(q => q).ToList();
        currentSolvedModules = new List<string>();

        switch (desiredTask)
        {
            case Task.Eat:
                desiredVCs = new VoiceChannel[1];
                VoiceChannel[] chillZones = voiceChannelList.Where(vc => vc.Name != "Modded Alfa").ToArray();
                List<VoiceChannel> voiceChannelByOrder = voiceChannelList.Where(vc => vc.Name != "Modded Alfa").OrderByDescending(vc => vc.people.Count).ToList();

                if (currentMood == Mood.Happy)
                {
                    desiredVCs = new VoiceChannel[] { voiceChannelByOrder[0] };
                }
                else if (currentMood == Mood.Neutral)
                {
                    desiredVCs = new VoiceChannel[] { voiceChannelByOrder[1] };
                }
                else if (currentMood == Mood.Angry)
                {
                    desiredVCs = new VoiceChannel[] { voiceChannelByOrder[2] };
                }
                else if (currentMood == Mood.Devious)
                {
                    desiredVCs = new VoiceChannel[] { voiceChannelByOrder.First(vc => HasCurlBot(vc.people)) };
                }
                else if (currentMood == Mood.Curious)
                {
                    desiredVCs = voiceChannelByOrder.ToArray();
                }

                if (currentMood != Mood.Devious && currentMood != Mood.Curious && HasCurlBot(desiredVCs[0].people))
                {
                    desiredVCs[0] = chillZones[(Array.IndexOf(chillZones, desiredVCs[0]) + 1) % chillZones.Length];
                }
                Log($"You are {currentMood}. You are able to join {desiredVCs.Select(vc => vc.Name).Join(", ")}");

                break;


            case Task.MaintainRepo:
                Log("You must look at #repo-requests");
                break;
            case Task.CreateModule:
                if (onBombKuroModules.Count > 0)
                {
                    Log($"You must join Chill Zone Alfa. Then solve the following modules: {GetGroupModuleString()}");
                }
                else
                {
                    Log($"You must look at #mod-ideas");
                }
                break;

            case Task.PlayKTANE:
                Log("You must play join Modded Alfa and look at #voice-text-modded in that order");
                break;

            case Task.Bed:
                desiredVCs = voiceChannelList.Where(vc => vc.Name != "Modded Alfa" && vc.people.All(p => p.Name != "CurlBot")).ToArray();
                Log($"You are able to join {desiredVCs.Select(vc => vc.Name).Join(", ")}");
                break;
        }

        loadingState.SetActive(false);
        EnableModuleActive(true);
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

        if (desiredTask != Task.CreateModule || onBombKuroModules.Count > 0)
        {
            WrongChannel("#mod-ideas");
            return;
        }

        currentTextLocation = TextLocation.ModIdeas;
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
            personTransform.Find("Request/Text").GetComponent<Text>().text = request;
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
        if (currentTextLocation == TextLocation.RepoRequest)
        {
            return;
        }
        Log("You pressed #repo-request");
        currentTextLocation = TextLocation.RepoRequest;
        if (desiredTask != Task.MaintainRepo) 
        {
            WrongChannel("#repo-request");
            return;
        }

        if (currentTextLocation == TextLocation.None)
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
            currentTextLocation = TextLocation.RepoRequest;
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
            currentVoiceLocation = ObjectToEnum(vc);
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

    private VoiceLocation ObjectToEnum(VoiceChannel vc)
    {
        if (vc == voiceChannelList[0])
            return VoiceLocation.ChillZoneAlfa;
        if (vc == voiceChannelList[1])
            return VoiceLocation.ChillZoneBravo;
        if (vc == voiceChannelList[2])
            return VoiceLocation.ChillZoneCharlie;
        return VoiceLocation.ModdedAlfa;
    }

    private string GetTolerance(int num)
    {
        return num == int.MinValue ? "Skull" : num.ToString();
    }

    private void OnVoiceTextModded()
    {
        if (currentTextLocation == TextLocation.VoiceTextModded)
            return;
        Log("You pressed #voice-text-modded");
        if (desiredTask != Task.PlayKTANE || currentVoiceLocation != VoiceLocation.ModdedAlfa)
        {
            WrongChannel("#voice-text-modded");
            return;
        }

        Role kuroDesiredRole = currentMood == Mood.Happy || currentMood == Mood.Neutral ? Role.Defuse : Role.Expert;
        moddedAlfaPeople[0].Role = Rnd.Range(0, 2) == 0 ? Role.Defuse : Role.Expert;
        moddedAlfaPeople[1].Role = moddedAlfaPeople[0].Role == Role.Defuse ? Role.Expert : Role.Defuse;

        for (int i = 0; i < 2; i++)
        {
            Transform person = transform.Find($"Module Active State/Play KTANE/Person {i + 1}");
            person.Find("PFP").GetComponent<MeshRenderer>().material = moddedAlfaPeople[i].ProfilePicture;
            person.Find("Name").GetComponent<TextMesh>().text = moddedAlfaPeople[i].Name;

            Text request = person.Find("Request/Text").GetComponent<Text>();
            if (moddedAlfaPeople[i].Role == Role.Defuse)
            {
                request.text = "I would like to defuse";
            }

            else
            { 
                request.text = "I would like to expert";
            }
        }

        Log($"Kuro would like to {kuroDesiredRole}");
        for (int i = 0; i < 2; i++)
        {
            Log($"{moddedAlfaPeople[i].Name} would like to {moddedAlfaPeople[i].Role} with a tolerance of {GetTolerance(moddedAlfaPeople[i].Tolerance)}");
        }

        Person moreToleratedPerson = moddedAlfaPeople.OrderByDescending(p => p.Tolerance).First();

        //check if curlbot is in the call
        if (HasCurlBot(moddedAlfaPeople.ToList()))
        {
            correctRole = Role.None;
        }

        //If there is a tie, pick your desired role
        else if (moddedAlfaPeople.Count(p => p.Tolerance == moddedAlfaPeople[0].Tolerance) == 2)
        {
            correctRole = kuroDesiredRole;
        }

        //If the person you tolerate more prefers do the same role as you, pick the opposite role
        else if (moreToleratedPerson.Role == kuroDesiredRole)
        {
            correctRole = kuroDesiredRole == Role.Defuse ? Role.Expert : Role.Defuse;
        }

        //Otherwise, pick your desired role.
        else
        {
            correctRole = kuroDesiredRole;
        }

        string s = "";
        switch (correctRole)
        { 
            case Role.None:
                s = "leave the call";
                break;
            case Role.Expert:
                s = "expert";
                break;
            case Role.Defuse:
                s = "defuse";
                break;
        }

        Log($"You should {s}");

        transform.Find("Module Active State/Play KTANE/Defuse Button").GetComponent<KMSelectable>().OnInteract += delegate 
        { 
            Log("You pressed defuse");
            if (correctRole == Role.Defuse)
            {
                Solve("This is correct.");
            }
            else
            {
                Strike("This is incorrect.");
            } 
            return false; 
        };

        transform.Find("Module Active State/Play KTANE/Expert Button").GetComponent<KMSelectable>().OnInteract += delegate
        {
            Log("You pressed expert");
            if (correctRole == Role.Expert)
            {
                Solve("This is correct.");
            }
            else
            {
                Strike("This is incorrect.");
            }
            return false;
        };


        EnablePlayKTANE(true);
        currentTextLocation = TextLocation.VoiceTextModded;
    }

    private IEnumerator OnModdedAlfa()
    {
        if (currentVoiceLocation == VoiceLocation.ModdedAlfa)
            yield break;

        Log("You pressed Modded Alfa");
        float time = 1f;
        if (desiredTask != Task.PlayKTANE)
        {
            WrongChannel("Modded Alfa");
            yield break;
        }

        pause = true;
        VoiceChannel vc = voiceChannelList[3];
        MoveToVoiceChannel(vc);
        for (int i = 0; i < 2; i++)
        {
            pause = true;
            yield return new WaitForSeconds(time);
            MoveToVoiceChannel(vc, people.PickRandom().Name);
        }
        moddedAlfaPeople = vc.people.Where(p => p.Name != "Kuro").ToArray();

        List<Person> nonModdedPeople = new List<Person>();

        for (int i = 0; i < 3; i++)
        {
            nonModdedPeople.AddRange(voiceChannelList[i].people);
        }

        Log($"{moddedAlfaPeople[0].Name} and {moddedAlfaPeople[1].Name} have joined the call");
    }

    private void OnChillZoneAlfa()
    {
        OnChillZone(VoiceLocation.ChillZoneAlfa);
    }

    private void OnChillZoneBravo()
    {
        OnChillZone(VoiceLocation.ChillZoneBravo);
    }

    private void OnChillZoneCharlie()
    {
        OnChillZone(VoiceLocation.ChillZoneCharlie);
    }

    private void OnChillZone(VoiceLocation voiceLocation)
    {
        if (currentVoiceLocation == voiceLocation)
            return;

        VoiceChannel vc = voiceChannelList[(int)voiceLocation - 1];
        string name = $"Chill Zone {voiceLocation.ToString().Substring(9)}";

        Log($"You pressed {name}");

        if ((!((desiredTask == Task.Eat || desiredTask == Task.Bed) && desiredVCs.Contains(vc)) && !(desiredTask == Task.CreateModule && onBombKuroModules.Count > 0)) || currentVoiceLocation != VoiceLocation.None)
        {
            WrongChannel(name);
            return;
        }

        MoveToVoiceChannel(vc);
        if (desiredTask == Task.Eat)
        {
            StartCoroutine(GetFood(vc));
        }
    }

    private string FormatHourMinute(DateTime dateTime)
    {
        return $"{dateTime.DayOfWeek}, {dateTime.Hour:00}:{dateTime.Minute:00}";
    }


    private string GetTask(Task task) 
    {
        switch (task) 
        {
            case Task.MaintainRepo:
                return "maintaining the repo";
            case Task.CreateModule:
                return "creating a module";
            case Task.Eat:
                return "eating";
            case Task.PlayKTANE:
                return "playing KTANE";
            case Task.Bed:
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
        solveTime = DateTime.Now;
        if (s != "")
            Log(s + " Solving module...");

        //choose 3 random vcs that have people inside of them
        VoiceChannel[] populatedVCs = voiceChannelList.Where(vc => vc.people.Count > 0).ToArray().Shuffle();

        activities = new Activity[3];


        /* sierra system
         * outer wilds
         * rabbit and steel
         * celeste
         * 
         * 
         */
        for (int i = 0; i < 3; i++)
        {
            if (i == 1)
                break;
            VoiceChannel vc = populatedVCs[i];
            Transform activity = transform.Find($"Solved State/Activity {i + 1}");
            //randomize what to show
            Activity selectedActivity;
            selectedActivity = new Activity[] { Activity.VC, Activity.Game, Activity.Song }.Shuffle()[0];
            selectedActivity  = Activity.Song;
            activities[i] = selectedActivity;
            Person selectedPerson;
            Person[] peopleArr = vc.people.Where(p => p.Name != "Kuro").OrderBy(p => p.Name).ToArray();

            switch (selectedActivity)
            {
                case Activity.VC:
                    string inVCName = "";
                    //change the name of the people in the vc

                    switch (peopleArr.Length)
                    {
                        case 1:
                            inVCName = peopleArr[0].Name;
                            break;
                        case 2:
                            inVCName = $"{peopleArr[0].Name} and {peopleArr[1].Name}";
                            break;
                        case 3:
                            inVCName = $"{peopleArr[0].Name}, {peopleArr[1].Name}, and {peopleArr[2].Name}";
                            break;
                    }

                    activity.Find("Canvas/Name").GetComponent<Text>().text = inVCName;

                    //change name of the vc that people are in
                    activity.Find("Canvas/Sub Text").GetComponent<Text>().text = vc.Name;

                    //change the material
                    activity.Find("PFP").GetComponent<MeshRenderer>().material = peopleArr[0].ProfilePicture;

                    //change the pfp in the activity
                    for (int j = 0; j < 3; j++)
                    {
                        Transform t = activity.Find($"Detailed Activity/small pfp {j + 1}");
                        if (j >= peopleArr.Length)
                        {
                            t.gameObject.SetActive(false);
                        }
                        else
                        {
                            t.GetComponent<MeshRenderer>().material = peopleArr[j].ProfilePicture;
                        }
                    }
                    break;
                case Activity.Game:
                    //select a person
                    selectedPerson = peopleArr.Shuffle()[0];
                    //select game here

                    //disable Background
                    activity.Find("Canvas/Background").gameObject.SetActive(false);

                    //disable pfp
                    activity.Find("Canvas/PFP").gameObject.SetActive(false);

                    //Disable online
                    activity.Find("Canvas/Online").gameObject.SetActive(false);

                    //disable small pfps
                    Enumerable.Range(1, 3).ToList().ForEach(ix => activity.Find($"Detailed Activity/small pfp {ix}").gameObject.SetActive(false));

                    //disable spotify logo
                    activity.Find("Canvas/Spotify Logo").gameObject.SetActive(false);

                    //change media image to pfp
                    //disable name
                    //disable activity

                    //display name
                    //display time activity
                    //display game image

                    break;
                case Activity.Song:

                    //select a person 
                    selectedPerson = peopleArr.Shuffle()[0];
                    Song selectedSong = selectedPerson.Songs.Shuffle()[0];

                    //change activity name
                    activity.Find("Canvas/Activity").GetComponent<Text>().text = $"Spotify - {GetActivityTime()}";

                    //change the person's material
                    activity.Find("PFP").GetComponent<MeshRenderer>().material = selectedPerson.ProfilePicture;
                    
                    //change the person's Name
                    activity.Find("Canvas/Name").GetComponent<Text>().text = selectedPerson.Name;

                    //change the name of the song, the artist, and the cover
                    activity.Find("Canvas/Main Text").GetComponent<Text>().text = selectedSong.Name;
                    activity.Find("Canvas/Sub Text").GetComponent<Text>().text = selectedSong.Author;
                    activity.Find($"Detailed Activity/Media Image").GetComponent<SpriteRenderer>().sprite = selectedSong.Image;
                    
                    //disable the small pfps
                    Enumerable.Range(1, 3).ToList().ForEach(ix => activity.Find($"Detailed Activity/small pfp {ix}").gameObject.SetActive(false));
                    break;
            }
            
            //enable big spotify logo depending on if it's the song

        }




        EnableModuleActive(false);
        solvedState.SetActive(true);
        BombModule.HandlePass();
        ModuleSolved = true;
    }

    private string GetActivityTime()
    {
        TimeSpan diffTime = DateTime.Now - solveTime;

        Debug.Log(diffTime);

        if (diffTime.Days >= 1)
        {
            return $"{diffTime.Days}d";
        }

        if (diffTime.Hours >= 1)
        {
            return $"{diffTime.Hours}h";
        }

        if (diffTime.Minutes >= 1)
        {
            return $"{diffTime.Minutes}m";
        }

        return "just now";
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

    private bool HasCurlBot(List<Person> people)
    {
        return people.Any(p => p.Name == "CurlBot");
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
