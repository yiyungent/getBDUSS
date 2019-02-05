using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text.RegularExpressions;
using System.Drawing;
using System.Text;

namespace getBDUSS.WebApp.Models
{
    public class BaiduLogin
    {
        private string _referrer = "https://wappass.baidu.com/passport/login?clientfrom=native&tpl=tb&login_share_strategy=choice&client=android&adapter=3&t=1485501702555&act=bind_mobile&loginLink=0&smsLoginLink=0&lPFastRegLink=0&fastRegLink=1&lPlayout=0&loginInitType=0";

        #region 获取 ServerTime
        /// <summary>
        /// 获取 ServerTime
        /// </summary>
        /// <returns></returns>
        public dynamic ServerTime()
        {
            string url = "https://wappass.baidu.com/wp/api/security/antireplaytoken?tpl=tb&v=" + Common.GetTimeStamp() + "0000";
            string data = Common.HttpGet(url: url, referer: this._referrer);
            dynamic jsonObj = Common.JsonStr2Obj(data);
            if (jsonObj.errno == 110000)
            {
                dynamic rtnObj = new { code = 0, time = jsonObj.time };
                return rtnObj;
            }
            else
            {
                dynamic rtnObj = new { code = -1, msg = jsonObj.errormsg };
                return rtnObj;
            }
        }
        #endregion

        #region 获取验证码图片
        /// <summary>
        /// 获取验证码图片
        /// </summary>
        /// <param name="vCodeStr"></param>
        /// <returns></returns>
        public Image GetVcpic(string vCodeStr)
        {
            string url = "https://wappass.baidu.com/cgi-bin/genimage?" + vCodeStr + "&v=" + Common.GetTimeStamp() + "0000";
            //return Common.HttpAide(url: url,  referer: this._referrer);
            return Common.HttpGetImg(url: url, referer: this._referrer);
        }
        #endregion

        #region 普通登录操作
        /// <summary>
        /// 普通登录操作
        /// </summary>
        /// <param name="time"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="p"></param>
        /// <param name="vCode"></param>
        /// <param name="vCodeStr"></param>
        /// <returns></returns>
        public dynamic Login(string time, string user, string pwd, string p, string vCode = null, string vCodeStr = null)
        {
            if (string.IsNullOrEmpty(user)) return new { code = -1, msg = "用户名不能为空" };
            if (string.IsNullOrEmpty(pwd)) return new { code = -1, msg = "pwd不能为空" };
            if (string.IsNullOrEmpty(p)) return new { code = -1, msg = "密码不能为空" };
            if (string.IsNullOrEmpty(vCode)) vCode = "";
            if (string.IsNullOrEmpty(vCodeStr)) vCodeStr = "";

            string url = "https://wappass.baidu.com/wp/api/login?v=" + Common.GetTimeStamp() + "0000";
            string postData = "username=" + user + "&code=&password=" + p + "&verifycode=" + (vCode == null || vCode == "null" ? "" : vCode) + "&clientfrom=native&tpl=tb&login_share_strategy=choice&client=android&adapter=3&t=" + Common.GetTimeStamp() + "0000&act=bind_mobile&loginLink=0&smsLoginLink=1&lPFastRegLink=0&fastRegLink=1&lPlayout=0&loginInitType=0&lang=zh-cn&regLink=1&action=login&loginmerge=1&isphone=0&dialogVerifyCode=&dialogVcodestr=&dialogVcodesign=&gid=660BDF6-30E5-4A83-8EAC-F0B4752E1C4B&vcodestr=" + (vCodeStr == null || vCodeStr == "null" ? "" : vCodeStr) + "&countrycode=&servertime=" + time + "&logLoginType=sdk_login&passAppHash=&passAppVersion=";
            string data = Common.HttpPost(url: url, postDataStr: postData, referer: this._referrer);
            dynamic jsonObj = Common.JsonStr2Obj(data);
            if (Common.IsPropertyExist(jsonObj, "errInfo") && jsonObj.errInfo.no == "0")
            {
                if (!string.IsNullOrEmpty(jsonObj.data.loginProxy))
                {
                    data = Common.HttpGet(url: jsonObj.data.loginProxy, referer: this._referrer);
                    jsonObj = Common.JsonStr2Obj(data);
                }
                data = jsonObj.data.xml;
                user = Regex.Match(data, @"<uname>(.*?)</uname>").Groups[1].Value;
                string uid = Regex.Match(data, @"<uid>(.*?)</uid>").Groups[1].Value;
                string face = Regex.Match(data, @"<portrait>(.*?)</portrait>").Groups[1].Value;
                string displayname = Regex.Match(data, @"<displayname>(.*?)</displayname>").Groups[1].Value;
                string bduss = Regex.Match(data, @"<bduss>(.*?)</bduss>").Groups[1].Value;
                string ptoken = Regex.Match(data, @"<ptoken>(.*?)</ptoken>").Groups[1].Value;
                string stoken = Regex.Match(data, @"<stoken>(.*?)</stoken>").Groups[1].Value;
                return new { code = 0, uid = uid, user = user, displayname = displayname, face = face, bduss = bduss, ptoken = ptoken, stoken = stoken };
            }
            else if (jsonObj.errInfo.no == "310006" || jsonObj.errInfo.no == "500001" || jsonObj.errInfo.no == "500002")
            {
                return new { code = jsonObj.errInfo.no, msg = jsonObj.errInfo.msg, vcodestr = jsonObj.data.codeString };
            }
            else if (jsonObj.errInfo.no == "400023")
            {
                return new { code = jsonObj.errInfo.no, msg = jsonObj.errInfo.msg, type = jsonObj.data.showType, phone = jsonObj.data.phone, email = jsonObj.data.email, lstr = jsonObj.data.lstr, ltoken = jsonObj.data.ltoken };
            }
            else if (Common.IsPropertyExist(jsonObj, "errInfo"))
            {
                return new { code = jsonObj.errInfo.no, msg = jsonObj.errInfo.msg };
            }
            else
            {
                return new { code = -1, msg = "登录失败，原因未知" };
            }
        }
        #endregion

