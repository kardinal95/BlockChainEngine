using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace BlockChainNode.Lib.Net
{
    public static class WebRequest
    {
        public static HttpWebResponse NewJsonPost(string target,
                                                  Dictionary<string, string> parameters)
        {
            var req = (HttpWebRequest) System.Net.WebRequest.Create(target);
            req.Method = "POST";
            req.ContentType = "text/json";

            using (var streamWriter = new StreamWriter(req.GetRequestStream()))
            {
                foreach (var item in parameters)
                {
                    var json = new JObject(new JProperty(item.Key, item.Value));
                    streamWriter.Write(json);
                }
            }

            var resp = (HttpWebResponse) req.GetResponse();
            return resp;
        }

        public static HttpWebResponse NewJsonGet(string target)
        {
            var req = (HttpWebRequest) System.Net.WebRequest.Create(target);
            req.Method = "GET";
            req.ContentType = "text/json";

            var resp = (HttpWebResponse) req.GetResponse();
            return resp;
        }

        public static string GetJsonResponseBody(HttpWebResponse response)
        {
            var resStream = response.GetResponseStream();
            var reader = new StreamReader(resStream ?? throw new InvalidOperationException());
            var respBody = reader.ReadToEnd();
            return respBody;
        }
    }
}