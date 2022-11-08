using Runly.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;
using Timer = System.Timers.Timer;

namespace Runly.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Training : ContentPage
    {
        Timer timer;
        int hours = 0, mins = 0, secs = 0;

        bool isTraining = false;
        DateTime startDate;
        int trainingIndex;

        Position position;
        //Pin currentLocation;

        List<PositionList> positionsList = new List<PositionList>();
        double way = 0;
        double tempo = 0;
        double caloriesBurned = 0;
        double avrSpeed = 0;

        private readonly SQLiteAsyncConnection _database;
        private SQLiteAsyncConnection _databaseTraining;



        public Training()
        {
            InitializeComponent();

            _database = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "trainingHistory.db3"));
            _database.CreateTableAsync<TrainingData>();
            //_database.DeleteAllAsync<TrainingData>();

            GetLocation();
        }

        private async void GetLocation()
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best);
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {

                //map.Pins.Remove(currentLocation);
                position = new Position(location.Latitude, location.Longitude);
                /*currentLocation = new Pin
                {
                    Label = "Current Location",
                    Type = PinType.Generic,
                    Position = position
                };
                map.Pins.Add(currentLocation);*/
                map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(0.5)));

                if (isTraining)
                {
                    positionsList.Add(new PositionList { Location = location, TimeLasted = (hours * 3600 + mins * 60 + secs) });
                    await SaveCurrentData(new CurrentData
                    {
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        TimeLasted = (hours * 3600 + mins * 60 + secs)
                    });
                    //positionsList.Add(new PositionList { Latitude = location.Latitude, Longitude = location.Longitude, TimeLasted = (hours * 3600 + mins * 60 + secs) });
                    UpdateInfo();
                }

            }

            await Task.Delay(2000);
            GetLocation();

        }

        private void StartTraining(object sender, EventArgs e)
        {
            btnStartF.IsVisible = false;
            btnResumeF.IsVisible = false;
            btnEndF.IsVisible = false;
            btnStopF.IsVisible = true;
            isTraining = true;
            startDate = DateTime.Now;

            _databaseTraining = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "training" + startDate.ToString("dd_MM_yyyy_HH_mm_ss") + ".db3"));
            _databaseTraining.CreateTableAsync<CurrentData>();

            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void ResumeTraining(object sender, EventArgs e)
        {
            btnStartF.IsVisible = false;
            btnResumeF.IsVisible = false;
            btnEndF.IsVisible = false;
            btnStopF.IsVisible = true;
            isTraining = true;
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            secs++;
            if (secs == 59)
            {
                mins++;
                secs = 0;
            }
            if (mins == 59)
            {
                hours++;
                mins = 0;
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                timerValue.Text = string.Format("{0:00}:{1:00}:{2:00}", hours, mins, secs);
            });
        }

        private void StopTraining(object sender, EventArgs e)
        {
            timer.Stop();
            timer = null;
            btnEndF.IsVisible = true;
            btnResumeF.IsVisible = true;
            btnStopF.IsVisible = false;
            isTraining = false;
        }

        private async void EndTraining(object sender, EventArgs e)
        {
            btnResumeF.IsVisible = false;
            btnEndF.IsVisible = false;
            btnStopF.IsVisible = false;
            btnStartF.IsVisible = true;

            await SaveTrainingData(new TrainingData
            {
                DateDay = startDate.ToString("dd/MM/yyyy"),
                DateTime = startDate.ToString("HH:mm"),
                Time = string.Format("{0:00}:{1:00}", (hours * 60 + mins), secs),
                Distance = way.ToString() + " km",
                Calories = caloriesBurned,
                AvrSpeed = avrSpeed, 
                TrainingDatabase = "training" + startDate.ToString("dd_MM_yyyy_HH_mm_ss") + ".db3"
            });

            await Navigation.PushAsync(new StatisticsView(CountTrainings().Result.ToString()));

            way = 0;
            tempo = 0;
            caloriesBurned = 0;
            timerValue.Text = "00:00:00";
            amountDistance.Text = "0";
            amountCalories.Text = "0";
            amountSpeed.Text = "0.0";
            hours = 0;
            mins = 0;
            secs = 0;
            map.MapElements.Clear();
        }

        private void UpdateInfo()
        {
            Polyline polyline = new Polyline();
            polyline.StrokeColor = Color.FromHex("#192126");
            polyline.StrokeWidth = 20;
            int length = positionsList.Count - 1;
            double tmpWay;
            if (length > 1)
            {
                polyline.Geopath.Add(new Position(positionsList[length - 1].Location.Latitude, positionsList[length - 1].Location.Longitude));
                polyline.Geopath.Add(new Position(positionsList[length].Location.Latitude, positionsList[length].Location.Longitude));
                //polyline.Geopath.Add(new Position(positionsList[length - 1].Latitude, positionsList[length - 1].Longitude));
                //polyline.Geopath.Add(new Position(positionsList[length].Latitude, positionsList[length].Longitude));
                map.MapElements.Add(polyline);
                tmpWay = Location.CalculateDistance(positionsList[length].Location, positionsList[length - 1].Location, DistanceUnits.Kilometers);
                    //Math.Sqrt(((positionsList[length].Latitude - positionsList[length - 1].Latitude) * (positionsList[length].Latitude - positionsList[length - 1].Latitude)) + ((Math.Cos(positionsList[length - 1].Latitude * Math.PI / 180) * (positionsList[length].Longitude - positionsList[length - 1].Longitude)) * (Math.Cos(positionsList[length - 1].Latitude * Math.PI / 180) * (positionsList[length].Longitude - positionsList[length - 1].Longitude)))) * (40075.704 / 360);
                way = Math.Round((way + tmpWay), 3);
                tempo = (tmpWay / (positionsList[length].TimeLasted - positionsList[length - 1].TimeLasted)) * 3600;
                tempo = Math.Round(tempo, 1);
            }


            if (way < 1)
            {
                amountDistance.Text = (way * 1000).ToString();
                distanceSize.Text = " m";
            }
            else if (way < 10)
            {
                amountDistance.Text = string.Format("{0:0.00}", way);
                distanceSize.Text = " km";
            }
            else if (way < 100)
            {
                amountDistance.Text = string.Format("{0:00.0}", way);
                distanceSize.Text = " km";
            }
            else
            {
                amountDistance.Text = string.Format("{0:000}", way);
                distanceSize.Text = " km";
            }

            if (tempo < 10)
                amountSpeed.Text = string.Format("{0:0.0}", tempo);
            else
                amountSpeed.Text = string.Format("{0:00}", tempo);

            amountCalories.Text = caloriesBurned.ToString();
        }


        public Task<int> SaveTrainingData(TrainingData trainingData)      
        {
            return _database.InsertAsync(trainingData);
        }
        
        public Task<int> CountTrainings()
        {
            return _database.Table<TrainingData>().CountAsync();
        }

        public Task<int> SaveCurrentData(CurrentData currentPosition)
        {
            return _databaseTraining.InsertAsync(currentPosition);
        }

    }
}