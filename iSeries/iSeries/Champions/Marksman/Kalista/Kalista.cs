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
    using System.Linq;

    using iSeries.Champions.Utilities;
    using iSeries.General;

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
        ///     Gets the incoming damage
        /// </summary>
        private readonly Dictionary<float, float> incomingDamage = new Dictionary<float, float>();

        /// <summary>
        ///     Gets the instant damage
        /// </summary>
        private readonly Dictionary<float, float> instantDamage = new Dictionary<float, float>();

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
        ///     <see cref="bool"/>
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
            return target.AttackShield > 0
                       ? target.Health + target.AttackShield
                       : target.MagicShield > 0 ? target.Health + target.MagicShield : target.Health;
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

                if (rendTarget != null && this.GetActualDamage(rendTarget) >= this.GetActualHealth(rendTarget)
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

            foreach (var source in HeroManager.Enemies.Where(x => this.spells[SpellSlot.E].IsInRange(x)))
            {
                var stacks = source.GetBuffCount("kalistaexpungemarker");

                if (stacks > 0)
                {
                    Drawing.DrawText(
                        Drawing.WorldToScreen(source.Position)[0] - 20, 
                        Drawing.WorldToScreen(source.Position)[1], 
                        Color.White, 
                        "Stacks: " + stacks);
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
                var qMinions = MinionManager.GetMinions(
                    this.Player.ServerPosition, 
                    this.spells[SpellSlot.Q].Range);

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
                       ? this.spells[SpellSlot.E].GetDamage(target) + target.HPRegenRate / 2 * 0.5f
                       : this.spells[SpellSlot.E].GetDamage(target) + target.HPRegenRate / 2;
        }

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
            return this.Player.HasBuff("s5test_dragonslayerbuff")
                       ? this.spells[SpellSlot.E].GetDamage(target)
                         + target.HPRegenRate / 2 * (.07f * target.GetBuffCount("s5test_dragonslayerbuff"))
                       : this.spells[SpellSlot.E].GetDamage(target) + target.HPRegenRate / 2;
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
        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "KalistaExpungeWrapper")
            {
                Orbwalking.ResetAutoAttackTimer();
            }

            if (sender.IsEnemy)
            {
                if (this.SoulBound != null && this.GetItemValue<bool>("com.iseries.kalista.misc.saveAlly"))
                {
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
                    x => this.spells[SpellSlot.E].IsInRange(x) && this.GetActualHealth(x) < this.GetActualDamage(x)))
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
                        MinionTeam.NotAlly, 
                        MinionOrderTypes.MaxHealth)
                        .FirstOrDefault(
                            x => x.IsValid && x.Health < this.GetBaronReduction(x) && x.Name.Contains("Baron"));

                var dragon =
                    MinionManager.GetMinions(
                        this.Player.ServerPosition, 
                        this.spells[SpellSlot.E].Range, 
                        MinionTypes.All, 
                        MinionTeam.NotAlly, 
                        MinionOrderTypes.MaxHealth)
                        .FirstOrDefault(
                            x => x.IsValid && x.Health < this.GetDragonReduction(x) && x.Name.Contains("Dragon"));

                if ((normalMob != null && this.spells[SpellSlot.E].CanCast(normalMob))
                    || (baron != null && this.spells[SpellSlot.E].CanCast(baron))
                    || (dragon != null && this.spells[SpellSlot.E].CanCast(dragon)))
                {
                    this.spells[SpellSlot.E].Cast();
                }
            }

            this.HandleSentinels();
        }

        #endregion
    }
}