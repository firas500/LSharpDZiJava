// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Kalista.cs" company="">
//   
// </copyright>
// <summary>
//   An Assembly for <see cref="Kalista" /> okay
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace IKalista
{
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

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
                                                                       { SpellSlot.E, new Spell(SpellSlot.E, 1000) }
                                                                   };

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

        #endregion

        #region Methods

        /// <summary>
        ///     This is where the magic happens, we like to steal other peoples stuff.
        /// </summary>
        private void DoMobSteal()
        {
            var minion =
                MinionManager.GetMinions(
                    this.spells[SpellSlot.E].Range, 
                    MinionTypes.All, 
                    MinionTeam.Neutral, 
                    MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(x => x.Health + (x.HPRegenRate / 2) <= this.spells[SpellSlot.E].GetDamage(x));

            if (minion != null)
            {
                this.spells[SpellSlot.E].Cast();
            }
        }

        /// <summary>
        ///     Handles the grab
        /// </summary>
        private void HandleBalista()
        {
            var allTargets = HeroManager.Enemies.Where(x => x.IsValid && x.Distance(ObjectManager.Player) <= 2450f);

            var blitzcrank =
                HeroManager.Allies.SingleOrDefault(
                    x =>
                    x.IsAlly && ObjectManager.Player.Distance(x.ServerPosition) < 1500
                    && ObjectManager.Player.Distance(x.ServerPosition) >= 700 && x.ChampionName == "Blitzcrank");

            if (blitzcrank == null)
            {
                return;
            }

            foreach (var target in allTargets)
            {
                if (this.boolLinks["disable" + target.ChampionName].Value || !this.spells[SpellSlot.R].IsReady()
                    || !this.boolLinks["useBalista"].Value)
                {
                    return;
                }

                foreach (var buff in
                    target.Buffs.Where(
                        buff => buff.Name == "rocketgrab2" && buff.IsActive && this.spells[SpellSlot.R].IsReady()))
                {
                    this.spells[SpellSlot.R].Cast();
                }
            }
        }

        /// <summary>
        ///     Handles the Sentinel Bug
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
            if (ObjectManager.Player.Name == "Hawk XD")
            {
                ShowNotification("Hawk is a faggot <3", Color.Aqua, 10000);
            }

            // TODO auto W dragon / baron, whichever your closest to
            Game.OnUpdate += args =>
                {
                    this.orbwalkingModesDictionary[this.menu.Orbwalker.ActiveMode]();
                    this.HandleSentinels();
                    if (this.boolLinks["useJungleSteal"].Value)
                    {
                        this.DoMobSteal();
                    }
                };

            Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
                {
                    if (sender.IsMe && args.SData.Name == "KalistaExpungeWrapper")
                    {
                        ////Utility.DelayAction.Add(250, Orbwalking.ResetAutoAttackTimer);
                        Orbwalking.ResetAutoAttackTimer();
                    }
                };

            Orbwalking.OnNonKillableMinion += minion =>
                {
                    var killableMinion = minion as Obj_AI_Base;
                    if (killableMinion == null || !killableMinion.HasBuff("KalistaExpungeMarker")
                        || !this.spells[SpellSlot.E].IsReady())
                    {
                        return;
                    }

                    if (killableMinion.Health <= this.spells[SpellSlot.E].GetDamage(killableMinion)
                        && this.spells[SpellSlot.E].CanCast(killableMinion))
                    {
                        this.spells[SpellSlot.E].Cast();
                    }
                };
        }

        /// <summary>
        ///     The Initialization
        /// </summary>
        private void InitializeBalista()
        {
            var enemies = HeroManager.Enemies.Any(x => x.IsAlly && !x.IsMe && x.ChampionName == "Blitzcrank");

            if (!enemies)
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
                            targetMenu.AddLinkedBool("Disable " + hero.ChampionName));
                    }
                }

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
                this.ProcessLink("useE", comboMenu.AddLinkedBool("useE"));
                this.ProcessLink("minStacks", comboMenu.AddLinkedSlider("Min Stacks E", 10, 5, 20));
            }

            var harassMenu = this.menu.MainMenu.AddSubMenu("Harass Options");
            {
                this.ProcessLink("useEH", harassMenu.AddLinkedBool("Use E"));
                this.ProcessLink("harassStacks", harassMenu.AddLinkedSlider("Min Stacks for E", 6, 2, 15));
                this.ProcessLink("useEMin", harassMenu.AddLinkedBool("Use Minion Harass"));
            }

            var laneclear = this.menu.MainMenu.AddSubMenu("Laneclear Options");
            {
                this.ProcessLink("useELC", laneclear.AddLinkedBool("Use E"));
                this.ProcessLink("minLC", laneclear.AddLinkedBool("Minion Harass"));
                this.ProcessLink("eHit", laneclear.AddLinkedSlider("Min Minions E", 4, 2, 10));
            }

            this.InitializeBalista();

            var misc = this.menu.MainMenu.AddSubMenu("Misc Options");
            {
                this.ProcessLink("useJungleSteal", misc.AddLinkedBool("Enabled Jungle Steal"));
                this.ProcessLink("qMana", misc.AddLinkedBool("Save Mana For E"));
                this.ProcessLink(
                    "sentBaron", 
                    misc.AddLinkedKeyBind("Sentinel Baron", "T".ToCharArray()[0], KeyBindType.Press));
                this.ProcessLink(
                    "sentDragon", 
                    misc.AddLinkedKeyBind("Sentinel Dragon", "Y".ToCharArray()[0], KeyBindType.Press));
            }
        }

        /// <summary>
        ///     Initialize the spells
        /// </summary>
        private void InitSpells()
        {
            this.spells[SpellSlot.Q].SetSkillshot(0.25f, 40f, 1200f, true, SkillshotType.SkillshotLine);
        }

        /// <summary>
        ///     Perform the combo
        /// </summary>
        private void OnCombo()
        {
            var spearTarget = TargetSelector.GetTarget(
                this.spells[SpellSlot.Q].Range, 
                TargetSelector.DamageType.Physical);

            this.HandleBalista();

            if (this.boolLinks["useQ"].Value && this.spells[SpellSlot.Q].IsReady())
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
                        case HitChance.High:
                        case HitChance.VeryHigh:
                            this.spells[SpellSlot.Q].Cast(spearTarget);
                            break;
                    }
                }
            }

            if (!this.boolLinks["useE"].Value || !this.spells[SpellSlot.E].IsReady())
            {
                return;
            }

            var rendTarget =
                HeroManager.Enemies.Where(
                    x =>
                    this.spells[SpellSlot.E].IsInRange(x) && x.HasBuff("KalistaExpungeMarker")
                    && !x.HasBuffOfType(BuffType.Invulnerability))
                    .OrderByDescending(x => this.spells[SpellSlot.E].GetDamage(x))
                    .FirstOrDefault();

            if (rendTarget != null)
            {
                var stackCount =
                    rendTarget.Buffs.Find(
                        b => b.Caster.IsMe && b.IsValidBuff() && b.DisplayName == "KalistaExpungeMarker").Count;
                if (this.spells[SpellSlot.E].GetDamage(rendTarget) > rendTarget.Health
                    || stackCount >= this.sliderLinks["harassStacks"].Value.Value)
                {
                    this.spells[SpellSlot.E].Cast();
                }
            }
        }

        /// <summary>
        ///     Perform the harass function
        /// </summary>
        private void OnHarass()
        {
            if (!this.spells[SpellSlot.E].IsReady())
            {
                return;
            }

            if (this.boolLinks["useEH"].Value)
            {
                var rendTarget =
                    HeroManager.Enemies.Where(
                        x =>
                        this.spells[SpellSlot.E].IsInRange(x) && x.HasBuff("KalistaExpungeMarker")
                        && !x.HasBuffOfType(BuffType.Invulnerability))
                        .OrderByDescending(x => this.spells[SpellSlot.E].GetDamage(x))
                        .FirstOrDefault();

                if (rendTarget != null)
                {
                    var stackCount =
                        rendTarget.Buffs.Find(
                            b => b.Caster.IsMe && b.IsValidBuff() && b.DisplayName == "KalistaExpungeMarker").Count;
                    if (this.spells[SpellSlot.E].GetDamage(rendTarget) > rendTarget.Health + 10
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
                        this.spells[SpellSlot.E].CanCast(x) && x.HasBuff("KalistaExpungeMarker")
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
            var minion =
                MinionManager.GetMinions(this.spells[SpellSlot.E].Range, MinionTypes.All, MinionTeam.NotAlly)
                    .Where(x => x.Health <= this.spells[SpellSlot.E].GetDamage(x))
                    .OrderBy(x => x.Health)
                    .FirstOrDefault();

            var rendTarget =
                HeroManager.Enemies.Where(
                    x =>
                    this.spells[SpellSlot.E].IsInRange(x) && x.HasBuff("KalistaExpungeMarker")
                    && !x.HasBuffOfType(BuffType.Invulnerability))
                    .OrderByDescending(x => this.spells[SpellSlot.E].GetDamage(x))
                    .FirstOrDefault();

            if (this.boolLinks["minLC"].Value && minion != null && rendTarget != null
                && this.spells[SpellSlot.E].CanCast(minion) && this.spells[SpellSlot.E].CanCast(rendTarget))
            {
                this.spells[SpellSlot.E].Cast();
            }

            var minions = MinionManager.GetMinions(this.spells[SpellSlot.E].Range, MinionTypes.All, MinionTeam.NotAlly);

            if (this.spells[SpellSlot.E].IsReady() && this.boolLinks["useELC"].Value)
            {
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
        }

        /// <summary>
        ///     The target to check the collision for.
        /// </summary>
        /// <param name="target">The target</param>
        private void QCollisionCheck(Obj_AI_Hero target)
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, this.spells[SpellSlot.Q].Range);

            if (minions.Count < 1 || !this.boolLinks["useQMin"].Value)
            {
                // TODO possibly Projection checking
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