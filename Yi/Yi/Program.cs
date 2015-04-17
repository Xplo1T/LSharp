using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
 
namespace Yi
{
    class Program
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;
        public static string ChampionName = "MasterYi";
        public static Obj_AI_Hero Player;
        public static Spell Q;
        public static Spell W;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
 
        static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
 
            if (Player.ChampionName != ChampionName) return;
 
            Config = new Menu(ChampionName, ChampionName, true);
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Menu menuTS = new Menu("Target Select", "Target Select");
            TargetSelector.AddToMenu(menuTS);
            Config.AddSubMenu(menuTS);
            Config.AddToMainMenu();
 
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
 
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
 
            Game.PrintChat("Vervorris Yi loaded!");
        }
 
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            W.Cast();
            var Target = TargetSelector.GetTarget(300, TargetSelector.DamageType.Physical);
            Player.IssueOrder(GameObjectOrder.MoveTo, target.Position);
            //Player.IssueOrder(GameObjectOrder.)
            //Game.PrintChat("y");
        }
    }
}