// --------------------------------------------------------------------------------------------------------------------
// <copyright file="iDZLucian.cs" company="LeagueSharp">
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
//   TODO The i dz lucian.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iDzLucian
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using global::iDzLucian.cleansing;
    using global::iDzLucian.Helpers;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    // ReSharper disable once InconsistentNaming
    /// <summary>
    ///     TODO The i dz lucian.
    /// </summary>
    internal class iDzLucian
    {
        #region Static Fields

        /// <summary>
        ///     The Menu
        /// </summary>
        public static Menu Menu;

        // Do not resharp _spells name, tyvm mkkk :3
        // ReSharper disable once InconsistentNaming
        /// <summary>
        ///     The Spells
        /// </summary>
        private static readonly Dictionary<SpellSlot, Spell> Spells = new Dictionary<SpellSlot, Spell>
                                                                          {
                                                                              {
                                                                                  SpellSlot.Q, 
                                                                                  new Spell(SpellSlot.Q, 675f)
                                                                              }, 
                                                                              {
                                                                                  SpellSlot.W, 
                                                                                  new Spell(SpellSlot.W, 1000f)
                                                                              }, 
                                                                              {
                                                                                  SpellSlot.E, 
                                                                                  new Spell(SpellSlot.E, 425f)
                                                                              }, 
                                                                              {
                                                                                  SpellSlot.R, 
                                                                                  new Spell(SpellSlot.R, 1400f)
                                                                              }
                                                                          };

        /// <summary>
        ///     The Orbwalker
        /// </summary>
        private static Orbwalking.Orbwalker orbwalker;

        /// <summary>
        ///     The Player
        /// </summary>
        private static Obj_AI_Hero player;

        /// <summary>
        ///     The Extended Spell
        /// </summary>
        private static Spell qExtended;

        /// <summary>
        ///     The Passive Check
        /// </summary>
        private static bool shouldHavePassive;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The Loading Method kappa
        /// </summary>
        /// <param name="args">
        ///     The Event Arguments
        /// </param>
        public static void OnLoad(EventArgs args)
        {
            player = ObjectManager.Player;

            if (player.ChampionName != "Lucian")
            {
                return;
            }

            LoadSpells();
            CreateMenu();
            Notifications.AddNotification(
                new Notification("iDZLucian v" + Assembly.GetExecutingAssembly().GetName().Version + " loaded!", 2500));
            Game.PrintChat("iDZLucian v" + Assembly.GetExecutingAssembly().GetName().Version + " loaded!");
            Game.OnUpdate += OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;
            AntiGapcloser.OnEnemyGapcloser += OnGapcloser;
            Drawing.OnDraw += OnDraw;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The Combo
        /// </summary>
        private static void Combo()
        {
            ExtendedQ(Mode.Combo);
        }

        /// <summary>
        ///     TODO The create menu.
        /// </summary>
        private static void CreateMenu()
        {
            Menu = new Menu("iDzLucian", "com.idzlucian", true);

            var orbMenu = new Menu("Lucian - Orbwalker", "com.idzlucian.orbwalker");
            orbwalker = new Orbwalking.Orbwalker(orbMenu);
            Menu.AddSubMenu(orbMenu);

            var tsMenu = new Menu("Lucian - Target Selector", "com.idzlucian.ts");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            var comboMenu = new Menu("Lucian - Combo", "com.idzlucian.combo");
            comboMenu.AddModeMenu(
                Mode.Combo, 
                new[] { SpellSlot.Q, SpellSlot.W, SpellSlot.E }, 
                new[] { true, true, false, false });
            comboMenu.AddManaManager(Mode.Combo, new[] { SpellSlot.Q, SpellSlot.W, SpellSlot.E }, new[] { 35, 35, 25 });

            var skillOptionsCombo = new Menu("Skill Options", "com.idzlucian.combo.skilloptions");
            {
                skillOptionsCombo.AddItem(
                    new MenuItem("com.idzlucian.skilloptions.weave", "Spell Weaving").SetValue(true));
                skillOptionsCombo.AddItem(
                    new MenuItem("com.idzlucian.combo.useextendedq", "Use Extended Q Combo").SetValue(true));
                skillOptionsCombo.AddItem(
                    new MenuItem("com.idzlucian.harass.useextendedq", "Use Extended Q Harass").SetValue(true));
            }

            comboMenu.AddSubMenu(skillOptionsCombo);

            Menu.AddSubMenu(comboMenu);

            var harassMenu = new Menu("Lucian - Harass", "com.idzlucian.harass");
            harassMenu.AddModeMenu(Mode.Harass, new[] { SpellSlot.Q, SpellSlot.W }, new[] { true, true });
            harassMenu.AddManaManager(Mode.Harass, new[] { SpellSlot.Q, SpellSlot.W }, new[] { 35, 35 });

            // harassMenu.AddItem(
            // new MenuItem("com.idzlucian.harass.useextendedq", "Use Extended Q Harass").SetValue(true));
            Menu.AddSubMenu(harassMenu);

            var farmMenu = new Menu("Lucian - Farm", "com.idzlucian.farm");
            farmMenu.AddModeMenu(Mode.Laneclear, new[] { SpellSlot.Q }, new[] { true });
            farmMenu.AddManaManager(Mode.Laneclear, new[] { SpellSlot.Q }, new[] { 35 });
            var farmOptions = new Menu("Farm Options", "com.idzlucian.farm.farm");
            {
                farmOptions.AddItem(
                    new MenuItem("com.idzlucian.farm.q.lc.minhit", "Min Minions for Q LC").SetValue(new Slider(2, 1, 6)));
            }

            farmMenu.AddSubMenu(farmOptions);

            Menu.AddSubMenu(farmMenu);

            var miscMenu = new Menu("Lucian - Misc", "com.idzlucian.misc");
            {
                miscMenu.AddHitChanceSelector();
                miscMenu.AddItem(
                    new MenuItem("com.idzlucian.misc.antigpe", "Use E Against enemy gapclosers").SetValue(false));
                miscMenu.AddItem(new MenuItem("com.idzlucian.misc.debug", "Debug").SetValue(false));
            }

            Menu.AddSubMenu(miscMenu);
            Cleanser.OnLoad();

            Menu.AddToMainMenu();
        }

        /// <summary>
        ///     Goes ham on a target using a minion for collision, and finishing the target with an extended Q
        /// </summary>
        private static void DashKillsteal()
        {
            // TODO test this, remains untesed due to my high ping. cmon dz embaress me
            var minions = MinionManager.GetMinions(player.ServerPosition, Spells[SpellSlot.Q].Range);
            var extendedQTarget = TargetSelector.GetTarget(qExtended.Range, TargetSelector.DamageType.Physical);

            if (extendedQTarget == null || !extendedQTarget.IsValidTarget(qExtended.Range)
                || !Spells[SpellSlot.Q].IsReady() || !Spells[SpellSlot.E].IsReady())
            {
                return;
            }

            foreach (var selectedMinion in
                minions.Where(minion => Spells[SpellSlot.Q].IsInRange(minion) && qExtended.IsInRange(extendedQTarget)))
            {
                var bestPosition = qExtended.GetPrediction(extendedQTarget, true).CastPosition.To2D();
                var collisionObjects = qExtended.GetCollision(
                    selectedMinion.Position.To2D(), 
                    new List<Vector2> { bestPosition }); // FROM e endPositiono

                if (Spells[SpellSlot.E].IsInRange(bestPosition) && bestPosition != player.Position.To2D())
                {
                    Spells[SpellSlot.E].Cast(bestPosition);
                }

                if (Spells[SpellSlot.Q].IsReady() && collisionObjects.Any())
                {
                    Spells[SpellSlot.Q].CastOnUnit(selectedMinion);
                }
            }
        }

        /// <summary>
        ///     TODO The extended q.
        /// </summary>
        /// <param name="mode">
        ///     TODO The mode.
        /// </param>
        private static void ExtendedQ(Mode mode)
        {
            if (
                !MenuHelper.IsMenuEnabled(
                    "com.idzlucian." + MenuHelper.GetFullNameFromMode(mode).ToLowerInvariant() + ".useextendedq")
                || ObjectManager.Player.ManaPercent
                < MenuHelper.GetSliderValue(
                    "com.idzlucian.manamanager.qmana" + MenuHelper.GetStringFromMode(mode).ToLowerInvariant()))
            {
                return;
            }

            var target = TargetSelector.GetTarget(Spells[SpellSlot.Q].Range, TargetSelector.DamageType.Physical);
            var targetExtended = TargetSelector.GetTarget(qExtended.Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget(Spells[SpellSlot.Q].Range) || !targetExtended.IsValidTarget(qExtended.Range)
                || (HasPassive() && Orbwalking.InAutoAttackRange(target)))
            {
                return;
            }

            var targetPrediction = qExtended.GetPrediction(targetExtended).UnitPosition.To2D();
            var minions = MinionManager.GetMinions(
                player.ServerPosition, 
                Spells[SpellSlot.Q].Range, 
                MinionTypes.All, 
                MinionTeam.NotAlly);

            if (!minions.Any() || !targetExtended.IsMoving)
            {
                return;
            }

            // Credits xSalice
            foreach (var minion in minions)
            {
                var polygon = new Geometry.Polygon.Rectangle(
                    player.ServerPosition, 
                    player.ServerPosition.Extend(minion.ServerPosition, qExtended.Range), 
                    qExtended.Width);

                if (polygon.IsInside(targetPrediction)
                    && Spells[SpellSlot.Q].Cast(minion) == Spell.CastStates.SuccessfullyCasted)
                {
                    Spells[SpellSlot.Q].LastCastAttemptT = Environment.TickCount;
                }
            }
        }

        /// <summary>
        ///     TODO The farm.
        /// </summary>
        private static void Farm()
        {
            var allMinions = MinionManager.GetMinions(player.ServerPosition, Spells[SpellSlot.Q].Range);
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.LaneClear:
                    var minionFarmLocation = Spells[SpellSlot.Q].GetCircularFarmLocation(allMinions, 60);
                    if (minionFarmLocation.MinionsHit >= MenuHelper.GetSliderValue("com.idzlucian.farm.q.lc.minhit"))
                    {
                        var minionC =
                            allMinions.FindAll(m => m.Distance(minionFarmLocation.Position) <= 60)
                                .OrderBy(m => m.Distance(minionFarmLocation.Position));
                        if (!minionC.Any())
                        {
                            return;
                        }

                        var minion = minionC.First(m => m.IsValidTarget());
                        if (minion.IsValidTarget())
                        {
                            if (Spells[SpellSlot.Q].IsEnabledAndReady(Mode.Laneclear) && !HasPassive()
                                && Spells[SpellSlot.Q].CanCast(minion) && Orbwalking.InAutoAttackRange(minion))
                            {
                                Spells[SpellSlot.Q].CastOnUnit(minion);
                                Spells[SpellSlot.Q].LastCastAttemptT = Environment.TickCount;
                            }
                        }
                    }

                    break;
            }
        }

        /// <summary>
        ///     TODO The get culling damage.
        /// </summary>
        /// <param name="target">
        ///     TODO The target.
        /// </param>
        /// <returns>
        ///     <see cref="double" />
        /// </returns>
        private static double GetCullingDamage(Obj_AI_Hero target)
        {
            var level = Spells[SpellSlot.R].Level;
            return
                (float)
                (player.GetSpellDamage(target, SpellSlot.Q)
                 * (level == 1
                        ? 7.5 + 7.5 * (player.AttackSpeedMod - .6) / 1.4
                        : level == 2
                              ? 7.5 + 9 * (player.AttackSpeedMod - .6) / 1.4
                              : level == 3 ? 7.5 + 10.5 * (player.AttackSpeedMod - .6) : 0));
        }

        /// <summary>
        ///     TODO The harass.
        /// </summary>
        private static void Harass()
        {
            // TODO needs testing, its basically just the same as combo imo
            var target = TargetSelector.GetTarget(Spells[SpellSlot.Q].Range, TargetSelector.DamageType.Physical);

            ExtendedQ(Mode.Harass);

            if (target.IsValidTarget(Spells[SpellSlot.Q].Range))
            {
                if (Spells[SpellSlot.Q].IsEnabledAndReady(Mode.Harass))
                {
                    if (Spells[SpellSlot.Q].CanCast(target) && !HasPassive() && Orbwalking.InAutoAttackRange(target))
                    {
                        Spells[SpellSlot.Q].CastOnUnit(target);
                        Spells[SpellSlot.Q].LastCastAttemptT = Environment.TickCount;
                    }
                }

                if (Spells[SpellSlot.W].IsEnabledAndReady(Mode.Harass) && Spells[SpellSlot.W].CanCast(target)
                    && !HasPassive() && !Spells[SpellSlot.Q].CanCast(target)
                    && !Spells[SpellSlot.Q].IsEnabledAndReady(Mode.Harass) && Orbwalking.InAutoAttackRange(target))
                {
                    Spells[SpellSlot.W].CastIfHitchanceEquals(target, MenuHelper.GetHitchance());
                    Spells[SpellSlot.W].LastCastAttemptT = Environment.TickCount;
                }
            }
        }

        /// <summary>
        ///     Checks if we have the lucian buff passive.
        /// </summary>
        /// <returns>
        ///     true / false
        /// </returns>
        private static bool HasPassive()
        {
            if (!MenuHelper.IsMenuEnabled("com.idzlucian.skilloptions.weave"))
            {
                return false;
            }

            return shouldHavePassive || ObjectManager.Player.HasBuff("LucianPassiveBuff")
                   || (Environment.TickCount - Spells[SpellSlot.Q].LastCastAttemptT < 500
                       || Environment.TickCount - Spells[SpellSlot.W].LastCastAttemptT < 500
                       || Environment.TickCount - Spells[SpellSlot.E].LastCastAttemptT < 500);
        }

        /// <summary>
        ///     Loads the spells and sets the skill shots if needed
        /// </summary>
        private static void LoadSpells()
        {
            Spells[SpellSlot.Q].SetTargetted(0.25f, float.MaxValue);
            qExtended = new Spell(SpellSlot.Q, 1100);
            qExtended.SetSkillshot(0.25f, 5f, float.MaxValue, true, SkillshotType.SkillshotLine);
            Spells[SpellSlot.W].SetSkillshot(0.3f, 80, 1600, true, SkillshotType.SkillshotLine);
            Spells[SpellSlot.E].SetSkillshot(.25f, 1f, float.MaxValue, false, SkillshotType.SkillshotLine);
            Spells[SpellSlot.R].SetSkillshot(.1f, 110, 2800, true, SkillshotType.SkillshotLine);
        }

        /// <summary>
        ///     The On Process Spell casting
        /// </summary>
        /// <param name="sender">
        ///     The <see cref="Spell" /> sender
        /// </param>
        /// <param name="args">
        ///     The <see cref="Spell" /> arguments
        /// </param>
        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                switch (args.SData.Name)
                {
                    case "LucianQ":
                        Utility.DelayAction.Add(
                            (int)(Math.Ceiling(Game.Ping / 2f) + 250 + 325), 
                            Orbwalking.ResetAutoAttackTimer);
                        Spells[SpellSlot.Q].LastCastAttemptT = Environment.TickCount;
                        break;
                    case "LucianW":
                        Utility.DelayAction.Add(
                            (int)(Math.Ceiling(Game.Ping / 2f) + 250 + 325), 
                            Orbwalking.ResetAutoAttackTimer);
                        Spells[SpellSlot.W].LastCastAttemptT = Environment.TickCount;
                        break;
                    case "LucianE":
                        Utility.DelayAction.Add(
                            (int)(Math.Ceiling(Game.Ping / 2f) + 250 + 325), 
                            Orbwalking.ResetAutoAttackTimer);
                        Spells[SpellSlot.E].LastCastAttemptT = Environment.TickCount;
                        break;
                }

                // Console.WriteLine(args.SData.Name);
            }
        }

        /// <summary>
        ///     TODO The on draw.
        /// </summary>
        /// <param name="args">
        ///     TODO The args.
        /// </param>
        private static void OnDraw(EventArgs args)
        {
        }

        /// <summary>
        ///     TODO The on game update.
        /// </summary>
        /// <param name="args">
        ///     TODO The args.
        /// </param>
        private static void OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            foreach (
                var killTarget in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            x =>
                            player.Distance(x) <= Spells[SpellSlot.Q].Range + x.BoundingRadius
                            && Spells[SpellSlot.Q].GetDamage(x) > x.Health + 10)
                        .Where(killTarget => Spells[SpellSlot.Q].IsReady() && Spells[SpellSlot.Q].CanCast(killTarget)))
            {
                Spells[SpellSlot.Q].CastOnUnit(killTarget);
            }

            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Farm();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farm();
                    break;
            }
        }

        /// <summary>
        ///     Gap closer event
        /// </summary>
        /// <param name="gapcloser">
        ///     The Incoming Gap Closer
        /// </param>
        private static void OnGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuHelper.IsMenuEnabled("com.idzlucian.misc.antigpe") && Spells[SpellSlot.E].IsReady())
            {
                var extended = gapcloser.Start.Extend(
                    player.Position, 
                    gapcloser.Start.Distance(player.ServerPosition) + Spells[SpellSlot.E].Range);
                if (PositionHelper.IsSafePosition(extended))
                {
                    Spells[SpellSlot.E].Cast(extended);
                    Spells[SpellSlot.E].LastCastAttemptT = Environment.TickCount;
                }
            }
        }

        /// <summary>
        ///     the after attack event
        /// </summary>
        /// <param name="unit">
        ///     The <see cref="AttackableUnit" /> unit
        /// </param>
        /// <param name="attackableTarget">
        ///     The <see cref="AttackableUnit" /> target
        /// </param>
        private static void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit attackableTarget)
        {
            if (!unit.IsMe)
            {
                return;
            }

            shouldHavePassive = false;
            var target = attackableTarget as Obj_AI_Hero;

            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (target.IsValidTarget(Spells[SpellSlot.Q].Range) && target != null)
                    {
                        if (Spells[SpellSlot.Q].IsEnabledAndReady(Mode.Combo) && Spells[SpellSlot.Q].IsInRange(target) && !HasPassive())
                        {
                            Spells[SpellSlot.Q].CastOnUnit(target);
                            Spells[SpellSlot.Q].LastCastAttemptT = Environment.TickCount;
                        }
                    }

                    if (target.IsValidTarget(Spells[SpellSlot.W].Range) && target != null)
                    {
                        if (Spells[SpellSlot.W].IsEnabledAndReady(Mode.Combo) && Spells[SpellSlot.W].IsInRange(target) && !HasPassive())
                        {
                            Spells[SpellSlot.W].CastOnUnit(target);
                            Spells[SpellSlot.W].LastCastAttemptT = Environment.TickCount;
                        }
                    }

                    if (target != null)
                    {
                        if (!Spells[SpellSlot.E].IsEnabledAndReady(Mode.Combo))
                        {
                            return;
                        }

                        var hypotheticalPosition = ObjectManager.Player.ServerPosition.Extend(
                            Game.CursorPos, 
                            Spells[SpellSlot.E].Range);
                        if (ObjectManager.Player.HealthPercent <= 30
                            && target.HealthPercent >= ObjectManager.Player.HealthPercent)
                        {
                            if (ObjectManager.Player.Position.Distance(ObjectManager.Player.ServerPosition) >= 35
                                && target.Distance(ObjectManager.Player.ServerPosition)
                                < target.Distance(ObjectManager.Player.Position)
                                && PositionHelper.IsSafePosition(hypotheticalPosition))
                            {
                                Spells[SpellSlot.E].Cast(hypotheticalPosition);
                                Spells[SpellSlot.E].LastCastAttemptT = Environment.TickCount;
                            }
                        }

                        if (PositionHelper.IsSafePosition(hypotheticalPosition)
                            && hypotheticalPosition.Distance(target.ServerPosition)
                            <= Orbwalking.GetRealAutoAttackRange(null)
                            && (!Spells[SpellSlot.Q].IsEnabledAndReady(Mode.Combo)
                                || !Spells[SpellSlot.Q].CanCast(target))
                            && (!Spells[SpellSlot.W].IsEnabledAndReady(Mode.Combo)
                                || !Spells[SpellSlot.W].CanCast(target)
                                && (hypotheticalPosition.Distance(target.ServerPosition) > 400) && !HasPassive()))
                        {
                            Spells[SpellSlot.E].Cast(hypotheticalPosition);
                            Spells[SpellSlot.E].LastCastAttemptT = Environment.TickCount;
                        }
                    }
                    break;
            }
        }

        #endregion
    }
}