using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoiceChannel 
{
    public List<Person> people;
    private GameObject[] peopleGameObjects;
    private TextMesh textMesh;
    private GameObject highlight;
    private Color defaultColor = new Color(148f / 255, 155f / 255, 164f / 255);
    public string Name { get; private set; }
    private bool active;
    public bool Active { get { return active; } }

    public VoiceChannel(GameObject gameObject, string name)
    {
        Name = name;
        textMesh = gameObject.transform.Find("label").GetComponent<TextMesh>();
        highlight = gameObject.transform.Find("background").gameObject;
        people = new List<Person>();

        int length = 4;
        peopleGameObjects = new GameObject[length];

        for (int i = 0; i < length; i++)
        {
            peopleGameObjects[i] = gameObject.transform.Find($"Person {i + 1}").gameObject;
        }
    }

    public void Activate()
    {
        highlight.SetActive(true);
        active = true;
    }

    public void Deactivate() 
    {
        highlight.SetActive(false);
        active = false;
    }

    public override string ToString()
    {
        return $"This vc has {string.Join(", ", people.Select(x => x.Name).ToArray())}";
    }

    public void DisplayPeople()
    {
        for (int i = 0; i < people.Count; i++)
        {
            Person p = people[i];
            GameObject gameObject = peopleGameObjects[i];
            MeshRenderer meshRenderer = gameObject.transform.Find("PFP").GetComponent<MeshRenderer>();
            TextMesh textMesh = gameObject.transform.Find("Name").GetComponent<TextMesh>();

            meshRenderer.material = p.ProfilePicture;
            textMesh.text = p.Name;
        }

        for (int i = 0; i < peopleGameObjects.Length; i++)
        {
            peopleGameObjects[i].SetActive(i < people.Count);
        }
    }

}
