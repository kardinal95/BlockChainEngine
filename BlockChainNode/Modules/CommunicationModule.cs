using System.Collections.Generic;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses;
using Newtonsoft.Json.Linq;

namespace BlockChainNode.Modules
{
    public class CommunicationModule : NancyModule
    {
        public static SortedSet<string> NodeSet;

        public CommunicationModule() : base("/comm")
        {
            Post["/register"] = parameters => RegisterNewNode();
            // TODO rebalancing chains
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
                StatusCode = HttpStatusCode.Accepted
            };
        }
    }
}