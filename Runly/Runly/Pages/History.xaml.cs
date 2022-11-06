using Runly.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
            Console.WriteLine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)));
        }

        protected override async void OnAppearing() // funkcja uruchamia się po  każdej zmianie widoku
        {
            base.OnAppearing();
            collectionView.ItemsSource = await GetTrainingData(); // pobranie danych do obiektu colectionView 
        }

        public Task<List<TrainingData>> GetTrainingData()              // Uruchomiemie zadania odczytu
        {
            return _database.Table<TrainingData>().ToListAsync();
        }

        
    }
}