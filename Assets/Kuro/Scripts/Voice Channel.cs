using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoiceChannel 
{
    private List<Person> people;
    private GameObject[] peopleGameObjects;
    public int PeopleCount { get { return people.Count; } }
    private TextMesh textMesh;
    private GameObject highlight;
    private Color defaultColor = new Color(148f / 255, 155f / 255, 164f / 255);

    public VoiceChannel(GameObject gameObject)
    {
        textMesh = gameObject.transform.Find("label").GetComponent<TextMesh>();
        highlight = gameObject.transform.Find("background").gameObject;
        people = new List<Person>();

        peopleGameObjects = new GameObject[3];

        for (int i = 0; i < 3; i++)
        {
            peopleGameObjects[i] = gameObject.transform.Find($"Person {i + 1}").gameObject;
        }
    }

    public void Activate()
    {
        textMesh.color = Color.white;
        highlight.SetActive(true);
    }

    public void Deactivate() 
    {
        textMesh.color = defaultColor;
        highlight.SetActive(false);
    }

    public void AddPerson(Person person)
    {
        people.Add(person);
    }

    public override string ToString()
    {
        return $"This vc has {string.Join(", ", people.Select(x => x.Name).ToArray())}";
    }

    public void DisplayPeople()
    {
        for (int i = 0; i < PeopleCount; i++)
        {
            Person p = people[i];
            GameObject gameObject = peopleGameObjects[i];
            MeshRenderer meshRenderer = gameObject.transform.Find("PFP").GetComponent<MeshRenderer>();
            TextMesh textMesh = gameObject.transform.Find("Name").GetComponent<TextMesh>();

            meshRenderer.material = p.ProfilePicture;
            textMesh.text = p.Name;
        }

        for(int i = PeopleCount; i < 3; i++) 
        {
            peopleGameObjects[i].SetActive(false);
        }
    }

}
