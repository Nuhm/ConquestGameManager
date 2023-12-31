namespace ConquestGameManager.Webhook
{
    public class Author
    {
        public string name { get; set; }
        public string url { get; set; }
        public string icon_url { get; set; }

        public Author(string name, string url, string icon_url)
        {
            this.name = name;
            this.url = url;
            this.icon_url = icon_url;
        }
    }
}