using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
namespace MatDetails
{
    class Main
    {
        private const double EARTH_RADIUS = 6378137;
        private const double EPS = 1e-6;
        private const int disc = 20;
        public struct Point
        {
            public string orderid;
            public string oldName;
            public string eventTime;
            public string FolderNmae; //1104111716_100KM_2
            public string StaName;
            public float sta_Long;
            public float sta_Lat;
            public double Ptime;
            public double Depth;
            public float sta_Magnitude;
            public float sta_Azi;
            public double sta_EpcDist;
            public float PGA_Curr;
            public float PGV_Curr;
            public float PGD_Curr;
            public float DurCurr;
            public float real_Lng;
            public float real_Lat;
            public float rel_Magnitude;
        }
        public struct Earthquake
        {
            public string orderid;
            public string oldName;
            public string FolderNmae; //1104111716_100KM_2
            public DateTime Starttime;
            public DateTime Endtime;
            public float real_Lng;
            public float real_Lat;
            public float rel_Magnitude;
        }
        private List<Point> points = new List<Point>();

        private List<Point> fivepoints = new List<Point>();

        private int datacount = 1;
        public static string chosen = "";
        public string received = "";
        public bool flag = false;
        public bool hasfinded = false;
        public string mydate = "";
        //循环读取txt文件内容
        public void getFilePath() {


            Console.WriteLine("请选择需要处理的数据格式：1，发波日志，2.日本海域地震分组");
            chosen = Console.ReadLine();

            Console.WriteLine("请输入数据信息所在路径...");
            string insert = Console.ReadLine();
            if (chosen == "1")
            {
                Console.WriteLine("请输入Receive_wavep日志所在路径...");
                received = Console.ReadLine();

            }
           checkRecord();
            //当前数据所在目录，D:\NIED数据下载\海域地震数据有头\4-5
            string datapath = insert;
            DirectoryInfo TheFolder = new DirectoryInfo(datapath);
            //遍历文件夹TheFolder
            string[] arr = Directory.GetDirectories(insert);
            string lastpath = arr[arr.Length - 1];
            //Console.WriteLine(lastpath);
            string childlastpath = Directory.GetDirectories(lastpath)[Directory.GetDirectories(lastpath).Length - 1];
            //Console.WriteLine(childlastpath);
            foreach (DirectoryInfo nextFile in TheFolder.GetDirectories()) {
                if (chosen == "2") { DataFZ(); }
                //Console.WriteLine(nextFile.FullName);  //例如 D:\NIED数据下载\海域地震数据有头\4-5\20100107053900_4.1
               

                DirectoryInfo thirdFile = new DirectoryInfo(nextFile.FullName);
                    foreach (DirectoryInfo nextFile2 in thirdFile.GetDirectories()) {
                    
                    if (chosen == "2")
                    {
                        Console.WriteLine(nextFile2.FullName);//D:\NIED数据下载\海域地震数据有头\4-5\20100107053900_4.1\AOM0071001070539
                                                              //Console.WriteLine(nextFile2.Name); //AOM0071001070539
                        Disc(nextFile2.FullName, nextFile2.Name);
                        //最后一个文件夹处理结束后，调用DataFZ方法完成最后一个事件的分组

                        if (nextFile.FullName == lastpath && childlastpath == nextFile2.FullName)
                        {
                            DataFZ();
                        }
                    }
                    if (chosen == "1")
                    {
                        Console.WriteLine(nextFile2.FullName);
                        this.FBdata(nextFile2.FullName);
                    }
                }
            }
            //foreach (var info in points)
            //{
            //    string str = info.FolderNmae + datacount + "     " + info.StaName + "    " + info.sta_Long.ToString("f6") + "   " + info.sta_Lat.ToString("f6") + "    " + info.Ptime.ToString("f6") + "  " + info.Depth.ToString("f6") + "  " +
            //                                        info.sta_Azi.ToString("f6") + "  " + info.sta_EpcDist.ToString("f6") + "    " + info.PGA_Curr.ToString("f6") + "   " + info.PGV_Curr.ToString("f6") + "   " + info.PGV_Curr.ToString("f6") + "   " + info.DurCurr.ToString("f6") + "    " + info.real_Lng.ToString("f6") +
            //                                        "   " + info.real_Lat.ToString("f6") + "   " + info.rel_Magnitude.ToString("f6") + "\n";
            //    Console.WriteLine(str);
            //    writeRecord(str);
            //}
            fivepoints.Clear();
            datacount = 1;
            getFilePath();

        }
        //发波日志分组
        private void FBdata(string path)
        {
            //datacount = 1;
            string[] filestxt = Directory.GetFiles(path, "*.txt");
            string[] lines=null;
            //List<Point> linespoint = new List<Point>();
            //遍历txt 文件，获取每个txt文件的所有行
            foreach (string filepath in filestxt)
            {
                lines = File.ReadAllLines(filepath);
                foreach(string str in lines)
                {
                    fivepoints.Clear();
                    //Earthquake earinfo = new Earthquake();
                    //获取震源信息，获取文件名 1001070539_100KM_1 
                    string[] arr1 = str.Split('|');
                    if (str=="") {
                        return;
                    }
                    
                    Earthquake earinfo = new Earthquake();
                     earinfo = getfilename(arr1[0]);
                    hasfinded = false;
                    readWavepLOgo(received,earinfo);
                    //Console.WriteLine(arr1[0]+"________"+earinfo.Ptime);
                    //continue;

                    //    for (int i = 1; i < arr1.Length; i++)
                    //    {

                    //        string info = arr1[i];  
                    //        Point p = new Point();
                    //        string[] stationinfo = info.Split(',');

                    //        double distance = double.Parse(stationinfo[2]);
                    //        if (distance < 100)
                    //        {
                    //            p.FolderNmae = earinfo.FolderNmae + "_100KM_"+ datacount;
                    //        }
                    //        else if (distance > 200)
                    //        {
                    //            p.FolderNmae = earinfo.FolderNmae + "_out200KM_"+ datacount;
                    //        }
                    //        else
                    //        {
                    //            p.FolderNmae = earinfo.FolderNmae + "_200KM_"+ datacount;
                    //        }
                    //        p.StaName = stationinfo[1];
                    //        p.sta_Long = float.Parse(stationinfo[4]);
                    //        p.sta_Lat = float.Parse(stationinfo[3]);
                    //        p.Ptime = earinfo.Ptime;
                    //        p.sta_EpcDist = distance;
                    //        p.real_Lng = earinfo.real_Lng;
                    //        p.real_Lat = earinfo.real_Lat;
                    //        p.rel_Magnitude = earinfo.rel_Magnitude;

                    //        linespoint.Add(p);

                    //}
                    //datacount += 1;
                }
                //foreach (var info in linespoint)
                //{
                //    string str = info.FolderNmae + "     " + info.StaName + "    " + info.sta_Long.ToString("f6") + "   " + info.sta_Lat.ToString("f6") + "    " + info.Ptime.ToString("f6") + "  " + info.Depth.ToString("f6") + "  " + info.sta_Magnitude.ToString("f6")+"    "+
                //    info.sta_Azi.ToString("f6") + "  " + info.sta_EpcDist.ToString("f6") + "    " + info.PGA_Curr.ToString("f6") + "   " + info.PGV_Curr.ToString("f6") + "   " + info.PGV_Curr.ToString("f6") + "   " + info.DurCurr.ToString("f6") + "    " + info.real_Lng.ToString("f6") +
                //                                        "   " + info.real_Lat.ToString("f6") + "   " + info.rel_Magnitude.ToString("f6") + "\n";
                //    //Console.WriteLine(str);
                //    writeRecord(str);
                //}
                //linespoint.Clear();
            }


        }
        // 服务器Receive_wavep分组
        private void readWavepLOgo(string path, Earthquake earinfo)
        {
            DirectoryInfo TheFolder = new DirectoryInfo(path);
            foreach (DirectoryInfo nextFile in TheFolder.GetDirectories())
            {
                if (hasfinded) { break; }
                //Console.WriteLine(nextFile.FullName);  //例如 D:\NIED数据下载\发波记录转分组用\Receive_wavep\Info\202205
                DirectoryInfo thirdFile = new DirectoryInfo(nextFile.FullName);
                foreach (DirectoryInfo nextFile2 in thirdFile.GetDirectories())
                {
                    if (hasfinded) { break; }
                    if (chosen == "1" && hasfinded==false)
                    {
                        //Console.WriteLine(nextFile2.FullName);
                        this.Receive_wavep(nextFile2.FullName, earinfo);
                    }
                }
            }
        }


