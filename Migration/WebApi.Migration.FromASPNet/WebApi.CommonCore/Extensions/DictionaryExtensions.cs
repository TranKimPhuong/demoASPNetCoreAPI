
using Microsoft.Azure.ServiceBus;

namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        public static Message ToBrokeredMessage(this Dictionary<string, string> properties)
        {
            var message = new Message();
            foreach (var pair in properties)
                message.UserProperties[pair.Key] = pair.Value;
            return message;
        }
    }
}
