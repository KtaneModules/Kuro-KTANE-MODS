using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoiceChannel 
{
    private List<Person> people;
    private GameObject[] peopleGameObjects;
    public int PeopleCount { get { return people.Count; } }

    public VoiceChannel(GameObject[] peopleGameObjects)
    {
        people = new List<Person>();
        this.peopleGameObjects = peopleGameObjects;
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
        int maxIndex = Mathf.Min(PeopleCount, peopleGameObjects.Length);

        for (int i = 0; i < maxIndex; i++)
        {
            Person p = people[i];
            MeshRenderer meshRenderer = peopleGameObjects[i].transform.Find("PFP").GetComponent<MeshRenderer>();
            TextMesh textMesh = peopleGameObjects[i].transform.Find("Name").GetComponent<TextMesh>();

            meshRenderer.material = p.ProfilePicture;
            textMesh.text = p.Name;
        }
    }

}
