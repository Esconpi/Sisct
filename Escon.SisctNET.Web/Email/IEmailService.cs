using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Web.Email
{
    public interface IEmailService
    {
        Tuple<bool, string> Send(EmailMessage emailMessage, string[] attachment);

        List<EmailMessage> ReceiveEmail(int maxCount = 10);
    }
}
