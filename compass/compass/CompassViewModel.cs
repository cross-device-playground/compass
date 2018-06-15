using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace compass
{
    public class CompassViewModel : MvvmHelpers.BaseViewModel
    {

        public CompassViewModel()
        {
            this.StopCommand = new Command(Stop);
            this.StartCommand = new Command(Start);
        }

        // mobile device sensor data
        public struct MobileSensingData
        {
            public string ID;
            public string DeviceName;
            public double Timestamp;
            public float Compass;
            public float Accelerometer_x;
            public float Accelerometer_y;
            public float Accelerometer_z;
            public float LinearAccelerometer_x;
            public float LinearAccelerometer_y;
            public float LinearAccelerometer_z;
            public float Gyroscope_x;
            public float Gyroscope_y;
            public float Gyroscope_z;
            public float Magnetometer_x;
            public float Magnetometer_y;
            public float Magnetometer_z;
        }

        MobileSensingData _sensingData = new MobileSensingData();
        private readonly object _sensingDataLock = new object();
        private HttpClient _deviceRESTClient = new HttpClient();
        private string _remoteRESTAddress = "http://10.42.0.1:5566/mobile/sensing/";

        public Command StopCommand { get; }


        void Stop()
        {
            if (Compass.IsMonitoring)
            {
                Compass.ReadingChanged -= Compass_ReadingChanged;
                Compass.Stop();
            }

            if (Accelerometer.IsMonitoring)
            {
                Accelerometer.ReadingChanged -= Accelerometer_Essentials_ReadingChanged;
                Accelerometer.Stop();
            }

            if (Gyroscope.IsMonitoring)
            {
                Gyroscope.ReadingChanged -= Gyroscope_ReadingChanged;
                Gyroscope.Stop();
            }

            if (Magnetometer.IsMonitoring)
            {
                Magnetometer.ReadingChanged -= Magnetometer_ReadingChanged;
                Magnetometer.Stop();
            }

            // device-specific sensor readings
            if (Device.RuntimePlatform.Equals((Device.Android)))
            {
                MessagingCenter.Unsubscribe<Sensors.Android.LinearAcceleration>(this, Sensors.Android.SubscriberMessage.LinearAcceleration);
            }
        }

        public Command StartCommand { get; }

        void Start()
        {
            this._sensingData.ID = "charlie";
            this._sensingData.DeviceName = DeviceInfo.Name;

            if (!Compass.IsMonitoring)
            {
                Compass.ReadingChanged += Compass_ReadingChanged;
                Compass.Start(SensorSpeed.Fastest);
            }

            if (!Accelerometer.IsMonitoring)
            {
                Accelerometer.ReadingChanged += Accelerometer_Essentials_ReadingChanged;
                Accelerometer.Start(SensorSpeed.Fastest);
            }

            if (!Gyroscope.IsMonitoring)
            {
                Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
                Gyroscope.Start(SensorSpeed.Fastest);
            }

            if (!Magnetometer.IsMonitoring)
            {
                Magnetometer.ReadingChanged += Magnetometer_ReadingChanged;
                Magnetometer.Start(SensorSpeed.Fastest);
            }

            // device-specific sensor readings
            if (Device.RuntimePlatform.Equals((Device.Android)))
            {
                MessagingCenter.Subscribe<Sensors.Android.LinearAcceleration>(this, Sensors.Android.SubscriberMessage.LinearAcceleration, (linear_acceleration) =>
                {
                    this.LinearAccelerometer_Android_ReadingChanged(linear_acceleration);
                });
            }
        }

        private void Compass_ReadingChanged(CompassChangedEventArgs e)
        {
            //HeadingDisplay = $"Compass: {e.Reading.HeadingMagneticNorth:F3}";
            lock (_sensingDataLock)
            {
                _sensingData.Timestamp = DateTime.UtcNow.Ticks;
                _sensingData.Compass = (float)e.Reading.HeadingMagneticNorth;
            }
        }

        private void LinearAccelerometer_Android_ReadingChanged(Sensors.Android.LinearAcceleration data)
        {
            lock (_sensingDataLock)
            {
                _sensingData.Timestamp = DateTime.UtcNow.Ticks / (double)(TimeSpan.TicksPerSecond);
                _sensingData.LinearAccelerometer_x = data.x;
                _sensingData.LinearAccelerometer_y = data.y;
                _sensingData.LinearAccelerometer_z = data.z;

                // hack
                POST_Sensing_Data();
            }
        }

        private void Accelerometer_Essentials_ReadingChanged(AccelerometerChangedEventArgs e)
        {
            //AccelerometerDisplay = $"Acceleromter: {e.Reading.Acceleration.X:F3}, {e.Reading.Acceleration.Y:F3}, {e.Reading.Acceleration.Z:F3}";
            lock (_sensingDataLock)
            {
                _sensingData.Timestamp = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
                _sensingData.Accelerometer_x = e.Reading.Acceleration.X;
                _sensingData.Accelerometer_y = e.Reading.Acceleration.Y;
                _sensingData.Accelerometer_z = e.Reading.Acceleration.Z;

                // hack
                //POST_Sensing_Data();
            }
        }

        private void Gyroscope_ReadingChanged(GyroscopeChangedEventArgs e)
        {
            //GyroscopeDisplay = $"Gyroscope: {e.Reading.AngularVelocity.X:F3}, {e.Reading.AngularVelocity.Y:F3}, {e.Reading.AngularVelocity.Z:F3}";
            lock (_sensingDataLock)
            {
                _sensingData.Timestamp = DateTime.UtcNow.Ticks;
                _sensingData.Gyroscope_x = e.Reading.AngularVelocity.X;
                _sensingData.Gyroscope_y = e.Reading.AngularVelocity.Y;
                _sensingData.Gyroscope_z = e.Reading.AngularVelocity.Z;
            }
        }

        private void Magnetometer_ReadingChanged(MagnetometerChangedEventArgs e)
        {
            //MagnetometerDisplay = $"Magnetometer: {e.Reading.MagneticField.X:F3}, {e.Reading.MagneticField.Y:F3}, {e.Reading.MagneticField.Z:F3}";
            lock (_sensingDataLock)
            {
                _sensingData.Timestamp = DateTime.UtcNow.Ticks;
                _sensingData.Magnetometer_x = e.Reading.MagneticField.X;
                _sensingData.Magnetometer_y = e.Reading.MagneticField.Y;
                _sensingData.Magnetometer_z = e.Reading.MagneticField.Z;
            }
        }

        // POST mobile device sensing data
        private async void POST_Sensing_Data()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this._sensingData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await this._deviceRESTClient.PostAsync(this._remoteRESTAddress, content);
                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("POST success!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("POST error {0}!", ex.Message));
            }
        }

        // UI
        private string _headingDisplay;
        public string HeadingDisplay
        {
            get => _headingDisplay;
            set => SetProperty(ref _headingDisplay, value);
        }

        private string _accelerometerDisplay;
        public string AccelerometerDisplay
        {
            get => _accelerometerDisplay;
            set => SetProperty(ref _accelerometerDisplay, value);
        }

        private string _gyroscopeDisplay;
        public string GyroscopeDisplay
        {
            get => _gyroscopeDisplay;
            set => SetProperty(ref _gyroscopeDisplay, value);
        }

        private string _magnetometerDisplay;
        public string MagnetometerDisplay
        {
            get => _magnetometerDisplay;
            set => SetProperty(ref _magnetometerDisplay, value);
        }
    }
}
