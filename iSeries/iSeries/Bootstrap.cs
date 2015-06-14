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
    using System;
    using System.Collections.Generic;

    using iSeries.Champions.Draven;
    using iSeries.Champions.Ezreal;
    using iSeries.Champions.Graves;
    using iSeries.Champions.Kalista;
    using iSeries.Champions.Lucian;
    using iSeries.Champions.Twitch;

    using LeagueSharp;
    using LeagueSharp.Common;

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
                                                                               { "Twitch", () => new Twitch().Invoke() }
                                                                           };

        #endregion

        #region Methods

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
                Variables.Menu = new Menu("iSeries: " + ObjectManager.Player.ChampionName, "com.iseries", true);

                var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                TargetSelector.AddToMenu(targetSelectorMenu);
                Variables.Menu.AddSubMenu(targetSelectorMenu);
                Variables.Orbwalker =
                    new Orbwalking.Orbwalker(Variables.Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking")));

                ChampList[ObjectManager.Player.ChampionName]();

                Console.WriteLine("Loaded: " + ObjectManager.Player.ChampionName);
            }
        }

        #endregion
    }
}