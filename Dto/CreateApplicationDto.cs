using System;
using System.Collections.Generic;

namespace Test.Dto
{
    [Serializable]
    public class Info
    {
        public Links _links { get; set; }
        public Embedded _embedded { get; set; }
    }

    public class MeetingSettings
    {
        public string attendanceAnnouncementsStatus { get; set; }
        public string description { get; set; }
        public string subject { get; set; }
        public string[] attendees { get; set; }
        public string[] leaders { get; set; }
        public string onlineMeetingId { get; set; }
        public string onlineMeetingUri { get; set; }
        public string joinUrl { get; set; }
    }

    public class Embedded
    {
        public OnlineMeetings onlineMeetings { get; set; }
        public Me me { get; set; }
    }
    public class Me
    {
        public string name;
        public string uri;
    }

    public class OnlineMeetings
    {
        public Links _links { get; set; }
    }

    public class Links
    {
        public Link self { get; set; }
        public Link user { get; set; }
        public Link xframe { get; set; }
        public Link applications { get; set; }
        public Link myOnlineMeetings { get; set; }
    }

    public class Link
    {
        public string href { get; set; }
    }

    public class OauthLink : Link
    {
        public string grant_type { get; set; }
    }

    public class OauthToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string ms_rtc_identityscope { get; set; }
        public string token_type { get; set; }
    }
}