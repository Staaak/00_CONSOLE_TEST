#define DEBUG_ON     //ONの場合、DEBUG用のConsole.WriteLineが複数個所で動く
#define TEST_MODE_OFF //ONの場合、proxy OFFかつローカル環境でhtml取得する（ローカルサーバーの起動が必須)
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.IO;

namespace CONSOLE_TEST
{
    public class HttpRequest
    {
        /* proxy・setting.txt読込情報 */
        private Setting _settingInfo;
        private List<ProxyInfo> _proxyInfoList;

        private HttpClient _client;
        public  string _html { get; set; }
        private int _proxyIndexCount = -1;

        public HttpRequest()
        {

        }

        /// <summary>
        /// Html取得処理 Mainメソッド
        /// 
        /// [OUT]this._html
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task MainGetHtml()
        {
            bool retryFlag = true;
            while (retryFlag)
            {
                retryFlag = false;
                try
                {
                    /* HTML取得処理 */
                    await GetHtml();
                    Program.dt = DateTime.Now;
                    break;//forループ終了
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine($"\n###An error has occurred.Enter the proxy information in \"proxy.txt\".[1]\n{e}\nExit the app after 15 seconds.");
                    Thread.Sleep(15000);
                    Environment.Exit(0x8020);//アプリを終了する
                }
                catch (OperationCanceledException e)
                {
                    //whileループでリトライする
                    retryFlag = true;
                    Console.WriteLine($"\n###An error has occurred.[1-1]");//\n{e}\n
#if DEBUG_ON
                    Console.WriteLine("----TimeOut[1]----");
#endif
                }
                catch (HttpRequestException e)
                {
                    //whileループでリトライする
                    retryFlag = true;
                    Console.WriteLine($"\n###An error has occurred.[1-2]");
                }
                catch (Exception e)
                {
                    //whileループでリトライする
                    retryFlag = true;
                    Console.WriteLine($"\n###An error has occurred.[1-3]{e}\n");
                }
                /************/
                /* 遅延処理① */
                /************/
                //await Task.Delay(Program.settingInfo.delayTime);
            }
        }


        /// <summary>
        /// HttpRequestを送信してhtmlを取得する(this._htmlに取得したhtmlを設定)
        /// 
        /// [OUT]this._html
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task GetHtml()//モニター対象サイトのhtmlをリクエスト送信して取得
        {
            /* proxy・setting.txt読込情報 */
            _settingInfo   = Program.settingInfo;
            _proxyInfoList = Program.proxyInfoList;
            _client        = Program.client;

            int retryCount = 0;
            bool retrySendAsync = true;
            while(retrySendAsync)
            {
                retrySendAsync = false;
                _proxyIndexCount++;//html取得リクエストを送信するたびにproxyを変える。またリトライは別のproxyで行う

                /* １から「proxy.txt」に設定したproxy数(proxy_infoリストの要素数)までの数値を取得する(無限ループ) */
                if (_proxyIndexCount > (_proxyInfoList.Count - 1))
                {
                    /* proxy_infoリストの要素数を超えた場合 */
                    _proxyIndexCount = 0;//0リセット
                }
#if DEBUG_ON
                Console.WriteLine("-----------------------------");                          //DEBUG用★★★
                Console.WriteLine("proxyIndexCount:{0}",_proxyIndexCount);                   //DEBUG用★★★
                Console.WriteLine("ip      :{0}", _proxyInfoList[_proxyIndexCount].Ip);      //DEBUG用★★★
                Console.WriteLine("port    :{0}", _proxyInfoList[_proxyIndexCount].Port);    //DEBUG用★★★
                Console.WriteLine("username:{0}", _proxyInfoList[_proxyIndexCount].Username);//DEBUG用★★★
                Console.WriteLine("password:{0}", _proxyInfoList[_proxyIndexCount].Password);//DEBUG用★★★
#endif
                Program.proxyLog = "No." + _proxyIndexCount 
                    + " " + _proxyInfoList[_proxyIndexCount].Ip 
                    + ":" + _proxyInfoList[_proxyIndexCount].Port
                    + ":" + _proxyInfoList[_proxyIndexCount].Username 
                    + ":" + _proxyInfoList[_proxyIndexCount].Password;

                /*** HttpClient ＆ HttpRequestMessageを生成 ***/
                HttpClientHandler ch = new HttpClientHandler
                {
                    /* proxy設定 */
                    Proxy = new WebProxy("http://" + _proxyInfoList[_proxyIndexCount].Ip + ":" + _proxyInfoList[_proxyIndexCount].Port)
                    {
                        Credentials = new NetworkCredential(_proxyInfoList[_proxyIndexCount].Username, _proxyInfoList[_proxyIndexCount].Password),
                    },
#if TEST_MODE_OFF
                    UseProxy = true,
#elif TEST_MODE_ON
                    UseProxy = false,
#endif
                    UseCookies = true
                };

                _client = new HttpClient(ch)
                {
                    Timeout = TimeSpan.FromMilliseconds(9000)//リクエストタイムアウト30,000ミリ秒(3秒)
                    //デフォルトは100秒みたい
                    //「Timeout = TimeSpan.FromMilliseconds(100)」0.1秒に設定したらTimeoutエラーのエクセプションが発生する。
                };

                HttpRequestMessage requestMessage = new HttpRequestMessage()
                {
                    /*******************************/
                    /*** Requestパラメータを設定 ***/
                    /*******************************/
                　 //★★★★
                    RequestUri = new Uri(Program.url),

                };

                /****************************/
                /*** Request Headerを設定 ***/
                /****************************/
                //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//モグラの穴はこの行がないとhtmlが取れない
                SetHeaders(ref requestMessage);

                await Task.Delay(Program.settingInfo.delayTime);
                HttpResponseMessage response = await _client.SendAsync(requestMessage);//Request送信 responsを受信(非同期操作として HTTP 要求を送信します。)
                HttpStatusCode status_code = response.StatusCode;

                string html = response.Content.ReadAsStringAsync().Result;//Responsのhtmlをrcv

                string filePath = @"c:\01_work\test.html";
                System.IO.StreamWriter sw = new StreamWriter(filePath, false, Encoding.GetEncoding(65001));
                sw.Write(html);
                sw.Close();
#if DEBUG_ON
                //Console.WriteLine(this._html);//DEBUG用★★★
                Console.WriteLine("StatsCode = {0}({1})", (int)response.StatusCode, response.StatusCode);//DEBUG用★★★
                Console.WriteLine("retry数 = {0}", retryCount);       //DEBUG用★★★
                Console.WriteLine("-----------------------------"); //DEBUG用★★★
                Console.WriteLine("");                              //DEBUG用★★★
#endif
                if ((int)status_code == 200)
                {
                    this._html = response.Content.ReadAsStringAsync().Result;//Responsのhtmlをrcv
                    //statu code = OKの時は、正常なのでループを抜ける
                    break;
                }
                else
                {
                    //statu code = OKの以外の時は、異常なのでretryする
                    retrySendAsync = true;

                    //statud code が200以外の時は、コンソールに出力
                    //Console.WriteLine($"\nstatus code = {(int)status_code}({status_code})\npryoxy→{Program.proxyLog}");

                    retryCount++;
                }
                /************/
                /* 遅延処理 */
                /************/
                /* html比較処理を何秒毎に行うかここで決める */
                //await Task.Delay(_settingInfo.delayTime );
            }//while文
        }

