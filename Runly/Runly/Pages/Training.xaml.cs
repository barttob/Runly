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
using System.Globalization;

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
        Label amountDistance = new Label { Text = "000 m", TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center, FontSize = 30, FontAttributes = FontAttributes.Bold };
        Label amountSpeed = new Label { Text = "0 km/h", TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center, FontSize = 30, FontAttributes = FontAttributes.Bold };
        Label amountCalories = new Label { Text = "0 cal", TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center, FontSize = 30, FontAttributes = FontAttributes.Bold };

        Button btnStart = new Button { Text = "Start", IsVisible = true, WidthRequest = 50, HeightRequest = 50, CornerRadius = 5, BorderWidth = 2, BorderColor = Color.Gray, Margin = 5 };
        Button btnStop = new Button { Text = "Stop", IsVisible = false, WidthRequest = 50, HeightRequest = 50, CornerRadius = 5, BorderWidth = 2, BorderColor = Color.Gray, Margin = 5 };
        Button btnRestart = new Button { Text = "Restart", IsVisible = false, WidthRequest = 50, HeightRequest = 50, CornerRadius = 5, BorderWidth = 2, BorderColor = Color.Gray, Margin = 5 };
        Button btnResume = new Button { Text = "Wznów", IsVisible = false, WidthRequest = 50, HeightRequest = 50, CornerRadius = 5, BorderWidth = 2, BorderColor = Color.Gray, Margin = 5 };


        Map map = new Map();

        bool isTraining = false;

        Position position;
        Pin currentLocation;

        List<PositionList> positionsList = new List<PositionList>();
        double way = 0;



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
            btnRestart.Clicked += RestartTraining;

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
            btnGrid.Children.Add(btnRestart, 1, 0);


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

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                GetLocation();
                return true;
            });
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
                    //Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    map.Pins.Remove(currentLocation);
                    position = new Position(location.Latitude, location.Longitude);
                    currentLocation = new Pin
                    {
                        Label = "Current Location",
                        Type = PinType.Generic,
                        Position = position
                    };
                    map.Pins.Add(currentLocation);
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(0.2)));
                    
                    if (isTraining) {
                        positionsList.Add(new PositionList { Latitude = location.Latitude, Longitude = location.Longitude });
                        UpdateInfo();
                    }     
                }
            }

        }

        private void StartTraining(object sender, EventArgs e)
        {
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            btnStart.IsVisible = false;
            btnResume.IsVisible = false;
            btnRestart.IsVisible = false;  
            btnStop.IsVisible = true;
            isTraining = true;
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
            btnRestart.IsVisible = true;
            btnResume.IsVisible = true;
            btnStop.IsVisible = false;
            isTraining = false;
        }

        private void RestartTraining(object sender, EventArgs e)
        {
            btnResume.IsVisible = false;
            btnRestart.IsVisible = false;
            btnStop.IsVisible = false;
            btnStart.IsVisible = true;
            way = 0;
            txtTimer.Text = "00:00:00";
            amountDistance.Text = "000 m";
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
            foreach (var pos in positionsList) { 
                polyline.Geopath.Add(new Position(pos.Latitude, pos.Longitude));
            }
            map.MapElements.Add(polyline);
            int length = positionsList.Count - 1;
            if (length > 1) { 
                way += Math.Sqrt(((positionsList[length].Latitude - positionsList[length - 1].Latitude) * (positionsList[length].Latitude - positionsList[length - 1].Latitude)) + ((Math.Cos(positionsList[length - 1].Latitude * Math.PI / 180) * (positionsList[length].Longitude - positionsList[length - 1].Longitude)) * (Math.Cos(positionsList[length - 1].Latitude * Math.PI / 180) * (positionsList[length].Longitude - positionsList[length - 1].Longitude)))) * (40075.704 / 360);
            }
            if (way < 1)
                amountDistance.Text = string.Format("{0:000} m", way * 1000);
            else if(way < 10)
                amountDistance.Text = string.Format("{0:0.00} km", way);
            else if (way < 100)
                amountDistance.Text = string.Format("{0:00.0} km", way);
            else
                amountDistance.Text = string.Format("{0:000} km", way);
        }   
    }
}