using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;

namespace Nidalee
{
    class Program
    {
        private const string championName = "Nidalee";
        private static Menu nidMenu;
        private static Menu tsMenu;
        private static Menu qMenu;
        private static Obj_AI_Hero Player;
        private static Obj_AI_Hero Target;

        //Spells
        private static Spell Q;

        static void Game_OnLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != championName)
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 1500f);

            Q.SetSkillshot(0.125f,40f,1300f,true,SkillshotType.SkillshotLine);


            nidMenu = new Menu("Nidalee", "Nidalee", true);
            tsMenu = new Menu("Target Selector", "Target Selector");
            qMenu = new Menu("Q Cast", "Q Cast");
            TargetSelector.AddToMenu(tsMenu);
            nidMenu.AddSubMenu(tsMenu);
            nidMenu.AddSubMenu(qMenu);
            qMenu.AddItem(new MenuItem("Cast Q", "Cast Q").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            nidMenu.AddToMainMenu();
            Game.PrintChat("Nidalee version 1.0.0.0 Loaded.");

            Game.OnUpdate += Game_OnUpdate;
        }


        static void Game_OnUpdate(EventArgs args)
        {
            if (nidMenu.Item("Cast Q").GetValue<KeyBind>().Active)
            {

                Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (Target != null)
                    CastQ(Q, Target);
            }
        }


        //Credits to Sebby (OKTW) <3
        static void CastQ(Spell q, Obj_AI_Hero target)
        {
            /*
            if (q.Delay < 0.25)
            {
                Q.CastIfHitchanceEquals(target, HitChance.Immobile, true);
            }
             */

            List<Vector2> waypoint = target.GetWaypoints();
            //SitetoSite
            float StS = ((target.MoveSpeed * Q.Delay) +  (Player.Distance(target.ServerPosition) / Q.Speed)) * 6 - Q.Width;
            //Backtofront
            float BtF = ((target.MoveSpeed * Q.Delay) + (Player.Distance(target.ServerPosition) / Q.Speed)) * 5;
            if (ObjectManager.Player.Distance(waypoint.Last<Vector2>().To3D()) < StS || ObjectManager.Player.Distance(target.Position) < StS)
                q.CastIfHitchanceEquals(target, HitChance.VeryHigh, true);
            else if (target.Path.Count() < 2
                    && (target.ServerPosition.Distance(waypoint.Last<Vector2>().To3D()) > StS
                    || (ObjectManager.Player.Distance(waypoint.Last<Vector2>().To3D()) - ObjectManager.Player.Distance(target.Position)) < 0 - BtF
                    || (ObjectManager.Player.Distance(waypoint.Last<Vector2>().To3D()) - ObjectManager.Player.Distance(target.Position)) > (target.MoveSpeed * q.Delay)
                    || target.Path.Count() == 0))
            {

                if (ObjectManager.Player.Distance(waypoint.Last<Vector2>().To3D()) <= ObjectManager.Player.Distance(target.Position))
                {
                    if (ObjectManager.Player.Distance(target.ServerPosition) < q.Range - ((target.MoveSpeed * q.Delay) + (Player.Distance(target.ServerPosition) / q.Speed)))
                        q.CastIfHitchanceEquals(target, HitChance.High, true);
                }
                else
                {
                    q.CastIfHitchanceEquals(target, HitChance.High, true);
                }

            }

        }



















        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnLoad;
        }
    }
}
