using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Song {

	public string Name  { get; private set; }
    public string Author { get; private set; }

	public Sprite Image  { get; private set; }

	public Song(string name, string author, Sprite image) 
	{
		Name = name;
        Author = author;
		Image = image;
    }
}
