using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Mathematica;
using TGC.Group.Utils;

namespace TGC.Group.Model
{
    internal static class GameCraftingManager
    {
        private struct Constants
        {
            public static int WEAPON_COUNT_ORE_SILVER = 5;
            public static int WEAPON_COUNT_CORAL_NORMAL = 3;
            public static int WEAPON_COUNT_CORAL_TREE = 2;
            public static int WEAPON_COUNT_FISH_NORMAL = 4;
            public static int WEAPON_COUNT_FISH_YELLOW = 6;

            public static int DIVING_HELMET_COUNT_ORE_GOLD = 2;
            public static int DIVING_HELMET_COUNT_ORE_IRON = 3;
            public static int DIVING_HELMET_COUNT_CORAL_SPIRAL = 1;
            public static int DIVING_HELMET_COUNT_CORAL_TREE = 2;
            public static int DIVING_HELMET_COUNT_FISH_NORMAL = 4;
            public static int DIVING_HELMET_COUNT_FISH_YELLOW = 1;

            public static int CATCH_FISH_COUNT_ORE_IRON = 1;
            public static int CATCH_FISH_COUNT_ORE_SILVER = 2;
            public static int CATCH_FISH_COUNT_CORAL_NORMAL = 3;
            public static int CATCH_FISH_COUNT_CORAL_TREE = 1;
        }

        public static Dictionary<string, List<string>> Items { get; set; }
        private static Dictionary<string, DrawText> CountItemsText;
        public static bool HasWeapon { get; set; }
        public static bool HasDivingHelmet { get; set; }
        public static bool CanFish { get; set; }

        public static Dictionary<string, DrawText> GetTextCraftingItems()
        {
            CountItemsText = new Dictionary<string, DrawText>();
            var weapon = new DrawText();
            var catchFish = new DrawText();
            var divingHelmet = new DrawText();

            catchFish.Text = "\nItems: " +
                         "\n IRON: " + Items["IRON"].Count + " / " + Constants.CATCH_FISH_COUNT_ORE_IRON +
                         "\n SILVER: " + Items["SILVER"].Count + " / " + Constants.CATCH_FISH_COUNT_ORE_SILVER +
                         "\n NORMAL CORAL: " + Items["NORMALCORAL"].Count + " / " + Constants.CATCH_FISH_COUNT_CORAL_NORMAL +
                         "\n TREE FISH: " + Items["TREECORAL"].Count + " / " + Constants.CATCH_FISH_COUNT_CORAL_TREE;

            weapon.Text = "\nItems: " +
                          "\n SILVER: " + Items["SILVER"].Count + " / " + Constants.WEAPON_COUNT_ORE_SILVER +
                          "\n NORMAL CORAL: " + Items["NORMALCORAL"].Count + " / " + Constants.WEAPON_COUNT_CORAL_NORMAL +
                          "\n TREE CORAL: " + Items["TREECORAL"].Count + " / " + Constants.WEAPON_COUNT_CORAL_TREE +
                          "\n NORMAL FISH: " + Items["NORMALFISH"].Count + " / " + Constants.WEAPON_COUNT_FISH_NORMAL +
                          "\n YELLOW FISH: " + Items["YELLOWFISH"].Count + " / " + Constants.WEAPON_COUNT_FISH_YELLOW;

            divingHelmet.Text = "\nItems: " +
                         "\n GOLD: " + Items["GOLD"].Count + " / " + Constants.DIVING_HELMET_COUNT_ORE_GOLD +
                         "\n IRON: " + Items["IRON"].Count + " / " + Constants.DIVING_HELMET_COUNT_ORE_IRON +
                         "\n SPIRAL CORAL: " + Items["SPIRALCORAL"].Count + " / " + Constants.DIVING_HELMET_COUNT_CORAL_SPIRAL +
                         "\n TREE CORAL: " + Items["TREECORAL"].Count + " / " + Constants.DIVING_HELMET_COUNT_CORAL_TREE +
                         "\n NORMAL FISH: " + Items["NORMALFISH"].Count + " / " + Constants.DIVING_HELMET_COUNT_FISH_NORMAL +
                         "\n YELLOW FISH: " + Items["YELLOWFISH"].Count + " / " + Constants.DIVING_HELMET_COUNT_FISH_YELLOW;

            catchFish.Size = new TGCVector2(300, 200);
            weapon.Size = new TGCVector2(300, 200);
            divingHelmet.Size = new TGCVector2(300, 200);

            catchFish.Font = new Font("Arial Black", 10, FontStyle.Bold);
            weapon.Font = new Font("Arial Black", 10, FontStyle.Bold);
            divingHelmet.Font = new Font("Arial Black", 10, FontStyle.Bold);

            CountItemsText.Add("CATCHFISH", catchFish);
            CountItemsText.Add("WEAPON", weapon);
            CountItemsText.Add("OXYGEN", divingHelmet);

            return CountItemsText;
        }

