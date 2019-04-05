using UnityEngine;

public class QueueItem
{
    public QueueItem(string a, GameObject t)
    {
        Action = a;
        Target = t;
    }

    public string Action;
    public GameObject Target;
}
