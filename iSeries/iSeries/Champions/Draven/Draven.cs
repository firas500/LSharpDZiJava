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
namespace iSeries.Champions.Draven
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using iSeries.Champions.Utilities;
    using iSeries.General;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;

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
                                                                       { SpellSlot.W, new Spell(SpellSlot.W) },
                                                                       { SpellSlot.R, new Spell(SpellSlot.R, 2000f) }
                                                                   };

        /// <summary>
        ///     The Axe List
        /// </summary>
        private readonly List<Axe> axesList = new List<Axe>();

        /// <summary>
        ///     The checking tick?
        /// </summary>
        private float lastListCheckTick;

        /// <summary>
        /// The number of axes the player has on him.
        /// </summary>
        private int QStacks
        {
            get
            {
                return ObjectManager.Player.GetBuff("dravenspinningattack").Count;
            }
        }
        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Draven"/> class. 
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

            Orbwalking.OnNonKillableMinion += minion => { };

            GameObject.OnCreate += (sender, args) =>
                {
                    if (sender != null && sender.Name.Contains("Q_reticle_self"))
                    {
                        this.axesList.Add(
                            new Axe()
                                {
                                   Position = sender.Position, CreationTime = Game.Time, EndTime = Game.Time + 1.20f 
                                });
                    }
                };

            GameObject.OnDelete += (sender, args) =>
                {
                    if (sender != null && sender.Name.Contains("Q_reticle_self"))
                    {
                        this.axesList.RemoveAll(axe => axe.AxeObject.NetworkId == sender.NetworkId);
                    }
                };

        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     <c>OnCombo</c> subscribed orbwalker function.
        /// </summary>
        public override void OnCombo()
        {
            CatchAxes(Mode.Combo);
            if (Menu.Item("com.iseries.draven.combo.useQ").GetValue<bool>() && ObjectManager.Player.GetEnemiesInRange(900f).Any(en => en.IsValidTarget()) && spells[SpellSlot.Q].IsReady())
            {
                var maxQ = Menu.Item("com.iseries.draven.misc.maxQ").GetValue<Slider>().Value;
                var onPlayer = QStacks;
                var onGround = axesList.Count;
                if (onGround + onPlayer + 1 <= maxQ)
                {
                    spells[SpellSlot.Q].Cast();
                }
            }
            var eTarget = TargetSelector.GetTarget(spells[SpellSlot.E].Range-175f, TargetSelector.DamageType.Physical);
            if (Menu.Item("com.iseries.draven.combo.useE").GetValue<bool>() && eTarget.IsValidTarget() && spells[SpellSlot.E].IsReady())
            {
                spells[SpellSlot.E].CastIfHitchanceEquals(eTarget, HitChance.VeryHigh); 
            }

            /**TODO
             * 
             * R logic here, with:
             * Collision
             * Distance Checking
             * Return logic(?)
             */
        }

        /// <summary>
        ///     <c>OnDraw</c> subscribed event function.
        /// </summary>
        /// <param name="args">
        ///     The event data
        /// </param>
        public override void OnDraw(EventArgs args)
        {
            Render.Circle.DrawCircle(
                Game.CursorPos,
                this.Menu.Item("com.iseries.draven.misc.catchrange").GetValue<Slider>().Value,
                Color.Gold);

            foreach (var reticle in this.axesList)
            {
                Render.Circle.DrawCircle(reticle.Position, 100f, Color.Blue);
            }
        }

        /// <summary>
        ///     <c>OnHarass</c> subscribed orb walker function.
        /// </summary>
        public override void OnHarass()
        {
            CatchAxes(Mode.Harass);
        }

        /// <summary>
        ///     <c>OnLaneclear</c> subscribed orb walker function.
        /// </summary>
        public override void OnLaneclear()
        {
            CatchAxes(Mode.Farm);
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

        /// <summary>
        ///     The Axe Catching Logic
        /// </summary>
        /// <param name="mode">
        ///     The Mode
        /// </param>
        private void CatchAxes(Mode mode)
        {
            var modeName = mode.ToString().ToLowerInvariant();
            if (this.Menu.Item("com.iseries.draven.combo.catch" + modeName).GetValue<bool>() && this.axesList.Any())
            {
                // Starting Axe Catching Logic
                var closestAxe =
                    this.axesList.FindAll(
                        axe =>
                        axe.IsValid
                        && (axe.CanBeReachedNormal
                            || (this.Menu.Item("com.iseries.draven.misc.wcatch").GetValue<bool>()
                                && axe.CanBeReachedWithW && mode == Mode.Combo))
                        && (axe.Position.Distance(Game.CursorPos)
                            <= this.Menu.Item("com.iseries.draven.misc.catchrange").GetValue<Slider>().Value))
                        .OrderBy(axe => axe.Position.Distance(Game.CursorPos))
                        .ThenBy(axe => axe.Position.Distance(ObjectManager.Player.ServerPosition))
                        .FirstOrDefault();
                if (closestAxe != null)
                {
                    if (
                        closestAxe.Position.CountAlliesInRange(
                            this.Menu.Item("com.iseries.draven.misc.safedistance").GetValue<Slider>().Value)
                        >= closestAxe.Position.CountEnemiesInRange(
                            this.Menu.Item("com.iseries.draven.misc.safedistance").GetValue<Slider>().Value))
                    {
                        if (!closestAxe.CanBeReachedNormal && closestAxe.CanBeReachedWithW)
                        {
                            if (this.Menu.Item("com.iseries.draven.misc.wcatch").GetValue<bool>())
                            {
                                this.spells[SpellSlot.W].Cast();
                            }
                        }

                        // Allies >= Enemies. Catching axe.
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

        /// <summary>
        ///     The Axe Checking List
        /// </summary>
        private void CheckList()
        {
            if (Environment.TickCount - this.lastListCheckTick < 1200)
            {
                return;
            }

            this.lastListCheckTick = Environment.TickCount;
            this.axesList.RemoveAll(axe => !axe.IsValid);
        }   

        /// <summary>
        ///     The Functions to always process
        /// </summary>
        private void OnUpdateFunctions()
        {
            this.CheckList();
        }

        #endregion
    }

    /// <summary>
    ///     The Axe Class
    /// </summary>
    internal class Axe
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the axe object.
        /// </summary>
        public GameObject AxeObject { get; set; }

        /// <summary>
        ///     Gets a value indicating whether can be reached normal.
        /// </summary>
        public bool CanBeReachedNormal
        {
            get
            {
                var path = ObjectManager.Player.GetPath(ObjectManager.Player.ServerPosition, this.Position);
                var pathLength = 0f;
                for (var i = 1; i <= path.Count(); i++)
                {
                    var previousPoint = path[i - 1];
                    var currentPoint = path[i];
                    var currentDistance = Vector3.Distance(previousPoint, currentPoint);
                    pathLength += currentDistance;
                }

                var canBeReached = pathLength / (ObjectManager.Player.MoveSpeed + Game.Time) < this.EndTime;
                return canBeReached;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether can be reached with w.
        /// </summary>
        public bool CanBeReachedWithW
        {
            get
            {
                var buffedSpeed = (5 * ObjectManager.Player.GetSpell(SpellSlot.W).Level)
                                  + 0.35f * ObjectManager.Player.MoveSpeed;
                var path = ObjectManager.Player.GetPath(ObjectManager.Player.ServerPosition, this.Position);
                var pathLength = 0f;
                for (var i = 1; i <= path.Count(); i++)
                {
                    var previousPoint = path[i - 1];
                    var currentPoint = path[i];
                    var currentDistance = Vector3.Distance(previousPoint, currentPoint);
                    pathLength += currentDistance;
                }

                var canBeReached = pathLength / (ObjectManager.Player.MoveSpeed + buffedSpeed + Game.Time)
                                   < this.EndTime;
                return canBeReached;
            }
        }

        /// <summary>
        ///     Gets or sets the creation time.
        /// </summary>
        public float CreationTime { get; set; }

        /// <summary>
        ///     Gets or sets the end time.
        /// </summary>
        public float EndTime { get; set; }

        /// <summary>
        ///     Gets a value indicating whether is being caught.
        /// </summary>
        public bool IsBeingCaught
        {
            get
            {
                return ObjectManager.Player.ServerPosition.Distance(this.Position)
                       < 49 + (ObjectManager.Player.BoundingRadius / 2) + 50;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return this.AxeObject.IsValid && this.EndTime >= Game.Time;
            }
        }

        /// <summary>
        ///     Gets or sets the position.
        /// </summary>
        public Vector3 Position { get; set; }

        #endregion
    }
}