using System.Collections.Generic;
using BlockChainNode.Lib.Net;
using BlockChainNode.Net;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses;
using Nancy.Serialization.JsonNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Common = BlockChainNode.Lib.Net.Common;

namespace BlockChainNode.Modules
{
    // ReSharper disable once UnusedMember.Global
    public class CommunicationModule : NancyModule
    {
        private static readonly JsonNetSerializer Serializer = new JsonNetSerializer();

        public CommunicationModule() : base("/comm")
        {
            Get["/info"] = parameters => GetNodeInfo();
            Post["/register"] = parameters => RegisterNewNode();
            Post["/sync"] = parameters => Sync();
        }

        private static JsonResponse Sync()
        {
            NodeBalance.PerformOnAllNodes(NodeBalance.RebalanceNode);

            var nodeResponse = new NodeResponse
            {
                Host = Common.HostName,
                ResponseString = "Sync completed",
                HttpCode = HttpStatusCode.OK,
                DataRows = new Dictionary<string, string>()
            };

            return new JsonResponse(nodeResponse, Serializer)
            {
                StatusCode = HttpStatusCode.OK,
                ReasonPhrase = "Successfully synced"
            };
        }

        private static JsonResponse GetNodeInfo()
        {
            var nodeResponse = new NodeResponse
            {
                Host = Common.HostName,
                ResponseString = "Node info returned",
                HttpCode = HttpStatusCode.OK,
                DataRows = new Dictionary<string, string>
                {
                    {"Host", Common.HostName},
                    {"Chain Length", OperationModule.Machine.Chain.Count.ToString()},
                    {"Visible Hosts", NodeBalance.NodeSet.Count.ToString()},
                    {"Pending Transactions", OperationModule.Machine.Pending.ToString()}
                }
            };

            return new JsonResponse(nodeResponse, Serializer)
            {
                StatusCode = HttpStatusCode.OK,
                ReasonPhrase = "Node info returned"
            };
        }

        private JsonResponse RegisterNewNode()
        {
            // TODO Validation of host?
            var nodeResponse = new NodeResponse
            {
                Host = Common.HostName,
                DataRows = new Dictionary<string, string>()
            };

            var jsonString = Request.Body.AsString();
            JObject jsonObject;
            try
            {
                jsonObject = JObject.Parse(jsonString);
            }
            catch (JsonReaderException)
            {
                nodeResponse.HttpCode = HttpStatusCode.BadRequest;
                nodeResponse.ResponseString = "Missing host parameter";

                return new JsonResponse(nodeResponse, Serializer)
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Missing parameters"
                };
            }

            var host = (string) jsonObject["host"];
            if (string.IsNullOrEmpty(host))
            {
                nodeResponse.HttpCode = HttpStatusCode.BadRequest;
                nodeResponse.ResponseString = "No host provided";

                return new JsonResponse(nodeResponse, Serializer)
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Missing value"
                };
            }

            NodeBalance.NodeSet.Add(host);
            nodeResponse.HttpCode = HttpStatusCode.OK;
            nodeResponse.ResponseString = "New host added, full host list returned";
            nodeResponse.DataRows.Add("Nodes", JsonConvert.SerializeObject(NodeBalance.NodeSet));

            return new JsonResponse(nodeResponse, Serializer)
            {
                StatusCode = HttpStatusCode.OK,
                ReasonPhrase = "Successfully added"
            };
        }
    }
}