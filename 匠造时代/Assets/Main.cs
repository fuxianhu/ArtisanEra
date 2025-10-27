using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class Main : MonoBehaviour
{
    void Start()
    {
        FunctionManager.instance.ShowUIOnly();
        SaveManager.instance.SaveGame(new PlayerData { level = 114514, playerName = "woc_114514_nmd" });
    }

    void Update()
    {
        
    }
}