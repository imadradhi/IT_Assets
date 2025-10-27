using IT_Assets.FireHelpers;

namespace IT_Assets.Pages;

public partial class CreateAccountPage : ContentPage
{
    private FirebaseHelper _firebase = new FirebaseHelper();

    public CreateAccountPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
            string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
        {
            await DisplayAlert("Error", "Please fill in all fields.", "OK");
            return;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlert("Error", "Passwords do not match.", "OK");
            return;
        }

        bool success = await _firebase.RegisterUserAsync(EmailEntry.Text, PasswordEntry.Text);
        if (success)
        {
            await DisplayAlert("Success", "Account created successfully!", "OK");

            // Automatically navigate to Dashboard after registration
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }
        else
        {
            await DisplayAlert("Error", "Registration failed.", "OK");
        }
    }
}