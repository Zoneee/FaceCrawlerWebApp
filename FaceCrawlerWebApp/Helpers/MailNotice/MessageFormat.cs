using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.Message
{
    /// <summary>
    /// 消息格式化
    /// </summary>
    public class MessageFormat
    {
        private static readonly IPAddress address = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork) ?? new IPAddress(0L);
        /// <summary>
        /// 邮件信息格式化
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static StringBuilder MailMessageFormat(string message)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<br/>");
            builder.Append($"<b><font color=\"#003366\">机器地址：</font></b><font color=\"#ff0000\">{address.ToString()}</font>");
            builder.Append("<br/>");
            builder.Append($"<b><font color=\"#003366\">发生时间：</font></b><font color=\"#ff0000\">{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}</font>");
            builder.Append("<br/>");
            builder.Append($"<b><font color=\"#003366\">消息内容：</font></b><font color=\"#ff0000\">{message}</font>");
            builder.Append("<br/>");

            return builder;
        }

        /// <summary>
        /// 短信信息格式化
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string SMSMessageFormat(string message)
        {
            message = string.Concat(address.ToString(), ":", message);
            //相同内容有5min间隔限制且内容不能含有特殊字符
            message = Regex.Replace(message, "[.\\\\/#]", match =>
            {
                switch (match.Value)
                {
                    case ".": return "·";
                    case "\\":
                    case "//": return "$";
                    case "#": return "井";
                    default: return match.Value;
                }
            });
            return message;
        }
    }
}