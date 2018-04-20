using System.Collections.Generic;
using System.Configuration;
using BlockChainNode.Net;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses;
using Nancy.Serialization.JsonNet;
using Newtonsoft.Json.Linq;

namespace BlockChainNode.Modules
{
    public class CommunicationModule : NancyModule
    {
        public CommunicationModule() : base("/comm")
        {
            Get["/info"] = parameters => GetNodeInfo();
            Post["/register"] = parameters => RegisterNewNode();
            Post["/sync"] = parameters => Sync();

            // TODO rebalancing chains
        }

        public JsonResponse Sync()
        {
            NodeBalance.PerformOnAllNodes(NodeBalance.RebalanceNode);

            return new JsonResponse(
                new Dictionary<string, string>
                {
                    {"CurrentNodes", NodeBalance.NodeSet.Count.ToString()}
                }, new JsonNetSerializer()) {StatusCode = HttpStatusCode.OK};
        }

        private static JsonResponse GetNodeInfo()
        {
            return new JsonResponse(
                new Dictionary<string, string>
                {
                    {"HostIp", ConfigurationManager.AppSettings["host"]},
                    {"CurrentNodes", NodeBalance.NodeSet.Count.ToString()}
                }, new JsonNetSerializer()) {StatusCode = HttpStatusCode.OK};
        }

        private JsonResponse RegisterNewNode()
        {
            var jsonString = Request.Body.AsString();
            var jsonObject = JObject.Parse(jsonString);
            var host = (string) jsonObject["host"];

            if (string.IsNullOrEmpty(host))
            {
                return new JsonResponse(null, new JsonNetSerializer())
                {
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            NodeBalance.NodeSet.Add(host);
            return new JsonResponse(NodeBalance.NodeSet, new JsonNetSerializer())
            {
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}