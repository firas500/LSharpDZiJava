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

    using iSeries.Champions.Kalista;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     The bootstrap.
    /// </summary>
    internal class Bootstrap
    {
        #region Static Fields

        /// <summary>
        ///     Supports Champions List.
        /// </summary>
        private static readonly IDictionary<string, Action> SupportedChampions = new Dictionary<string, Action>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="Bootstrap" /> class.
        /// </summary>
        static Bootstrap()
        {
            SupportedChampions.Add("Kalista", new Kalista().Invoke);
        }

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
            if (args != null)
            {
                CustomEvents.Game.OnGameLoad += eventArgs =>
                    {
                        Variables.Player = ObjectManager.Player;
                        if (!SupportedChampions.ContainsKey(Variables.Player.ChampionName))
                        {
                            return;
                        }

                        Variables.Menu = new Menu("iSeries: " + Variables.Player.ChampionName, "iSeries", true);
                        {
                            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                            TargetSelector.AddToMenu(targetSelectorMenu);
                            Variables.Menu.AddSubMenu(targetSelectorMenu);
                            Variables.Orbwalker =
                                new Orbwalking.Orbwalker(
                                    Variables.Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking")));
                        }

                        Variables.Menu.AddToMainMenu();
                        SupportedChampions[Variables.Player.ChampionName]();
                    };
            }
        }

        #endregion
    }
}