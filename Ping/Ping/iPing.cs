using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using System.Data.SqlClient;
using Excel = Microsoft.Office.Interop.Excel;
using Spire.Xls;
using Spire.Xls.Core.Spreadsheet.Sorting;

namespace PingDebug
{
    class iPing
    {
        public static Mutex m = new Mutex(); 
        private bool flag = true;
        private bool chose1 = false;
        private bool chose2 = false;
        private int outtime;
        private string stationname = "";
        private int taskCount = 0;
        private List<string> ipArray = new List<string>();
        private List<string> ipResult = new List<string>();
        private List<string> StationName = new List<string>();

        //Oledb读取execl表格内容
        public void OledbRead() {
            ipArray.Clear();
            ipResult.Clear();
            StationName.Clear();
            taskCount = 0;
            flag = true;
            //检查IP.xlsx文件是否存在
            string filepath = Directory.GetCurrentDirectory();
            string newpath = Path.Combine(filepath, "IP.xlsx");
            if (!File.Exists(newpath)) {
                Console.Write("IP.xlsx文件不存在！");
                return;
            }

            //.xlsx表格的连接字符串。pathName：带路径的Excel文件名 例如D:\vsCode\Ping\Ping\bin\Debug\测试.xlsx
            string pathName="IP.xlsx";
            string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + pathName + ";Extended Properties='Excel 12.0;HDR=Yes;IMEX=1;'";
            //1.使用数据源与excel进行连接
            OleDbConnection connection = new OleDbConnection(strConn);
            //2.打开连接
            connection.Open();
            //3.查询
            string sql = "select * from [Sheet1$]"; //查询命令，从Sheet1中查询数据
            //3.1 查询适配器，OleDbDataAdapter 返回的是datatable类型的数据
            OleDbDataAdapter adapter = new OleDbDataAdapter(sql, connection);
            // 4.使用dataset存放DataTable数据，可以存放很多张表格
            DataSet dataset = new DataSet();
            // 4.1将数据存入dataset
            adapter.Fill(dataset);
            // 查询结束，关闭连接
            connection.Close();

            //取出dataSet中所有表格的数据，返回的是DataTableCollection
            DataTableCollection tableCollection = dataset.Tables;
            //只有一个表格，直接取索引0
            DataTable dataTable = tableCollection[0];
            
            //取出所有行
            DataRowCollection rowCollection = dataTable.Rows;
           //取出所有列
            DataColumnCollection cloumnCollection = dataTable.Columns;

            // 遍历DataRowCollection对象集合，取得每个行的rowCollection数据，通过索引取值

            //foreach (DataRow row in rowCollection) {
            //    for (int i = 0; i < row.ItemArray.Length;i++ )
            //    {
                    
            //        Console.Write(row[i] + " ");
            //    }
            //    Console.WriteLine();
            //}

            //string str4 = dataTable.Rows[0]["台站"].ToString();
            //string str5 = dataTable.Rows[0]["设备"].ToString();
            //string str6 = dataTable.Rows[0]["ip"].ToString();
            //Console.Write("台站:" + str4 + ",设备:" + str5 + ",ip:" + str6);
            //Console.WriteLine();
            
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                string str1 = dataTable.Rows[i]["台站"].ToString();
                string str2 = dataTable.Rows[i]["设备"].ToString();
                string str3 = dataTable.Rows[i]["ip"].ToString();
                if (str1 != "")
                {
                    stationname = str1;
                    StationName.Add(stationname);
                    ipArray.Add(str1 + "+" + str2 + "+" + str3);
                }
                else {
                    ipArray.Add(stationname + "+" + str2 + "+" + str3);
                }
            }
            string chosestr="";
            foreach (string item in StationName) {
                int index = StationName.IndexOf(item)+1;
                chosestr += index + "," + item+";\n";
            
            }
            while (flag)
            {
                Console.WriteLine("请选择需要进行通道验证的台站：\n0,验证所有台站;\n"+chosestr);
                string chose = Console.ReadLine();
                bool isContain=true;
                bool isnum = false;
                try
                {
                    int.Parse(chose);
                    isnum = true;
                }
                catch {
                    isnum = false;
                }
                if (isnum)
                {
                    if (0 <= int.Parse(chose) - 1 && int.Parse(chose) - 1 < StationName.Count)
                    {
                        isContain = true;
                    }
                    else { isContain = false; }
                }
                else {
                    isContain = false;
                }

                if (chose != "0" && !isContain)
                {
                    Console.WriteLine("您的输入有误，请对照可选项重新输入...");
                }
                else
                {
                    if (chose == "0") {
                        taskCount = dataTable.Rows.Count;
                        this.addTask("all");
                        
                    } else {
                        for (int i = 0; i < ipArray.Count; i++)
                        {
                            string[] arr = ipArray[i].Split('+');
                            string name = arr[0];
                            if (name == StationName[int.Parse(chose) - 1])
                            {
                                taskCount =taskCount+1;
                            }
                            else
                            { continue; }
                        }
                        this.addTask(int.Parse(chose) - 1+"");
                        Console.Write(taskCount);

                    }
                    flag = false;
                    break;
                }
            }

            

        }

