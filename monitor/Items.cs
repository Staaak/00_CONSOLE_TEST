using System;
using System.Collections.Generic;
using System.Text;

namespace CONSOLE_TEST
{
    class Item
    {
        public string ItemName;   //商品名
        public string ItemUrl;　　//商品個別サイトのURL
        public string ItemImg;    //商品画像
        public string ItemPrice;  //価格
        public bool   ItemStatus; //sold out かどうか
        public string AtcUrl;     //ATCのURL
        public int PageNo;        //当該商品が掲載されているページの番号
        public int ItemNewRestock;//New:0x01 ReStock:0x02
    }
}
