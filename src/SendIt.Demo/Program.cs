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
using System.Net;
using System.Threading.Tasks;

namespace SendIt.Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // (Optional) Configure the SMTP account details (by default, the details configured in <mailSettings> in the config file are used).
            var smtpConfig = new SmtpConfig("<smtp host address>", 587, new NetworkCredential("<username>", "<password>"), "<from e-mail address>");

            Recipient[] testRecipients = { new Recipient("test@example.com") };

            // Create the Sender object.
            var sender = new Sender(testRecipients, testMode: true, dryRun: true);

            // Configure the Sender object with custom SMTP account configuration.
            sender.Configure(smtpConfig);

            Recipient[] recipients = { new Recipient("example@example.com") };

            // Send two messages 10 times.
            for (int i = 0; i < 10; i++)
            {
                // Send an e-mail with content in text format.
                Console.WriteLine(await sender.SendMailAsync(recipients, "Hello World!", "Sent in text format using SendIt.", isHtml: false));

                IEmailable[] exampleObjects = {
                    new ExampleClass { PropertyA = "A1", PropertyB = "B1" },
                    new ExampleClass { PropertyA = "A2", PropertyB = "B2" },
                    new ExampleClass { PropertyA = "A3", PropertyB = "B3" }
                };

                // Send an e-mail with content and IEmailable objects in HTML format.
                Console.WriteLine(await sender.SendMailAsync(recipients, "Hello World!", "<h3>Sent in HTML format using SendIt.</h3>", exampleObjects));
            }
        }
    }
}
