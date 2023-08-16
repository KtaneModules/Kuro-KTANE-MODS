using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class Kuro : MonoBehaviour {

    //todo create a class of the voice channels
    //todo assign people to the chill vc randomly on start
    //todo fix pfps looking the opposite way

    private List<VoiceChannel> voiceChannels; //chillZoneAlfa, chillZoneBravo, chillZoneCharlie

    public GameObject[] chillZoneAlfaPeople;

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


    private DateTime currentTime;
    private KMBombInfo Bomb;
    private KMAudio Audio;

   static int ModuleIdCounter = 1;
   int ModuleId;
   private bool ModuleSolved;

    void Awake() {

        Bomb = GetComponent<KMBombInfo>();
        Audio = GetComponent<KMAudio>();
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
        voiceChannels = new List<VoiceChannel>() { new VoiceChannel(chillZoneAlfaPeople), new VoiceChannel(null), new VoiceChannel(null) };

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

        voiceChannels.ForEach(x => Debug.Log(x.ToString()));

        voiceChannels[0].DisplayPeople();


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


        /*
        foreach (KMSelectable object in keypad) {
            object.OnInteract += delegate () { keypadPress(object); return false; };
        }
        */

        //button.OnInteract += delegate () { buttonPress(); return false; };

    }

    void Start () {

   }

   void Update () {

   }

    void OnActivate()
    {
        //calculations start here
        currentTime = DateTime.Now;
        DayOfWeek day = currentTime.DayOfWeek;
        people.ForEach(person => person.SetTolerance(day));
    }

    public TextChannel CreateTextChannel(GameObject gameObject)
    {
        GameObject highlight = gameObject.transform.Find("highlight").gameObject;
        TextMesh textMesh = gameObject.transform.Find("label").GetComponent<TextMesh>();

        return new TextChannel(textMesh, highlight);
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
