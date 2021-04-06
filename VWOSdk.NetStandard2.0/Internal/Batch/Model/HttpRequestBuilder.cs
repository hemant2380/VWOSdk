

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VWOSdk
{



    public class HttpRequestBuilder
    {
        //Batch Tracking Events

        public static IDictionary<string, dynamic> getBatchEventForTrackingUser(long accountId, int campaignId, int variationId, string userId,  bool isDevelopmentMode)
        {

            BuildQueryParams requestParams =
            BuildQueryParams.Builder.getInstance()
                    .withMinifiedCampaignId(campaignId)
                    .withMinifiedVariationId(variationId)
                    .withMinifiedEventType(1)
                    .withSid(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    .withUuid(accountId, userId)
                   // .withsdkVersion()              
                    .build();
           
            IDictionary<string, dynamic> map = requestParams.removeNullValues(requestParams);
          
        
           
            return map;
        }


        //Batch Goal Events


        public static IDictionary<string, dynamic> getBatchEventForTrackingGoal(long accountId, int campaignId, int variationId, string userId,
            int goalId,  string revenueValue, bool isDevelopmentMode)
        {
           

            BuildQueryParams requestParams = BuildQueryParams.Builder.getInstance()
                .withMinifiedCampaignId(campaignId)
                .withMinifiedVariationId(variationId)
                .withMinifiedEventType(2)
                .withMinifiedGoalId(goalId)
                // .withRevenue(revenueValue)
                .withSid(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                .withUuid(accountId, userId)
               // .withsdkVersion()
                .build();
         
            IDictionary<string, dynamic> map = requestParams.removeNullValues(requestParams);       
            return map;
        }



        //Batch Push Events

        public static IDictionary<string, dynamic> getBatchEventForPushTags(long accountId, string tagKey, string tagValue, string userId, bool isDevelopmentMode)
        {

            BuildQueryParams requestParams =
            BuildQueryParams.Builder.getInstance()
                    .withMinifiedEventType(3)
                    .withMinifiedTags(tagKey, tagValue)
                    .withSid(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    .withUuid(accountId, userId)
                    .build();
            IDictionary<string, dynamic> map = requestParams.removeNullValues(requestParams);
            return map;
        }


        

        //Generate Event Batching Post Call Params
        public static string getBatchEventPostCallParams( Queue<IDictionary<string, dynamic>> properties )
        {
            string jsonString = JsonConvert.SerializeObject(properties);
            jsonString = "{\"ev\":" + jsonString + "}";

            return jsonString;

        }

    }
  
  


}