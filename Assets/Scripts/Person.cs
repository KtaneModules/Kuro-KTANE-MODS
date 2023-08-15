using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person {
    public string Name { get; private set; }
    public string Id { get; private set; }

    public Person(string name, string id) 
    {
        Name = name;
        Id = id;
    }
}
