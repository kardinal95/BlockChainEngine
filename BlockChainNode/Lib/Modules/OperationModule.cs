using System;
using System.Collections.Generic;
using BlockChainMachine.Core;
using BlockChainNode.Lib.Logging;
using BlockChainNode.Lib.Net;
using Nancy;
using Nancy.Extensions;
using Nancy.ModelBinding;
using Nancy.Responses;
using Nancy.Serialization.JsonNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlockChainNode.Lib.Modules
{
    public class OperationModule : NancyModule
    {
        public static readonly BlockChain Machine = new BlockChain();

        private static readonly JsonNetSerializer Serializer = new JsonNetSerializer();

        public OperationModule() : base("/bc")
        {
            Get["/lastblock"] = parameters => ParseRequest(GetLastBlock);
            Get["/chain"] = parameters => ParseRequest(GetChain);
            Post["/newvote"] = parameters => ParseRequest(PostNewTransaction);
            Post["/newblock"] = parameters => ParseRequest(PostNewBlock);
        }

        private JsonResponse ParseRequest(Func<NodeResponse> func)
        {
            Logger.Log.Info($"Получен {Request.Method} запрос " +
                            $"на {Request.Url} от {Request.UserHostAddress}");

            var nodeResponse = func.Invoke();
            nodeResponse.Host = Common.HostName;

            Logger.Log.Debug(nodeResponse);

            if (nodeResponse.HttpCode == HttpStatusCode.Conflict)
            {
                NodeBalance.RebalanceSelf();
            }

            return new JsonResponse(nodeResponse, Serializer) {StatusCode = nodeResponse.HttpCode};
        }

        private static NodeResponse GetLastBlock()
        {
            return new NodeResponse
            {
                ResponseString = "Last block returned",
                HttpCode = HttpStatusCode.OK,
                DataRows = new Dictionary<string, string>
                {
                    {"Block", JsonConvert.SerializeObject(Machine.LastBlock)}
                }
            };
        }

        private static NodeResponse GetChain()
        {
            return new NodeResponse
            {
                ResponseString = $"Chain of length {Machine.Chain.Count} provided",
                HttpCode = HttpStatusCode.OK,
                DataRows = new Dictionary<string, string>
                {
                    {"Chain", JsonConvert.SerializeObject(Machine.Chain)}
                }
            };
        }

        private NodeResponse PostNewTransaction()
        {
            var nodeResponse = new NodeResponse {DataRows = new Dictionary<string, string>()};

            Transaction transaction;

            try
            {
                transaction = this.Bind<Transaction>();
            }
            catch (ModelBindingException)
            {
                nodeResponse.HttpCode = HttpStatusCode.BadRequest;
                nodeResponse.ResponseString = "Не удалось получить данные модели из запроса!";
                return nodeResponse;
            }

            var validationErrors = new List<string>();
            if (string.IsNullOrEmpty(transaction.UserHash))
            {
                validationErrors.Add("Не передан хэш пользователя!");
            }

            if (transaction.Signature is null)
            {
                validationErrors.Add("Не передана подпись!");
            }

            if (transaction.Data is null)
            {
                validationErrors.Add("Не переданы данные!");
            }

            if (!transaction.Valid)
            {
                validationErrors.Add("Подпись не совпадает с переданным ключом!");
            }

            if (validationErrors.Count != 0)
            {
                Logger.Log.Error("Провести транзакцию не удалось!:\n" +
                                 $"{string.Join("\r\n", validationErrors)}");

                nodeResponse.HttpCode = HttpStatusCode.BadRequest;
                nodeResponse.ResponseString = string.Join("\r\n", validationErrors);

                return nodeResponse;
            }

            Machine.AddNewTransaction(transaction);
            if (!Machine.Pending)
            {
                NodeBalance.BroadcastNewBlock();
            }

            nodeResponse.HttpCode = HttpStatusCode.OK;
            nodeResponse.ResponseString = "Added new transaction";

            return nodeResponse;
        }

        private NodeResponse PostNewBlock()
        {
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
                nodeResponse.ResponseString = "Missing parameters";

                return nodeResponse;
            }

            var block = JsonConvert.DeserializeObject<Block>((string) jsonObject["Block"]);

            var validationErrors = new List<string>();

            if (block.Transactions.Count == 0)
            {
                validationErrors.Add("Нет транзакций!");
            }

            if (string.IsNullOrEmpty(block.TimeStamp))
            {
                validationErrors.Add("Не передан таймштамп!");
            }

            if (string.IsNullOrEmpty(block.PreviousHash))
            {
                validationErrors.Add("Не передан хэш блока!");
            }

            if (!block.Valid || Machine.LastBlock.Hash != block.PreviousHash)
            {
                validationErrors.Add("Некорректный блок!");
            }

            if (validationErrors.Count != 0)
            {
                Logger.Log.Error("Добавить блок не удалось!:\n" +
                                 $"{string.Join("\r\n", validationErrors)}");

                nodeResponse.HttpCode = HttpStatusCode.BadRequest;
                nodeResponse.ResponseString = string.Join("\r\n", validationErrors);

                return nodeResponse;
            }

            var success = Machine.TryAddBlock(block);
            if (!success)
            {
                nodeResponse.HttpCode = HttpStatusCode.Conflict;
                nodeResponse.ResponseString = "Блок отклонен";
            }
            else
            {
                nodeResponse.HttpCode = HttpStatusCode.OK;
                nodeResponse.ResponseString = "Блок принят";
            }

            return nodeResponse;
        }
    }
}