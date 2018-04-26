using System.Collections.Generic;
using BlockChainMachine.Core;
using BlockChainNode.Net;
using BlockChainNode.ScaleVote;
using BlockChainNode.Lib.Logging;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses;
using Nancy.Serialization.JsonNet;
using Newtonsoft.Json;

namespace BlockChainNode.Modules
{
    public class OperationModule : NancyModule
    {
        public static BlockChain Machine = new BlockChain();

        public OperationModule() : base("/bc")
        {
            Get["/lastblock"] = parameters => GetLastBlock();
            Get["/chain"] = parameters => GetChain();
            Post["/newtrans"] = parameters => PostNewTransaction<VoteTransaction>();
        }

        public JsonResponse GetChain()
        {
            Logger.Log.Info($"Запрос на {this.Request.Url} от {this.Request.UserHostAddress} на получение данных о цепочке...");
            var resp = new JsonResponse(Machine.Chain,
                                        new JsonNetSerializer(new JsonSerializer
                                        {
                                            TypeNameHandling = TypeNameHandling.All
                                        })) {StatusCode = HttpStatusCode.OK};
            Logger.Log.Debug($"Возвращена цепочка длинной {Machine.Chain.Count}");
            return resp;
        }

        public JsonResponse GetLastBlock()
        {
            Logger.Log.Info($"Запрос на {this.Request.Url} от {this.Request.UserHostAddress} на получение последнего блока...");
            var resp = new JsonResponse(Machine.LastBlock, new JsonNetSerializer())
            {
                StatusCode = HttpStatusCode.OK
            };
            Logger.Log.Debug($"Возвращён блок №{Machine.LastBlock.Index}");
            return resp;
        }

        public JsonResponse PostNewTransaction<T>()
            where T : ITransaction
        {
            var transaction = this.Bind<T>();
            var failedValidation = false;
            var validationErrors = new List<string>();

            Logger.Log.Info($"Запрос на {this.Request.Url} от {this.Request.UserHostAddress} на совершение транзакции:");
            Logger.Log.Debug($"Data: {transaction.Data}\n" +
                             $"UserHash: {transaction.UserHash}\n" +
                             $"Signature: {transaction.Signature}");

            if (!transaction.HasValidData)
            {
                failedValidation = true;
                validationErrors.Add("Некорректные данные транзакции!");
            }

            if (string.IsNullOrEmpty(transaction.UserHash))
            {
                failedValidation = true;
                validationErrors.Add("Не передан хэш пользователя!");
            }

            if (transaction.Signature == null)
            {
                failedValidation = true;
                validationErrors.Add("Не передана подпись!");
            }

            if (failedValidation)
            {
                Logger.Log.Error($"Провести транзакцию не удалось!:\n" +
                                 $"{string.Join("\r\n", validationErrors)}");
                return new JsonResponse(null, new JsonNetSerializer())
                {
                    ReasonPhrase = string.Join("\r\n", validationErrors),
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var blocknum = Machine.AddNewTransaction(transaction);
            if (!Machine.Pending)
            {
                NodeBalance.PerformOnAllNodes(NodeBalance.SyncNode);
            }

            Logger.Log.Info("Транзакция проведена успешно");
            return new JsonResponse(null, new JsonNetSerializer())
            {
                ReasonPhrase = $"Adding transaction on block {blocknum}",
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}
