#define DEBUG_ON     //ONの場合、DEBUG用のConsole.WriteLineが複数個所で動く
#define TEST_MODE_OFF //ONの場合、proxy OFFかつローカル環境でhtml取得する（ローカルサーバーの起動が必須)
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;


//★★★★
namespace CONSOLE_TEST
{
    class Program
    {
        /* const */
        /* 管理者にも商品情報を通知するためのwebhook admin通知[管理者確認用兼test用] */
#if TEST_MODE_ON
        const string TEST_URL = "http://192.168.56.1";
#else
        const string MONITOR_SITE_URL  = "https://www.supremenewyork.com/shop/new";//★★★★
#endif

        public static string url;
        public static string firstHtml;
        public static string latestHtml;

        public static List<Item> itemListFirst;    //変更される前のhtmlから取得した商品情報設定用
        public static List<Item> itemListLatest;   //最新のhtmlから取得した商品情報設定用
        //public static List<string> StaticPageList = new();

        public static HttpClient client;
        public static HttpRequest httpRequest;
        public static ReadSettings readSettings;
        public static IEnumerable<Item> comparedItemsSub;

        /* proxy・setting.txt読込情報設定用 */
        public static Setting settingInfo;
        public static List<ProxyInfo> proxyInfoList;

        public static HtmlParser parser = new();     // AngleSharp操作用
        public static IHtmlDocument doc;             // AngleSharp操作用
        public static DateTime dt;
        public static string proxyLog;

        private static object lockTest = new object(); //ロック処理用オブジェクト

        public Program()
        {

        }

        /// <summary>
        /// Mianメソッド
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            //必要なインスタンス生成
            client = new();
            httpRequest = new();
            readSettings = new ReadSettings();
            proxyInfoList = new List<ProxyInfo>();

            /************************/
            /* Textファイル読込処理 */
            /************************/
            //「setting.txt」ファイルの読込
            readSettings.ReadTextSetting();
            settingInfo = readSettings._settingInfo;
            //「proxy.txt」Textファイル読込
            readSettings.ReadTextProxy();
            proxyInfoList = readSettings._proxyInfoList;

            Console.WriteLine("Checks for site updates every {0} millisecond.", settingInfo.delayTime);
#if DEBUG_ON
            Console.WriteLine("###Text読込 OK ####");
#endif
            Console.WriteLine("\n～App running～");

            /********************/
            /* 初回HTML取得処理 */
            /********************/
            /* firstHtmlに取得したhtmlを設定 */
#if TEST_MODE_ON
            url = TEST_URL;//[IN]
#else           
            url = MONITOR_SITE_URL;//[IN]
#endif
            await httpRequest.MainGetHtml();//html取得
            firstHtml = httpRequest._html;//[OUT]
            Console.WriteLine(firstHtml);


        }
    }
}
