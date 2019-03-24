using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public enum EventType
{
    Hit,
    Death
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