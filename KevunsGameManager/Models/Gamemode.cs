namespace KevunsGameManager.Models
{
    public class Gamemode
    {
        public string Name { get; set; }
        public int Duration { get; set; }
        public bool HasTeams { get; set; }
        public bool IsEnabled { get; set; }
    }
}
