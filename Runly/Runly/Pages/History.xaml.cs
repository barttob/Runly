using Runly.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
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

            //Inicjalizacja połączenia z bazą danych
            _database = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "trainingHistory.db3"));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //Wypisanie danych na ekran
            collectionView.ItemsSource = await GetTrainingData();
        }

        //Pobranie danych z bazy danych
        public async Task<List<TrainingData>> GetTrainingData()
        {
            var query = await _database.Table<TrainingData>().ToListAsync();
            results = Enumerable.Reverse(query).ToList();
            return results;
        }

        //Funkcja otwieracjąca widok statystyk, podsumowania
        private async void OpenStatistics(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            await Navigation.PushAsync(new StatisticsView(btn.ClassId));
        }
    }
}