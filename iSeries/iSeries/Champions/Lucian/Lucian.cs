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
namespace iSeries.Champions.Lucian
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using iSeries.Champions.Utilities;

    using LeagueSharp;
    using LeagueSharp.Common;
    using LeagueSharp.Common.Data;

    /// <summary>
    ///     The Champion Class
    /// </summary>
    internal class Lucian : Champion
    {

        enum Spells
        {
            Q, Q1, W, E, R
        }

        #region Fields

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
            DamageIndicator.Initialize();

            this.spells[Spells.Q].SetTargetted(0.25f, float.MaxValue);
            this.spells[Spells.Q1].SetSkillshot(0.25f, 5f, float.MaxValue, true, SkillshotType.SkillshotLine);
            this.spells[Spells.W].SetSkillshot(0.4f, 150f, 1600, true, SkillshotType.SkillshotLine);

            Obj_AI_Base.OnProcessSpellCast += this.OnProcessSpellCast;
            Orbwalking.AfterAttack += this.OrbwalkingAfterAttack;
            AntiGapcloser.OnEnemyGapcloser += this.OnGapcloser;
            Spellbook.OnCastSpell += this.OnCastSpell;
            Obj_AI_Base.OnBuffAdd += this.OnAddBuff;
            Obj_AI_Base.OnBuffRemove += this.OnRemoveBuff;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     TODO The get combo damage.
        /// </summary>
        /// <param name="target">
        ///     TODO The attackableTarget.
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
                        if (this.spells[Spells.Q].IsReady() && this.spells[Spells.Q].IsInRange(target) && !this.HasPassive())
                        {
                            this.spells[Spells.Q].CastOnUnit(target);
                            this.spells[Spells.Q].LastCastAttemptT = Environment.TickCount;
                        }
                    }

                    if (this.GetItemValue<bool>("com.iseries.lucian.combo.useW")
                        && target.IsValidTarget(this.spells[Spells.W].Range) && target != null && !this.HasPassive())
                    {
                        if (this.spells[Spells.W].IsReady())
                        {
                            this.spells[Spells.W].Cast(this.spells[Spells.W].GetPrediction(target).CastPosition);
                            this.spells[Spells.W].LastCastAttemptT = Environment.TickCount;
                        }
                    }

                    if (this.GetItemValue<bool>("com.iseries.lucian.misc.peel") && this.spells[Spells.E].IsReady() && this.Player.HealthPercent < 30)
                    {
                        var meleeEnemies = ObjectManager.Player.GetEnemiesInRange(400f).FindAll(m => m.IsMelee());
                        if (meleeEnemies.Any())
                        {
                            var mostDangerous = meleeEnemies.OrderByDescending(m => m.GetAutoAttackDamage(ObjectManager.Player)).First();
                            if (mostDangerous != null)
                            {
                                var position = this.Player.Position.To2D().Extend((mostDangerous.Position - this.Player.Position).To2D(), 425);
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
                var allMinions = MinionManager.GetMinions(this.Player.Position, this.spells[Spells.Q].Range, MinionTypes.All, MinionTeam.NotAlly);
                var minion = allMinions.FirstOrDefault(minionn => minionn.Distance(this.Player.Position) <= this.spells[Spells.Q].Range && HealthPrediction.LaneClearHealthPrediction(minionn, 500) > 0);
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
        ///     Checks if we have the lucian buff passive.
        /// </summary>
        /// <returns>
        ///     true / false
        /// </returns>
        private bool HasPassive()
        {
            return this.shouldHavePassive || Variables.Player.HasBuff("LucianPassiveBuff");
        }

        private void ExtendedQ()
        {
            if (!this.GetItemValue<bool>("com.iseries.lucian.combo.extendedQ"))
            {
                return;
            }

            var target = TargetSelector.GetTarget(this.spells[Spells.Q1].Range, TargetSelector.DamageType.Physical);
            var prediction = this.spells[Spells.Q1].GetPrediction(target, true);
            var minions = MinionManager.GetMinions(
                this.Player.ServerPosition,
                this.spells[Spells.Q].Range,
                MinionTypes.All,
                MinionTeam.NotAlly);

            if (!minions.Any() || target == null)
            {
                return;
            }

            foreach (var minion in (from minion in minions let poly = new Geometry.Polygon.Rectangle(this.Player.ServerPosition, this.Player.ServerPosition.Extend(minion.ServerPosition, this.spells[Spells.Q1].Range), this.spells[Spells.Q1].Width) where poly.IsInside(prediction.UnitPosition) select minion).Where(minion => this.spells[Spells.Q].IsReady()))
            {
                this.spells[Spells.Q].Cast(minion);
            }
        }

        /// <summary>
        ///     TODO The on add buff.
        /// </summary>
        /// <param name="sender">
        ///     TODO The sender.
        /// </param>
        /// <param name="args">
        ///     TODO The args.
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
        ///     TODO The on cast spell.
        /// </summary>
        /// <param name="sender">
        ///     TODO The sender.
        /// </param>
        /// <param name="args">
        ///     TODO The args.
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
        ///     TODO The on gapcloser.
        /// </summary>
        /// <param name="gapcloser">
        ///     TODO The gapcloser.
        /// </param>
        private void OnGapcloser(ActiveGapcloser gapcloser)
        {
        }

        /// <summary>
        ///     TODO The obj_ a i_ base_ on process spell cast.
        /// </summary>
        /// <param name="sender">
        ///     TODO The sender.
        /// </param>
        /// <param name="args">
        ///     TODO The args.
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
        ///     TODO The on remove buff.
        /// </summary>
        /// <param name="sender">
        ///     TODO The sender.
        /// </param>
        /// <param name="args">
        ///     TODO The args.
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
                HeroManager.Enemies.Where(
                    x => this.spells[Spells.Q].IsInRange(x) && x.Health + 5 < this.spells[Spells.Q].GetDamage(x)))
            {
                this.spells[Spells.Q].CastOnUnit(hero);
                this.spells[Spells.Q].LastCastAttemptT = Environment.TickCount;
            }

            foreach (var hero in
                HeroManager.Enemies.Where(
                    x => this.spells[Spells.W].IsInRange(x) && x.Health + 5 < this.spells[Spells.W].GetDamage(x))
                    .Where(hero => this.spells[Spells.W].GetPrediction(hero).Hitchance >= HitChance.Medium))
            {
                this.spells[Spells.W].CastOnUnit(hero);
                this.spells[Spells.W].LastCastAttemptT = Environment.TickCount;
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
                        if (!this.spells[Spells.E].IsReady() || !this.GetItemValue<bool>("com.iseries.lucian.combo.useE"))
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

                        if (PositionHelper.IsSafePosition(hypotheticalPosition) && hypotheticalPosition.Distance(target.ServerPosition)
                            <= Orbwalking.GetRealAutoAttackRange(null)
                            && (!this.spells[Spells.Q].IsReady()
                                || !this.spells[Spells.Q].CanCast(target))
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