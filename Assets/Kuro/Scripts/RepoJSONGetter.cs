using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RepoJSONGetter : MonoBehaviour {


    public static bool Success = false;
    public static bool Loading = false;
    public static bool LoadingDone = false;
    private static bool Error;

    private List<KtaneModule> allModules;

    //Stores the URL of the JSON that is grabbed.
    private string url = "https://ktane.timwi.de/json/raw";
    //private string url = "https://ktane-mods.github.io/Weakest-Link-Data/data.json";


    public static List<string> ModuleNames;
    public static List<string> kuroModules;
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
            Debug.Log("There was an error getting the data");
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
                allModules = RepoJSONParser.ParseRaw(request.text);
                ModuleNames = allModules.Select(mod => mod.Name).ToList();
                kuroModules = allModules.Where(mod => mod.Contributors != null && mod.Contributors.Developer != null && mod.Contributors.Developer.Contains("Kuro")).Select(mod => mod.Name).ToList();
            }

            catch
            {
                Debug.Log("There was an error parsing the data");
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
