using System.Collections.Generic;
using System.Configuration;
using BlockChainNode.Net;
using BlockChainNode.Lib.Logging;
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
            Logger.Log.Info($"Запрос на {this.Request.Url} от {this.Request.UserHostAddress}" +
                            $"на синхронизацию узлов...");
            NodeBalance.PerformOnAllNodes(NodeBalance.RebalanceNode);
            Logger.Log.Debug($"Возвращено число узлов: {NodeBalance.NodeSet.Count}");
            return new JsonResponse(
                new Dictionary<string, string>
                {
                    {"CurrentNodes", NodeBalance.NodeSet.Count.ToString()}
                }, new JsonNetSerializer()) {StatusCode = HttpStatusCode.OK};
        }

        private JsonResponse GetNodeInfo()
        {
            Logger.Log.Info($"Запрос на {this.Request.Url} от {this.Request.UserHostAddress} " +
                            $"на получение информации об узлах");
            Logger.Log.Debug($"Адрес хоста: {ConfigurationManager.AppSettings["host"]}\n" +
                             $"Количество узлов: {NodeBalance.NodeSet.Count}");
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
            Logger.Log.Info($"Запрос на {this.Request.Url} от {this.Request.UserHostAddress} " +
                            $"на регистрацию нового узла");

            if (string.IsNullOrEmpty(host))
            {
                Logger.Log.Error("Не предоставлены данные о хосте");
                return new JsonResponse(null, new JsonNetSerializer())
                {
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            NodeBalance.NodeSet.Add(host);
            Logger.Log.Info("Добавлен новый хост");
            Logger.Log.Debug($"Адрес хоста: {host}");
            Logger.Log.Debug($"Количество узлов: {NodeBalance.NodeSet.Count}");
            return new JsonResponse(NodeBalance.NodeSet, new JsonNetSerializer())
            {
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}
