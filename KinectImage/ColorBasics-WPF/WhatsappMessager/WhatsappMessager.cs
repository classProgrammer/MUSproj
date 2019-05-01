using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace WhatsappMessager
{
    public static class WhatsappSender
    {
        static string sid = "AC28eb5f09e9740e5e9782ba8d86e12133";
        static string authToken = "13b20f505448e873f1a4ba618af0745a";

        public static void Send(string person)
        {
            TwilioClient.Init(sid, authToken);

            string body = person != "" ? $"{person} is waiting at your door. Your code is 666." : "An unknown person is waiting at your door. Your code is 666.";

            var message = MessageResource.Create(
                from: new Twilio.Types.PhoneNumber("whatsapp:+14155238886"),
                body: body,
                to: new Twilio.Types.PhoneNumber("whatsapp:+4367761236459")
            );
            Console.WriteLine("message sent");
        }
    }
}
