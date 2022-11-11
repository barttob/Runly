using Runly.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;
using Button = Xamarin.Forms.Button;

namespace Runly.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class History : ContentPage
    {
        private readonly SQLiteAsyncConnection _database;
        List<TrainingData> results;

        public History()
        {
            InitializeComponent();

            _database = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "trainingHistory.db3"));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            collectionView.ItemsSource = await GetTrainingData();
        }

        public async Task<List<TrainingData>> GetTrainingData()
        {
            var query = await _database.Table<TrainingData>().ToListAsync();
            results = Enumerable.Reverse(query).ToList();
            return results;
        }

        private async void OpenStatistics(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            await Navigation.PushAsync(new StatisticsView(btn.ClassId));
        }
    }
}