        #region 登录异常时发送手机/邮件验证码
        /// <summary>
        /// 登录异常时发送手机/邮件验证码
        /// </summary>
        /// <param name="type"></param>
        /// <param name="lstr"></param>
        /// <param name="ltoken"></param>
        /// <returns></returns>
        public dynamic SendCode(string type, string lstr, string ltoken)
        {
            string url = "https://wappass.baidu.com/wp/login/sec?ajax=1&v=" + Common.GetTimeStamp() + "0000&vcode=&clientfrom=native&tpl=tb&login_share_strategy=choice&client=android&adapter=3&t=" + Common.GetTimeStamp() + "0000&act=bind_mobile&loginLink=0&smsLoginLink=1&lPFastRegLink=0&fastRegLink=1&lPlayout=0&loginInitType=0&lang=zh-cn&regLink=1&action=login&loginmerge=1&isphone=0&dialogVerifyCode=&dialogVcodestr=&dialogVcodesign=&gid=660BDF6-30E5-4A83-8EAC-F0B4752E1C4B&showtype=" + type + "&lstr=" + RFC3986Encoder.UrlEncode(lstr) + "&ltoken=" + ltoken;
            string data = Common.HttpGet(url: url, referer: this._referrer);
            dynamic jsonObj = Common.JsonStr2Obj(data);
            if (Common.IsPropertyExist(jsonObj, "errInfo") && jsonObj.errInfo.no == "0")
            {
                return new { code = 0 };
            }
            else if (Common.IsPropertyExist(jsonObj, "errInfo"))
            {
                return new { code = jsonObj.errInfo.no, msg = jsonObj.errInfo.msg };
            }
            else
            {
                return new { code = -1, msg = "发生验证码失败，原因未知" };
            }
        }
        #endregion

