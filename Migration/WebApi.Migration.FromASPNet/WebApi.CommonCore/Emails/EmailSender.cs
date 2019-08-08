using System;
using System.Collections.Generic;
using log4net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using WebApi.CommonCore.KeyVault;

namespace WebApi.CommonCore.Emails
{
    public class EmailSender
    {
        private readonly ILog Logger;
        private readonly IConfiguration _configuration;
        private readonly SendGridClient sendGridClient;
        private static Lazy<EmailSender> _lazy = new Lazy<EmailSender>(() => new EmailSender());
        public static EmailSender Current => _lazy.Value;

        //TODO: test lai
        private EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private EmailSender()
        {
            sendGridClient = new SendGridClient(_configuration["SendGridApiKey"]);
            Logger = LogManager.GetLogger(GetType());
        }
        public void Send(EmailItem emailItem)
        {
            try
            {
                SendGridMessage message = new SendGridMessage();
                emailItem.To.ForEach((cc) => message.AddTo(new EmailAddress(cc)));
                emailItem.CC.ForEach((cc) => message.AddCc(new EmailAddress(cc)));
                emailItem.BCC.ForEach((bcc) => message.AddBcc(new EmailAddress(bcc)));

                message.Subject = (emailItem.Subject);
                message.HtmlContent = emailItem.Content;

                var from = !string.IsNullOrEmpty(emailItem.From) ? emailItem.From : "no-reply@acomsolutions.com";

                message.SetFrom(new EmailAddress(from));

                foreach (var pair in emailItem.Attachments)
                {
                    message.AddAttachment(pair.Key, pair.Value);
                }

                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                if (emailItem.Priority)
                {
                    dictionary.Add("Priority", "Urgent");
                    dictionary.Add("Importance", "high");
                    message.AddHeaders(dictionary);
                }
                var result = this.sendGridClient.SendEmailAsync(message).GetAwaiter().GetResult();
                Logger.Info($"StatusCode={result.StatusCode};Body={JsonConvert.SerializeObject(result.DeserializeResponseBodyAsync(result.Body))}");
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }
    }
    public class EmailItem
    {
        public string From { get; set; }//Option
        public List<string> To { get; private set; } = new List<string>();
        public string Subject { get; set; }//Option
        public string Content { get; set; }//Required
        public bool HtmlFormat { get; set; } //Option        
        public string Replyto { get; set; }//Option        
        public List<string> CC { get; private set; } = new List<string>();
        public List<string> BCC { get; private set; } = new List<string>();
        public Dictionary<string, string> Attachments { get; private set; } = new Dictionary<string, string>();
        public bool Priority { get; set; }

        public void AddAttachment(byte[] attach, string fileName = "")
        {
            Attachments = Attachments ?? new Dictionary<string, string>();
            if (attach?.Length > 0)
                Attachments.Add(fileName, Convert.ToBase64String(attach));
        }
        public void AddTo(ICollection<string> to)
        {
            to = to ?? new List<string>();
            To.AddRange(to);
        }
        public void AddCC(ICollection<string> cc)
        {
            cc = cc ?? new List<string>();
            CC.AddRange(cc);
        }
        public void AddBCC(ICollection<string> bcc)
        {
            bcc = bcc ?? new List<string>();
            BCC.AddRange(bcc);
        }
    }
}
