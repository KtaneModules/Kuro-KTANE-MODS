using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class Kuro : MonoBehaviour {

    //todo make text on module a bit bigger X
    //todo fix pfps looking the opposite way
    //todo calcuate the correct time X
    //todo based on the correct time, make them do the correct thing
    //todo maintaining the repo

    private List<VoiceChannel> voiceChannels; //chillZoneAlfa, chillZoneBravo, chillZoneCharlie

    public GameObject chillZoneAlfaGameObject;
    public GameObject chillZoneBravoGameObject;
    public GameObject chillZoneCharlieGameObject;

    public KMSelectable modIdeasButton;
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

    public Material[] kuroMoods;
    private Material currentKuroMood;

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


    void Awake() {

        BombInfo = GetComponent<KMBombInfo>();
        Audio = GetComponent<KMAudio>();
        BombModule = GetComponent<KMBombModule>();
        ModuleId = ModuleIdCounter++;

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
        voiceChannels = new List<VoiceChannel>() { new VoiceChannel(chillZoneAlfaGameObject), new VoiceChannel(chillZoneBravoGameObject), new VoiceChannel(chillZoneCharlieGameObject) };

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
            VoiceChannel vc = voiceChannels[i];
            for (int j = 0; j < vcCount[i]; j++)
            {
                Person p = notInVcsPeople[Rnd.Range(0, notInVcsPeople.Count)];
                notInVcsPeople.Remove(p);
                vc.AddPerson(p);
            }
        }

        voiceChannels.ForEach(x => { x.DisplayPeople(); });

        const float offset = -0.0081f;

        int alfaPeople = voiceChannels[0].PeopleCount;
        int bravoPeople = voiceChannels[1].PeopleCount;

        Vector3 bravoVector = chillZoneBravoGameObject.transform.localPosition;
        chillZoneBravoGameObject.transform.localPosition = new Vector3(bravoVector.x, bravoVector.y, bravoVector.z + (offset * alfaPeople));
        Vector3 charlieVector = chillZoneCharlieGameObject.transform.localPosition;
        chillZoneCharlieGameObject.transform.localPosition = new Vector3(charlieVector.x, charlieVector.y, charlieVector.z + (offset * (alfaPeople + bravoPeople)));


        //changing kuro pfp
        GameObject textChannels = transform.Find("Text Channels").gameObject;
        MeshRenderer kuroPfp = transform.Find("Profile").Find("PFP").GetComponent<MeshRenderer>();
        currentKuroMood = kuroMoods[Rnd.Range(0, kuroMoods.Length)];
        kuroPfp.material = currentKuroMood;

        //setting text channels
        generalTextChannel = CreateTextChannel(textChannels.transform.Find("general").gameObject);
        modIdeasTextChannel = CreateTextChannel(textChannels.transform.Find("mod ideas").gameObject);
        repoRequestTextChannel = CreateTextChannel(textChannels.transform.Find("repo request").gameObject);
        voiceTextModdedTextChannel = CreateTextChannel(textChannels.transform.Find("voice text modded").gameObject);
        generalTextChannel.Activate();
        modIdeasTextChannel.Deactivate();
        repoRequestTextChannel.Deactivate();
        voiceTextModdedTextChannel.Deactivate();

        //setting buttons
        modIdeasButton.OnInteract += delegate () { if (moduleActivated) { OnModIdeas(); }  return false; };
        repoRequestButton.OnInteract += delegate () { if (moduleActivated) { OnRepoRequest(); } return false; }; ;
        voiceTextModdedButton.OnInteract += delegate () { if (moduleActivated) { OnVoiceTextModded(); } return false; }; ;
        moddedAlfaButton.OnInteract += delegate () { if (moduleActivated) { OnModdedAlfa(); } return false; }; ;
        chillZoneAlfaButton.OnInteract += delegate () { if (moduleActivated) { OnChillZoneAlfa(); } return false; }; ;
        chillZoneBravoButton.OnInteract += delegate () { if (moduleActivated) { OnChillZoneBravo(); } return false; }; ;
        chillZoneCharlieButton.OnInteract += delegate () { if (moduleActivated) { OnChillZoneCharlie(); } return false; }; ;
    }

    void Start()
    {
        BombModule.OnActivate += OnActivate;
    }

    void OnActivate()
    {
        //calculations start here
        currentTime = DateTime.Now;
        DayOfWeek day = currentTime.DayOfWeek;
        people.ForEach(person => person.SetTolerance(day));

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
        if (indicatorCount > batteryCount && indicatorCount > portCount)
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
        return string.Format("{0:00}:{1:00}", dateTime.Hour, dateTime.Minute);
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
