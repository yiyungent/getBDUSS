using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using getBDUSS.WebApp.Models;
using Newtonsoft.Json;
using System.Text;
using System.Drawing;
using System.IO;

namespace getBDUSS.WebApp.Api
{
    /// <summary>
    /// Summary description for Login
    /// </summary>
    public class Login : IHttpHandler
    {
        private HttpContext _context;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            this._context = context;

            BaiduLogin login = new BaiduLogin();
            dynamic jsonObj = null;
            if (Get("do") == "time")
            {
                jsonObj = login.ServerTime();
            }
            if (Get("do") == "checkvc")
            {
                jsonObj = login.CheckVc(Post("user"));
            }
            if (Get("do") == "sendcode")
            {
                jsonObj = login.SendCode(Post("type"), Post("lstr"), Post("ltoken"));
            }
            if (Get("do") == "login")
            {
                jsonObj = login.Login(Post("time"), Post("user"), Post("pwd"), Post("p"), Post("vcode"), Post("vcodestr"));
            }
            if (Get("do") == "login2")
            {
                jsonObj = login.Login2(Post("type"), Post("lstr"), Post("ltoken"), Post("vcode"));
            }
            if (Get("do") == "getvcpic")
            {
                context.Request.ContentType = "image/jpeg";
                //string imgStr = login.GetVcpic(Get("vcodestr"));
                //context.Response.ClearContent();
                //byte[] imgBuffer = Encoding.UTF8.GetBytes(imgStr);
                //context.Response.BinaryWrite(imgBuffer);
                Image img = login.GetVcpic(Get("vcodestr"));
                MemoryStream ms = new MemoryStream();
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                img.Dispose();
                context.Response.BinaryWrite(ms.ToArray());
                context.Response.End();
            }
            if (Get("do") == "getphone")
            {
                jsonObj = login.GetPhone(Post("phone"));
            }
            if (Get("do") == "sendsms")
            {
                jsonObj = login.SendSms(Post("phone"), Post("vcode"), Post("vcodestr"), Post("vcodesign"));
            }
            if (Get("do") == "login3")
            {
                jsonObj = login.Login3(Post("phone"), Post("smsvc"));
            }
            if (Get("do") == "getqrcode")
            {
                jsonObj = login.GetQrCode();
            }
            if (Get("do") == "qrlogin")
            {
                jsonObj = login.QrLogin(Post("sign"));
            }
            context.Response.Write(JsonConvert.SerializeObject(jsonObj));
        }

        private string Get(string key)
        {
            return this._context.Request.QueryString[key];
        }

        private string Post(string key)
        {
            return this._context.Request.Form[key];
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}