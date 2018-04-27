using System.Collections.Generic;
using BlockChainMachine.Core;
using BlockChainNode.Lib.Logging;
using BlockChainNode.Lib.Net;
using BlockChainNode.Lib.ScaleVote;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses;
using Nancy.Serialization.JsonNet;
using Newtonsoft.Json;
using Common = BlockChainNode.Lib.Net.Common;

namespace BlockChainNode.Lib.Modules
{
    public class OperationModule : NancyModule
    {
        public static readonly BlockChain Machine = new BlockChain();

        private static readonly JsonNetSerializer Serializer =
            new JsonNetSerializer(new JsonSerializer {TypeNameHandling = TypeNameHandling.All});

        public OperationModule() : base("/bc")
        {
            Get["/lastblock"] = parameters => GetLastBlock();
            Get["/chain"] = parameters => GetChain();
            Post["/newvote"] = parameters => PostNewTransaction<VoteTransaction>();
        }

        private JsonResponse GetChain()
        {
            Logger.Log.Info(
                $"Запрос на {Request.Url} от {Request.UserHostAddress} на получение данных о цепочке...");
            var nodeResponse = new NodeResponse
            {
                Host = Common.HostName,
                ResponseString = $"Chain of length {Machine.Chain.Count} provided",
                HttpCode = HttpStatusCode.OK,
                DataRows = new Dictionary<string, string>
                {
                    {"Chain", JsonConvert.SerializeObject(Machine.Chain)}
                }
            };

            Logger.Log.Debug($"Возвращена цепочка длинной {Machine.Chain.Count}");
            return new JsonResponse(nodeResponse, Serializer)
            {
                StatusCode = HttpStatusCode.OK,
                ReasonPhrase = "Successful"
            };
        }

        private JsonResponse GetLastBlock()
        {
            Logger.Log.Info(
                $"Запрос на {Request.Url} от {Request.UserHostAddress} на получение последнего блока...");
            var nodeResponse = new NodeResponse
            {
                Host = Common.HostName,
                ResponseString = "Last block returned",
                HttpCode = HttpStatusCode.OK,
                DataRows = new Dictionary<string, string>
                {
                    {"Block", JsonConvert.SerializeObject(Machine.LastBlock)}
                }
            };

            Logger.Log.Debug($"Возвращён блок №{Machine.LastBlock.Index}");
            return new JsonResponse(nodeResponse, Serializer)
            {
                StatusCode = HttpStatusCode.OK,
                ReasonPhrase = "Successful"
            };
        }

        private JsonResponse PostNewTransaction<T>()
            where T : ITransaction
        {
            var nodeResponse = new NodeResponse
            {
                Host = Common.HostName,
                DataRows = new Dictionary<string, string>()
            };

            var transaction = this.Bind<T>();
            var validationErrors = new List<string>();

            Logger.Log.Info(
                $"Запрос на {Request.Url} от {Request.UserHostAddress} на совершение транзакции:");
            Logger.Log.Debug($"Data: {transaction.Data}\n" + $"UserHash: {transaction.UserHash}\n" +
                             $"Signature: {transaction.Signature}");

            if (!transaction.HasValidData)
            {
                validationErrors.Add("Некорректные данные транзакции!");
            }

            if (string.IsNullOrEmpty(transaction.UserHash))
            {
                validationErrors.Add("Не передан хэш пользователя!");
            }

            if (transaction.Signature == null)
            {
                validationErrors.Add("Не передана подпись!");
            }

            if (validationErrors.Count != 0)
            {
                Logger.Log.Error($"Провести транзакцию не удалось!:\n" +
                                 $"{string.Join("\r\n", validationErrors)}");

                nodeResponse.HttpCode = HttpStatusCode.BadRequest;
                nodeResponse.ResponseString = string.Join("\r\n", validationErrors);

                return new JsonResponse(nodeResponse, Serializer)
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Errors found"
                };
            }

            // TODO Correct sync of transactions
            Machine.AddNewTransaction(transaction);
            if (!Machine.Pending)
            {
                NodeBalance.PerformOnAllNodes(NodeBalance.SyncNode);
            }

            nodeResponse.HttpCode = HttpStatusCode.OK;
            nodeResponse.ResponseString = "Added new transaction";

            Logger.Log.Info("Транзакция проведена успешно");
            return new JsonResponse(nodeResponse, Serializer)
            {
                StatusCode = HttpStatusCode.OK,
                ReasonPhrase = "Successful"
            };
        }
    }
}