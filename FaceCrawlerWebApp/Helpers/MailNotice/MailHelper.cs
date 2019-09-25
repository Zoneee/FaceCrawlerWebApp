using System;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Common.Message
{
    /// <summary>
    /// 发送消息通知类
    /// </summary>
    public class MailHelper
    {
        /// <summary>
        /// 收件人邮箱
        /// </summary>
        private string _alarmMailAccounts;
        /// <summary>
        /// 邮箱帐号
        /// </summary>
        private string _mailAccount;
        /// <summary>
        /// 邮箱密码
        /// </summary>
        private string _mailPwd;
        /// <summary>
        /// 邮箱服务器
        /// </summary>
        private string _stmpHost;
        /// <summary>
        /// 邮箱服务器端口
        /// </summary>
        private int _stmpPort;

        /// <param name="alarmMailAccounts">收件人邮箱。用","或"，"隔开</param>
        /// <param name="mailAccount">邮箱帐号</param>
        /// <param name="mailPwd">邮箱密码</param>
        /// <param name="stmpHost">邮箱服务器</param>
        /// <param name="stmpPort">邮箱服务器端口</param>
        public MailHelper(string alarmMailAccounts, string mailAccount, string mailPwd, string stmpHost, int stmpPort)
        {
            _alarmMailAccounts = alarmMailAccounts;
            _mailAccount = mailAccount;
            _mailPwd = mailPwd;
            _stmpHost = stmpHost;
            _stmpPort = stmpPort;
        }


        public async Task SendMail(string mailBody, string mailTitle = "监控服务通知")
        {
            await SendMail(mailBody, _alarmMailAccounts, mailTitle: mailTitle);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mailBody">邮件内容，格式如机器地址：127.0.0.1<br/>发生时间：2016-01-27 11:11:11<br/>消息内容：[mailBody]</param>
        /// <param name="mailAccountsReceive">接收者邮件列表，","或"，"隔开</param>
        /// <param name="timeout">邮件发送超时时间（默认60s）</param>
        /// <param name="mailTitle">邮件主题</param>
        /// <returns>成功则返回空字符串，否则返回失败原因</returns>
        private async Task SendMail(string mailBody, string mailAccountsReceive, int timeout = 60 * 1000, string mailTitle = "消息通知")
        {
            mailAccountsReceive = (mailAccountsReceive ?? string.Empty).Replace('，', ',').Trim(',', ' ');

            mailBody = MessageFormat.MailMessageFormat(mailBody).ToString();
            using (MailMessage myMail = new MailMessage())
            {
                myMail.From = new MailAddress(_mailAccount);
                myMail.To.Add(mailAccountsReceive);
                myMail.Subject = mailTitle;
                myMail.Body = mailBody;
                myMail.IsBodyHtml = true;
                myMail.Priority = MailPriority.High;
                myMail.SubjectEncoding = Encoding.UTF8;
                myMail.BodyEncoding = Encoding.UTF8;
                //设置SmtpClient参数
                SmtpClient SMTPClient = new SmtpClient(_stmpHost, _stmpPort);
                SMTPClient.EnableSsl = true;
                SMTPClient.Timeout = timeout;
                SMTPClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                SMTPClient.Credentials = new System.Net.NetworkCredential(_mailAccount, _mailPwd);//验证发件人身份的凭据
                await SMTPClient.SendMailAsync(myMail);
            }
        }
    }
}