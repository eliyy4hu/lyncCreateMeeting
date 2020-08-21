using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Test.Dto;

namespace WSSC.PRT.PNT7.Domain.Services
{
    public class LyncHttpClientInitState
    {
        protected LyncHttpClientState state = new LyncHttpClientState();

        /// <summary>
        /// https://docs.microsoft.com/en-us/skype-sdk/ucwa/createanapplication
        /// </summary>
        public static LyncHttpClientConfigured GetConfiguredLyncClient(string autodiscoveryUrl, string lyncServerUrl)
        {
            return new LyncHttpClientInitState(autodiscoveryUrl, lyncServerUrl)
                .ReceiveLinks()
                .ReceiveOauthTokenUrl()
                .ReceiveOauthToken()
                .ReceiveApplicationsLink()
                .ReceiveConfiguredApplicationLink();
        }

        protected LyncHttpClientInitState(LyncHttpClientState state)
        {
            this.state = state;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }

        private LyncHttpClientInitState(string autodiscoveryUrl, string lyncServerUrl)
        {
            state.AutodiscoveryUrl = autodiscoveryUrl;
            state.LyncServerUrl = lyncServerUrl;
            HttpClientHandler handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true
            };
            state.HttpClient = new HttpClient(handler);
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }

        protected Task<HttpResponseMessage> Send(HttpRequestMessage requestMessage)
        {
            if (!string.IsNullOrEmpty(state.XFrameHref))
            {
                requestMessage.Headers.Referrer =
                    new Uri(state.XFrameHref);
            }
            if (state.OauthToken != null)
            {
                requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue(state.OauthToken.token_type, state.OauthToken.access_token);
            }

            return state.HttpClient.SendAsync(requestMessage);
        }

        private LyncHttpClientInitState ReceiveLinks()
        {
            var main = state.HttpClient.GetAsync(state.AutodiscoveryUrl).Result;
            var contentString = main.Content.ReadAsStringAsync().Result;
            var links = JsonConvert.DeserializeObject<Info>(contentString)._links;
            state.UserHref = links.user.href;
            state.XFrameHref = links.xframe.href;
            state.SelfHref = links.self.href;
            return this;
        }

        private LyncHttpClientInitState ReceiveOauthTokenUrl()
        {
            using (var requestMessage =
            new HttpRequestMessage(HttpMethod.Get, state.UserHref))
            {
                var response = Send(requestMessage).Result;
                var oauthString = response.Headers.WwwAuthenticate.First(x => x.Scheme == "MsRtcOAuth").Parameter;
                var regexp = new Regex("href=\"(.*)\",grant_type=\"(.*)\"");
                var match = regexp.Match(oauthString);
                var result = new OauthLink
                { href = match.Groups[1].Value, grant_type = match.Groups[2].Value };
                state.OauthTokenHref = result;
                return this;
            }
        }

        private LyncHttpClientInitState ReceiveOauthToken()
        {
            var tokenUrl = state.OauthTokenHref.href;
            using (var requestMessage =
            new HttpRequestMessage(HttpMethod.Post, tokenUrl))
            {
                var nvc = new List<KeyValuePair<string, string>>();

                nvc.Add(new KeyValuePair<string, string>("grant_type", "urn:microsoft.rtc:windows"));

                //var mySecret = File.ReadAllText("mySecret.txt").Split(' ');
                //var login = "wss\\wss9";
                //var pass = "wss";
                //nvc.Add(new KeyValuePair<string, string>("grant_type", "password"));
                //nvc.Add(new KeyValuePair<string, string>("username", login));
                //nvc.Add(new KeyValuePair<string, string>("password", pass));

                requestMessage.Content = new FormUrlEncodedContent(nvc);

                var response = Send(requestMessage).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                var oauthToken = JsonConvert.DeserializeObject<OauthToken>(content);
                if (response.IsSuccessStatusCode)
                {
                    state.OauthToken = oauthToken;
                }
                return this;
            }
        }

        private LyncHttpClientInitState ReceiveApplicationsLink()
        {
            using (var requestMessage =
           new HttpRequestMessage(HttpMethod.Get, state.UserHref))
            {
                var result = Send(requestMessage).Result;
                var contentString = result.Content.ReadAsStringAsync().Result;
                var links = JsonConvert.DeserializeObject<Info>(contentString)._links;
                state.ApplicationsHref = links.applications.href;
            }
            return this;
        }

        private LyncHttpClientConfigured ReceiveConfiguredApplicationLink()
        {
            using (var requestMessage =
           new HttpRequestMessage(HttpMethod.Post, state.ApplicationsHref))
            {
                var json = JsonConvert.SerializeObject(new
                {
                    UserAgent = "UCWA Samples",
                    EndpointId = "a917c6f4-976c-4cf3-847d-cdfffa28ccdd",
                    Culture = "en-US"
                });
                requestMessage.Content =
                    new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var result = Send(requestMessage).Result;
                var content = result.Content.ReadAsStringAsync().Result;
                var info = JsonConvert.DeserializeObject<Info>(content);
                var links = info._links;
                state.RegisteredApplcationLink = links.self.href;
                state.Me = info._embedded.me;

                Console.WriteLine($"Hello, {state.Me.name}!");
                Console.WriteLine();
                Console.WriteLine($"your application: {state.RegisteredApplcationLink}");
                Console.WriteLine();
                state.MyOnlineMeetingsHref = info._embedded.onlineMeetings._links.myOnlineMeetings.href;
            }
            return new LyncHttpClientConfigured(state);
        }
    }

    public class LyncHttpClientConfigured : LyncHttpClientInitState
    {
        public LyncHttpClientConfigured(LyncHttpClientState state) : base(state)
        {
        }

        public void CreateMeeting(MeetingSettings settings)
        {
            var path = state.LyncServerUrl + state.MyOnlineMeetingsHref;
            using (var requestMessage =
           new HttpRequestMessage(HttpMethod.Post, path
           ))
            {
                settings.subject = "my subj";
                settings.description = "my description";
                settings.attendanceAnnouncementsStatus = "Disabled";
                settings.attendees = new string[] { "sip:ishakov@wss-consulting.ru" };
                settings.leaders = new string[] { "sip:ishakov@wss-consulting.ru" };

                var json = JsonConvert.SerializeObject(new { subject = "mySubj", description ="desc"});
                requestMessage.Content =
                    new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var result = Send(requestMessage).Result;
                var contentString = result.Content.ReadAsStringAsync().Result;
                settings = JsonConvert.DeserializeObject<MeetingSettings>(contentString);
                Console.WriteLine($"Created onlineMeetingId: {settings.onlineMeetingId}\n");
                Console.WriteLine($"With uri: {settings.onlineMeetingUri}\n");
                Console.WriteLine($"Url to join: {settings.joinUrl}\n");
                Console.ReadLine();
            }
        }
    }
}