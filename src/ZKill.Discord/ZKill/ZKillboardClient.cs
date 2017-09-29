using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using ZKill.Discord.Logging;

namespace ZKill.Discord.ZKill
{
    public class ZKillboardClient
    {
        private readonly ILogger _logger;
        private static readonly HttpClient httpClient = new HttpClient();

        public ZKillboardClient(ILogger logger)
        {
            _logger = logger;
            httpClient.BaseAddress = new Uri("https://redisq.zkillboard.com");
        }

        public KillMail GetNextKillMail()
        {
            KillMail result = null;

            do
            {
                var httpResult = httpClient.GetAsync("/listen.php").Result;
                if (!httpResult.IsSuccessStatusCode)
                {
                    _logger.Log(LoggingEventType.Error, "HttpClient Error.");
                    continue;
                }

                var resultString = httpResult.Content.ReadAsStringAsync().Result;
                dynamic dynamicResult = JsonConvert.DeserializeObject(resultString);
                if (dynamicResult.package == null)
                {
                    //_logger.Log("No new kill mail.");
                    continue;
                }

                var killMail = new KillMail();
                killMail.KillId = dynamicResult.package.killID;
                killMail.KillTime = dynamicResult.package.killmail.killTime != null ? dynamicResult.package.killmail.killTime : DateTime.MinValue;
                killMail.SystemName = dynamicResult.package.killmail.solarSystem.name;
                killMail.FittedValue = dynamicResult.package.zkb.fittedValue;
                killMail.TotalValue = dynamicResult.package.zkb.totalValue;
                killMail.KbUrl = $"https://zkillboard.com/kill/{(string)dynamicResult.package.killID}/";
                killMail.ShipTypeName = dynamicResult.package.killmail.victim.shipType.name;
                killMail.DamageTaken = dynamicResult.package.killmail.victim.damageTaken;
                killMail.CharacterId = dynamicResult.package.killmail.victim.character != null ? dynamicResult.package.killmail.victim.character.id : null;

                if (dynamicResult.package.killmail.victim.character != null)
                    killMail.VictimName = (string)dynamicResult.package.killmail.victim.character.name;
                else if (dynamicResult.package.killmail.victim.aliance != null)
                    killMail.VictimName =
                        $"(Alliance) {(string)dynamicResult.package.killmail.victim.aliance.name}";
                else
                    killMail.VictimName =
                        $"(Corporation) {(string)dynamicResult.package.killmail.victim.corporation.name}";

                killMail.Attackers = new List<Attacker>();

                foreach (var attacker in dynamicResult.package.killmail.attackers)
                {
                    killMail.Attackers.Add(new Attacker
                    {
                        Name = attacker.character != null ? attacker.character.name : "(Unknown)",
                        ShipTypeName = attacker.shipType != null ? attacker.shipType.name : "(Unknown)",
                        CharacterId = attacker.character != null ? attacker.character.id : null
                    });
                }
                    ;

                result = killMail;
            } while (result == null);
            return result;
        }
    }
}
