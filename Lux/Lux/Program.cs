using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;

namespace Lux
{
    class Program
    {
        private const string championName = "Lux";
        private static Obj_AI_Hero Player;
        private static Obj_AI_Hero Target;
        public static Menu menu;
        //Spells
        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;


        static void Game_OnGameLoad(EventArgs args)
        {


            Player = ObjectManager.Player;
            if (Player.BaseSkinName != championName)
            {
                return;
            }

            Game.OnGameUpdate += Game_OnGameUpdate;
            //Skills:

            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 3340);

            Q.SetSkillshot(0.25f, 80f, 1200f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 150f, 1200f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 275f, 1300f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1.35f, 190f, float.MaxValue, false, SkillshotType.SkillshotLine);



            //Menu:

            menu = new Menu("Lux", "Lux", true);
            Menu MenuTS = new Menu("Target S", "Target S");
            TargetSelector.AddToMenu(MenuTS);
            menu.AddSubMenu(MenuTS);
            Menu UseQ = new Menu("Use Q", "Use Q");
            Menu UseR = new Menu("Use R", "Use R");
            menu.AddSubMenu(UseQ);
            UseQ.AddItem(new MenuItem("useQ Active", "UseQ Key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            menu.AddSubMenu(UseR);
            UseR.AddItem(new MenuItem("useR Active", "useR Key").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Press)));


            menu.AddToMainMenu();

            Game.PrintChat("<font color=\"#00BFFF\">Lux ver. 1.0.0.9 Loaded.</font>");



        } //endOf: Game_OnGameLoad()

        public static bool AnalyzeQ(PredictionInput input, PredictionOutput output)
        {
            var posList = new List<Vector3> { ObjectManager.Player.ServerPosition, output.CastPosition };
            var collision = Collision.GetCollision(posList, input);
            var minions = collision.Count(collisionObj => collisionObj.IsMinion);
            return minions > 1;
        }


        private static void Game_OnGameUpdate(EventArgs args)
        {

            Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            //if (sivirOb.ActiveMode.ToString() == "Combo" && Q.IsReady() && sivirMenu.Item("combo.use.q").GetValue<bool>()==true)

            //  Config.Item(s).GetValue<KeyBind>().Active;




            if (menu.Item("useQ Active").GetValue<KeyBind>().Active)
            {
                var prediction = Q.GetPrediction(Target, true);
                var minions = prediction.CollisionObjects.Count(thing => thing.IsMinion);
                Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (Target != null && !(minions > 1))
                {
                    //Q.CastIfHitchanceEquals(Target, HitChance.VeryHigh, true);
                    CastSpell(Q, Target, 3);
                }


            }



            if (menu.Item("useR Active").GetValue<KeyBind>().Active)
            {
                Target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                if (Target != null && R.IsReady())
                {
                    if (R.GetPrediction(Target).Hitchance >= HitChance.High)
                        CastSpell(R, Target, 0);
                }
            }




        } //endOf: Game_OnGameUpdate()


        /*
            public static bool AnalyzeQ(PredictionInput input, PredictionOutput output)
            {
                var posList = new List<Vector3> { ObjectManager.Player.ServerPosition, output.CastPosition };
                var collision = Collision.GetCollision(posList, input);
                var minions = collision.Count(collisionObj => collisionObj.IsMinion);
                return minions > 1;
            }

            public static void CastQ(Obj_AI_Hero target)
            {
                Console.Clear();

                var prediction = Q.GetPrediction(target, true);
                var minions = prediction.CollisionObjects.Count(thing => thing.IsMinion);

                if (minions > 1)
                {
                    return;
                }

                Q.Cast(prediction.CastPosition, true);
            }
         */


        private static void CastSpell(Spell QWER, Obj_AI_Hero target, int HitChanceNum)
        {
            //HitChance 0 - 3
            // example CastSpell(Q, target, 3);
            if (HitChanceNum == 0)
                QWER.Cast(target, true);
            else if (HitChanceNum == 1)
                QWER.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
            else if (HitChanceNum == 2)
            {
                if (QWER.Delay < 0.3)
                    QWER.CastIfHitchanceEquals(target, HitChance.Dashing, true);
                QWER.CastIfHitchanceEquals(target, HitChance.Immobile, true);
                QWER.CastIfWillHit(target, 2, true);
                if (target.Path.Count() < 2)
                    QWER.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
            }
            else if (HitChanceNum == 3)
            {
                if (QWER.Delay < 0.3)
                    QWER.CastIfHitchanceEquals(target, HitChance.Dashing, true);
                QWER.CastIfHitchanceEquals(target, HitChance.Immobile, true);
                QWER.CastIfWillHit(target, 2, true);

                List<Vector2> waypoints = target.GetWaypoints();
                float SiteToSite = ((target.MoveSpeed * QWER.Delay) + (Player.Distance(target.ServerPosition) / QWER.Speed)) * 6 - QWER.Width;
                float BackToFront = ((target.MoveSpeed * QWER.Delay) + (Player.Distance(target.ServerPosition) / QWER.Speed)) * 5;
                if (ObjectManager.Player.Distance(waypoints.Last<Vector2>().To3D()) < SiteToSite || ObjectManager.Player.Distance(target.Position) < SiteToSite)
                    QWER.CastIfHitchanceEquals(target, HitChance.High, true);
                else if (target.Path.Count() < 2
                    && (target.ServerPosition.Distance(waypoints.Last<Vector2>().To3D()) > SiteToSite
                    || (ObjectManager.Player.Distance(waypoints.Last<Vector2>().To3D()) - ObjectManager.Player.Distance(target.Position)) < 0 - BackToFront
                    || (ObjectManager.Player.Distance(waypoints.Last<Vector2>().To3D()) - ObjectManager.Player.Distance(target.Position)) > (target.MoveSpeed * QWER.Delay)
                    || target.Path.Count() == 0))
                {
                    if (ObjectManager.Player.Distance(waypoints.Last<Vector2>().To3D()) <= ObjectManager.Player.Distance(target.Position))
                    {
                        if (ObjectManager.Player.Distance(target.ServerPosition) < QWER.Range - ((target.MoveSpeed * QWER.Delay) + (Player.Distance(target.ServerPosition) / QWER.Speed)))
                            QWER.CastIfHitchanceEquals(target, HitChance.High, true);
                    }
                    else
                    {
                        QWER.CastIfHitchanceEquals(target, HitChance.High, true);
                    }

                }
            }
        }













        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

        }
    }
}
