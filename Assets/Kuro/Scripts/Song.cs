using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Song {

	public string Name  { get; private set; }
    public string Author { get; private set; }

	public Texture2D Image  { get; private set; }

	public Song(string name, string author, Texture2D image) 
	{
		Name = name;
        Author = author;
		Image = image;
    }
}
