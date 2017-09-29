using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using ZKill.Discord.EveData.Models;
using ZKill.Discord.Logging;

namespace ZKill.Discord.EveData
{
    public class EveDataStore
    {
        private readonly ILogger _logger;
        public List<EveSolarSystem> EveSolarSystems = new List<EveSolarSystem>();
        public List<EveItem> EveItems = new List<EveItem>();

        public string GetSystemNameFromId(long id)
        {
            var res = EveSolarSystems.FirstOrDefault(x => x.solarSystemID == id);
            return res == null ? "Unknown" : res.solarSystemName;
        }

        public string GetItemNameFromId(long id)
        {
            var res = EveItems.FirstOrDefault(x => x.typeID == id);
            return res == null ? "Unknown" : res.typeName;
        }

        public EveDataStore(ILogger logger)
        {
            _logger = logger;
            var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "data");

            _logger.Log($"Data directory: {dataDirectory}");
            LoadSolarSystems(dataDirectory);
            LoadEveItems(dataDirectory);
        }

        private void LoadSolarSystems(string dataDirectory)
        {
            _logger.Log("Reading Eve Systems... ");
            using (TextReader reader = File.OpenText(Path.Combine(dataDirectory, "mapSolarSystems.csv")))
            {
                var csv = new CsvReader(reader);
                EveSolarSystems = csv.GetRecords<EveSolarSystem>().ToList();
            }
            _logger.Log($"Loaded {EveSolarSystems.Count} systems");
        }

        private void LoadEveItems(string dataDirectory)
        {
            _logger.Log("Reading Eve Items... ");
            using (TextReader reader = File.OpenText(Path.Combine(dataDirectory, "invTypes.csv")))
            {
                var csv = new CsvReader(reader);
                EveItems = csv.GetRecords<EveItem>().ToList();
            }
            _logger.Log($"Loaded {EveItems.Count} items");
        }
    }
}
