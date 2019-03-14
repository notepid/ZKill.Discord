using System;
using System.Net.Http;
using Newtonsoft.Json;
using ZKill.Discord.EveData.Models;

namespace ZKill.Discord.EveData
{
    public class EveApiClient
    {
        private const string ApiHostAdress = "https://esi.evetech.net";

        private static readonly HttpClient Client = new HttpClient
        {
            BaseAddress = new Uri(ApiHostAdress)
        };

        public EveApiClient()
        {
            Client.DefaultRequestHeaders.Clear();
        }

        public EveCharacter GetCharacterById(long id)
        {
            var result = Client.GetAsync($"/v4/characters/{id}/?datasource=tranquility").Result;
            if (!result.IsSuccessStatusCode)
                return null;

            var apiResultString = result.Content.ReadAsStringAsync().Result;
            var apiResult = JsonConvert.DeserializeObject<EveCharacterDto>(apiResultString);

            return new EveCharacter
            {
                Name = apiResult.name,
                Id = id,
                CorporationId = apiResult.corporation_id,
                AllianceId = apiResult.alliance_id
            };
        }

        public EveAlliance GetAllianceById(long id)
        {
            var result = Client.GetAsync($"/v2/alliances/{id}/?datasource=tranquility").Result;
            if (!result.IsSuccessStatusCode)
                return null;

            var apiResultString = result.Content.ReadAsStringAsync().Result;
            var apiResult = JsonConvert.DeserializeObject<EveAllianceDto>(apiResultString);

            return new EveAlliance
            {
                Name = apiResult.alliance_name,
                Ticker = apiResult.ticker
            };
        }

        public EveCorporation GetCorporationById(long id)
        {
            var result = Client.GetAsync($"/v3/corporations/{id}/?datasource=tranquility").Result;
            if (!result.IsSuccessStatusCode)
                return null;

            var apiResultString = result.Content.ReadAsStringAsync().Result;
            var apiResult = JsonConvert.DeserializeObject<EveCorporationDto>(apiResultString);

            return new EveCorporation
            {
                Name = apiResult.corporation_name,
                Ticker = apiResult.ticker,
                AllianceId = apiResult.alliance_id
            };
        }

        // ReSharper disable InconsistentNaming
        // ReSharper disable ClassNeverInstantiated.Local
        private class EveCharacterDto
        {
            public long corporation_id { get; set; }
            public string name { get; set; }
            public long alliance_id { get; set; }
        }

        private class EveAllianceDto
        {
            public string alliance_name { get; set; }
            public string ticker { get; set; }
        }

        private class EveCorporationDto
        {
            public string corporation_name { get; set; }
            public string ticker { get; set; }
            public long alliance_id { get; set; }
        }
        // ReSharper restore ClassNeverInstantiated.Local
        // ReSharper restore InconsistentNaming
    }
}
