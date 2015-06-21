// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Vayne.cs" company="LeagueSharp">
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
//   The Champion Class
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iSeries.Champions.Marksman.Vayne
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
    ///     The Champion Class
    /// </summary>
    internal class Vayne : Champion
    {
        #region Fields

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
                                                                   {
                                                                       { SpellSlot.Q, new Spell(SpellSlot.Q) }, 
                                                                       { SpellSlot.W, new Spell(SpellSlot.W) }, 
                                                                       { SpellSlot.E, new Spell(SpellSlot.E, 590f) }, 
                                                                       { SpellSlot.R, new Spell(SpellSlot.R) }
                                                                   };

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Vayne" /> class.
        /// </summary>
        public Vayne()
        {
            this.CreateMenu = MenuGenerator.Generate;

            this.spells[SpellSlot.E].SetTargetted(0.25f, 2000f);

            Orbwalking.AfterAttack += this.OrbwalkingAfterAttack;

            AntiGapcloser.OnEnemyGapcloser += gapcloser =>
                {
                    if (this.GetItemValue<bool>("com.iseries.vayne.misc.gapcloser"))
                    {
                        if (gapcloser.Sender.IsValidTarget(this.spells[SpellSlot.E].Range)
                            && gapcloser.End.Distance(ObjectManager.Player.ServerPosition) <= 400f)
                        {
                            this.spells[SpellSlot.E].Cast(gapcloser.Sender);
                        }
                    }
                };

            Interrupter2.OnInterruptableTarget += (sender, args) =>
                {
                    if (this.GetItemValue<bool>("dz191.vhr.misc.general.interrupt"))
                    {
                        if (args.DangerLevel == Interrupter2.DangerLevel.High
                            && sender.IsValidTarget(this.spells[SpellSlot.E].Range))
                        {
                            this.spells[SpellSlot.E].Cast(sender);
                        }
                    }
                };
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the champion type
        /// </summary>
        /// <returns>
        ///     The <see cref="ChampionType" />.
        /// </returns>
        public override ChampionType GetChampionType()
        {
            return ChampionType.Marksman;
        }

        /// <summary>
        ///     <c>OnCombo</c> subscribed orb walker function.
        /// </summary>
        public override void OnCombo()
        {
            if (!this.spells[SpellSlot.E].IsReady() || !this.GetItemValue<bool>("com.iseries.vayne.combo.useE"))
            {
                return;
            }

            foreach (var target in
                HeroManager.Enemies.Where(
                    h =>
                    h.IsValidTarget(this.spells[SpellSlot.E].Range) && !h.HasBuffOfType(BuffType.SpellShield)
                    && !h.HasBuffOfType(BuffType.SpellImmunity)
                    && !this.GetItemValue<bool>("com.iseries.vayne.noe." + h.ChampionName.ToLowerInvariant())))
            {
                const int PushDistance = 400;
                var targetPosition = target.ServerPosition;
                var endPosition = targetPosition.Extend(ObjectManager.Player.ServerPosition, -PushDistance);
                for (var i = 0; i < PushDistance; i += (int)target.BoundingRadius)
                {
                    var extendedPosition = targetPosition.Extend(ObjectManager.Player.ServerPosition, -i);
                    if (extendedPosition.IsWall() || endPosition.IsWall())
                    {
                        this.spells[SpellSlot.E].Cast(target);
                        break;
                    }
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
            if (this.GetItemValue<bool>("com.iseries.twitch.vayne.drawE"))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, this.spells[SpellSlot.E].Range, Color.Red);
            }
        }

        /// <summary>
        ///     <c>OnHarass</c> subscribed orb walker function.
        /// </summary>
        public override void OnHarass()
        {
            if (!this.spells[SpellSlot.E].IsReady() || !this.GetItemValue<bool>("com.iseries.vayne.harass.useE"))
            {
                return;
            }

            foreach (var target in
                HeroManager.Enemies.Where(
                    h =>
                    h.IsValidTarget(this.spells[SpellSlot.E].Range) && !h.HasBuffOfType(BuffType.SpellShield)
                    && !h.HasBuffOfType(BuffType.SpellImmunity)
                    && !this.GetItemValue<bool>("com.iseries.vayne.noe." + h.ChampionName.ToLowerInvariant())))
            {
                var pushDistance = 400;
                var targetPosition = target.ServerPosition;
                var endPosition = targetPosition.Extend(ObjectManager.Player.ServerPosition, -pushDistance);
                for (var i = 0; i < pushDistance; i += (int)target.BoundingRadius)
                {
                    var extendedPosition = targetPosition.Extend(ObjectManager.Player.ServerPosition, -i);
                    if (extendedPosition.IsWall() || endPosition.IsWall())
                    {
                        this.spells[SpellSlot.E].Cast(target);
                        break;
                    }
                }
            }
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
            }
        }

        /// <summary>
        ///     The After
        /// </summary>
        /// <param name="unit">
        ///     The Unit
        /// </param>
        /// <param name="target">
        ///     The Target
        /// </param>
        public void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!(target is Obj_AI_Base) || !unit.IsMe)
            {
                return;
            }

            Console.WriteLine("After Attack Called??");

            switch (Variables.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (this.GetItemValue<bool>("com.iseries.vayne.combo.useQ"))
                    {
                        this.CastTumble((Obj_AI_Hero)target);
                    }

                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (this.GetItemValue<bool>("com.iseries.vayne.harass.useQ"))
                    {
                        this.CastTumble((Obj_AI_Hero)target);
                        Utility.DelayAction.Add((int)(Game.Ping / 2f + 250 + 325), Orbwalking.ResetAutoAttackTimer);
                    }

                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (this.spells[SpellSlot.Q].IsReady()
                        && this.GetItemValue<bool>("com.iseries.vayne.laneclear.useQ"))
                    {
                        var minionsInRange =
                            MinionManager.GetMinions(ObjectManager.Player.ServerPosition, this.Player.AttackRange)
                                .FindAll(
                                    m =>
                                    m.Health
                                    <= this.Player.GetAutoAttackDamage(m) + this.spells[SpellSlot.Q].GetDamage(m))
                                .ToList();
                        if (minionsInRange.Any())
                        {
                            if (minionsInRange.Count > 1)
                            {
                                var firstMinion = minionsInRange.OrderBy(m => m.HealthPercent).First();
                                var endPosition = ObjectManager.Player.ServerPosition.Extend(
                                    firstMinion.ServerPosition, 
                                    this.spells[SpellSlot.Q].Range);
                                if (PositionHelper.IsSafePosition(endPosition))
                                {
                                    this.spells[SpellSlot.Q].Cast(firstMinion.ServerPosition);
                                    Variables.Orbwalker.ForceTarget(firstMinion);
                                }
                            }
                        }
                    }

                    break;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     TODO The cast tumble.
        /// </summary>
        /// <param name="target">
        ///     TODO The target.
        /// </param>
        private void CastTumble(Obj_AI_Base target)
        {
            if (!this.spells[SpellSlot.Q].IsReady())
            {
                return;
            }

            var positionAfter = this.Player.ServerPosition.To2D().Extend(Game.CursorPos.To2D(), 300f).To3D();
            var distanceAfterTumble = Vector3.DistanceSquared(positionAfter, target.ServerPosition);

            if (distanceAfterTumble <= 550 * 550 && distanceAfterTumble >= 100 * 100
                && PositionHelper.IsSafePosition(positionAfter))
            {
                this.spells[SpellSlot.Q].Cast(Game.CursorPos);
            }
        }

        #endregion
    }
}