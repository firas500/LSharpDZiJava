﻿// --------------------------------------------------------------------------------------------------------------------
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
            // Marksman
            SupportedChampions.Add("Kalista", new Kalista().Invoke);
            SupportedChampions.Add("Lucian", new Lucian().Invoke);
            SupportedChampions.Add("Draven", new Draven().Invoke);
            SupportedChampions.Add("Ezreal", new Ezreal().Invoke);
            SupportedChampions.Add("Graves", new Graves().Invoke);
            SupportedChampions.Add("Twitch", new Twitch().Invoke);
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
                        if (!SupportedChampions.ContainsKey(Variables.Player.ChampionName))
                        {
                            return;
                        }

                        Variables.Menu = new Menu("iSeries: " + Variables.Player.ChampionName, "com.iseries", true);
                        SupportedChampions[Variables.Player.ChampionName]();
                    };
            }
        }

        #endregion
    }
}