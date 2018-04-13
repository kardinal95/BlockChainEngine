using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace BlockChainNode.Net
{
    public static class NodeBalance
    {
        public static string RegisterThisNode(string targetIp, string nodeIp)
        {
            var req = (HttpWebRequest) WebRequest.Create($"{targetIp}/comm/register");
            req.Method = "POST";
            req.ContentType = "text/json";

            using (var streamWriter = new StreamWriter(req.GetRequestStream()))
            {
                var json = new JObject(new JProperty("host", nodeIp));
                streamWriter.Write(json);
            }

            var resp = (HttpWebResponse) req.GetResponse();
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException("Cannot connect with node network!");
            }

            var resStream = resp.GetResponseStream();
            var reader = new StreamReader(resStream ?? throw new InvalidOperationException());
            var respBody = reader.ReadToEnd();
            return respBody;
        }

        public static bool NodeOnline(string host)
        {
            var req = (HttpWebRequest) WebRequest.Create($"{host}/comm/info");
            req.Method = "GET";
            req.ContentType = "text/json";

            var resp = (HttpWebResponse) req.GetResponse();
            return resp.StatusCode == HttpStatusCode.OK;
        }
    }
}