        private void Receive_wavep(string path, Earthquake earinfo)
        {
            //datacount = 1;
            string[] filestxt = Directory.GetFiles(path, "*.log");
            string[] lines = null;
            
            //List<Point> linespoint = new List<Point>();
            //遍历txt 文件，获取每个txt文件的所有行
            int index = 0;
            if (filestxt.Length > 0)
            {
                flag = true;
            }
            else
            {
                Console.WriteLine(filestxt.Length);
                flag = false;
            }
            while(flag)
            {
                //FileInfo fl = new FileInfo(filestxt[index]);
                //double creattime = DateTimeToStamp(fl.CreationTime);
                    lines = File.ReadAllLines(filestxt[index]);
                    foreach (string str in lines)
                    {
                        string[] arr1 = Regex.Split(str, "\\s+", RegexOptions.IgnoreCase);
                        if (str == "")
                        {
                            return;
                        }
                     
                    string time = arr1[2].Split(':')[1]; //20220513162459 ->2022-05-13 16:24:59.000
                    string year = time.Substring(0, 4) +"-"+ time.Substring(4, 2) +"-"+ time.Substring(6, 2) + " " + time.Substring(8, 2) + ":" + time.Substring(10, 2) + ":" + time.Substring(12, 2)+".000";
                    DateTime eventid = DateTime.Parse(year);
                    DateTime recodtime = DateTime.Parse(arr1[0] + " " + arr1[1]);
                    int result = DateTime.Compare(eventid, earinfo.Starttime); //eventid 大 返回  1 , earinfo.Starttime 大  返回-1
                    int result2 = DateTime.Compare(eventid, earinfo.Endtime);
                    if ((result==1) && (result2==-1))
                    {
                        //Console.WriteLine(eventid + "," + earinfo.Starttime + "," + earinfo.Endtime);
                        //Console.WriteLine(result + "," + result2);
                        Point p = new Point();
                        double distance = double.Parse(arr1[5].Split(':')[1]);
                        //string[] datearr = arr1[0].Split('-');
                        //string year = datearr[0];
                        //string[] timearr = arr1[1].Split(':');

                        //string filename = year.Substring(year.Length - 2) + datearr[1] + datearr[2] + timearr[0] + timearr[1] + timearr[2].Substring(0, 2);
                        string FolderNmae = earinfo.FolderNmae;
                        if (distance < 100)
                        {
                            p.FolderNmae = FolderNmae + "_100KM_" + datacount;
                        }
                        else if (distance > 200)
                        {
                            p.FolderNmae = FolderNmae + "_out200KM_" + datacount;
                        }
                        else
                        {
                            p.FolderNmae = FolderNmae + "_200KM_" + datacount;
                        }
                        p.orderid = earinfo.orderid;
                        p.oldName = earinfo.oldName;
                        p.StaName = arr1[3].Split(':')[1];
                        p.sta_Long = float.Parse(arr1[6].Split(':')[1]);
                        p.sta_Lat = float.Parse(arr1[7].Split(':')[1]);
                        DateTime RecordTime = DateTime.Parse(arr1[8].Split(':')[1]+" "+ arr1[9]);
                        p.Ptime = DateTimeToStamp(RecordTime);
                        p.sta_EpcDist = distance;
                        p.real_Lng = earinfo.real_Lng;
                        p.real_Lat = earinfo.real_Lat;
                        p.rel_Magnitude = earinfo.rel_Magnitude;
                        p.DurCurr = float.Parse(arr1[16].Split(':')[1]);
                        p.eventTime = time;
                        if (fivepoints.Count < 5)
                        {
                            bool isneweventid = false;
                            bool iscontain = false;
                            foreach (var info in fivepoints)
                            {
                                if (info.eventTime!= time) {
                                    
                                    if (fivepoints.Count >= 2)
                                    {
                                        flag = false;
                                        isneweventid = true;
                                        hasfinded = true;
                                    }
                                    fivepoints.Clear();
                                    break;
                                    //iscontain = true;
                                }
                                else
                                {
                                    if (arr1[3].Split(':')[1] == info.StaName)
                                    {
                                        iscontain = true;
                                        break;
                                    }
                                }
                            }
                            //Console.WriteLine(iscontain);
                            if (!iscontain && !isneweventid)
                            {
                             fivepoints.Add(p);
                            }
                            if (isneweventid)
                            {
                                break;
                            }
                           
                        }
                    }
                    if (fivepoints.Count == 5)
                    {
                        List<Point> upList = fivepoints.OrderBy(a => a.Ptime).ToList(); //按Ptime对fivepoints进行排序
                        
                        foreach (var info in upList)
                        {
                            string str2 = info.orderid + "     " + info.oldName + "     " + info.eventTime + "     " + earinfo.Starttime + "," + earinfo.Endtime +","+ info.FolderNmae + "     " + info.StaName+"\n";

                            string str1 =info.oldName + "     "+info.FolderNmae + "     " + info.StaName + "    " + info.sta_Long.ToString("f6") + "   " + info.sta_Lat.ToString("f6") + "    " + info.Ptime.ToString("f6") + "  " + info.Depth.ToString("f6") + "  " + info.sta_Magnitude.ToString("f6") + "    " +
                                info.sta_Azi.ToString("f6") + "  " + info.sta_EpcDist.ToString("f6") + "    " + info.PGA_Curr.ToString("f6") + "   " + info.PGV_Curr.ToString("f6") + "   " + info.PGV_Curr.ToString("f6") + "   " + info.DurCurr.ToString("f6") + "    " + info.real_Lng.ToString("f6") +
                                "   " + info.real_Lat.ToString("f6") + "   " + info.rel_Magnitude.ToString("f6") + "\n";

                            Console.WriteLine(str1);
                            writeRecord(str1);
                        }
                        fivepoints.Clear();
                        datacount = datacount + 1;
                        flag = false;
                        hasfinded = true;
                        break;
                    }
                    //datacount += 1;
                }
                //foreach (var info in fivepoints)
                //{
                //    string str = info.FolderNmae + "     " + info.StaName + "    " + info.sta_Long.ToString("f6") + "   " + info.sta_Lat.ToString("f6") + "    " + info.Ptime.ToString("f6") + "  " + info.Depth.ToString("f6") + "  " + info.sta_Magnitude.ToString("f6") + "    " +
                //    info.sta_Azi.ToString("f6") + "  " + info.sta_EpcDist.ToString("f6") + "    " + info.PGA_Curr.ToString("f6") + "   " + info.PGV_Curr.ToString("f6") + "   " + info.PGV_Curr.ToString("f6") + "   " + info.DurCurr.ToString("f6") + "    " + info.real_Lng.ToString("f6") +
                //                                        "   " + info.real_Lat.ToString("f6") + "   " + info.rel_Magnitude.ToString("f6") + "\n";
                //    Console.WriteLine(str);
                //    //writeRecord(str);
                //}
                //fivepoints.Clear();
                index += 1;
                if (index> filestxt.Length-1)
                {
                    if (fivepoints.Count<=5)
                    {
                     fivepoints.Clear();
                    }
                    flag = false;
                }
                
                
            }
        }

