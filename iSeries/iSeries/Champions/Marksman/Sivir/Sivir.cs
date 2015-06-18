// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Sivir.cs" company="LeagueSharp">
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
// --------------------------------------------------------------------------------------------------------------------
namespace iSeries.Champions.Marksman.Sivir
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using iSeries.General;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    /// <summary>
    ///     The given champion class
    /// </summary>
    internal class Sivir : Champion
    {
        #region Fields

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
                                                                   {
                                                                       { SpellSlot.Q, new Spell(SpellSlot.Q, 1250f) }, 
                                                                       { SpellSlot.W, new Spell(SpellSlot.W) }, 
                                                                       { SpellSlot.E, new Spell(SpellSlot.E) }, 
                                                                       { SpellSlot.R, new Spell(SpellSlot.R, 1000f) }
                                                                   };

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Sivir" /> class.
        /// </summary>
        public Sivir()
        {
            // Menu Generation
            this.CreateMenu = MenuGenerator.Generate;

            // Spell initialization
            this.spells[SpellSlot.Q].SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);

            // Useful shit
            Orbwalking.AfterAttack += (unit, target) =>
                {
                    if (this.spells[SpellSlot.W].IsReady() && this.GetItemValue<bool>("com.iseries.sivir.combo.useW")
                        && target.IsValidTarget(ObjectManager.Player.AttackRange)
                        && ObjectManager.Player.ManaPercent
                        >= this.GetItemValue<Slider>("com.iseries.sivir.combo.wmana").Value)
                    {
                        this.spells[SpellSlot.W].Cast();
                    }
                };

            Obj_AI_Base.OnProcessSpellCast += this.OnProcessSpellCast;
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
            var qTarget = TargetSelector.GetTarget(this.spells[SpellSlot.Q].Range, TargetSelector.DamageType.Physical);
            if (qTarget.IsValidTarget() && this.spells[SpellSlot.Q].IsReady()
                && this.GetItemValue<bool>("com.iseries.sivir.combo.useQ"))
            {
                this.spells[SpellSlot.Q].CastIfHitchanceEquals(qTarget, HitChance.VeryHigh);
            }
        }

        /// <summary>
        /// <c>OnDraw</c> subscribed event function.
        /// </summary>
        /// <param name="args">
        /// The event data
        /// </param>
        public override void OnDraw(EventArgs args)
        {
            if (this.GetItemValue<bool>("com.iseries.sivir.draw.q"))
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.ServerPosition, 
                    this.spells[SpellSlot.Q].Range, 
                    System.Drawing.Color.Red);
            }
        }

        /// <summary>
        ///     <c>OnHarass</c> subscribed orb walker function.
        /// </summary>
        public override void OnHarass()
        {
            var qTarget = TargetSelector.GetTarget(this.spells[SpellSlot.Q].Range, TargetSelector.DamageType.Physical);
            if (qTarget.IsValidTarget() && this.spells[SpellSlot.Q].IsReady()
                && this.GetItemValue<bool>("com.iseries.sivir.harass.useQ")
                && ObjectManager.Player.ManaPercent > this.GetItemValue<Slider>("com.iseries.sivir.harass.qmana").Value)
            {
                this.spells[SpellSlot.Q].CastIfHitchanceEquals(qTarget, HitChance.VeryHigh);
            }
        }

        /// <summary>
        ///     <c>OnLaneclear</c> subscribed orb walker function.
        /// </summary>
        public override void OnLaneclear()
        {
            if (this.GetItemValue<bool>("com.iseries.sivir.farm.useQ")
                && ObjectManager.Player.ManaPercent < this.GetItemValue<Slider>("com.iseries.sivir.farm.qmana").Value)
            {
                var minions = MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition, 
                    this.spells[SpellSlot.Q].Range);
                var lineFarmLocation = this.spells[SpellSlot.Q].GetLineFarmLocation(minions);
                if (lineFarmLocation.MinionsHit >= 3)
                {
                    this.spells[SpellSlot.Q].Cast(lineFarmLocation.Position);
                }
            }
        }

        /// <summary>
        /// TODO The under ally turret.
        /// </summary>
        /// <param name="position">
        /// TODO The position.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool UnderAllyTurret(Vector3 position)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Any(turret => turret.IsValidTarget(950, false, position) && turret.IsAlly);
        }

        /// <summary>
        /// <c>OnUpdate</c> subscribed event function.
        /// </summary>
        /// <param name="args">
        /// The event data
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
        /// The on process spell function
        /// </summary>
        /// <param name="sender">
        /// The Spell Sender
        /// </param>
        /// <param name="args">
        /// The Arguments
        /// </param>
        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy && args.Target.IsMe && this.GetItemValue<bool>("com.iseries.sivir.misc.eshield"))
            {
                var onlyIfKill = this.GetItemValue<bool>("com.iseries.sivir.misc.eshieldkill");
                var willKill = sender.GetSpellDamage(ObjectManager.Player, args.SData.Name) > ObjectManager.Player.Health + 15;
                if (onlyIfKill && !willKill)
                {
                    return;
                }

                if (willKill)
                {
                    this.spells[SpellSlot.E].Cast();
                }
                else
                {
                    if (!Orbwalking.IsAutoAttack(args.SData.Name))
                    {
                        this.spells[SpellSlot.E].Cast();
                    }
                }
            }
        }

        /// <summary>
        ///     The Functions to always process
        /// </summary>
        private void OnUpdateFunctions()
        {
        }

        #endregion
    }
}