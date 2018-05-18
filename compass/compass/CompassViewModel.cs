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
        public struct MobileSensorData
        {
            public string ID { get; set; }
            public float Compass { get; set; }
            public float AccelerometerX { get; set; }
            public float AccelerometerY { get; set; }
            public float AccelerometerZ { get; set; }
            public float GyroscopeX { get; set; }
            public float GyroscopeY { get; set; }
            public float GyroscopeZ { get; set; }
            public float MagnetometerX { get; set; }
            public float MagnetometerY { get; set; }
            public float MagnetometerZ { get; set; }
        }

        private List<MobileSensorData> _mobileSensorDataStore = new List<MobileSensorData> { };
        private HttpClient _mobileSensorRESTClient = new HttpClient();
        private string _mobileSensorRESTAddress = "0.0.0.0:5566/mobile/sensor";

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
                Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
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
        }

        public Command StartCommand { get; }

        void Start()
        {
            if (!Compass.IsMonitoring)
            {
                Compass.ReadingChanged += Compass_ReadingChanged;
                Compass.Start(SensorSpeed.Fastest);
            }

            if (!Accelerometer.IsMonitoring)
            {
                Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
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
        }

        private void Compass_ReadingChanged(CompassChangedEventArgs e)
        {
            HeadingDisplay = $"Compass: {e.Reading.HeadingMagneticNorth:F3}";
            MobileSensorData data = new MobileSensorData
            {
                Compass = (float) e.Reading.HeadingMagneticNorth
            };
            this._mobileSensorDataStore.Add(data);
        }

        private void Accelerometer_ReadingChanged(AccelerometerChangedEventArgs e)
        {
            AccelerometerDisplay = $"Acceleromter: {e.Reading.Acceleration.X:F3}, {e.Reading.Acceleration.Y:F3}, {e.Reading.Acceleration.Z:F3}";
            MobileSensorData data = new MobileSensorData
            {
                AccelerometerX = e.Reading.Acceleration.X,
                AccelerometerY = e.Reading.Acceleration.Y,
                AccelerometerZ = e.Reading.Acceleration.Z
            };
            this._mobileSensorDataStore.Add(data);

            // hack
            //POST_SENSOR_DATA();
        }

        private void Gyroscope_ReadingChanged(GyroscopeChangedEventArgs e)
        {
            GyroscopeDisplay = $"Gyroscope: {e.Reading.AngularVelocity.X:F3}, {e.Reading.AngularVelocity.Y:F3}, {e.Reading.AngularVelocity.Z:F3}";
            MobileSensorData data = new MobileSensorData
            {
                GyroscopeX = e.Reading.AngularVelocity.X,
                GyroscopeY = e.Reading.AngularVelocity.Y,
                GyroscopeZ = e.Reading.AngularVelocity.Z
            };
            this._mobileSensorDataStore.Add(data);
        }

        private void Magnetometer_ReadingChanged(MagnetometerChangedEventArgs e)
        {
            MagnetometerDisplay = $"Magnetometer: {e.Reading.MagneticField.X:F3}, {e.Reading.MagneticField.Y:F3}, {e.Reading.MagneticField.Z:F3}";
            MobileSensorData data = new MobileSensorData
            {
                MagnetometerX = e.Reading.MagneticField.X,
                MagnetometerY = e.Reading.MagneticField.Y,
                MagnetometerZ = e.Reading.MagneticField.Z
            };
            this._mobileSensorDataStore.Add(data);
        }

        // POST mobile device sensor data
        private async void POST_SENSOR_DATA()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this._mobileSensorDataStore[0]);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await this._mobileSensorRESTClient.PostAsync(this._mobileSensorRESTAddress, content);

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine(@"POST SUCCESS!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"ERROR {0}!", ex.Message);
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
