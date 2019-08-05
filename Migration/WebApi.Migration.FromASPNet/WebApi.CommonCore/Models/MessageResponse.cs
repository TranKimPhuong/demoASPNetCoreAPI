using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace WebApi.CommonCore.Models
{
    public class MessageResponse
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int code { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> messages { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object data { get; set; }

        public MessageResponse()
        {
            this.messages = new List<string>();
        }

        public MessageResponse addMessage(string msg)
        {
            if (this.messages == null) this.messages = new List<string>();
            messages.Add(msg);
            return this;
        }

        public static MessageResponse ok(Object dataOK)
        {
            MessageResponse result = new MessageResponse();
            result.data = dataOK;
            result.code = (int)HttpStatusCode.OK;
            
            return result;
        }


        public static MessageResponse info(string msgBadRequest)
        {
            return info(new List<string>(new string[] { msgBadRequest }));
        }

        public static MessageResponse info(List<string> msgBadRequest)
        {
            MessageResponse result = new MessageResponse();
            result.messages = msgBadRequest;
            result.code = (int)HttpStatusCode.BadRequest;

            return result;
        }

        public static MessageResponse error(string msgError)
        {
            return error(new List<string>(new string[] { msgError }));
        }

        public static MessageResponse error(List<string> msgError)
        {
            MessageResponse result = new MessageResponse();
            result.messages = msgError;
            result.code = (int)HttpStatusCode.InternalServerError;

            return result;
        }
    }
}