        #region 登录异常时登录操作
        /// <summary>
        /// 登录异常时登录操作
        /// </summary>
        /// <param name="type"></param>
        /// <param name="lstr"></param>
        /// <param name="ltoken"></param>
        /// <param name="vcode"></param>
        /// <returns></returns>
        public dynamic Login2(string type, string lstr, string ltoken, string vcode)
        {
            if (string.IsNullOrEmpty(type)) return new { code = -1, msg = "type不能为空" };
            if (string.IsNullOrEmpty(lstr)) return new { code = -1, msg = "lstr不能为空" };
            if (string.IsNullOrEmpty(ltoken)) return new { code = -1, msg = "ltoken不能为空" };
            if (string.IsNullOrEmpty(vcode)) return new { code = -1, msg = "vcode不能为空" };

            string url = "https://wappass.baidu.com/wp/login/sec?type=2&v=" + Common.GetTimeStamp() + "0000";
            string postData = "vcode=" + vcode + "&clientfrom=native&tpl=tb&login_share_strategy=choice&client=android&adapter=3&t=" + Common.GetTimeStamp() + "0000&act=bind_mobile&loginLink=0&smsLoginLink=1&lPFastRegLink=0&fastRegLink=1&lPlayout=0&loginInitType=0&lang=zh-cn&regLink=1&action=login&loginmerge=1&isphone=0&dialogVerifyCode=&dialogVcodestr=&dialogVcodesign=&gid=660BDF6-30E5-4A83-8EAC-F0B4752E1C4B&showtype=" + type + "&lstr=" + RFC3986Encoder.UrlEncode(lstr) + "&ltoken=" + ltoken;
            string data = Common.HttpPost(url: url, postDataStr: postData, referer: this._referrer);
            dynamic jsonObj = Common.JsonStr2Obj(data);
            if (Common.IsPropertyExist(jsonObj, "errInfo") && jsonObj.errInfo.no == "0")
            {
                if (!string.IsNullOrEmpty(jsonObj.data.loginProxy.ToString()))
                {
                    data = Common.HttpGet(url: jsonObj.data.loginProxy.ToString(), referer: this._referrer);
                    jsonObj = Common.JsonStr2Obj(data);
                }
                data = jsonObj.data.xml;
                string user = Regex.Match(data, @"<uname>(.*?)</uname>").Groups[1].Value;
                string uid = Regex.Match(data, @"<uid>(.*?)</uid>").Groups[1].Value;
                string face = Regex.Match(data, @"<portrait>(.*?)</portrait>").Groups[1].Value;
                string displayname = Regex.Match(data, @"<displayname>(.*?)</displayname>").Groups[1].Value;
                string bduss = Regex.Match(data, @"<bduss>(.*?)</bduss>").Groups[1].Value;
                string ptoken = Regex.Match(data, @"<ptoken>(.*?)</ptoken>").Groups[1].Value;
                string stoken = Regex.Match(data, @"<stoken>(.*?)</stoken>").Groups[1].Value;
                return new { code = 0, uid = uid, user = user, displayname = displayname, face = face, bduss = bduss, ptoken = ptoken, stoken = stoken };
            }
            else if (Common.IsPropertyExist(jsonObj, "errInfo"))
            {
                return new { code = jsonObj.errInfo.no, msg = jsonObj.errInfo.msg };
            }
            else
            {
                return new { code = -1, msg = "登录失败，原因未知" };
            }
        }
        #endregion

        #region 检测是否需要验证码
        /// <summary>
        /// 检测是否需要验证码
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public dynamic CheckVc(string user)
        {
            if (string.IsNullOrEmpty(user)) return new { saveOK = -1, msg = "请先输入用户名" };
            string url = "https://wappass.baidu.com/wp/api/login/check?tt=" + Common.GetTimeStamp() + "9117&username=" + user + "&countrycode=&clientfrom=wap&sub_source=leadsetpwd&tpl=tb";
            string data = Common.HttpGet(url: url, referer: this._referrer);
            dynamic jsonObj = Common.JsonStr2Obj(data);
            if (Common.IsPropertyExist(jsonObj, "errInfo") && jsonObj.errInfo.no == "0" && string.IsNullOrEmpty(jsonObj.data.codeString.ToString()))
            {
                return new { code = 0 };
            }
            else if (Common.IsPropertyExist(jsonObj, "errInfo") && jsonObj.errInfo.no == "0")
            {
                return new { code = 1, vcodestr = jsonObj.data.codeString };
            }
            else
            {
                return new { code = jsonObj.errInfo.no, msg = jsonObj.errInfo.msg };
            }
        }
        #endregion

