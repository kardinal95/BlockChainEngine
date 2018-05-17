using System;
using System.Collections.Generic;
using BlockChainNode.Lib.Logging;
using BlockChainNode.Lib.Net;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses;
using Nancy.Serialization.JsonNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlockChainNode.Lib.Modules
{
    // ReSharper disable once UnusedMember.Global
    public class CommunicationModule : NancyModule
    {
        private static readonly JsonNetSerializer Serializer = new JsonNetSerializer();

        public CommunicationModule() : base("/comm")
        {
            Get["/info"] = parameters => ParseRequest(GetNodeInfo);
            Post["/register"] = parameters => ParseRequest(RegisterNewNode);
        }

        private JsonResponse ParseRequest(Func<NodeResponse> func)
        {
            Logger.Log.Info($"Получен {Request.Method} запрос " +
                            $"на {Request.Url} от {Request.UserHostAddress}");

            var nodeResponse = func.Invoke();
            nodeResponse.Host = Common.HostName;

            Logger.Log.Debug(nodeResponse);

            return new JsonResponse(nodeResponse, Serializer) {StatusCode = nodeResponse.HttpCode};
        }

        private static NodeResponse GetNodeInfo()
        {
            Logger.Log.Debug($"Адрес хоста: {Common.HostName}\n" +
                             $"Количество узлов: {NodeBalance.NodeSet.Count}");

            var nodeResponse = new NodeResponse
            {
                HttpCode = HttpStatusCode.OK,
                ResponseString = "Node info returned",
                DataRows = new Dictionary<string, string>
                {
                    ["Host"] = Common.HostName,
                    ["Chain Length"] = OperationModule.Machine.Chain.Count.ToString(),
                    ["Visible Hosts"] = NodeBalance.NodeSet.Count.ToString(),
                    ["Pending Transactions"] = OperationModule.Machine.Pending.ToString()
                }
            };

            return nodeResponse;
        }

        private NodeResponse RegisterNewNode()
        {
            // TODO Validation of host?
            var nodeResponse = new NodeResponse {DataRows = new Dictionary<string, string>()};

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

                return nodeResponse;
            }

            var host = (string) jsonObject["host"];
            if (string.IsNullOrEmpty(host))
            {
                nodeResponse.HttpCode = HttpStatusCode.BadRequest;
                nodeResponse.ResponseString = "No host provided";

                return nodeResponse;
            }

            NodeBalance.NodeSet.Add(host);
            nodeResponse.HttpCode = HttpStatusCode.OK;
            nodeResponse.ResponseString = "New host added, full host list returned";
            nodeResponse.DataRows.Add("Nodes", JsonConvert.SerializeObject(NodeBalance.NodeSet));

            Logger.Log.Info($"Добавлен новый хост {host}");
            Logger.Log.Debug($"Новое количество узлов: {NodeBalance.NodeSet.Count}");

            return nodeResponse;
        }
    }
}