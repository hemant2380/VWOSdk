namespace VWOSdk
{
    internal enum EVENT_TYPES
    {
        TRACK_USER = 1,
        TRACK_GOAL = 2,
        PUSH = 3

    }


    public class BatchEventData
    {
        internal int? eventsPerRequest;
        internal int? requestTimeInterval;
       
        internal IFlushInterface flushCallback;
       
        public  int? EventsPerRequest
        {
            get
            {
                return eventsPerRequest;
            }
            set
            {
                this.eventsPerRequest = value;
            }
        }

       
        public  int? RequestTimeInterval
        {
            get
            {
                return requestTimeInterval;
            }
            set
            {
                this.requestTimeInterval = value;
            }
        }
      

        public  IFlushInterface FlushCallback
        {
            get
            {
                return flushCallback;
            }
            set
            {
                this.flushCallback = value;
            }
        }

        internal EVENT_TYPES _Eventtype { get; private set; }
        
    }

}