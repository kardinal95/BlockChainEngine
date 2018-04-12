using System.Collections.Generic;
using BlockChainMachine.Core;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses;

namespace BlockChainNode.Modules
{
    public class OperationModule : NancyModule
    {
        private static readonly BlockChain Machine = new BlockChain();

        public OperationModule() : base("/bc")
        {
            Get["/lastblock"] = parameters => GetLastBlock();
            Get["/chain"] = parameters => GetChain();
            Post["/newtrans"] = parameters => PostNewTransaction();
        }

        public JsonResponse GetChain()
        {
            var resp = new JsonResponse(Machine.Chain, new DefaultJsonSerializer());
            return resp;
        }

        public JsonResponse GetLastBlock()
        {
            var resp = new JsonResponse(Machine.LastBlock, new DefaultJsonSerializer());
            return resp;
        }

        public Response PostNewTransaction()
        {
            var resp = new Response();

            var transaction = this.Bind<Transaction>();
            var failedValidation = false;
            var validationErrors = new List<string>();

            if (string.IsNullOrEmpty(transaction.PollId))
            {
                failedValidation = true;
                validationErrors.Add("Не передан идентификатор опроса!");
            }

            if (string.IsNullOrEmpty(transaction.OptionId))
            {
                failedValidation = true;
                validationErrors.Add("Не передан идентификатор выбранного ответа!");
            }

            if (string.IsNullOrEmpty(transaction.UserHash))
            {
                failedValidation = true;
                validationErrors.Add("Не передан хэш пользователя!");
            }

            if (failedValidation)
            {
                resp.ReasonPhrase = string.Join("\r\n", validationErrors);
                resp.StatusCode = HttpStatusCode.BadRequest;
                return resp;
            }

            var blocknum = Machine.AddNewTransaction(transaction);

            resp.ReasonPhrase = $"Adding transaction on block {blocknum}";
            resp.StatusCode = HttpStatusCode.Created;
            return resp;
        }
    }
}