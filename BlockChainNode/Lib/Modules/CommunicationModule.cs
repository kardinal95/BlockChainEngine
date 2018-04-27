using System.Collections.Generic;
using System.Configuration;
using BlockChainNode.Lib.Logging;
using BlockChainNode.Lib.Net;
using BlockChainNode.Net;
using Nancy;
using Nancy.Extensions;
using Nancy.Responses;
using Nancy.Serialization.JsonNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Common = BlockChainNode.Lib.Net.Common;

namespace BlockChainNode.Lib.Modules
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

        private JsonResponse Sync()
        {
            Logger.Log.Info($"Запрос на {Request.Url} от {Request.UserHostAddress}" +
                            $"на синхронизацию узлов...");

            NodeBalance.PerformOnAllNodes(NodeBalance.RebalanceNode);

            var nodeResponse = new NodeResponse
            {
                Host = Common.HostName,
                ResponseString = "Sync completed",
                HttpCode = HttpStatusCode.OK,
                DataRows = new Dictionary<string, string>()
            };

            Logger.Log.Debug($"Возвращено число узлов: {NodeBalance.NodeSet.Count}");
            return new JsonResponse(nodeResponse, Serializer)
            {
                StatusCode = HttpStatusCode.OK,
                ReasonPhrase = "Successfully synced"
            };
        }

        private JsonResponse GetNodeInfo()
        {
            Logger.Log.Info($"Запрос на {Request.Url} от {Request.UserHostAddress} " +
                            $"на получение информации об узлах");
            Logger.Log.Debug($"Адрес хоста: {ConfigurationManager.AppSettings["host"]}\n" +
                             $"Количество узлов: {NodeBalance.NodeSet.Count}");

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

            Logger.Log.Info($"Запрос на {Request.Url} от {Request.UserHostAddress} " +
                            $"на регистрацию нового узла");

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

            Logger.Log.Info("Добавлен новый хост");
            Logger.Log.Debug($"Адрес хоста: {host}");
            Logger.Log.Debug($"Количество узлов: {NodeBalance.NodeSet.Count}");

            return new JsonResponse(nodeResponse, Serializer)
            {
                StatusCode = HttpStatusCode.OK,
                ReasonPhrase = "Successfully added"
            };
        }
    }
}