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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

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
        ///     The Axe List
        /// </summary>
        private readonly List<Axe> axesList = new List<Axe>();

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
        ///     The checking tick?
        /// </summary>
        private float lastListCheckTick;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Draven" /> class.
        /// </summary>
        public Draven()
        {
            // Menu Generation
            this.CreateMenu = MenuGenerator.Generate;

            // Spell initialization
            this.spells[SpellSlot.E].SetSkillshot(250f, 130f, 1400f, false, SkillshotType.SkillshotLine);
            this.spells[SpellSlot.R].SetSkillshot(400f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            GameObject.OnCreate += (sender, args) =>
                {
                    if (sender != null && sender.Name.Contains("Q_reticle_self"))
                    {
                        var axe = new Axe()
                                      {
                                          AxeObject = sender,
                                          Position = sender.Position, CreationTime = Game.Time, 
                                          EndTime = Game.Time + 1.20f
                                      };
                        this.axesList.Add(axe);
                        Utility.DelayAction.Add(1800, () => { 
                            if(this.axesList.Contains(axe))
                            {
                                this.axesList.Remove(axe);
                            } 
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

            AntiGapcloser.OnEnemyGapcloser += this.OnIncomingGapcloser;
            Interrupter2.OnInterruptableTarget += this.OnInterruptableTarget;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the Q Stacks
        /// </summary>
        private int QStacks
        {
            get
            {
                return ObjectManager.Player.GetBuff("dravenspinningattack").Count;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets Enemies near a position
        /// </summary>
        /// <param name="position">
        ///     The Position
        /// </param>
        /// <param name="range">
        ///     The Range
        /// </param>
        /// <returns>
        ///     a list of enemies
        /// </returns>
        public static List<Obj_AI_Hero> GetLhEnemiesNearPosition(Vector3 position, float range)
        {
            return
                HeroManager.Enemies.Where(hero => hero.IsValidTarget(range, true, position) && hero.HealthPercent <= 15)
                    .ToList();
        }

        /// <summary>
        ///     <c>OnCombo</c> subscribed orbwalker function.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        public override void OnCombo()
        {
            this.CatchAxes(Mode.Combo);

            if (this.Menu.Item("com.iseries.draven.combo.useQ").GetValue<bool>()
                && ObjectManager.Player.GetEnemiesInRange(900f).Any(en => en.IsValidTarget())
                && this.spells[SpellSlot.Q].IsReady())
            {
                var maxQ = this.Menu.Item("com.iseries.draven.misc.maxQ").GetValue<Slider>().Value;
                var onPlayer = this.QStacks;
                var onGround = this.axesList.Count;
                if (onGround + onPlayer + 1 <= maxQ)
                {
                    this.spells[SpellSlot.Q].Cast();
                }

                Console.WriteLine("OnPlayer: " + onPlayer + " OnGround: " + onGround + " Max Q:" + maxQ);
            }

            var eTarget = TargetSelector.GetTarget(
                this.spells[SpellSlot.E].Range - 175f, 
                TargetSelector.DamageType.Physical);
            if (this.Menu.Item("com.iseries.draven.combo.useE").GetValue<bool>() && eTarget.IsValidTarget()
                && this.spells[SpellSlot.E].IsReady() )
            {
                this.spells[SpellSlot.E].CastIfHitchanceEquals(eTarget, HitChance.VeryHigh);
            }

            if (this.GetItemValue<bool>("com.iseries.draven.combo.useR"))
            {
                var rTarget = TargetSelector.GetTarget(
                    this.spells[SpellSlot.R].Range, 
                    TargetSelector.DamageType.Physical);
                if (!rTarget.IsValidTarget())
                {
                    return;
                }
                var rPrediction = this.spells[SpellSlot.R].GetPrediction(rTarget);
                var rCollision = this.spells[SpellSlot.R].GetCollision(
                    ObjectManager.Player.ServerPosition.To2D(), 
                    new List<Vector2>() { rPrediction.CastPosition.To2D() });
                var rDamageMultiplier = 1.0;
                if (rCollision.Any())
                {
                    rDamageMultiplier = (rCollision.Count() > 7) ? 0.4 : (1 - (rCollision.Count() / 12.5));
                }

                if (rTarget.Health + 20 < this.spells[SpellSlot.R].GetDamage(rTarget) * rDamageMultiplier
                    && rPrediction.Hitchance >= HitChance.VeryHigh)
                {
                    this.spells[SpellSlot.R].Cast(rTarget);
                }
            }
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
            var eTarget = TargetSelector.GetTarget(
                this.spells[SpellSlot.E].Range - 175f, 
                TargetSelector.DamageType.Physical);
            if (this.Menu.Item("com.iseries.draven.harass.useE").GetValue<bool>() && eTarget.IsValidTarget()
                && this.spells[SpellSlot.E].IsReady())
            {
                this.spells[SpellSlot.E].CastIfHitchanceEquals(eTarget, HitChance.VeryHigh);
            }

            this.CatchAxes(Mode.Harass);
        }

        /// <summary>
        ///     <c>OnLaneclear</c> subscribed orb walker function.
        /// </summary>
        public override void OnLaneclear()
        {
            this.CatchAxes(Mode.Farm);
        }

        /// <summary>
        ///     <c>OnUpdate</c> subscribed event function.
        /// </summary>
        /// <param name="args">
        ///     The event data
        /// </param>
        public override void OnUpdate(EventArgs args)
        {
            if (!axesList.Any())
            {
                Variables.Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
            }

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

        /// <summary>
        ///     Checks if the position is under an ally turret
        /// </summary>
        /// <param name="position">
        ///     The Position
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool UnderAllyTurret(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Any(t => t.IsAlly && !t.IsDead);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Checks if you can cast your spell W
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        private bool CanCastW()
        {
            return this.spells[SpellSlot.W].IsReady() && this.GetItemValue<bool>("com.iseries.draven.combo.useW")
                   && this.GetItemValue<Slider>("com.iseries.draven.combo.wmana").Value
                   > ObjectManager.Player.ManaPercent;
        }

        /// <summary>
        ///     The Axe Catching Logic
        /// </summary>
        /// <param name="mode">
        ///     The Mode
        /// </param>
        private void CatchAxes(Mode mode)
        {
            var modeName = mode.ToString().ToLowerInvariant();
            if (this.axesList.Any())
            {
                if (!this.Menu.Item("com.iseries.draven." + modeName + ".catch" + modeName).GetValue<bool>())
                {
                    return;
                }
                // Starting Axe Catching Logic
                var closestAxe =
                    this.axesList.FindAll(
                        axe =>
                            axe.IsValid && IsSafe(axe.Position) &&
                            (axe.CanBeReachedNormal || (this.CanCastW() && axe.CanBeReachedWithW && mode == Mode.Combo)) &&
                            (axe.Position.Distance(Game.CursorPos) <=
                             this.Menu.Item("com.iseries.draven.misc.catchrange").GetValue<Slider>().Value))
                        .OrderBy(axe => axe.Position.Distance(Game.CursorPos))
                        .ThenBy(axe => axe.Position.Distance(ObjectManager.Player.ServerPosition))
                        .FirstOrDefault();
                if (closestAxe != null && !closestAxe.IsBeingCaught)
                {
                    if (
                        closestAxe.Position.CountAlliesInRange(
                            this.Menu.Item("com.iseries.draven.misc.safedistance").GetValue<Slider>().Value) + 1 >=
                        closestAxe.Position.CountEnemiesInRange(
                            this.Menu.Item("com.iseries.draven.misc.safedistance").GetValue<Slider>().Value))
                    {
                        if (!closestAxe.CanBeReachedNormal && closestAxe.CanBeReachedWithW)
                        {
                            if (this.CanCastW() && !this.HasWBuff())
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
                            Variables.Orbwalker.SetOrbwalkingPoint(closestAxe.Position.Extend(Game.CursorPos, 45f));
                        }
                    }
                }
                else
                {
                    Variables.Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                }
            }
        }

        /// <summary>
        ///     The Axe Checking List
        /// </summary>
        private void CheckList()
        {
            if (Environment.TickCount - this.lastListCheckTick < 1000)
            {
                return;
            }

            this.lastListCheckTick = Environment.TickCount;
            this.axesList.RemoveAll(axe => !axe.IsValid);
        }

        /// <summary>
        ///     Checks if the player has the speed buff
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        private bool HasWBuff()
        {
            return ObjectManager.Player.HasBuff("dravenfurybuff") || ObjectManager.Player.HasBuff("DravenFury");
        }

        /// <summary>
        ///     Checks if a position is safe
        /// </summary>
        /// <param name="position">
        ///     The Position
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        private bool IsSafe(Vector3 position)
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

        /// <summary>
        ///     On incoming gap closer
        /// </summary>
        /// <param name="gapcloser">
        ///     The Gap closer
        /// </param>
        private void OnIncomingGapcloser(ActiveGapcloser gapcloser)
        {
            if (this.GetItemValue<bool>("com.iseries.draven.misc.eagp") && this.spells[SpellSlot.E].IsReady()
                && gapcloser.Sender.IsValidTarget(450f))
            {
                this.spells[SpellSlot.E].Cast(gapcloser.Sender);
            }
        }

        /// <summary>
        ///     The possible interrupted target
        /// </summary>
        /// <param name="sender">
        ///     The Sender
        /// </param>
        /// <param name="args">
        ///     The args
        /// </param>
        private void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (this.GetItemValue<bool>("com.iseries.draven.misc.eint") && this.spells[SpellSlot.E].IsReady()
                && args.DangerLevel > Interrupter2.DangerLevel.Medium && sender.IsValidTarget(450f))
            {
                this.spells[SpellSlot.E].Cast(sender);
            }
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
                for (var i = 1; i < path.Count(); i++)
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
                for (var i = 1; i < path.Count(); i++)
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
                return AxeObject != null && AxeObject.IsValid && this.EndTime >= Game.Time;
            }
        }

        /// <summary>
        ///     Gets or sets the position.
        /// </summary>
        public Vector3 Position { get; set; }

        #endregion
    }
}