using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace Runly.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StepCounter : ContentPage
    {

        public StepCounter()
        {
            InitializeComponent();
            

            GetStepCounter steps = new GetStepCounter();

            MessagingCenter.Subscribe<string>(this, "steps", message =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    amountSteps.Text = message;
                });
            });

            MessagingCenter.Subscribe<string>(this, "trainingSteps", message =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (Int16.Parse(message) > 0)
                    {
                        trainingGrid.IsVisible= true;
                        amountTrainingSteps.Text = message;
                    } else
                    {
                        trainingGrid.IsVisible = false;
                    }
                        
                        
                });
            });

            

            if (Preferences.Get("LocationServiceRunning", false) == true)
            {
                StartService();
            }
        }

        private void StartService()
        {
            var startServiceMessage = new StartServiceMessage();
            MessagingCenter.Send(startServiceMessage, "ServiceStarted");
            Preferences.Set("StepCounterService", true);
            //amountSteps.Text = "Location Service has been started!";
        }
    }
}