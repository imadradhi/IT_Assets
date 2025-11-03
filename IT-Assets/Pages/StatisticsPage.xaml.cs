using IT_Assets.Models;
using Microcharts;
using SkiaSharp;

namespace IT_Assets.Pages;

public partial class StatisticsPage : ContentPage
{
    private List<AssetModel> assetModels;

    public int All_in_oneCount { get; set; }
    public int LaptopCount { get; set; }
    public int DesktopCount { get; set; }
    public int PrinterCount { get; set; }
    public int XPrinterCount { get; set; }
    public int Barcodeprinter { get; set; }
    public int BarcodeReader { get; set; }
    public int UpsCount { get; set; }
    public int ScannerCount { get; set; }
    public int PosCount { get; set; }

    public DonutChart AssetsPieChart { get; set; }

    public StatisticsPage()
    {
        InitializeComponent();

        // Set data
        LaptopCount = 12;
        DesktopCount = 8;
        PrinterCount = 6;

        // Create the chart
        AssetsPieChart = new DonutChart
        {
            Entries = new[]
            {
                new ChartEntry(LaptopCount) { Label = "Laptops", ValueLabel = LaptopCount.ToString(), Color = SKColor.Parse("#0066CC") },
                new ChartEntry(DesktopCount) { Label = "Desktops", ValueLabel = DesktopCount.ToString(), Color = SKColor.Parse("#00C851") },
                new ChartEntry(PrinterCount) { Label = "Printers", ValueLabel = PrinterCount.ToString(), Color = SKColor.Parse("#FF4444") }
            }
        };

        // Set the BindingContext
        BindingContext = this;
    }

    public StatisticsPage(List<AssetModel> assetModels)
    {
        InitializeComponent();
        this.assetModels = assetModels;

        // Calculate counts dynamically
        LaptopCount = assetModels.Count(a => a.Code.StartsWith("L"));
        DesktopCount = assetModels.Count(a => a.Code.StartsWith("D"));
        PrinterCount = assetModels.Count(a => (a.Code.StartsWith("P") && !a.Code.Contains("POS")));
        XPrinterCount = assetModels.Count(a => a.Code.StartsWith("X"));
        Barcodeprinter = assetModels.Count(a => a.Code.StartsWith("BP"));
        BarcodeReader = assetModels.Count(a => a.Code.StartsWith("BR"));
        UpsCount = assetModels.Count(a => a.Code.StartsWith("U"));
        ScannerCount = assetModels.Count(a => a.Code.StartsWith("S"));
        PosCount = assetModels.Count(a => a.Code.StartsWith("POS")); // example
        All_in_oneCount = assetModels.Count(a => a.Code.StartsWith("A")); // example

        // Create the chart
        AssetsPieChart = new DonutChart
        {
            Entries = new[]
            {
            new ChartEntry(LaptopCount) { Label = "Laptops", ValueLabel = LaptopCount.ToString(), Color = SKColor.Parse("#0066CC") },
            new ChartEntry(DesktopCount) { Label = "Desktops", ValueLabel = DesktopCount.ToString(), Color = SKColor.Parse("#00C851") },
            new ChartEntry(PrinterCount) { Label = "Printers", ValueLabel = PrinterCount.ToString(), Color = SKColor.Parse("#FF4444") }
            // Add other entries if you want
        }
        };

        BindingContext = this;
    }


    private Frame CreateLaptopFrame()
    {
        return new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 15,
            Padding = 15,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 5,
                Offset = new Point(5, 5),
                Radius = 10
            },
            Content = new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Image { Source = "laptop_icon.png", HeightRequest = 40 },
                    new Label { Text = "Laptops", FontAttributes = FontAttributes.Bold, FontSize = 16 },
                    new Label { Text = LaptopCount.ToString(), FontSize = 22, TextColor = Color.FromHex("#0066CC") }
                }
            }
        };
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

}