        //从配置文件读信息（已弃用）
        public void getip()
        {
            chose1 = false;
            chose2 = false;
            flag = true;
            while (flag) {
                Console.WriteLine("请选择ping模式：1，ping现场设备的主备交换机；2，ping现场设备的主备工控机");
                string chose = Console.ReadLine();
                if (chose != "1" && chose != "2")
                {
                    Console.WriteLine("您的选择有误，请重新选择...");
                }
                else {
                    if (chose == "1") { chose1 = true; chose2 = false; } else { chose2 = true; chose1 = false; }
                    flag = false;
                    break;
                }
            }
            this.checkRecord();
            //读取自定义配置节点的对应值
            IDictionary timeout = (IDictionary)ConfigurationManager.GetSection("timeout");
            string str = (string)timeout["value"];
           outtime = Convert.ToInt32(str);

            string[] keys = ConfigurationManager.AppSettings.AllKeys;
            Task[] tasks=new Task[keys.Length];
            //int chose1length=0,chose2length=0;
            //for(int k=0;k<keys.Length;k++){
            //     string key = keys[k];
            //    if (chose1) {
            //        if (!key.Contains("device_")) {
            //            chose1length++;
            //            continue;
            //        }
            //    }
            //    if (chose2) {
            //        if (key.Contains("device_"))
            //        {
            //            chose2length++;
            //            continue;
            //        }
            //    }
            //}
            //if (chose1)
            //{
            //    tasks = new Task[chose1length];
            //}
            //else {
            //    tasks = new Task[chose2length];
            //}

            
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                if (chose1) {
                    if (key.Contains("device_")) {
                        tasks[i] = Task.Factory.StartNew(() => {});
                        continue;
                    }
                }
                if (chose2) {
                    if (!key.Contains("device_"))
                    {
                        tasks[i] = Task.Factory.StartNew(() => { });
                        continue;
                    }
                }
                //通过Key来索引Value
                string value = ConfigurationManager.AppSettings[key];
                Console.WriteLine(i.ToString() + ": " + key + " = " + value);
                // value:中继1（地震主）+172.16.0.1.11 分割字符串
                string[] arr = value.Split('+');
                string name = arr[0];
                string ip = arr[1];
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    this.Pingcs(name, ip);
                });
            }
            Console.WriteLine("正在执行操作，请勿关闭...");
            Task.WaitAll(tasks);
            Console.WriteLine("执行完毕，结果请查看日志blog");
            getip();
        }

        //从execl读取信息后添加任务栈
        public void addTask(string flag) {
            //读取自定义配置节点的对应值
            IDictionary timeout = (IDictionary)ConfigurationManager.GetSection("timeout");
            string str = (string)timeout["value"];
            outtime = Convert.ToInt32(str);

            Task[] tasks = new Task[taskCount];
            int taskid = 0;
            this.checkRecord();
            this.checkExeclFile();
            for (int i = 0; i < ipArray.Count;i++)
            {
                string[] arr = ipArray[i].Split('+');
                string name = arr[0];
                string device = arr[1];
                string ip = arr[2];

                if (flag != "all")
                {
                    if (name == StationName[int.Parse(flag)])
                    {
                        if (ip == "")
                        {
                            tasks[taskid] = Task.Factory.StartNew(() =>
                            {

                            });
                            taskid = taskid+1;
                            continue;
                        }
                        else
                        {
                            tasks[taskid] = Task.Factory.StartNew(() =>
                            {
                                this.Pingcs(name + ":" + device, ip);
                            });
                            taskid = taskid + 1;
                        }
                    }
                    else {
                        continue;
                    }
                }
                else {
                    if (ip == "")
                    {
                        tasks[taskid] = Task.Factory.StartNew(() =>
                        {

                        });
                        taskid = taskid + 1;
                        continue;
                    }
                    else
                    {
                        tasks[taskid] = Task.Factory.StartNew(() =>
                        {
                            this.Pingcs(name + ":" + device, ip);
                        });
                        taskid = taskid + 1;
                    }
                }
                
                
                
            }
            Console.WriteLine("正在执行操作，请勿关闭...");
            Task.WaitAll(tasks);
            this.wirteExecl();
        }

        //使用process调用cmd命令执行ping命令
        private void getping(string name, string ip)
        {
            string strIp = ip;
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";//设定程序名

            p.StartInfo.UseShellExecute = false; //关闭Shell的使用

            p.StartInfo.RedirectStandardInput = true;//重定向标准输入

            p.StartInfo.RedirectStandardOutput = true;//重定向标准输出

            p.StartInfo.RedirectStandardError = true;//重定向错误输出

            p.StartInfo.CreateNoWindow = false;//设置不显示窗口

            string pingrst; p.Start(); p.StandardInput.WriteLine("ping " + strIp);

            p.StandardInput.WriteLine("exit");

            string strRst = p.StandardOutput.ReadToEnd();

            if (strRst.IndexOf("(0% loss)") != -1)
            {

                pingrst = "连接";

            }

            else if (strRst.IndexOf("Destination host unreachable") != -1)
            {

                pingrst = "无法到达目的主机";

            }

            else if (strRst.IndexOf("Request timed out") != -1)
            {

                pingrst = "超时";

            }

            else if (strRst.IndexOf("Unknown host") != -1)
            {

                pingrst = "无法解析主机";

            }

            else
            {

                pingrst = "请求找不到主机，请检查名称！";

            }
            //Console.WriteLine("name={0},ip:{1},结果:{2}",name,ip,pingrst);

            //将结果写入日志blog/blog.txt
            this.saveRecord(name, ip, pingrst);
            p.Close();

        }

        //检查日志文件是否存在
        private void checkRecord()
        {
            Console.WriteLine("开始检查日志文件");
            string filepath = Directory.GetCurrentDirectory();

            string newpath = Path.Combine(filepath, "日志");
            DateTime dateTime = DateTime.Now;
            string mydate = string.Format("{0:D4}{1:D2}{2:D2}", dateTime.Year, dateTime.Month, dateTime.Day);
            // 判断日志文件夹是否存在
            if (!File.Exists(newpath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(newpath); //创建文件夹
                directoryInfo.Create();
                string path = "blog_" + mydate + ".txt";
                string blogpath = Path.Combine(newpath, path);
                if (!File.Exists(blogpath)) //判断blog.txt文件是否存在
                {
                    FileStream fs = File.Create(blogpath);//创建文件
                    fs.Close();
                }
                else
                {
                    FileStream stream = File.Open(blogpath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    StreamWriter sw = new StreamWriter(stream);
                    sw.WriteLine("------------------------------------------------------------------分割线-------------------------------------------------------------------");
                    sw.Flush();
                    sw.Close();
                    //stream.SetLength(0);
                    //stream.Close();
                }


            }
            else
            {
                string path = "blog_" + mydate + ".txt";
                string blogpath = Path.Combine(newpath, path);
                if (!File.Exists(blogpath))
                {
                    FileStream fs = File.Create(blogpath);//创建文件
                    fs.Close();
                }
                else
                {
                    FileStream stream = File.Open(blogpath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    StreamWriter sw = new StreamWriter(stream);
                    sw.WriteLine("--------------------------------------------------------------------分割线------------------------------------------------------------------");
                    sw.Flush();
                    sw.Close();
                    //stream.Seek(0, SeekOrigin.Begin);
                    //stream.SetLength(0);
                    //stream.Close();
                }
            }

        }

        //检查result.xlsx是否存在，若不存在则创建，若存在则先删除再创建
        private void checkExeclFile() { 
            //bug，如果result被打开，将无法进行操作
            //如何对创建的表格进行按台站名称排列，如何在表格打开时更新数据
            //文件打开后，如何进行删除替换
            Console.WriteLine("开始检查execl文件");
            string filepath = Directory.GetCurrentDirectory();
            string newpath = Path.Combine(filepath, "Result.xlsx");
            if (!File.Exists(newpath)) //判断blog.txt文件是否存在
            {
                this.createExcel(newpath);
            }
            else
            {
                try
                {
                    File.Delete(newpath);
                }
                catch {

                    Console.Write("Result.xlsx文件正在使用，请关闭文件后重试...\n");
                    this.OledbRead();
                    return;
                }
               
                this.createExcel(newpath);
            }
        }
        //创建execl文件
        private void createExcel(string filename) {
            object Nothing = System.Reflection.Missing.Value;
            var app = new Excel.Application();
            app.Visible = false;
            Excel.Workbook workbook = app.Workbooks.Add(Nothing);
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Sheets[1];
            Excel.Range myrange = worksheet.Range["E" + 1, Type.Missing];
            worksheet.Name = "PingResult";
            worksheet.Cells[1, 1] = "Station";
            worksheet.Cells[1, 2] = "GOPT";
            worksheet.Cells[1, 3] = "IP";
            worksheet.Cells[1, 4] = "Result";
            worksheet.Cells[1, 5] = "PLP";
            worksheet.Columns.ColumnWidth = 28; //批量设置列宽
            myrange.ColumnWidth = 14;// 单独设置列宽
            worksheet.Cells[1, 1].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;//水平居中  
            worksheet.Cells[1, 1].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;//垂直居中  
            worksheet.Cells[1, 2].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter; 
            worksheet.Cells[1, 2].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            worksheet.Cells[1, 3].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            worksheet.Cells[1, 3].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter; 
            worksheet.Cells[1, 4].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            worksheet.Cells[1, 4].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter; 
            worksheet.Cells[1, 5].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            worksheet.Cells[1, 5].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

            worksheet.SaveAs(filename, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing);
            workbook.Close(false,Type.Missing,Type.Missing);
            app.Quit();
        }

        //写入execl数据
        private void wirteExecl() {
            Console.WriteLine("开始写入execl...");
            string filepath = Directory.GetCurrentDirectory();
            string excleName = Path.Combine(filepath, "Result.xlsx");
            object Nothing = System.Reflection.Missing.Value;
            var app = new Excel.Application();
            app.Visible = false;
            Excel.Workbook mybook = app.Workbooks.Open(excleName, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing);
            Excel.Worksheet mysheet = (Excel.Worksheet)mybook.Worksheets[1];
            mysheet.Activate();
            string Station, GOPT, IP, Result, PLP;
            for (int i = 0; i < ipResult.Count; i++) {
                string[] arr=ipResult[i].Split('+');
                Station = arr[0];
                GOPT = arr[1];
                IP = arr[2];
                Result = arr[3];
                PLP = arr[4];
                int maxrow = mysheet.UsedRange.Rows.Count + 1;
                mysheet.Cells[maxrow, 1] = Station;
                mysheet.Cells[maxrow, 2] = GOPT;
                mysheet.Cells[maxrow, 3] = IP;
                mysheet.Cells[maxrow, 4] = Result;
                mysheet.Cells[maxrow, 5] = PLP;
                mysheet.Cells[maxrow, 1].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;//水平居中  
                mysheet.Cells[maxrow, 1].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;//垂直居中  
                mysheet.Cells[maxrow, 2].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                mysheet.Cells[maxrow, 2].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                mysheet.Cells[maxrow, 3].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                mysheet.Cells[maxrow, 3].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                mysheet.Cells[maxrow, 5].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                mysheet.Cells[maxrow, 5].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                Excel.Range myrange = mysheet.Range["D"+maxrow,Type.Missing];
                myrange.WrapText = true;//单元格自动换行
                myrange.EntireRow.AutoFit();
                if (PLP == "100%") {
                    Excel.Range plprange = mysheet.Range["E" + maxrow, Type.Missing];
                    plprange.Interior.ColorIndex = 3;
                    plprange.Font.ColorIndex = 2;
                }
            }
            mybook.Save();
            mybook.Close(false, Type.Missing, Type.Missing);
            mybook = null;
            app.Quit();
            this.exChangeExecl();
            //Console.WriteLine("执行完毕，结果请查看日志blog或Result.xlsx");
            //this.OledbRead();
        }

        //调整result.xlsx的格式，排序，合并单元格等
        public void exChangeExecl()
        {
            //创建workbook实例
            Workbook workbook = new Workbook();
            //加载Excel文件
            workbook.LoadFromFile("Result.xlsx");
            //获取第一个工作表
            Worksheet worksheet = workbook.Worksheets[0];
            int length = worksheet.Range.Rows.Length;
            //设置单元格边框及颜色
            CellRange range = worksheet.Range["A1:E" + length];
            //区域内部应用线条
            range.BorderInside(LineStyleType.Thin);
            range.BorderAround(LineStyleType.Thin);
            //区域外部应用线条

            //指定需要排序的列，以及排序方式（基于单元格的值）
            SortColumn column = workbook.DataSorter.SortColumns.Add(0, SortComparsionType.Values, OrderBy.Descending);
            workbook.DataSorter.SortColumns.Add(1, SortComparsionType.Values, OrderBy.Ascending);
            //排序是否包含标题（默认第一个数据为标题，不对其进行排序）
            workbook.DataSorter.IsIncludeTitle = false;
            //指定要排序的单元格范围并进行排序
           
            if (length >2) {
                workbook.DataSorter.Sort(worksheet.Range["A2:E"+length]);
                //单元格合并
                 string stationname="";
                 int index=2;
                 int nextindex = 2;
                for(int i=2;i<=length;i++){
                    string text = worksheet.Range["A" + i].Text;
                    if (i == 2)
                    {
                        stationname = text;
                        nextindex = 2;
                    }
                    else {
                        if (i == length)
                        {
                            worksheet.Range["A" + index + ":A" + i].Merge();
                            stationname = "";
                            nextindex = 2;
                            index = 2;
                        }
                        else {
                            if (text != stationname)
                            {
                                if (index != nextindex) {
                                    worksheet.Range["A" + index + ":A" + nextindex].Merge();
                                } 
                                stationname = text;
                                index = i;
                                nextindex = i;
                            }
                            else
                            {
                                nextindex =nextindex+1;
                            }
                        }
                    }
                    
                }

            }
            //设置列宽、行高为自适应（应用于指定数据范围）
            //worksheet.AllocatedRange["A1:F15"].AutoFitColumns();
            //worksheet.AllocatedRange.AutoFitColumns(); 设置整个工作表自适应列宽
            worksheet.AllocatedRange.AutoFitRows();  //设置整个工作表行高
            //保存文档
            workbook.Save();

            Console.WriteLine("执行完毕，结果请查看日志blog或Result.xlsx");
            this.OledbRead();
        }

        //开始写入文件数据
        private void saveRecord(string name, string ip, string pingstr)
        {
            m.WaitOne();
            string filePath = Directory.GetCurrentDirectory();
            string newpath = Path.Combine(filePath, "日志");
            DateTime dateTime = DateTime.Now;
            string mydate = string.Format("{0:D4}{1:D2}{2:D2}", dateTime.Year, dateTime.Month, dateTime.Day);

            string path = "blog_" + mydate + ".txt";

            string mypath = Path.Combine(newpath, path);
            FileStream fs = new FileStream(mypath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);
            try
            {
                Console.WriteLine(name + ":" + ip + "，result：" + pingstr);
                sw.WriteLine(name + ":" + ip + "，result：" + pingstr);
                sw.Flush();
                sw.Close();


            }
            catch (IOException e)
            {
                sw.Flush();
                sw.Close();
                Console.WriteLine(e.ToString());
            }
            m.ReleaseMutex();

        }
        //调用内置Ping方法
        private void Pingcs(string name, string ip)
        {
            Ping pingSender = new Ping();
            //Ping 选项设置  
            PingOptions options = new PingOptions();
            options.DontFragment = true;

            string data = "ping test data";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            //设置超时时间  
            int timeout = outtime;
            //调用同步 send 方法发送数据,将返回结果保存至PingReply实例  
            PingReply reply = pingSender.Send(ip, timeout, buffer, options);
            string pingstr;
            string resultString;
            string package=((buffer.Length-reply.Buffer.Length)/buffer.Length)*100+"%";
            if (reply.Status == IPStatus.Success)
            {
                resultString="ping状态：连接正常" + "，答复的主机地址：" + reply.Address.ToString() + "，往返时间：" + reply.RoundtripTime;
                pingstr = "ping状态：连接正常" + "，答复的主机地址：" + reply.Address.ToString() + "，往返时间：" + reply.RoundtripTime+"，丢包率："+package;
            }
            else
            {
                resultString = reply.Status.ToString();
                pingstr = reply.Status.ToString() + "，丢包率：" + package;
            }

            string[] arr = name.Split(':');
            string execlContent = arr[0] + "+" + arr[1] + "+" + ip + "+" + resultString + "+" + package;
            ipResult.Add(execlContent);

            this.saveRecord(name, ip, pingstr);
            
        }
    }
}
