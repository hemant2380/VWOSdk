using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace VWOSdk
{

    public class BatchEventQueue
    {
        public const int MAX_EVENTS_PER_REQUEST = 5000;

        internal Queue<IDictionary<string, dynamic>> batchQueue = new Queue<IDictionary<string, dynamic>>();
        private static readonly Dictionary<string, int> queueMetaData = new Dictionary<string, int>();
        internal int requestTimeInterval = 600; //default:- 10 * 60(secs) = 600 secs i.e. 10 minutes
        internal int eventsPerRequest = 100; //default
        internal IFlushInterface flushCallback;
        internal Timer timer;
        private readonly int accountId;
        private readonly string apikey;
        private bool isDevelopmentMode;
        private bool isBatchProcessing = false;

        private static readonly string file = typeof(BatchEventQueue).FullName;


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public BatchEventQueue(BatchEventData batchEvents, string apikey, int accountId, bool isDevelopmentMode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {

            if (batchEvents != null)
            {
                if (batchEvents.RequestTimeInterval > 1 && batchEvents.RequestTimeInterval!=null)
                {
                    this.requestTimeInterval = (int)batchEvents.RequestTimeInterval;
                }
                else
                {
                    LogDebugMessage.RequestTimeIntervalOutOfBound(file, 1, batchEvents.RequestTimeInterval ==null ? 
                        0:(int)batchEvents.RequestTimeInterval);

                }

                if (batchEvents.EventsPerRequest > 0 && batchEvents.EventsPerRequest <= MAX_EVENTS_PER_REQUEST 
                    && batchEvents.EventsPerRequest !=null)
                {
                    this.eventsPerRequest = Math.Min((int)batchEvents.EventsPerRequest, MAX_EVENTS_PER_REQUEST);
                }
                else
                {
                    LogDebugMessage.EventsPerRequestOutOfBound(file, 1, MAX_EVENTS_PER_REQUEST, eventsPerRequest);


                }

                if (batchEvents.FlushCallback != null)
                {
                    this.flushCallback = batchEvents.FlushCallback;
                }

            }



            this.accountId = accountId;
            this.isDevelopmentMode = isDevelopmentMode;
            this.apikey = apikey;
        }



        /// <summary>
        /// Add Events In a Queue Memory.
        /// </summary>
        /// <param name="@event">Collection value returns from getBatchEventForTrackingGoal .</param>
        public void addInQueue(IDictionary<string, dynamic> @event)
        {
            if (isDevelopmentMode)
            {
                return;
            }

            batchQueue.Enqueue(@event);

            if (@event.ContainsKey("eT"))
            {
                int eT = @event.ContainsKey("eT") ? (int)@event["eT"] : 0;
                addEventCount(eT);
            }


            if (timer == null)
            {
                createNewBatchTimer();
            }
            if (eventsPerRequest == batchQueue.Count)
            {
                flush(true);
            }
        }

        /// <summary>
        /// When queue data successfully posted in the Uri, flush method is called . This method initializes the timer and events queue. 
        /// </summary>
        /// <param name="manual">flash method can be called manually . If called manually , set it as true else false</param>
        public bool flush(bool manual)
        {
            if (batchQueue.Count == 0)
            {

            }
            if (batchQueue.Count > 0 && !isBatchProcessing)
            {
                isBatchProcessing = true;

                Task<bool> response = sendPostCall(manual);
                disposeData();
                return response.Result;
            }
            clearRequestTimer();
            return true;
        }

        public void createNewBatchTimer()
        {
            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = requestTimeInterval * 1000L;
            timer.Enabled = true;
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            flush(true);
        }

        public void clearRequestTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
                isBatchProcessing = false;
            }
        }


        public bool flushAndClearInterval()
        {
            disposeData();
            return true;
        }


        /// <summary>
        /// Async Post Called When RequestTimeInterval or EventsPerRequest Satisfied.
        /// </summary>
        /// <param name="sendAsyncRequest">Accept true or false . Send a POST request as an asynchronous operation </param>
        private async Task<bool> sendPostCall(bool sendAsyncRequest)
        {
            if (sendAsyncRequest)
            {
                try
                {

                    string PayLoad = HttpRequestBuilder.getBatchEventPostCallParams(this.batchQueue);
                    var ApiRequest = ServerSideVerb.EventBatchingUri(this.accountId, this.isDevelopmentMode);

                    HttpClient httpClient = new HttpClient();

                    httpClient.DefaultRequestHeaders.Add("Authorization", this.apikey);

                    var data = new StringContent(PayLoad, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(ApiRequest.Uri, data);
                    response.EnsureSuccessStatusCode();



                    if (response.StatusCode == System.Net.HttpStatusCode.OK && response.StatusCode < System.Net.HttpStatusCode.Ambiguous)
                    {

                        if (flushCallback != null)
                        {
                            flushCallback.onFlush("Valid call", PayLoad);
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.RequestEntityTooLarge)
                    {

                        if (flushCallback != null)
                        {
                            flushCallback.onFlush("Payload size too large", PayLoad);
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {

                        if (flushCallback != null)
                        {
                            flushCallback.onFlush("Account id not found, no request app id found, or invalid API key", PayLoad);
                        }
                    }
                    else
                    {

                        if (flushCallback != null)
                        {
                            flushCallback.onFlush("Invalid call", PayLoad);
                        }
                    }


                    return ReferenceEquals(response.StatusCode, null);

                }
                catch (HttpRequestException)
                {

                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Count RequestTimeInterval or EventsPerRequest.
        /// </summary>
        /// <param name="eventType">Track=1 , Goal=2 , Push=3 </param>

        private void addEventCount(int eventType)
        {
            if (eventType == (int)EVENT_TYPES.TRACK_USER)
            {
                if (queueMetaData.ContainsKey("visitorEvents"))
                {
                    int visitorEvents = queueMetaData.ContainsKey("visitorEvents") ? queueMetaData["visitorEvents"] : 0;
                    queueMetaData["visitorEvents"] = visitorEvents + 1;
                }
                else
                {
                    queueMetaData["visitorEvents"] = 1;
                }
            }
            if (eventType == (int)EVENT_TYPES.TRACK_GOAL)
            {
                if (queueMetaData.ContainsKey("goalEvents"))
                {
                    int goalEvents = queueMetaData.ContainsKey("goalEvents") ? queueMetaData["goalEvents"] : 0;
                    queueMetaData["goalEvents"] = goalEvents + 1;

                }
                else
                {
                    queueMetaData["goalEvents"] = 1;
                }
            }
            if (eventType == (int)EVENT_TYPES.PUSH)
            {
                if (queueMetaData.ContainsKey("pushEvents"))
                {
                    int pushEvents = queueMetaData.ContainsKey("pushEvents") ? queueMetaData["pushEvents"] : 0;
                    queueMetaData["pushEvents"] = pushEvents + 1;

                }
                else
                {
                    queueMetaData["pushEvents"] = 1;
                }
            }
        }

        private void disposeData()
        {
            batchQueue.Clear();
            queueMetaData.Clear();
            clearRequestTimer();
        }

        public Queue<IDictionary<string, dynamic>> BatchQueue()
        {
           
                return this.batchQueue;
            
        }
        public int BatchQueueCount()
        {

            return this.batchQueue.Count;

        }


    }
}