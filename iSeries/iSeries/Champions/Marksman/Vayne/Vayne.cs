// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Vayne.cs" company="LeagueSharp">
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
// Vayne!
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using iSeries.Champions.Utilities;
using LeagueSharp;
using LeagueSharp.Common;

namespace iSeries.Champions.Marksman.Vayne
{
    using System;

    using iSeries.General;

    /// <summary>
    ///     The Champion Class
    /// </summary>
    internal class Vayne : Champion
    {
        #region Fields

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
                                                                   {
                                                                        { SpellSlot.Q, new Spell(SpellSlot.Q) },
                                                                        { SpellSlot.W, new Spell(SpellSlot.W) },
                                                                        { SpellSlot.E, new Spell(SpellSlot.E, 590f) },
                                                                        { SpellSlot.R, new Spell(SpellSlot.R) }
                                                                   };

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Initializes a new instance of the <see cref="Vayne"/> class.
        /// </summary>
        public Vayne()
        {
            this.CreateMenu = MenuGenerator.Generate;

            spells[SpellSlot.E].SetTargetted(0.25f, 2000f);

            Orbwalking.AfterAttack += Orbwalking_AfterAttack;

            AntiGapcloser.OnEnemyGapcloser += gapcloser =>
            {
                if (GetItemValue<bool>("com.iseries.vayne.misc.gapcloser"))
                {
                      if (gapcloser.Sender.IsValidTarget(spells[SpellSlot.E].Range) && gapcloser.End.Distance(ObjectManager.Player.ServerPosition) <= 400f)
                       {
                            spells[SpellSlot.E].Cast(gapcloser.Sender);
                       }
                }
            };

            Interrupter2.OnInterruptableTarget += (sender, args) =>
            {
                if (GetItemValue<bool>("dz191.vhr.misc.general.interrupt"))
                {
                    if (args.DangerLevel == Interrupter2.DangerLevel.High && sender.IsValidTarget(spells[SpellSlot.E].Range))
                    {
                        spells[SpellSlot.E].Cast(sender);
                    }
                }
            };
        }

        public void Orbwalking_AfterAttack(LeagueSharp.AttackableUnit unit, LeagueSharp.AttackableUnit target)
        {
            //com.iseries.vayne.combo.useQ"
            if (!spells[SpellSlot.Q].IsReady() || !(target is Obj_AI_Hero) || !(target.IsValidTarget()))
            {
                return;
            }
            var orbwalkerTarget = (Obj_AI_Hero) target;
            var tumbleEnd = ObjectManager.Player.ServerPosition.Extend(Game.CursorPos, spells[SpellSlot.Q].Range);

            switch (Variables.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (GetItemValue<bool>("com.iseries.vayne.combo.useQ") && PositionHelper.IsSafePosition(tumbleEnd) && tumbleEnd.Distance(orbwalkerTarget.ServerPosition) <= 570f)
                    {
                        spells[SpellSlot.Q].Cast(tumbleEnd);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (GetItemValue<bool>("com.iseries.vayne.harass.useQ") && PositionHelper.IsSafePosition(tumbleEnd) && tumbleEnd.Distance(orbwalkerTarget.ServerPosition) <= 570f)
                    {
                        spells[SpellSlot.Q].Cast(tumbleEnd);
                        Utility.DelayAction.Add((int)(Game.Ping/2f + 250 + 325), Orbwalking.ResetAutoAttackTimer);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (spells[SpellSlot.Q].IsReady() && GetItemValue<bool>("com.iseries.vayne.laneclear.useQ"))
                    {
                        var minionsInRange =
                            MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Player.AttackRange)
                                .FindAll(
                                    m => m.Health <= Player.GetAutoAttackDamage(m) + spells[SpellSlot.Q].GetDamage(m))
                                .ToList();
                        if (minionsInRange.Any())
                        {
                            if (minionsInRange.Count > 1)
                            {
                                var firstMinion = minionsInRange.OrderBy(m => m.HealthPercent).First();
                                var endPosition = ObjectManager.Player.ServerPosition.Extend(
                                    firstMinion.ServerPosition, spells[SpellSlot.Q].Range);
                                if (PositionHelper.IsSafePosition(endPosition))
                                {
                                    spells[SpellSlot.Q].Cast(firstMinion.ServerPosition);
                                    Variables.Orbwalker.ForceTarget(firstMinion);
                                }
                            }
                        }
                    }
                    break;
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
        ///     <c>OnCombo</c> subscribed orb walker function.
        /// </summary>
        public override void OnCombo()
        {
            if (!spells[SpellSlot.E].IsReady() || !GetItemValue<bool>("com.iseries.vayne.combo.useE"))
            {
                return;
            }

            foreach (var target in HeroManager.Enemies.Where(h => h.IsValidTarget(spells[SpellSlot.E].Range) 
                && !h.HasBuffOfType(BuffType.SpellShield) 
                && !h.HasBuffOfType(BuffType.SpellImmunity)
                && !GetItemValue<bool>("com.iseries.vayne.noe." + h.ChampionName.ToLowerInvariant())))
            {
                var pushDistance = 400;
                var targetPosition = target.ServerPosition;
                var endPosition = targetPosition.Extend(ObjectManager.Player.ServerPosition, -pushDistance);
                for (var i = 0; i < pushDistance; i += (int)target.BoundingRadius)
                {
                    var extendedPosition = targetPosition.Extend(ObjectManager.Player.ServerPosition, -i);
                    if ((extendedPosition.IsWall()) || endPosition.IsWall())
                    {
                        spells[SpellSlot.E].Cast(target);
                        break;
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
            if (GetItemValue<bool>("com.iseries.twitch.vayne.drawE"))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position,spells[SpellSlot.E].Range,Color.Red);
            }
        }

        /// <summary>
        ///     <c>OnHarass</c> subscribed orb walker function.
        /// </summary>
        public override void OnHarass()
        {
            if (!spells[SpellSlot.E].IsReady() || !GetItemValue<bool>("com.iseries.vayne.harass.useE"))
            {
                return;
            }

            foreach (var target in HeroManager.Enemies.Where(h => h.IsValidTarget(spells[SpellSlot.E].Range) 
                && !h.HasBuffOfType(BuffType.SpellShield) 
                && !h.HasBuffOfType(BuffType.SpellImmunity) 
                && !GetItemValue<bool>("com.iseries.vayne.noe." + h.ChampionName.ToLowerInvariant())))
            {
                var pushDistance = 400;
                var targetPosition = target.ServerPosition;
                var endPosition = targetPosition.Extend(ObjectManager.Player.ServerPosition, -pushDistance);
                for (var i = 0; i < pushDistance; i += (int)target.BoundingRadius)
                {
                    var extendedPosition = targetPosition.Extend(ObjectManager.Player.ServerPosition, -i);
                    if ((extendedPosition.IsWall()) || endPosition.IsWall())
                    {
                        spells[SpellSlot.E].Cast(target);
                        break;
                    }
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
            
        }

        #endregion
    }
}