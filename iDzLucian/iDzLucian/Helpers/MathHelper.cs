// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MathHelper.cs" company="LeagueSharp">
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
//   TODO The math helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iDzLucian.Helpers
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    /// <summary>
    ///     TODO The math helper.
    /// </summary>
    internal class MathHelper
    {
        #region Public Methods and Operators

        /// <summary>
        /// TODO The get cicle line interaction.
        /// </summary>
        /// <param name="from">
        /// TODO The from.
        /// </param>
        /// <param name="to">
        /// TODO The to.
        /// </param>
        /// <param name="cPos">
        /// TODO The c pos.
        /// </param>
        /// <param name="radius">
        /// TODO The radius.
        /// </param>
        /// <returns>
        /// </returns>
        public static CircInter GetCicleLineInteraction(Vector2 from, Vector2 to, Vector2 cPos, float radius)
        {
            var res = new CircInter();

            var dx = from.X - to.X;
            var dy = from.Y - to.Y;

            var A = dx * dx + dy * dy;
            var B = 2 * (dx * (to.X - cPos.X) + dy * (to.Y - cPos.Y));
            var C = (to.X - cPos.X) * (to.X - cPos.X) + (to.Y - cPos.Y) * (to.Y - cPos.Y) - radius * radius;

            var det = B * B - 4 * A * C;
            if ((A <= 0.0000001) || (det < 0))
            {
                res.none = true;

                // No real solutions.
            }
            else if (det == 0)
            {
                res.one = true;

                // One solution.
                var t = -B / (2 * A);
                res.inter1 = new Vector2(to.X + t * dx, to.Y + t * dy);
            }
            else
            {
                // Two solutions.
                var t = (float)((-B + Math.Sqrt(det)) / (2 * A));
                res.inter1 = new Vector2(to.X + t * dx, to.Y + t * dy);
                t = (float)((-B - Math.Sqrt(det)) / (2 * A));
                res.inter2 = new Vector2(to.X + t * dx, to.Y + t * dy);
            }

            return res;
        }

        #endregion

        /// <summary>
        ///     TODO The circ inter.
        /// </summary>
        public class CircInter
        {
            #region Fields

            /// <summary>
            ///     TODO The inter 1.
            /// </summary>
            public Vector2 inter1;

            /// <summary>
            ///     TODO The inter 2.
            /// </summary>
            public Vector2 inter2;

            /// <summary>
            ///     TODO The none.
            /// </summary>
            public bool none;

            /// <summary>
            ///     TODO The one.
            /// </summary>
            public bool one;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="CircInter" /> class.
            /// </summary>
            public CircInter()
            {
                this.one = false;
                this.none = false;
                this.inter1 = new Vector2();
                this.inter2 = new Vector2();
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            ///     TODO The get best inter.
            /// </summary>
            /// <param name="target">
            ///     TODO The target.
            /// </param>
            /// <returns>
            /// </returns>
            public Vector2 GetBestInter(Obj_AI_Base target)
            {
                if (this.none)
                {
                    return new Vector2(0, 0);
                }

                if (this.one)
                {
                    return this.inter1;
                }

                var dist1 = target.Distance(this.inter1, true);
                var dist2 = target.Distance(this.inter2, true);

                return dist1 > dist2 ? this.inter2 : this.inter1;
            }

            #endregion
        }
    }
}