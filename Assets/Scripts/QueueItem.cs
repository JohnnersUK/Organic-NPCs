using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueItem
{
    public string Action;
    public GameObject Target;

    public QueueItem(string a, GameObject t)
    {
        Action = a;
        Target = t;
    }
}
