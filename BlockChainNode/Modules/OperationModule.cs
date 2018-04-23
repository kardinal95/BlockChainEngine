using System.Collections.Generic;
using BlockChainMachine.Core;
using BlockChainNode.Lib.Net;
using BlockChainNode.Net;
using BlockChainNode.ScaleVote;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses;
using Nancy.Serialization.JsonNet;
using Newtonsoft.Json;
using Common = BlockChainNode.Lib.Net.Common;

namespace BlockChainNode.Modules
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

        private static JsonResponse GetChain()
        {
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

            return new JsonResponse(nodeResponse, Serializer)
            {
                StatusCode = HttpStatusCode.OK,
                ReasonPhrase = "Successful"
            };
        }

        private static JsonResponse GetLastBlock()
        {
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

            return new JsonResponse(nodeResponse, Serializer)
            {
                StatusCode = HttpStatusCode.OK,
                ReasonPhrase = "Successful"
            };
        }
    }
}