using UnityEngine;
using System.Collections;

public class LevelController {
    //WORLD
    public static int GetCurrentLevel(int world)
    {
        return CPlayerPrefs.GetInt("current_level_world_" + world, 1);
    }

    public static void SetCurrentLevel(int world, int level)
    {
        CPlayerPrefs.SetInt("current_level_world_" + world, level);
    }

    public static void SetCurrentWorld(int world)
    {
        CPlayerPrefs.SetInt("current_world", world);
    }

    public static int GetCurrentWorld()
    {
        return CPlayerPrefs.GetInt("current_world", 1);
    }

    public static int GetUnlockWorld()
    {
        return CPlayerPrefs.GetInt("unlocked_world", 1);
    }

    public static void SetUnlockWorld(int world) //start with 1
    {
        CPlayerPrefs.SetInt("unlocked_world", world);
    }

    public static int GetUnlockLevel(int world)
    {
        return CPlayerPrefs.GetInt("unlocked_level_world_" + world, 1);
    }

    public static void SetUnlockLevel(int world, int level) //start with 1
    {
        int unlockLvl = GetUnlockLevel(world);
        CPlayerPrefs.SetInt("unlocked_level_world_" + world, Mathf.Max(unlockLvl, level));
    }

    public static int GetNumStar(int world, int level)
    {
        return PlayerPrefs.GetInt("num_star_world_" + world + "level_" + level, 0);
    }

    public static void SetNumStar(int world, int level, int value)
    {
        int currStar = GetNumStar(world, level);
        PlayerPrefs.SetInt("num_star_world_" + world + "level_" + level, Mathf.Max(currStar, value));
    }
    /////////////////////////////////////////
    public static int GetMovedLevel()
    {
        return PlayerPrefs.GetInt("moved_level", 1);
    }

    public static void SetMovedLevel(int value)
    {
        PlayerPrefs.SetInt("moved_level", value);
    }

    public static int GetNumStar(int level)
    {
        return PlayerPrefs.GetInt("num_star_level_" + level, 0);
    }

    public static void SetNumStar(int level, int value)
    {
        int currStar = GetNumStar(level);
        PlayerPrefs.SetInt("num_star_level_" + level, Mathf.Max(currStar, value));
    }
}
