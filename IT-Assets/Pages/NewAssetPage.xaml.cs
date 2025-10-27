using IT_Assets.FireHelpers;
using IT_Assets.Global;
using IT_Assets.Models;

namespace IT_Assets.Pages;

public partial class NewAssetPage : ContentPage
{
    private FirebaseHelper _firebase;
    private Func<Task> _reloadAssets; // Use Func<Task> instead of Action

    public NewAssetPage(FirebaseHelper firebase, Func<Task> reloadAssets)
    {
        InitializeComponent();
        _firebase = firebase;
        _reloadAssets = reloadAssets;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // 1?? Validate fields
        if (string.IsNullOrWhiteSpace(CodeEntry.Text) ||
            string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(ModelEntry.Text) ||
            string.IsNullOrWhiteSpace(LocationEntry.Text) ||
            string.IsNullOrWhiteSpace(ReceiptFormEntry.Text))
        {
            await DisplayAlert("Error", "Please fill in all fields.", "OK");
            return;
        }

        // 2?? Create new asset
        var asset = new AssetModel
        {
            Code = CodeEntry.Text.Trim(),
            Name = NameEntry.Text.Trim(),
            Model = ModelEntry.Text.Trim(),
            Location = LocationEntry.Text.Trim(),
            ReceiptForm = ReceiptFormEntry.Text.Trim(),
            MaintenanceDate = MaintenanceDatePicker.Date.Date,
            UpdatedBy = GlobalVar.UserEmail,
            Note = NoteEditor.Text?.Trim() ?? ""
        };

        // 3?? Save to Firebase
        bool success = await _firebase.AddAssetAsync(asset);

        if (success)
        {
            await DisplayAlert("Success", "Asset added successfully.", "OK");

            // 4?? Refresh dashboard list
            if (_reloadAssets != null)
                await _reloadAssets();

            // 5?? Go back to dashboard
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Failed to add asset.", "OK");
        }
    }

}
