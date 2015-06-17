// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Graves.cs" company="LeagueSharp">
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
//   TODO The graves.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using SharpDX;

namespace iSeries.Champions.Graves
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using iSeries.General;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     TODO The graves.
    /// </summary>
    internal class Graves : Champion
    {
        #region Fields

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
                                                                   {
                                                                       { SpellSlot.Q, new Spell(SpellSlot.Q, 800f) }, 
                                                                       { SpellSlot.W, new Spell(SpellSlot.W, 950f) }, 
                                                                       { SpellSlot.E, new Spell(SpellSlot.E, 425f) }, 
                                                                       { SpellSlot.R, new Spell(SpellSlot.R, 1100f) }

                                                                       // TODO Tweak this. It has 1000 range + 800 in cone
                                                                   };

        private float LastCheckTick;

        #endregion

        /// <summary>
        ///     Initializes a new instance of the <see cref="Graves"/> class.
        /// </summary>
        public Graves()
        {
            this.CreateMenu = MenuGenerator.Generate;

            this.spells[SpellSlot.Q].SetSkillshot(0.26f, 10f * 2 * (float)Math.PI / 180, 1950f, false, SkillshotType.SkillshotCone);
            this.spells[SpellSlot.W].SetSkillshot(0.30f, 250f, 1650f, false, SkillshotType.SkillshotCircle);
            this.spells[SpellSlot.R].SetSkillshot(0.22f, 150f, 2100, true, SkillshotType.SkillshotLine);
        }

        #region Public Methods and Operators

        /// <summary>
        /// TODO The get combo damage.
        /// </summary>
        /// <param name="spells">
        /// TODO The spells.
        /// </param>
        /// <param name="unit">
        /// TODO The unit.
        /// </param>
        /// <returns>
        /// </returns>
        public static float GetComboDamage(Dictionary<SpellSlot, Spell> spells, Obj_AI_Hero unit)
        {
            if (!unit.IsValidTarget())
            {
                return 0;
            }

            return
                spells.Where(spell => spell.Value.IsReady())
                    .Sum(spell => (float)ObjectManager.Player.GetSpellDamage(unit, spell.Key))
                + (float)ObjectManager.Player.GetAutoAttackDamage(unit) * 2;
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
        ///     <c>OnCombo</c> subscribed orb walker function.
        /// </summary>
        public override void OnCombo()
        {
            var target = TargetSelector.GetTarget(
                this.spells[SpellSlot.E].Range + this.spells[SpellSlot.Q].Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget())
            {
                if (GetComboDamage(this.spells, target) > target.Health + 20)
                {
                    if (ObjectManager.Player.Distance(target) > spells[SpellSlot.Q].Range)
                    {
                        spells[SpellSlot.Q].From = ObjectManager.Player.ServerPosition.Extend(
                            target.ServerPosition, spells[SpellSlot.E].Range);
                        spells[SpellSlot.Q].RangeCheckFrom =
                            ObjectManager.Player.ServerPosition.Extend(target.ServerPosition, spells[SpellSlot.E].Range);
                        var QPrediction = spells[SpellSlot.Q].GetPrediction(target);
                        if (QPrediction.Hitchance >= HitChance.High)
                        {
                            //EQR
                            if (
                                IsSafe(
                                    ObjectManager.Player.ServerPosition.Extend(
                                        target.ServerPosition, spells[SpellSlot.E].Range)))
                            {
                                spells[SpellSlot.E].Cast(
                                ObjectManager.Player.ServerPosition.Extend(
                                    target.ServerPosition, spells[SpellSlot.E].Range));
                                Utility.DelayAction.Add(
                                    (int)(Game.Ping / 2f + 250 + 220), () =>
                                    {
                                        spells[SpellSlot.Q].Cast(QPrediction.CastPosition);
                                        spells[SpellSlot.R].Cast(QPrediction.CastPosition);

                                    });
                            }
                            spells[SpellSlot.Q].RangeCheckFrom = ObjectManager.Player.ServerPosition;
                            spells[SpellSlot.Q].From = ObjectManager.Player.ServerPosition;
                        }
                        else
                        {
                            var QPrediction2 = spells[SpellSlot.Q].GetPrediction(target);
                            if (QPrediction2.Hitchance >= HitChance.High)
                            {
                                spells[SpellSlot.Q].Cast(QPrediction2.CastPosition);
                                Utility.DelayAction.Add(
                                    (int) (Game.Ping / 2f + 250 + 100), () =>
                                    {
                                        if (IsSafe(ObjectManager.Player.ServerPosition.Extend(Game.CursorPos, 100)))
                                        {
                                            spells[SpellSlot.E].Cast(
                                            ObjectManager.Player.ServerPosition.Extend(Game.CursorPos, 100));
                                        }
                                        spells[SpellSlot.R].Cast(QPrediction2.CastPosition);
                                    });
                            }
                        }
                    }
                    else
                    {
                        var myTarget = TargetSelector.GetTarget(
                            this.spells[SpellSlot.Q].Range, TargetSelector.DamageType.Physical);
                        var rTarget = TargetSelector.GetTarget(
                            this.spells[SpellSlot.R].Range, TargetSelector.DamageType.Physical);

                        if (this.GetItemValue<bool>("com.iseries.graves.combo.useQ") &&
                            this.spells[SpellSlot.Q].IsReady() && myTarget.IsValidTarget(this.spells[SpellSlot.Q].Range))
                        {
                            this.spells[SpellSlot.Q].CastIfHitchanceEquals(myTarget, HitChance.VeryHigh);
                        }

                        if (this.GetItemValue<bool>("com.iseries.graves.combo.useW") &&
                            this.spells[SpellSlot.W].IsReady() && myTarget.IsValidTarget(this.spells[SpellSlot.Q].Range))
                        {
                            this.spells[SpellSlot.W].CastIfWillHit(
                                myTarget, this.GetItemValue<Slider>("com.iseries.graves.combo.minW").Value);
                        }

                        if (rTarget.IsValidTarget(this.spells[SpellSlot.R].Range) && this.spells[SpellSlot.R].IsReady())
                        {
                            if (this.GetItemValue<bool>("com.iseries.graves.combo.useR") &&
                                this.spells[SpellSlot.R].GetDamage(rTarget) >= rTarget.Health + 20 &&
                                !(ObjectManager.Player.Distance(rTarget) < ObjectManager.Player.AttackRange + 120))
                            {
                                this.spells[SpellSlot.R].CastIfHitchanceEquals(rTarget, HitChance.VeryHigh);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Checks if a position is safe
        /// </summary>
        /// <param name="position">
        ///     The Position
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        private bool IsSafe(Vector3 position)
        {
            if (position.UnderTurret(true) && !ObjectManager.Player.UnderTurret(true))
            {
                return false;
            }

            var allies = position.CountAlliesInRange(ObjectManager.Player.AttackRange);
            var enemies = position.CountEnemiesInRange(ObjectManager.Player.AttackRange);
            var lhEnemies = GetLhEnemiesNearPosition(position, ObjectManager.Player.AttackRange).Count();

            if (enemies == 1)
            {
                // It's a 1v1, safe to assume I can E
                return true;
            }

            // Adding 1 for the Player
            return allies + 1 > enemies - lhEnemies;
        }

        /// <summary>
        ///     Gets Enemies near a position
        /// </summary>
        /// <param name="position">
        ///     The Position
        /// </param>
        /// <param name="range">
        ///     The Range
        /// </param>
        /// <returns>
        ///     a list of enemies
        /// </returns>
        public static List<Obj_AI_Hero> GetLhEnemiesNearPosition(Vector3 position, float range)
        {
            return
                HeroManager.Enemies.Where(hero => hero.IsValidTarget(range, true, position) && hero.HealthPercent <= 15)
                    .ToList();
        }

        /// <summary>
        ///     <c>OnDraw</c> subscribed event function.
        /// </summary>
        /// <param name="args">
        ///     The event data
        /// </param>
        public override void OnDraw(EventArgs args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     <c>OnHarass</c> subscribed orb walker function.
        /// </summary>
        public override void OnHarass()
        {
            var myTarget = TargetSelector.GetTarget(
                             this.spells[SpellSlot.Q].Range, TargetSelector.DamageType.Physical);

            if (this.GetItemValue<bool>("com.iseries.graves.harass.useQ") &&
                this.spells[SpellSlot.Q].IsReady() && myTarget.IsValidTarget(this.spells[SpellSlot.Q].Range))
            {
                this.spells[SpellSlot.Q].CastIfHitchanceEquals(myTarget, HitChance.VeryHigh);
            }
        }

        /// <summary>
        ///     <c>OnLaneclear</c> subscribed orb walker function.
        /// </summary>
        public override void OnLaneclear()
        {
            var farmLocation = spells[SpellSlot.Q].GetLineFarmLocation(
                MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[SpellSlot.Q].Range));
            if (this.GetItemValue<bool>("com.iseries.graves.laneclear.useQ") &&
                this.spells[SpellSlot.Q].IsReady() && farmLocation.MinionsHit > 2)
            {
                spells[SpellSlot.Q].Cast(farmLocation.Position);
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
        ///     The Code to always check and execute
        /// </summary>
        private void OnUpdateFunctions()
        {
            if (Environment.TickCount - LastCheckTick < 120)
            {
                return;
            }
            LastCheckTick = Environment.TickCount;
            if (GetItemValue<bool>("com.iseries.graves.misc.peel") 
                && spells[SpellSlot.E].IsReady() 
                && ObjectManager.Player.CountEnemiesInRange(380f) > 0 
                && ObjectManager.Player.CountAlliesInRange(380f) == 0
                && ObjectManager.Player.HealthPercent < 20)
            {
                var closest =
                    ObjectManager.Player.GetEnemiesInRange(380f).OrderBy(h => h.Distance(ObjectManager.Player)).First();
                var extended = closest.ServerPosition.Extend(
                    ObjectManager.Player.ServerPosition,
                    ObjectManager.Player.Distance(closest) + spells[SpellSlot.E].Range);
                if (IsSafe(extended))
                {
                    spells[SpellSlot.E].Cast(extended);
                }
            }
        }

        #endregion
    }
}