        #region 手机验证码登录，获取手机号是否存在
        /// <summary>
        /// 手机验证码登录，获取手机号是否存在
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public dynamic GetPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return new { saveOK = -1, msg = "请先输入手机号" };
            if (phone.Length != 11) return new { saveOK = -1, msg = "请输入正确的手机号" };
            string phone2 = "";
            for (int i = 0; i < 11; i++)
            {
                phone2 += phone[i];
                if (i == 2 || i == 6) phone2 += "+";
            }
            string url = "https://wappass.baidu.com/wp/api/security/getphonestatus?v=" + Common.GetTimeStamp() + "0000";
            string postData = "mobilenum=" + phone2 + "&clientfrom=native&tpl=tb&login_share_strategy=choice&client=android&adapter=3&t=" + Common.GetTimeStamp() + "0000&act=bind_mobile&loginLink=0&smsLoginLink=1&lPFastRegLink=0&fastRegLink=1&lPlayout=0&lang=zh-cn&regLink=1&action=login&loginmerge=1&isphone=0&dialogVerifyCode=&dialogVcodestr=&dialogVcodesign=&gid=E528690-4ADF-47A5-BA87-1FD76D2583EA&agreement=1&vcodesign=&vcodestr=&sms=1&username=" + phone + "&countrycode=";
            string data = Common.HttpPost(url: url, postDataStr: postData, referer: this._referrer);
            dynamic jsonObj = Common.JsonStr2Obj(data);
            if (Common.IsPropertyExist(jsonObj, "errInfo") && jsonObj.errInfo.no == "0")
            {
                return new { code = 0, msg = jsonObj.errInfo.msg };
            }
            else
            {
                return new { code = jsonObj.errInfo.no, msg = jsonObj.errInfo.msg };
            }
        }
        #endregion

        #region 手机验证码登录，发送验证码
        /// <summary>
        /// 手机验证码登录，发送验证码
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="vCode"></param>
        /// <param name="vCodeStr"></param>
        /// <param name="vCodeSign"></param>
        /// <returns></returns>
        public dynamic SendSms(string phone, string vCode = null, string vCodeStr = null, string vCodeSign = null)
        {
            if (string.IsNullOrEmpty(phone)) return new { saveOK = -1, msg = "请先输入手机号" };
            if (phone.Length != 11) return new { saveOK = -1, msg = "请输入正确的手机号" };
            if (vCode == null) vCode = "";
            if (vCodeStr == null) vCodeStr = "";
            if (vCodeSign == null) vCodeSign = "";
            string url = "https://wappass.baidu.com/wp/api/login/sms?v=" + Common.GetTimeStamp() + "0000";
            string postData = "username=" + phone + "&tpl=tb&clientfrom=native&countrycode=&gid=E528690-4ADF-47A5-BA87-1FD76D2583EA&dialogVerifyCode=" + (vCode == null || vCode == "null" ? "" : vCode) + "&vcodesign=" + (vCodeSign == null || vCodeSign == "null" ? "" : vCodeSign) + "&vcodestr=" + (vCodeStr == null || vCodeStr == "null" ? "" : vCodeStr);
            string data = Common.HttpPost(url: url, postDataStr: postData, referer: this._referrer);
            dynamic jsonObj = Common.JsonStr2Obj(data);
            if (Common.IsPropertyExist(jsonObj, "errInfo") && jsonObj.errInfo.no == "0")
            {
                return new { code = 0, msg = jsonObj.errInfo.msg };
            }
            else if (jsonObj.errInfo.no == "50020")
            {
                return new { code = jsonObj.errInfo.no, msg = jsonObj.errInfo.msg, vcodestr = jsonObj.data.vcodestr, vcodesign = jsonObj.data.vcodesign, };
            }
            else
            {
                return new { code = jsonObj.errInfo.no, msg = jsonObj.errInfo.msg };
            }
        }
        #endregion

