using System.Collections.Generic;

namespace ZKill.Discord.Models
{
    public class AppConfiguration
    {
        public string DiscordWebHookUrl { get; set; }
        public HighValueKills HighValueKills { get; set; }
        public List<WatchedCharacter> WatchedCharacters { get; set; }
    }
}
