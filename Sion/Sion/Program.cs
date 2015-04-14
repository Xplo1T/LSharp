using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
using System.Reflection;

namespace Sion
{
     class Program
    {
        private const string championName = "Sion";
        private static Obj_AI_Hero Player;
        private static Obj_AI_Hero Target;
        private static Spell Q, W, E, R;

        //
        private static Menu menu;
        private static Orbwalking.Orbwalker orbwalker;




        private static void Game_OnLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != championName)
            { 
                return;
            }


            Q = new Spell(SpellSlot.Q, 550f);
            Q.SetSkillshot(0.5f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);
            Q.SetCharged("CrypticGaze","CrypticGaze",550,720,0.5f);
            W = new Spell(SpellSlot.W,0);
            E = new Spell(SpellSlot.E, 1800);
            E.SetSkillshot(0.25f, 80f, 1800, false, SkillshotType.SkillshotLine);
            // fix?
            R = new Spell(SpellSlot.R, 800);
            R.SetSkillshot(500f, 120f, 1000f, true, SkillshotType.SkillshotLine);



            //Menu:

            menu = new Menu("Sion", "Sion", true);
            orbwalker = new Orbwalking.Orbwalker(menu.SubMenu("Orbwalking"));
            Menu menuTS = new Menu("Target Select", "Target Select");
            TargetSelector.AddToMenu(menuTS);
            menu.AddSubMenu(menuTS);

            menu.AddSubMenu(new Menu("Combo", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("Use Q", "Use Q").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("Use W", "Use W").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("Use E", "Use E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("Use R", "Use R").SetValue(true));
            menu.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ComboA", "Combo!").SetValue(
                        new KeyBind(menu.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            menu.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
            Game.PrintChat("Sion " +  Assembly.GetExecutingAssembly().GetName().Version + " Loaded!");


        } //Game_OnGameLoad


        private static void Game_OnUpdate(EventArgs args)
        {
            if (menu.Item("ComboA").GetValue<KeyBind>().Active)
            {
                Obj_AI_Hero QTarget = TargetSelector.GetTarget(Q.ChargedMaxRange, TargetSelector.DamageType.Physical);
                Obj_AI_Hero ETarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                Obj_AI_Hero RTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

                if (QTarget != null && menu.Item("Use Q").GetValue<bool>())
                {
                    if (Q.ChargeDuration>=1.1)
                    {
                        CastSpell(Q, QTarget);
                    }

                if (Q.IsReady())
                {
                
                    Q.StartCharging(QTarget.ServerPosition);
                }

                }//QEnd.

            if (ETarget != null && menu.Item("Use E").GetValue<bool>() && E.IsReady())
            {
                CastSpell(E, ETarget);
            }

            if (QTarget != null && menu.Item("Use W").GetValue<bool>() && W.IsReady())
            {
                W.Cast();
            }

            }

            

        }



        //THANKS SEBBY (ONEKEYTOWIN).
        private static void CastSpell(Spell QWER, Obj_AI_Hero target)
        {
           
                List<Vector2> waypoints = target.GetWaypoints();
                //debug("" + target.Path.Count() + " " + (target.Position == target.ServerPosition) + (waypoints.Last<Vector2>().To3D() == target.ServerPosition));
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



       private static void Main(string[] args)
        {

            CustomEvents.Game.OnGameLoad += Game_OnLoad;


        }
    }
}
