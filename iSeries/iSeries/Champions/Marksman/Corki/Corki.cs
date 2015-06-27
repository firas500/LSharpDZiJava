// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Corki.cs" company="LeagueSharp">
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
namespace iSeries.Champions.Marksman.Corki
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
    internal class Corki : Champion
    {
        #region Fields

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>()
                                                                   {
                                                                       { SpellSlot.Q, new Spell(SpellSlot.Q, 825f) }, 
                                                                       { SpellSlot.W, new Spell(SpellSlot.W, 800f) }, 
                                                                       { SpellSlot.E, new Spell(SpellSlot.E, 600f) }, 
                                                                       { SpellSlot.R, new Spell(SpellSlot.R, 1300f) }
                                                                   };

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Corki" /> class.
        /// </summary>
        public Corki()
        {
            // Menu Generation
            this.CreateMenu = MenuGenerator.Generate;

            // Damage Indicator
            DamageIndicator.DamageToUnit = this.GetDamage;
            DamageIndicator.Enabled = true;

            // Spell initialization
            this.spells[SpellSlot.Q].SetSkillshot(0.35f, 250f, 1000f, false, SkillshotType.SkillshotCircle);
            this.spells[SpellSlot.E].SetSkillshot(
                0f, 
                (float)(45 * Math.PI / 180), 
                1500, 
                false, 
                SkillshotType.SkillshotCone);
            this.spells[SpellSlot.R].SetSkillshot(0.2f, 40f, 2000f, true, SkillshotType.SkillshotLine);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Performs the Auto Harass
        /// </summary>
        public void AutoHarass()
        {
            if (!this.GetItemValue<bool>("com.iseries.corki.harass.auto.autoHarass"))
            {
                return;
            }

            if (this.GetItemValue<bool>("com.iseries.corki.harass.auto.useQ") && this.spells[SpellSlot.Q].IsReady())
            {
                var target = TargetSelector.GetTarget(
                    this.spells[SpellSlot.Q].Range, 
                    TargetSelector.DamageType.Physical);

                if (this.GetItemValue<bool>("com.iseries.corki.harass.auto.disable" + target.ChampionName))
                {
                    return;
                }

                var prediction = this.spells[SpellSlot.Q].GetPrediction(target);

                if (prediction.Hitchance >= this.GetHitchance() && target.IsValidTarget(this.spells[SpellSlot.Q].Range))
                {
                    this.spells[SpellSlot.Q].Cast(prediction.CastPosition);
                }
            }
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
        ///     Gets the currently selected hit chance
        /// </summary>
        /// <returns>
        ///     The <see cref="HitChance" />.
        /// </returns>
        public HitChance GetHitchance()
        {
            switch (this.GetItemValue<StringList>("com.iseries.corki.misc.hitchance").SelectedIndex)
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
        ///     <c>OnCombo</c> subscribed orb walker function.
        /// </summary>
        public override void OnCombo()
        {
            if (this.GetItemValue<bool>("com.iseries.corki.combo.useQ") && this.spells[SpellSlot.Q].IsReady())
            {
                var target = TargetSelector.GetTarget(
                    this.spells[SpellSlot.Q].Range, 
                    TargetSelector.DamageType.Physical);
                var prediction = this.spells[SpellSlot.Q].GetPrediction(target);

                if (prediction.Hitchance >= this.GetHitchance() && target.IsValidTarget(this.spells[SpellSlot.Q].Range))
                {
                    this.spells[SpellSlot.Q].Cast(prediction.CastPosition);
                }
            }

            if (this.GetItemValue<bool>("com.iseries.corki.combo.useE") && this.spells[SpellSlot.E].IsReady())
            {
                var target = HeroManager.Enemies.FirstOrDefault(h => h.IsValidTarget(this.spells[SpellSlot.E].Range));
                if (target.IsValidTarget())
                {
                    this.spells[SpellSlot.E].Cast(target);
                }
            }

            if (this.GetItemValue<bool>("com.iseries.corki.combo.useR") && this.spells[SpellSlot.R].IsReady())
            {
                var target = TargetSelector.GetTarget(
                    this.spells[SpellSlot.R].Range, 
                    TargetSelector.DamageType.Physical);
                var prediction = this.spells[SpellSlot.R].GetPrediction(target);

                if (target.IsValidTarget(this.spells[SpellSlot.R].Range) && prediction.Hitchance >= this.GetHitchance())
                {
                    this.spells[SpellSlot.R].Cast(prediction.CastPosition);
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
            if (this.GetItemValue<bool>("com.iseries.corki.draw.q"))
            {
                Render.Circle.DrawCircle(this.Player.Position, this.spells[SpellSlot.Q].Range, Color.DarkRed);
            }
        }

        /// <summary>
        ///     <c>OnHarass</c> subscribed orb walker function.
        /// </summary>
        public override void OnHarass()
        {
            if (this.GetItemValue<bool>("com.iseries.corki.harass.useQ") && this.spells[SpellSlot.Q].IsReady())
            {
                var target = TargetSelector.GetTarget(
                    this.spells[SpellSlot.Q].Range, 
                    TargetSelector.DamageType.Physical);
                var prediction = this.spells[SpellSlot.Q].GetPrediction(target);

                if (prediction.Hitchance >= this.GetHitchance() && target.IsValidTarget(this.spells[SpellSlot.Q].Range))
                {
                    this.spells[SpellSlot.Q].Cast(prediction.CastPosition);
                }
            }

            if (this.GetItemValue<bool>("com.iseries.corki.harass.useR") && this.spells[SpellSlot.R].IsReady())
            {
                var target = TargetSelector.GetTarget(
                    this.spells[SpellSlot.R].Range, 
                    TargetSelector.DamageType.Physical);
                var prediction = this.spells[SpellSlot.R].GetPrediction(target);

                if (target.IsValidTarget(this.spells[SpellSlot.R].Range) && prediction.Hitchance >= this.GetHitchance())
                {
                    this.spells[SpellSlot.R].Cast(prediction.CastPosition);
                }
            }
        }

        /// <summary>
        ///     <c>OnLaneclear</c> subscribed orb walker function.
        /// </summary>
        public override void OnLaneclear()
        {
            if (this.GetItemValue<bool>("com.iseries.corki.laneclear.useQ") && this.spells[SpellSlot.Q].IsReady())
            {
                var minions = MinionManager.GetMinions(this.spells[SpellSlot.Q].Range).ToList();
                var farmLocation = this.spells[SpellSlot.Q].GetCircularFarmLocation(minions);

                if (farmLocation.MinionsHit >= this.GetItemValue<Slider>("com.iseries.corki.laneclear.qMinions").Value)
                {
                    this.spells[SpellSlot.Q].Cast(farmLocation.Position);
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
            double damage = 0f;

            if (this.spells[SpellSlot.Q].IsReady())
            {
                damage += this.spells[SpellSlot.Q].GetDamage(target);
            }

            if (this.spells[SpellSlot.R].IsReady())
            {
                damage += this.spells[SpellSlot.R].GetDamage(target);
            }

            return (float)damage;
        }

        /// <summary>
        ///     The Code to always check and execute
        /// </summary>
        private void OnUpdateFunctions()
        {
            this.spells[SpellSlot.R].Range =
                ObjectManager.Player.Buffs.Any(x => x.Name.Contains("corkimissilebarragecounterbig")) ? 1500f : 1300f;
            this.AutoHarass();
            foreach (var hero in
                HeroManager.Enemies.Where(
                    x =>
                    x.IsValidTarget(this.spells[SpellSlot.Q].Range) && this.spells[SpellSlot.Q].GetDamage(x) > x.Health)
                    .Where(hero => this.spells[SpellSlot.Q].IsReady()))
            {
                this.spells[SpellSlot.Q].Cast(hero);
            }
        }

        #endregion
    }
}