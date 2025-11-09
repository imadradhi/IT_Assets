using IT_Assets.Models;
using Microcharts;
using SkiaSharp;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using IT_Assets.Global;
using IT_Assets.FireHelpers;

namespace IT_Assets.Pages;

public partial class ReportsPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();
    private ObservableCollection<AssetModel> _allAssets = new();
    private ObservableCollection<AssetModel> _filteredAssets = new();
    private string _databaseUrl = GlobalVar.DatabaseUrl; // e.g. your Firebase base URL
    private FirebaseHelper _firebase = new FirebaseHelper();
    private bool _isLoading = false; // Flag to prevent duplicate loading
    public ObservableCollection<AssetModel> FilteredAssets { get; set; } = new ObservableCollection<AssetModel>();


    public ReportsPage()
    {
        InitializeComponent();

        // _ = InitializeAsync();
        // LoadAssetsAsync();
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
            _allAssets.Clear(); // Clear the collection before loading new data
            var assets = await _firebase.GetAllAssetsAsync();

            if (assets == null || !assets.Any())
            {
                await DisplayAlert("Info", "No assets found in Firebase.", "OK");
                return;
            }

            foreach (var asset in assets)
            {
                _allAssets.Add(asset); // Add each asset to the collection
            }

            FilteredAssets.Clear(); // Clear the filtered collection
            foreach (var asset in _allAssets)
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

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

}
