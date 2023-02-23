using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GameData
{
    // A slightly lazy set of public definitions, but good enough for now.
    // dd -> Dragon's den
    // hv -> Heroes and villains
    public int hv_quest_complete;
    public int hv_npcs_alive;
    public int dd_equiped_index; // Can be 0-16
    public int dragons_den_won;
    public int color_code; // Can be 0-6?
    public int bow;
    public int axe;
    public int dagger;
    public int staff;
    public int spear;
    public int claws;
    public int shield;
    public int orb;
    public int gun;
    public int sword;
    public int mushroom;
    public int pandoras_box; // Can be 0-10
    public int armor;
    public int UFO;

    public string ToJSON()
    {
        return JsonUtility.ToJson(this);
    }

    public void FromJSON(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }

    public string ToString() {
        // append all of the fields to a string
        string s = "";
        s += "hv_quest_complete: " + hv_quest_complete + "\n";
        s += "hv_npcs_alive: " + hv_npcs_alive + "\n";
        s += "dd_equiped_index: " + dd_equiped_index + "\n";
        s += "dragons_den_won: " + dragons_den_won + "\n";
        s += "color_code: " + color_code + "\n";
        s += "bow: " + bow + "\n";
        s += "axe: " + axe + "\n";
        s += "dagger: " + dagger + "\n";
        s += "staff: " + staff + "\n";
        s += "spear: " + spear + "\n";
        s += "claws: " + claws + "\n";
        s += "shield: " + shield + "\n";
        s += "orb: " + orb + "\n";
        s += "gun: " + gun + "\n";
        s += "sword: " + sword + "\n";
        s += "mushroom: " + mushroom + "\n";
        s += "pandoras_box: " + pandoras_box + "\n";
        s += "armor: " + armor + "\n";
        s += "UFO: " + UFO + "\n";
        return s;
    }
}
