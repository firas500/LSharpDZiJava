// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Lucian.cs" company="LeagueSharp">
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
namespace iSeries.Champions.Marksman.Lucian
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using iSeries.Champions.Utilities;
    using iSeries.General;

    using LeagueSharp;
    using LeagueSharp.Common;
    using LeagueSharp.Common.Data;

    using SharpDX;

    /// <summary>
    ///     The Champion Class
    /// </summary>
    internal class Lucian : Champion
    {
        #region Fields

        /// <summary>
        ///     Distance check
        ///     Credits Pastel!
        /// </summary>
        private readonly Func<Obj_AI_Hero, Obj_AI_Base, bool> checkDistance =
            (champ, minion) =>
            Math.Abs(
                champ.Distance(ObjectManager.Player) - (minion.Distance(ObjectManager.Player) + minion.Distance(champ)))
            <= 2;

        /// <summary>
        ///     Line check, credits pastel!
        /// </summary>
        private readonly Func<Vector3, Vector3, Vector3, bool> checkLine =
            (v1, v2, v3) =>
            Math.Abs((v1.X * v2.Y) + (v1.Y * v3.X) + (v2.X * v3.Y) - (v1.Y * v2.X) - (v1.X * v3.Y) - (v2.Y * v3.X))
            <= 20000;

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>
                                                                {
                                                                    { Spells.Q, new Spell(SpellSlot.Q, 675) }, 
                                                                    { Spells.Q1, new Spell(SpellSlot.Q, 1100) }, 
                                                                    { Spells.W, new Spell(SpellSlot.W, 1000) }, 
                                                                    { Spells.E, new Spell(SpellSlot.E, 425) }, 
                                                                    { Spells.R, new Spell(SpellSlot.R, 1400) }
                                                                };

        /// <summary>
        ///     The Passive Check
        /// </summary>
        private bool shouldHavePassive;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Lucian" /> class.
        /// </summary>
        public Lucian()
        {
            this.CreateMenu = MenuGenerator.Generate;

            DamageIndicator.DamageToUnit = this.GetComboDamage;
            DamageIndicator.Enabled = true;

            this.spells[Spells.Q].SetTargetted(0.25f, float.MaxValue);
            this.spells[Spells.Q1].SetSkillshot(0.55f, 75f, float.MaxValue, false, SkillshotType.SkillshotLine);
            this.spells[Spells.W].SetSkillshot(0.4f, 150f, 1600, true, SkillshotType.SkillshotLine);

            Obj_AI_Base.OnProcessSpellCast += this.OnProcessSpellCast;
            Orbwalking.AfterAttack += this.OrbwalkingAfterAttack;
            AntiGapcloser.OnEnemyGapcloser += this.OnGapcloser;
            Spellbook.OnCastSpell += this.OnCastSpell;
            Obj_AI_Base.OnBuffAdd += this.OnAddBuff;
            Obj_AI_Base.OnBuffRemove += this.OnRemoveBuff;
        }

        #endregion

        #region Enums

        /// <summary>
        ///     The Spells
        /// </summary>
        private enum Spells
        {
            /// <summary>
            ///     The Q Spell
            /// </summary>
            Q, 

            /// <summary>
            ///     The Extended Q Spell
            /// </summary>
            Q1, 

            /// <summary>
            ///     The W Spell
            /// </summary>
            W, 

            /// <summary>
            ///     The E Spell
            /// </summary>
            E, 

            /// <summary>
            ///     The R  Spell
            /// </summary>
            R
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
        ///     Gets the combo damage
        /// </summary>
        /// <param name="target">
        ///     The target
        /// </param>
        /// <returns>
        ///     The combo damage
        /// </returns>
        public float GetComboDamage(Obj_AI_Base target)
        {
            var damage = 0f;
            var qDamage = this.spells[Spells.Q].GetDamage(target);
            var wDamage = this.spells[Spells.W].GetDamage(target);

            if (this.spells[Spells.Q].IsReady())
            {
                damage += qDamage;
            }

            if (this.spells[Spells.W].IsReady())
            {
                damage += wDamage;
            }

            return damage;
        }

        /// <summary>
        ///     <c>OnCombo</c> subscribed orb walker function.
        /// </summary>
        public override void OnCombo()
        {
            var target = TargetSelector.GetTarget(this.spells[Spells.Q].Range, TargetSelector.DamageType.Physical);
            switch (Variables.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    this.ExtendedQ();
                    if (this.GetItemValue<bool>("com.iseries.lucian.combo.useQ")
                        && target.IsValidTarget(this.spells[Spells.Q].Range) && target != null)
                    {
                        if (this.spells[Spells.Q].IsReady() && this.spells[Spells.Q].IsInRange(target)
                            && !this.HasPassive())
                        {
                            this.spells[Spells.Q].CastOnUnit(target);
                            this.spells[Spells.Q].LastCastAttemptT = Environment.TickCount;
                        }
                    }

                    if (this.GetItemValue<bool>("com.iseries.lucian.combo.useW")
                        && target.IsValidTarget(this.spells[Spells.W].Range) && target != null && !this.HasPassive())
                    {
                        if (this.spells[Spells.W].IsReady() && !target.IsDead)
                        {
                            var prediction = this.spells[Spells.W].GetPrediction(target);
                            if (prediction.Hitchance >= HitChance.Medium)
                            {
                                this.spells[Spells.W].Cast(target);
                                this.spells[Spells.W].LastCastAttemptT = Environment.TickCount;
                            }
                        }
                    }

                    if (this.GetItemValue<bool>("com.iseries.lucian.combo.useR") && this.spells[Spells.R].IsReady())
                    {
                        var ultTarget = TargetSelector.GetTarget(
                            this.spells[Spells.R].Range, 
                            TargetSelector.DamageType.Physical);

                        if (ultTarget != null
                            && this.spells[Spells.R].GetDamage(ultTarget) * this.GetShots() > ultTarget.Health
                            && !this.HasPassive())
                        {
                            this.spells[Spells.R].Cast(ultTarget);
                        }
                    }

                    if (this.GetItemValue<bool>("com.iseries.lucian.misc.peel") && this.spells[Spells.E].IsReady()
                        && this.Player.HealthPercent < 30)
                    {
                        var meleeEnemies = ObjectManager.Player.GetEnemiesInRange(400f).FindAll(m => m.IsMelee());
                        if (meleeEnemies.Any())
                        {
                            var mostDangerous =
                                meleeEnemies.OrderByDescending(m => m.GetAutoAttackDamage(ObjectManager.Player)).First();
                            if (mostDangerous != null)
                            {
                                var position =
                                    this.Player.Position.To2D()
                                        .Extend((mostDangerous.Position - this.Player.Position).To2D(), 425);
                                if (position.To3D().UnderTurret(true) || position.To3D().IsWall())
                                {
                                    return;
                                }

                                this.spells[Spells.E].Cast(position);
                            }
                        }
                    }

                    break;
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
            if (this.GetItemValue<bool>("com.iseries.lucian.laneclear.useQ"))
            {
                var allMinions = MinionManager.GetMinions(
                    this.Player.Position, 
                    this.spells[Spells.Q].Range, 
                    MinionTypes.All, 
                    MinionTeam.NotAlly);
                var minion =
                    allMinions.FirstOrDefault(
                        minionn =>
                        minionn.Distance(this.Player.Position) <= this.spells[Spells.Q].Range
                        && HealthPrediction.LaneClearHealthPrediction(minionn, 500) > 0);
                if (minion == null)
                {
                    return;
                }

                this.spells[Spells.Q].Cast(minion);
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
        ///     Casts an extended spell to the target Credits Pastel Com!
        /// </summary>
        private void ExtendedQ()
        {
            if (!this.GetItemValue<bool>("com.iseries.lucian.combo.extendedQ"))
            {
                return;
            }

            foreach (var collisionMinion in from target in this.Player.GetEnemiesInRange(this.spells[Spells.Q1].Range)
                                            let position = new List<Vector2> { target.Position.To2D() }
                                            select
                                                this.spells[Spells.Q1].GetCollision(
                                                    this.Player.Position.To2D(), 
                                                    position)
                                                .FirstOrDefault(
                                                    minion =>
                                                    this.spells[Spells.Q].CanCast(minion)
                                                    && this.spells[Spells.Q].IsInRange(minion)
                                                    && this.checkLine(
                                                        this.Player.Position, 
                                                        minion.Position, 
                                                        target.ServerPosition) && this.checkDistance(target, minion)
                                                    && target.Distance(this.Player) > minion.Distance(this.Player)
                                                    && this.Player.Distance(minion) + minion.Distance(target)
                                                    <= this.Player.Distance(target) + 10f)
                                            into collisionMinion
                                            where collisionMinion != null
                                            select collisionMinion)
            {
                this.spells[Spells.Q].CastOnUnit(collisionMinion);
            }
        }

        /// <summary>
        ///     Gets the Ultimate Shots
        /// </summary>
        /// <returns>
        ///     The <see cref="double" />.
        /// </returns>
        private double GetShots()
        {
            double shots = 0;

            switch (this.spells[Spells.R].Level)
            {
                case 1:
                    shots = 7.5 + 7.5 * (this.Player.AttackSpeedMod - .6);
                    break;
                case 2:
                    shots = 7.5 + 9 * (this.Player.AttackSpeedMod - .6);
                    break;
                case 3:
                    shots = 7.5 + 10.5 * (this.Player.AttackSpeedMod - .6);
                    break;
            }

            return shots / 1.4;
        }

        /// <summary>
        ///     Checks if we have the lucian buff passive.
        /// </summary>
        /// <returns>
        ///     true / false
        /// </returns>
        private bool HasPassive()
        {
            return this.shouldHavePassive || this.Player.HasBuff("LucianPassiveBuff");
        }

        /// <summary>
        ///     The On Add Buff
        /// </summary>
        /// <param name="sender">
        ///     The Sender
        /// </param>
        /// <param name="args">
        ///     The Args
        /// </param>
        private void OnAddBuff(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if (sender.IsMe && args.Buff.Name == "lucianpassivebuff")
            {
                this.shouldHavePassive = true;
                Console.WriteLine("Has Passive buff");
            }
        }

        /// <summary>
        ///     The On Cast Spell Method
        /// </summary>
        /// <param name="sender">
        ///     The Sender
        /// </param>
        /// <param name="args">
        ///     The Args
        /// </param>
        private void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            switch (args.Slot)
            {
                case SpellSlot.Q:
                case SpellSlot.W:
                case SpellSlot.E:
                    this.shouldHavePassive = true;
                    break;
                case SpellSlot.R:
                    if (ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                    {
                        ItemData.Youmuus_Ghostblade.GetItem().Cast();
                    }

                    break;
            }
        }

        /// <summary>
        ///     The Gap closer method
        /// </summary>
        /// <param name="gapcloser">
        ///     The Gap closer
        /// </param>
        private void OnGapcloser(ActiveGapcloser gapcloser)
        {
        }

        /// <summary>
        ///     The Process Spell Casting
        /// </summary>
        /// <param name="sender">
        ///     The Sender
        /// </param>
        /// <param name="args">
        ///     The Args
        /// </param>
        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                switch (args.SData.Name)
                {
                    case "LucianQ":
                        Utility.DelayAction.Add(
                            (int)(Math.Ceiling(Game.Ping / 2f) + 250 + 325), 
                            Orbwalking.ResetAutoAttackTimer);
                        this.spells[Spells.Q].LastCastAttemptT = Environment.TickCount;
                        break;
                    case "LucianW":
                        Utility.DelayAction.Add(
                            (int)(Math.Ceiling(Game.Ping / 2f) + 250 + 325), 
                            Orbwalking.ResetAutoAttackTimer);
                        this.spells[Spells.W].LastCastAttemptT = Environment.TickCount;
                        break;
                    case "LucianE":
                        Utility.DelayAction.Add(
                            (int)(Math.Ceiling(Game.Ping / 2f) + 250 + 325), 
                            Orbwalking.ResetAutoAttackTimer);
                        this.spells[Spells.E].LastCastAttemptT = Environment.TickCount;
                        break;
                }
            }
        }

        /// <summary>
        ///     On Remove Buff
        /// </summary>
        /// <param name="sender">
        ///     The Sender
        /// </param>
        /// <param name="args">
        ///     The Args
        /// </param>
        private void OnRemoveBuff(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args)
        {
            if (sender.IsMe && args.Buff.Name == "lucianpassivebuff")
            {
                this.shouldHavePassive = false;
                Console.WriteLine("No Passive Buff");
            }
        }

        /// <summary>
        ///     The Functions to always process
        /// </summary>
        private void OnUpdateFunctions()
        {
            foreach (var hero in
                HeroManager.Enemies.Where(x => x.Health + 5 < this.spells[Spells.Q].GetDamage(x)))
            {
                if (this.Player.Distance(hero) > this.spells[Spells.Q].Range
                    && this.Player.Distance(hero) <= this.spells[Spells.Q1].Range)
                {
                    this.ExtendedQ();
                }
                else
                {
                    this.spells[Spells.Q].CastOnUnit(hero);
                }
            }
        }

        /// <summary>
        ///     After attack
        /// </summary>
        /// <param name="unit">
        ///     The Unit
        /// </param>
        /// <param name="attackableTarget">
        ///     The attackable target
        /// </param>
        private void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit attackableTarget)
        {
            if (!unit.IsMe)
            {
                return;
            }

            this.shouldHavePassive = false;

            var target = attackableTarget as Obj_AI_Hero;
            switch (Variables.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (target != null)
                    {
                        if (!this.spells[Spells.E].IsReady()
                            || !this.GetItemValue<bool>("com.iseries.lucian.combo.useE"))
                        {
                            return;
                        }

                        var hypotheticalPosition = ObjectManager.Player.ServerPosition.Extend(
                            Game.CursorPos, 
                            this.spells[Spells.E].Range);
                        if (ObjectManager.Player.HealthPercent <= 30
                            && target.HealthPercent >= ObjectManager.Player.HealthPercent)
                        {
                            if (ObjectManager.Player.Position.Distance(ObjectManager.Player.ServerPosition) >= 35
                                && target.Distance(ObjectManager.Player.ServerPosition)
                                < target.Distance(ObjectManager.Player.Position)
                                && PositionHelper.IsSafePosition(hypotheticalPosition))
                            {
                                this.spells[Spells.E].Cast(hypotheticalPosition);
                                this.spells[Spells.E].LastCastAttemptT = Environment.TickCount;
                            }
                        }

                        if (PositionHelper.IsSafePosition(hypotheticalPosition)
                            && hypotheticalPosition.Distance(target.ServerPosition)
                            <= Orbwalking.GetRealAutoAttackRange(null)
                            && (!this.spells[Spells.Q].IsReady() || !this.spells[Spells.Q].CanCast(target))
                            && (!this.spells[Spells.W].IsReady()
                                || !this.spells[Spells.W].CanCast(target)
                                && (hypotheticalPosition.Distance(target.ServerPosition) > 400) && !this.HasPassive()))
                        {
                            this.spells[Spells.E].Cast(hypotheticalPosition);
                            this.spells[Spells.E].LastCastAttemptT = Environment.TickCount;
                        }
                    }

                    break;
            }
        }

        #endregion
    }
}