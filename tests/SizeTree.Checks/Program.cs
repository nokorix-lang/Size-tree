using SizeTree.Core;

string root = Path.Combine(Path.GetTempPath(), "sizetree-check-" + Guid.NewGuid());
Directory.CreateDirectory(root);
int checks = 0;
void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
try
{
    Check(TreeBuilder.Build(root).Files == 0, "Leerer Ordner");
    Directory.CreateDirectory(Path.Combine(root, "Unterordner"));
    File.WriteAllText(Path.Combine(root, "Unterordner", "ä.txt"), "Test");
    File.WriteAllText(Path.Combine(root, "z.txt"), "Test");
    var tree = TreeBuilder.Build(root);
    Check(tree.Files == 2 && tree.Folders == 1 && tree.Warnings == 0, "Dateien und Unterordner zählen");
    Check(tree.Text.Contains("├── Unterordner/") && tree.Text.Contains("│   └── ä.txt") && tree.Text.Contains("└── z.txt"), "Baumzeichen, Sortierung und Unicode");
    Directory.CreateSymbolicLink(Path.Combine(root, "Loop"), root);
    tree = TreeBuilder.Build(root);
    Check(tree.Files == 2 && tree.Text.Contains("[Verknüpfung; nicht verfolgt]"), "Verknüpfungsschleife wird nicht verfolgt");
    using var cancel = new CancellationTokenSource(); cancel.Cancel();
    bool canceled = false;
    try { TreeBuilder.Build(root, cancel.Token); } catch (OperationCanceledException) { canceled = true; }
    Check(canceled, "Abbruch");
    bool missing = false;
    try { TreeBuilder.Build(Path.Combine(root, "fehlt")); } catch (DirectoryNotFoundException) { missing = true; }
    Check(missing, "Fehlender Ordner");
    Console.WriteLine($"{checks} Prüfungen bestanden.");
}
finally { Directory.Delete(root, true); }
