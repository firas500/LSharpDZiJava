// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PositionHelper.cs" company="LeagueSharp">
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
//   TODO The position helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iSeries.Champions.Utilities
{
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    /// <summary>
    ///     TODO The position helper.
    /// </summary>
    internal class PositionHelper
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Enemies please
        /// </summary>
        /// <param name="position">
        ///     Position please
        /// </param>
        /// <param name="range">
        ///     Range please
        /// </param>
        /// <returns>
        ///     The List of enemies
        /// </returns>
        public static List<Obj_AI_Hero> GetLhEnemiesNearPosition(Vector3 position, float range)
        {
            return
                HeroManager.Enemies.Where(hero => hero.IsValidTarget(range, true, position) && hero.HealthPercent <= 15)
                    .ToList();
        }

        /// <summary>
        ///     Checks if the <see cref="Vector3" /> Position is safe
        /// </summary>
        /// <param name="position">
        ///     The Position
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool IsSafePosition(Vector3 position)
        {
            if (position.UnderTurret(true) && !ObjectManager.Player.UnderTurret(true))
            {
                return false;
            }

            var allies = position.CountAlliesInRange(ObjectManager.Player.AttackRange);
            var enemies = position.CountEnemiesInRange(ObjectManager.Player.AttackRange);
            var lhEnemies = GetLhEnemiesNearPosition(position, ObjectManager.Player.AttackRange).Count();

            if (enemies == 1)
            {
                // It's a 1v1, safe to assume I can E
                return true;
            }

            // Adding 1 for the Player
            return allies + 1 > enemies - lhEnemies;
        }

        #endregion
    }
}