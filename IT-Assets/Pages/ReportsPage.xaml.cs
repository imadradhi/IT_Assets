using IT_Assets.Models;

namespace IT_Assets.Pages;

public partial class ReportsPage : ContentPage
{
	public ReportsPage()
	{
		InitializeComponent();
	}

    public ReportsPage(List<AssetModel> assetModels)
    {
        AssetModels = assetModels;
        InitializeComponent();
    }

    public List<AssetModel> AssetModels { get; }
}