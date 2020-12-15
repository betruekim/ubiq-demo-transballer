using UnityEngine;
using System.Collections.Generic;

public class HintManager : MonoBehaviour
{
    public static Dictionary<string, bool> complete = new Dictionary<string, bool>();
    public static Dictionary<string, bool> currentlyShowing = new Dictionary<string, bool>();

    public const string grasping = "graspObjects";
    public const string levelDoors = "levelDoors";
    public const string hoops = "hoops";
    public const string spawners = "spawners";
    public const string levelButton = "levelButton";
    public const string equipping = "equipping";

    public static bool IsComplete(string id)
    {
        if (!complete.ContainsKey(id))
        {
            return false;
        }

        return complete[id];
    }

    public static void SetComplete(string id, bool isComplete)
    {
        complete[id] = isComplete;
        OnHintChanged?.Invoke(id, isComplete);
    }

    public delegate void Update(string id, bool complete);

    public static event Update OnHintChanged;

    public static void SetShowing(string id, bool showing)
    {
        currentlyShowing[id] = showing;
    }

    public static bool GetShowing(string id)
    {
        if (!currentlyShowing.ContainsKey(id))
        {
            return false;
        }
        return currentlyShowing[id];
    }
}