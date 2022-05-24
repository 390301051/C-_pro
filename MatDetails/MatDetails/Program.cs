using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MatDetails
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("执行方法...");
           
            Main min = new Main();
            min.getFilePath();
            Console.WriteLine("按Enter键结束...");
            Console.ReadKey();
        }
    }
}
