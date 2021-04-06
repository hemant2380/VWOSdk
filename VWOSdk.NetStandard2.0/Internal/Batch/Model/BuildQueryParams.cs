using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace VWOSdk
{


    public class BuildQueryParams
    {
     
    
        public string u;
   
        public object r;
        public long sId;


        public string t;
        public int? e = null;
        public int? c = null;
        public int eT;
      
        public int? g = null;
        public BuildQueryParams(Builder builder)
        {
           
            this.u = builder.u;
         
            this.r = builder.r;
            this.sId = builder.sId;
            this.t = builder.t;
            this.e = builder.e;
            this.c = builder.c;
            this.eT = builder.eT;
            this.g = builder.g;
         
        }


        public class Builder
        {
          
            public string u;
         
            public object r;
            public long sId;

            public string t;

            public int? e = null;
            public int? c = null;
            public int eT;
          
            public int? g = null;

            public Builder()
            {
            }

          

            public Builder withMinifiedCampaignId(int campaignId)
            {
                this.e = campaignId;
                return this;
            }

          
            private static double GetRandomNumber()
            {
                Random random = new Random();
                return random.NextDouble();
            }
            public Builder withUuid(long account_id, string uId)
            {
                this.u = UuidV5Helper.Compute(account_id, uId);

                return this;
            }

           

            public Builder withMinifiedVariationId(int variationId)
            {
                this.c = variationId;
                return this;
            }

           

            public Builder withSid(long sId)
            {
                this.sId = sId;
                return this;
            }

          

            public Builder withMinifiedGoalId(int goal_id)
            {
                this.g = goal_id;
                return this;
            }

            public Builder withRevenue(Object r)
            {
                this.r = r;
                return this;
            }

          

            public Builder withMinifiedEventType(int eventType)
            {
                this.eT = eventType;
                return this;
            }

            public Builder withMinifiedTags(String tagKey, String tagValue)
            {
                this.t = "{\"u\":{\"" + tagKey + "\":\"" + tagValue + "\"}}";
                return this;
            }


            public static Builder getInstance()
            {
                return new Builder();
            }

            public BuildQueryParams build()
            {
                return new BuildQueryParams(this);
            }
        }

        public IDictionary<string, dynamic> convertToMap(IDictionary<string, dynamic> map)
        {

            // Rename 'sdk_v' as 'sdk-v'
            dynamic value;
            map.TryGetValue("sdk_v", out value);
            map.Add("sdk-v", value);
            map.Remove("sdk_v");

            return map;
        }

        public IDictionary<string, dynamic> removeNullValues(BuildQueryParams val)
        {
            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(val);
            var allValue = JsonConvert.DeserializeObject<IDictionary<string, dynamic>>(jsonString);
            var withoutNull = allValue.Where(f => f.Value != null ).ToDictionary(x => x.Key, x => x.Value);

            return withoutNull;
        }
    }
}
