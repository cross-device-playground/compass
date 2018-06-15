using System;
using System.Collections.Generic;
using System.Text;

namespace compass
{
    namespace Sensors
    {
        public class Android
        {
            public class SubscriberMessage
            {
                public static readonly string LinearAcceleration = "Sensors.Android.LinearAcceleration";
            }

            public class LinearAcceleration
            {
                public float x;
                public float y;
                public float z;
                public long timestamp;
                public string accuracy;

                public LinearAcceleration()
                {
                    this.accuracy = "Empty";
                }

                public LinearAcceleration(float _x, float _y, float _z, long _timestamp, string _accuracy)
                {
                    this.x = _x;
                    this.y = _y;
                    this.z = _z;
                    this.timestamp = _timestamp;
                    this.accuracy = _accuracy;
                }
            }
        }
    }
}