        #region 手机验证码登录操作
        /// <summary>
        /// 手机验证码登录操作
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="smsVc"></param>
        /// <returns></returns>
        public dynamic Login3(string phone, string smsVc)
        {
            if (string.IsNullOrEmpty(phone)) return new { code = -1, msg = "手机号不能为空" };
            if (string.IsNullOrEmpty(smsVc)) return new { code = -1, msg = "验证码不能为空" };

            string url = "https://wappass.baidu.com/wp/api/login?v=" + Common.GetTimeStamp() + "0000";
            string postData = "smsvc=" + smsVc + "&clientfrom=native&tpl=tb&login_share_strategy=choice&client=android&adapter=3&t=" + Common.GetTimeStamp() + "0000&act=bind_mobile&loginLink=0&smsLoginLink=1&lPFastRegLink=0&fastRegLink=1&lPlayout=0&lang=zh-cn&regLink=1&action=login&loginmerge=&isphone=0&dialogVerifyCode=&dialogVcodestr=&dialogVcodesign=&gid=E528690-4ADF-47A5-BA87-1FD76D2583EA&agreement=1&vcodesign=&vcodestr=&smsverify=1&sms=1&mobilenum=" + phone + "&username=" + phone + "&countrycode=&passAppHash=&passAppVersion=";
            string data = Common.HttpPost(url: url, postDataStr: postData, referer: this._referrer);
            dynamic jsonObj = Common.JsonStr2Obj(data);
            if (Common.IsPropertyExist(jsonObj, "errInfo") && jsonObj.errInfo.no == "0")
            {
                if (!string.IsNullOrEmpty(jsonObj.data.loginProxy.ToString()))
                {
                    data = Common.HttpGet(url: jsonObj.data.loginProxy, referer: this._referrer);
                }
                data = jsonObj.data.xml;
                string user = Regex.Match(data, @"<uname>(.*?)</uname>").Groups[1].Value;
                string uid = Regex.Match(data, @"<uid>(.*?)</uid>").Groups[1].Value;
                string face = Regex.Match(data, @"<portrait>(.*?)</portrait>").Groups[1].Value;
                string displayname = Regex.Match(data, @"<displayname>(.*?)</displayname>").Groups[1].Value;
                string bduss = Regex.Match(data, @"<bduss>(.*?)</bduss>").Groups[1].Value;
                string ptoken = Regex.Match(data, @"<ptoken>(.*?)</ptoken>").Groups[1].Value;
                string stoken = Regex.Match(data, @"<stoken>(.*?)</stoken>").Groups[1].Value;
                return new { code = 0, uid = uid, user = user, displayname = displayname, face = face, bduss = bduss, ptoken = ptoken, stoken = stoken };
            }
            else if (Common.IsPropertyExist(jsonObj, "errInfo"))
            {
                return new { code = jsonObj.errInfo.no, msg = jsonObj.errInfo.msg };
            }
            else
            {
                return new { code = -1, msg = "登录失败，原因未知" };
            }
        }
        #endregion

        #region 获取扫码登录二维码
        /// <summary>
        /// 获取扫码登录二维码
        /// </summary>
        /// <returns></returns>
        public dynamic GetQrCode()
        {
            string url = "https://passport.baidu.com/v2/api/getqrcode?lp=pc&gid=07D9D20-91EB-43D8-8553-16A98A0B24AA&apiver=v3&tt=" + Common.GetTimeStamp() + "0000&callback=callback";
            string data = Common.HttpGet(url: url, referer: "https://passport.baidu.com/v2/?login");
            string callback = Regex.Match(data, @"callback\((.*?)\)").Groups[1].Value;
            dynamic jsonObj = Common.JsonStr2Obj(callback);
            if (Common.IsPropertyExist(jsonObj, "errno") && jsonObj.errno == "0")
            {
                return new { code = 0, imgurl = jsonObj.imgurl, sign = jsonObj.sign, link = "https://wappass.baidu.com/wp/?qrlogin&t=" + Common.GetTimeStamp() + "&error=0&sign=" + jsonObj.sign + "&cmd=login&lp=pc&tpl=&uaonly=" };
            }
            else
            {
                return new { code = jsonObj.errno, msg = "获取二维码失败" };
            }
        }
        #endregion

