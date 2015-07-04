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
namespace iSeries.Champions.Marksman.Kalista
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
    internal class Kalista : Champion
    {
        #region Fields

        /// <summary>
        ///     Gets the incoming damage
        /// </summary>
        private readonly Dictionary<float, float> incomingDamage = new Dictionary<float, float>();

        /// <summary>
        ///     Gets the instant damage
        /// </summary>
        private readonly Dictionary<float, float> instantDamage = new Dictionary<float, float>();

        /// <summary>
        ///     The dictionary to call the Spell Slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
                                                                   {
                                                                       { SpellSlot.Q, new Spell(SpellSlot.Q, 1150) }, 
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
            this.spells[SpellSlot.Q].SetSkillshot(0.25f, 40f, 1200f, true, SkillshotType.SkillshotLine);
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
                        if (Environment.TickCount - this.spells[SpellSlot.E].LastCastAttemptT < 500)
                        {
                            return;
                        }

                        this.spells[SpellSlot.E].Cast();
                        this.spells[SpellSlot.E].LastCastAttemptT = Environment.TickCount;
                    }
                };
            Spellbook.OnCastSpell += (sender, args) =>
                {
                    if (sender.Owner.IsMe && args.Slot == SpellSlot.Q && ObjectManager.Player.IsDashing())
                    {
                        args.Process = false;
                    }
                };
            Obj_AI_Base.OnProcessSpellCast += this.OnProcessSpellCast;

            // Damage Indicator
            DamageIndicator.DamageToUnit = this.GetActualDamage;
            DamageIndicator.Enabled = true;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the total incoming damage sum
        /// </summary>
        private float IncomingDamage
        {
            get
            {
                return this.incomingDamage.Sum(e => e.Value) + this.instantDamage.Sum(e => e.Value);
            }
        }

        /// <summary>
        ///     Gets or sets the Soul bound hero
        /// </summary>
        private Obj_AI_Hero SoulBound { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Checks if the given position is under our turret
        /// </summary>
        /// <param name="position">
        ///     The Position
        /// </param>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        public static bool UnderAllyTurret(Vector3 position)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Any(turret => turret.IsValidTarget(950, false, position) && turret.IsAlly);
        }

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
            var result = target.Health;

            if (target.AttackShield > 0)
            {
                result += target.AttackShield;
            }

            return result;
        }

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
                damage += this.GetActualDamage(target);
            }

            return damage;
        }

        /// <summary>
        ///     Checks if a target has an immortal buff
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool HasUndyingBuff(Obj_AI_Hero target)
        {
            // Tryndamere R
            if (target.ChampionName == "Tryndamere"
                && target.Buffs.Any(
                    b => b.Caster.NetworkId == target.NetworkId && b.IsValidBuff() && b.DisplayName == "Undying Rage"))
            {
                return true;
            }

            // Zilean R
            if (target.Buffs.Any(b => b.IsValidBuff() && b.DisplayName == "Chrono Shift"))
            {
                return true;
            }

            // Kayle R
            if (target.Buffs.Any(b => b.IsValidBuff() && b.DisplayName == "JudicatorIntervention"))
            {
                return true;
            }

            // Poppy R
            if (target.ChampionName == "Poppy")
            {
                if (
                    HeroManager.Allies.Any(
                        o =>
                        !o.IsMe
                        && o.Buffs.Any(
                            b =>
                            b.Caster.NetworkId == target.NetworkId && b.IsValidBuff()
                            && b.DisplayName == "PoppyDITarget")))
                {
                    return true;
                }
            }

            return false;
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

                var dashPosition = this.Player.GetDashInfo().EndPos.To3D();

                if (dashPosition != Vector3.Zero)
                {
                    this.spells[SpellSlot.Q].UpdateSourcePosition(dashPosition);
                }

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
                        x.IsValidTarget(this.spells[SpellSlot.E].Range) && this.spells[SpellSlot.E].GetDamage(x) > 1
                        && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield))
                        .OrderByDescending(x => this.spells[SpellSlot.E].GetDamage(x))
                        .FirstOrDefault();

                if (rendTarget != null && this.GetActualDamage(rendTarget) >= this.GetActualHealth(rendTarget)
                    && !rendTarget.IsDead && Environment.TickCount - this.spells[SpellSlot.E].LastCastAttemptT > 500
                    && !this.HasUndyingBuff(rendTarget))
                {
                    this.spells[SpellSlot.E].Cast();
                    this.spells[SpellSlot.E].LastCastAttemptT = Environment.TickCount;
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

            foreach (var source in HeroManager.Enemies.Where(x => this.Player.Distance(x) <= 2000f && !x.IsDead))
            {
                var stacks = source.GetBuffCount("kalistaexpungemarker");

                if (stacks > 0)
                {
                    if (this.GetItemValue<bool>("com.iseries.kalista.drawing.drawStacks"))
                    {
                        Drawing.DrawText(
                            Drawing.WorldToScreen(source.Position)[0] - 80, 
                            Drawing.WorldToScreen(source.Position)[1], 
                            Color.White, 
                            "Stacks: " + stacks);
                    }
                }

                if (this.GetItemValue<bool>("com.iseries.kalista.drawing.drawPercentage"))
                {
                    var currentPercentage =
                        Math.Ceiling(this.GetActualDamage(source) * 100 / this.GetActualHealth(source));

                    Drawing.DrawText(
                        Drawing.WorldToScreen(source.Position)[0], 
                        Drawing.WorldToScreen(source.Position)[1], 
                        currentPercentage >= 100 ? Color.DarkRed : Color.White, 
                        currentPercentage >= 100 ? "Killable With E" : "Current Damage: " + currentPercentage + "%");
                }
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
                    if (this.GetActualDamage(target) >= this.GetActualHealth(target)
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
                var qMinions = MinionManager.GetMinions(this.Player.ServerPosition, this.spells[SpellSlot.Q].Range);

                if (qMinions.Count <= 0)
                {
                    return;
                }

                foreach (var source in qMinions.Where(x => x.Health <= this.spells[SpellSlot.Q].GetDamage(x)))
                {
                    var killable = 0;

                    foreach (var collisionMinion in
                        this.spells[SpellSlot.Q].GetCollision(
                            ObjectManager.Player.ServerPosition.To2D(), 
                            new List<Vector2> { source.ServerPosition.To2D() }, 
                            this.spells[SpellSlot.Q].Range))
                    {
                        if (collisionMinion.Health <= this.spells[SpellSlot.Q].GetDamage(collisionMinion))
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
                        this.spells[SpellSlot.Q].Cast(source.ServerPosition);
                        break;
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

                var minionkillcountTurret =
                    MinionManager.GetMinions(this.spells[SpellSlot.E].Range)
                        .Count(
                            x =>
                            this.spells[SpellSlot.E].CanCast(x) && x.Health <= this.spells[SpellSlot.E].GetDamage(x)
                            && UnderAllyTurret(x.ServerPosition));

                if ((minionkillcount >= this.GetItemValue<Slider>("com.iseries.kalista.laneclear.useENum").Value)
                    || (this.GetItemValue<bool>("com.iseries.kalista.laneclear.esingle") && minionkillcountTurret > 0))
                {
                    if (Environment.TickCount - this.spells[SpellSlot.E].LastCastAttemptT < 500)
                    {
                        return;
                    }

                    this.spells[SpellSlot.E].Cast();
                    this.spells[SpellSlot.E].LastCastAttemptT = Environment.TickCount;
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
        ///     Gets actual damage blah blah
        /// </summary>
        /// <param name="target">
        ///     The target
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        private float GetActualDamage(Obj_AI_Base target)
        {
            if (target.HasBuff("FerociousHowl"))
            {
                return (float)(this.spells[SpellSlot.E].GetDamage(target) * 0.7);
            }

            if (this.Player.HasBuff("summonerexhaust"))
            {
                return (float)(this.spells[SpellSlot.E].GetDamage(target) * 0.4);
            }

            return this.spells[SpellSlot.E].GetDamage(target);
        }

        /// <summary>
        ///     Gets the damage to baron
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        private float GetBaronReduction(Obj_AI_Base target)
        {
            // Buff Name: barontarget or barondebuff
            // Baron's Gaze: Baron Nashor takes 50% reduced damage from champions he's damaged in the last 15 seconds. 
            return this.Player.HasBuff("barontarget")
                       ? this.spells[SpellSlot.E].GetDamage(target) * 0.5f
                       : this.spells[SpellSlot.E].GetDamage(target);
        }

        /// <summary>
        ///     Gets the damage to drake
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        private float GetDragonReduction(Obj_AI_Base target)
        {
            // DragonSlayer: Reduces damage dealt by 7% per a stack
            return this.Player.HasBuff("s5test_dragonslayerbuff")
                       ? this.spells[SpellSlot.E].GetDamage(target)
                         * (1 - (.07f * this.Player.GetBuffCount("s5test_dragonslayerbuff")))
                       : this.spells[SpellSlot.E].GetDamage(target);
        }

        /// <summary>
        ///     Handles the Sentinel trick
        /// </summary>
        private void HandleSentinels()
        {
            if (!this.spells[SpellSlot.W].IsReady())
            {
                return;
            }

            if (this.GetItemValue<KeyBind>("com.iseries.kalista.misc.baronBug").Active
                && ObjectManager.Player.Distance(SummonersRift.River.Baron) <= this.spells[SpellSlot.W].Range)
            {
                this.spells[SpellSlot.W].Cast(SummonersRift.River.Baron);
            }
            else if (this.GetItemValue<KeyBind>("com.iseries.kalista.misc.dragonBug").Active
                     && ObjectManager.Player.Distance(SummonersRift.River.Dragon) <= this.spells[SpellSlot.W].Range)
            {
                this.spells[SpellSlot.W].Cast(SummonersRift.River.Dragon);
            }
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
        [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1404:CodeAnalysisSuppressionMustHaveJustification", 
            Justification = "Reviewed. Suppression is OK here.")]
        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "KalistaExpungeWrapper")
            {
                Utility.DelayAction.Add(0x7D, Orbwalking.ResetAutoAttackTimer);
            }

            if (sender.IsEnemy)
            {
                if (this.SoulBound == null || !this.GetItemValue<bool>("com.iseries.kalista.misc.saveAlly"))
                {
                    return;
                }

                if ((!(sender is Obj_AI_Hero) || args.SData.IsAutoAttack()) && args.Target != null
                    && args.Target.NetworkId == this.SoulBound.NetworkId)
                {
                    this.incomingDamage.Add(
                        this.SoulBound.ServerPosition.Distance(sender.ServerPosition) / args.SData.MissileSpeed
                        + Game.Time, 
                        (float)sender.GetAutoAttackDamage(this.SoulBound));
                }
                else
                {
                    var hero = sender as Obj_AI_Hero;
                    if (hero == null)
                    {
                        return;
                    }

                    var attacker = hero;
                    var slot = attacker.GetSpellSlot(args.SData.Name);

                    if (slot == SpellSlot.Unknown)
                    {
                        return;
                    }

                    if (slot == attacker.GetSpellSlot("SummonerDot") && args.Target != null
                        && args.Target.NetworkId == this.SoulBound.NetworkId)
                    {
                        this.instantDamage.Add(
                            Game.Time + 2, 
                            (float)attacker.GetSummonerSpellDamage(this.SoulBound, Damage.SummonerSpell.Ignite));
                    }
                    else if (slot.HasFlag(SpellSlot.Q | SpellSlot.W | SpellSlot.E | SpellSlot.R)
                             && ((args.Target != null && args.Target.NetworkId == this.SoulBound.NetworkId)
                                 || args.End.Distance(this.SoulBound.ServerPosition, true)
                                 < Math.Pow(args.SData.LineWidth, 2)))
                    {
                        this.instantDamage.Add(
                            Game.Time + 2, 
                            (float)attacker.GetSpellDamage(this.SoulBound, slot));
                    }
                }
            }
        }

        /// <summary>
        ///     The Functions to always process
        /// </summary>
        private void OnUpdateFunctions()
        {
            if (this.SoulBound == null)
            {
                this.SoulBound =
                    HeroManager.Allies.Find(
                        h => h.Buffs.Any(b => b.Caster.IsMe && b.Name.Contains("kalistacoopstrikeally")));
            }
            else if (this.GetItemValue<bool>("com.iseries.kalista.misc.saveAlly") && this.spells[SpellSlot.R].IsReady())
            {
                if (this.SoulBound.HealthPercent < 5
                    && (this.SoulBound.CountEnemiesInRange(500) > 0 || this.IncomingDamage > this.SoulBound.Health))
                {
                    this.spells[SpellSlot.R].Cast();
                }
            }

            var itemsToRemove = this.incomingDamage.Where(entry => entry.Key < Game.Time).ToArray();
            foreach (var item in itemsToRemove)
            {
                this.incomingDamage.Remove(item.Key);
            }

            itemsToRemove = this.instantDamage.Where(entry => entry.Key < Game.Time).ToArray();
            foreach (var item in itemsToRemove)
            {
                this.instantDamage.Remove(item.Key);
            }

            if (Variables.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo
                && this.GetItemValue<bool>("com.iseries.kalista.misc.autoHarass"))
            {
                var target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsEnemy && x.HasBuff("kalistaexpungewrapper"))
                        .MinOrDefault(x => x.Distance(ObjectManager.Player));
                var killableMinion =
                    ObjectManager.Get<Obj_AI_Base>()
                        .Any(
                            x =>
                            x.IsEnemy && this.spells[SpellSlot.E].IsInRange(x)
                            && this.spells[SpellSlot.E].GetDamage(x) > x.Health);
                if (target != null
                    && target.Distance(ObjectManager.Player) < Math.Pow(this.spells[SpellSlot.E].Range + 200, 2)
                    && killableMinion)
                {
                    if (Environment.TickCount - this.spells[SpellSlot.E].LastCastAttemptT < 500)
                    {
                        return;
                    }

                    this.spells[SpellSlot.E].Cast();
                    this.spells[SpellSlot.E].LastCastAttemptT = Environment.TickCount;
                }
            }

            foreach (var hero in
                HeroManager.Enemies.Where(
                    x =>
                    this.spells[SpellSlot.E].IsInRange(x) && this.GetActualHealth(x) < this.GetActualDamage(x)
                    && !x.IsDead))
            {
                if (hero.HasBuffOfType(BuffType.Invulnerability) || hero.HasBuffOfType(BuffType.SpellImmunity)
                    || hero.HasBuffOfType(BuffType.SpellShield))
                {
                    return;
                }

                if (Environment.TickCount - this.spells[SpellSlot.E].LastCastAttemptT < 500 || this.HasUndyingBuff(hero))
                {
                    return;
                }

                this.spells[SpellSlot.E].Cast();
                this.spells[SpellSlot.E].LastCastAttemptT = Environment.TickCount;
            }

            foreach (var hero in
                HeroManager.Enemies.Where(
                    x =>
                    this.spells[SpellSlot.Q].IsInRange(x)
                    && this.GetActualHealth(x) < this.spells[SpellSlot.Q].GetDamage(x)))
            {
                if (hero.HasBuffOfType(BuffType.Invulnerability) || hero.HasBuffOfType(BuffType.SpellImmunity)
                    || hero.HasBuffOfType(BuffType.SpellShield) || this.spells[SpellSlot.E].IsReady())
                {
                    return;
                }

                var prediction = this.spells[SpellSlot.Q].GetPrediction(hero);

                if (prediction.Hitchance >= HitChance.VeryHigh)
                {
                    this.spells[SpellSlot.Q].Cast(prediction.CastPosition);
                }
            }

            if (this.GetItemValue<bool>("com.iseries.kalista.misc.mobsteal") && this.spells[SpellSlot.E].IsReady())
            {
                var normalMob =
                    MinionManager.GetMinions(
                        this.Player.ServerPosition, 
                        this.spells[SpellSlot.E].Range, 
                        MinionTypes.All, 
                        MinionTeam.NotAlly, 
                        MinionOrderTypes.MaxHealth)
                        .FirstOrDefault(
                            x =>
                            x.IsValid && x.Health < this.GetActualDamage(x) && !x.Name.Contains("Mini")
                            && !x.Name.Contains("Dragon") && !x.Name.Contains("Baron"));

                var baron =
                    MinionManager.GetMinions(
                        this.Player.ServerPosition, 
                        this.spells[SpellSlot.E].Range, 
                        MinionTypes.All, 
                        MinionTeam.Neutral, 
                        MinionOrderTypes.MaxHealth)
                        .FirstOrDefault(
                            x => x.IsValid && x.Health < this.GetBaronReduction(x) && x.Name.Contains("Baron"));

                var dragon =
                    MinionManager.GetMinions(
                        this.Player.ServerPosition, 
                        this.spells[SpellSlot.E].Range, 
                        MinionTypes.All, 
                        MinionTeam.Neutral, 
                        MinionOrderTypes.MaxHealth)
                        .FirstOrDefault(
                            x => x.IsValid && x.Health < this.GetDragonReduction(x) && x.Name.Contains("Dragon"));

                if ((normalMob != null && this.spells[SpellSlot.E].CanCast(normalMob))
                    || (baron != null && this.spells[SpellSlot.E].CanCast(baron))
                    || (dragon != null && this.spells[SpellSlot.E].CanCast(dragon)))
                {
                    if (Environment.TickCount - this.spells[SpellSlot.E].LastCastAttemptT < 500)
                    {
                        return;
                    }

                    this.spells[SpellSlot.E].Cast();
                    this.spells[SpellSlot.E].LastCastAttemptT = Environment.TickCount;
                }
            }

            this.HandleSentinels();
        }

        #endregion
    }
}