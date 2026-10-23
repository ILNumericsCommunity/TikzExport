using System;
using System.Windows.Forms;
using ILNumerics.Community.ExportSamples;
using ILNumerics.Community.TikzExport;
using Size = System.Drawing.Size;

namespace TikzExportDemo;

/// <summary>Displays standard ILNumerics scenes and their TikZ exports.</summary>
public partial class TikzDemoForm : Form
{
    /// <summary>Initializes the demo controls and shared scene selector.</summary>
    public TikzDemoForm()
    {
        InitializeComponent();

        if (comboBoxScene == null)
            return;

        comboBoxScene.DataSource = ExportDemoScenes.Names;
    }

    private void TikzDemoForm_Load(object? sender, EventArgs e)
    {
        if (comboBoxScene == null)
            return;

        comboBoxScene.SelectedIndex = 0;
    }

    private void comboBoxScene_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (comboBoxScene == null || ilPanel == null)
            return;

        if (comboBoxScene.SelectedIndex == -1)
            return;

        var previousScene = ilPanel.Scene;
        ilPanel.Scene = ExportDemoScenes.Create(comboBoxScene.SelectedIndex);
        previousScene?.Dispose();

        ilPanel.Configure();
        ilPanel.Refresh();
    }

    private void btnExportFile_Click(object? sender, EventArgs e)
    {
        if (ilPanel == null)
            return;

        using var saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "TIKZ Image|*.tikz";
        saveFileDialog.Title = "Save as TIKZ Image";
        saveFileDialog.ShowDialog();

        if (String.IsNullOrEmpty(saveFileDialog.FileName))
            return;

        var size = new Size(120, 120);
        var currentScene = ilPanel.GetCurrentScene() ?? ilPanel.Scene;
        if (currentScene == null)
            return;

        TikzExport.ExportFile(currentScene, saveFileDialog.FileName, size);
    }

    private void btnExportText_Click(object? sender, EventArgs e)
    {
        if (ilPanel == null || textBoxTikz == null)
            return;

        var currentScene = ilPanel.GetCurrentScene() ?? ilPanel.Scene;
        if (currentScene == null)
            return;

        textBoxTikz.Text = TikzExport.ExportString(currentScene);
    }
}
