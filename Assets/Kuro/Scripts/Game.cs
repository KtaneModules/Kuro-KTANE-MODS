using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game {
    public string Name { get; private set; }
    public Image Image { get; private set; }

    public Game(string name, Image image) 
    {
        Name = name;
        Image = image;
    }
}
