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

namespace SendIt
{
    public enum RecipientType
    {
        To,
        Cc,
        Bcc,
        ReplyTo
    }

    public class Recipient
    {
        public string Email { get; private set; }
        public string DisplayName { get; private set; }
        public RecipientType Type { get; private set; }

        public Recipient(string email, RecipientType type = RecipientType.To, string displayName = "")
        {
            email = email.ToLower();
            if (!EmailAdderssValidator.IsValid(email))
            {
                throw new FormatException($"Provided e-mail address is invalid (value: \"{email}\").");
            }
            if (email.Split('@').Length != 2)
            {
                throw new FormatException("Multiple e-mail addresses for a single recipient are not allowed.");
            }

            Email = email;
            DisplayName = displayName;
            Type = type;
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(DisplayName))
                return Email;
            return $"{DisplayName} <{Email}>";
        }
    }
}
