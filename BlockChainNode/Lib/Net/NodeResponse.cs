using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace BlockChainNode.Lib.Net
{
    struct NodeResponse
    {
        public string Host { get; set; }
        public string ResponseString { get; set; }
        public HttpStatusCode HttpCode { get; set; }
        public Dictionary<string, string> DataRows { get; set; }
}
}
