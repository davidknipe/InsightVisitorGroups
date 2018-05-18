using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Web.Mvc;
using EPiServer.Personalization.VisitorGroups;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc.VisitorGroups;
using InsightVisitorGroups.Impl;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;

namespace InsightVisitorGroups.Criterion
{
    public class MailingCriterionModel : CriterionModelBase
    {
        public class MailingSelectionFactory : ISelectionFactory
        {
            public const string ConfigUsername = "campaign:Username";
            public const string ConfigPassword = "campaign:Password";
            public const string ConfigClientid = "campaign:Clientid";

            private Injected<IMailing> _mailing;

            public IEnumerable<SelectListItem> GetSelectListItems(Type property)
            {
                List<SelectListItem> items = new List<SelectListItem>();

                foreach (var activeMailing in _mailing.Service.GetActiveMailings())
                {
                    items.Add(new SelectListItem() {Value = activeMailing.Item1.ToString(), Text = activeMailing.Item2 + " (Smart Campaign)" });
                }

                var allMailings = getMailings();
                if (allMailings != null)
                    foreach (var mailing in (allMailings as dynamic).elements)
                    {
                        items.Add(new SelectListItem() {Value = mailing.id, Text = mailing.name + " (Mailing)"});
                    }


                return items;
            }

            private JObject getMailings()
            {
                // Set up the request
                var client = new RestClient("https://api.campaign.episerver.net");
                client.Authenticator = new HttpBasicAuthenticator(ConfigurationManager.AppSettings[ConfigUsername], ConfigurationManager.AppSettings[ConfigPassword]);

                var request = new RestRequest("/rest/" + ConfigurationManager.AppSettings[ConfigClientid] + "/transactionalmail", Method.GET);

                // Execute the request to get the events
                IRestResponse getEventResponse = client.Execute(request);
                var getEventContent = getEventResponse.Content;

                // Get the results as a JArray object
                var eventResponseObject = JObject.Parse(getEventContent);

                return eventResponseObject;
            }

        }

        /// <summary>
        /// The condition to apply when checking the mailing condition
        /// </summary>
        [Required, DojoWidget(SelectionFactoryType = typeof(EnumSelectionFactory), LabelTranslationKey = "")]
        public MailingCondition Condition { get; set; }
        
        /// <summary>
        /// The name of the cookie to check
        /// </summary>
        [Required, DojoWidget(SelectionFactoryType = typeof(MailingSelectionFactory), LabelTranslationKey = "")]
        public string Mailing { get; set; }

        public override ICriterionModel Copy()
        {
            return base.ShallowCopy();
        }
    }
}