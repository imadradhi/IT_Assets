
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using IT_Assets.FireHelpers;
using IT_Assets.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace IT_Assets.Pages;

public partial class DashboardPage : ContentPage
{
    private FirebaseHelper _firebase = new FirebaseHelper();
    private bool _isLoading = false; // Flag to prevent duplicate loading

    private int _assetsCount = 0;
    private int _maitenancelistCount = 0;

    public ObservableCollection<AssetModel> AllAssets { get; set; } = new ObservableCollection<AssetModel>();
    public ObservableCollection<AssetModel> FilteredAssets { get; set; } = new ObservableCollection<AssetModel>();

    public DashboardPage(string userEmail)
    {
        InitializeComponent();

        BindingContext = this; // Set the BindingContext to the current page

        // Display the logged-in user email
        UserEmailLabel.Text = userEmail;

        // Set the CollectionView's ItemsSource
        AssetsCollectionView.ItemsSource = FilteredAssets;

        // Initialize async tasks
        _ = InitializeAsync();
           
    }

    private async Task InitializeAsync()
    {
        // Load assets from Firebase only once during initialization
        await LoadAssetsAsync();
    }

    private async Task LoadAssetsAsync()
    {
        if (_isLoading) return; // Prevent duplicate calls
        _isLoading = true;

        try
        {
            AllAssets.Clear(); // Clear the collection before loading new data
            var assets = await _firebase.GetAllAssetsAsync();

            if (assets == null || !assets.Any())
            {
                await DisplayAlert("Info", "No assets found in Firebase.", "OK");
                return;
            }

            foreach (var asset in assets)
            {
                AllAssets.Add(asset); // Add each asset to the collection
            }

            FilteredAssets.Clear(); // Clear the filtered collection
            foreach (var asset in AllAssets)
            {
                FilteredAssets.Add(asset); // Copy all assets to the filtered collection
            }

            Console.WriteLine($"Loaded {FilteredAssets.Count} assets.");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load assets: {ex.Message}", "OK");
        }
        finally
        {
            _isLoading = false; // Reset the loading flag
        }
    }

    // -------------------------------
    // ReloadAssets for Add/Edit pages
    // -------------------------------
    public async Task ReloadAssets()
    {
        await LoadAssetsAsync();
    }

    // -------------------------------
    // Search functionality
    // -------------------------------
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var query = e.NewTextValue?.ToLower() ?? "";
        FilteredAssets.Clear();
        foreach (var asset in AllAssets.Where(a => a.Code.ToLower().Contains(query) || 
        a.Name.ToLower().Contains(query) || 
        a.Model.ToLower().Contains(query) ||
        a.Location.ToLower().Contains(query) ||
        a.ReceiptForm.ToLower().Contains(query) ||
        a.MaintenanceDate.Equals(query) ||
        a.UpdatedBy.ToLower().Contains(query)))

