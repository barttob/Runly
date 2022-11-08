using Runly.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;

namespace Runly.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StatisticsView : ContentPage
    {
        private readonly SQLiteAsyncConnection _database;
        private SQLiteAsyncConnection _databaseTraining;

        int buttonId;

        public StatisticsView(string btnId)
        {
            buttonId = Int16.Parse(btnId);
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            _database = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "trainingHistory.db3"));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            GetTrainingData();
            
            //dataStartu.Text = result[0].Da
            //testowe.Text = GetTrainingData().ToString();
            //collectionView.ItemsSource = await GetTrainingData();
        }

        public async Task<List<TrainingData>> GetTrainingData()
        {
            var query = _database.Table<TrainingData>().Where(p => p.Id == buttonId);
            var result =  await query.ToListAsync();
            nazwa.Text = "Trening: " + result[0].DateDay;

            _databaseTraining = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), result[0].TrainingDatabase));
            var locations = await _databaseTraining.Table<CurrentData>().ToListAsync();

            Polyline polyline = new Polyline();
            polyline.StrokeColor = Color.FromHex("#192126");
            polyline.StrokeWidth = 20;
            foreach (var pos in locations)
                polyline.Geopath.Add(new Position(pos.Latitude, pos.Longitude));
            map.MapElements.Add(polyline);

            Position startPos = new Position(locations[0].Latitude, locations[0].Longitude);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(startPos, Distance.FromKilometers(1)));


            dataStartu.Text = result[0].DateDay;
            godzinaStartu.Text = result[0].DateTime;
            dystans.Text = result[0].Distance;
            czas.Text = result[0].Time;
            calories.Text = result[0].Calories.ToString() + " cal";
            avrSpeed.Text = result[0].AvrSpeed.ToString() + " km/h";
            return result;
        }

        private async void ClosePage(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}