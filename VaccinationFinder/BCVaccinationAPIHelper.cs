﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VaccinationFinder.Models;

namespace VaccinationFinder
{
    public class BCVaccinationAPIHelper
    {
        private string? VaccinationUrl = "https://www.getvaccinated.gov.bc.ca/s/sfsites/aura?r=11&aura.ApexAction.execute=1";
        private string FacilityMessage = "{{\"actions\":[{{\"id\":\"147;a\",\"descriptor\":\"aura://ApexActionController/ACTION$execute\",\"callingDescriptor\":\"UNKNOWN\",\"params\":{{\"namespace\":\"\",\"classname\":\"BCH_SchedulerController\",\"method\":\"getFacilities\",\"params\":{{\"territory\":\"{0}\",\"priorityCode\":\"\"}},\"cacheable\":true,\"isContinuation\":false}}}}]}}";
        private string SpecificFacilityMessage = "{{\"actions\":[{{\"id\":\"149;a\",\"descriptor\":\"aura://ApexActionController/ACTION$execute\",\"callingDescriptor\":\"UNKNOWN\",\"params\":{{\"namespace\":\"\",\"classname\":\"BCH_SchedulerController\",\"method\":\"getAppointmentDays\",\"params\":{{\"facility\":\"{0}\",\"appointmentType\":\"COVID-19 Vaccination\"}},\"cacheable\":true,\"isContinuation\":false}}}}]}}";
        private string AppointmentBlockMessage = "{{\"actions\":[{{\"id\":\"156;a\",\"descriptor\":\"aura://ApexActionController/ACTION$execute\",\"callingDescriptor\":\"UNKNOWN\",\"params\":{{\"namespace\":\"\",\"classname\":\"BCH_SchedulerController\",\"method\":\"getAppointmentBlocks\",\"params\":{{\"appointmentDay\":\"{0}\", \"facility\":\"{1}\",\"appointmentType\":\"COVID-19 Vaccination\"}},\"cacheable\":true,\"isContinuation\":false}}}}]}}";
        private string AuraContext = "{\"mode\":\"PROD\",\"fwuid\":\"SOME_FWUID\",\"app\":\"siteforce:communityApp\",\"loaded\":{\"APPLICATION@markup://siteforce:communityApp\":\"APP_ID\"},\"dn\":[],\"globals\":{},\"uad\":false}";
        private string AuraToken = "undefined";

        public async Task<AuthoritativeFacilityList> GetFacilities(string region)
        {
            List<KeyValuePair<string, string>> _requestValues = new List<KeyValuePair<string, string>>();
            _requestValues.Add(new KeyValuePair<string, string>("message", string.Format(FacilityMessage, region)));
            _requestValues.Add(new KeyValuePair<string, string>("aura.context", AuraContext));
            _requestValues.Add(new KeyValuePair<string, string>("aura.token", AuraToken));

            return await SendRequest<AuthoritativeFacilityList>(_requestValues);
        }

        public async Task<AuthoritativeVaccinationFacility> GetFacilityDays(string facilityId)
        {
            List<KeyValuePair<string, string>> _requestValues = new List<KeyValuePair<string, string>>();
            _requestValues.Add(new KeyValuePair<string, string>("message", string.Format(SpecificFacilityMessage, facilityId)));
            _requestValues.Add(new KeyValuePair<string, string>("aura.context", AuraContext));
            _requestValues.Add(new KeyValuePair<string, string>("aura.token", AuraToken));

            return await SendRequest<AuthoritativeVaccinationFacility>(_requestValues);
        }

        public async Task<VaccinationBlock> GetTimeBlocks(string dayId, string facility)
        {
            List<KeyValuePair<string, string>> _requestValues = new List<KeyValuePair<string, string>>();
            _requestValues.Add(new KeyValuePair<string, string>("message", string.Format(AppointmentBlockMessage, dayId, facility)));
            _requestValues.Add(new KeyValuePair<string, string>("aura.context", AuraContext));
            _requestValues.Add(new KeyValuePair<string, string>("aura.token", AuraToken));

            return await SendRequest<VaccinationBlock>(_requestValues);
        }

        private async Task<T> SendRequest<T>(List<KeyValuePair<string, string>> messages)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, VaccinationUrl)
            {
                Content = new FormUrlEncodedContent(messages)
            };

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                return JsonSerializer.Deserialize<T>(result);
            }
            else
            {
                return default(T);
            }
        }
    }
}