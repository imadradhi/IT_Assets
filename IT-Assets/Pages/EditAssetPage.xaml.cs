using System;
using IT_Assets.FireHelpers;
using Microsoft.Maui.Controls;
using IT_Assets.Models;
using IT_Assets.Global;

namespace IT_Assets.Pages;

public partial class EditAssetPage : ContentPage
{
    private readonly AssetModel asset;
    private readonly FirebaseHelper firebase;
    private readonly Action reloadAssets;

    public EditAssetPage(AssetModel asset, FirebaseHelper firebase, Action reloadAssets)
    {
        InitializeComponent();
        this.asset = asset;
        this.firebase = firebase;
        this.reloadAssets = reloadAssets;

        // Populate the fields with the asset data
        CodeEntry.Text = asset.Code;
        NameEntry.Text = asset.Name;
        ModelEntry.Text = asset.Model;
        LocationEntry.Text = asset.Location;
        MaintenanceDatePicker.Date = asset.MaintenanceDate.Date;
        ReceiptFormEntry.Text = asset.ReceiptForm;
        NoteEditor.Text = asset.Note;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var updatedAsset = new AssetModel
        {
            Id = asset.Id, // Preserve the ID
            Code = CodeEntry.Text,
            Name = NameEntry.Text,
            Model = ModelEntry.Text,
            Location = LocationEntry.Text,
            MaintenanceDate = MaintenanceDatePicker.Date.Date,
            RequiredMaintenanceDate = MaintenanceDatePicker.Date.AddMonths(1).Date,
            ReceiptForm = ReceiptFormEntry.Text,
            Note = NoteEditor.Text,
            UpdatedBy = GlobalVar.UserEmail
        };

        bool success = await firebase.UpdateAssetAsync(asset.Id, updatedAsset);
        if (success)
        {
            await DisplayAlert("Success", "Asset updated successfully.", "OK");
            if (reloadAssets != null)
            {
                reloadAssets.Invoke();
            }
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Failed to update asset.", "OK");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirm", "Delete this asset?", "Yes", "No");
        if (confirm)
        {
            bool success = await firebase.DeleteAssetAsync(asset.Id);
            if (success)
            {
                await DisplayAlert("Deleted", "Asset removed.", "OK");
                if (reloadAssets != null)
                {
                    reloadAssets.Invoke();
                }
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", "Failed to delete asset.", "OK");
            }
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}