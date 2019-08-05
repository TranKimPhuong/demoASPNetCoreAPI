using System;
using System.Collections.Generic;
using Microsoft.Azure.ServiceBus;

using BrokeredMessageProperties = System.Collections.Generic.Dictionary<string, string>;

namespace WebApi.CommonCore.Helper
{
    public static class MessageHelper
    {
        public static string ERR_APP = "An error has occurred and logged on the server. Please try again later.";

        public static void SendMessageToServiceBus(string nameSpace, string issuerName, string issuerKey, string queueName, Message message)
        {
            try
            {
                var sbConnectionString = string.Format("Endpoint=sb://{0}/;SharedAccessKeyName={1};SharedAccessKey={2}", nameSpace, issuerName, issuerKey);
                QueueClient client = new QueueClient(sbConnectionString, queueName);
                client.SendAsync(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SendMessageToServiceBus(string serviceBusConnectionString, string queueName,Message message)
        {
            try
            {
                QueueClient client = new QueueClient(serviceBusConnectionString, queueName);

                client.SendAsync(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void SendMessageToServiceBus(string serviceBusConnectionString, string queueName, BrokeredMessageProperties messageProperties)
        {
            try
            {
                var message = messageProperties.ToBrokeredMessage();

                QueueClient client = new QueueClient(serviceBusConnectionString, queueName);

                client.SendAsync(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void SendMessageToServiceBus(string queueName, BrokeredMessageProperties messageProperties)
        {
            try
            {
                var message = messageProperties.ToBrokeredMessage();

                QueueClient client = new QueueClient(KeyVault.Vault.Current.ServiceBusConnectionString, queueName);

                client.SendAsync(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
