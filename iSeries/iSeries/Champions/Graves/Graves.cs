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

namespace iSeries.Champions.Graves
{
    using System;

    using iSeries.General;

    using LeagueSharp.Common;

    /// <summary>
    ///     TODO The graves.
    /// </summary>
    internal class Graves : Champion
    {

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
        {
                            { SpellSlot.Q, new Spell(SpellSlot.Q, 800f) },
                            { SpellSlot.W, new Spell(SpellSlot.W, 950f) },
                            { SpellSlot.E, new Spell(SpellSlot.E, 425f) },
                            { SpellSlot.R, new Spell(SpellSlot.R, 1100f) } //TODO Tweak this. It has 1000 range + 800 in cone
        };

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
            var target = TargetSelector.GetTarget(
                spells[SpellSlot.E].Range + spells[SpellSlot.Q].Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget())
            {
                if (GetComboDamage(spells, target) > target.Health + 20)
                {
                    ////TODO Burst Combo
                }
                else
                {
                    var myTarget =  TargetSelector.GetTarget( spells[SpellSlot.Q].Range, TargetSelector.DamageType.Physical);
                    var rTarget = TargetSelector.GetTarget(spells[SpellSlot.R].Range, TargetSelector.DamageType.Physical);

                    if (GetItemValue<bool>("com.iseries.graves.combo.useQ") && spells[SpellSlot.Q].IsReady() && myTarget.IsValidTarget(spells[SpellSlot.Q].Range))
                    {
                        spells[SpellSlot.Q].CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    }

                    if (GetItemValue<bool>("com.iseries.graves.combo.useW") && spells[SpellSlot.W].IsReady() && myTarget.IsValidTarget(spells[SpellSlot.Q].Range))
                    {
                        spells[SpellSlot.W].CastIfWillHit(
                            myTarget, GetItemValue<Slider>("com.iseries.graves.combo.minW").Value);
                    }

                    if (rTarget.IsValidTarget(spells[SpellSlot.R].Range) && spells[SpellSlot.R].IsReady() )
                    {
                        if (GetItemValue<bool>("com.iseries.graves.combo.useR") && spells[SpellSlot.R].GetDamage(rTarget) >= rTarget.Health + 20 &&
                        !(ObjectManager.Player.Distance(rTarget) < ObjectManager.Player.AttackRange + 120))
                        {
                            spells[SpellSlot.R].CastIfHitchanceEquals(rTarget, HitChance.VeryHigh);
                        }
                    }
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
            throw new NotImplementedException();
        }

        /// <summary>
        ///     <c>OnHarass</c> subscribed orb walker function.
        /// </summary>
        public override void OnHarass()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     <c>OnLaneclear</c> subscribed orb walker function.
        /// </summary>
        public override void OnLaneclear()
        {
            throw new NotImplementedException();
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
        }

        public static float GetComboDamage(Dictionary<SpellSlot, Spell> spells, Obj_AI_Hero unit)
        {
            if (!unit.IsValidTarget())
                return 0;
            return spells.Where(spell => spell.Value.IsReady()).Sum(spell => (float)ObjectManager.Player.GetSpellDamage(unit, spell.Key)) + (float)ObjectManager.Player.GetAutoAttackDamage(unit) * 2;
        }

        #endregion
    }
}