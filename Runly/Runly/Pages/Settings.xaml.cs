using Plugin.LocalNotification;
using Runly.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

namespace Runly.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Settings : ContentPage
    {
        Dictionary<int, string> options = new Dictionary<int, string>();
        private readonly SQLiteAsyncConnection _database;
        private SQLiteAsyncConnection _databaseTraining;

        public Settings()
        {
            InitializeComponent();
            options[0] = "Login";
            options[1] = "Hasło";
            options[2] = "Waga";
            options[3] = "Wiek";
            Display_Options();
            _database = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "trainingHistory.db3"));
            /*List<SettingsList> settingsOptions = new List<SettingsList>
            {
                new SettingsList{OptionName="Waga", OptionValue="70"},
                new SettingsList{OptionName="Waga", OptionValue="70"},
                new SettingsList{OptionName="Waga", OptionValue="70"}
            };*/
            //settingsList.ItemsSource = settingsOptions;
        }

        public async void Open_Popup(object sender, EventArgs e)
        {
            var option = (Xamarin.Forms.Button)sender;
            int id = Int16.Parse(option.ClassId);
            string result = await DisplayPromptAsync(options[id], "");
            Preferences.Set(options[id], result);
            Display_Options();
        }

        public async void Open_Popup_Weight(object sender, EventArgs e)
        {
            var option = (Xamarin.Forms.Button)sender;
            int id = Int16.Parse(option.ClassId);
            bool valid = false;
            do
            {
                string result = await DisplayPromptAsync("Waga, 10-150", "");
                if (result == null)
                    result = Preferences.Get(options[2], "70");
                if (Int16.Parse(result) <= 150 && Int16.Parse(result) >= 10)
                {
                    valid = true;
                    Preferences.Set(options[id], result);
                }
            } while (!valid);
            
            Display_Options();
        }

        public void Display_Options()
        {
            Login.Text = Preferences.Get(options[0], "User");
            Waga.Text = Preferences.Get(options[2], "70");
            Wiek.Text = Preferences.Get(options[3], "20");
        }

        public async void Clear_Database(object sender, EventArgs e)
        {
            /*int count = (int)(await CountTrainings());
            for (int i = 0; i < count - 1; i++)
            {
                var query = _database.Table<TrainingData>().Where(p => p.Id == i);
                var result = await query.ToListAsync();
                _databaseTraining = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), result[0].TrainingDatabase));
                await ClearDatabase();
            }*/
            await ClearMainDatabase();
        }

        public async void Set_Notification(object sender, EventArgs e)
        {
            int hour;
            bool valid = false;
            do {
                string result = await DisplayPromptAsync("Podaj godzine. 0-23", "");
                if (result == null)
                    result = "18";
                hour = Int16.Parse(result);
                if(hour <= 23 && hour >= 0)
                    valid = true;
            } while (!valid);

            DateTime setTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, 0, 0);
            int seconds = (setTime.Hour - DateTime.Now.Hour) * 3600 - DateTime.Now.Minute * 60 + DateTime.Now.Second;
            Console.WriteLine(seconds);

            var notification = new NotificationRequest
            {
                BadgeNumber = 1,
                Description = "Wygląda, że zapomniałeś o treningu",
                Title = "Runly",
                NotificationId = 1337,
                Schedule =
                {
                    NotifyTime = DateTime.Now.AddSeconds(seconds),
                    RepeatType = NotificationRepeat.Daily,            
                }
            };
            LocalNotificationCenter.Current.Show(notification);
        }

        public Task<int> ClearMainDatabase()
        {
            return _database.DeleteAllAsync<TrainingData>();
        }
        /*public Task<int> CountTrainings()
        {
            return _database.Table<TrainingData>().CountAsync();
        }

        public Task<int> ClearDatabase()
        {
            return _databaseTraining.DeleteAllAsync<CurrentData>();
        }*/
    }
}