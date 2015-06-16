// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Champion.cs" company="LeagueSharp">
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
//   The Generic champion
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iSeries.Champions
{
    using System;

    using iSeries.General;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     The Generic champion
    /// </summary>
    public abstract class Champion
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the creation menu action.
        /// </summary>
        public Action<Menu> CreateMenu { get; set; }

        /// <summary>
        ///     Gets the menu.
        /// </summary>
        public Menu Menu
        {
            get
            {
                return Variables.Menu;
            }
        }

        /// <summary>
        ///     Gets the player.
        /// </summary>
        public Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the champion type
        /// </summary>
        /// <returns>
        ///     The <see cref="ChampionType" />.
        /// </returns>
        public abstract ChampionType GetChampionType();

        /// <summary>
        ///     Gets a menu items value
        /// </summary>
        /// <param name="item">
        ///     the item name
        /// </param>
        /// <typeparam name="T">
        ///     The type parameter
        /// </typeparam>
        /// <returns>
        ///     The <see cref="T" />.
        /// </returns>
        public T GetItemValue<T>(string item)
        {
            return Variables.Menu.Item(item).GetValue<T>();
        }

        /// <summary>
        ///     Invokes the champion.
        /// </summary>
        public void Invoke()
        {
            Game.OnUpdate += this.OnUpdateFunctions;
            Drawing.OnDraw += this.OnDraw;
            this.CreateMenu(this.Menu);
        }

        /// <summary>
        ///     <c>OnCombo</c> subscribed orb walker function.
        /// </summary>
        public abstract void OnCombo();

        /// <summary>
        ///     <c>OnDraw</c> subscribed event function.
        /// </summary>
        /// <param name="args">
        ///     The event data
        /// </param>
        public abstract void OnDraw(EventArgs args);

        /// <summary>
        ///     <c>OnHarass</c> subscribed orb walker function.
        /// </summary>
        public abstract void OnHarass();

        /// <summary>
        ///     <c>OnLaneclear</c> subscribed orb walker function.
        /// </summary>
        public abstract void OnLaneclear();

        /// <summary>
        ///     <c>OnUpdate</c> subscribed event function.
        /// </summary>
        /// <param name="args">
        ///     The event data
        /// </param>
        public abstract void OnUpdate(EventArgs args);

        #endregion

        #region Methods

        /// <summary>
        ///     <c>OnUpdate</c> subscribed event function.
        /// </summary>
        /// <param name="args">
        ///     The event data
        /// </param>
        private void OnUpdateFunctions(EventArgs args)
        {
            switch (this.GetChampionType())
            {
                case ChampionType.Marksman:
                    // TODO more ad carry shit
                    if (ObjectManager.Player.Level >= 6 && ObjectManager.Player.InShop() && !(Items.HasItem(3342) || Items.HasItem(3363)))
                    {
                        ObjectManager.Player.BuyItem(ItemId.Scrying_Orb_Trinket);
                    }
                    break;
                case ChampionType.Mage:
                    // TODO ap caster shit here :S
                    break;
                case ChampionType.Tank:
                    // TODO tank shit
                    break;
                case ChampionType.Support:
                    // TODO support shit
                    break;
            }

            this.OnUpdate(args);
        }

        #endregion
    }
}