            FilteredAssets.Add(asset);
        ShownAssetsCount.Text = $"{FilteredAssets.Count} Items shown!";
    }

    // -------------------------------
    // Edit / Remove functionality
    // -------------------------------
    private async void OnEditClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var id = button.CommandParameter.ToString();
        var asset = await _firebase.GetAssetById(id);

        if (asset != null)
        {
            // Wrap ReloadAssets in an Action
            Action reloadAssetsAction = () => { _ = ReloadAssets(); };
            await Navigation.PushAsync(new EditAssetPage(asset, _firebase, reloadAssetsAction));
        }
    }

    private async void OnRemoveClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var id = button.CommandParameter.ToString();

        bool confirm = await DisplayAlert("Confirm", "Are you sure you want to delete this asset?", "Yes", "No");
        if (confirm)
        {
            await _firebase.DeleteAssetAsync(id);
            await LoadAssetsAsync();
        }
    }

    // -------------------------------
    // Sign out button
    // -------------------------------
    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Sign Out",
            "You will be signed out and returned to the login page. Do you want to continue?",
            "Yes",   // Accept button
            "No");   // Cancel button

        if (confirm)
        {
            await _firebase.LogoutUser();
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }
    }

    // -------------------------------
    // Top buttons (New / Maintenance / Stats / Reports)
    // -------------------------------
    private async void OnNewClicked(object sender, EventArgs e)
    {
        // Navigate to the NewAssetPage
        // Pass the shared FirebaseHelper and the ReloadAssets method
        await Navigation.PushAsync(new NewAssetPage(_firebase, ReloadAssets));
    }

    private async void OnMaintenanceClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MaintenancePage());
    }

    private async void OnStatisticsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new StatisticsPage(AllAssets.ToList()));
    }

    private async void OnReportsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ReportsPage()); // AllAssets.ToList()
    }

    // -------------------------------
    // Export to Excel functionality
    // -------------------------------
    private async void OnExportToExcelClicked(object sender, EventArgs e)
    {
        try
        {
            // Create a DataTable from the FilteredAssets collection
            var dataTable = new System.Data.DataTable("Assets");
            dataTable.Columns.Add("Code");
            dataTable.Columns.Add("Name");
            dataTable.Columns.Add("Model");
            dataTable.Columns.Add("Location");
            dataTable.Columns.Add("Receipt Form");
            dataTable.Columns.Add("Maintenance Date");
            dataTable.Columns.Add("Updated By");
            dataTable.Columns.Add("Note");

            foreach (var asset in FilteredAssets)
            {
                dataTable.Rows.Add(
                    asset.Code,
                    asset.Name,
                    asset.Model,
                    asset.Location,
                    asset.ReceiptForm,
                    asset.MaintenanceDate.ToString("yyyy-MM-dd"),
                    asset.UpdatedBy,
                    asset.Note
                );
            }

            // Ask the user where to save the file
            string filePath = await PickSaveFileAsync("AssetsExport.xlsx");
            if (string.IsNullOrEmpty(filePath))
            {
                await DisplayAlert("Cancelled", "Export cancelled by the user.", "OK");
                return;
            }

            // Create an Excel workbook
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(dataTable, "Assets");

                // Save the Excel file to the chosen path
                workbook.SaveAs(filePath);

                // Notify the user
                await DisplayAlert("Success", $"Assets exported to {filePath}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to export assets: {ex.Message}", "OK");
        }
    }

    private async Task<string> PickSaveFileAsync(string defaultFileName)
    {
#if WINDOWS
        var savePicker = new Windows.Storage.Pickers.FileSavePicker();
        var hwnd = ((MauiWinUIWindow)Application.Current.Windows[0].Handler.PlatformView).WindowHandle;
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

        savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
        savePicker.SuggestedFileName = defaultFileName;
        savePicker.FileTypeChoices.Add("Excel File", new List<string> { ".xlsx" });

        var file = await savePicker.PickSaveFileAsync();
        return file?.Path;
#elif MACCATALYST
        // macOS implementation (if needed)
        await DisplayAlert("Not Supported", "Save file dialog is not implemented for macOS.", "OK");
        return null;
#elif ANDROID || IOS
        // Mobile platforms do not have a native save file dialog
        await DisplayAlert("Not Supported", "Save file dialog is not supported on mobile platforms.", "OK");
        return null;
#else
        await DisplayAlert("Not Supported", "Save file dialog is not supported on this platform.", "OK");
        return null;
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Reload assets when the page appears
        await ReloadAssets();

        // Load the number of shown items and maintenance list items
        _maitenancelistCount = await _firebase.GetMaintenanceAssetsCountAsync();
        MaintenancelistCount.Text = $"{_maitenancelistCount.ToString()} Items in maintenance list";
        _assetsCount = await _firebase.GetAssetsCountAsync();
        ShownAssetsCount.Text = $"{_assetsCount} Items shown";
    }


}
