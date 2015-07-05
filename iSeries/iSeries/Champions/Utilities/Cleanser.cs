// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cleanser.cs" company="LeagueSharp">
//   Copyright (C) 2015 LeagueSharp
//   
//             This program is free software: you can redistribute it and/or modify
//             it under the terms of the GNU General Public License as published by
//             the Free Software Foundation, either version 3 of the License, or
//             (at your option) any later version.
//   
//             This program is distributed in the hope that it will be useful,
//             but WITHOUT ANY WARRANTY; without even the implied warranty of
//             MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//             GNU General Public License for more details.
//   
//             You should have received a copy of the GNU General Public License
//             along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   The cleansing class
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iSeries.Champions.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     The cleansing class
    /// </summary>
    internal class Cleanser
    {
        #region Static Fields

        /// <summary>
        ///     All the types of buffs
        /// </summary>
        private static readonly BuffType[] Buffs =
            {
                BuffType.Blind, BuffType.Charm, BuffType.CombatDehancer, 
                BuffType.Fear, BuffType.Flee, BuffType.Knockback, BuffType.Knockup, 
                BuffType.Polymorph, BuffType.Silence, BuffType.Sleep, 
                BuffType.Snare, BuffType.Stun, BuffType.Suppression, BuffType.Taunt
            };

        /// <summary>
        ///     The cleanse spells
        /// </summary>
        private static readonly List<CleanseableSpell> CleanseableSpells = new List<CleanseableSpell>
                                                                               {
                                                                                   new CleanseableSpell
                                                                                       {
                                                                                           ChampName = "Warwick", 
                                                                                           IsEnabled = true, 
                                                                                           SpellBuff = "InfiniteDuress", 
                                                                                           SpellName = "Warwick R", 
                                                                                           RealName = "warwickR", 
                                                                                           OnlyKill = false, 
                                                                                           Slot = SpellSlot.R, 
                                                                                           Delay = 100f
                                                                                       }, 
                                                                                   new CleanseableSpell
                                                                                       {
                                                                                           ChampName = "Zed", 
                                                                                           IsEnabled = true, 
                                                                                           SpellBuff = "zedulttargetmark", 
                                                                                           SpellName = "Zed R", 
                                                                                           RealName = "zedultimate", 
                                                                                           OnlyKill = true, 
                                                                                           Slot = SpellSlot.R, 
                                                                                           Delay = 800f
                                                                                       }, 
                                                                                   new CleanseableSpell
                                                                                       {
                                                                                           ChampName = "Rammus", 
                                                                                           IsEnabled = true, 
                                                                                           SpellBuff = "PuncturingTaunt", 
                                                                                           SpellName = "Rammus E", 
                                                                                           RealName = "rammusE", 
                                                                                           OnlyKill = false, 
                                                                                           Slot = SpellSlot.E, 
                                                                                           Delay = 100f
                                                                                       }, 
                                                                                   /** Danger Level 4 Spells*/
                                                                                   new CleanseableSpell
                                                                                       {
                                                                                           ChampName = "Skarner", 
                                                                                           IsEnabled = true, 
                                                                                           SpellBuff = "SkarnerImpale", 
                                                                                           SpellName = "Skaner R", 
                                                                                           RealName = "skarnerR", 
                                                                                           OnlyKill = false, 
                                                                                           Slot = SpellSlot.R, 
                                                                                           Delay = 100f
                                                                                       }, 
                                                                                   new CleanseableSpell
                                                                                       {
                                                                                           ChampName = "Fizz", 
                                                                                           IsEnabled = true, 
                                                                                           SpellBuff = "FizzMarinerDoom", 
                                                                                           SpellName = "Fizz R", 
                                                                                           RealName = "FizzR", 
                                                                                           OnlyKill = false, 
                                                                                           Slot = SpellSlot.R, 
                                                                                           Delay = 100f
                                                                                       }, 
                                                                                   new CleanseableSpell
                                                                                       {
                                                                                           ChampName = "Galio", 
                                                                                           IsEnabled = true, 
                                                                                           SpellBuff = "GalioIdolOfDurand", 
                                                                                           SpellName = "Galio R", 
                                                                                           RealName = "GalioR", 
                                                                                           OnlyKill = false, 
                                                                                           Slot = SpellSlot.R, 
                                                                                           Delay = 100f
                                                                                       }, 
                                                                                   new CleanseableSpell
                                                                                       {
                                                                                           ChampName = "Malzahar", 
                                                                                           IsEnabled = true, 
                                                                                           SpellBuff =
                                                                                               "AlZaharNetherGrasp", 
                                                                                           SpellName = "Malz R", 
                                                                                           RealName = "MalzaharR", 
                                                                                           OnlyKill = false, 
                                                                                           Slot = SpellSlot.R, 
                                                                                           Delay = 200f
                                                                                       }, 
                                                                                   /** Danger Level 3 Spells*/
                                                                                   new CleanseableSpell
                                                                                       {
                                                                                           ChampName = "Zilean", 
                                                                                           IsEnabled = false, 
                                                                                           SpellBuff = "timebombenemybuff", 
                                                                                           SpellName = "Zilean Q", 
                                                                                           OnlyKill = true, 
                                                                                           Slot = SpellSlot.Q, 
                                                                                           Delay = 700f
                                                                                       }, 
                                                                                   new CleanseableSpell
                                                                                       {
                                                                                           ChampName = "Vladimir", 
                                                                                           IsEnabled = false, 
                                                                                           SpellBuff =
                                                                                               "VladimirHemoplague", 
                                                                                           SpellName = "Vlad R", 
                                                                                           RealName = "VladimirR", 
                                                                                           OnlyKill = true, 
                                                                                           Slot = SpellSlot.R, 
                                                                                           Delay = 700f
                                                                                       }, 
                                                                                   new CleanseableSpell
                                                                                       {
                                                                                           ChampName = "Mordekaiser", 
                                                                                           IsEnabled = true, 
                                                                                           SpellBuff =
                                                                                               "MordekaiserChildrenOfTheGrave", 
                                                                                           SpellName = "Morde R", 
                                                                                           OnlyKill = true, 
                                                                                           Slot = SpellSlot.R, 
                                                                                           Delay = 800f
                                                                                       }, 
                                                                                   /** Danger Level 2 Spells*/
                                                                                   new CleanseableSpell
                                                                                       {
                                                                                           ChampName = "Poppy", 
                                                                                           IsEnabled = true, 
                                                                                           SpellBuff =
                                                                                               "PoppyDiplomaticImmunity", 
                                                                                           SpellName = "Poppy R", 
                                                                                           RealName = "PoppyR", 
                                                                                           OnlyKill = false, 
                                                                                           Slot = SpellSlot.R, 
                                                                                           Delay = 100f
                                                                                       }
                                                                               };

        /// <summary>
        ///     The last time it got checked
        /// </summary>
        private static float lastCheckTick;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Casts the cleanse item
        /// </summary>
        /// <param name="target">
        ///     The target
        /// </param>
        public static void CastCleanseItem(Obj_AI_Hero target)
        {
            if (target == null)
            {
                return;
            }

            if (IsEnabled("iseries.cleanser.items.qss") && Items.HasItem(3140) && Items.CanUseItem(3140) && target.IsMe)
            {
                Items.UseItem(3140, ObjectManager.Player);
                return;
            }

            if (IsEnabled("iseries.cleanser.items.scimitar") && Items.HasItem(3139) && Items.CanUseItem(3139)
                && target.IsMe)
            {
                Items.UseItem(3139, ObjectManager.Player);
                return;
            }

            if (IsEnabled("iseries.cleanser.items.dervish") && Items.HasItem(3137) && Items.CanUseItem(3137)
                && target.IsMe)
            {
                Items.UseItem(3137, ObjectManager.Player);
            }
        }

        /// <summary>
        ///     Checks if a menu item is ready
        /// </summary>
        /// <param name="name">
        ///     The menu item name
        /// </param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public static bool IsEnabled(string name)
        {
            return Variables.Menu.Item(name).GetValue<bool>();
        }

        /// <summary>
        ///     The on load
        /// </summary>
        /// <param name="menu">
        ///     The menu.
        /// </param>
        public static void OnLoad(Menu menu)
        {
            var spellSubmenu = new Menu("Cleanser", "Cleanser");

            var spellCleanserMenu = new Menu("Spell Cleanser", "iseries.cleanser.spell");
            foreach (var spell in CleanseableSpells.Where(h => GetChampByName(h.ChampName) != null))
            {
                var sMenu = new Menu(spell.SpellName, ObjectManager.Player.ChampionName + spell.SpellBuff);
                sMenu.AddItem(
                    new MenuItem("iseries.cleanser.spell." + spell.SpellBuff + "A", "Always").SetValue(!spell.OnlyKill));
                sMenu.AddItem(
                    new MenuItem("iseries.cleanser.spell." + spell.SpellBuff + "K", "Only if killed by it").SetValue(
                        spell.OnlyKill));
                sMenu.AddItem(
                    new MenuItem("iseries.cleanser.spell." + spell.SpellBuff + "D", "Delay before cleanse").SetValue(
                        new Slider((int)spell.Delay, 0, 10000)));
                spellCleanserMenu.AddSubMenu(sMenu);
            }

            // Bufftype cleanser menu
            var buffCleanserMenu = new Menu(
                "Bufftype Cleanser", 
                ObjectManager.Player.ChampionName + "iseries.cleanser.bufftype");

            foreach (var buffType in Buffs)
            {
                buffCleanserMenu.AddItem(
                    new MenuItem(ObjectManager.Player.ChampionName + buffType, buffType.ToString()).SetValue(true));
            }

            buffCleanserMenu.AddItem(
                new MenuItem("iseries.cleanser.bufftype.minbuffs", "Min Buffs").SetValue(new Slider(2, 1, 5)));

            spellSubmenu.AddItem(new MenuItem("iseries.cleanser.items.qss", "Use QSS").SetValue(true));
            spellSubmenu.AddItem(new MenuItem("iseries.cleanser.items.scimitar", "Use Mercurial Scimitar").SetValue(true));
            spellSubmenu.AddItem(new MenuItem("iseries.cleanser.items.dervish", "Use Dervish Blade").SetValue(true));

            spellSubmenu.AddSubMenu(spellCleanserMenu);
            spellSubmenu.AddSubMenu(buffCleanserMenu);
            Variables.Menu.AddSubMenu(spellSubmenu);

            Game.OnUpdate += OnUpdate;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Buff Cleansing
        /// </summary>
        private static void BuffTypeCleansing()
        {
            if (!OneReady())
            {
                return;
            }

            var buffCount = Buffs.Count(buff => ObjectManager.Player.HasBuffOfType(buff) && BuffTypeEnabled(buff));
            if (buffCount >= Variables.Menu.Item("iseries.cleanser.bufftype.minbuffs").GetValue<Slider>().Value)
            {
                CastCleanseItem(ObjectManager.Player);
            }
        }

        /// <summary>
        ///     Checks if a buff type is enabled
        /// </summary>
        /// <param name="buffType">
        ///     The buff type
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        private static bool BuffTypeEnabled(BuffType buffType)
        {
            return IsEnabled(ObjectManager.Player.ChampionName + buffType);
        }

        /// <summary>
        ///     Gets a champion name as an <see cref="Obj_AI_Hero" />
        /// </summary>
        /// <param name="enemyName">
        ///     The enemy name.
        /// </param>
        /// <returns>
        ///     The <see cref="Obj_AI_Hero" />.
        /// </returns>
        private static Obj_AI_Hero GetChampByName(string enemyName)
        {
            return ObjectManager.Get<Obj_AI_Hero>().Find(h => h.IsEnemy && h.ChampionName == enemyName);
        }

        /// <summary>
        ///     Kill Cleansing
        /// </summary>
        private static void KillCleansing()
        {
            if (!OneReady())
            {
                return;
            }

            CleanseableSpell mySpell = null;
            if (
                CleanseableSpells.Where(
                    spell =>
                    ObjectManager.Player.HasBuff(spell.SpellBuff) && SpellEnabledOnKill(spell.SpellBuff)
                    && GetChampByName(spell.ChampName).GetSpellDamage(ObjectManager.Player, spell.Slot)
                    > ObjectManager.Player.Health + 20)
                    .OrderBy(
                        spell => GetChampByName(spell.ChampName).GetSpellDamage(ObjectManager.Player, spell.Slot))
                    .Any())
            {
                mySpell =
                    CleanseableSpells.Where(
                        spell =>
                        ObjectManager.Player.HasBuff(spell.SpellBuff) && SpellEnabledOnKill(spell.SpellBuff))
                        .OrderBy(
                            spell =>
                            GetChampByName(spell.ChampName).GetSpellDamage(ObjectManager.Player, spell.Slot))
                        .First();
            }

            if (mySpell != null)
            {
                UseCleanser(mySpell, ObjectManager.Player);
            }
        }

        /// <summary>
        ///     Checks if a cleanse item is ready
        /// </summary>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        private static bool OneReady()
        {
            return (IsEnabled("iseries.cleanser.items.qss") && Items.HasItem(3140) && Items.CanUseItem(3140))
                   || (IsEnabled("iseries.cleanser.items.scimitar") && Items.HasItem(3139) && Items.CanUseItem(3139))
                   || (IsEnabled("iseries.cleanser.items.dervish") && Items.HasItem(3137));
        }

        /// <summary>
        ///     The on update function
        /// </summary>
        /// <param name="args">
        ///     The Arguments
        /// </param>
        private static void OnUpdate(EventArgs args)
        {
            if (Environment.TickCount - lastCheckTick < 100)
            {
                return;
            }

            lastCheckTick = Environment.TickCount;

            KillCleansing();
            SpellCleansing();
            BuffTypeCleansing();
        }

        /// <summary>
        ///     Spell Cleansing
        /// </summary>
        private static void SpellCleansing()
        {
            if (!OneReady())
            {
                return;
            }

            CleanseableSpell mySpell = null;
            if (
                CleanseableSpells.Where(
                    spell => ObjectManager.Player.HasBuff(spell.SpellBuff) && SpellEnabledAlways(spell.SpellBuff))
                    .OrderBy(spell => GetChampByName(spell.ChampName).GetSpellDamage(ObjectManager.Player, spell.Slot))
                    .Any())
            {
                mySpell =
                    CleanseableSpells.Where(
                        spell => ObjectManager.Player.HasBuff(spell.SpellBuff) && SpellEnabledAlways(spell.SpellBuff))
                        .OrderBy(
                            spell => GetChampByName(spell.ChampName).GetSpellDamage(ObjectManager.Player, spell.Slot))
                        .First();
            }

            if (mySpell != null)
            {
                UseCleanser(mySpell, ObjectManager.Player);
            }
        }

        /// <summary>
        ///     The Spell Delay
        /// </summary>
        /// <param name="sName">
        ///     The spell name
        /// </param>
        /// <returns>
        ///     <see cref="int" />
        /// </returns>
        private static int SpellDelay(string sName)
        {
            return Variables.Menu.Item("iseries.cleanser.spell." + sName + "D").GetValue<Slider>().Value;
        }

        /// <summary>
        ///     Checks if a spell is enabled
        /// </summary>
        /// <param name="sName">
        ///     the spell name
        /// </param>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        private static bool SpellEnabledAlways(string sName)
        {
            return IsEnabled("iseries.cleanser.spell." + sName + "A");
        }

        /// <summary>
        ///     Checks if a spell is enabled on kill
        /// </summary>
        /// <param name="sName">
        ///     the spell name
        /// </param>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        private static bool SpellEnabledOnKill(string sName)
        {
            return IsEnabled("iseries.cleanser.spell." + sName + "K");
        }

        /// <summary>
        ///     Use Cleanser
        /// </summary>
        /// <param name="spell">
        ///     The Spell
        /// </param>
        /// <param name="target">
        ///     The target
        /// </param>
        private static void UseCleanser(CleanseableSpell spell, Obj_AI_Hero target)
        {
            Utility.DelayAction.Add(SpellDelay(spell.RealName), () => CastCleanseItem(target));
        }

        #endregion
    }

    /// <summary>
    ///     The cleanse spell
    /// </summary>
    internal class CleanseableSpell
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the champ name.
        /// </summary>
        public string ChampName { get; set; }

        /// <summary>
        ///     Gets or sets the delay.
        /// </summary>
        public float Delay { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether only kill.
        /// </summary>
        public bool OnlyKill { get; set; }

        /// <summary>
        ///     Gets or sets the real name.
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        ///     Gets or sets the slot.
        /// </summary>
        public SpellSlot Slot { get; set; }

        /// <summary>
        ///     Gets or sets the spell buff.
        /// </summary>
        public string SpellBuff { get; set; }

        /// <summary>
        ///     Gets or sets the spell name.
        /// </summary>
        public string SpellName { get; set; }

        #endregion
    }
}