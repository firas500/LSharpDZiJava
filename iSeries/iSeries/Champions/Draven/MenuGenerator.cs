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
namespace iSeries.Champions.Draven
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
            var comboMenu = new Menu("Combo Options", "com.iseries.draven.combo");
            {
                comboMenu.AddItem(new MenuItem("com.iseries.draven.combo.useQ", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.draven.combo.useW", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.draven.combo.useE", "Use E").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.draven.combo.useR", "Use R").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.draven.combo.catchcombo", "Catch Axes in Combo").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.draven.combo.wmana", "W Mana").SetValue(new Slider(35)));
                root.AddSubMenu(comboMenu);
            }

            var harassMenu = new Menu("Harass Options", "com.iseries.draven.harass");
            {
                harassMenu.AddItem(new MenuItem("com.iseries.draven.harass.useE", "Use E").SetValue(false));
                harassMenu.AddItem(new MenuItem("com.iseries.draven.harass.catchharass", "Catch Axes in Harass").SetValue(true));

                root.AddSubMenu(harassMenu);
            }

            var farmMenu = new Menu("Farm Options", "com.iseries.draven.farm");
            {
                farmMenu.AddItem(new MenuItem("com.iseries.draven.farm.catchfarm", "Catch Axes in Farm").SetValue(true));
                root.AddSubMenu(farmMenu);
            }

            var misc = new Menu("Misc Options", "com.iseries.draven.misc");
            {
                misc.AddItem(
                    new MenuItem("com.iseries.draven.misc.maxQ", "Max Axes").SetValue(new Slider(2, 1, 4)));
                misc.AddItem(
                    new MenuItem("com.iseries.draven.misc.catchrange", "Catch Range").SetValue(new Slider(395,65,850)));
                misc.AddItem(new MenuItem("com.iseries.draven.misc.safedistance", "Axes Safe Distance").SetValue(new Slider(120, 0, 550)));
                misc.AddItem(new MenuItem("com.iseries.draven.misc.eagp", "E Antigapcloser").SetValue(true));
                misc.AddItem(new MenuItem("com.iseries.draven.misc.eint", "E Interrupter").SetValue(true));
                misc.AddItem(new MenuItem("com.iseries.draven.misc.epeel", "E Peel").SetValue(true));
                root.AddSubMenu(misc);
            }
            root.AddToMainMenu();
        }

        #endregion
    }
}