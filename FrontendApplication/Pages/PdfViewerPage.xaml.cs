namespace FrontendApplication.Pages;
public partial class PdfViewerPage : ContentPage
{
    public PdfViewerPage(string pdfPath)
    {
        InitializeComponent();
        PdfWebView.Source = new UrlWebViewSource { Url = $"file://{pdfPath}" };
        PdfWebView.Source = pdfPath;
    }
}
