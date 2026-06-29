using System.Windows.Controls;

namespace DineFlow.WPFApp.Views;

public partial class ModulePlaceholderView : UserControl
{
    public ModulePlaceholderView(ModulePreview preview)
    {
        InitializeComponent();

        txtBreadcrumb.Text = $"DINEFLOW  /  {preview.Title.ToUpperInvariant()}";
        txtIcon.Text = preview.IconGlyph;
        txtTitle.Text = preview.Title;
        txtSubtitle.Text = preview.Subtitle;
        txtMetricOneLabel.Text = preview.MetricOne;
        txtMetricTwoLabel.Text = preview.MetricTwo;
        txtMetricThreeLabel.Text = preview.MetricThree;
        txtSectionTitle.Text = preview.SectionTitle;
        txtAction.Text = preview.ActionLabel;
        txtColumnOne.Text = preview.ColumnOne;
        txtColumnTwo.Text = preview.ColumnTwo;
        txtColumnThree.Text = preview.ColumnThree;
        txtEmptyMessage.Text = preview.EmptyMessage;
    }
}

public sealed record ModulePreview(
    string Title,
    string Subtitle,
    string IconGlyph,
    string MetricOne,
    string MetricTwo,
    string MetricThree,
    string SectionTitle,
    string ActionLabel,
    string ColumnOne,
    string ColumnTwo,
    string ColumnThree,
    string EmptyMessage);
