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
            var resp = new JsonResponse(Machine.Chain, new DefaultJsonSerializer())
            {
                StatusCode = HttpStatusCode.OK
            };
            return resp;
        }

        public JsonResponse GetLastBlock()
        {
            var resp = new JsonResponse(Machine.LastBlock, new DefaultJsonSerializer())
            {
                StatusCode = HttpStatusCode.OK
            };
            return resp;
        }

        public JsonResponse PostNewTransaction()
        {
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
                return new JsonResponse(null, new DefaultJsonSerializer())
                {
                    ReasonPhrase = string.Join("\r\n", validationErrors),
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var blocknum = Machine.AddNewTransaction(transaction);

            return new JsonResponse(null, new DefaultJsonSerializer())
            {
                ReasonPhrase = $"Adding transaction on block {blocknum}",
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}