﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Kalista.cs" company="">
//   
// </copyright>
// <summary>
//   An Assembly for <see cref="Kalista" /> okay
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace IKalista
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Collision = LeagueSharp.Common.Collision;
    using Color = System.Drawing.Color;

    /// <summary>
    ///     An Assembly for <see cref="Kalista" /> okay
    /// </summary>
    public class Kalista
    {
        #region Fields

        /// <summary>
        ///     The Boolean link values
        /// </summary>
        private readonly Dictionary<string, MenuWrapper.BoolLink> boolLinks =
            new Dictionary<string, MenuWrapper.BoolLink>();

        /// <summary>
        ///     The Slider Link Values
        /// </summary>
        private readonly Dictionary<string, MenuWrapper.CircleLink> circleLinks =
            new Dictionary<string, MenuWrapper.CircleLink>();

        /// <summary>
        ///     The Key bind link values
        /// </summary>
        private readonly Dictionary<string, MenuWrapper.KeyBindLink> keyLinks =
            new Dictionary<string, MenuWrapper.KeyBindLink>();

        /// <summary>
        ///     The dictionary to store the current mode and the on orb walking event
        /// </summary>
        private readonly Dictionary<Orbwalking.OrbwalkingMode, OnOrbwalkingMode> orbwalkingModesDictionary;

        /// <summary>
        ///     The Slider Link values
        /// </summary>
        private readonly Dictionary<string, MenuWrapper.SliderLink> sliderLinks =
            new Dictionary<string, MenuWrapper.SliderLink>();

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>
                                                                   {
                                                                       { SpellSlot.Q, new Spell(SpellSlot.Q, 1150) }, 
                                                                       { SpellSlot.W, new Spell(SpellSlot.W, 5200) }, 
                                                                       { SpellSlot.E, new Spell(SpellSlot.E, 950) }, 
                                                                       { SpellSlot.R, new Spell(SpellSlot.R, 1200) }
                                                                   };

        /// <summary>
        ///     The StringList Link Values
        /// </summary>
        private readonly Dictionary<string, MenuWrapper.StringListLink> stringListLinks =
            new Dictionary<string, MenuWrapper.StringListLink>();

        /// <summary>
        ///     Calling the menu wrapper
        /// </summary>
        private MenuWrapper menu;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Kalista" /> class
        /// </summary>
        public Kalista()
        {
            this.orbwalkingModesDictionary = new Dictionary<Orbwalking.OrbwalkingMode, OnOrbwalkingMode>
                                                 {
                                                     { Orbwalking.OrbwalkingMode.Combo, this.OnCombo }, 
                                                     { Orbwalking.OrbwalkingMode.Mixed, this.OnHarass }, 
                                                     { Orbwalking.OrbwalkingMode.LastHit, this.OnLastHit }, 
                                                     { Orbwalking.OrbwalkingMode.LaneClear, this.OnLaneClear }, 
                                                     { Orbwalking.OrbwalkingMode.None, () => { } }
                                                 };

            this.InitMenu();
            this.InitSpells();
            this.InitEvents();
        }

        #endregion

        #region Delegates

        /// <summary>
        ///     The delegate for the On orb walking Event
        /// </summary>
        private delegate void OnOrbwalkingMode();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     TODO The show notification.
        /// </summary>
        /// <param name="message">
        ///     TODO The message.
        /// </param>
        /// <param name="colour">
        ///     TODO The color.
        /// </param>
        /// <param name="duration">
        ///     TODO The duration.
        /// </param>
        /// <param name="dispose">
        ///     TODO The dispose.
        /// </param>
        public static void ShowNotification(string message, Color colour, int duration = -1, bool dispose = true)
        {
            var notify = new Notification(message).SetTextColor(colour);
            Notifications.AddNotification(notify);
            if (dispose)
            {
                Utility.DelayAction.Add(duration, () => notify.Dispose());
            }
        }

        /// <summary>
        ///     TODO The has undying buff.
        /// </summary>
        /// <param name="target">
        ///     TODO The target.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool HasUndyingBuff(Obj_AI_Hero target)
        {
            // Tryndamere R
            if (target.ChampionName == "Tryndamere"
                && target.Buffs.Any(
                    b => b.Caster.NetworkId == target.NetworkId && b.IsValidBuff() && b.DisplayName == "Undying Rage"))
            {
                return true;
            }

            // Zilean R
            if (target.Buffs.Any(b => b.IsValidBuff() && b.DisplayName == "Chrono Shift"))
            {
                return true;
            }

            // Kayle R
            if (target.Buffs.Any(b => b.IsValidBuff() && b.DisplayName == "JudicatorIntervention"))
            {
                return true;
            }

            // Poppy R
            if (target.ChampionName == "Poppy")
            {
                if (
                    HeroManager.Allies.Any(
                        o =>
                        !o.IsMe
                        && o.Buffs.Any(
                            b =>
                            b.Caster.NetworkId == target.NetworkId && b.IsValidBuff()
                            && b.DisplayName == "PoppyDITarget")))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     This is where the magic happens, we like to steal other peoples stuff.
        /// </summary>
        private void DoMobSteal()
        {
            var minion =
                MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition, 
                    this.spells[SpellSlot.E].Range, 
                    MinionTypes.All, 
                    MinionTeam.Enemy, 
                    MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(
                        x =>
                        x.Health <= this.spells[SpellSlot.E].GetDamage(x)
                        && (x.SkinName.ToLower().Contains("siege") || x.SkinName.ToLower().Contains("super")));

            var bikMinion =
                MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition, 
                    this.spells[SpellSlot.E].Range, 
                    MinionTypes.All, 
                    MinionTeam.Neutral, 
                    MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.Health <= this.spells[SpellSlot.E].GetDamage(x));

            switch (this.stringListLinks["jungStealMode"].Value.SelectedIndex)
            {
                case 0:
                    if (bikMinion != null)
                    {
                        this.spells[SpellSlot.E].Cast();
                    }

                    break;
                case 1:
                    if (minion != null)
                    {
                        this.spells[SpellSlot.E].Cast();
                    }

                    break;
                case 2:
                    if (bikMinion != null || minion != null)
                    {
                        this.spells[SpellSlot.E].Cast();
                    }

                    break;
            }
        }

        /// <summary>
        ///     Gets the collision minions
        /// </summary>
        /// <param name="source">
        ///     the source
        /// </param>
        /// <param name="targetPosition">
        ///     the target position
        /// </param>
        /// <returns>
        ///     The list of minions
        /// </returns>
        private List<Obj_AI_Base> GetCollisionMinions(Obj_AI_Base source, Vector3 targetPosition)
        {
            var input = new PredictionInput
                            {
                                Unit = source, Radius = this.spells[SpellSlot.Q].Width, 
                                Delay = this.spells[SpellSlot.Q].Delay, Speed = this.spells[SpellSlot.Q].Speed, 
                            };

            input.CollisionObjects[0] = CollisionableObjects.Minions;

            return
                Collision.GetCollision(new List<Vector3> { targetPosition }, input)
                    .OrderBy(obj => obj.Distance(source))
                    .ToList();
        }

        /// <summary>
        ///     Gets the correct Rend Damage
        /// </summary>
        /// <param name="target">
        ///     The target
        /// </param>
        /// <returns>
        ///     The correct damage hopefully..
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1407:ArithmeticExpressionsMustDeclarePrecedence", Justification = "Reviewed. Suppression is OK here.")]
        private float GetEDamage(Obj_AI_Hero target)
        {
            var baseDamage = new[] { 20, 30, 40, 50, 60 };
            var additionalBaseDamage = new[] { 0.6, 0.6, 0.6, 0.6, 0.6 };

            var spearDamage = new[] { 5, 9, 14, 20, 27 };
            var additionalSpearDamage = new[] { 0.15, 0.18, 0.21, 0.24, 0.27 };

            var stacks =
                target.Buffs.Find(b => b.Caster.IsMe && b.IsValidBuff() && b.DisplayName == "KalistaExpungeMarker");

            if (!target.IsValidTarget(this.spells[SpellSlot.E].Range) || stacks.Count <= 0)
            {
                return 0;
            }

            var totalDamage = 
                (float) (baseDamage[this.spells[SpellSlot.E].Level - 1] + additionalBaseDamage[this.spells[SpellSlot.E].Level - 1] * ObjectManager.Player.TotalAttackDamage()) + (stacks.Count - 1) *
                (spearDamage[this.spells[SpellSlot.E].Level - 1]
                 + additionalSpearDamage[this.spells[SpellSlot.E].Level - 1]
                 * ObjectManager.Player.TotalAttackDamage());

            return (float)(100 / (100 + (target.Armor * ObjectManager.Player.PercentArmorPenetrationMod) - ObjectManager.Player.FlatArmorPenetrationMod) * totalDamage);
        }

        /// <summary>
        ///     Handles the grab
        /// </summary>
        private void HandleBalista()
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            var blitzcrank =
                HeroManager.Allies.SingleOrDefault(
                    x =>
                    x.IsAlly
                    && ObjectManager.Player.Distance(x.ServerPosition) < this.sliderLinks["maxRange"].Value.Value
                    && ObjectManager.Player.Distance(x.ServerPosition) >= this.sliderLinks["minRange"].Value.Value
                    && x.ChampionName == "Blitzcrank");

            if (blitzcrank == null)
            {
                return;
            }

            foreach (var target in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(enem => enem.IsValid && enem.IsEnemy && enem.Distance(ObjectManager.Player) <= 2450f))
            {
                if (this.boolLinks["disable" + target.ChampionName].Value || !this.spells[SpellSlot.R].IsReady()
                    || !this.boolLinks["useBalista"].Value)
                {
                    return;
                }

                if (target.Buffs != null && target.Health > 200 && blitzcrank.Distance(target) > 450f)
                {
                    for (var i = 0; i < target.Buffs.Count(); i++)
                    {
                        if (target.Buffs[i].Name == "rocketgrab2" && target.Buffs[i].IsActive)
                        {
                            this.spells[SpellSlot.R].Cast();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Handles the Sentinel trick
        /// </summary>
        private void HandleSentinels()
        {
            var baronPosition = new Vector3(4944, 10388, -712406f);
            var dragonPosition = new Vector3(9918f, 4474f, -71.2406f);

            if (!this.spells[SpellSlot.W].IsReady())
            {
                return;
            }

            if (this.keyLinks["sentBaron"].Value.Active
                && ObjectManager.Player.Distance(baronPosition) <= this.spells[SpellSlot.W].Range)
            {
                this.spells[SpellSlot.W].Cast(baronPosition);
            }
            else if (this.keyLinks["sentDragon"].Value.Active
                     && ObjectManager.Player.Distance(dragonPosition) <= this.spells[SpellSlot.W].Range)
            {
                this.spells[SpellSlot.W].Cast(dragonPosition);
            }
        }

        /// <summary>
        ///     Initialize all the events
        /// </summary>
        private void InitEvents()
        {
            Utility.HpBarDamageIndicator.DamageToUnit = this.GetEDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            CustomDamageIndicator.Initialize(this.GetEDamage);

            Game.OnUpdate += this.OnUpdate;

            /**
             *      Vi R  // NO
                    Morgana R
                    Sejuani R
                    Maokai W (if HP <15%)
                    Nautilus R // NOP
                    Ashe R
                    Riven R (if HP low)
                    Leona R
             */
            Obj_AI_Base.OnProcessSpellCast += this.OnProcessSpell;

            Orbwalking.OnNonKillableMinion += minion =>
                {
                    var killableMinion = minion as Obj_AI_Base;
                    if (killableMinion == null || !this.spells[SpellSlot.E].IsReady())
                    {
                        return;
                    }

                    if (this.boolLinks["qKillable"].Value && !killableMinion.HasBuff("KalistaExpungeMarker")
                        && this.spells[SpellSlot.Q].IsReady() && this.spells[SpellSlot.Q].CanCast(killableMinion)
                        && !ObjectManager.Player.IsWindingUp && !ObjectManager.Player.IsDashing())
                    {
                        this.spells[SpellSlot.Q].Cast(killableMinion);
                    }

                    if (this.boolLinks["eUnkillable"].Value
                        && this.spells[SpellSlot.E].GetDamage(killableMinion) > killableMinion.Health + 10
                        && this.spells[SpellSlot.E].CanCast(killableMinion)
                        && killableMinion.HasBuff("KalistaExpungeMarker"))
                    {
                        this.spells[SpellSlot.E].Cast();
                    }
                };
            Drawing.OnDraw += args =>
                {
                    foreach (
                        var link in this.circleLinks.Where(link => link.Value.Value.Active && link.Key != "drawEDamage")
                        )
                    {
                        Render.Circle.DrawCircle(
                            ObjectManager.Player.Position, 
                            link.Value.Value.Radius, 
                            link.Value.Value.Color);
                    }

                    CustomDamageIndicator.DrawingColor = this.circleLinks["drawEDamage"].Value.Color;
                    CustomDamageIndicator.Enabled = this.circleLinks["drawEDamage"].Value.Active;
                };
        }

        /// <summary>
        ///     The Initialization
        /// </summary>
        private void InitializeBalista()
        {
            if (!HeroManager.Allies.Any(x => x.IsAlly && !x.IsMe && x.ChampionName == "Blitzcrank"))
            {
                return;
            }

            var balistaMenu = this.menu.MainMenu.AddSubMenu("Balista");
            {
                var targetMenu = balistaMenu.AddSubMenu("Disabled Targets");
                {
                    foreach (var hero in HeroManager.Enemies.Where(x => x.IsValid))
                    {
                        this.ProcessLink(
                            "disable" + hero.ChampionName, 
                            targetMenu.AddLinkedBool("Disable " + hero.ChampionName, false));
                    }
                }

                this.ProcessLink("minRange", balistaMenu.AddLinkedSlider("Min Range", 700, 100, 1450));
                this.ProcessLink("maxRange", balistaMenu.AddLinkedSlider("Max Range", 1500, 100, 1500));
                this.ProcessLink("useBalista", balistaMenu.AddLinkedBool("Use Balista"));
            }
        }

        /// <summary>
        ///     Initialize the menu
        /// </summary>
        private void InitMenu()
        {
            this.menu = new MenuWrapper("iKalista");

            var comboMenu = this.menu.MainMenu.AddSubMenu("Combo Options");
            {
                this.ProcessLink("useQ", comboMenu.AddLinkedBool("Use Q"));
                this.ProcessLink("useQMin", comboMenu.AddLinkedBool("Q > Minon Combo"));
                this.ProcessLink("useE", comboMenu.AddLinkedBool("Use E"));
                this.ProcessLink("eLeaving", comboMenu.AddLinkedBool("Auto E Leaving"));
                this.ProcessLink("minStacks", comboMenu.AddLinkedSlider("Min Stacks E", 10, 5, 20));
                this.ProcessLink("eDamageReduction", comboMenu.AddLinkedSlider("Damage Reduction", 20, 100, 0));
                this.ProcessLink("saveAllyR", comboMenu.AddLinkedBool("Save Ally with R"));
                this.ProcessLink("allyPercent", comboMenu.AddLinkedSlider("Save Ally Percentage", 20));
            }

            var harassMenu = this.menu.MainMenu.AddSubMenu("Harass Options");
            {
                this.ProcessLink("useQH", harassMenu.AddLinkedBool("Use Q"));
                this.ProcessLink("useEH", harassMenu.AddLinkedBool("Use E"));
                this.ProcessLink("harassStacks", harassMenu.AddLinkedSlider("Min Stacks for E", 6, 2, 15));
                this.ProcessLink("useEMin", harassMenu.AddLinkedBool("Use Minion Harass"));
            }

            var laneclear = this.menu.MainMenu.AddSubMenu("Laneclear Options");
            {
                this.ProcessLink("useQLC", laneclear.AddLinkedBool("Use Q"));
                this.ProcessLink("minHitQ", laneclear.AddLinkedSlider("Q Minions Killed", 3, 1, 7));
                this.ProcessLink("useELC", laneclear.AddLinkedBool("Use E"));
                this.ProcessLink("minLC", laneclear.AddLinkedBool("Minion Harass"));
                this.ProcessLink("eUnkillable", laneclear.AddLinkedBool("E Unkillable Minions"));
                this.ProcessLink("qKillable", laneclear.AddLinkedBool("Q Unkillable if no buff"));
                this.ProcessLink("eHit", laneclear.AddLinkedSlider("Min Minions E", 4, 2, 10));
            }

            this.InitializeBalista();

            var misc = this.menu.MainMenu.AddSubMenu("Misc Options");
            {
                this.ProcessLink("fleeKey", misc.AddLinkedKeyBind("Flee Key", "G".ToCharArray()[0], KeyBindType.Press));
                this.ProcessLink("useJungleSteal", misc.AddLinkedBool("Enabled Jungle Steal"));
                this.ProcessLink(
                    "jungStealMode", 
                    misc.AddLinkedStringList("Steal Mode", new[] { "Baron - Dragon", "Big Minions", "Both" }));
                this.ProcessLink("qMana", misc.AddLinkedBool("Save Mana For E"));
                this.ProcessLink(
                    "sentBaron", 
                    misc.AddLinkedKeyBind("Sentinel Baron", "T".ToCharArray()[0], KeyBindType.Press));
                this.ProcessLink(
                    "sentDragon", 
                    misc.AddLinkedKeyBind("Sentinel Dragon", "Y".ToCharArray()[0], KeyBindType.Press));
                this.ProcessLink("autoTrinket", misc.AddLinkedBool("Auto Blue Trinket"));
            }

            var drawing = this.menu.MainMenu.AddSubMenu("Drawing Options");
            {
                this.ProcessLink(
                    "drawEDamage", 
                    drawing.AddLinkedCircle("Draw E Damage", true, Color.FromArgb(150, Color.LawnGreen), 0));
                this.ProcessLink(
                    "drawQ", 
                    drawing.AddLinkedCircle(
                        "Draw Q Range", 
                        true, 
                        Color.FromArgb(150, Color.Red), 
                        this.spells[SpellSlot.Q].Range));
                this.ProcessLink(
                    "drawE", 
                    drawing.AddLinkedCircle(
                        "Draw E Range", 
                        true, 
                        Color.FromArgb(150, Color.Red), 
                        this.spells[SpellSlot.E].Range));
            }
        }

        /// <summary>
        ///     Initialize the spells
        /// </summary>
        private void InitSpells()
        {
            this.spells[SpellSlot.Q].SetSkillshot(0.25f, 60f, 1600f, true, SkillshotType.SkillshotLine);
            this.spells[SpellSlot.R].SetSkillshot(0.50f, 1500, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        /// <summary>
        ///     Kill steal
        /// </summary>
        private void KillstealQ()
        {
            foreach (Obj_AI_Hero source in HeroManager.Enemies.Where(x => this.spells[SpellSlot.E].IsInRange(x) && this.GetEDamage(x) >= x.Health))
            {
                if (source.IsValidTarget(this.spells[SpellSlot.E].Range) && !this.HasUndyingBuff(source))
                {
                    this.spells[SpellSlot.E].Cast();
                }
            }

            var target =
                HeroManager.Enemies.FirstOrDefault(
                    x => this.spells[SpellSlot.Q].IsInRange(x) && this.spells[SpellSlot.Q].GetDamage(x) > x.Health + 10);

            if (target != null && this.spells[SpellSlot.Q].IsReady()
                && target.IsValidTarget(this.spells[SpellSlot.Q].Range) && !ObjectManager.Player.IsWindingUp
                && !ObjectManager.Player.IsDashing())
            {
                var prediction = this.spells[SpellSlot.Q].GetPrediction(target);
                if (prediction.Hitchance >= HitChance.VeryHigh || prediction.Hitchance == HitChance.Immobile)
                {
                    this.spells[SpellSlot.Q].Cast(target);
                }
            }
        }

        /// <summary>
        ///     Perform the combo
        /// </summary>
        private void OnCombo()
        {
            var spearTarget = TargetSelector.GetTarget(
                this.spells[SpellSlot.Q].Range, 
                TargetSelector.DamageType.Physical);

            if (this.boolLinks["useQ"].Value && this.spells[SpellSlot.Q].IsReady() && !ObjectManager.Player.IsWindingUp
                && !ObjectManager.Player.IsDashing())
            {
                if (this.boolLinks["qMana"].Value
                    && ObjectManager.Player.Mana
                    < this.spells[SpellSlot.Q].Instance.ManaCost + this.spells[SpellSlot.E].Instance.ManaCost)
                {
                    return;
                }

                foreach (var unit in
                    HeroManager.Enemies.Where(x => x.IsValidTarget(this.spells[SpellSlot.Q].Range))
                        .Where(unit => this.spells[SpellSlot.Q].GetPrediction(unit).Hitchance == HitChance.Immobile))
                {
                    this.spells[SpellSlot.Q].Cast(unit);
                }

                var prediction = this.spells[SpellSlot.Q].GetPrediction(spearTarget);

                switch (prediction.Hitchance)
                {
                    case HitChance.Collision:
                        this.QCollisionCheck(spearTarget);
                        break;
                    case HitChance.VeryHigh:
                        this.spells[SpellSlot.Q].Cast(spearTarget);
                        break;
                }
            }

            if (!this.boolLinks["useE"].Value || !this.spells[SpellSlot.E].IsReady())
            {
                return;
            }

            var rendTarget =
                HeroManager.Enemies.Where(
                    x =>
                    x.IsValidTarget(this.spells[SpellSlot.E].Range) && this.spells[SpellSlot.E].GetDamage(x) >= 1
                    && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield))
                    .OrderByDescending(x => this.spells[SpellSlot.E].GetDamage(x))
                    .FirstOrDefault();

            if (rendTarget != null && !this.HasUndyingBuff(rendTarget))
            {
                var rendBuff =
                    rendTarget.Buffs.Find(
                        b => b.Caster.IsMe && b.IsValidBuff() && b.DisplayName == "KalistaExpungeMarker");

                if (this.boolLinks["eLeaving"].Value && rendBuff.Count >= this.sliderLinks["minStacks"].Value.Value
                    && rendTarget.HealthPercent > 20
                    && rendTarget.ServerPosition.Distance(ObjectManager.Player.ServerPosition, true)
                    > Math.Pow(this.spells[SpellSlot.E].Range * 0.8, 2))
                {
                    this.spells[SpellSlot.E].Cast();
                }

                if (this.GetEDamage(rendTarget) >= rendTarget.Health
                    || (rendBuff.Count >= this.sliderLinks["minStacks"].Value.Value))
                {
                    this.spells[SpellSlot.E].Cast();
                }
            }
        }

        /// <summary>
        /// TODO The on flee.
        /// </summary>
        private void OnFlee()
        {
            var bestTarget =
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(x => x.IsEnemy && ObjectManager.Player.Distance(x) <= Orbwalking.GetRealAutoAttackRange(x))
                    .OrderBy(x => ObjectManager.Player.Distance(x))
                    .FirstOrDefault();

            // ReSharper disable once ConstantNullCoalescingCondition
            Orbwalking.Orbwalk(bestTarget ?? null, Game.CursorPos);
        }

        /// <summary>
        ///     Perform the harass function
        /// </summary>
        private void OnHarass()
        {
            var spearTarget = TargetSelector.GetTarget(
                this.spells[SpellSlot.Q].Range, 
                TargetSelector.DamageType.Physical);
            if (this.boolLinks["useQH"].Value && this.spells[SpellSlot.Q].IsReady() && !ObjectManager.Player.IsWindingUp
                && !ObjectManager.Player.IsDashing())
            {
                if (this.boolLinks["qMana"].Value
                    && ObjectManager.Player.Mana
                    < this.spells[SpellSlot.Q].Instance.ManaCost + this.spells[SpellSlot.E].Instance.ManaCost
                    && this.spells[SpellSlot.Q].GetDamage(spearTarget) < spearTarget.Health)
                {
                    return;
                }

                foreach (var unit in
                    HeroManager.Enemies.Where(x => x.IsValidTarget(this.spells[SpellSlot.Q].Range))
                        .Where(unit => this.spells[SpellSlot.Q].GetPrediction(unit).Hitchance == HitChance.Immobile))
                {
                    this.spells[SpellSlot.Q].Cast(unit);
                }

                var prediction = this.spells[SpellSlot.Q].GetPrediction(spearTarget);
                if (!ObjectManager.Player.IsWindingUp && !ObjectManager.Player.IsDashing())
                {
                    switch (prediction.Hitchance)
                    {
                        case HitChance.Collision:
                            this.QCollisionCheck(spearTarget);
                            break;
                        case HitChance.VeryHigh:
                            this.spells[SpellSlot.Q].Cast(spearTarget);
                            break;
                    }
                }
            }

            if (this.boolLinks["useEH"].Value)
            {
                var rendTarget =
                    HeroManager.Enemies.Where(
                        x =>
                        x.IsValidTarget(this.spells[SpellSlot.E].Range) && this.spells[SpellSlot.E].GetDamage(x) >= 1
                        && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield))
                        .OrderByDescending(x => this.spells[SpellSlot.E].GetDamage(x))
                        .FirstOrDefault();

                if (rendTarget != null)
                {
                    var stackCount =
                        rendTarget.Buffs.Find(
                            b => b.Caster.IsMe && b.IsValidBuff() && b.DisplayName == "KalistaExpungeMarker").Count;
                    if (this.GetEDamage(rendTarget) > rendTarget.Health + 10
                        || stackCount >= this.sliderLinks["minStacks"].Value.Value)
                    {
                        this.spells[SpellSlot.E].Cast();
                    }
                }
            }

            if (this.boolLinks["useEMin"].Value)
            {
                var minion =
                    MinionManager.GetMinions(this.spells[SpellSlot.E].Range, MinionTypes.All, MinionTeam.NotAlly)
                        .Where(x => x.Health <= this.spells[SpellSlot.E].GetDamage(x))
                        .OrderBy(x => x.Health)
                        .FirstOrDefault();
                var target =
                    HeroManager.Enemies.Where(
                        x =>
                        this.spells[SpellSlot.E].CanCast(x) && this.spells[SpellSlot.E].GetDamage(x) >= 1
                        && !x.HasBuffOfType(BuffType.SpellShield))
                        .OrderByDescending(x => this.spells[SpellSlot.E].GetDamage(x))
                        .FirstOrDefault();

                if (minion != null && target != null && this.spells[SpellSlot.E].CanCast(minion)
                    && this.spells[SpellSlot.E].CanCast(target))
                {
                    this.spells[SpellSlot.E].Cast();
                }
            }
        }

        /// <summary>
        ///     Perform the lane clear function
        /// </summary>
        private void OnLaneClear()
        {
            if (this.boolLinks["useQLC"].Value && this.spells[SpellSlot.Q].IsReady())
            {
                foreach (var selectedMinion in
                    from selectedMinion in MinionManager.GetMinions(this.spells[SpellSlot.Q].Range)
                    let killcount =
                        this.GetCollisionMinions(
                            ObjectManager.Player, 
                            ObjectManager.Player.ServerPosition.Extend(
                                selectedMinion.ServerPosition, 
                                this.spells[SpellSlot.Q].Range))
                        .Count(
                            collisionMinion =>
                            collisionMinion.Health <= this.spells[SpellSlot.Q].GetDamage(collisionMinion))
                    where killcount >= this.sliderLinks["minHitQ"].Value.Value
                    where !ObjectManager.Player.IsWindingUp && !ObjectManager.Player.IsDashing()
                    select selectedMinion)
                {
                    this.spells[SpellSlot.Q].Cast(selectedMinion.ServerPosition);
                }
            }

            var minion =
                MinionManager.GetMinions(this.spells[SpellSlot.E].Range, MinionTypes.All, MinionTeam.NotAlly)
                    .Where(x => x.Health <= this.spells[SpellSlot.E].GetDamage(x))
                    .OrderBy(x => x.Health)
                    .FirstOrDefault();

            var rendTarget =
                HeroManager.Enemies.Where(
                    x =>
                    this.spells[SpellSlot.E].IsInRange(x) && this.spells[SpellSlot.E].GetDamage(x) >= 1
                    && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield))
                    .OrderByDescending(x => this.spells[SpellSlot.E].GetDamage(x))
                    .FirstOrDefault();

            if (this.boolLinks["minLC"].Value && minion != null && rendTarget != null
                && this.spells[SpellSlot.E].CanCast(minion) && this.spells[SpellSlot.E].CanCast(rendTarget))
            {
                this.spells[SpellSlot.E].Cast();
            }

            if (this.spells[SpellSlot.E].IsReady() && this.boolLinks["useELC"].Value)
            {
                var minions = MinionManager.GetMinions(
                    this.spells[SpellSlot.E].Range, 
                    MinionTypes.All, 
                    MinionTeam.NotAlly);
                var count =
                    minions.Count(
                        x => this.spells[SpellSlot.E].CanCast(x) && x.Health < this.spells[SpellSlot.E].GetDamage(x));

                if (count >= this.sliderLinks["eHit"].Value.Value)
                {
                    this.spells[SpellSlot.E].Cast();
                }
            }
        }

        /// <summary>
        ///     Perform the last hit function
        /// </summary>
        private void OnLastHit()
        {
        }

        /// <summary>
        ///     The on process spell function
        /// </summary>
        /// <param name="sender">
        ///     The Spell Sender
        /// </param>
        /// <param name="args">
        ///     The Arguments
        /// </param>
        private void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "KalistaExpungeWrapper")
            {
                Orbwalking.ResetAutoAttackTimer();
            }

            if (sender.Type == GameObjectType.obj_AI_Hero && sender.IsEnemy && this.boolLinks["saveAllyR"].Value)
            {
                var soulboundhero =
                    HeroManager.Allies.FirstOrDefault(
                        hero =>
                        hero.HasBuff("kalistacoopstrikeally") && args.Target.NetworkId == hero.NetworkId
                        && hero.HealthPercent <= 15);

                if (soulboundhero != null && soulboundhero.HealthPercent < this.sliderLinks["allyPercent"].Value.Value)
                {
                    this.spells[SpellSlot.R].Cast();
                }
            }
        }

        /// <summary>
        ///     TODO The on update.
        /// </summary>
        /// <param name="args">
        ///     TODO The args.
        /// </param>
        private void OnUpdate(EventArgs args)
        {
            this.orbwalkingModesDictionary[this.menu.Orbwalker.ActiveMode]();
            this.HandleSentinels();
            this.KillstealQ();
            if (this.boolLinks["useJungleSteal"].Value)
            {
                this.DoMobSteal();
            }

            if (this.keyLinks["fleeKey"].Value.Active)
            {
                this.OnFlee();
            }

            if (this.boolLinks["autoTrinket"].Value && ObjectManager.Player.Level >= 6 && ObjectManager.Player.InShop()
                && !(Items.HasItem(3342) || Items.HasItem(3363)))
            {
                ObjectManager.Player.BuyItem(ItemId.Scrying_Orb_Trinket);
            }

            this.HandleBalista();
        }

        /// <summary>
        ///     Process The current Link
        /// </summary>
        /// <param name="key">
        ///     The name of the link
        /// </param>
        /// <param name="value">
        ///     The Value of the link
        /// </param>
        private void ProcessLink(string key, object value)
        {
            var boolLink = value as MenuWrapper.BoolLink;
            if (boolLink != null)
            {
                this.boolLinks.Add(key, boolLink);
            }

            var sliderLink = value as MenuWrapper.SliderLink;
            if (sliderLink != null)
            {
                this.sliderLinks.Add(key, sliderLink);
            }

            var keybindLink = value as MenuWrapper.KeyBindLink;
            if (keybindLink != null)
            {
                this.keyLinks.Add(key, keybindLink);
            }

            var circleLink = value as MenuWrapper.CircleLink;
            if (circleLink != null)
            {
                this.circleLinks.Add(key, circleLink);
            }

            var stringLink = value as MenuWrapper.StringListLink;
            if (stringLink != null)
            {
                this.stringListLinks.Add(key, stringLink);
            }
        }

        /// <summary>
        ///     The target to check the collision for.
        /// </summary>
        /// <param name="target">The target</param>
        private void QCollisionCheck(Obj_AI_Hero target)
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, this.spells[SpellSlot.Q].Range);

            if (minions.Count < 1 || !this.boolLinks["useQMin"].Value || ObjectManager.Player.IsWindingUp
                || ObjectManager.Player.IsDashing())
            {
                return;
            }

            foreach (var minion in minions.Where(x => x.IsValidTarget(this.spells[SpellSlot.Q].Range)))
            {
                var difference = ObjectManager.Player.Distance(target) - ObjectManager.Player.Distance(minion);

                for (var i = 0; i < difference; i += (int)target.BoundingRadius)
                {
                    var point =
                        minion.ServerPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -i).To3D();
                    var time = this.spells[SpellSlot.Q].Delay
                               + (ObjectManager.Player.Distance(point) / this.spells[SpellSlot.Q].Speed * 1000f);

                    var prediction = Prediction.GetPrediction(target, time);

                    var collision = this.spells[SpellSlot.Q].GetCollision(
                        point.To2D(), 
                        new List<Vector2> { prediction.UnitPosition.To2D() });

                    if (collision.Any(x => x.Health > this.spells[SpellSlot.Q].GetDamage(x)))
                    {
                        return;
                    }

                    if (prediction.UnitPosition.Distance(point) <= this.spells[SpellSlot.Q].Width
                        && !minions.Any(m => m.Distance(point) <= this.spells[SpellSlot.Q].Width))
                    {
                        this.spells[SpellSlot.Q].Cast(minion);
                    }
                }
            }
        }

        #endregion
    }
}