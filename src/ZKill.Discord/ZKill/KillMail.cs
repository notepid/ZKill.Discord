using System;
using System.Collections.Generic;

namespace ZKill.Discord.ZKill
{
    public class KillMail
    {
        public int KillId { get; set; }
        public DateTime KillTime { get; set; }
        public float FittedValue { get; set; }
        public float TotalValue { get; set; }
        public string KbUrl { get; set; }
        public string VictimName { get; set; }
        public int? CharacterId { get; set; }
        public string ShipTypeName { get; set; }
        public string SystemName { get; set; }
        public int DamageTaken { get; set; }

        public List<Item> Items { get; set; }
        public List<Attacker> Attackers { get; set; }
    }
}
