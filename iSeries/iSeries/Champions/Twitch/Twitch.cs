// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Twitch.cs" company="LeagueSharp">
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
//   TODO The twitch.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iSeries.Champions.Twitch
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using iSeries.Champions.Utilities;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    ///     TODO The twitch.
    /// </summary>
    internal class Twitch : Champion
    {
        #region Fields

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
                                                                   {
                                                                       { SpellSlot.W, new Spell(SpellSlot.W, 950) }, 
                                                                       { SpellSlot.E, new Spell(SpellSlot.E, 1200) }
                                                                   };

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Twitch"/> class. 
        ///     Initializes a new instance of the <see cref="Kalista"/> class.
        /// </summary>
        public Twitch()
        {
            // Menu Generation
            this.CreateMenu = MenuGenerator.Generate;

            // Spell initialization
            this.spells[SpellSlot.W].SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotCircle);

            // Damage Indicator
            DamageIndicator.DamageToUnit = this.GetDamage;
            DamageIndicator.Enabled = true;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     <c>OnCombo</c> subscribed orb walker function.
        /// </summary>
        public override void OnCombo()
        {
            if (this.GetItemValue<bool>("com.iseries.twitch.combo.useE") && this.spells[SpellSlot.E].IsReady())
            {
                var killableTarget =
                    HeroManager.Enemies.FirstOrDefault(
                        x =>
                        x.IsValidTarget(this.spells[SpellSlot.E].Range) && this.spells[SpellSlot.E].IsInRange(x)
                        && this.GetDamage(x) > x.Health);
                if (killableTarget != null)
                {
                    this.spells[SpellSlot.E].Cast();
                }
            }

            if (this.GetItemValue<bool>("com.iseries.twitch.combo.useW") && this.spells[SpellSlot.W].IsReady())
            {
                var wTarget = TargetSelector.GetTarget(
                    this.spells[SpellSlot.W].Range, 
                    TargetSelector.DamageType.Physical);
                if (wTarget.IsValidTarget(this.spells[SpellSlot.W].Range))
                {
                    this.spells[SpellSlot.W].Cast(wTarget);
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
            Render.Circle.DrawCircle(this.Player.Position, this.spells[SpellSlot.E].Range, Color.DarkRed);
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
        ///     Gets the total E Damage
        /// </summary>
        /// <param name="hero">
        ///     The hero
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        private float GetDamage(Obj_AI_Hero hero)
        {
            float damage = 0;

            if (this.spells[SpellSlot.E].IsReady())
            {
                damage += this.spells[SpellSlot.E].GetDamage(hero);
            }

            return damage;
        }

        /// <summary>
        ///     The Functions to always process
        /// </summary>
        private void OnUpdateFunctions()
        {
            if (this.GetItemValue<bool>("com.iseries.twitch.misc.killsteal") && this.spells[SpellSlot.E].IsReady())
            {
                foreach (
                    var hero in
                        HeroManager.Enemies.Where(
                            x => x.IsValidTarget(this.spells[SpellSlot.E].Range) && this.GetDamage(x) > x.Health))
                {
                    this.spells[SpellSlot.E].Cast();
                }
            }

            if (this.GetItemValue<bool>("com.iseries.twitch.misc.mobsteal") && this.spells[SpellSlot.E].IsReady())
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