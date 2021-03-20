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

using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SendIt
{
    public interface ISender
    {
        bool DryRun { get; }
        SmtpConfig SmtpConfig { get; }
        bool TestMode { get; }
        bool UseSSL { get; }

        /// <summary>
        /// Allows to set up custom SMTP mail account for sending messages.
        /// </summary>
        /// <param name="smtpConfig">Custom SMTP account settings.</param>
        void Configure(SmtpConfig smtpConfig);

        /// <summary>
        /// Creates and sends the message.
        /// </summary>
        /// <param name="recipients">Recipients of the message. Recipient TO is required.</param>
        /// <param name="subject">Message subject.</param>
        /// <param name="content">Message content.</param>
        /// <param name="sections">Message sections.</param>
        /// <param name="attachments">Message attachments.</param>
        /// <param name="isHtml">Send message in HTML format.</param>
        /// <param name="encoding">Message encoding (default is UTF-8).</param>
        /// <returns>Log message.</returns>
        Task<string> SendMailAsync(Recipient[] recipients, string subject, string content = "", IEmailable[] sections = null, Attachment[] attachments = null, bool isHtml = true, Encoding encoding = null);
    }
}