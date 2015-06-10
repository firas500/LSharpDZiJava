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

    using SharpDX;

    /// <summary>
    ///     The Champion Class
    /// </summary>
    internal class Lucian : Champion
    {
        #region Fields

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
                                                                   {
                                                                       { SpellSlot.Q, new Spell(SpellSlot.Q, 675) }, 
                                                                       { SpellSlot.W, new Spell(SpellSlot.W, 1000) }, 
                                                                       { SpellSlot.E, new Spell(SpellSlot.E, 425) }, 
                                                                       { SpellSlot.R, new Spell(SpellSlot.R, 1400) }
                                                                   };

        /// <summary>
        ///     The Passive Check
        /// </summary>
        private bool shouldHavePassive;

        public Vector3 REndPosition { get; private set; }

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

            this.spells[SpellSlot.Q].SetTargetted(0.25f, float.MaxValue);
            this.spells[SpellSlot.W].SetSkillshot(0.4f, 150f, 1600, true, SkillshotType.SkillshotLine);

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
        /// </returns>
        public float GetComboDamage(Obj_AI_Base target)
        {
            var damage = 0f;
            var qDamage = this.spells[SpellSlot.Q].GetDamage(target);
            var wDamage = this.spells[SpellSlot.W].GetDamage(target);

            if (this.spells[SpellSlot.Q].IsReady())
            {
                damage += qDamage;
            }

            if (this.spells[SpellSlot.W].IsReady())
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
            var target = TargetSelector.GetTarget(this.spells[SpellSlot.Q].Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget(this.spells[SpellSlot.W].Range) && target != null && !this.HasPassive())
            {
                if (this.spells[SpellSlot.W].IsReady())
                {
                    this.spells[SpellSlot.W].Cast(target.ServerPosition);
                    this.spells[SpellSlot.W].LastCastAttemptT = Environment.TickCount;
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

        private void LockR(Obj_AI_Hero target)
        {
            var targetPosition = this.spells[SpellSlot.R].GetPrediction(target).CastPosition;
            var endPosition = this.Player.ServerPosition.To2D()
                              + Vector2.Normalize(this.Player.ServerPosition.To2D() - this.REndPosition.To2D()).Perpendicular()
                              * 650;
            var projection = this.Player.ServerPosition.To2D().ProjectOn(endPosition, targetPosition.To2D());
            var projection1 = this.Player.ServerPosition.To2D().ProjectOn(endPosition, targetPosition.To2D());
            var pointSegment1 = new Vector2(projection.SegmentPoint.X, projection.SegmentPoint.Y);
            var pointSegment2 = new Vector2(projection1.SegmentPoint.X, projection1.SegmentPoint.Y);

            this.Player.IssueOrder(
                GameObjectOrder.MoveTo,
                (Vector3)
                (!pointSegment1.IsWall() ? pointSegment1 : !pointSegment2.IsWall() ? pointSegment2 : pointSegment1));
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
                        this.spells[SpellSlot.Q].LastCastAttemptT = Environment.TickCount;
                        break;
                    case "LucianW":
                        Utility.DelayAction.Add(
                            (int)(Math.Ceiling(Game.Ping / 2f) + 250 + 325), 
                            Orbwalking.ResetAutoAttackTimer);
                        this.spells[SpellSlot.W].LastCastAttemptT = Environment.TickCount;
                        break;
                    case "LucianE":
                        Utility.DelayAction.Add(
                            (int)(Math.Ceiling(Game.Ping / 2f) + 250 + 325), 
                            Orbwalking.ResetAutoAttackTimer);
                        this.spells[SpellSlot.E].LastCastAttemptT = Environment.TickCount;
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
            if (this.Player.IsCastingInterruptableSpell(true))
            {
                this.LockR(TargetSelector.GetTarget(this.spells[SpellSlot.R].Range, TargetSelector.DamageType.Physical));
            }

            foreach (var hero in
                HeroManager.Enemies.Where(
                    x => this.spells[SpellSlot.Q].IsInRange(x) && x.Health + 5 < this.spells[SpellSlot.Q].GetDamage(x)))
            {
                this.spells[SpellSlot.Q].CastOnUnit(hero);
                this.spells[SpellSlot.Q].LastCastAttemptT = Environment.TickCount;
            }

            foreach (var hero in
                HeroManager.Enemies.Where(
                    x => this.spells[SpellSlot.W].IsInRange(x) && x.Health + 5 < this.spells[SpellSlot.W].GetDamage(x))
                    .Where(hero => this.spells[SpellSlot.W].GetPrediction(hero).Hitchance >= HitChance.Medium))
            {
                this.spells[SpellSlot.W].CastOnUnit(hero);
                this.spells[SpellSlot.W].LastCastAttemptT = Environment.TickCount;
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
                    if (target.IsValidTarget(this.spells[SpellSlot.Q].Range) && target != null)
                    {
                        if (this.spells[SpellSlot.Q].IsReady() && this.spells[SpellSlot.Q].IsInRange(target)
                            && !this.HasPassive())
                        {
                            this.spells[SpellSlot.Q].CastOnUnit(target);
                            this.spells[SpellSlot.Q].LastCastAttemptT = Environment.TickCount;
                        }
                    }

                    if (target.IsValidTarget(this.spells[SpellSlot.W].Range) && target != null && !this.HasPassive())
                    {
                        if (this.spells[SpellSlot.W].IsReady())
                        {
                            this.spells[SpellSlot.W].Cast(this.spells[SpellSlot.W].GetPrediction(target).CastPosition);
                            this.spells[SpellSlot.W].LastCastAttemptT = Environment.TickCount;
                        }
                    }

                    break;
            }
        }

        #endregion
    }
}