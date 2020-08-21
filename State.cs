using System.Net.Http;
using Test.Dto;

namespace WSSC.PRT.PNT7.Domain.Services
{
    public class LyncHttpClientState
    {
        public string AutodiscoveryUrl;
        public HttpClient HttpClient;
        public string UserHref;
        public string XFrameHref;
        public string SelfHref;
        public string ApplicationsHref;
        public OauthLink OauthTokenHref;
        public OauthToken OauthToken;
        public string RegisteredApplcationLink;
        public string MyOnlineMeetingsHref;

        public string LyncServerUrl { get; internal set; }
        public Me Me { get; internal set; }
    }
}