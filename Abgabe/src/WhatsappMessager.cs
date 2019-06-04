using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace WhatsappMessager
{
    public static class WhatsappSender
    {
        private static string configPath = @"C:/Users/GeraldSpenlingwimmer/Desktop/config/twilioconfig.json";

        private class Config
        {
            public string Sid { get; set; }
            public string AuthToken { get; set; }
            public string TwilioPhone { get; set; }
            public string TargetPhone { get; set; }

        }
        private static string twilioPhone = "";
        private static string targetPhone = "";

        static WhatsappSender()
        {
            using (StreamReader r = new StreamReader(configPath))
            {
                string jsonString = r.ReadToEnd();
                Config config = JsonConvert.DeserializeObject<Config>(jsonString);
                TwilioClient.Init(config.Sid, config.AuthToken);
                twilioPhone = config.TwilioPhone;
                targetPhone = config.TargetPhone;
            }
        }

        public static void Send(string messageBody)
        {
            Task.Factory.StartNew(() =>
            {
                var message = MessageResource.Create(
                from: new Twilio.Types.PhoneNumber(twilioPhone),
                body: messageBody,
                to: new Twilio.Types.PhoneNumber(targetPhone)
                );
            });
        }
    }
}
