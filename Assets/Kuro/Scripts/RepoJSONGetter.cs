using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

public class RepoJSONGetter : MonoBehaviour {


    public static bool Success = false;
    public static bool Loading = false;
    public static bool LoadingDone = false;
    private static bool Error;

    //Stores the URL of the JSON that is grabbed.
    private string url = "https://ktane.timwi.de/json/raw";
    //private string url = "https://ktane-mods.github.io/Weakest-Link-Data/data.json";


    public static List<string> ModuleNames;

    public IEnumerator LoadData()
    {
        Loading = true;

        WWW request = null;
        try
        {
            request = new WWW(url);
        }

        catch
        {
            Error = true;
        }

        if (Error)
        {
            Loading = false;
            LoadingDone = true;
            yield break;
        }

        //Waits until the WWW request returns the JSON file.
        yield return request;

        //If an error occurs
        if (request.error != null)
        {
            Success = false;
        }

        else
        {
            try
            {
                List<KtaneModule> allData = RepoJSONParser.ParseRaw(request.text);
                ModuleNames = allData.Select(mod => mod.Name).ToList();
            }

            catch
            {
                Success = false;
                Loading = false;
                LoadingDone = true;
                yield break;
            }

            Success = true;
        }

        Loading = false;
        LoadingDone = true;
    }    
}
