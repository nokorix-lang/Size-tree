using System.Text;
using SizeTree.Core;

namespace SizeTree;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    readonly TextBox path = new() { Dock = DockStyle.Fill, ReadOnly = true, PlaceholderText = "Bitte einen Ordner auswählen …" };
    readonly TextBox output = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, WordWrap = false, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 10), MaxLength = 0 };
    readonly Label status = new() { AutoSize = true, Text = "Bereit. Verknüpfungen werden nicht verfolgt.", Padding = new Padding(0, 8, 0, 0) };
    readonly Button choose = new() { Text = "Ordner wählen …", AutoSize = true };
    readonly Button refresh = new() { Text = "Aktualisieren", AutoSize = true, Enabled = false };
    readonly Button cancel = new() { Text = "Abbrechen", AutoSize = true, Enabled = false };
    readonly Button copy = new() { Text = "Kopieren", AutoSize = true, Enabled = false };
    readonly Button save = new() { Text = "Als TXT speichern …", AutoSize = true, Enabled = false };
    CancellationTokenSource? scan;
    string? result;

    public MainForm()
    {
        Text = "SizeTree – Ordnerstruktur"; Width = 1000; Height = 720; MinimumSize = new Size(650, 420);
        StartPosition = FormStartPosition.CenterScreen;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), RowCount = 4, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 6) };
        buttons.Controls.AddRange([choose, refresh, cancel, copy, save]);
        layout.Controls.Add(path, 0, 0); layout.Controls.Add(buttons, 0, 1); layout.Controls.Add(output, 0, 2); layout.Controls.Add(status, 0, 3);
        Controls.Add(layout);
        choose.Click += async (_, _) =>
        {
            using var dialog = new FolderBrowserDialog { Description = "Ordner für die Baumstruktur auswählen", UseDescriptionForTitle = true, ShowNewFolderButton = false };
            if (Directory.Exists(path.Text)) dialog.InitialDirectory = path.Text;
            if (dialog.ShowDialog(this) == DialogResult.OK) { path.Text = dialog.SelectedPath; await Scan(); }
        };
        refresh.Click += async (_, _) => await Scan();
        cancel.Click += (_, _) => scan?.Cancel();
        copy.Click += (_, _) =>
        {
            try { if (result != null) { Clipboard.SetText(result); status.Text = "Baumstruktur kopiert."; } }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Kopieren fehlgeschlagen"); }
        };
        save.Click += (_, _) => Save();
        FormClosing += (_, _) => scan?.Cancel();
    }

    async Task Scan()
    {
        using var cts = new CancellationTokenSource(); scan = cts;
        choose.Enabled = refresh.Enabled = copy.Enabled = save.Enabled = false; cancel.Enabled = true;
        result = null; output.Clear(); status.Text = "Ordnerstruktur wird eingelesen …";
        string selected = path.Text;
        try
        {
            var tree = await Task.Run(() => TreeBuilder.Build(selected, cts.Token));
            if (IsDisposed || Disposing) return;
            result = tree.Text; output.Text = result;
            status.Text = $"{tree.Folders:N0} Unterordner · {tree.Files:N0} Dateien · {tree.Warnings:N0} Hinweise";
        }
        catch (OperationCanceledException) { if (!IsDisposed) status.Text = "Einlesen abgebrochen."; }
        catch (Exception ex) { if (!IsDisposed) { status.Text = "Einlesen fehlgeschlagen."; MessageBox.Show(this, ex.Message, "Fehler"); } }
        finally
        {
            scan = null;
            if (!IsDisposed) { choose.Enabled = refresh.Enabled = true; cancel.Enabled = false; copy.Enabled = save.Enabled = result != null; }
        }
    }

    void Save()
    {
        using var dialog = new SaveFileDialog { Filter = "Textdatei (*.txt)|*.txt", FileName = "Ordnerstruktur.txt", DefaultExt = "txt" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { File.WriteAllText(dialog.FileName, result, new UTF8Encoding(true)); status.Text = "UTF-8-Textdatei gespeichert."; }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Speichern fehlgeschlagen"); }
    }
}
