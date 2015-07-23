// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tristana.cs" company="LeagueSharp">
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
namespace iSeries.Champions.Marksman.Tristana
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using iSeries.Champions.Utilities;
    using iSeries.General;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     The Champion Class
    /// </summary>
    internal class Tristana : Champion
    {
        #region Fields

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
                                                                   {
                                                                       { SpellSlot.Q, new Spell(SpellSlot.Q) }, 
                                                                       { SpellSlot.W, new Spell(SpellSlot.W, 900f) }, 
                                                                       { SpellSlot.E, new Spell(SpellSlot.E, 630f) }, 
                                                                       { SpellSlot.R, new Spell(SpellSlot.R, 630f) }
                                                                   };

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Tristana" /> class.
        /// </summary>
        public Tristana()
        {
            this.CreateMenu = MenuGenerator.Generate;

            DamageIndicator.DamageToUnit = this.GetComboDamage;
            DamageIndicator.Enabled = true;

            Orbwalking.AfterAttack += this.OnAfterAttack;

            AntiGapcloser.OnEnemyGapcloser += gapcloser =>
                {
                    if (!this.spells[SpellSlot.R].IsReady()
                        || !gapcloser.Sender.IsValidTarget(this.spells[SpellSlot.R].Range)
                        || !this.GetItemValue<bool>("com.iseries.tristana.misc.gapcloser"))
                    {
                        return;
                    }

                    this.spells[SpellSlot.R].CastOnUnit(gapcloser.Sender);
                };

            Interrupter2.OnInterruptableTarget += (sender, args) =>
                {
                    if (sender.IsValidTarget(this.spells[SpellSlot.R].Range) && this.spells[SpellSlot.R].IsReady()
                        && args.DangerLevel > Interrupter2.DangerLevel.Medium
                        && this.GetItemValue<bool>("com.iseries.tristana.misc.interrupter"))
                    {
                        this.spells[SpellSlot.R].CastOnUnit(sender);
                    }
                };
            Obj_AI_Base.OnProcessSpellCast += this.OnSpellCast;
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
            /*if (this.GetItemValue<bool>("com.iseries.tristana.combo.useE") && this.spells[SpellSlot.E].IsReady())
            {
                var target = TargetSelector.GetTarget(
                    this.spells[SpellSlot.E].Range,
                    TargetSelector.DamageType.Physical);

                if (target.IsValidTarget(this.spells[SpellSlot.E].Range))
                {
                    this.spells[SpellSlot.E].CastOnUnit(target);
                }
            }*/
            if (this.GetItemValue<bool>("com.iseries.tristana.combo.useR") && this.spells[SpellSlot.R].IsReady())
            {
                var target =
                    HeroManager.Enemies.FirstOrDefault(
                        x =>
                        x.Health + 10 <= this.spells[SpellSlot.R].GetDamage(x)
                        && x.IsValidTarget(this.spells[SpellSlot.R].Range) && !x.HasBuffOfType(BuffType.Invulnerability)
                        && !x.HasBuffOfType(BuffType.SpellShield));

                if (target != null && !target.HasBuffOfType(BuffType.SpellShield))
                {
                    this.spells[SpellSlot.R].CastOnUnit(target);
                }
            }

            if (this.GetItemValue<bool>("com.iseries.tristana.misc.useRE") && this.spells[SpellSlot.R].IsReady())
            {
                var target = TargetSelector.GetTarget(
                    this.spells[SpellSlot.R].Range, 
                    TargetSelector.DamageType.Physical);
                var stacks = target.GetBuffCount("tristanaecharge");

                var totalDamage = this.spells[SpellSlot.E].GetDamage(target) * (0.30 * stacks)
                                  + this.spells[SpellSlot.R].GetDamage(target);

                if (target.IsValidTarget(this.spells[SpellSlot.R].Range) && totalDamage > target.Health + 10 && !target.HasBuffOfType(BuffType.SpellShield))
                {
                    this.spells[SpellSlot.R].CastOnUnit(target);
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
            if (this.GetItemValue<bool>("com.iseries.tristana.drawing.drawE"))
            {
                Render.Circle.DrawCircle(this.Player.Position, this.spells[SpellSlot.E].Range, Color.DarkRed);
            }

            if (this.GetItemValue<bool>("com.iseries.tristana.drawing.drawW"))
            {
                Render.Circle.DrawCircle(this.Player.Position, this.spells[SpellSlot.W].Range, Color.DarkRed);
            }
        }

        /// <summary>
        ///     <c>OnHarass</c> subscribed orb walker function.
        /// </summary>
        public override void OnHarass()
        {
            if (this.GetItemValue<bool>("com.iseries.tristana.harass.useE") && this.spells[SpellSlot.E].IsReady())
            {
                var target = TargetSelector.GetTarget(
                    this.spells[SpellSlot.E].Range, 
                    TargetSelector.DamageType.Physical);

                if (target.IsValidTarget(this.spells[SpellSlot.E].Range) && !target.HasBuffOfType(BuffType.SpellShield))
                {
                    this.spells[SpellSlot.E].CastOnUnit(target);
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

                case Orbwalking.OrbwalkingMode.LaneClear:
                    this.OnLaneclear();
                    break;
            }

            this.OnUpdateFunctions();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the total damage
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        private float GetComboDamage(Obj_AI_Hero target)
        {
            return 0;
        }

        /// <summary>
        ///     The After Attack Event
        /// </summary>
        /// <param name="unit">
        ///     The Unit
        /// </param>
        /// <param name="target">
        ///     The Target
        /// </param>
        private void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var hero = target as Obj_AI_Hero;
            if (hero == null || !unit.IsMe)
            {
                return;
            }

            if (this.spells[SpellSlot.Q].IsReady() && Variables.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo
                && this.GetItemValue<bool>("com.iseries.tristana.combo.useQ") && hero.IsValidTarget(1000f))
            {
                this.spells[SpellSlot.Q].Cast();
            }

            if (this.GetItemValue<bool>("com.iseries.tristana.combo.useE") && this.spells[SpellSlot.E].IsReady()
                && Variables.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (hero.IsValidTarget(this.spells[SpellSlot.E].Range) && !hero.HasBuffOfType(BuffType.SpellShield))
                {
                    this.spells[SpellSlot.E].CastOnUnit(hero);
                }
            }
        }

        /// <summary>
        ///     The spell casting shit
        /// </summary>
        /// <param name="sender1">
        ///     The sender 1.
        /// </param>
        /// <param name="args">
        ///     The Args
        /// </param>
        private void OnSpellCast(Obj_AI_Base sender1, GameObjectProcessSpellCastEventArgs args)
        {
            var sender = sender1 as Obj_AI_Hero;
            if (sender != null
                && (sender.IsEnemy && args.SData.Name.ToLower() == "summonerflash"
                    && args.End.Distance(ObjectManager.Player.Position, true) <= this.spells[SpellSlot.R].Range
                    && this.spells[SpellSlot.R].IsReady() && this.GetItemValue<bool>("com.iseries.tristana.misc.kickFlash")))
            {
                this.spells[SpellSlot.R].CastOnUnit(sender);
            }
        }

        /// <summary>
        ///     The Functions to always process
        /// </summary>
        private void OnUpdateFunctions()
        {
            foreach (var source in HeroManager.Enemies.Where(x => this.spells[SpellSlot.R].IsKillable(x) && this.spells[SpellSlot.R].IsInRange(x) && !x.HasBuffOfType(BuffType.SpellShield)).Where(source => this.spells[SpellSlot.R].IsReady()))
            {
                this.spells[SpellSlot.R].CastOnUnit(source);
            }
        }

        #endregion
    }
}