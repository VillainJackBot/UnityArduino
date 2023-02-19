using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TokenData
{
    // A slightly lazy set of public definitions, but good enough for now.
    // dd -> Dragon's den
    // hv -> ?
    public bool     hv_npcs_alive = true;
    public bool     hv_quest_complete = false;
    public bool     dd_won = false;
    public int      pandoras_box = 5;
    public int      dd_equiped_index = 0;
    public string   color_code = "red";
    public string[] dd_items = new string[2]{"sword", "armor"};

    public string SerializeIntoJSON()
    {
        return JsonUtility.ToJson(this);
    }

    public void DeserializeFromJSON(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }
}
