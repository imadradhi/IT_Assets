using System.Threading.Tasks;
using IT_Assets.Pages;
using IT_Assets.FireHelpers;
using IT_Assets.Global;

namespace IT_Assets
{
    public partial class MainPage : ContentPage
    {
        private FirebaseHelper _firebase = new FirebaseHelper();

        public MainPage()
        {
            InitializeComponent();
            string version = $"V {AppInfo.Current.VersionString}";
            VersionLabel.Text = version;
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            if(EmailPhoneEntry.Text == null || PasswordEntry.Text == null)
            {
                await DisplayAlert("Error", "Please enter both email/phone and password.", "OK");
                return;
            }
            bool success = await _firebase.LoginUserAsync(EmailPhoneEntry.Text, PasswordEntry.Text);
            if (success)
            {
                GlobalVar.UserEmail = EmailPhoneEntry.Text;
                Application.Current.MainPage = new NavigationPage(new DashboardPage(EmailPhoneEntry.Text));
            }
              
            //await DisplayAlert("Success", "Login successful!", "OK");
            else
                await DisplayAlert("Error", "Login failed", "OK");

        }

        private async void OnJoinTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateAccountPage());
            //DisplayAlert("Create account","Create link clicked","Ok");
        }
    }

}
