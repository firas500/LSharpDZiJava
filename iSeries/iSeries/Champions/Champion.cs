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
//      The Generic champion
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace iSeries.Champions
{
    using System;

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
                return Variables.Player;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Invokes the champion.
        /// </summary>
        public void Invoke()
        {
            Game.OnUpdate += this.OnUpdate;
            Drawing.OnDraw += this.OnDraw;
            this.CreateMenu(this.Menu);
        }

        /// <summary>
        ///     <c>OnDraw</c> subscribed event function.
        /// </summary>
        /// <param name="args">
        ///     The event data
        /// </param>
        public abstract void OnDraw(EventArgs args);

        /// <summary>
        ///     <c>OnUpdate</c> subscribed event function.
        /// </summary>
        /// <param name="args">
        ///     The event data
        /// </param>
        public abstract void OnUpdate(EventArgs args);

        /// <summary>
        ///     <c>OnCombo</c> subscribed orb walker function.
        /// </summary>
        public abstract void OnCombo();

        /// <summary>
        ///     <c>OnHarass</c> subscribed orb walker function.
        /// </summary>
        public abstract void OnHarass();


        /// <summary>
        ///     <c>OnLaneclear</c> subscribed orb walker function.
        /// </summary>
        public abstract void OnLaneclear();

        #endregion
    }
}