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
namespace iSeries.Champions.Marksman.Ezreal
{
    using LeagueSharp.Common;

    /// <summary>
    ///     TODO The menu generator.
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
            var comboMenu = new Menu("Combo Options", "com.iseries.ezreal.combo");
            {
                comboMenu.AddItem(new MenuItem("com.iseries.ezreal.combo.useQ", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.ezreal.combo.useW", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.ezreal.combo.useR", "Use R").SetValue(true));
                root.AddSubMenu(comboMenu);
            }

            var harassMenu = new Menu("Harass Options", "com.iseries.ezreal.harass");
            {
                var autoHarassMenu = new Menu("Auto Harass", "com.iseries.ezreal.harass.auto");
                {
                    var disabledChampions = new Menu("Disabled Champions", "com.iseries.ezreal.harass.auto.disable");
                    {
                        foreach (var hero in HeroManager.Enemies)
                        {
                            disabledChampions.AddItem(
                                new MenuItem(
                                    "com.iseries.ezreal.harass.auto.disable" + hero.ChampionName,
                                    "Disable: " + hero.ChampionName).SetValue(false));
                        }
                        autoHarassMenu.AddSubMenu(disabledChampions);
                    }
                    autoHarassMenu.AddItem(
                        new MenuItem("com.iseries.ereal.harass.auto.useQ", "Auto Harass Q").SetValue(false));
                    autoHarassMenu.AddItem(
                        new MenuItem("com.iseries.ereal.harass.auto.useW", "Auto Harass W").SetValue(false));
                    autoHarassMenu.AddItem(
                        new MenuItem("com.iseries.ereal.harass.auto.autoHarass", "Enable Auto Harass").SetValue(false));
                    harassMenu.AddSubMenu(autoHarassMenu);
                }
                harassMenu.AddItem(new MenuItem("com.iseries.ezreal.harass.useQ", "Use Q").SetValue(false));
                harassMenu.AddItem(new MenuItem("com.iseries.ezreal.harass.useW", "Use W").SetValue(false));
                root.AddSubMenu(harassMenu);
            }

            var laneclearMenu = new Menu("Laneclear Options", "com.iseries.ezreal.laneclear");
            {
                laneclearMenu.AddItem(new MenuItem("com.iseries.ezreal.laneclear.useQ", "Use Q").SetValue(true));
                laneclearMenu.AddItem(
                    new MenuItem("com.iseries.ezreal.laneclear.useQKill", "Q Unkillable Minions").SetValue(true));
                root.AddSubMenu(laneclearMenu);
            }

            var misc = new Menu("Misc Options", "com.iseries.ezreal.misc");
            {
                misc.AddItem(new MenuItem("com.iseries.ezreal.misc.sheen", "Sheen Weave").SetValue(true));
                misc.AddItem(
                    new MenuItem("com.iseries.ezreal.misc.hitchance", "Hitchance").SetValue(
                        new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
                misc.AddItem(new MenuItem("com.iseries.ezreal.misc.peel", "Peel With E").SetValue(true));
                misc.AddItem(new MenuItem("com.iseries.ezreal.misc.muramana", "Use Muramana").SetValue(true));
                root.AddSubMenu(misc);
            }

            var draw = new Menu("Misc Options", "com.iseries.ezreal.draw");
            {
                draw.AddItem(new MenuItem("com.iseries.ezreal.draw.q", "Draw Q").SetValue(false));
                root.AddSubMenu(draw);
            }
            root.AddToMainMenu();
        }

        #endregion
    }
}