using Runly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Runly.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Settings : ContentPage
    {
        public Settings()
        {
            InitializeComponent();
            List<SettingsOption> myList = new List<SettingsOption>
            {
                new SettingsOption{Name="Opcja 1", Icon="icon_settings.png" },
                new SettingsOption{Name="Opcja 2", Icon="icon_settings.png" },
                new SettingsOption{Name="Opcja 3", Icon="icon_settings.png" },
                new SettingsOption{Name="Opcja 4", Icon="icon_settings.png" },
                new SettingsOption{Name="Opcja 5", Icon="icon_settings.png" },
            };
            myListView.ItemsSource = myList;
        }
    }
}