        /// <summary>
        /// HttpRequesHeaderを設定する★★★★
        /// /// </summary>
        /// <param name="requestMessage"></param>
        public void SetHeaders(ref HttpRequestMessage requestMessage)
        {
#if true
            /* BIG CAMERA */
            requestMessage.Headers.Add("authority", "www.biccamera.com");
            requestMessage.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            requestMessage.Headers.Add("accept-encoding", "deflate");
            requestMessage.Headers.Add("accept-language", "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7");
            requestMessage.Headers.Add("cache-control", "max-age=0");
            requestMessage.Headers.Add("sec-ch-ua-mobile", "?0");
            requestMessage.Headers.Add("sec-ch-ua-platform", "Windows");
            requestMessage.Headers.Add("sec-fetch-dest", "document");
            requestMessage.Headers.Add("sec-fetch-mode", "navigate");
            requestMessage.Headers.Add("sec-fetch-site", "none");
            requestMessage.Headers.Add("sec-fetch-user", "?1");
            requestMessage.Headers.Add("upgrade-insecure-requests", "1");
            requestMessage.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.54 Safari/537.36");
#endif
#if false
            /* nike jp */
            requestMessage.Headers.Add("authority", "www.nike.com");
            requestMessage.Headers.Add("path", "/jp/launch?s=in-stock");
            requestMessage.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            requestMessage.Headers.Add("accept-encoding", "deflate");
            requestMessage.Headers.Add("accept-language", "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7");
            requestMessage.Headers.Add("cache-control", "max-age=0");
            requestMessage.Headers.Add("sec-ch-ua-mobile", "?0");
            requestMessage.Headers.Add("sec-ch-ua-platform", "Windows");
            requestMessage.Headers.Add("sec-fetch-dest", "document");
            requestMessage.Headers.Add("sec-fetch-mode", "navigate");
            requestMessage.Headers.Add("sec-fetch-site", "same-origin");
            requestMessage.Headers.Add("sec-fetch-user", "?1");
            requestMessage.Headers.Add("upgrade-insecure-requests", "1");
            requestMessage.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.54 Safari/537.36");
#endif

#if false
            /* SlothAIO */
            requestMessage.Headers.Add("accept-encoding", "deflate");
            requestMessage.Headers.Add("accept-language", "ja; q = 0.9,en-US; q = 0.8,en; q = 0.7");//ja - JP,ja; q = 0.9,en - US; q = 0.8,en; q = 0.7  //ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7
            requestMessage.Headers.Add("cache-control", "max-age=0");
            requestMessage.Headers.Add("Connection", "keep-alive");
            requestMessage.Headers.Add("sec-fetch-dest", "document");
            requestMessage.Headers.Add("sec-fetch-mode", "navigate");
            requestMessage.Headers.Add("sec-fetch-site", "none");//same-origin
#endif
        }
    }
}
