using System;
using UnityEngine;

[Serializable]
public class RegimentTask
{
    public TaskType taskType = TaskType.HoldPosition;
    public Vector2 destination; // Vector2 is used, thus its y coordinate corresponds actually to z world axis
}
