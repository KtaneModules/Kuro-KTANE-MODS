using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enums {

    public enum Task
    {
        CreateModule,
        MaintainRepo,
        Eat,
        PlayKTANE,
        Bed
    }

    public enum Mood
    {
        Angry,
        Curious,
        Devious,
        Happy,
        Neutral,
        Random
    }

    public enum TextLocation
    { 
        General,
        ModIdeas,
        RepoRequest,
        VoiceTextModded,
        None,
    }

    public enum VoiceLocation 
    {
        ModdedAlfa,
        ChillZoneAlfa,
        ChillZoneBravo,
        ChillZoneCharlie,
        None
    }

    public enum Role
    { 
        Defuse,
        Expert,
        None
    }

    public enum Activity
    {
        VC,
        Game,
        Song
    }
}
