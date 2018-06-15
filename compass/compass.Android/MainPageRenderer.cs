using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using compass;
using compass.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MainPage), typeof(MainPageRenderer))]
namespace compass.Droid
{
    [Activity(Label = "MainPageRenderer")]
    public class MainPageRenderer : PageRenderer, ISensorEventListener
    {

        global::Android.Hardware.SensorManager sensorManager;
        global::Android.Hardware.Sensor sensorLinearAccleration;

        public MainPageRenderer(Context context) : base(context)
        {
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            Sensors.Android.LinearAcceleration linear_acceleration = new Sensors.Android.LinearAcceleration
            {
                x = e.Values[0],
                y = e.Values[1],
                z = e.Values[2],
                timestamp = e.Timestamp,
                accuracy = e.Accuracy.ToString()
            };
            MessagingCenter.Send<Sensors.Android.LinearAcceleration>(linear_acceleration, Sensors.Android.SubscriberMessage.LinearAcceleration);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
            sensorManager = (SensorManager)Context.GetSystemService(Android.Content.Context.SensorService);
            sensorLinearAccleration = sensorManager.GetDefaultSensor(Android.Hardware.SensorType.LinearAcceleration);
            sensorManager.RegisterListener(this, sensorLinearAccleration, SensorDelay.Fastest);
        }
    }
}