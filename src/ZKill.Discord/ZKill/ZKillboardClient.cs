using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using ZKill.Discord.EveData;
using ZKill.Discord.Logging;

namespace ZKill.Discord.ZKill
{
    public class ZKillboardClient
    {
        private readonly ILogger _logger;
        private readonly EveDataStore _eveDataStore;
        private readonly EveApiClient _eveApiClient;
        private static readonly HttpClient httpClient = new HttpClient();

        public ZKillboardClient(ILogger logger, EveDataStore eveDataStore, EveApiClient eveApiClient)
        {
            _logger = logger;
            _eveDataStore = eveDataStore;
            _eveApiClient = eveApiClient;
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
                killMail.KillTime = dynamicResult.package.killmail.killmail_time;
                killMail.SystemName = _eveDataStore.GetSystemNameFromId((long)dynamicResult.package.killmail.solar_system_id);
                killMail.FittedValue = dynamicResult.package.zkb.fittedValue;
                killMail.TotalValue = dynamicResult.package.zkb.totalValue;
                killMail.KbUrl = $"https://zkillboard.com/kill/{(string)dynamicResult.package.killID}/";
                killMail.ShipTypeName =  _eveDataStore.GetItemNameFromId((long)dynamicResult.package.killmail.victim.ship_type_id);
                killMail.DamageTaken = dynamicResult.package.killmail.victim.damage_taken;
                killMail.CharacterId = dynamicResult.package.killmail.victim.character_id != null ? dynamicResult.package.killmail.victim.character_id : null;

                if (dynamicResult.package.killmail.victim.character_id != null)
                {
                    var victim = _eveApiClient.GetCharacterById((long)dynamicResult.package.killmail.victim.character_id);
                    killMail.VictimName = victim != null ? victim.Name : "Unknown";
                }
                else if (dynamicResult.package.killmail.victim.alliance_id != null)
                {
                    var alliance = _eveApiClient.GetAllianceById((long)dynamicResult.package.killmail.victim.alliance_id);
                    killMail.VictimName = alliance != null ? alliance.Name : "Unknown";
                }
                else
                {
                    var corporation = _eveApiClient.GetCorporationById((long)dynamicResult.package.killmail.victim.corporation_id);
                    killMail.VictimName = corporation != null ? corporation.Name : "Unknown";
                }

                killMail.Attackers = new List<Attacker>();

                foreach (var attacker in dynamicResult.package.killmail.attackers)
                {
                    killMail.Attackers.Add(new Attacker
                    {
                        //Name = attacker.character != null ? attacker.character.name : "(Unknown)",
                        ShipTypeName = attacker.ship_type_id != null ? _eveDataStore.GetItemNameFromId((long)attacker.ship_type_id) : "(Unknown)",
                        CharacterId = attacker.character_id != null ? attacker.character_id : null
                    });
                }
                    ;

                result = killMail;
            } while (result == null);
            return result;
        }
    }
}
