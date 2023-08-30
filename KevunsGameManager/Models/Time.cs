namespace KevunsGameManager.Models
{
    public class Time
    {
        public float Dawn { get; set; }
        public float Day { get; set; }
        public float Dusk { get; set; }
        public float Night { get; set; }

        public Time(float dawn, float day, float dusk, float night)
        {
            Dawn = dawn;
            Day = day;
            Dusk = dusk;
            Night = night;
        }

        public Time() { }
    }
}