        #region 扫码登录操作
        /// <summary>
        /// 扫码登录操作
        /// </summary>
        /// <param name="sign"></param>
        /// <returns></returns>
        public dynamic QrLogin(string sign)
        {
            if (string.IsNullOrEmpty(sign)) return new { code = -1, msg = "sign不能为空" };
            string url = "https://passport.baidu.com/channel/unicast?channel_id=" + sign + "&tpl=pp&gid=07D9D20-91EB-43D8-8553-16A98A0B24AA&apiver=v3&tt=" + Common.GetTimeStamp() + "0000&callback=callback";
            // BUG：当展示二维码后未进行登录操作，此时因为前端js不断请求二维码登录，然后到此处请求超时，导致返回500，临时解决：取消HttpGet内的抛异常
            string data = Common.HttpGet(url: url, referer: "https://passport.baidu.com/v2/?login");
            string callback = Regex.Match(data, @"callback\((.*?)\)").Groups[1].Value;
            dynamic jsonObj = Common.JsonStr2Obj(callback);
            if (Common.IsPropertyExist(jsonObj, "errno") && jsonObj.errno == "0")
            {
                jsonObj = Common.JsonStr2Obj(jsonObj.channel_v.ToString());
                StringBuilder sbResHeadersData = new StringBuilder();
                data = Common.HttpGet(url: "https://passport.baidu.com/v2/api/bdusslogin?bduss=" + jsonObj.v + "&u=https%3A%2F%2Fpassport.baidu.com%2F&qrcode=1&tpl=pp&apiver=v3&tt=" + Common.GetTimeStamp() + "0000&callback=callback", referer: "https://passport.baidu.com/v2/?login", responseHeadersSb: sbResHeadersData);
                string resHeadersData = sbResHeadersData.ToString();

                #region 调试
                StringBuilder sbDebug = new StringBuilder();
                sbDebug.AppendLine("-------" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "------------start--------------------");
                sbDebug.AppendLine(resHeadersData + Environment.NewLine + Environment.NewLine + data);
                sbDebug.AppendLine("--------------------end-------------------" + Environment.NewLine);

                System.Diagnostics.Debug.WriteLine(sbDebug.ToString());
                System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "/App_Data/resData.txt", sbDebug.ToString(), Encoding.UTF8);
                #endregion

                callback = Regex.Match(data, @"callback\((.*?)\)").Groups[1].Value;
                jsonObj = Common.JsonStr2Obj(callback);
                if (Common.IsPropertyExist(jsonObj, "errInfo") && jsonObj.errInfo.no == "0")
                {
                    data = data.Replace("=deleted", "");
                    // 注意：bduss, ptoken, stoken 在响应头中
                    string bduss = Regex.Match(resHeadersData, @"BDUSS=(.*?);").Groups[1].Value;
                    string ptoken = Regex.Match(resHeadersData, @"PTOKEN=(.*?);").Groups[1].Value;
                    string stoken = Regex.Match(resHeadersData, @"STOKEN=(.*?);").Groups[1].Value;
                    string userId = GetUserId(jsonObj.data.userName.ToString());
                    return new { code = 0, uid = userId, user = jsonObj.data.userName, displayname = jsonObj.data.displayname, mail = jsonObj.data.mail, phone = jsonObj.data.phoneNumber, bduss = bduss, ptoken = ptoken, stoken = stoken };
                }
                else if (Common.IsPropertyExist(jsonObj, "errInfo"))
                {
                    return new { code = jsonObj.errInfo.no, msg = jsonObj.errInfo.msg };
                }
                else
                {
                    return new { code = -1, msg = "登录失败，原因未知" };
                }
            }
            else if (Common.IsPropertyExist(jsonObj, "errno"))
            {
                return new { code = jsonObj.errno };
            }
            else
            {
                return new { code = -1, msg = "登录失败，原因未知" };
            }
        }
        #endregion

        #region 获取用户ID
        /// <summary>
        /// 获取用户ID
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string GetUserId(string uname)
        {
            string ua = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
            string data = Common.HttpGet(url: "http://tieba.baidu.com/home/get/panel?ie=utf-8&un=" + HttpUtility.UrlEncode(uname), ua: ua);
            dynamic jsonObj = Common.JsonStr2Obj(data);
            string userId = jsonObj.data.id;
            return userId;
        }
        #endregion
    }
}