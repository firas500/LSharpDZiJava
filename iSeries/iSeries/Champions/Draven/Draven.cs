// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Draven.cs" company="LeagueSharp">
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
//   The given champion class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Runtime.Remoting.Channels;
using System.Xml.Linq;
using iSeries.General;

namespace iSeries.Champions.Draven
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using iSeries.Champions.Utilities;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Collision = LeagueSharp.Common.Collision;

    /// <summary>
    ///     The given champion class
    /// </summary>
    internal class Draven : Champion
    {
        #region Fields

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
        {
           { SpellSlot.E, new Spell(SpellSlot.E, 1000f) }, 
           { SpellSlot.R, new Spell(SpellSlot.R, 2000f) }
        };

        private List<Axe> axesList = new List<Axe>();

        private float LastListCheckTick;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Kalista" /> class.
        /// </summary>
        public Draven()
        {
            // Menu Generation
            this.CreateMenu = MenuGenerator.Generate;

            // Damage Indicator
            DamageIndicator.Enabled = false;
            DamageIndicator.Initialize();

            // Spell initialization
            this.spells[SpellSlot.E].SetSkillshot(250f, 130f, 1400f, false, SkillshotType.SkillshotLine);
            this.spells[SpellSlot.R].SetSkillshot(400f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            Orbwalking.OnNonKillableMinion += minion =>
            {

            };

            GameObject.OnCreate += (sender, args) =>
            {
                if (sender != null && sender.Name.Contains("Q_reticle_self"))
                {
                    axesList.Add(new Axe()
                    {
                        Position = sender.Position,
                        CreationTime = Game.Time,
                        EndTime = Game.Time + 1.20f
                    });
                }
            };

            GameObject.OnDelete += (sender, args) =>
            {
                if (sender != null && sender.Name.Contains("Q_reticle_self"))
                {
                    axesList.RemoveAll(axe => axe.AxeObject.NetworkId == sender.NetworkId);
                }
            };

            Drawing.OnDraw += Drawing_OnDraw;
        }


        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     <c>OnCombo</c> subscribed orbwalker function.
        /// </summary>
        public override void OnCombo()
        {

        }

        /// <summary>
        ///     <c>OnDraw</c> subscribed event function.
        /// </summary>
        /// <param name="args">
        ///     The event data
        /// </param>
        public override void OnDraw(EventArgs args)
        {
        }

        /// <summary>
        ///     <c>OnHarass</c> subscribed orb walker function.
        /// </summary>
        public override void OnHarass()
        {

        }

        /// <summary>
        ///     <c>OnLaneclear</c> subscribed orb walker function.
        /// </summary>
        public override void OnLaneclear()
        {

        }

        /// <summary>
        ///     <c>OnUpdate</c> subscribed event function.
        /// </summary>
        /// <param name="args">
        ///     The event data
        /// </param>
        public override void OnUpdate(EventArgs args)
        {
            switch (Variables.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    this.OnCombo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    this.OnHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    this.OnLaneclear();
                    break;
            }

            this.OnUpdateFunctions();
        }

        #endregion

        #region Methods
        private void Drawing_OnDraw(EventArgs args)
        {
                Render.Circle.DrawCircle(Game.CursorPos, Menu.Item("com.iseries.draven.misc.catchrange").GetValue<Slider>().Value,System.Drawing.Color.Gold);

            foreach (var reticle in axesList)
            {
                Render.Circle.DrawCircle(reticle.Position, 100f, System.Drawing.Color.Blue);
            }
        }

        /// <summary>
        ///     The Functions to always process
        /// </summary>
        private void OnUpdateFunctions()
        {
            CheckList();
        }

        private void CheckList()
        {
            if (Environment.TickCount - LastListCheckTick < 1200)
            {
                return;
            }
            LastListCheckTick = Environment.TickCount;
            axesList.RemoveAll(axe => !axe.IsValid);
        }

        private void CatchAxes(Mode mode)
        {
            var ModeName = mode.ToString().ToLowerInvariant();
            if (this.Menu.Item("com.iseries.draven.combo.catch" + ModeName).GetValue<bool>() && axesList.Any())
            {
                //Starting Axe Catching Logic
                var closestAxe =
                    axesList
                    .FindAll(axe => axe.IsValid && (axe.CanBeReachedNormal || (Menu.Item("com.iseries.draven.misc.wcatch").GetValue<bool>() && axe.CanBeReachedWithW && mode == Mode.Combo))
                        && (axe.Position.Distance(Game.CursorPos) <= Menu.Item("com.iseries.draven.misc.catchrange").GetValue<Slider>().Value))
                    .OrderBy(axe => axe.Position.Distance(Game.CursorPos))
                        .ThenBy(axe => axe.Position.Distance(ObjectManager.Player.ServerPosition)).FirstOrDefault();
                if (closestAxe != null)
                {
                    if ((closestAxe.Position.CountAlliesInRange(Menu.Item("com.iseries.draven.misc.safedistance").GetValue<Slider>().Value) >= closestAxe.Position.CountEnemiesInRange(Menu.Item("com.iseries.draven.misc.safedistance").GetValue<Slider>().Value)))
                    {
                        if (!closestAxe.CanBeReachedNormal && closestAxe.CanBeReachedWithW)
                        {
                            if (Menu.Item("com.iseries.draven.misc.wcatch").GetValue<bool>())
                            {
                                spells[SpellSlot.W].Cast();
                            }
                        }
                        //Allies >= Enemies. Catching axe.
                        if (Variables.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
                        {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, closestAxe.Position);
                        }
                        else
                        {
                            Variables.Orbwalker.SetOrbwalkingPoint(closestAxe.Position);
                        }
                    }
                }
            }
        }
        #endregion
    }

    internal class Axe
    {
        public GameObject AxeObject { get; set; }

        public Vector3 Position { get; set; }

        public float EndTime { get; set; }

        public float CreationTime { get; set; }

        public bool IsValid
        {
            get { return AxeObject.IsValid && EndTime >= Game.Time; }
        }
        public bool CanBeReachedNormal
        {
            get
            {
                var path = ObjectManager.Player.GetPath(ObjectManager.Player.ServerPosition, Position);
                var pathLength = 0f;
                for (var i = 1; i <= path.Count(); i++)
                {
                    var previousPoint = path[i - 1];
                    var currentPoint = path[i];
                    var currentDistance = Vector3.Distance(previousPoint, currentPoint);
                    pathLength += currentDistance;
                }
                var CanBeReached = pathLength / (ObjectManager.Player.MoveSpeed + Game.Time) < EndTime;
                return CanBeReached;
            }
        }

        public bool CanBeReachedWithW
        {
            get
            {
                var BuffedSpeed = (5 * ObjectManager.Player.GetSpell(SpellSlot.W).Level) + 0.35f * ObjectManager.Player.MoveSpeed;
                var path = ObjectManager.Player.GetPath(ObjectManager.Player.ServerPosition, Position);
                var pathLength = 0f;
                for (var i = 1; i <= path.Count(); i++)
                {
                    var previousPoint = path[i - 1];
                    var currentPoint = path[i];
                    var currentDistance = Vector3.Distance(previousPoint, currentPoint);
                    pathLength += currentDistance;
                }
                var CanBeReached = pathLength / (ObjectManager.Player.MoveSpeed + BuffedSpeed + Game.Time) < EndTime;
                return CanBeReached;
            }
        }

        public bool IsBeingCaught
        {
            get
            {
                return ObjectManager.Player.ServerPosition.Distance(Position) <
                       49 + (ObjectManager.Player.BoundingRadius / 2) + 50;
            }
        }
    }
}