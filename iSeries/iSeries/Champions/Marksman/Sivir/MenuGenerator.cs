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
namespace iSeries.Champions.Marksman.Sivir
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
            var comboMenu = new Menu("Combo Options", "com.iseries.sivir.combo");
            {
                comboMenu.AddItem(new MenuItem("com.iseries.sivir.combo.useQ", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.sivir.combo.useW", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.sivir.combo.wmana", "W Mana").SetValue(new Slider(35)));
                root.AddSubMenu(comboMenu);
            }

            var harassMenu = new Menu("Harass Options", "com.iseries.sivir.harass");
            {
                harassMenu.AddItem(new MenuItem("com.iseries.sivir.harass.useQ", "Use Q").SetValue(false));
                harassMenu.AddItem(new MenuItem("com.iseries.sivir.harass.qmana", "Q Mana").SetValue(new Slider(35)));
                root.AddSubMenu(harassMenu);
            }

            var farmMenu = new Menu("Laneclear Options", "com.iseries.sivir.farm");
            {
                farmMenu.AddItem(new MenuItem("com.iseries.sivir.farm.useQ", "Q in Laneclear").SetValue(false));
                farmMenu.AddItem(new MenuItem("com.iseries.sivir.farm.qmana", "Q Mana").SetValue(new Slider(35)));
                root.AddSubMenu(farmMenu);
            }

            var misc = new Menu("Misc Options", "com.iseries.sivir.misc");
            {
                misc.AddItem(new MenuItem("com.iseries.sivir.misc.eshield", "E Shield Targetted Spells").SetValue(true));
                misc.AddItem(new MenuItem("com.iseries.sivir.misc.eshieldkill", "Only if they will kill").SetValue(false));

                root.AddSubMenu(misc);
            }

            var draw = new Menu("Draw Options", "com.iseries.sivir.draw");
            {
                draw.AddItem(new MenuItem("com.iseries.sivir.draw.q", "Draw Q").SetValue(true));
                root.AddSubMenu(draw);
            }

            root.AddToMainMenu();
        }

        #endregion
    }
}