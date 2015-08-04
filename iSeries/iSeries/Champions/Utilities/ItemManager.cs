using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace iSeries.Champions.Utilities
{
    class ItemManager
    {
        private static float _lastCheckTick;
        //TODO: List of Activator Features here:

        //TODO: Shield Module
        //TODO: Summoners Spells Implementation

        private static readonly List<DzItem> ItemList = new List<DzItem>
        {
            new DzItem
            {
                Id = 3144,
                Name = "Bilgewater Cutlass",
                Range = 600f,
                Class = ItemClass.Offensive,
                Mode = ItemMode.Targeted
            },
            new DzItem
            {
                Id= 3153,
                Name = "Blade of the Ruined King",
                Range = 600f,
                Class = ItemClass.Offensive,
                Mode = ItemMode.Targeted
            },
            new DzItem
            {
                Id= 3142,
                Name = "Youmuu",
                Range = float.MaxValue,
                Class = ItemClass.Offensive,
                Mode = ItemMode.NoTarget
            }
        };


        public static void OnLoad(Menu menu)
        {
            //Create the menu here.
            var cName = ObjectManager.Player.ChampionName;
            var activatorMenu = new Menu("[iSeries] Activator", "iseries.activator");

            //Offensive Menu
            var offensiveMenu = new Menu("Activator - Offensive", "iseries.activator.offensive");
            var offensiveItems = ItemList.FindAll(item => item.Class == ItemClass.Offensive);
            foreach (var item in offensiveItems)
            {
                var itemMenu = new Menu(item.Name, cName + item.Id);
                itemMenu.AddItem(new MenuItem("iseries.activator." + item.Id + ".always", "Always").SetValue(true));
                itemMenu.AddItem(new MenuItem("iseries.activator." + item.Id + ".onmyhp", "On my HP < then %").SetValue(new Slider(30)));
                itemMenu.AddItem(new MenuItem("iseries.activator." + item.Id + ".ontghpgreater", "On Target HP > then %").SetValue(new Slider(40)));
                itemMenu.AddItem(new MenuItem("iseries.activator." + item.Id + ".ontghplesser", "On Target HP < then %").SetValue(new Slider(0)));
                itemMenu.AddItem(new MenuItem("iseries.activator." + item.Id + ".ontgkill", "On Target Killable").SetValue(true));
                offensiveMenu.AddSubMenu(itemMenu);
            }
            activatorMenu.AddSubMenu(offensiveMenu);

            //Defensive Menu
            AddHitChanceSelector(activatorMenu);

            activatorMenu.AddItem(new MenuItem("iseries.activator.activatordelay", "Global Activator Delay").SetValue(new Slider(80, 0, 300)));
            activatorMenu.AddItem(new MenuItem("iseries.activator.enabledalways", "Enabled Always?").SetValue(false));
            activatorMenu.AddItem(new MenuItem("iseries.activator.enabledcombo", "Enabled On Press?").SetValue(new KeyBind(32, KeyBindType.Press)));
            menu.AddSubMenu(activatorMenu);
            Game.OnUpdate += Game_OnGameUpdate;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (!GetItemValue<bool>("iseries.activator.enabledalways") &&
                !GetItemValue<KeyBind>("iseries.activator.enabledcombo").Active)
            {
                return;
            }
            if (Environment.TickCount - _lastCheckTick < GetItemValue<Slider>("iseries.activator.activatordelay").Value)
            {
                return;
            }
            _lastCheckTick = Environment.TickCount;
            UseOffensive();
        }

        static void UseOffensive()
        {
            var offensiveItems = ItemList.FindAll(item => item.Class == ItemClass.Offensive);
            foreach (var item in offensiveItems)
            {
                var selectedTarget = Hud.SelectedUnit as Obj_AI_Base ?? TargetSelector.GetTarget(item.Range, TargetSelector.DamageType.True);
                if (!selectedTarget.IsValidTarget(item.Range) && item.Mode != ItemMode.NoTarget)
                {
                    return;
                }
                if (GetItemValue<bool>("iseries.activator." + item.Id + ".always"))
                {
                    UseItem(selectedTarget, item);
                }
                if (ObjectManager.Player.HealthPercentage() < GetItemValue<Slider>("iseries.activator." + item.Id + ".onmyhp").Value)
                {
                    UseItem(selectedTarget, item);
                }
                if (selectedTarget.HealthPercentage() < GetItemValue<Slider>("iseries.activator." + item.Id + ".ontghplesser").Value && !GetItemValue<bool>("iseries.activator." + item.Id + ".ontgkill"))
                {
                    UseItem(selectedTarget, item);
                }
                if (selectedTarget.HealthPercentage() > GetItemValue<Slider>("iseries.activator." + item.Id + ".ontghpgreater").Value)
                {
                    UseItem(selectedTarget, item);
                }
                if (selectedTarget.Health < ObjectManager.Player.GetSpellDamage(selectedTarget, GetItemSpellSlot(item)) && GetItemValue<bool>("iseries.activator." + item.Id + ".ontgkill"))
                {
                    UseItem(selectedTarget, item);
                }
            }
        }


        static void UseItem(Obj_AI_Base target, DzItem item)
        {
            if (!Items.HasItem(item.Id) || !Items.CanUseItem(item.Id))
            {
                return;
            }
            switch (item.Mode)
            {
                case ItemMode.Targeted:
                    Items.UseItem(item.Id, target);
                    break;
                case ItemMode.NoTarget:
                    Items.UseItem(item.Id, ObjectManager.Player);
                    break;
                case ItemMode.Skillshot:
                    if (item.CustomInput == null)
                    {
                        return;
                    }
                    var customPred = Prediction.GetPrediction(item.CustomInput);
                    if (customPred.Hitchance >= GetHitchance())
                    {
                        Items.UseItem(item.Id, customPred.CastPosition);
                    }
                    break;
            }
        }

        static SpellSlot GetItemSpellSlot(DzItem item)
        {
            foreach (var it in ObjectManager.Player.InventoryItems.Where(it => (int)it.Id == item.Id))
            {
                return it.SpellSlot != SpellSlot.Unknown ? it.SpellSlot : SpellSlot.Unknown;
            }
            return SpellSlot.Unknown;
        }

        public static HitChance GetHitchance()
        {
            switch (Variables.Menu.Item("iseries.activator.customhitchance").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        public static void AddHitChanceSelector(Menu menu)
        {
            menu.AddItem(new MenuItem("iseries.activator.customhitchance", "Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 2)));
        }

        public static T GetItemValue<T>(string item)
        {
            return Variables.Menu.Item(item).GetValue<T>();
        }
    }

    internal class DzItem
    {
        public String Name { get; set; }
        public int Id { get; set; }
        public float Range { get; set; }
        public ItemClass Class { get; set; }
        public ItemMode Mode { get; set; }
        public PredictionInput CustomInput { get; set; }
    }

    enum ItemMode
    {
        Targeted, Skillshot, NoTarget
    }

    enum ItemClass
    {
        Offensive, Defensive
    }
   
}
