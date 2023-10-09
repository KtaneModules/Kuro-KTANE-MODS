using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RepoJSONGetter : MonoBehaviour {


    public static bool Success = false;
    public static bool Loading = false;
    public static bool LoadingDone = false;

    //Stores the URL of the JSON that is grabbed.
    private string url = "https://ktane.timwi.de/json/raw";
    //private string url = "";


    public static List<string> ModuleNames;

    public IEnumerator LoadData()
    { 
        Loading = true;

        WWW request = new WWW(url);
        //Waits until the WWW request returns the JSON file.
        yield return request;

        //If an error occurs
        if (request.error != null)
        {
            Success = false;
        }

        else
        {
            Success = true;
            List<KtaneModule> allData = RepoJSONParser.ParseRaw(request.text);
            ModuleNames = allData.Select(mod => mod.Name).ToList();
        }

        Loading = false;
        LoadingDone = true;
    }
}
