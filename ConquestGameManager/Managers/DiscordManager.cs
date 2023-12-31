using Newtonsoft.Json;
using System.Net;
using System.Text;
using ConquestGameManager.Webhook;

namespace ConquestGameManager.Managers
{
    public class DiscordManager
    {
        public static void SendEmbed(Embed embed, string name, string webhookurl)
        {
            Message webhookMessage = new Message(name, null, new Embed[1] { embed });
            SendHook(webhookMessage, webhookurl);
        }

        public static void SendHook(Message embed, string webhookUrl)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(embed));
            using (WebClient webClient = new WebClient())
            {
                WebHeaderCollection headers = webClient.Headers;
                headers.Set(HttpRequestHeader.ContentType, "application/json");
                webClient.UploadData(webhookUrl, bytes);
            }
        }
    }
}