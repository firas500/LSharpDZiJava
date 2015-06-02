// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpriteHandler.cs" company="LeagueSharp">
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
//   Sprite Handler
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace IKalista
{
    using System.Linq;

    using IKalista.Properties;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    /// <summary>
    ///     Sprite Handler
    /// </summary>
    internal class SpriteHandler
    {
        /// <summary>
        /// The R sprite instance.
        /// </summary>
        private static Render.Sprite sprite;

        /// <summary>
        /// Gets the current target we will draw the sprite on
        /// </summary>
        private static Obj_AI_Hero WTarget
        {
            get
            {
                foreach (Obj_AI_Hero source in HeroManager.Enemies.Where(x => ObjectManager.Player.Distance(x) <= 1000f))
                {
                    if (source.IsValidTarget(1000f) && source.HasBuff("KalistaCoopStrikeProtect"))
                    {
                        return source;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the R target position on the screen.
        /// </summary>
        private static Vector2 WTargetPosition
        {
            get
            {
                return WTarget != null ?
                    new Vector2(
                        Drawing.WorldToScreen(WTarget.Position).X - WTarget.BoundingRadius * 2 +
                        WTarget.BoundingRadius / 1.5f,
                        Drawing.WorldToScreen(WTarget.Position).Y - WTarget.BoundingRadius * 2) : new Vector2();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the sprite should be drawn or not.
        /// </summary>
        private static bool DrawCondition
        {
            get { return WTarget != null && Render.OnScreen(Drawing.WorldToScreen(WTarget.Position)) && Kalista.boolLinks["drawSprite"].Value; }
        }

        /// <summary>
        /// Initializes the sprite reference. To be called when the assembly is loaded.
        /// </summary>
        internal static void InitializeSprite()
        {
            sprite = new Render.Sprite(Properties.Resources.ScopeSprite, new Vector2());
            {
                sprite.Scale = new Vector2(1.0f, 1.0f);
                sprite.PositionUpdate = () => WTargetPosition;
                sprite.VisibleCondition = s => DrawCondition;
            }

            sprite.Add();
        }
    }
}