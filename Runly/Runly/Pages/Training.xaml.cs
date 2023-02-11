using Runly.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
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

        Position position;


        List<PositionList> positionsList = new List<PositionList>();

        double weight = Int16.Parse(Preferences.Get("Waga", "70"));
        double way = 0;
        double tempo = 0;
        double caloriesBurned = 0;
        double avrSpeed = 0;
        double speedSum = 0;
        int steps = 0;
        int startSteps = 0;

        private readonly SQLiteAsyncConnection _database;
        private SQLiteAsyncConnection _databaseTraining;



        public Training()
        {
            InitializeComponent();
            
            //Połączenie z bazą danych
            _database = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "trainingHistory.db3"));
            _database.CreateTableAsync<TrainingData>();

            MessagingCenter.Subscribe<string>(this, "steps", message =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    steps = Int16.Parse(message);
                });
            });

            GetLocation();
        }

        //Funkcja pobierająca lokalizację
        private async void GetLocation()
        {
            //Pobranie lokalizacji
            var request = new GeolocationRequest(GeolocationAccuracy.Best);
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                position = new Position(location.Latitude, location.Longitude);
                
                //Przesunięcie widoku na lokalizację przeciwnika
                map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(0.5)));

                if (isTraining)
                {   
                    //Zapis danych lokalizacji
                    positionsList.Add(new PositionList { Location = location, TimeLasted = (hours * 3600 + mins * 60 + secs) });
                    await SaveCurrentData(new CurrentData
                    {
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        TimeLasted = (hours * 3600 + mins * 60 + secs)
                    });
                    UpdateInfo();
                }

            }

            //Pobranie lokalizacji wywoływane co 2 sekundy
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
            startSteps = steps;

            //Baza danych lokalizacji w treningu
            _databaseTraining = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "training" + startDate.ToString("dd_MM_yyyy_HH_mm_ss") + ".db3"));
            _databaseTraining.CreateTableAsync<CurrentData>();

            //Inicjalizacja timera
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

        //Funkcja licząca sekundy, wyłwoywana co sekundę
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

            //Zapis danych treningu do głównej bazy danych
            await SaveTrainingData(new TrainingData
            {
                DateDay = startDate.ToString("dd/MM/yyyy"),
                DateTime = startDate.ToString("HH:mm"),
                Time = string.Format("{0:00}:{1:00}", (hours * 60 + mins), secs),
                Distance = way.ToString() + " km",
                Calories = caloriesBurned,
                AvrSpeed = avrSpeed, 
                Steps = steps - startSteps,
                TrainingDatabase = "training" + startDate.ToString("dd_MM_yyyy_HH_mm_ss") + ".db3"
            });

            //Otwarcie strony podsumowania treningu
            await Navigation.PushAsync(new StatisticsView(CountTrainings().Result.ToString()));

            //Wyzerowanie zmiennych pomiarowych
            way = 0;
            tempo = 0;
            caloriesBurned = 0;
            avrSpeed = 0;
            speedSum = 0;
            timerValue.Text = "00:00:00";
            amountDistance.Text = "0";
            amountCalories.Text = "0";
            amountSpeed.Text = "0.0";
            startSteps = 0;
            hours = 0;
            mins = 0;
            secs = 0;
            map.MapElements.Clear();
            MessagingCenter.Send("0", "trainingSteps");
        }

        //Funkcja aktualizująca informacje na ekranie
        private void UpdateInfo()
        {
            //Polyline rysuje trasę na mapie
            Polyline polyline = new Polyline();
            polyline.StrokeColor = Color.FromHex("#192126");
            polyline.StrokeWidth = 20;
            int length = positionsList.Count - 1;
            double tmpWay;
            if (length > 1)
            {
                //Ustawienie lini na dwie ostanie pozycje
                polyline.Geopath.Add(new Position(positionsList[length - 1].Location.Latitude, positionsList[length - 1].Location.Longitude));
                polyline.Geopath.Add(new Position(positionsList[length].Location.Latitude, positionsList[length].Location.Longitude));
                //Wywołanie lini na mapie
                map.MapElements.Add(polyline);

                //Obliczeia parametrów treningu
                tmpWay = Location.CalculateDistance(positionsList[length].Location, positionsList[length - 1].Location, DistanceUnits.Kilometers);
                way = Math.Round((way + tmpWay), 3);
                tempo = (tmpWay / (positionsList[length].TimeLasted - positionsList[length - 1].TimeLasted)) * 3600;
                tempo = Math.Round(tempo, 1);
                speedSum += tempo * (positionsList[length].TimeLasted - positionsList[length - 1].TimeLasted);
                avrSpeed = speedSum / (positionsList[length].TimeLasted);
                avrSpeed = Math.Round(avrSpeed, 1);
                //kalorie na minute = (MET * 3.5 * waga) / 200
                //MET - ile razy więcej kalori spala przy danej aktywności w porówaniu do odpoczynku
                caloriesBurned = ((avrSpeed * 3.5 * weight) / 200) * (((double)positionsList[length].TimeLasted) / 60);
                caloriesBurned = Math.Round(caloriesBurned, 1);
            }
            
            //Wyświetlenie dystansu w określonym formacie zależnym od dystansu
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

            //Wyświetlenie prędkości w określonym formacie
            if (tempo < 10)
                amountSpeed.Text = string.Format("{0:0.0}", tempo);
            else
                amountSpeed.Text = string.Format("{0:00}", tempo);

            if (caloriesBurned < 10)
                amountCalories.Text = caloriesBurned.ToString();
            else
                amountCalories.Text = string.Format("{0:00}", caloriesBurned);

            MessagingCenter.Send((steps - startSteps).ToString(), "trainingSteps");
        }

        //Zapisanie danych do bazy danych
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