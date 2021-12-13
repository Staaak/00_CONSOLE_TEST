#define DEBUG_ON     //ONの場合、DEBUG用のConsole.WriteLineが複数個所で動く
#define TEST_MODE_OFF //ONの場合、proxy OFFかつローカル環境でhtml取得する（ローカルサーバーの起動が必須)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CONSOLE_TEST
{
    class ReadSettings
    {
        public Setting _settingInfo { get; set; }
        public List<ProxyInfo> _proxyInfoList { get; set; }

        public ReadSettings()
        {
            _settingInfo = new();
            _proxyInfoList = new();
        }
        
        /// <summary>
        /// setting.txtを読み込む
        /// </summary>
        public Setting ReadTextSetting()
        {
            int i = 0;
            string[] _line = new string[7];

            try
            {
                string FileToRead = @"settings/setting.txt";//@"C:\04_Aile\NANZUKA_SECRET_setting.txt";
                                                                           // Creating enumerable object  
                IEnumerable<string> line = File.ReadLines(FileToRead);//ファイルが存在しないときのケアOK(例外処理)

                foreach (string st in line)
                {
                    _line[i] = st;
                    //Console.WriteLine(_line[i]);//DEBUG用★★★
                    i++;
                }
                _settingInfo.webHookUrl = _line[1];
                _settingInfo.delayTime = Int32.Parse(_line[3]);//キャストエラーのケアOK(例外処理)

#if DEBUG_ON
                Console.WriteLine("webhook URL : {0}",_line[1]);//DEBUG用★★★
                Console.WriteLine("delay time  : {0}",_line[3]);//DEBUG用★★★
#endif
                return _settingInfo;
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine($"\nAn error has occurred.\"setting.txt\" was not found.[Re-1-1]\n{e}\nExit the app after 15 seconds."); Thread.Sleep(15000);
                Environment.Exit(0x8020);//アプリを終了する
                return _settingInfo;
            }
            catch (FileNotFoundException e)//path によって指定されたファイルが見つかりませんでした。（テキストファイルがないときを想定）
            {
                Console.WriteLine($"\nAn error has occurred.\"setting.txt\" was not found.[Re-1-2]\n{e}\nExit the app after 15 seconds."); Thread.Sleep(15000);
                Environment.Exit(0x8020);//アプリを終了する
                return _settingInfo;
            }
            catch (FormatException e)//キャストエラー：引数の形式が無効である場合、または複合書式指定文字列が整形式でない場合にスローされる例外
            {
                Console.WriteLine($"\nAn error has occurred.There is an input error in \"setting.txt\".[Re-1-3]\n{e}\nExit the app after 15 seconds."); Thread.Sleep(15000);
                Environment.Exit(0x8020);//アプリを終了する
                return _settingInfo;
            }
        }

        /// <summary>
        /// proxy.txtを読み込む
        /// </summary>
        public void ReadTextProxy()
        {
            ProxyInfo proxy;
            try
            {
                string FileToRead = @"settings/proxy.txt";
                // Creating enumerable object
                IEnumerable<string> line = File.ReadLines(FileToRead);//ファイルが存在しないときのケア必須(例外処理)

                /* 読み込んだテキストを1行ずつループ処理 */
                foreach (string st in line)
                {
                    /* 当該行の先頭に「#」があるか判定 */
                    if (!(Regex.IsMatch(st, "#.*")))
                    {
                        /* 先頭が「#」以外の場合 */
#if DEBUG_ON
                        Console.WriteLine(st);            //DEBUG用★★★
#endif
                        /*************************/
                        /* proxy情報をListに設定 */
                        /*************************/
                        var proxy_array = st.Split(':');
#if DEBUG_ON
                        //Console.WriteLine(proxy_array[0]);//DEBUG用★★★
                        //Console.WriteLine(proxy_array[1]);//DEBUG用★★★
                        //Console.WriteLine(proxy_array[2]);//DEBUG用★★★
                        //Console.WriteLine(proxy_array[3]);//DEBUG用★★★
#endif
                        proxy = new ProxyInfo
                        {
                            Ip = proxy_array[0],
                            Port = proxy_array[1],
                            Username = proxy_array[2],
                            Password = proxy_array[3]
                        };
                        _proxyInfoList.Add(proxy);
                    }
                    else
                    {
                        /* 先頭が「#」の場合 */
                        /* コメント行と判断するので、do nothing */
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine($"\nAn error has occurred.Please enter the proxy information correctly in \"proxy.txt\".[Re-2-1]\n{e}\nExit the app after 15 seconds.");                Thread.Sleep(15000);
                Environment.Exit(0x8020);//アプリを終了する
            }
            catch (FileNotFoundException e)//path によって指定されたファイルが見つかりませんでした。（テキストファイルがないときを想定）
            {
                Console.WriteLine($"\nAn error has occurred.\"proxy.txt\" was not found.[Re-2-2]\n{e}\nExit the app after 15 seconds."); Thread.Sleep(15000);
                Environment.Exit(0x8020);//アプリを終了する
            }
            catch (FormatException e)//キャストエラー：引数の形式が無効である場合、または複合書式指定文字列が整形式でない場合にスローされる例外
            {
                Console.WriteLine($"\nAn error has occurred.There is an input error in \"proxy.txt\".[Re-2-3]\n{e}\nExit the app after 15 seconds."); Thread.Sleep(15000);
                Environment.Exit(0x8020);//アプリを終了する
            }
        }
    }
}
