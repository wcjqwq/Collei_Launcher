﻿using Collei_Launcher;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

public static class Methods
{
    public static string Choice_Save_Path(string Filter = null, string Title = null, string InitialDirectory = null, string FileName = null)
    {
        if (InitialDirectory == null)
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }
        SaveFileDialog saveFileDialog1 = new SaveFileDialog();
        if (Filter != null)
            saveFileDialog1.Filter = Filter;
        if (FileName != null)
        {
            saveFileDialog1.FileName = FileName;
        }
        saveFileDialog1.InitialDirectory = InitialDirectory;
        if (Title != null)
            saveFileDialog1.Title = Title;
        DialogResult dr = saveFileDialog1.ShowDialog();
        if (dr == DialogResult.OK)
        {
            if (saveFileDialog1.FileName == "")
            {
                MessageBox.Show("请选择保存位置！", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return saveFileDialog1.FileName;
        }
        return null;
    }
    public static string Choice_Path(string Filter = null, string Title = null, string InitialDirectory = null)
    {
        if (InitialDirectory == null)
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }
        OpenFileDialog openFileDialog1 = new OpenFileDialog();
        if (Filter != null)
            openFileDialog1.Filter = Filter;
        openFileDialog1.FileName = "";
        openFileDialog1.InitialDirectory = InitialDirectory;
        if (Title != null)
            openFileDialog1.Title = Title;
        DialogResult dr = openFileDialog1.ShowDialog();
        if (dr == DialogResult.OK)
        {
            if (openFileDialog1.FileName == "")
            {
                MessageBox.Show("请选择一个文件！", "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return openFileDialog1.FileName;
        }
        return null;
    }
    public static bool HasChinese(string str)
    {
        return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
    }

    public static void Set_Proxy(string proxy)
    {
        using (RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true))
        {
            regkey.SetValue("ProxyEnable", 1);
            regkey.SetValue("ProxyHttp1.1", 1);
            regkey.SetValue("ProxyServer", proxy);
        }
    }
    public static bool Start(string target)
    {
        string show = "您将以管理员权限运行或打开以下内容:\n\n";
        show += target;
        show += "\n\n若您想运行或打开此内容,请点击“确定”，若不想打开，请点击“取消”";
        if (MessageBox.Show(show, "要运行或打开此内容吗?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
        {
            System.Diagnostics.Process.Start(target);
            return true;
        }
        else
        {
            return false;
        }
    }
    public static void Clear_Proxy()
    {
        using (RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true))
        {
            try
            {
                regkey.SetValue("ProxyEnable", 0);
                regkey.DeleteValue("ProxyServer");
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }
    }

    public static Proxy_Config using_proxy()
    {
        Proxy_Config proxy = new Proxy_Config();
        try
        {
            using (RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings"))
            {
                if (regkey.GetValue("ProxyEnable").ToString() == "1")
                {
                    proxy.ProxyEnable = true;
                }
                object ps = regkey.GetValue("ProxyServer");
                if (ps != null)
                {
                    proxy.ProxyServer = ps.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return proxy;
    }


    public static string Get_Proxy_Text()
    {
        string st = "代理";
        Proxy_Config pc = using_proxy();
        if (pc.ProxyEnable == true)
        {
            st += "已开启,代理服务器地址:";
            string[] servers = pc.ProxyServer.Split(';');
            for (int i = 0; i < servers.Length; i++)
            {
                st += "\n" + servers[i];
            }
        }
        else
        {
            st += "未开启";
        }
        Debug.Print(st);
        return st;
    }
    public static void Add_Cert(X509Certificate2 cert)
    {
        if (cert == null)
            return;
        X509Store certStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
        certStore.Open(OpenFlags.ReadWrite);
        try
        {
            //将伪造的证书加入到本地的证书库
            certStore.Add(cert);
        }
        finally
        {
            certStore.Close();
        }
    }
    public static void Remove_Cert(X509Certificate2 cert)
    {
        if (cert == null)
            return;
        X509Store certStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
        certStore.Open(OpenFlags.ReadWrite);
        try
        {
            //将伪造的证书刪除
            if (certStore.Certificates.Contains(cert))
                certStore.Remove(cert);
        }
        finally
        {
            certStore.Close();
        }
    }
    /// <summary>
    /// 16进制原码字符串转字节数组
    /// </summary>
    /// <param name="hexString">"AABBCC"或"AA BB CC"格式的字符串</param>
    /// <returns></returns>
    public static byte[] ConvertHexStringToBytes(string hexString)
    {
        try
        {
            hexString = hexString.Replace(" ", "");
            if (hexString.Length % 2 != 0)
            {
                MessageBox.Show("参数长度不正确", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "在转换byte[]时出现错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }
    /// <summary>
    /// 将ua中的key转换为正常的dispatchkey
    /// </summary>
    /// <param name="bytes">ua中的key</param>
    /// <returns></returns>
    public static string ToOriginalKey(byte[] bytes)
    {
        List<byte> list = bytes.ToList();
        for (int i = 1; i < 30; i++)
        {
            list.RemoveRange(list.Count - 7 - i * 8, 9);
        }
        for (int i = 0; i < 14; i++)
        {
            list.RemoveRange(2 + 8 * i, 6);
        }
        return Encoding.UTF8.GetString(list.ToArray());
    }
    /// <summary>
    /// 填充passwordkey
    /// </summary>
    /// <param name="key"></param>
    /// <returns>填充后key的Byte[]</returns>
    public static byte[] ToFixBytesP1(string key)
    {
        byte[] bp1 = Encoding.UTF8.GetBytes(key.Substring(0, 48));
        byte[] bp2 = Encoding.UTF8.GetBytes(key.Substring(48, 57));
        byte[] bp3 = Encoding.UTF8.GetBytes(key.Substring(105, 57));
        byte[] bp4 = Encoding.UTF8.GetBytes(key.Substring(162));
        byte[] ret = bp1.Concat(bytes).Concat(bp2).Concat(bytes).Concat(bp3).Concat(bytes).Concat(bp4).ToArray();
        return ret;

    }
    internal static long Pas = 0x4889501048BA;
    internal static long Pbs = 0x908000000048BA;
    internal static byte[] Pbb = { 0x48, 0x89 };
    /// <summary>
    /// 计算填充第一部分数据
    /// </summary>
    /// <param name="count">位置</param>
    /// <returns></returns>
    internal static byte[] GetBytesPa(int count)//0-13
    {
        byte[] pa = BitConverter.GetBytes(Pas + (0x80000 * count));
        Array.Resize(ref pa, 6);
        pa = pa.Reverse().ToArray();
        return pa;
    }
    /// <summary>
    /// 计算填充第二部分数据
    /// </summary>
    /// <param name="count">位置</param>
    /// <returns></returns>
    internal static byte[] GetBytesPb(int count)//0-28
    {
        byte[] pb = BitConverter.GetBytes(Pbs + (0x80000000000 * count));
        Array.Resize(ref pb, 7);
        byte[] pb2 = Pbb.Concat(pb.Reverse()).ToArray();
        if (count >= 16)
        {
            pb2[2] = 0x90;
            pb2[4] = 0x01;
        }
        return pb2;
    }
    /// <summary>
    /// 填充ua的dispatchkey
    /// </summary>
    /// <param name="key">需要填充的key</param>
    /// <returns>填充后key的Byte[]</returns>
    public static byte[] ToUABytes(string key)
    {
        int count = key.Length + 2;
        List<byte> uabytes = Encoding.UTF8.GetBytes(key).ToList();
        for (int i = 1; i < 44; i++)
        {
            if (i <= 29)
            {
                byte[] k = GetBytesPb(29 - i);
                for (int j = 0; j < k.Length; j++)
                {
                    uabytes.Insert(count - 8 * i, k[k.Length - 1 - j]);
                }
            }
            else
            {
                byte[] k = GetBytesPa(14 - (i - 29));
                for (int j = 0; j < k.Length; j++)
                {
                    uabytes.Insert(count - 8 * i, k[k.Length - 1 - j]);
                }
            }
        }
        return uabytes.ToArray();
    }
    public static byte[] bytes = { 0x0D, 0x0A, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

    /// <summary>
    /// 查找Byte[]位置
    /// </summary>
    /// <param name="src">被查找的Byte[]</param>
    /// <param name="find">查找内容</param>
    /// <returns>位置</returns>
    public static int FindBytes(byte[] src, byte[] find)
    {
        int index = -1;
        int matchIndex = 0;

        for (int i = 0; i < src.Length; i++)
        {
            if (src[i] == find[matchIndex])
            {
                if (matchIndex == (find.Length - 1))
                {
                    index = i - matchIndex;
                    break;
                }
                matchIndex++;
            }
            else
            {
                matchIndex = 0;
            }

        }
#if DEBUG
        Debug.Print("FindIndex:" + index);
#endif
        return index;
    }

    /// <summary>
    /// 替换Byte[]
    /// </summary>
    /// <param name="src">需要替换Byte[]的Byte[]</param>
    /// <param name="search">原Byte[]</param>
    /// <param name="repl">需要替换成的Byte[]</param>
    /// <param name="i">替换成功会+1</param>
    /// <returns>替换完成的Byte</returns>
    public static byte[] ReplaceBytes(byte[] src, byte[] search, byte[] repl, ref int i)
    {
        byte[] dst = src;
        int index = FindBytes(src, search);
        if (index == -1)
        {
            return src;
        }
        if (index >= 0)
        {
            dst = new byte[src.Length - search.Length + repl.Length];

            Buffer.BlockCopy(src, 0, dst, 0, index);

            Buffer.BlockCopy(repl, 0, dst, index, repl.Length);

            Buffer.BlockCopy(
                src,
                index + search.Length,
                dst,
                index + repl.Length,
                src.Length - (index + search.Length));
        }
        i++;
        return dst;
    }
    /// <summary>
    /// 字节数组转16进制字符串：空格分隔
    /// </summary>
    /// <param name="byteDatas"></param>
    /// <returns></returns>
    public static string ToHexStrFromByte(this byte[] byteDatas)
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < byteDatas.Length; i++)
        {
            builder.Append(string.Format("{0:X2} ", byteDatas[i]));
        }
        return builder.ToString().Trim();
    }

    public static bool DebugBuild(Assembly assembly)
    {
        foreach (object attribute in assembly.GetCustomAttributes(false))
        {
            if (attribute is DebuggableAttribute)
            {
                DebuggableAttribute _attribute = attribute as DebuggableAttribute;

                return _attribute.IsJITTrackingEnabled;
            }
        }
        return false;
    }
    public static string ConvertJsonString(string str)
    {
        //格式化json字符串
        JsonSerializer serializer = new JsonSerializer();
        TextReader tr = new StringReader(str);
        JsonTextReader jtr = new JsonTextReader(tr);
        object obj = serializer.Deserialize(jtr);
        if (obj != null)
        {
            StringWriter textWriter = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = 4,
                IndentChar = ' '
            };
            serializer.Serialize(jsonWriter, obj);
            return textWriter.ToString();
        }
        else
        {
            return str;
        }
    }
    /// <summary>
    /// 将文件大小(字节)转换为最适合的显示方式
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static string ConvertFileSize(long size)
    {
        string result = "0KB";
        int filelength = size.ToString().Length;
        if (filelength < 4)
            result = size + "byte";
        else if (filelength < 7)
            result = Math.Round(Convert.ToDouble(size / 1024d), 2) + "KB";
        else if (filelength < 10)
            result = Math.Round(Convert.ToDouble(size / 1024d / 1024), 2) + "MB";
        else if (filelength < 13)
            result = Math.Round(Convert.ToDouble(size / 1024d / 1024 / 1024), 2) + "GB";
        else
            result = Math.Round(Convert.ToDouble(size / 1024d / 1024 / 1024 / 1024), 2) + "TB";
        return result;
    }

    /// <summary>
    /// 指定Post地址使用Get 方式获取全部字符串
    /// </summary>
    /// <param name="url">请求后台地址</param>
    /// <param name="content">Post提交数据内容(utf-8编码的)</param>
    /// <returns></returns>
    public static string Post(string url, string content)
    {
        try
        {
            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.Timeout = 2000;
            req.ContentType = "application/json";
            #region 添加Post 参数
            byte[] data = Encoding.UTF8.GetBytes(content);
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            #endregion

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
        catch (Exception e)
        {
            Debug.Print(e.Message);
            return null;
        }
    }
    /// <summary>
    /// 获取当前的时间戳
    /// </summary>
    /// <returns></returns>
    public static string Timestamp()
    {
        long ts = ConvertDateTimeToInt(DateTime.Now);
        return ts.ToString();
    }

    /// <summary>  
    /// 将c# DateTime时间格式转换为Unix时间戳格式  
    /// </summary>  
    /// <param name="time">时间</param>  
    /// <returns>long</returns>  
    public static long ConvertDateTimeToInt(System.DateTime time)
    {
        //System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
        //long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
        long t = (time.Ticks - 621356256000000000) / 10000;
        return t;
    }
    public static DateTime getdate(long jsTimeStamp)
    {
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
        DateTime dt = startTime.AddMilliseconds(jsTimeStamp);
        return dt;
        //System.Console.WriteLine(dt.ToString("yyyy/MM/dd HH:mm:ss:ffff"));
    }
    /// <summary>
    /// 设置证书安全性
    /// </summary>
    public static void SetCertificatePolicy()
    {
        ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
    }
    ///  <summary>
    ///  远程证书验证
    ///  </summary>
    public static bool RemoteCertificateValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
    {
        return true;
    }

    public static string Get(string url)
    {
        try
        {
            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
            DateTime st = DateTime.Now;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 3000;
            request.ContentType = "application/json";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var text = new StreamReader(response.GetResponseStream()).ReadToEnd();
            Debug.WriteLine(Methods.ConvertDateTimeToInt(response.LastModified) - Methods.ConvertDateTimeToInt(st) + "ms");
            response.Close();
            return text;
        }
        catch (Exception e)
        {
            Debug.Print(e.Message);
            return null;
        }
    }
    public static Details_Get Get_for_Index(string url)
    {
        var tk = Task.Factory.StartNew(() =>
        {
            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 10000;
            request.ContentType = "application/json";
            Details_Get res = new Details_Get();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var text = new StreamReader(response.GetResponseStream()).ReadToEnd();
                sw.Stop();
                res.Result = text;
                res.Use_time = sw.ElapsedMilliseconds;
                res.StatusCode = response.StatusCode;
                response.Close();
                return res;
            }
            catch (WebException ex)
            {
                sw.Stop();
                if (ex.Response == null)
                {
                    res.Result = ex.Message;
                    res.Use_time = -1;
                    //Console.WriteLine("ex.Response == null");
                    return res;
                }
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                res.Use_time = sw.ElapsedMilliseconds;
                res.StatusCode = response.StatusCode;
                res.Result = response.StatusDescription;
                Console.WriteLine("错误码:" + (int)response.StatusCode);
                Console.WriteLine("错误码描述:" + response.StatusDescription);
                return res;
            }
            catch (Exception e)
            {
                res.Use_time = -2;
                res.Result = e.Message; ;
                Debug.Print(e.Message);
                return null;
            }
        });
        tk.Wait(3000);
        return tk.Result;

    }


    public static class GameRegReader
    {
        /// <summary>
        /// 获取游戏目录，是静态方法
        /// </summary>
        /// <returns></returns>
        public static string GetGamePath()
        {
            try
            {
                string startpath = "";
                string launcherpath = GetLauncherPath();
                #region 获取游戏启动路径，和官方配置一致
                string cfgPath = Path.Combine(launcherpath, "config.ini");
                if (File.Exists(launcherpath) || File.Exists(cfgPath))
                {
                    //获取游戏本体路径
                    using (StreamReader reader = new StreamReader(cfgPath))
                    {
                        string[] abc = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        foreach (var item in abc)
                        {
                            //从官方获取更多配置
                            if (item.IndexOf("game_install_path") != -1)
                            {
                                startpath += item.Substring(item.IndexOf("=") + 1);
                            }
                        }
                    }
                }
                byte[] bytearr = Encoding.UTF8.GetBytes(startpath);
                string path = Encoding.UTF8.GetString(bytearr);
                return path;
            }
            catch
            {
                return null;
            }
            #endregion
        }


        /// <summary>
        /// 启动器地址
        /// </summary>
        /// <returns></returns>
        public static string GetLauncherPath()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine;            //打开指定注册表根
                                                                    //获取官方启动器路径
                string launcherpath = "";
                try
                {
                    launcherpath = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\原神").GetValue("InstallPath").ToString();


                }
                catch (Exception)
                {
                    launcherpath = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Genshin Impact").GetValue("InstallPath").ToString();

                }

                byte[] bytepath = Encoding.UTF8.GetBytes(launcherpath);     //编码转换
                string path = Encoding.UTF8.GetString(bytepath);
                return path;

            }
            catch
            {
                return null;
            }
        }

        public static string GetGameExePath()
        {

            var gamepath = GameRegReader.GetGamePath();
            if (gamepath == null)
            {
                return null;
            }
            var cnpath = gamepath + @"/YuanShen.exe";
            var ospath = gamepath + @"/GenshinImpact.exe";

            if (File.Exists(cnpath))
            {
                return cnpath;
            }
            else if (File.Exists(ospath))
            {
                return ospath;
            }
            else
            {
                return null;
            }
        }
    }
}

