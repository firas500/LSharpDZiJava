// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ezreal.cs" company="LeagueSharp">
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
namespace iSeries.Champions.Ezreal
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
    internal class Ezreal : Champion
    {
        #region Fields

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
                                                                   {
                                                                       { SpellSlot.Q, new Spell(SpellSlot.Q, 1190) }, 
                                                                       { SpellSlot.W, new Spell(SpellSlot.W, 800) }, 
                                                                       { SpellSlot.R, new Spell(SpellSlot.R, 2500) }
                                                                   };

        private float _lastCheckTick;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Ezreal" /> class.
        /// </summary>
        public Ezreal()
        {
            // Menu Generation
            this.CreateMenu = MenuGenerator.Generate;

            // Damage Indicator
            DamageIndicator.DamageToUnit = this.GetDamage;
            DamageIndicator.Enabled = true;

            // Spell initialization
            this.spells[SpellSlot.Q].SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);
            this.spells[SpellSlot.W].SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);
            this.spells[SpellSlot.R].SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            // Useful shit
            Orbwalking.OnNonKillableMinion += minion =>
                {
                    if (!this.GetItemValue<bool>("com.iseries.ezreal.laneclear.useQKill")
                        || !this.spells[SpellSlot.Q].IsReady()
                        || Variables.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
                    {
                        return;
                    }

                    var minionHealth = HealthPrediction.GetHealthPrediction(
                        (Obj_AI_Base)minion,
                        (int)
                        (this.spells[SpellSlot.Q].Delay
                         + (this.Player.Distance(minion) / this.spells[SpellSlot.Q].Speed) * 1000f + Game.Ping / 2f));

                    if (this.spells[SpellSlot.E].CanCast((Obj_AI_Base)minion)
                        && minionHealth <= this.spells[SpellSlot.E].GetDamage((Obj_AI_Base)minion))
                    {
                        this.spells[SpellSlot.E].Cast();
                    }

                    var pred = this.spells[SpellSlot.Q].GetPrediction((Obj_AI_Base)minion);
                    if (pred.Hitchance >= HitChance.Medium
                        && this.spells[SpellSlot.Q].GetDamage((Obj_AI_Base)minion) > minionHealth)
                    {
                        this.spells[SpellSlot.Q].Cast((Obj_AI_Base)minion);
                    }
                };
            Orbwalking.OnAttack += (unit, target) =>
            {
                if (!unit.IsMe || !(target is Obj_AI_Hero))
                {
                    return;
                }

                if (this.GetItemValue<bool>("com.iseries.ezreal.misc.muramana") && Items.HasItem(3042) &&
                    Items.CanUseItem(3042) && !ObjectManager.Player.HasBuff("Muramana"))
                {
                    Items.UseItem(3042);
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
        ///     Gets the currently selected hit chance
        /// </summary>
        /// <returns>
        ///     The <see cref="HitChance" />.
        /// </returns>
        public HitChance GetHitchance()
        {
            switch (this.GetItemValue<StringList>("com.iseries.ezreal.misc.hitchance").SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        /// <summary>
        ///     Sheen checking
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool HasSheen()
        {
            return this.GetItemValue<bool>("com.iseries.ezreal.misc.sheen") && this.Player.HasBuff("sheen");
        }

        /// <summary>
        ///     <c>OnCombo</c> subscribed orb walker function.
        /// </summary>
        public override void OnCombo()
        {
            if (this.GetItemValue<bool>("com.iseries.ezreal.combo.useQ") && this.spells[SpellSlot.Q].IsReady())
            {
                var target = TargetSelector.GetTargetNoCollision(this.spells[SpellSlot.Q]);
                var prediction = this.spells[SpellSlot.Q].GetPrediction(target);
                if (prediction.Hitchance >= this.GetHitchance() && target.IsValidTarget(this.spells[SpellSlot.Q].Range))
                {
                    if (!this.HasSheen() || target.Health + 15 < spells[SpellSlot.Q].GetDamage(target))
                    {
                        this.spells[SpellSlot.Q].Cast(target);
                    }
                }
            }

            if (this.GetItemValue<bool>("com.iseries.ezreal.combo.useW") && this.spells[SpellSlot.W].IsReady())
            {
                var target = TargetSelector.GetTarget(this.spells[SpellSlot.W].Range, TargetSelector.DamageType.Magical);
                if (target.IsValidTarget(this.spells[SpellSlot.W].Range) && !this.HasSheen())
                {
                    if (!this.HasSheen() || target.Health + 15 < spells[SpellSlot.W].GetDamage(target))
                    {
                        this.spells[SpellSlot.W].Cast(target);
                    }
                }
            }

            if (this.GetItemValue<bool>("com.iseries.ezreal.combo.useR") && this.spells[SpellSlot.R].IsReady())
            {
                foreach (var prediction in
                    HeroManager.Enemies.Where(
                        x =>
                        x.IsValidTarget(this.spells[SpellSlot.R].Range)
                        && this.Player.Distance(x) > this.spells[SpellSlot.Q].Range && !x.IsZombie)
                        .Where(this.CanExecuteTarget)
                        .Select(hero => this.spells[SpellSlot.R].GetPrediction(hero))
                        .Where(prediction => prediction.Hitchance >= HitChance.Medium))
                {
                    this.spells[SpellSlot.R].Cast(prediction.CastPosition);
                }
            }

            if (this.GetItemValue<bool>("com.iseries.ezreal.misc.peel") && this.spells[SpellSlot.E].IsReady()
                && this.Player.HealthPercent < 30)
            {
                var meleeEnemies = this.Player.GetEnemiesInRange(400f).FindAll(m => m.IsMelee());
                if (!meleeEnemies.Any())
                {
                    return;
                }

                var mostDangerous = meleeEnemies.OrderByDescending(m => m.GetAutoAttackDamage(this.Player)).First();
                if (mostDangerous == null)
                {
                    return;
                }

                var position = this.Player.Position.To2D()
                    .Extend((mostDangerous.Position - this.Player.Position).To2D(), 425);

                if (position.To3D().UnderTurret(true) || position.To3D().IsWall())
                {
                    return;
                }

                this.spells[SpellSlot.E].Cast(position);
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
            if (this.GetItemValue<bool>("com.iseries.ezreal.draw.q"))
            {
                Render.Circle.DrawCircle(this.Player.Position, this.spells[SpellSlot.Q].Range, Color.DarkRed);
            }
        }

        /// <summary>
        ///     <c>OnHarass</c> subscribed orb walker function.
        /// </summary>
        public override void OnHarass()
        {
            if (this.GetItemValue<bool>("com.iseries.ezreal.harass.useQ") && this.spells[SpellSlot.Q].IsReady())
            {
                var target = TargetSelector.GetTargetNoCollision(this.spells[SpellSlot.Q]);
                var prediction = this.spells[SpellSlot.Q].GetPrediction(target);
                if (prediction.Hitchance >= this.GetHitchance() && target.IsValidTarget(this.spells[SpellSlot.Q].Range))
                {
                    this.spells[SpellSlot.Q].Cast(prediction.CastPosition);
                }
            }

            if (this.GetItemValue<bool>("com.iseries.ezreal.harass.useW") && this.spells[SpellSlot.W].IsReady())
            {
                var target = TargetSelector.GetTarget(this.spells[SpellSlot.W].Range, TargetSelector.DamageType.Magical);
                if (target.IsValidTarget(this.spells[SpellSlot.W].Range))
                {
                    this.spells[SpellSlot.W].Cast(target);
                }
            }
        }

        /// <summary>
        ///     <c>OnLaneclear</c> subscribed orb walker function.
        /// </summary>
        public override void OnLaneclear()
        {
            // idek...
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
        ///     Checks if the spell can execute the given target
        /// </summary>
        /// <param name="target">
        ///     The target
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        private bool CanExecuteTarget(Obj_AI_Hero target)
        {
            double damage = 1f;

            var prediction = this.spells[SpellSlot.R].GetPrediction(target);
            var count = prediction.CollisionObjects.Count;

            damage += this.Player.GetSpellDamage(target, SpellSlot.R);

            if (count >= 7)
            {
                damage = damage * .3;
            }
            else if (count != 0)
            {
                damage = damage * (10 - count / 10);
            }

            return damage > target.Health + 5;
        }

        /// <summary>
        ///     Gets the damage output
        /// </summary>
        /// <param name="target">
        ///     the given target
        /// </param>
        /// <returns>
        ///     The total damage
        /// </returns>
        private float GetDamage(Obj_AI_Base target)
        {
            return (float)(this.spells[SpellSlot.Q].GetDamage(target) + this.Player.GetAutoAttackDamage(target, true));
        }

        /// <summary>
        ///     The Code to always check and execute
        /// </summary>
        private void OnUpdateFunctions()
        {
            foreach (var hero in
                HeroManager.Enemies.Where(
                    x =>
                    x.IsValidTarget(this.spells[SpellSlot.Q].Range) && this.spells[SpellSlot.Q].GetDamage(x) > x.Health)
                    .Where(hero => this.spells[SpellSlot.Q].IsReady()))
            {
                this.spells[SpellSlot.Q].Cast(hero);
            }

            if (Environment.TickCount - this._lastCheckTick < 3000)
            {
                return;
            }
            this._lastCheckTick = Environment.TickCount;

            if (this.GetItemValue<bool>("com.iseries.ezreal.misc.muramana") && Items.HasItem(3042) && Items.CanUseItem(3042) && ObjectManager.Player.HasBuff("Muramana"))
            {
                Items.UseItem(3042);
            }
        }

        #endregion
    }
}