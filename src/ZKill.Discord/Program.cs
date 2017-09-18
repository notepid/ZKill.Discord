using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using ZKill.Discord.Discord;
using ZKill.Discord.Logging;
using ZKill.Discord.Models;
using ZKill.Discord.ZKill;

namespace ZKill.Discord
{
    class Program
    {
        private static ILogger _logger;
        private static DiscordClient _discordClient;
        private static DiscordClient _discordClientHighValue;

        static void Main(string[] args)
        {
            _logger = new ConsoleLogger();

            var appConfig = JsonConvert.DeserializeObject<AppConfiguration>(File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "appsettings.json")));

            _discordClient = new DiscordClient(appConfig.DiscordWebHookUrl, _logger);
            _discordClientHighValue = new DiscordClient(appConfig.HighValueKills.DiscordWebHookUrlHighValue, _logger);

            var numberFormatingCulture = new CultureInfo("en-US");

            _logger.Log("Staring ZKill.Discord");
            _logger.Log("Press Q to quit.");
            var client = new ZKillboardClient(_logger);

            do
            {
                while (!Console.KeyAvailable)
                {
                    var killMail = client.GetNextKillMail();

                    Console.WriteLine();
                    _logger.Log($"Got new ZKill: ID = {killMail.KillId} - {killMail.KillTime}");
                    _logger.Log($"{killMail.KbUrl}");

                    _logger.Log($"\tVictim: {killMail.VictimName} flying a {killMail.ShipTypeName} in system \"{killMail.SystemName}\"");
                    _logger.Log($"\t\t{killMail.DamageTaken} damage taken");
                    _logger.Log($"\t\tFitted value: {killMail.FittedValue.ToString("N0", numberFormatingCulture)} ISK");
                    _logger.Log($"\t\tTotal value: {killMail.TotalValue.ToString("N0", numberFormatingCulture)} ISK");

                    _logger.Log("\tAttackers:");
                    foreach (var attacker in killMail.Attackers)
                    {
                        _logger.Log($"\t\t{attacker.Name} in a {attacker.ShipTypeName}");
                        if (appConfig.WatchedCharacters.Any(w => w.EveCharacterId == attacker.CharacterId))
                        {
                            var character = appConfig.WatchedCharacters.First(c => c.EveCharacterId == attacker.CharacterId);
                            _logger.Log(LoggingEventType.Warning, $"{character.CharacterName}({character.StreamerName}) killed {killMail.VictimName}!!!");

                            var killType = killMail.Attackers.Count == 1 ? "solo kill" : "fleet kill";

                            var message =
                                $"{character.StreamerName} scored a {killType} against {killMail.VictimName}!\n" +
                                $"Eve Time: {killMail.KillTime}\n" +
                                $"KB: {killMail.KbUrl}\n" +
                                $"{killMail.VictimName} was in a {killMail.ShipTypeName} and tanked {killMail.DamageTaken} damage\n" +
                                $"Total worth: {killMail.TotalValue.ToString("N0", numberFormatingCulture)} ISK";
                            _discordClient.SendTextMessage(message);
                        }
                    }

                    if (appConfig.WatchedCharacters.Any(w => w.EveCharacterId == killMail.CharacterId))
                    {
                        var character = appConfig.WatchedCharacters.First(c => c.EveCharacterId == killMail.CharacterId);
                        _logger.Log(LoggingEventType.Warning, $"{character.StreamerName} got killed while playing character {character.CharacterName}");

                        var message =
                            $"{character.StreamerName} got killed in a {killMail.ShipTypeName} :( \n" +
                            $"Eve Time: {killMail.KillTime}\n" +
                            $"KB: {killMail.KbUrl}\n" +
                            $"{killMail.VictimName} was in a {killMail.ShipTypeName} and tanked {killMail.DamageTaken} damage\n" +
                            $"Total worth: {killMail.TotalValue.ToString("N0", numberFormatingCulture)} ISK";
                        _discordClient.SendTextMessage(message);
                    }

                    if (appConfig.HighValueKills.Enabled && killMail.TotalValue > appConfig.HighValueKills.Value)
                    {
                        _logger.Log(LoggingEventType.Warning, "High value kills detected!");

                        var message =
                            "**High value kill!**\n" +
                            $"{killMail.VictimName} got killed in a {killMail.ShipTypeName} and tanked {killMail.DamageTaken} damage\n" +
                            $"Eve Time: {killMail.KillTime}\n" +
                            $"KB: {killMail.KbUrl}\n" +
                            $"Total worth: {killMail.TotalValue.ToString("N0", numberFormatingCulture)} ISK";
                        _discordClientHighValue.SendTextMessage(message);
                    }

                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Q);
        }
    }
}
