using SimpleJSON;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Superpow
{
    public class Utils
    {
        public static void SetDifficulty(int value)
        {
            CPlayerPrefs.SetInt("difficulty_index", value);
        }

        public static int GetDifficulty()
        {
            return CPlayerPrefs.GetInt("difficulty_index", 1);
        }

        public static void SetHighScore(int value)
        {
            CPlayerPrefs.SetInt("high_score", value);
        }

        public static int GetHighScore()
        {
            return CPlayerPrefs.GetInt("high_score", 0);
        }

        public static void SetHighLine(int value)
        {
            CPlayerPrefs.SetInt("high_line", value);
        }

        public static int GetHighLine()
        {
            return CPlayerPrefs.GetInt("high_line", 0);
        }

        public static void SetGameMode(int value)
        {
            CPlayerPrefs.SetInt("game_mode", value);
        }

        public static int GetGameMode()
        {
            return CPlayerPrefs.GetInt("game_mode", 0);
        }

        public static JSONNode LoadLevelJson(int world, int level)
        {
            string json = CUtils.ReadFileContent("World_" + world + "/level_" + level);
            if (json == null) json = CUtils.ReadFileContent("World_1/level_1");
            return JSON.Parse(json);
        }

        public static List<Vector2> GetPositions(string[] rows)
        {
            List<Vector2> positions = new List<Vector2>();
            for (int i = 0; i < rows.Length; i++)
            {
                string[] items = Regex.Split(rows[i].Trim(), @"\s+");
                int row = rows.Length - 1 - i;
                for (int c = 0; c < items.Length; c++)
                {
                    if (items[c] == "x")
                    {
                        positions.Add(new Vector2(c, row));
                    }
                }
            }
            return positions;
        }

        public static int CalculateTotalStar()
        {
            int unlockWorld = LevelController.GetUnlockWorld();
            int total = 0;
            for (int world = 1; world <= Const.NUM_WORLD; world++)
            {
                if (world > unlockWorld) break;
                int unlockLvl = LevelController.GetUnlockLevel(world);
                for (int lvl = 1; lvl <= GetNumLevels(world); lvl++)
                {
                    if (lvl > unlockLvl) break;
                    int star = LevelController.GetNumStar(world, lvl);
                    total += star;
                }
            }
            return total;
            
        }

        public static int GetNumLevels(int world)
        {
            Object[] objects = Resources.LoadAll("World_" + world);
            return objects.Length;
        }

        public static int GetTotalLevels()
        {
            int totalStar = 0;
            for (int i = 1; i <= Const.NUM_WORLD; i++)
            {
                totalStar += Superpow.Utils.GetNumLevels(i);
            }
            return totalStar;
        }
    }
}