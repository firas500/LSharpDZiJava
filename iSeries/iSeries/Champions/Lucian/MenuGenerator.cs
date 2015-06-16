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
//   The menu generator class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iSeries.Champions.Lucian
{
    using LeagueSharp.Common;

    /// <summary>
    ///     The menu generator class.
    /// </summary>
    public static class MenuGenerator
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
            var comboMenu = new Menu("Combo Options", "com.iseries.lucian.combo");
            {
                comboMenu.AddItem(new MenuItem("com.iseries.lucian.combo.useQ", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.lucian.combo.extendedQ", "Extended Q").SetValue(false));
                comboMenu.AddItem(new MenuItem("com.iseries.lucian.combo.useW", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.lucian.combo.useE", "Use E").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.lucian.combo.useR", "Use R").SetValue(false));
                root.AddSubMenu(comboMenu);
            }

            var harassMenu = new Menu("Harass Options", "com.iseries.lucian.harass");
            {
                harassMenu.AddItem(new MenuItem("com.iseries.lucian.harass.useQ", "Use Q").SetValue(false));
                harassMenu.AddItem(new MenuItem("com.iseries.lucian.harass.useW", "Use W").SetValue(true));
                root.AddSubMenu(harassMenu);
            }

            var laneclearMenu = new Menu("Laneclear Options", "com.iseries.lucian.laneclear");
            {
                laneclearMenu.AddItem(new MenuItem("com.iseries.lucian.laneclear.useQ", "Use Q").SetValue(false));
                root.AddSubMenu(laneclearMenu);
            }

            var miscMenu = new Menu("Misc Menu", "com.iseries.lucian.misc");
            {
                miscMenu.AddItem(new MenuItem("com.iseries.lucian.misc.peel", "Peel With E").SetValue(true));
                root.AddSubMenu(miscMenu);
            }

            root.AddToMainMenu();
        }

        #endregion
    }
}