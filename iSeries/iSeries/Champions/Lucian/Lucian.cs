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

    using iSeries.Champions.Utilities;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     The Champion Class
    /// </summary>
    internal class Lucian : Champion
    {
        #region Static Fields

        /// <summary>
        ///     The Passive Check
        /// </summary>
        private static bool shouldHavePassive;

        #endregion

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
            var aaDamage = Variables.Player.GetAutoAttackDamage(target, true) * 2;
            var qDamage = this.spells[SpellSlot.Q].GetDamage(target);
            var wDamage = this.spells[SpellSlot.W].GetDamage(target);

            return (float)(aaDamage + qDamage + wDamage);
        }

        /// <summary>
        ///     <c>OnCombo</c> subscribed orb walker function.
        /// </summary>
        public override void OnCombo()
        {
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
            return shouldHavePassive || Variables.Player.HasBuff("LucianPassiveBuff")
                   || (Environment.TickCount - this.spells[SpellSlot.Q].LastCastAttemptT < 500
                       || Environment.TickCount - this.spells[SpellSlot.W].LastCastAttemptT < 500
                       || Environment.TickCount - this.spells[SpellSlot.E].LastCastAttemptT < 500);
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

                // Console.WriteLine(args.SData.Name);
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

            shouldHavePassive = false;
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
                        if (this.spells[SpellSlot.W].IsReady()
                            && Variables.Player.Distance(target) <= Orbwalking.GetRealAutoAttackRange(target))
                        {
                            this.spells[SpellSlot.W].Cast(target.ServerPosition);
                            this.spells[SpellSlot.W].LastCastAttemptT = Environment.TickCount;
                        }
                    }

                    break;
            }
        }

        #endregion
    }
}