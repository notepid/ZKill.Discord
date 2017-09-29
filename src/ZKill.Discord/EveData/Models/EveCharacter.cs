namespace ZKill.Discord.EveData.Models
{
    public class EveCharacter
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long CorporationId { get; set; }
        public long AllianceId { get; set; }
    }
}
