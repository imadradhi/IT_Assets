using IT_Assets.Models;
using IT_Assets.FireHelpers; // your FirebaseHelper namespace
using System.Collections.ObjectModel;

namespace IT_Assets.Pages;

public partial class MaintenancePage : ContentPage
{


    private readonly FirebaseHelper _firebaseHelper = new FirebaseHelper();
    public ObservableCollection<AssetModel> MaintenanceAssets { get; set; }
    public Command<AssetModel> MaintainCommand { get; set; }

    public MaintenancePage()
    {
        InitializeComponent();
        MaintainCommand = new Command<AssetModel>(OnMaintain);
        LoadAssets();
        BindingContext = this;
    }

    private async void LoadAssets()
    {
        try
        {
            var allAssets = await _firebaseHelper.GetAllAssetsAsync();
            var today = DateTime.Now;

            // Filter assets that need maintenance:
            // Required maintenance is one month after the last maintenance date
            var filtered = allAssets
                .Where(a => a.MaintenanceDate.AddMonths(1) <= today)
                .OrderBy(a => a.MaintenanceDate)
                .ToList();

            MaintenanceAssets = new ObservableCollection<AssetModel>(filtered);
            MaintenanceCollectionView.ItemsSource = MaintenanceAssets;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
        }
    }

    private async void OnMaintain(AssetModel asset)
    {
        bool confirm = await DisplayAlert("Update Maintenance",
                                          $"Mark {asset.Name} as maintained?",
                                          "Yes", "Cancel");
        if (!confirm)
            return;

        // Update maintenance date with the current date
        asset.MaintenanceDate = DateTime.Now;
        asset.RequiredMaintenanceDate = asset.MaintenanceDate.AddMonths(1);

        try
        {
            await _firebaseHelper.UpdateAssetAsync(asset.Code, asset);
            await DisplayAlert("Success", $"{asset.Name} marked as maintained.", "OK");

            // Refresh the list after update
            LoadAssets();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Update failed: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
