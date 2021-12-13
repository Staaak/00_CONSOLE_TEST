#define DEBUG_ON     //ONの場合、DEBUG用のConsole.WriteLineが複数個所で動く
#define TEST_MODE_OFF //ONの場合、proxy OFFかつローカル環境でhtml取得する（ローカルサーバーの起動が必須)
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace CONSOLE_TEST
{
    public class Webhook
    {
        //public const string Image = "";
        public static async Task PostWebhook(string webhookUrl, Content webhookContent)
        {
            string webhookContentJson = JsonConvert.SerializeObject(webhookContent, Formatting.None);
            using HttpClient client = new();
            HttpResponseMessage hrm = null;
            int postWebhookRetryCount = 0;
            bool retry = true;
            while(retry)
            {
                retry = false;
                try
                {
                    StringContent content = new(webhookContentJson, Encoding.UTF8, "application/json");
                    hrm = await client.PostAsync(webhookUrl, content);
                    //NoContent:HTTP ステータス 204 と等価です。 NoContent は、要求が正常に処理され、応答が意図的に空白になっていることを示します。
                    Console.WriteLine(hrm.StatusCode);
                    if ((int)hrm.StatusCode is 204)
                    {
                        return;
                    }
                    else
                    {
                        retry = true;
                        Console.WriteLine($"Error has occurred[We - 1]\nStatus Code Error!!\nstatus code = { (int)hrm.StatusCode }({ hrm.StatusCode})\nretry_num = { postWebhookRetryCount}");
                    }
                }
                catch (Exception e)
                {
                    retry = true;
                }
                postWebhookRetryCount++;
            }
        }
    }

    #region Webhookオブジェクト
    [JsonObject]
    public class Content
    {
        [JsonProperty("username")]
        public string name { get; set; } = "MonitorBot";
        [JsonProperty("avatar_url")]
        public string avatarUrl { get; set; } = "";// Webhook.Image;
        //[JsonProperty("content")]
        //public string content { get; set; }
        [JsonProperty("embeds")]
        public List<Embed> embeds { get; set; }
    }
    [JsonObject]
    public class Embed
    {
        //[JsonProperty("title")]
        //public string title { get; set; }
        //[JsonProperty("description")]
        //public string description { get; set; }
        //[JsonProperty("url")]
        //public string url { get; set; }
        //[JsonProperty("timestamp")]
        //public string timestamp { get; set; }
        //[JsonProperty("author")]
        //public Author author { get; set; }
        //[JsonProperty("image")]
        //public Image image { get; set; }

        [JsonProperty("color")]
        public int color { get; set; }
        [JsonProperty("footer")]
        public Footer footer { get; set; }
        [JsonProperty("thumbnail")]
        public ThumbnailUrl thumbnail { get; set; }
        [JsonProperty("fields")]
        public List<Field> fields { get; set; }
    }
    [JsonObject]
    public class Footer
    {
        [JsonProperty("text")]
        public string text { get; set; } = $"MonitorBot [{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}]";//yyyy-MM-dd HH:mm:ss:ffff
        [JsonProperty("icon_url")]
        public string iconUrl { get; set; } = "";// Webhook.Image;
    }
    [JsonObject]
    public class Field
    {
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("value")]
        public string value { get; set; }
        [JsonProperty("inline")]
        public bool inline { get; set; }
    }
    [JsonObject]
    public class ThumbnailUrl
    {
        //商品URL
        [JsonProperty("url")]
        public string imageUrl { get; set; }
    }
    //[JsonObject]
    //public class Author
    //{
    //    [JsonProperty("name")]
    //    public string name { get; set; }
    //    [JsonProperty("url")]
    //    public string url { get; set; }
    //    [JsonProperty("icon_url")]
    //    public string iconUrl { get; set; } = Webhook.Image;
    //}
    [JsonObject]
    public class Image
    {
        [JsonProperty("url")]
        public string imageUrl { get; set; } = "";// Webhook.Image;
    }
    #endregion
}