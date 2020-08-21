
namespace WSSC.PRT.PNT7.Domain.Services
{
    public class LyncService
    {
        //private DataContext context;
        private LyncHttpClientConfigured httpClient;

        public LyncService(/*DataContext context,*/ string autodiscoveryUrl = @"https://lyncdiscover.wss.loc", string lyncServerUrl = @"https://lync.wss-consulting.ru")
        {
            
            httpClient = LyncHttpClientInitState.GetConfiguredLyncClient(autodiscoveryUrl, lyncServerUrl);
        }

        public void CreateMeeting()
        {
            httpClient.CreateMeeting(new Test.Dto.MeetingSettings());
        }
    }
}