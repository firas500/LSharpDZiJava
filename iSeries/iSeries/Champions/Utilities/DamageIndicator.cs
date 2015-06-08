// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DamageIndicator.cs" company="LeagueSharp">
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
//   TODO The damage indicator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iSeries.Champions.Utilities
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;

    /// <summary>
    ///     The Damage Inidicator
    /// </summary>
    internal class DamageIndicator
    {
        #region Constants

        /// <summary>
        ///     TODO The bar width.
        /// </summary>
        private const int BarWidth = 104;

        /// <summary>
        ///     TODO The line thickness.
        /// </summary>
        private const int LineThickness = 9;

        #endregion

        #region Static Fields

        /// <summary>
        ///     TODO The bar offset.
        /// </summary>
        private static readonly Vector2 BarOffset = new Vector2(10, 25);

        /// <summary>
        ///     TODO The damage to unit.
        /// </summary>
        private static DamageToUnitDelegate damageToUnit;

        #endregion

        #region Delegates

        /// <summary>
        ///     The Damage to unit delegate
        /// </summary>
        /// <param name="target">
        ///     The Target to draw
        /// </param>
        public delegate float DamageToUnitDelegate(Obj_AI_Base target);

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the damage to unit.
        /// </summary>
        public static DamageToUnitDelegate DamageToUnit
        {
            get
            {
                return damageToUnit;
            }

            set
            {
                if (damageToUnit == null)
                {
                    Drawing.OnDraw += Drawing_OnDraw;
                }

                damageToUnit = value;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether enabled.
        /// </summary>
        public static bool Enabled { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Initialize the Damage Indicator
        /// </summary>
        public static void Initialize()
        {
            // Apply needed field delegate for damage calculation
            Enabled = true;

            // Register event handlers
            Drawing.OnDraw += Drawing_OnDraw;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The Drawing Method
        /// </summary>
        /// <param name="args">
        ///     The Event Arguments
        /// </param>
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Enabled)
            {
                return;
            }

            foreach (var unit in HeroManager.Enemies.Where(u => u.IsValidTarget() && u.IsHPBarRendered))
            {
                // Get damage to unit
                var damage = damageToUnit(unit);

                // Continue on 0 damage
                if (damage <= 0)
                {
                    continue;
                }

                // Get remaining HP after damage applied in percent and the current percent of health
                var damagePercentage = ((unit.Health - damage) > 0 ? (unit.Health - damage) : 0) / unit.MaxHealth;
                var currentHealthPercentage = unit.Health / unit.MaxHealth;

                // Calculate start and end point of the bar indicator
                var startPoint = new Vector2(
                    (int)(unit.HPBarPosition.X + BarOffset.X + (damagePercentage * BarWidth)), 
                    (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);
                var endPoint =
                    new Vector2(
                        (int)(unit.HPBarPosition.X + BarOffset.X + (currentHealthPercentage * BarWidth) + 1), 
                        (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);

                // Draw the line
                Drawing.DrawLine(startPoint, endPoint, LineThickness, Color.LawnGreen);
            }
        }

        #endregion
    }
}