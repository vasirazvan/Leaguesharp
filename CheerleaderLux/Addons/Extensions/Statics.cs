using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SPrediction;
using SharpDX;

namespace CheerleaderLux.Addons.Extensions
{
    public class Statics
    {
        public static Menu Config;
        public static readonly List<Obj_AI_Base> Attackers = new List<Obj_AI_Base>();
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q;
        public static Spell E;
        public static Spell W;
        public static Spell R;
        public static GameObject Lux_E;
        public static SpellSlot Ignite;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static readonly Obj_AI_Hero Player = ObjectManager.Player;
    }
}
