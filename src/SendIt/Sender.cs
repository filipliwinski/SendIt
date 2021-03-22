// MIT License
//  
//  Copyright (c) 2021 Filip Liwiński
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
//

using System;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendIt
{
    /// <summary>
    /// Allows to send e-mail messages using Simple Mail Transfer Protocol with support for message queuing.
    /// </summary>
    public class Sender : ISender
    {
        private readonly string testMessage;
        private readonly Recipient[] testRecipients;
        private readonly bool testMode;
        private readonly bool dryRun;
        private readonly string mailHeader;
        private readonly string mailFooter;

        private uint groupIndex = 0;
        private uint groupSize = 0;
        private uint individualTimeGap = 0;
        private uint groupTimeGap = 0;
        private DateTime lastSent;

        /// <summary>
        /// Specify whether the System.Net.Mail.SmtpClient uses Secure Sockets Layer (SSL) to encrypt the connection.
        /// </summary>
        public bool UseSSL { get; private set; }

        /// <summary>
        /// Text included in every message sent in test mode.
        /// </summary>
        public string TestMessage { get { return testMessage; } }

        /// <summary>
        /// Determines whether test mode is enabled. In test mode, all messages are sent to the defined test recipients, 
        /// and each message is accompanied by a header with the string defined in the TestMessage property. 
        /// </summary>
        public bool TestMode { get { return testMode; } }

        /// <summary>
        ///  Determines whether the dry run mode is enabled. In dry run mode, no message is sent.
        /// </summary>
        public bool DryRun { get { return dryRun; } }

        /// <summary>
        /// Date of the last message sent (exactly the moment just before it was sent).
        /// </summary>
        public DateTime LastSent { get { return lastSent; } }

        /// <summary>
        /// Configuration of the System.Net.Mail.SmtpClient.
        /// </summary>
        public SmtpConfig SmtpConfig { get; private set; }

        /// <summary>
        /// Number of messages to be sent in one group.
        /// </summary>
        public uint GroupSize
        {
            get { return groupSize; }
            set { groupSize = value; }
        }

        /// <summary>
        /// Time gap between message groups.
        /// </summary>
        public uint GroupTimeGap
        {
            get { return groupTimeGap; }
            set { groupTimeGap = value; }
        }

        /// <summary>
        /// Time gap between messages in a message group.
        /// </summary>
        public uint IndividualTimeGap
        {
            get { return individualTimeGap; }
            set { individualTimeGap = value; }
        }

        /// <summary>
        /// Creates a new instance of the SendIt.Sender class.
        /// </summary>
        /// <param name="testRecipients">Recipients to be used in test mode.</param>
        /// <param name="testMode">Determines whether test mode is enabled.</param>
        /// <param name="dryRun">Determines whether dry run mode is enabled.</param>
        /// <param name="useSSL">Specify whether the System.Net.Mail.SmtpClient uses Secure Sockets Layer (SSL) to encrypt the connection.</param>
        /// <param name="mailHeader">Text included at the begining of each e-mail message.</param>
        /// <param name="mailFooter">Text included at the end of each e-mail message.</param>
        /// <param name="testMessage">Text included in every message sent in test mode.</param>
        public Sender(Recipient[] testRecipients, bool testMode = true, bool dryRun = false, bool useSSL = true, string mailHeader = "", string mailFooter = "", string testMessage = "TEST MESSAGE")
        {
            this.dryRun = dryRun;
            this.mailHeader = mailHeader;
            this.mailFooter = mailFooter;
            this.testRecipients = testRecipients;
            this.testMessage = $"* * * {testMessage} * * *";
            this.testMode = testMode;
            UseSSL = useSSL;
        }

        public void Configure(SmtpConfig smtpConfig)
        {
            SmtpConfig = smtpConfig;
        }

        private MailMessage Compose(Recipient[] recipients, string subject, string content, IEmailable[] sections, Attachment[] attachments, bool isHtml, Encoding encoding)
        {
            if (testMode)
            {
                if (isHtml)
                {
                    content = $"<p style=\"color: red\">{testMessage}</p><br />" + content;
                }
                else
                {
                    content = $"{testMessage}\n\r" + content;
                }
            }

            if (sections != null)
            {
                for (int i = 0; i < sections.Length; i++)
                {
                    if (isHtml)
                    {
                        content += sections[i].ToHtml();
                    }
                    else
                    {
                        content += sections[i].ToText();
                    }
                }
            }

            var message = new MailMessage
            {
                Body = content,
                BodyEncoding = encoding ?? Encoding.UTF8,
                IsBodyHtml = isHtml,
                Subject = subject,
                SubjectEncoding = encoding ?? Encoding.UTF8,
            };

            for (int i = 0; i < recipients.Length; i++)
            {
                switch (recipients[i].Type)
                {
                    case RecipientType.To:
                        message.To.Add(recipients[i].ToString());
                        break;
                    case RecipientType.Cc:
                        message.CC.Add(recipients[i].ToString());
                        break;
                    case RecipientType.Bcc:
                        message.Bcc.Add(recipients[i].ToString());
                        break;
                    case RecipientType.ReplyTo:
                        message.ReplyToList.Add(recipients[i].ToString());
                        break;
                    default:
                        throw new Exception($"Unsupported recipient type: {recipients[i].Type}");
                }
            }

            if (attachments != null)
            {
                for (int i = 0; i < attachments.Length; i++)
                {
                    message.Attachments.Add(attachments[i]);
                }
            }

            return message;
        }

        public async Task<string> SendMailAsync(Recipient[] recipients, string subject, string content = "", IEmailable[] sections = null, Attachment[] attachments = null, bool isHtml = true, Encoding encoding = null)
        {
            var log = "";
            uint timeGap = 0;

            if (TestMode)
            {
                recipients = testRecipients;
            }

            using (var message = Compose(recipients, subject, content, sections, attachments, isHtml, encoding))
            {
                if (groupIndex == groupSize)
                {
                    groupIndex = 0;
                    timeGap = GetTimeGap(groupTimeGap);
                }
                else
                {
                    timeGap = GetTimeGap(individualTimeGap);
                }

                Thread.Sleep((int)timeGap);

                lastSent = DateTime.Now;

                if (DryRun)
                {
                    log += "[DRY RUN] ";
                }
                else
                {
                    await SendMailAsync(message);
                }
                groupIndex++;

                log += "Notification sent: ";

                for (int i = 0; i < recipients.Length; i++)
                {
                    log += $"{recipients[i].Type}: {recipients[i].Email};";
                }

                if (timeGap > 0)
                {
                    log += $" [{timeGap}ms time gap]";
                }
            }

            return log;
        }

        private uint GetTimeGap(uint configuredDelay)
        {
            if (lastSent == null || configuredDelay == 0)
            {
                return 0;
            }

            var timeDifference = (uint)(DateTime.Now - lastSent).TotalMilliseconds;

            if (timeDifference < configuredDelay)
            {
                return configuredDelay - timeDifference;
            }

            return 0;
        }

        private async Task SendMailAsync(MailMessage message)
        {
            using (var mailClient = new SmtpClient())
            {
                if (SmtpConfig != null)
                {
                    mailClient.Host = SmtpConfig.Host;
                    mailClient.Port = SmtpConfig.Port;
                    message.From = SmtpConfig.From;
                    mailClient.Credentials = SmtpConfig.Credentials;
                }

                mailClient.EnableSsl = UseSSL;

                await mailClient.SendMailAsync(message);
            }
        }
    }
}
