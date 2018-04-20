using System.Collections.Generic;
using BlockChainMachine.Core;
using BlockChainNode.Net;
using BlockChainNode.ScaleVote;
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
            var resp = new JsonResponse(Machine.Chain,
                                        new JsonNetSerializer(new JsonSerializer
                                        {
                                            TypeNameHandling = TypeNameHandling.All
                                        })) {StatusCode = HttpStatusCode.OK};
            return resp;
        }

        public JsonResponse GetLastBlock()
        {
            var resp = new JsonResponse(Machine.LastBlock, new JsonNetSerializer())
            {
                StatusCode = HttpStatusCode.OK
            };
            return resp;
        }

        public JsonResponse PostNewTransaction<T>()
            where T : ITransaction
        {
            var transaction = this.Bind<T>();
            var failedValidation = false;
            var validationErrors = new List<string>();

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

            return new JsonResponse(null, new JsonNetSerializer())
            {
                ReasonPhrase = $"Adding transaction on block {blocknum}",
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}