using System;

using UnityEngine;

public enum EventType
{
    Hit,
    Death,
    Other
}

public class PublicEventArgs : EventArgs
{
    public PublicEventArgs(GameObject a, GameObject s, EventType e, float r)
    {
        Agent = a;
        Subject = s;
        _eventType = e;
        Range = r;
    }

    public GameObject Agent { get; set; }
    public GameObject Subject { get; set; }
    public EventType _eventType;
    public float Range;
}