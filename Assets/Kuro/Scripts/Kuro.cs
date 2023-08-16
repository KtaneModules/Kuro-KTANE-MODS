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
    //todo create the text channels objects
    //todo assign people to the chill vc randomly on start
    //todo fix pfps looking the opposite way

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

    private Person acer;
    private Person blaise;
    private Person camia;
    private Person ciel;
    private Person curl;
    private Person goodhood;
    private Person hawker;
    private Person hazel;
    private Person kit;
    private Person mar;
    private Person piccolo;
    private Person play;


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
        acer = new Person(acerPfp);
        blaise = new Person(blaisePfp);
        camia = new Person(cielPfp);
        curl = new Person(curlPfp);
        goodhood = new Person(goodhoodPfp);
        hawker = new Person(hawkerPfp);
        hazel = new Person(hazelPfp);
        kit = new Person(kitPfp);
        mar = new Person(marPfp);
        piccolo = new Person(piccoloPfp);
        play = new Person(playPfp);

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

        acer.SetTolerance(day);
        blaise.SetTolerance(day);
        camia.SetTolerance(day);
        curl.SetTolerance(day);
        goodhood.SetTolerance(day);
        hawker.SetTolerance(day);
        hazel.SetTolerance(day);
        kit.SetTolerance(day);
        mar.SetTolerance(day);
        piccolo.SetTolerance(day);
        play.SetTolerance(day);
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
