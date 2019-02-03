using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Text;
using System.IO.Compression;

namespace getBDUSS.WebApp.Models
{
    public class Common
    {
        /// <summary>
        /// 返回 当前 Unix 时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            long unixDate = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            return unixDate.ToString();
        }

        /// <summary>
        /// HTTP Get请求
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="isPost"></param>
        /// <param name="postDataStr">参数.如name=admin&pwd=admin</param>
        /// <param name="referer"></param>
        /// <param name="cookies"></param>
        /// <param name="ua"></param>
        /// <returns>返回请求回复字符串</returns>
        public static string HttpGet(string url, string postDataStr = "", string referer = null, Dictionary<string, string> cookies = null, string ua = null)
        {
            string rtResult = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";
                // 注意：该 Accept由公开API提供，不能这样修改。参考：https://stackoverflow.com/questions/239725/cannot-set-some-http-headers-when-using-system-net-webrequest
                //request.Headers.Add("Accept", "application/json");
                request.Accept = "application/json";
                request.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
                request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
                #region Error
                //request.Headers.Add("Connection", "close");
                //request.Connection = "close"; 
                // System.ArgumentException: 'Keep-Alive and Close may not be set using this property.
                // Parameter name: value'
                #endregion
                request.KeepAlive = false;
                if (cookies != null && cookies.Count > 0)
                {
                    foreach (string name in cookies.Keys)
                    {
                        request.CookieContainer.Add(new Cookie(name, cookies[name]));
                    }
                }
                if (!string.IsNullOrEmpty(referer))
                {
                    request.Referer = "https://wappass.baidu.com/";
                }
                if (!string.IsNullOrEmpty(ua))
                {
                    request.UserAgent = ua;
                }
                else
                {
                    request.UserAgent = "Mozilla/5.0 (Linux; Android 4.4.2; H650 Build/KOT49H) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/30.0.0.0 Mobile Safari/537.36";
                }
                request.Timeout = 10000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                //如果http头中接受gzip的话，这里就要判断是否为有压缩，有的话，直接解压缩即可  
                if (response.Headers["Content-Encoding"] != null && response.Headers["Content-Encoding"].ToLower().Contains("gzip"))
                {
                    responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                }
                using (StreamReader sReader = new StreamReader(responseStream, System.Text.Encoding.UTF8))
                {
                    rtResult = sReader.ReadToEnd();
                }
                responseStream.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return rtResult;
        }


        public static string HttpPost(string url, string postDataStr = "", string referer = null, Dictionary<string, string> cookies = null, string ua = null)
        {
            string rtResult = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "text/html;charset=UTF-8";
                // 注意：该 Accept由公开API提供，不能这样修改。参考：https://stackoverflow.com/questions/239725/cannot-set-some-http-headers-when-using-system-net-webrequest
                //request.Headers.Add("Accept", "application/json");
                request.Accept = "application/json";
                request.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
                request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
                request.KeepAlive = false;
                if (cookies != null && cookies.Count > 0)
                {
                    foreach (string name in cookies.Keys)
                    {
                        request.CookieContainer.Add(new Cookie(name, cookies[name]));
                    }
                }
                if (string.IsNullOrEmpty(referer))
                {
                    request.Referer = "https://wappass.baidu.com/";
                }
                if (!string.IsNullOrEmpty(ua))
                {
                    request.UserAgent = ua;
                }
                else
                {
                    request.UserAgent = "Mozilla/5.0 (Linux; Android 4.4.2; H650 Build/KOT49H) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/30.0.0.0 Mobile Safari/537.36";
                }
                request.Timeout = 10000;
                byte[] postBytes = Encoding.UTF8.GetBytes(postDataStr);
                request.ContentLength = postBytes.Length;
                // 写 content-body 一定要在属性设置之后
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                //如果http头中接受gzip的话，这里就要判断是否为有压缩，有的话，直接解压缩即可  
                if (response.Headers["Content-Encoding"] != null && response.Headers["Content-Encoding"].ToLower().Contains("gzip"))
                {
                    responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                }
                using (StreamReader sReader = new StreamReader(responseStream, System.Text.Encoding.UTF8))
                {
                    rtResult = sReader.ReadToEnd();
                }
                responseStream.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return rtResult;
        }

        public static Image HttpGetImg(string url, string postDataStr = "", string referer = null, Dictionary<string, string> cookies = null, string ua = null)
        {
            Image rtResult;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create
                    (url + (postDataStr == "" ? "" : "?" + postDataStr));
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";
                // 注意：该 Accept由公开API提供，不能这样修改。参考：https://stackoverflow.com/questions/239725/cannot-set-some-http-headers-when-using-system-net-webrequest
                //request.Headers.Add("Accept", "application/json");
                request.Accept = "application/json";
                request.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
                request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
                #region Error
                //request.Headers.Add("Connection", "close");
                //request.Connection = "close"; 
                // System.ArgumentException: 'Keep-Alive and Close may not be set using this property.
                // Parameter name: value'
                #endregion
                request.KeepAlive = false;
                if (cookies != null && cookies.Count > 0)
                {
                    foreach (string name in cookies.Keys)
                    {
                        request.CookieContainer.Add(new Cookie(name, cookies[name]));
                    }
                }
                if (string.IsNullOrEmpty(referer))
                {
                    request.Referer = "https://wappass.baidu.com/";
                }
                if (!string.IsNullOrEmpty(ua))
                {
                    request.UserAgent = ua;
                }
                else
                {
                    request.UserAgent = "Mozilla/5.0 (Linux; Android 4.4.2; H650 Build/KOT49H) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/30.0.0.0 Mobile Safari/537.36";
                }
                request.Timeout = 10000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    rtResult = Image.FromStream(responseStream);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return rtResult;
        }

        public static dynamic JsonStr2Obj(string jsonStr)
        {
            return JsonConvert.DeserializeObject<dynamic>(jsonStr);
        }

        public static bool IsPropertyExist(dynamic data, string propertyname)
        {
            // 不能将 JObject (data) 转换成 IDictionary<string, object>
            //IDictionary<string, object> dic = (IDictionary<string, object>)data;
            if (data is JObject)
            {
                return ((JObject)data).ContainsKey(propertyname);
            }
            return false;
        }
    }
}