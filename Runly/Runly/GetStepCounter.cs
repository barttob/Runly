using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Runly
{
    public class GetStepCounter
    {
        List<double> accData = new List<double>();
        int stepsNumber = 0;
        DateTime czas = DateTime.Now;
        TimeSpan interval = TimeSpan.FromSeconds(0.1);

        readonly bool stopping = false;

        public GetStepCounter()
        {
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            //if (!Accelerometer.IsMonitoring)
            //    Accelerometer.Start(SensorSpeed.UI);
        }

        public async Task Run(CancellationToken token)
        {
            await Task.Run(async () => {
                while (!stopping)
                {
                    
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        //Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
                        if (!Accelerometer.IsMonitoring)
                            Accelerometer.Start(SensorSpeed.UI);

                    }
                    catch (Exception ex)
                    {
                       
                    }
                }
                return;
            }, token);
        }

        public void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs args)
        {
            if (DateTime.Now - czas > interval)
            {
                /*stepsNumber++;
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(stepsNumber.ToString(), "steps");
                });*/
                czas = DateTime.Now;
                var xVal = args.Reading.Acceleration.X * 10;
                var yVal = args.Reading.Acceleration.Y * 10;
                var zVal = args.Reading.Acceleration.Z * 10;
                var accValue = Math.Round(Math.Sqrt(xVal * xVal + yVal * yVal + zVal * zVal) - 9.8, 2);
                if (accData.Count > 100)
                    accData.RemoveAt(0);

                accData.Add(accValue);
                
                if (accData.Count > 1)
                {
                    int counter = 0;
                    for (int i = 1; i < accData.Count - 1; i++)
                    { 
                        if (accData[i] > 2 && accData[i] > accData[i - 1] && accData[i] > accData[i + 1])
                        {
                            counter++;
                            if (counter >= 6)
                            {
                                stepsNumber += 6;
                                accData.Clear();
                                counter = 0;
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    MessagingCenter.Send(stepsNumber.ToString(), "steps");
                                });
                            }
                        }
                    }
                }
            }
        }
    }
}
