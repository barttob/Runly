using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Runly.Pages;

namespace Runly
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new TabPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
