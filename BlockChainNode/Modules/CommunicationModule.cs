using System.Collections.Generic;
using BlockChainNode.Net;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses;
using Newtonsoft.Json.Linq;

namespace BlockChainNode.Modules
{
    public class CommunicationModule : NancyModule
    {
        public static HashSet<string> NodeSet;

        public CommunicationModule() : base("/comm")
        {
            Get["/info"] = parameters => GetNodeInfo();
            Post["/register"] = parameters => RegisterNewNode();
            Post["/deregister"] = parameters => DeregisterNode();

            // TODO rebalancing chains
        }

        public JsonResponse GetNodeInfo()
        {
            return new JsonResponse(null, new DefaultJsonSerializer())
            {
                StatusCode = HttpStatusCode.OK
            };
        }

        public JsonResponse DeregisterNode()
        {
            var jsonString = Request.Body.AsString();
            var jsonObject = JObject.Parse(jsonString);
            var host = (string) jsonObject["host"];

            if (string.IsNullOrEmpty(host) || !NodeSet.Contains(host) ||
                NodeBalance.NodeOnline(host))
            {
                return new JsonResponse(null, new DefaultJsonSerializer())
                {
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            NodeSet.Remove(host);
            return new JsonResponse(null, new DefaultJsonSerializer())
            {
                StatusCode = HttpStatusCode.OK
            };
        }

        public JsonResponse RegisterNewNode()
        {
            var jsonString = Request.Body.AsString();
            var jsonObject = JObject.Parse(jsonString);
            var host = (string) jsonObject["host"];

            if (string.IsNullOrEmpty(host))
            {
                return new JsonResponse(null, new DefaultJsonSerializer())
                {
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            NodeSet.Add(host);
            return new JsonResponse(NodeSet, new DefaultJsonSerializer())
            {
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}