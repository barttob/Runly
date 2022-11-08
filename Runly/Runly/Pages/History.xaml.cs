using Runly.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;
using Button = Xamarin.Forms.Button;

namespace Runly.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class History : ContentPage
    {
        private readonly SQLiteAsyncConnection _database;

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

        public Task<List<TrainingData>> GetTrainingData()
        {
            return _database.Table<TrainingData>().ToListAsync();
        }

        private async void OpenStatistics(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            await Navigation.PushAsync(new StatisticsView(btn.ClassId));
        }
    }
}