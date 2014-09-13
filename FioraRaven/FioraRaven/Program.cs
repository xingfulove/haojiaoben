﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace FioraRaven
{
    class Fiora
    {
        public static String champName = "Fiora";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static Menu menu;
        public static Obj_AI_Hero tar;
        public static Dictionary<string, SpellSlot> spellData;
        public static DZApi api = new DZApi();
        static void Main(string[] args)
        {
            try
            {
                CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
        }
        static void Game_OnGameLoad(EventArgs args)
        {
            if (player.BaseSkinName != champName) return;
            spellData = new Dictionary<string, SpellSlot>();
            menu = new Menu("Fiora Raven","FioraRMenu",true);
            menu.AddSubMenu(new Menu("Orbwalker","Orbwalker1"));
            Orbwalker = new Orbwalking.Orbwalker(menu.SubMenu("Orbwalker1"));
            var ts = new Menu("Target Selector","TargetSelector");
            SimpleTs.AddToMenu(ts);
            menu.AddSubMenu(ts);
            menu.AddSubMenu(new Menu("Fiora Combo", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            menu.AddSubMenu(new Menu("Fiora Harrass", "Harrass"));
            menu.SubMenu("Harrass").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            menu.SubMenu("Harrass").AddItem(new MenuItem("UseW", "Use W"));
            menu.SubMenu("Harrass").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            menu.AddSubMenu(new Menu("Fiora Misc", "Misc"));
            menu.SubMenu("Misc").AddItem(new MenuItem("WBlock", "Use W to block Autos").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("RDodge", "Use R to dodge dangerous").SetValue(true));
            menu.AddSubMenu(new Menu("Fiora Items", "Item"));
            menu.SubMenu("Item").AddItem(new MenuItem("Botrk", "Use Botrk").SetValue(true));
            menu.SubMenu("Item").AddItem(new MenuItem("Youmuu", "Use Youmuu").SetValue(true));
            menu.SubMenu("Item").AddItem(new MenuItem("Tiamat", "Use Tiamat").SetValue(true));
            menu.SubMenu("Item").AddItem(new MenuItem("Hydra", "Use Hydra").SetValue(true));
            menu.SubMenu("Item").AddItem(new MenuItem("OwnHPercBotrk", "Min Own H % Botrk").SetValue(new Slider(50, 1, 100)));
            menu.SubMenu("Item").AddItem(new MenuItem("EnHPercBotrk", "Min Enemy H % Botrk").SetValue(new Slider(20, 1, 100)));
            menu.SubMenu("Item").AddItem(new MenuItem("ItInComb", "Use Items In Combo").SetValue(true));
            menu.AddSubMenu(new Menu("Fiora Dangerous Spells", "DangSpells"));
            Dictionary<String, String> dSpellsDName = api.getDanSpellsName();
            foreach (KeyValuePair<string, string> entry in dSpellsDName)
            {
                menu.SubMenu("DangSpells").AddItem(new MenuItem(entry.Key, entry.Value).SetValue(true));
            }
            menu.AddSubMenu(new Menu("Fiora Drawing", "Drawing"));
            menu.SubMenu("Drawing").AddItem(new MenuItem("DrQ", "Draw Q"));
            menu.SubMenu("Drawing").AddItem(new MenuItem("DrR", "Draw R"));
           
            Game.PrintChat("Fiora The Raven By DZ191 Loaded ");
            Obj_AI_Base.OnProcessSpellCast += Game_ProcessSpell;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Orbwalking.OnAttack += Orbwalking_OnAttack;
            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W, float.MaxValue);
            E = new Spell(SpellSlot.E, float.MaxValue);
            R = new Spell(SpellSlot.R, 400f);
            menu.AddToMainMenu();
        }

       
        public static void Game_ProcessSpell(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args)
        {
        }
        static void Orbwalking_OnAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (unit.IsMe)
            {
                tar = (Obj_AI_Hero)target;
                if (isEn("Botrk") && isCombo() && target.IsValidTarget()&&isEn("ItInComb"))
                {
                    float OwnH = api.getPlHPer();
                    if ((menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= OwnH) && ((menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= api.getEnH(tar))))
                    {
                        api.useItem(3153, tar);
                    }
                }
                if (isEn("Youmuu") && isCombo() && target.IsValidTarget() && isEn("ItInComb"))
                {
                    api.useItem(3142, tar);
                }
            }
        }
        public static void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (unit.IsMe)
            {
                tar = (Obj_AI_Hero)target;
                if (isCombo() && E.IsReady() && target.IsValidTarget())
                {
                    E.Cast();
                }
                if (menu.Item("Tiamat").GetValue<bool>() && isCombo())
                {
                    int itemId = 3077;
                    api.useItem(itemId);
                }
                if (menu.Item("Hydra").GetValue<bool>() && isCombo())
                {
                    int itemId = 3074;
                    api.useItem(itemId);
                }
            }
        }
        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (player.HasBuff("FioraQLunge",true))
            {
                Game.PrintChat("Buff");
            }
            if (isCombo()) { 
                var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
                CastQ(target);
                
                
                if ((R.GetDamage(target) >= target.Health))
                {
                    CastR(target);
                }
            }
            
        }
        public static void CastQ(Obj_AI_Hero target)
        {        
            if (target == null) return;
            if(target.IsValidTarget(Q.Range) && Q.IsReady()&&Q.InRange(target.Position))
            {
                Q.Cast(target, true, false);
            }
        }
        public static void CastR(Obj_AI_Hero target)
        {
            if (isCombo() && target.IsValidTarget() && R.InRange(target.Position) && (R.GetDamage(target) >= target.Health))
            {
                R.Cast(target,true);
            }
        }
        public static bool isCombo()
        {
            return Orbwalker.ActiveMode.ToString() == "Combo";
        }
        public static bool isEn(String item)
        {
            return menu.Item(item).GetValue<bool>();
        }
    }
}