        private Earthquake getfilename(string name)
        {
            Earthquake ear = new Earthquake();
            string[] infoarr = name.Split(',');
            ear.orderid = infoarr[0];
            ear.oldName = infoarr[1];
            DateTime stratTime = DateTime.Parse(infoarr[2]);
            ear.Starttime = stratTime;
            DateTime endTime = DateTime.Parse(infoarr[3]);
            ear.Endtime= endTime;

            ear.real_Lng = float.Parse(infoarr[5]);
            ear.real_Lat = float.Parse(infoarr[4]);
            ear.rel_Magnitude = float.Parse(infoarr[6]);
            string[] namearr = Regex.Split(infoarr[2], "\\s+", RegexOptions.IgnoreCase);
            string[] datearr = namearr[0].Split('-');
            string year = datearr[0];
            string[] timearr = namearr[1].Split(':');

            string filename = year.Substring(year.Length - 2) + datearr[1] + datearr[2] + timearr[0] + timearr[1] + timearr[2].Substring(0, 2);
            ear.FolderNmae = filename;
            return ear;
    }

        //日本海域地震分组方法
        private void DataFZ()
        {
            List<Point> upList = points.OrderBy(a => a.sta_EpcDist).ToList(); //按震中距对points进行排序
            try
            {
                for (int j = 0; j < upList.Count; j++)
                {
                    Point info1 = upList[j];
                    double distance =info1.sta_EpcDist;
                    int res = Convert.ToInt32(distance);
                    int index = j;
                    fivepoints.Add(info1);
                    for (int i = 0; i < upList.Count; i++)
                    {
                        Point info2 = upList[i];
                        double distance2 = info2.sta_EpcDist;
                        int res2 = Convert.ToInt32(distance2);
                        int index2 = i;
                        if (index == index2) { continue; }
                        if (fivepoints.Count == 5)
                        {
                            foreach (var info in fivepoints)
                            {
                                string str = info.FolderNmae + datacount + "     " + info.StaName + "    " + info.sta_Long.ToString("f6") + "   " + info.sta_Lat.ToString("f6") + "    " + info.Ptime.ToString("f6") + "  " + info.Depth.ToString("f6") + "  " + info.sta_Magnitude.ToString("f6") + "    "+
                                    info.sta_Azi.ToString("f6") + "  " + info.sta_EpcDist.ToString("f6") + "    " + info.PGA_Curr.ToString("f6") + "   " + info.PGV_Curr.ToString("f6") + "   " + info.PGV_Curr.ToString("f6") + "   " + info.DurCurr.ToString("f6") + "    " + info.real_Lng.ToString("f6") +
                                    "   " + info.real_Lat.ToString("f6") + "   " + info.rel_Magnitude.ToString("f6") + "\n";
                                //if (Convert.ToInt32(info.sta_EpcDist) > 200)
                                //{
                                //    Console.WriteLine(str);
                                //}

                                writeRecord(str);
                            }
                            fivepoints.Clear();
                            datacount = datacount + 1;
                            break;
                        }
                        if (fivepoints.Count < 5)

                        {
                            if ((res-disc) < res2 && res2< (res+ disc))
                            {
                                //if (distance > 200)
                                //{
                                //    Console.WriteLine(distance + "," + (distance - disc) + "," + distance2 + "," + (distance + disc));
                                //    Console.WriteLine(distance - disc < distance2 && distance2 < distance + disc);
                                //}
                                fivepoints.Add(info2);
                            }
                           
                        }
                        if (i == (points.Count - 1) && fivepoints.Count < 5)
                        {
                            fivepoints.Clear();
                        }

                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("事件处理错误..." + e);
            }
            datacount = 1;
            fivepoints.Clear();
            points.Clear();
        }

        //初始化数据结构体并加入list
        private void Disc(string path,string filename) {
           //string path = "D:\\NIED数据下载\\海域地震数据有头\\4-5\\20100107053900_4.1\\AOM0071001070539";
           //string filename = "AOM0071001070539";
            string newpath = Path.Combine(path, filename + ".EW");
            if (!File.Exists(newpath))
            {
                newpath = Path.Combine(path, filename + ".EW1");
            }
            string[] lines = File.ReadAllLines(newpath);
            Point p = new Point() ;
            double lat=0, lng=0, stalat=0, stalng=0; //用于计算震中距
            string oTime="", rTime=""; //用于计算Ptime
            for (int i = 0; i < 16; i++) {
                string line = lines[i];
                //拆分行
                string[] V= Regex.Split(line, "\\s+", RegexOptions.IgnoreCase);
                switch (i)
                {
                    case 0:
                        Console.WriteLine(V[0]+V[1]+":"+V[2]+" "+V[3]);
                        oTime = V[2] + " " + V[3];
                        break;
                    case 1:
                        Console.WriteLine(V[0] + ":" + V[1]);
                        lat = double.Parse(V[1]);
                        p.real_Lat = float.Parse(V[1]);
                        break;
                    case 2:
                       Console.WriteLine(V[0] + ":" + V[1]);
                        lng = double.Parse(V[1]);
                        p.real_Lng = float.Parse(V[1]);
                        break;
                    case 3:
                       Console.WriteLine(V[0] + V[1] + ":" + V[2]);
                        p.Depth = double.Parse(V[2]);
                        break;
                    case 4:
                       Console.WriteLine(V[0] + ":" + V[1]);
                        p.rel_Magnitude = float.Parse(V[1]);
                        break;
                    case 5:
                      Console.WriteLine(V[0]+ V[1]+":" + V[2]);
                        p.StaName = V[2];
                        break;
                    case 6:
                        Console.WriteLine(V[0] + V[1] + ":" + V[2]);
                        p.sta_Lat = float.Parse(V[2]);
                        stalat = double.Parse(V[2]);
                        break;
                    case 7:
                        Console.WriteLine(V[0] + V[1] + ":" + V[2]);
                        p.sta_Long = float.Parse(V[2]);
                        stalng = double.Parse(V[2]);
                        p.sta_EpcDist = GetDistance(lat, lng, stalat, stalng);
                        break;
                    case 8:
                       Console.WriteLine(V[0] + V[1] + ":" + V[2]);
                        break;
                    case 9:
                        Console.WriteLine(V[0] + V[1] + ":" + V[2] + " " + V[3]);
                        rTime = V[2] + " " + V[3];
                        p.Ptime = getPtime(oTime, rTime);
                        break;
                    case 10:
                      Console.WriteLine(V[0] + V[1] + ":" + V[2]);
                        break;
                    case 11:
                       Console.WriteLine(V[0] + V[1] + ":" + V[2]);
                        p.DurCurr = float.Parse(V[2]);
                        break;
                    case 13:
                       Console.WriteLine(V[0] + V[1] + ":" + V[2]);
                        break;
                    case 14:
                        Console.WriteLine(V[0] + V[1]+V[2] + ":" +  V[3]);
                        break;
                    case 15:
                        Console.WriteLine(V[0] + V[1] + ":" + V[2] + " " + V[3]);
                        break;
                    default:
                        Console.WriteLine(V[0] + ":" + V[1]);
                        break;
                }
                //
               
            }
            string FolderNmae= filename.Substring(filename.Length - 10);
            if (p.sta_EpcDist < 100) {
                p.FolderNmae = FolderNmae + "_100KM_";
            }else if (p.sta_EpcDist > 200)
            {
                p.FolderNmae = FolderNmae + "_out200KM_";
            }
            else
            {
                p.FolderNmae = FolderNmae + "_200KM_";
            }
            points.Add(p);
        }

        //震中距计算
        private static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double radLat1 = Rad(lat1);
            double radLng1 = Rad(lng1);
            double radLat2 = Rad(lat2);
            double radLng2 = Rad(lng2);
            double a = radLat1 - radLat2;
            double b = radLng1 - radLng2;
            double result = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2))) * EARTH_RADIUS;
            double km =Math.Round((result / 1000),6);
            return km;
        }
        private static double Rad(double d)
        {
            return (double)d * Math.PI / 180d;
        }

        //Ptime计算 只需要RecordTime即可，也可调用DateTimeToStamp
        private static double getPtime(string OTime,string RTime) {
            DateTime OriginTime = DateTime.Parse(OTime);
            DateTime RecordTime = DateTime.Parse(RTime);

            double ptime=DateTimeToStamp(RecordTime);
            return ptime;
        }

        // DateTime时间格式转换为Unix时间戳格式
        private static double DateTimeToStamp(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            if (chosen == "1")
            {
                return (double)((time - startTime).TotalSeconds);
            }
            else
            {
                return (double)((time - startTime).TotalSeconds + 15);
            }
        }
        //检查输出信息txt是否存在
        public void checkRecord()
        {
            Console.WriteLine("开始检查输出文件");
            string filepath = Directory.GetCurrentDirectory();

            string newpath = Path.Combine(filepath, "日志");
            DateTime dateTime = DateTime.Now;
            //string mydate = string.Format("{0:D4}{1:D2}{2:D2}", dateTime.Year, dateTime.Month, dateTime.Day);
            mydate = dateTime.ToString("yyyyMMddHHmmss");
            // 判断日志文件夹是否存在
            if (!File.Exists(newpath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(newpath); //创建文件夹
                directoryInfo.Create();
                string path;
                if (chosen == "1")
                {
                    path = "blog_发波日志分组" + mydate + ".txt";
                }
                else
                {
                    path = "blog_日本数据分组" + mydate + ".txt";
                }
                
                string blogpath = Path.Combine(newpath, path);
                if (!File.Exists(blogpath)) //判断blog.txt文件是否存在
                {
                    FileStream fs = File.Create(blogpath);//创建文件
                    fs.Close();
                }
                //else
                //{
                //    File.Delete(blogpath);
                //    FileStream fs = File.Create(blogpath);//创建文件
                //    fs.Close();
                //    //stream.SetLength(0);
                //    //stream.Close();
                //}


            }
            else
            {
                string path;
                if (chosen == "1")
                {
                    path = "blog_发波日志分组" + mydate + ".txt";
                }
                else
                {
                    path = "blog_日本数据分组" + mydate + ".txt";
                }
                string blogpath = Path.Combine(newpath, path);
                if (!File.Exists(blogpath))
                {
                    FileStream fs = File.Create(blogpath);//创建文件
                    fs.Close();
                }
                //else
                //{
                //    File.Delete(blogpath);
                //    FileStream fs = File.Create(blogpath);//创建文件
                //    fs.Close();
                //    //stream.Seek(0, SeekOrigin.Begin);
                //    //stream.SetLength(0);
                //    //stream.Close();
                //}
            }

        }

        //输出信息写入
        private void writeRecord(string info) {
            string filePath = Directory.GetCurrentDirectory();
            string newpath = Path.Combine(filePath, "日志");
            //DateTime dateTime = DateTime.Now;
            //string mydate = string.Format("{0:D4}{1:D2}{2:D2}", dateTime.Year, dateTime.Month, dateTime.Day);

            string path;
            if (chosen == "1")
            {
                path = "blog_发波日志分组" + mydate + ".txt";
            }
            else
            {
                path = "blog_日本数据分组" + mydate + ".txt";
            }

            string mypath = Path.Combine(newpath, path);
            FileStream fs = new FileStream(mypath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);
            try
            {
                //Console.WriteLine();
                sw.WriteLine(info);
                sw.Flush();
                sw.Close();


            }
            catch (IOException e)
            {
                sw.Flush();
                sw.Close();
                Console.WriteLine(e.ToString());
            }

        }
    }
}
