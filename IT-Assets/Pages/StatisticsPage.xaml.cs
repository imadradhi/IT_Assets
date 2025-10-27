using IT_Assets.Models;

namespace IT_Assets.Pages;

public partial class StatisticsPage : ContentPage
{
	public StatisticsPage()
	{
		InitializeComponent();
	}

    public StatisticsPage(List<AssetModel> assetModels)
    {
        AssetModels = assetModels;
    }

    public List<AssetModel> AssetModels { get; }
}