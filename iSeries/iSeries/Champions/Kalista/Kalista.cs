// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Kalista.cs" company="LeagueSharp">
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
namespace iSeries.Champions.Kalista
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using iSeries.Champions.Utilities;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Collision = LeagueSharp.Common.Collision;
    using Color = System.Drawing.Color;

    /// <summary>
    ///     The given champion class
    /// </summary>
    internal class Kalista : Champion
    {

        #region Fields

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
                                                                   {
                                                                       { SpellSlot.Q, new Spell(SpellSlot.Q, 1130) }, 
                                                                       { SpellSlot.W, new Spell(SpellSlot.W, 5200) }, 
                                                                       { SpellSlot.E, new Spell(SpellSlot.E, 950) }, 
                                                                       { SpellSlot.R, new Spell(SpellSlot.R, 1200) }
                                                                   };

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Kalista" /> class.
        /// </summary>
        public Kalista()
        {
            // Menu Generation
            this.CreateMenu = MenuGenerator.Generate;

            // Spell initialization
            this.spells[SpellSlot.Q].SetSkillshot(0.25f, 60f, 1600f, true, SkillshotType.SkillshotLine);
            this.spells[SpellSlot.R].SetSkillshot(0.50f, 1500, float.MaxValue, false, SkillshotType.SkillshotCircle);

            // Useful shit
            Orbwalking.OnNonKillableMinion += minion =>
                {
                    if (!this.GetItemValue<bool>("com.iseries.kalista.misc.lasthit")
                        || !this.spells[SpellSlot.E].IsReady())
                    {
                        return;
                    }

                    if (this.spells[SpellSlot.E].CanCast((Obj_AI_Base)minion)
                        && minion.Health <= this.spells[SpellSlot.E].GetDamage((Obj_AI_Base)minion))
                    {
                        this.spells[SpellSlot.E].Cast();
                    }
                };
            Obj_AI_Base.OnProcessSpellCast += this.OnProcessSpellCast;

            // Damage Indicator
            DamageIndicator.DamageToUnit = this.GetComboDamage;
            DamageIndicator.Enabled = true;

            Console.WriteLine("Kalista Loaded no menu :S");
        }

        /// <summary>
        ///     The on process spell function
        /// </summary>
        /// <param name="sender">
        ///     The Spell Sender
        /// </param>
        /// <param name="args">
        ///     The Arguments
        /// </param>
        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "KalistaExpungeWrapper")
            {
                Orbwalking.ResetAutoAttackTimer();
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the targets health including the shield amount
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The targets health
        /// </returns>
        public float GetActualHealth(Obj_AI_Base target)
        {
            return target.AttackShield > 0
                       ? target.Health + target.AttackShield
                       : target.MagicShield > 0 ? target.Health + target.MagicShield : target.Health;
        }

        /// <summary>
        ///     Gets the Rend Damage
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        public float GetComboDamage(Obj_AI_Base target)
        {
            float damage = 0;

            if (this.spells[SpellSlot.E].IsReady())
            {
                damage += this.spells[SpellSlot.E].GetDamage(target);
            }

            return damage;
        }

        /// <summary>
        ///     <c>OnCombo</c> subscribed orb walker function.
        /// </summary>
        public override void OnCombo()
        {
            if (this.GetItemValue<bool>("com.iseries.kalista.combo.useQ") && this.spells[SpellSlot.Q].IsReady())
            {
                if (this.Player.ManaPercent < this.GetItemValue<Slider>("com.iseries.kalista.combo.qMana").Value)
                {
                    return;
                }

                var spearTarget = TargetSelector.GetTarget(
                    this.spells[SpellSlot.Q].Range, 
                    TargetSelector.DamageType.Physical);
                var prediction = this.spells[SpellSlot.Q].GetPrediction(spearTarget);
                if (prediction.Hitchance >= HitChance.VeryHigh)
                {
                    if (!this.Player.IsDashing() && !this.Player.IsWindingUp)
                    {
                        this.spells[SpellSlot.Q].Cast(prediction.CastPosition);
                    }
                }
            }

            if (this.GetItemValue<bool>("com.iseries.kalista.combo.useE") && this.spells[SpellSlot.E].IsReady())
            {
                var rendTarget =
                    HeroManager.Enemies.Where(
                        x =>
                        x.IsValidTarget(this.spells[SpellSlot.E].Range) && this.spells[SpellSlot.E].GetDamage(x) >= 1
                        && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield))
                        .OrderByDescending(x => this.spells[SpellSlot.E].GetDamage(x))
                        .FirstOrDefault();

                if (rendTarget != null
                    && this.spells[SpellSlot.E].GetDamage(rendTarget) >= this.GetActualHealth(rendTarget)
                    && !rendTarget.IsDead)
                {
                    this.spells[SpellSlot.E].Cast();
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
            if (this.GetItemValue<bool>("com.iseries.kalista.drawing.drawE"))
            {
                Render.Circle.DrawCircle(this.Player.Position, this.spells[SpellSlot.E].Range, Color.DarkRed);
            }
        }

        /// <summary>
        ///     <c>OnHarass</c> subscribed orb walker function.
        /// </summary>
        public override void OnHarass()
        {
            if (this.GetItemValue<bool>("com.iseries.kalista.harass.useQ") && this.spells[SpellSlot.Q].IsReady())
            {
                var spearTarget = TargetSelector.GetTarget(
                    this.spells[SpellSlot.Q].Range, 
                    TargetSelector.DamageType.Physical);
                var prediction = this.spells[SpellSlot.Q].GetPrediction(spearTarget);
                if (prediction.Hitchance >= HitChance.VeryHigh)
                {
                    if (!this.Player.IsDashing() && !this.Player.IsWindingUp)
                    {
                        this.spells[SpellSlot.Q].Cast(prediction.CastPosition);
                    }
                }
            }

            if (this.GetItemValue<bool>("com.iseries.kalista.combo.useE") && this.spells[SpellSlot.E].IsReady())
            {
                var target =
                    HeroManager.Enemies.Where(
                        x =>
                        x.IsValidTarget(this.spells[SpellSlot.E].Range) && this.spells[SpellSlot.E].GetDamage(x) >= 1
                        && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield))
                        .OrderByDescending(x => this.spells[SpellSlot.E].GetDamage(x))
                        .FirstOrDefault();

                if (target != null)
                {
                    var stacks = target.GetBuffCount("kalistaexpungemarker");
                    if (this.spells[SpellSlot.E].GetDamage(target) >= this.GetActualHealth(target)
                        || stacks >= this.GetItemValue<Slider>("com.iseries.kalista.harass.stacks").Value)
                    {
                        this.spells[SpellSlot.E].Cast();
                    }
                }
            }
        }

        /// <summary>
        ///     <c>OnLaneclear</c> subscribed orb walker function.
        /// </summary>
        public override void OnLaneclear()
        {
            if (this.GetItemValue<bool>("com.iseries.kalista.laneclear.useQ") && this.spells[SpellSlot.Q].IsReady())
            {
                var qMinions = MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition, 
                    this.spells[SpellSlot.Q].Range, 
                    MinionTypes.All, 
                    MinionTeam.NotAlly);

                if (qMinions.Count < 1)
                {
                    return;
                }

                foreach (var source in qMinions.Where(x => x.Health < this.spells[SpellSlot.Q].GetDamage(x)))
                {
                    var killable = 0;

                    foreach (var collisionMinion in
                        this.GetCollisionMinions(
                            this.Player, 
                            this.Player.ServerPosition.Extend(source.ServerPosition, this.spells[SpellSlot.Q].Range)))
                    {
                        if (collisionMinion.Health < this.spells[SpellSlot.Q].GetDamage(collisionMinion))
                        {
                            killable++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (killable >= this.GetItemValue<Slider>("com.iseries.kalista.laneclear.useQNum").Value
                        && !this.Player.IsWindingUp && !this.Player.IsDashing())
                    {
                        this.spells[SpellSlot.Q].CastOnUnit(source);
                    }
                }
            }

            if (this.GetItemValue<bool>("com.iseries.kalista.laneclear.useE") && this.spells[SpellSlot.E].IsReady())
            {
                var minionkillcount =
                    MinionManager.GetMinions(this.spells[SpellSlot.E].Range)
                        .Count(
                            x =>
                            this.spells[SpellSlot.E].CanCast(x) && x.Health <= this.spells[SpellSlot.E].GetDamage(x));

                if (minionkillcount >= this.GetItemValue<Slider>("com.iseries.kalista.laneclear.useENum").Value)
                {
                    this.spells[SpellSlot.E].Cast();
                }
            }
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
        ///     TODO The get collision minions.
        /// </summary>
        /// <param name="source">
        ///     TODO The source.
        /// </param>
        /// <param name="targetPosition">
        ///     TODO The target position.
        /// </param>
        /// <returns>
        ///     a list of minions
        /// </returns>
        private IEnumerable<Obj_AI_Base> GetCollisionMinions(Obj_AI_Base source, Vector3 targetPosition)
        {
            var input = new PredictionInput
                            {
                                Unit = source, Radius = this.spells[SpellSlot.Q].Width, 
                                Delay = this.spells[SpellSlot.Q].Delay, Speed = this.spells[SpellSlot.Q].Speed, 
                            };

            input.CollisionObjects[0] = CollisionableObjects.Minions;

            return
                Collision.GetCollision(new List<Vector3> { targetPosition }, input)
                    .OrderBy(obj => obj.Distance(source))
                    .ToList();
        }

        /// <summary>
        ///     Gets the real damage for the spell
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        private float GetRealDamage(Obj_AI_Base target)
        {
            var baseDamage = new[] { 20, 30, 40, 50, 60 };
            var baseMultiplier = new[] { 0.6, 0.6, 0.6, 0.6, 0.6 };

            var baseSpearDamage = new[] { 10, 14, 19, 25, 32 };
            var spearMultiplier = new[] { 0.2, 0.225, 0.25, 0.275, 0.3 };

            var buff =
                target.Buffs.Find(x => x.Caster.IsMe && x.IsValidBuff() && x.DisplayName == "KalistaExpungeMarker");

            if (buff != null)
            {
                var totalDamage = baseDamage[this.spells[SpellSlot.E].Level - 1]
                                  + baseMultiplier[this.spells[SpellSlot.E].Level - 1] * this.Player.TotalAttackDamage()
                                  + (buff.Count - 1)
                                  * (baseSpearDamage[this.spells[SpellSlot.E].Level - 1]
                                     + spearMultiplier[this.spells[SpellSlot.E].Level - 1]
                                     * this.Player.TotalAttackDamage());
                return
                    (float)
                    (100
                     / (100 + (target.Armor * this.Player.PercentArmorPenetrationMod)
                        - this.Player.FlatArmorPenetrationMod) * totalDamage);
            }

            return 0;
        }

        /// <summary>
        ///     The Functions to always process
        /// </summary>
        private void OnUpdateFunctions()
        {
            if (Variables.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo
                && this.GetItemValue<bool>("com.iseries.kalista.misc.autoHarass"))
            {
                var minion =
                    MinionManager.GetMinions(this.spells[SpellSlot.E].Range, MinionTypes.All, MinionTeam.NotAlly)
                        .Where(x => x.Health <= this.spells[SpellSlot.E].GetDamage(x))
                        .OrderBy(x => x.Health)
                        .FirstOrDefault();
                var target =
                    HeroManager.Enemies.Where(
                        x =>
                        this.spells[SpellSlot.E].CanCast(x) && this.spells[SpellSlot.E].GetDamage(x) >= 1
                        && !x.HasBuffOfType(BuffType.SpellShield))
                        .OrderByDescending(x => this.spells[SpellSlot.E].GetDamage(x))
                        .FirstOrDefault();

                if (minion != null && target != null && this.spells[SpellSlot.E].CanCast(minion)
                    && this.spells[SpellSlot.E].CanCast(target) && !ObjectManager.Player.HasBuff("summonerexhaust"))
                {
                    this.spells[SpellSlot.E].Cast();
                }
            }

            foreach (var hero in
                HeroManager.Enemies.Where(
                    x =>
                    this.spells[SpellSlot.E].IsInRange(x)
                    && this.GetActualHealth(x) < this.spells[SpellSlot.E].GetDamage(x)))
            {
                if (hero.HasBuffOfType(BuffType.Invulnerability) || hero.HasBuffOfType(BuffType.SpellImmunity)
                    || hero.HasBuffOfType(BuffType.SpellShield))
                {
                    return;
                }

                this.spells[SpellSlot.E].Cast();
            }

            foreach (var hero in
                HeroManager.Enemies.Where(
                    x =>
                    this.spells[SpellSlot.Q].IsInRange(x)
                    && this.GetActualHealth(x) < this.spells[SpellSlot.Q].GetDamage(x)))
            {
                if (hero.HasBuffOfType(BuffType.Invulnerability) || hero.HasBuffOfType(BuffType.SpellImmunity)
                    || hero.HasBuffOfType(BuffType.SpellShield))
                {
                    return;
                }

                this.spells[SpellSlot.Q].Cast(hero);
            }

            if (this.GetItemValue<bool>("com.iseries.kalista.misc.mobsteal") && this.spells[SpellSlot.E].IsReady())
            {
                var bigMinion =
                    MinionManager.GetMinions(
                        this.Player.ServerPosition, 
                        this.spells[SpellSlot.E].Range, 
                        MinionTypes.All, 
                        MinionTeam.NotAlly, 
                        MinionOrderTypes.MaxHealth)
                        .FirstOrDefault(x => x.IsValid && x.Health < this.spells[SpellSlot.E].GetDamage(x) && !x.Name.Contains("Mini"));
                if (bigMinion != null && this.spells[SpellSlot.E].CanCast(bigMinion))
                {
                    this.spells[SpellSlot.E].Cast();
                }
                
            }
        }

        #endregion
    }
}