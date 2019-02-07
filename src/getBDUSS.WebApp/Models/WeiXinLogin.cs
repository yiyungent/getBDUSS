using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text.RegularExpressions;
using System.Text;

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

        #region 微信登录
        public dynamic Login(string uuid, string last)
        {
            if (string.IsNullOrEmpty(uuid)) return new { saveOK = -1, msg = "uuid不能为空" };
            last = string.IsNullOrEmpty(last) ? "" : "&last=" + last;
            string url = "https://long.open.weixin.qq.com/connect/l/qrconnect?uuid=" + uuid + last + "&_=" + Common.GetTimeStamp() + "000";
            string rtnResponseData = Common.HttpGet(url: url, referer: "https://open.weixin.qq.com/connect/qrconnect");
            Match match = Regex.Match(rtnResponseData, "wx_errcode=(\\d+);window.wx_code=\'(.*?)\'");
            if (match.Success)
            {
                string errcode = match.Groups[1].Value;
                string code = match.Groups[2].Value;
                if ("408" == errcode)
                {
                    return new { code = 1, msg = "二维码未失效" };
                }
                else if ("404" == errcode)
                {
                    return new { code = 2, msg = "请在微信中点击确认即可登录" };
                }
                else if ("402" == errcode)
                {
                    return new { code = 3, msg = "二维码已失效" };
                }
                else if ("405" == errcode)
                {
                    url = "https://passport.baidu.com/phoenix/account/startlogin?type=42&tpl=pp&u=https%3A%2F%2Fpassport.baidu.com%2F&display=popup&act=optional";
                    StringBuilder responseHeadersSb = new StringBuilder();
                    string responseData = Common.HttpGet(url: url, responseHeadersSb: responseHeadersSb);
                    // 响应头 + 响应体
                    responseData = responseHeadersSb.ToString() + "\r\n\r\n" + responseData;
                    match = Regex.Match(responseData, "mkey=(.*?);");
                    string mkey;
                    Match bdussMatch, stokenMatch, ptokenMatch, unameMatch, displaynameMatch;
                    if (match.Success && !string.IsNullOrEmpty(mkey = match.Groups[1].Value))
                    {
                        url = "https://passport.baidu.com/phoenix/account/afterauth?mkey=" + mkey + "&appid=wx85f17c29f3e648bf&traceid=&code=" + code + "&state=" + Common.GetTimeStamp();
                        Dictionary<string, string> cookies = new Dictionary<string, string>();
                        cookies.Add("mkey", mkey);
                        responseData = Common.HttpGet(url: url, cookies: cookies, responseHeadersSb: responseHeadersSb);
                        // BUG: 响应头+响应体：响应体中显示已经登录成功，但是均无BDUSS等信息，导致 "登录成功，回调百度失败！"
                        responseData = responseHeadersSb.ToString() + "\r\n\r\n" + responseData;
                        bdussMatch = Regex.Match(responseData, "BDUSS=(.*?);");
                        stokenMatch = Regex.Match(responseData, "STOKEN=(.*?);");
                        ptokenMatch = Regex.Match(responseData, "PTOKEN=(.*?);");
                        unameMatch = Regex.Match(responseData, "passport_uname: '(.*?)'");
                        displaynameMatch = Regex.Match(responseData, "displayname: '(.*?)'");
                    }
                    else
                    {
                        return new { code = -1, msg = "登录成功，获取mkey失败！" };
                    }
                    if (bdussMatch.Success && stokenMatch.Success && ptokenMatch.Success)
                    {
                        return new { code = 0, uid = (new BaiduLogin()).GetUserId(unameMatch.Groups[1].Value), user = unameMatch.Groups[1].Value, displayname = displaynameMatch.Groups[1].Value, bduss = bdussMatch.Groups[1].Value, ptoken = ptokenMatch.Groups[1].Value, stoken = stokenMatch.Groups[1].Value };
                    }
                    else
                    {
                        return new { code = -1, msg = "登录成功，回调百度失败！" };
                    }
                }
                else
                {
                    return new { code = -1, msg = rtnResponseData };
                }
            }
            else if (!string.IsNullOrEmpty(rtnResponseData))
            {
                return new { code = -1, msg = rtnResponseData };
            }
            else
            {
                return new { code = 1 };
            }
        }
        #endregion
    }
}