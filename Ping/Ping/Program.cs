using System;  //一般在程序中包含命名空间
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingDebug  //包含的一系列类
{
    class main
    {
        static void Main(string[] args)
        {
            Console.WriteLine("执行方法...");
            //远程ping 
            iPing pin = new iPing();
            pin.OledbRead();
            Console.WriteLine("按Enter键结束...");
            Console.ReadKey();
        }
    }
}
