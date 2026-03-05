using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class Main
{
    public static CustomEvents CustomEvents { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Start()
    {
        CustomEvents = new CustomEvents();
    }
}
