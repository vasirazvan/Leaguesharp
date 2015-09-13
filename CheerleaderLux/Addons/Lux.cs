using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SPrediction;
using SharpDX;


namespace CheerleaderLux.Addons
{
    // Todo List: - Combo is le finished
    // - Ignite, Ludens, Lichbane Calcs
    // - Laneclear
    // - Killsteal
    // - Misc (anti-gap, anti-rengar, anti-khazix etc)
    public class Lux : Extensions.Statics
    {
        public static void OnLoad(EventArgs args)
        {
            Printmsg("Sucessfully loaded - Patch: " + Game.Version);
            Printmsg("Assembly Version: 1.0.0.1 Beta");

            Q = new Spell(SpellSlot.Q, 1300);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 3500);

            Q.SetSkillshot(0.25f, 70f, 1200f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 110f, 1200f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 300f, 1300f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1f, 190f, float.MaxValue, false, SkillshotType.SkillshotLine);

            #region Menu

            Config = new Menu("Cheerleader Lux", "Lux", true).SetFontStyle(System.Drawing.FontStyle.Bold, Color.LightGreen);
            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("Orbwalker Settings", "Orbwalker Settings")));
            var combo = Config.AddSubMenu(new Menu("Combat Settings", "Combat Settings"));
            var killsteal = Config.AddSubMenu(new Menu("Killsteal Settings", "Killsteal Settings"));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("Target Selector", "Target Selector")));
            SPrediction.Prediction.Initialize(Config);
            var misc = Config.AddSubMenu(new Menu("Misc Settings", "Misc Settings"));
            var drawing = Config.AddSubMenu(new Menu("Draw Settings", "Draw Settings"));

            //Credits
            Config.AddItem(new MenuItem("blank1", "                          "));
            Config.AddItem(new MenuItem("blank3", "Made by ScienceARK").SetFontStyle(System.Drawing.FontStyle.Bold));

            //Combo Menu
            var Rsettings = combo.AddSubMenu(new Menu("Advanced [R] Settings", "advR").SetFontStyle(System.Drawing.FontStyle.Bold));
            Rsettings.AddItem(new MenuItem("advanced.R.aoe", "Use [R] on X amount of Enemies").SetValue(true));
            Rsettings.AddItem(new MenuItem("advanced.R.aoe.count", "Enemy Count").SetValue(new Slider(3, 5, 1)));
            combo.AddItem(new MenuItem("combo.Q", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("combo.W", "Use W").SetValue(true));
            combo.AddItem(new MenuItem("combo.E", "Use E").SetValue(true));
            combo.AddItem(new MenuItem("combo.R", "Use R").SetValue(true));

            //Harras Menu
            var Harass = combo.AddSubMenu(new Menu("Harass Settings", "harras").SetFontStyle(System.Drawing.FontStyle.Bold));
            Harass.AddItem(new MenuItem("harass.support", "Disable Autoattack").SetValue(false));
            Harass.AddItem(new MenuItem("harass.mana.slider", "Enemy Count").SetValue(new Slider(3, 5, 1)));
            Harass.AddItem(new MenuItem("harass.Q", "Use [Q] in Harass").SetValue(true));
            Harass.AddItem(new MenuItem("harass.E", "Use [E] in Harass").SetValue(true));
            //AutoSpells
            var autospells = combo.AddSubMenu(new Menu("Auto Spell Settings", "ASS").SetFontStyle(System.Drawing.FontStyle.Bold));
            autospells.AddItem(new MenuItem("autospells.E.aoe", "Use [E] on X amount of Enemies").SetValue(true));
            autospells.AddItem(new MenuItem("autospells.E.aoe.count", "Enemy Count").SetValue(new Slider(3, 5, 1)));

            //Drawings
            var dmgdraw = drawing.AddSubMenu(new Menu("Damage Indicator Settings", "DMGI"));

            var miscdraw = drawing.AddSubMenu(new Menu("Misc Drawings", "MiscD"));
            miscdraw.AddItem(new MenuItem("draw.LuxE.position", "Draw Lux [E] Position").SetValue(true));
            miscdraw.AddItem(new MenuItem("print.debug.chat", "Print Debug Messages").SetValue(true));

            drawing.AddItem(new MenuItem("disable.draws", "Disable all Drawings").SetValue(false));
            drawing.AddItem(new MenuItem("draw.Q", "Draw Q Range").SetValue(new Circle(true, System.Drawing.Color.Orange)));
            drawing.AddItem(new MenuItem("draw.W", "Draw W Range").SetValue(new Circle(true, System.Drawing.Color.DarkOrange)));
            drawing.AddItem(new MenuItem("draw.E", "Draw E Range").SetValue(new Circle(true, System.Drawing.Color.LightBlue)));
            drawing.AddItem(new MenuItem("draw.R", "Draw R Range").SetValue(new Circle(true, System.Drawing.Color.CornflowerBlue)));

            Config.AddToMainMenu();

            #endregion Menu

            Game.OnUpdate += OrbwalkerModes;
            Game.OnUpdate += RefreshObjects;
            Game.OnUpdate += AutoSpells;
            Orbwalking.BeforeAttack += SupportMode;
            GameObject.OnCreate += EisGone;
            GameObject.OnCreate += EisAlive;
            Obj_AI_Base.OnProcessSpellCast += Tickcount;
            Drawing.OnDraw += Drawings;
        }

        private static void SupportMode(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && Config.Item("harass.support").GetValue<bool>())
            {
                if (((Obj_AI_Base)Orbwalker.GetTarget()).IsMinion) args.Process = false;
            }

        }

        private static void AutoSpells(EventArgs args)
        {
        }

        private static void Tickcount(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            //LuxLightStrikeKugel -- E Cast
            //LuxLightstrikeToggle -- E Toggle
            //LuxLightBinding -- Q
            //LuxPrismaticWave -- W
            //LuxMaliceCannon -- R

            var spellname = args.SData.Name;

            if (player.Name != "ScienceARK")
                return;

            if (spellname.Contains("LuxLightBinding"))
            {
                Q.LastCastAttemptT = Environment.TickCount;
                Printchat("[Q] Casted Tickcount: " + Q.LastCastAttemptT);
            }

            if (spellname.Contains("LuxPrismaticWave"))
            {
                W.LastCastAttemptT = Environment.TickCount;
                Printchat("[W] Casted Tickcount: " + W.LastCastAttemptT);
            }

            if (spellname.Contains("LuxLightstrikeToggle"))
            {
                E.LastCastAttemptT = Environment.TickCount;
                Printchat("[E-Toggle] Casted Tickcount: " + E.LastCastAttemptT);
            }
            if (spellname.Contains("LuxMaliceCannon"))
            {
                R.LastCastAttemptT = Environment.TickCount;
                Printchat("[R] Casted Tickcount: " + R.LastCastAttemptT);
            }

            //if (spellname.Contains("LuxMaliceCannon"))

        }

        private static void Drawings(EventArgs args)
        {
            if (Config.Item("disable.draws").GetValue<bool>())
                return;

            if (Lux_E != null && Config.Item("draw.LuxE.position").GetValue<bool>())
            {
                var pos1 = Drawing.WorldToScreen(player.Position);
                var pos2 = Drawing.WorldToScreen(Lux_E.Position);

                Drawing.DrawLine(pos1, pos2, 1, System.Drawing.Color.LightBlue);
                Drawing.DrawCircle(Lux_E.Position, 100, System.Drawing.Color.Gray);
            }

            if (Config.Item("draw.Q").GetValue<Circle>().Active)
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range,
                        Q.IsReady() ? Config.Item("draw.Q").GetValue<Circle>().Color : System.Drawing.Color.Red);

            if (Config.Item("draw.W").GetValue<Circle>().Active)
                if (W.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range,
                        W.IsReady() ? Config.Item("draw.W").GetValue<Circle>().Color : System.Drawing.Color.Red);

            if (Config.Item("draw.E").GetValue<Circle>().Active)
                if (E.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range - 1,
                        E.IsReady() ? Config.Item("draw.E").GetValue<Circle>().Color : System.Drawing.Color.Red);

            if (Config.Item("draw.R").GetValue<Circle>().Active)
                if (R.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range - 2,
                        R.IsReady() ? Config.Item("draw.R").GetValue<Circle>().Color : System.Drawing.Color.Red);

            Drawing.DrawCircle(player.Position, Player.BoundingRadius, System.Drawing.Color.Gray);
        }

        private static void EisAlive(GameObject sender, EventArgs args)
        {
            //Lux_Base_E_mis.troy
            //Lux_Base_E_tar_aoe_green.troy
            //Lux_Base_E_tar_nova.troy

            if (sender.Name == "Lux_Base_E_mis.troy")
            {
                Lux_E = sender;
                Printchat("Lux E object detected");
            }
        }

        private static void EisGone(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Lux_Base_E_tar_nova.troy")
            {
                Lux_E = null;
                Printchat("Lux E has detonated");
            }
        }

        private static void OrbwalkerModes(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Routine();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    HarassRoutine();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
            }
        }
        /// <summary>
        /// Printchat is being used for debug purposes.
        /// </summary>
        /// <param name="message"></param>
        private static void Printchat(string message)
        {
            if (!Config.Item("print.debug.chat").GetValue<bool>())
                return;

            Game.PrintChat(
                "<font color='#FFB90F'>[Console]:</font> <font color='#FFFFFF'>" + message + "</font>");
        }
        private static void Printmsg(string message)
        {
            Game.PrintChat(
                "<font color='#00ff00'>[Cheerleader Lux]:</font> <font color='#FFFFFF'>" + message + "</font>");
        }
        private static void RefreshObjects(EventArgs args)
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var rooted = target.HasBuff("LuxLightBindingMis");
            var debuff = target.HasBuff("luxilluminatingfraulein");

            //Lux E detonation (Object Bounding Radius)
            if (rooted && target.Distance(player.Position) <=
                Orbwalking.GetRealAutoAttackRange(player) && target.Health > E.GetDamage(target) && debuff)
            {
                Printchat("[E] Detonation Blocked. Reason: AA-able");
                return;
            }

            if (rooted && target.Distance(player.Position) <=
                Orbwalking.GetRealAutoAttackRange(player) + 300 && target.Health > E.GetDamage(target) && debuff && target.CountEnemiesInRange(600) <= 1)
            {
                Printchat("[E] Detonation Blocked. Reason: AA-able");
                return;
            }

            if (Lux_E != null && Lux_E.Position.CountEnemiesInRange(E.Width) >= 1)
            {
                E.Cast();
                Printchat("[E] Toggle Cast. Reason: Enemy Detected");
            }
        }
        private static int PassiveDMG(Obj_AI_Hero target)
        {
           double PassiveDMG = player.CalcDamage(target, Damage.DamageType.Magical,
                10 + (8 * player.Level) + 0.2 * player.FlatMagicDamageMod);
            return (int)PassiveDMG;
        }
        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
                return 0f;
            return (float)player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        /// <summary>
        /// Quick and Easy Spellcasts params for Lux
        /// </summary>
        /// <param name="target"></param> Which unit should we cast spells on
        /// <param name="range"></param> Range check for targets
        /// <param name="spellslot"></param> Which spellslot 
        /// <param name="collision"></param> Collision Check for Q usage
        /// <param name="count"></param>  Only use said ability if it can hit X amount of enemies
        /// <param name="objectcheck"></param> Objectcheck for E usage
        public static void SpellCast(Obj_AI_Hero target, float range, Spell spellslot, bool collision, byte count, bool objectcheck, HitChance Hitchance)
        {
            //Sprediction SpellCast
            if (spellslot.IsReady() && target.IsValidTarget(range))
            {
                spellslot.SPredictionCast(target, Hitchance, 0, count);
            }

            if (collision)
            {
                Q.SetSkillshot(0.25f, 150f, 1200f, false, SkillshotType.SkillshotLine);
            }
            //Collision check using Common.Prediction
            var qcollision = Q.GetCollision(player.Position.To2D(), new List<Vector2> { target.Position.To2D() });
            var minioncol = qcollision.Where(x => (x is Obj_AI_Hero) && x.IsEnemy).Count();


            //Sprediction - Collision Cast
            if (spellslot.IsReady() && target.IsValidTarget(range) && collision && minioncol <= 1)
            {
                spellslot.SPredictionCast(target, Hitchance, 0, count);
            }

            //Insert E cast information
            if (Lux_E == null && spellslot.IsReady() && objectcheck && target.IsValidTarget(E.Range))
            {
                spellslot.SPredictionCast(target, Hitchance, 0, count);
            }
        }
        public static void HarassRoutine() //Add only use Q on CC
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid) return;

            var cc = 0; //Add Requirements

            var manaslider = Config.Item("harass.mana.slider").GetValue<Slider>().Value;

            if (player.ManaPercent < manaslider) return;

            if (Config.Item("harass.Q").GetValue<bool>() && Environment.TickCount - E.LastCastAttemptT > 800 && Environment.TickCount - R.LastCastAttemptT > 800)
                SpellCast(target, Q.Range, Q, true, 1, false, HitChance.High);

            if (Config.Item("harass.E").GetValue<bool>() && Environment.TickCount - Q.LastCastAttemptT > 800 && Environment.TickCount - R.LastCastAttemptT > 800)
                SpellCast(target, E.Range, E, false, 1, true, HitChance.Medium);

        }
        public static void Routine()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid) return;


            float qdmg = Q.GetDamage(target);
            float edmg = E.GetDamage(target);
            float rdmg = R.GetDamage(target);
            float aa = (float)Player.GetAutoAttackDamage(target);
            var insideE = Lux_E != null && target.Distance(Lux_E.Position) <= E.Width;
            var thp = target.Health;
            var AArange = Orbwalking.GetRealAutoAttackRange(player);
            var debuff = target.HasBuff("luxilluminatingfraulein");
            if (debuff)
                rdmg += PassiveDMG(target);
            if (debuff)
                aa += PassiveDMG(target);
            if (insideE)
                rdmg += edmg;
            var rooted = target.HasBuff("LuxLightBindingMis");

            //Sprediction Cast [Q]
            if (insideE && thp < edmg && target.IsValidTarget(R.Range))
                return;

            if (thp < aa && target.IsValidTarget(AArange))
                return;

            if (Config.Item("combo.Q").GetValue<bool>() && Environment.TickCount - E.LastCastAttemptT > 800 && Environment.TickCount - R.LastCastAttemptT > 800)
                SpellCast(target, Q.Range, Q, true, 1, false, HitChance.High);

            if (rooted && thp < aa && target.IsValidTarget(AArange))
                return;

            //Sprediction Cast [E]
            if (Config.Item("combo.E").GetValue<bool>() && Environment.TickCount - Q.LastCastAttemptT > 800 && Environment.TickCount - R.LastCastAttemptT > 800)
                SpellCast(target, E.Range, E, false, 1, true, HitChance.Medium);

            if (Config.Item("combo.R").GetValue<bool>() && R.IsReady())
                SpellCastR(target);
                

        }
        public static float AllyCheck(Obj_AI_Hero target, int range)
        {
            var allies = 
                ObjectManager.Get<Obj_AI_Hero>().Where(a => a.IsAlly 
                && a.Distance(target.Position) < range && a.HealthPercentage() > 20).ToList();

            return allies.Count;
            
        }
        public static void SpellCastR(Obj_AI_Hero target)
        {

            //[R] Aoe Cast
            byte hitcount;
            hitcount = (byte) Config.Item("advanced.R.aoe.count").GetValue<Slider>().Value;

            if (Config.Item("advanced.R.aoe").GetValue<bool>() && R.IsReady())
            {
                SpellCast(target, R.Range, R, false, hitcount, false, HitChance.High);
            }

            //[R] Combo Sequences
            double qdmg = Q.GetDamage(target);
            double edmg = E.GetDamage(target);
            double rdmg = R.GetDamage(target);
            double aa = Player.GetAutoAttackDamage(target);
            var insideE = Lux_E != null && target.Distance(Lux_E.Position) < E.Width;
            var thp = target.Health;
            var AArange = Orbwalking.GetRealAutoAttackRange(player);

            var debuff = target.HasBuff("luxilluminatingfraulein");

            if (debuff)
                rdmg += PassiveDMG(target);
            if (debuff)
                aa += PassiveDMG(target);
            if (insideE)
                rdmg += edmg;

            var qcollision = Q.GetCollision(player.Position.To2D(), new List<Vector2> { target.Position.To2D() });
            var minioncol = qcollision.Where(x => (x is Obj_AI_Hero) && x.IsEnemy).Count();

            var rooted = target.HasBuff("LuxLightBindingMis");

            if (Environment.TickCount - E.LastCastAttemptT < 800 || Environment.TickCount - Q.LastCastAttemptT < 800)
                return;

            //Checks if Allies can kill the bitch
            if (AllyCheck(target, 800) >= 2 && target.Health < rdmg / 2)
                return;
            //Checks if an Ally can kill le bitch
            if (AllyCheck(target, 800) >= 1 && target.Health < rdmg / 2)
                return;
            // = 

            if (E.IsReady() && thp < edmg && target.IsValidTarget(E.Range))
                return;

            if (Q.IsReady() && thp < qdmg && target.IsValidTarget(Q.Range) && minioncol <= 1)
                return;

            if (insideE && thp < edmg)
                return;

            if (insideE && thp < edmg + aa && target.IsValidTarget(AArange))
                return;

            if (thp < aa && target.IsValidTarget(AArange))
                return;

            if (thp < edmg && E.IsReady() && rooted && target.IsValidTarget(E.Range))
                return;

            if (Q.IsReady() && !E.IsReady() && thp < qdmg && target.IsValidTarget(Q.Range) && minioncol <= 1)
                return;

            if (E.IsReady() && !Q.IsReady() && thp < edmg && target.IsValidTarget(E.Range))
                return;

            if (rooted && debuff && thp < aa && target.IsValidTarget(AArange))
                return;

            if (rooted && insideE && thp < rdmg + edmg && target.IsValidTarget(R.Range))
            {
                SpellCast(target, R.Range, R, false, 1, false, HitChance.High);
            }

            if (rooted && debuff && thp < rdmg + aa && target.IsValidTarget(AArange))
            {
                SpellCast(target, R.Range, R, false, 1, false, HitChance.High);
            }

            if (rooted && debuff && thp < rdmg && target.IsValidTarget(R.Range))
            {
                SpellCast(target, R.Range, R, false, 1, false, HitChance.High);
            }

            if (thp < rdmg)
            {
                SpellCast(target, R.Range, R, false, 1, false, HitChance.High);
            }         
        }
    }
}

