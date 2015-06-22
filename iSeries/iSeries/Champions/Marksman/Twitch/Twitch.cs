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
namespace iSeries.Champions.Marksman.Twitch
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using iSeries.Champions.Utilities;
    using iSeries.General;

    using LeagueSharp;
    using LeagueSharp.Common;
    using LeagueSharp.Common.Data;

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
                                                                       { SpellSlot.W, new Spell(SpellSlot.W, 950f) }, 
                                                                       { SpellSlot.E, new Spell(SpellSlot.E, 1200f) }
                                                                   };

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Twitch" /> class.
        ///     Initializes a new instance of the <see cref="Kalista" /> class.
        /// </summary>
        public Twitch()
        {
            // Menu Generation
            this.CreateMenu = MenuGenerator.Generate;

            // Spell initialization
            this.spells[SpellSlot.W].SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotCircle);

            // Damage Indicator
            DamageIndicator.DamageToUnit = this.GetActualDamage;
            DamageIndicator.Enabled = true;

            Spellbook.OnCastSpell += this.OnCastSpell;
        }

        /// <summary>
        ///     The on cast spell stuff
        /// </summary>
        /// <param name="sender">
        ///     The Sender
        /// </param>
        /// <param name="args">
        ///     The Args
        /// </param>
        private void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R)
            {
                if (ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                {
                    ItemData.Youmuus_Ghostblade.GetItem().Cast();
                }
            }
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
        ///     Gets actual damage blah blah
        /// </summary>
        /// <param name="target">
        ///     The target
        /// </param>
        /// <returns>
        ///     The <see cref="float"/>.
        /// </returns>
        private float GetActualDamage(Obj_AI_Base target)
        {
            if (target.HasBuff("ColossalStrength"))
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
        ///     <c>OnCombo</c> subscribed orb walker function.
        /// </summary>
        public override void OnCombo()
        {
            if (this.GetItemValue<bool>("com.iseries.twitch.combo.useEKillable") && this.spells[SpellSlot.E].IsReady())
            {
                var killableTarget =
                    HeroManager.Enemies.FirstOrDefault(
                        x =>
                        x.IsValidTarget(this.spells[SpellSlot.E].Range) && this.spells[SpellSlot.E].IsInRange(x)
                        && this.GetActualDamage(x) > this.GetActualHealth(x) && !this.GetItemValue<bool>("com.iseries.twitch.noe."+x.ChampionName.ToLowerInvariant()));
                if (killableTarget != null)
                {
                    this.spells[SpellSlot.E].Cast();
                }
            }

            if (this.GetItemValue<bool>("com.iseries.twitch.combo.useEMaxStacks") && this.spells[SpellSlot.E].IsReady())
            {
                var killableTarget =
                    HeroManager.Enemies.FirstOrDefault(
                        x =>
                        x.IsValidTarget(this.spells[SpellSlot.E].Range) && this.spells[SpellSlot.E].IsInRange(x)
                        && x.GetBuffCount("twitchdeadlyvenom") == 6 && !this.GetItemValue<bool>("com.iseries.twitch.noe." + x.ChampionName.ToLowerInvariant()));
                if (killableTarget != null)
                {
                    this.spells[SpellSlot.E].Cast();
                }
            }

            if (this.GetItemValue<bool>("com.iseries.twitch.combo.useENearlyOutOfRange") && this.spells[SpellSlot.E].IsReady())
            {
                var killableTarget =
                    HeroManager.Enemies.FirstOrDefault(
                        x =>
                        x.IsValidTarget(this.spells[SpellSlot.E].Range) && this.spells[SpellSlot.E].IsInRange(x)
                        && x.GetBuffCount("twitchdeadlyvenom") >= 4 && x.Distance(ObjectManager.Player) >= 1000f && !this.GetItemValue<bool>("com.iseries.twitch.noe." + x.ChampionName.ToLowerInvariant()));
                if (killableTarget != null)
                {
                    this.spells[SpellSlot.E].Cast();
                }

            }

            if (this.GetItemValue<bool>("com.iseries.twitch.combo.useW") && this.spells[SpellSlot.W].IsReady())
            {
                if (this.Player.ManaPercent < this.GetItemValue<Slider>("com.iseries.twitch.combo.wMana").Value)
                {
                    return;
                }

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
            if (this.GetItemValue<bool>("com.iseries.twitch.drawing.drawE"))
            {
                Render.Circle.DrawCircle(this.Player.Position, this.spells[SpellSlot.E].Range, Color.DarkRed);
            }

            if (this.GetItemValue<bool>("com.iseries.twitch.drawing.drawStacks"))
            {
                foreach (var source in HeroManager.Enemies.Where(x => this.spells[SpellSlot.E].IsInRange(x)))
                {
                    var stacks = source.GetBuffCount("twitchdeadlyvenom");

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
        }

        /// <summary>
        ///     <c>OnHarass</c> subscribed orb walker function.
        /// </summary>
        public override void OnHarass()
        {
            //com.iseries.twitch.harass.eStacks
            if (GetItemValue<bool>("com.iseries.twitch.harass.useE") && spells[SpellSlot.E].IsReady())
            {
                    var target =
                        HeroManager.Enemies.FirstOrDefault(
                            x =>
                            x.IsValidTarget(this.spells[SpellSlot.E].Range) && this.spells[SpellSlot.E].IsInRange(x)
                            && x.GetBuffCount("twitchdeadlyvenom") >= GetItemValue<Slider>("com.iseries.twitch.harass.eStacks").Value && !this.GetItemValue<bool>("com.iseries.twitch.noe." + x.ChampionName.ToLowerInvariant()));
                    if (target != null)
                    {
                        this.spells[SpellSlot.E].Cast();
                    }
            }
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
                foreach (var hero in
                    HeroManager.Enemies.Where(
                        x => x.IsValidTarget(this.spells[SpellSlot.E].Range) && this.GetActualDamage(x) > this.GetActualHealth(x)))
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
                        MinionTeam.Neutral, 
                        MinionOrderTypes.MaxHealth)
                        .FirstOrDefault(
                            x =>
                            x.IsValid && x.Health + 5 <= this.spells[SpellSlot.E].GetDamage(x)
                            && !x.Name.Contains("Mini") && (x.Name.Contains("Dragon") || x.Name.Contains("Baron")));

                if (bigMinion != null && this.spells[SpellSlot.E].CanCast(bigMinion))
                {
                    this.spells[SpellSlot.E].Cast();
                }
            }
        }

        #endregion
    }
}