        public static bool CanCraftWeapon(Dictionary<string, List<string>> items)
        {
            Items = items;
            if (HasWeapon)
            {
                return false;
            }

            if (items["SILVER"].Count >= Constants.WEAPON_COUNT_ORE_SILVER &&
                 items["NORMALCORAL"].Count >= Constants.WEAPON_COUNT_CORAL_NORMAL &&
                 items["TREECORAL"].Count >= Constants.WEAPON_COUNT_CORAL_TREE &&
                 items["NORMALFISH"].Count >= Constants.WEAPON_COUNT_FISH_NORMAL &&
                 items["YELLOWFISH"].Count >= Constants.WEAPON_COUNT_FISH_YELLOW)
            {
                items["SILVER"].RemoveRange(0, Constants.WEAPON_COUNT_ORE_SILVER);
                items["NORMALCORAL"].RemoveRange(0, Constants.WEAPON_COUNT_CORAL_NORMAL);
                items["TREECORAL"].RemoveRange(0, Constants.WEAPON_COUNT_CORAL_TREE);
                items["NORMALFISH"].RemoveRange(0, Constants.WEAPON_COUNT_FISH_NORMAL);
                items["YELLOWFISH"].RemoveRange(0, Constants.WEAPON_COUNT_FISH_YELLOW);
                HasWeapon = true;
            }

            return HasWeapon;
        }

        public static bool CanCraftDivingHelmet(Dictionary<string, List<string>> items)
        {
            Items = items;

            if (items["GOLD"].Count >= Constants.DIVING_HELMET_COUNT_ORE_GOLD &&
                 items["IRON"].Count >= Constants.DIVING_HELMET_COUNT_ORE_IRON &&
                 items["SPIRALCORAL"].Count >= Constants.DIVING_HELMET_COUNT_CORAL_SPIRAL &&
                 items["TREECORAL"].Count >= Constants.DIVING_HELMET_COUNT_CORAL_TREE &&
                 items["NORMALFISH"].Count >= Constants.DIVING_HELMET_COUNT_FISH_NORMAL &&
                 items["YELLOWFISH"].Count >= Constants.DIVING_HELMET_COUNT_FISH_YELLOW)
            {
                items["GOLD"].RemoveRange(0, Constants.DIVING_HELMET_COUNT_ORE_GOLD);
                items["IRON"].RemoveRange(0, Constants.DIVING_HELMET_COUNT_ORE_IRON);
                items["SPIRALCORAL"].RemoveRange(0, Constants.DIVING_HELMET_COUNT_CORAL_SPIRAL);
                items["TREECORAL"].RemoveRange(0, Constants.DIVING_HELMET_COUNT_CORAL_TREE);
                items["NORMALFISH"].RemoveRange(0, Constants.DIVING_HELMET_COUNT_FISH_NORMAL);
                items["YELLOWFISH"].RemoveRange(0, Constants.DIVING_HELMET_COUNT_FISH_YELLOW);
                HasDivingHelmet = true;
            }
            else
            {
                HasDivingHelmet = false;
            }

            return HasDivingHelmet;
        }

        public static bool CanCatchFish(Dictionary<string, List<string>> items)
        {
            Items = items;
            if (CanFish)
            {
                return false;
            }

            if (items["IRON"].Count >= Constants.CATCH_FISH_COUNT_ORE_IRON &&
                 items["SILVER"].Count >= Constants.CATCH_FISH_COUNT_ORE_SILVER &&
                 items["NORMALCORAL"].Count >= Constants.CATCH_FISH_COUNT_CORAL_NORMAL &&
                 items["TREECORAL"].Count >= Constants.CATCH_FISH_COUNT_CORAL_TREE)
            {
                items["IRON"].RemoveRange(0, Constants.CATCH_FISH_COUNT_ORE_IRON);
                items["SILVER"].RemoveRange(0, Constants.CATCH_FISH_COUNT_ORE_SILVER);
                items["NORMALCORAL"].RemoveRange(0, Constants.CATCH_FISH_COUNT_CORAL_NORMAL);
                items["TREECORAL"].RemoveRange(0, Constants.CATCH_FISH_COUNT_CORAL_TREE);
                CanFish = true;
            }
            return CanFish;
        }
    }
}
