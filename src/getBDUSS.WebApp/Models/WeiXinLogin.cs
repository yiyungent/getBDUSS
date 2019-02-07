using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text.RegularExpressions;

namespace getBDUSS.WebApp.Models
{
    /// <summary>
    /// 微信登录
    /// </summary>
    public class WeiXinLogin
    {
        #region 获取微信登录二维码图片地址
        public dynamic GetWeiXinPic()
        {
            string url = "https://open.weixin.qq.com/connect/qrconnect?appid=wx85f17c29f3e648bf&response_type=code&scope=snsapi_login&redirect_uri=https%3A%2F%2Fpassport.baidu.com%2Fphoenix%2Faccount%2Fafterauth&state=" + Common.GetTimeStamp() + "&display=page&traceid=";
            string responseData = Common.HttpGet(url: url);
            Match match = Regex.Match(responseData, "connect/qrcode/(.*?)\"");
            string uuid;
            if (match.Success && !string.IsNullOrEmpty(uuid = match.Groups[1].Value))
            {
                return new { code = 0, uuid = uuid, imgurl = "https://open.weixin.qq.com/connect/qrcode/" + uuid };
            }
            else
            {
                return new { code = 1, msg = "获取二维码失败" };
            }
        }
        #endregion
    }
}