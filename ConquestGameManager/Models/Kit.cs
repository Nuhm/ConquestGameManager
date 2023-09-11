namespace ConquestGameManager.Models
{
    public class Kit
    {
        public string KitName { get; set; }
        public int KitCooldownSeconds { get; set; }
        public bool ResetCooldownOnDie { get; set; }
        public bool WipeInventoryWhenClaim { get; set; }
        public bool HasCooldown { get; set; }

        public Kit()
        {

        }

        public Kit(string kitName, int kitCooldownSeconds, bool resetCooldownOnDie, bool wipeInventoryWhenClaim, bool hasCooldown)
        {
            KitName = kitName;
            KitCooldownSeconds = kitCooldownSeconds;
            ResetCooldownOnDie = resetCooldownOnDie;
            WipeInventoryWhenClaim = wipeInventoryWhenClaim;
            HasCooldown = hasCooldown;
        }
    }
}