using System;
using System.Collections.Generic;
using UnityEngine;

public class Person {
    public string Name { get; set; }
    public Texture2D ProfilePicture { get; private set; }
    public int Tolerance { get; private set; }
    public List<Song> Songs { get; }
    public List<Texture2D> Games { get; }
    public Texture2D SelectedGame { get; set; }
    public Enums.Role Role { get; set; }
    public Person(Texture2D picture)
    {
        Name = picture.name;
        ProfilePicture = picture;
        Role = Enums.Role.None;
        Songs = new List<Song>();
        Games = new List<Texture2D>();
    }

    public void SetTolerance(DayOfWeek day)
    {
        Tolerance = GetTolerance(day);
    }

    private int GetTolerance(DayOfWeek day)
    {
        switch (Name)
        {
            case "GoodHood":

                if (day == DayOfWeek.Sunday)
                {
                    return 3;
                }

                if (day == DayOfWeek.Monday)
                {
                    return -2;
                }

                if (day == DayOfWeek.Tuesday)
                {
                    return -1;
                }

                if(day == DayOfWeek.Wednesday)
                {
                    return 0;
                }

                if (day == DayOfWeek.Thursday)
                {
                    return 2;
                }

                if (day == DayOfWeek.Friday)
                {
                    return 1;
                }

                else //SAT
                {
                    return -3;
                }

            case "CurlBot":

                if (day == DayOfWeek.Sunday)
                {
                    return 2;
                }

                if (day == DayOfWeek.Monday)
                {
                    return 1;
                }

                if (day == DayOfWeek.Tuesday)
                {
                    return -3;
                }

                if (day == DayOfWeek.Wednesday)
                {
                    return -2;
                }

                if (day == DayOfWeek.Thursday)
                {
                    return 0;
                }

                if (day == DayOfWeek.Friday)
                {
                    return -1;
                }

                else //SAT
                {
                    return int.MinValue;
                }

            case "Hawker":

                if (day == DayOfWeek.Sunday)
                {
                    return -3;
                }

                if (day == DayOfWeek.Monday)
                {
                    return -1;
                }

                if (day == DayOfWeek.Tuesday)
                {
                    return -2;
                }

                if (day == DayOfWeek.Wednesday)
                {
                    return 3;
                }

                if (day == DayOfWeek.Thursday)
                {
                    return 1;
                }

                if (day == DayOfWeek.Friday)
                {
                    return 0;
                }

                else //SAT
                {
                    return 2;
                }

            case "Play":

                if (day == DayOfWeek.Sunday)
                {
                    return -1;
                }

                if (day == DayOfWeek.Monday)
                {
                    return 2;
                }

                if (day == DayOfWeek.Tuesday)
                {
                    return 1;
                }

                if (day == DayOfWeek.Wednesday)
                {
                    return -2;
                }

                if (day == DayOfWeek.Thursday)
                {
                    return 3;
                }

                if (day == DayOfWeek.Friday)
                {
                    return -3;
                }

                else //SAT
                {
                    return 0;
                }

            case "Ciel":
            case "Blaise":
            case "Piccolo":
            case "Acer":
            case "Kit":
            case "Camia":
                if (day == DayOfWeek.Sunday)
                {
                    return 0;
                }

                if (day == DayOfWeek.Monday)
                {
                    return -3;
                }

                if (day == DayOfWeek.Tuesday)
                {
                    return 3;
                }

                if (day == DayOfWeek.Wednesday)
                {
                    return 1;
                }

                if (day == DayOfWeek.Thursday)
                {
                    return -1;
                }

                if (day == DayOfWeek.Friday)
                {
                    return 2;
                }

                else //SAT
                {
                    return -2;
                }

            default: //Mar and Hazel
                return int.MinValue;
        }
    }

    

}
