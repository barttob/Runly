using Runly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;

using Xamarin.Essentials;

using Map = Xamarin.Forms.Maps.Map;
using Button = Xamarin.Forms.Button;
using System.Timers;
using Polyline = Xamarin.Forms.Maps.Polyline;
using System.Threading;
using Timer = System.Timers.Timer;

namespace Runly.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Training : ContentPage
    {
        Timer timer;
        int hours = 0, mins = 0, secs = 0;
        Label txtTime = new Label { Text = "Czas", TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center, FontSize = 15, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 20, 0, 0) };
        Label txtTimer = new Label { Text="00:00:00", TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center, FontSize = 50, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 0, 0, 20) };
        Label distance = new Label { Text = "Dystans", TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center, FontSize = 15, FontAttributes = FontAttributes.Bold, Margin = 0 };
        Label speed = new Label { Text = "Prędkość", TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center, FontSize = 15, FontAttributes = FontAttributes.Bold, Margin = 0 };
        Label calories = new Label { Text = "Kalorie", TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center, FontSize = 15, FontAttributes = FontAttributes.Bold, Margin = 0 };
        Label amountDistance = new Label { Text = "0 m", TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center, FontSize = 30, FontAttributes = FontAttributes.Bold };
        Label amountSpeed = new Label { Text = "0.0 km/h", TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center, FontSize = 30, FontAttributes = FontAttributes.Bold };
        Label amountCalories = new Label { Text = "0 cal", TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center, FontSize = 30, FontAttributes = FontAttributes.Bold };

        Button btnStart = new Button { Text = "Start", IsVisible = true, WidthRequest = 50, HeightRequest = 50, CornerRadius = 5, BorderWidth = 2, BorderColor = Color.Gray, Margin = 5 };
        Button btnStop = new Button { Text = "Stop", IsVisible = false, WidthRequest = 50, HeightRequest = 50, CornerRadius = 5, BorderWidth = 2, BorderColor = Color.Gray, Margin = 5 };
        Button btnEnd = new Button { Text = "Koniec", IsVisible = false, WidthRequest = 50, HeightRequest = 50, CornerRadius = 5, BorderWidth = 2, BorderColor = Color.Gray, Margin = 5 };
        Button btnResume = new Button { Text = "Wznów", IsVisible = false, WidthRequest = 50, HeightRequest = 50, CornerRadius = 5, BorderWidth = 2, BorderColor = Color.Gray, Margin = 5 };


        Map map = new Map();

        bool isTraining = false;

        Position position;
        Pin currentLocation;

        List<PositionList> positionsList = new List<PositionList>();
        double way = 0;
        double tempo = 0;
        double caloriesBurned = 0;

        double weight = 70;


        Grid dataGrid = new Grid
        {
            RowSpacing = 0,
            ColumnSpacing = 0,
            Padding = 0,
            Margin = new Thickness(0, 0, 0, 20),
            RowDefinitions =
            {
                new RowDefinition { Height = 20 },
                new RowDefinition()
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(),
                new ColumnDefinition(),
                new ColumnDefinition()
            }
        };

        Grid btnGrid = new Grid
        {
            RowSpacing = 0,
            ColumnSpacing = 0,
            Padding = 0,
            Margin = new Thickness(5, 5, 5, 10),
            RowDefinitions =
            {
                new RowDefinition()
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(),
                new ColumnDefinition()
            }
        };



        public Training()
        {
            InitializeComponent();
                        
            btnStart.Clicked += StartTraining;
            btnStop.Clicked += StopTraining;
            btnResume.Clicked += StartTraining;
            btnEnd.Clicked += EndTraining;

            dataGrid.Children.Add(distance, 0, 0);
            dataGrid.Children.Add(speed, 1, 0);
            dataGrid.Children.Add(calories, 2, 0);
            dataGrid.Children.Add(amountDistance, 0, 1);
            dataGrid.Children.Add(amountSpeed, 1, 1);
            dataGrid.Children.Add(amountCalories, 2, 1);

            Grid.SetRow(btnStart, 0);
            Grid.SetColumnSpan(btnStart, 2);
            Grid.SetRow(btnStop, 0);
            Grid.SetColumnSpan(btnStop, 2);
            btnGrid.Children.Add(btnStart);
            btnGrid.Children.Add(btnStop);
            btnGrid.Children.Add(btnResume, 0, 0);
            btnGrid.Children.Add(btnEnd, 1, 0);


            Content = new StackLayout
            {
                Children =
                {
                    txtTime,
                    txtTimer,
                    dataGrid,
                    map,
                    btnGrid 
                }
            };

            GetLocation();

        }

        private async void GetLocation()
        {

            var request = new GeolocationRequest(GeolocationAccuracy.Best);
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                if (location.IsFromMockProvider)
                {
                    Console.WriteLine("Get location error");
                }
                else
                {
                    map.Pins.Remove(currentLocation);
                    position = new Position(location.Latitude, location.Longitude);
                    currentLocation = new Pin
                    {
                        Label = "Current Location",
                        Type = PinType.Generic,
                        Position = position
                    };
                    map.Pins.Add(currentLocation);
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(0.5)));
                    
                    if (isTraining) {
                        positionsList.Add(new PositionList { Latitude = location.Latitude, Longitude = location.Longitude, TimeLasted = (hours * 3600 + mins * 60 + secs) });
                        UpdateInfo();
                    }     
                }
            }

            GetLocation();
        }

        private void StartTraining(object sender, EventArgs e)
        {
            btnStart.IsVisible = false;
            btnResume.IsVisible = false;
            btnEnd.IsVisible = false;
            btnStop.IsVisible = true;
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
                txtTimer.Text = string.Format("{0:00}:{1:00}:{2:00}", hours, mins, secs);  
            });
        }

        private void StopTraining(object sender, EventArgs e)
        {
            timer.Stop();
            timer = null;
            btnEnd.IsVisible = true;
            btnResume.IsVisible = true;
            btnStop.IsVisible = false;
            isTraining = false;
        }

        private void EndTraining(object sender, EventArgs e)
        {
            btnResume.IsVisible = false;
            btnEnd.IsVisible = false;
            btnStop.IsVisible = false;
            btnStart.IsVisible = true;
            way = 0;
            tempo = 0;
            caloriesBurned = 0;
            txtTimer.Text = "00:00:00";
            amountDistance.Text = "0 m";
            amountCalories.Text = "0 cal";
            amountSpeed.Text = "0.0 km/h";
            hours = 0;
            mins = 0;
            secs = 0;
            map.MapElements.Clear();
        }

        private void UpdateInfo()
        {
            Polyline polyline = new Polyline();
            polyline.StrokeColor = Color.FromHex("#1BA1E2");
            polyline.StrokeWidth = 20;
            int length = positionsList.Count - 1;
            double tmpWay;
            if (length > 1) {
                polyline.Geopath.Add(new Position(positionsList[length - 1].Latitude, positionsList[length - 1].Longitude));
                polyline.Geopath.Add(new Position(positionsList[length].Latitude, positionsList[length].Longitude));
                map.MapElements.Add(polyline);
                tmpWay = Math.Sqrt(((positionsList[length].Latitude - positionsList[length - 1].Latitude) * (positionsList[length].Latitude - positionsList[length - 1].Latitude)) + ((Math.Cos(positionsList[length - 1].Latitude * Math.PI / 180) * (positionsList[length].Longitude - positionsList[length - 1].Longitude)) * (Math.Cos(positionsList[length - 1].Latitude * Math.PI / 180) * (positionsList[length].Longitude - positionsList[length - 1].Longitude)))) * (40075.704 / 360);
                way = Math.Round((way + tmpWay), 3);
                tempo = (tmpWay / (positionsList[length].TimeLasted - positionsList[length - 1].TimeLasted)) * 3600;
            }
            tempo = Math.Round(tempo, 1);
            caloriesBurned += 8 * weight * tempo * 1.62 / 200;
            caloriesBurned = Math.Round(caloriesBurned, 0);

            if (way < 1)
                amountDistance.Text = (way * 1000).ToString() + " m";
            else if(way < 10)
                amountDistance.Text = string.Format("{0:0.00} km", way);
            else if (way < 100)
                amountDistance.Text = string.Format("{0:00.0} km", way);
            else
                amountDistance.Text = string.Format("{0:000} km", way);

            if (tempo < 10)
                amountSpeed.Text = string.Format("{0:0.0} km/h", tempo);
            else
                amountSpeed.Text = string.Format("{0:00} km/h", tempo);

            amountCalories.Text = caloriesBurned.ToString() + " cal";
        }   
    }
}