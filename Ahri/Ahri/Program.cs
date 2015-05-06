using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Ahri
{
    class Program
    {
        private const string championName = "Ahri";
        private static  Obj_AI_Hero Player;
        private static Spell Q, W, E, R;
        private static Menu AhriMenu;
        private static Orbwalking.Orbwalker ahriOrbwalking;

        private static  void Game_OnLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != championName)
            {
                Game.PrintChat("Failed to Load Ahri.");
                return;
            }

            //Spells
            Q = new Spell(SpellSlot.Q, 950f);
            W = new Spell(SpellSlot.W, 800f);
            E = new Spell(SpellSlot.E, 975f);
            R = new Spell(SpellSlot.R, 450f);

            Q.SetSkillshot(0.25f,100f,1250f,false,SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f,60f,1500f,true,SkillshotType.SkillshotLine);

            //menu
            AhriMenu = new Menu("Ahri", "Ahri", true);
            ahriOrbwalking = new Orbwalking.Orbwalker(AhriMenu.SubMenu("Orbwalking"));
            Menu AhriTS = new Menu("Target Selector","Target Selector");
            TargetSelector.AddToMenu(AhriTS);
            AhriMenu.AddSubMenu(AhriTS);
            //Combo
            AhriMenu.AddSubMenu(new Menu("Combo", "Combo"));
            AhriMenu.SubMenu("Combo").AddItem(new MenuItem("Use Q", "Use Q").SetValue(true));
            AhriMenu.SubMenu("Combo").AddItem(new MenuItem("Use W", "Use W").SetValue(true));
            AhriMenu.SubMenu("Combo").AddItem(new MenuItem("Use E", "Use E").SetValue(true));
            AhriMenu.SubMenu("Combo").AddItem(new MenuItem("Use R", "Use R").SetValue(true));
            AhriMenu.SubMenu("Combo")
             .AddItem(
                 new MenuItem("ComboActive", "Combo!").SetValue(
                     new KeyBind(AhriMenu.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));
            //Harras
            AhriMenu.AddSubMenu(new Menu("Harras", "Harras"));
            AhriMenu.SubMenu("Harras").AddItem(new MenuItem("HarrasQ","HarrasQ").SetValue(true));
            AhriMenu.SubMenu("Harras").AddItem(new MenuItem("HarrasW", "HarrasW").SetValue(true));
            AhriMenu.SubMenu("Harras").AddItem(new MenuItem("HarrasE", "HarrasE").SetValue(true));
            AhriMenu.AddToMainMenu();



            Game.PrintChat("Ahri " + Assembly.GetExecutingAssembly().GetName().Version + " Loaded!");
            Game.OnUpdate += Game_OnUpdate;
            //Other Events
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_onEnemycloser;

        } //Game_OnLoad



        private static void Game_OnUpdate(EventArgs args)
        {
            if (AhriMenu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
        
        }

        private static void Combo()
        {
            if ( AhriMenu.Item("Use E").GetValue<bool>() &&  E.IsReady())
            { 
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            CastSpell(E,target);
            }
            if (AhriMenu.Item("Use W").GetValue<bool>() && W.IsReady())
            {
                var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                W.Cast();
            }
            if (AhriMenu.Item("Use Q").GetValue<bool>() && Q.IsReady())
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                CastSpell(Q,target);
            }

        }


        private static void CastSpell(Spell QWER, Obj_AI_Hero target)
        {

            List<Vector2> waypoints = target.GetWaypoints();
            Game.PrintChat("" + target.Path.Count() + " " + (target.Position == target.ServerPosition) + (waypoints.Last<Vector2>().To3D() == target.ServerPosition));
            if (QWER.Delay < 0.3)
                QWER.CastIfHitchanceEquals(target, HitChance.Dashing, true);
            QWER.CastIfHitchanceEquals(target, HitChance.Immobile, true);
            QWER.CastIfWillHit(target, 2, true);

            float SiteToSite = ((target.MoveSpeed * QWER.Delay) + (Player.Distance(target.ServerPosition) / QWER.Speed)) * 6 - QWER.Width;
            float BackToFront = ((target.MoveSpeed * QWER.Delay) + (Player.Distance(target.ServerPosition) / QWER.Speed));
            if (ObjectManager.Player.Distance(waypoints.Last<Vector2>().To3D()) < SiteToSite || ObjectManager.Player.Distance(target.Position) < SiteToSite)
                QWER.CastIfHitchanceEquals(target, HitChance.High, true);
            else if (target.Path.Count() < 2
                && (target.ServerPosition.Distance(waypoints.Last<Vector2>().To3D()) > SiteToSite
                || Math.Abs(ObjectManager.Player.Distance(waypoints.Last<Vector2>().To3D()) - ObjectManager.Player.Distance(target.Position)) > BackToFront
                || target.HasBuffOfType(BuffType.Slow) || target.HasBuff("Recall")
                || (target.Path.Count() == 0 && target.Position == target.ServerPosition)
                ))
            {
                if (target.IsFacing(ObjectManager.Player) || target.Path.Count() == 0)
                {
                    if (ObjectManager.Player.Distance(target.Position) < QWER.Range - ((target.MoveSpeed * QWER.Delay) + (Player.Distance(target.Position) / QWER.Speed)))
                        QWER.CastIfHitchanceEquals(target, HitChance.High, true);
                }
                else
                {
                    QWER.CastIfHitchanceEquals(target, HitChance.High, true);
                }
            }
        }







        private static void AntiGapcloser_onEnemycloser(ActiveGapcloser closer)
        {
            if (E.IsReady())
            E.CastIfHitchanceEquals(closer.Sender, HitChance.High, true);
        }



        //Main.
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnLoad;
        }
    }
}
