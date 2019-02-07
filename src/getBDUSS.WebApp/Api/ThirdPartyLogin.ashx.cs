using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using getBDUSS.WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace getBDUSS.WebApp.Api
{
    /// <summary>
    /// Summary description for ThirdPartyLogin
    /// </summary>
    public class ThirdPartyLogin : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            WeiXinLogin weiXinLogin = new WeiXinLogin();
            string doAction = context.Request.QueryString["do"];
            dynamic jsonObj = new JObject();
            if (string.IsNullOrEmpty(doAction))
            {
                context.Response.End();
                return;
            }
            if ("qrlogin" == doAction)
            {
            }
            if ("getqrpic" == doAction)
            {
            }
            if ("getwxpic" == doAction)
            {
                jsonObj = weiXinLogin.GetWeiXinPic();
            }

            context.Response.Write(JsonConvert.SerializeObject(jsonObj));
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