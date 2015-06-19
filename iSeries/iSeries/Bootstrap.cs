// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Bootstrap.cs" company="LeagueSharp">
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
//   The bootstrap.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iSeries
{
    #region

    using System;
    using System.Collections.Generic;

    using iSeries.Champions.Marksman.Draven;
    using iSeries.Champions.Marksman.Ezreal;
    using iSeries.Champions.Marksman.Graves;
    using iSeries.Champions.Marksman.Kalista;
    using iSeries.Champions.Marksman.Lucian;
    using iSeries.Champions.Marksman.Sivir;
    using iSeries.Champions.Marksman.Tristana;
    using iSeries.Champions.Marksman.Twitch;
    using iSeries.Champions.Marksman.Vayne;

    using LeagueSharp;
    using LeagueSharp.Common;

    #endregion

    /// <summary>
    ///     The bootstrap.
    /// </summary>
    internal class Bootstrap
    {
        #region Static Fields

        /// <summary>
        ///     TODO The champ list.
        /// </summary>
        private static readonly Dictionary<string, Action> ChampList = new Dictionary<string, Action>
                                                                           {
                                                                               { "Kalista", () => new Kalista().Invoke() }, 
                                                                               { "Ezreal", () => new Ezreal().Invoke() }, 
                                                                               { "Lucian", () => new Lucian().Invoke() }, 
                                                                               { "Graves", () => new Graves().Invoke() }, 
                                                                               { "Draven", () => new Draven().Invoke() }, 
                                                                               { "Twitch", () => new Twitch().Invoke() }, 
                                                                               { "Sivir", () => new Sivir().Invoke() }, 
                                                                               { "Vayne", () => new Vayne().Invoke() }, 
                                                                               { "Tristana", () => new Tristana().Invoke() },
                                                                           };

        /// <summary>
        ///     TODO The orb walking.
        /// </summary>
        private static Menu orbwalking;

        #endregion

        #region Methods

        /// <summary>
        ///     TODO The check auto wind up.
        ///     This Broscience code iJava please ._.  -Asuna
        /// </summary>
        private static void CheckAutoWindUp()
        {
            var additional = 0;

            if (Game.Ping >= 100)
            {
                additional = Game.Ping / 100 * 10;
            }
            else if (Game.Ping > 40 && Game.Ping < 100)
            {
                additional = Game.Ping / 100 * 20;
            }
            else if (Game.Ping <= 40)
            {
                additional = +20;
            }

            var windUp = Game.Ping + additional;
            if (windUp < 40)
            {
                windUp = 40;
            }

            orbwalking.Item("ExtraWindup").SetValue(windUp < 200 ? new Slider(windUp, 200, 0) : new Slider(200, 200, 0));
        }

        /// <summary>
        ///     Generates the base menu
        /// </summary>
        private static void GenerateBaseMenu()
        {
            Variables.Menu = new Menu(
                "iSeries: " + ObjectManager.Player.ChampionName, 
                "iseries." + ObjectManager.Player.ChampionName, 
                true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Variables.Menu.AddSubMenu(targetSelectorMenu);

            orbwalking = Variables.Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Variables.Orbwalker = new Orbwalking.Orbwalker(orbwalking);

            orbwalking.AddItem(new MenuItem("AutoWindup", "iSeries - Auto Windup").SetValue(false)).ValueChanged +=
                (sender, argsEvent) =>
                    {
                        if (argsEvent.GetNewValue<bool>())
                        {
                            CheckAutoWindUp();
                        }
                    };
            Variables.Menu.AddItem(new MenuItem("com.iseries.autobuy", "AutoBuy (Scrying Orb, etc)").SetValue(true));

            // TODO add an item manager / auto qss etc / some utils maybe?
            // Activator# Bik
        }

        /// <summary>
        ///     The main method.
        /// </summary>
        /// <param name="args">
        ///     The passed arguments
        /// </param>
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        /// <summary>
        ///     TODO The on load.
        /// </summary>
        /// <param name="args">
        ///     TODO The args.
        /// </param>
        private static void OnLoad(EventArgs args)
        {
            if (ChampList.ContainsKey(ObjectManager.Player.ChampionName))
            {
                GenerateBaseMenu();
                ChampList[ObjectManager.Player.ChampionName]();
                Console.WriteLine("iSeries ADC - By Asuna and Corey");
                Console.WriteLine("Loaded: " + ObjectManager.Player.ChampionName);
            }
        }

        #endregion
    }
}