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
namespace iSeries.Champions.Twitch
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
            var comboMenu = new Menu("Combo Options", "com.iseries.twitch.combo");
            {
                comboMenu.AddItem(new MenuItem("com.iseries.twitch.combo.useW", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.twitch.combo.useEKillable", "Use E When Killable").SetValue(true));
                comboMenu.AddItem(new MenuItem("com.iseries.twitch.combo.useEMaxStacks", "Use E At Max Stacks").SetValue(false));
                comboMenu.AddItem(new MenuItem("com.iseries.twitch.combo.useENearlyOutOfRange", "Use E When Target is Nearly Out of Range").SetValue(false));
                comboMenu.AddItem(
                    new MenuItem("com.iseries.twitch.combo.wMana", "Min Mana W %").SetValue(new Slider(40, 10)));
                root.AddSubMenu(comboMenu);
            }

            var harassMenu = new Menu("Harass Options", "com.iseries.twitch.harass");
            {
                harassMenu.AddItem(new MenuItem("com.iseries.twitch.harass.useQ", "Use Q").SetValue(false));
                harassMenu.AddItem(new MenuItem("com.iseries.twitch.harass.useE", "Use E").SetValue(false));
                root.AddSubMenu(harassMenu);
            }

            var laneclearMenu = new Menu("Laneclear Options", "com.iseries.twitch.laneclear");
            {
                laneclearMenu.AddItem(new MenuItem("com.iseries.twitch.laneclear.useE", "Use E").SetValue(true));
                root.AddSubMenu(laneclearMenu);
            }

            var misc = new Menu("Misc Options", "com.iseries.twitch.misc");
            {
                misc.AddItem(new MenuItem("com.iseries.twitch.misc.killsteal", "Always Killsteal").SetValue(true));
                misc.AddItem(new MenuItem("com.iseries.twitch.misc.mobsteal", "Mobsteal").SetValue(true));
                root.AddSubMenu(misc);
            }

            var drawing = new Menu("Drawing Options", "com.iseries.twitch.drawing");
            {
                drawing.AddItem(new MenuItem("com.iseries.twitch.drawing.drawE", "Draw E Range").SetValue(true));
                drawing.AddItem(new MenuItem("com.iseries.twitch.drawing.drawStacks", "Draw Stacks").SetValue(true));
                root.AddSubMenu(drawing);
            }

            var noEOnMenu = new Menu("Don't E Options", "com.iseries.twitch.noe.");
            {
                foreach (var champ in HeroManager.Enemies)
                {
                    noEOnMenu.AddItem(
                        new MenuItem(
                            "com.iseries.twitch.noe." + champ.ChampionName.ToLowerInvariant(), champ.ChampionName).SetValue(false));
                }
                root.AddSubMenu(noEOnMenu);
            }
            
            root.AddToMainMenu();
        }

        #endregion
    }
}