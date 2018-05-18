using System;
using System.Configuration;
using System.Security.Principal;
using System.Web;
using EPiServer.Personalization.VisitorGroups;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace InsightVisitorGroups.Criterion
{
    [VisitorGroupCriterion(
        Category = "Insight Criteria",
        DisplayName = "Campaign mailing status",
        Description = "Check on the status of a mailing to a user")]
    public class MailingCriteria : CriterionBase<MailingCriterionModel>
    {
        private string deviceId;
        private string apiRootUrl;
        private string profileStoreApiKey;
        private string resourceGetEvent;
        private string resourceGetProfile;

        public MailingCriteria()
        {
            apiRootUrl = ConfigurationManager.AppSettings["profileStore.RootApiUrl"];
            profileStoreApiKey = ConfigurationManager.AppSettings["profileStore.SubscriptionKey"];
            resourceGetEvent = "/api/v1.0/trackevents/";
            resourceGetProfile = "api/v1.0/Profiles";
        }

        public override bool IsMatch(IPrincipal principal, HttpContextBase httpContext)
        {
            deviceId = HttpContext.Current.Request.Cookies["_madid"]?.Value;

            switch (Model.Condition)
            {
                case MailingCondition.Received:
                    if (hasStoreEvents("epiCampaignMailingtouser", Model.Mailing))
                        return true;
                    break;
                case MailingCondition.Opened:
                    if (hasStoreEvents("epiCampaignOpen", Model.Mailing))
                        return true;
                    break;
                case MailingCondition.Clicked:
                    if (hasStoreEvents("epiCampaignClick", Model.Mailing))
                        return true;
                    break;
            }

            return false;
        }


        private bool hasStoreEvents(string eventType, string mailingId)
        {
            try
            {
                string userEmail = GetProfile()["Info"]["Email"].ToString();

                // Set up the request
                var client = new RestClient(apiRootUrl);

                var request = new RestRequest(resourceGetEvent, Method.GET);
                request.AddHeader("Ocp-Apim-Subscription-Key", profileStoreApiKey);

                request.AddParameter("$top", "1");
                request.AddParameter("$filter",
                    "EventType eq " + eventType + " and DeviceId eq " + userEmail +
                    " and Payload.epi.campaign.mailingId eq " + mailingId);

                // Execute the request to get the events
                IRestResponse getEventResponse = client.Execute(request);
                var getEventContent = getEventResponse.Content;

                // Get the results as a JArray object
                var eventResponseObject = JObject.Parse(getEventContent);

                return eventResponseObject["items"].HasValues;
            }
            catch
            {
                return false;
            }
        }

        private JToken GetProfile()
        {
            // Set up the request
            var client = new RestClient(apiRootUrl);
            var request = new RestRequest(resourceGetProfile, Method.GET);
            request.AddHeader("Ocp-Apim-Subscription-Key", profileStoreApiKey);

            // Filter the profiles based on the current device id
            request.AddParameter("$filter", "DeviceIds eq " + deviceId);

            // Execute the request to get the profile
            var getProfileResponse = client.Execute(request);
            var getProfileContent = getProfileResponse.Content;

            // Get the results as a JArray object
            var profileResponseObject = JObject.Parse(getProfileContent);
            var profileArray = (JArray) profileResponseObject["items"];

            // Expecting an array of profiles with one item in it
            var profileObject = profileArray.First;

            return profileObject;
        }
    }
}