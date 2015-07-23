// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MenuGenerator.cs" company="LeagueSharp">
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
//   TODO The menu generator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace iSeries.Champions.Marksman.Tristana
{
    using LeagueSharp.Common;

    /// <summary>
    /// TODO The menu generator.
    /// </summary>
    internal class MenuGenerator
    {
        #region Public Methods and Operators

        /// <summary>
        ///     The generation function.
        /// </summary>
        /// <param name="root">
        ///     The root menu
        /// </param>
        public static void Generate(Menu root)
        {
            var comboMenu = new Menu("Combo Options", "com.iseries.tristana.combo");
            {
                comboMenu.AddItem(new MenuItem("com.iseries.tristana.combo.useQ", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.tristana.combo.useE", "Use E").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.tristana.combo.useR", "Use R").SetValue(true));
                root.AddSubMenu(comboMenu);
            }

            var harassMenu = new Menu("Harass Options", "com.tristana.vayne.harass");
            {
                harassMenu.AddItem(new MenuItem("com.iseries.tristana.harass.useE", "Use E").SetValue(false));
                root.AddSubMenu(harassMenu);
            }

            var laneclearMenu = new Menu("Laneclear Options", "com.iseries.tristana.laneclear");
            {
                laneclearMenu.AddItem(new MenuItem("soontm", "SOON™").SetValue(true));
                root.AddSubMenu(laneclearMenu);
            }

            var misc = new Menu("Misc Options", "com.iseries.tristana.misc");
            {
                misc.AddItem(new MenuItem("com.iseries.tristana.misc.gapcloser", "Anti Gapcloser").SetValue(true));
                misc.AddItem(new MenuItem("com.iseries.tristana.misc.interrupter", "Spell Interrupter").SetValue(true));
                misc.AddItem(new MenuItem("com.iseries.tristana.misc.useRE", "Try R-E Finisher").SetValue(false));
                misc.AddItem(new MenuItem("com.iseries.tristana.misc.kickFlash", "Ult away flashes (BETA)").SetValue(false));
                root.AddSubMenu(misc);
            }

            var drawing = new Menu("Drawing Options", "com.iseries.tristana.drawing");
            {
                drawing.AddItem(new MenuItem("com.iseries.tristana.drawing.drawW", "Draw W Range").SetValue(false));
                drawing.AddItem(new MenuItem("com.iseries.tristana.drawing.drawE", "Draw E Range").SetValue(true));
                root.AddSubMenu(drawing);
            }

            /*var noEOnMenu = new Menu("Don't E Options", "com.iseries.vayne.noe.");
            {
                foreach (var champ in HeroManager.Enemies)
                {
                    noEOnMenu.AddItem(
                        new MenuItem(
                            "com.iseries.twitch.noe." + champ.ChampionName.ToLowerInvariant(), 
                            champ.ChampionName).SetValue(false));
                }

                root.AddSubMenu(noEOnMenu);
            }*/

            root.AddToMainMenu();
        }

        #endregion
    }
}