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

namespace iSeries.Champions.Graves
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
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            root.AddSubMenu(targetSelectorMenu);

            Variables.Orbwalker = new Orbwalking.Orbwalker(root.AddSubMenu(new Menu("Orbwalking", "Orbwalking")));

            var comboMenu = new Menu("Combo Options", "com.iseries.graves.combo");
            {
                comboMenu.AddItem(new MenuItem("com.iseries.graves.combo.useQ", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.graves.combo.useW", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.graves.combo.useR", "Use R").SetValue(true));
                root.AddSubMenu(comboMenu);
            }

            var harassMenu = new Menu("Harass Options", "com.iseries.graves.harass");
            {
                harassMenu.AddItem(new MenuItem("com.iseries.graves.harass.useQ", "Use Q").SetValue(false));
                harassMenu.AddItem(new MenuItem("com.iseries.graves.harass.useW", "Use W").SetValue(false));
                root.AddSubMenu(harassMenu);
            }

            var laneclearMenu = new Menu("Laneclear Options", "com.iseries.graves.laneclear");
            {
                laneclearMenu.AddItem(new MenuItem("com.iseries.graves.laneclear.useQ", "Use Q").SetValue(true));
                root.AddSubMenu(laneclearMenu);
            }

            var misc = new Menu("Misc Options", "com.iseries.graves.misc");
            {
                misc.AddItem(
                    new MenuItem("com.iseries.graves.misc.hitchance", "Hitchance").SetValue(
                        new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
                misc.AddItem(new MenuItem("com.iseries.graves.misc.peel", "Peel With E").SetValue(true));
                root.AddSubMenu(misc);
            }

            root.AddToMainMenu();
        }

        #endregion
    }
}