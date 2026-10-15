using System.Text;

namespace SizeTree.Core;

public sealed record TreeResult(string Text, int Folders, int Files, int Warnings);

public static class TreeBuilder
{
    public static TreeResult Build(string root, CancellationToken cancellationToken = default)
    {
        root = Path.GetFullPath(root);
        if (!Directory.Exists(root)) throw new DirectoryNotFoundException("Der ausgewählte Ordner existiert nicht.");
        var text = new StringBuilder(root + Environment.NewLine);
        int folders = 0, files = 0, warnings = 0, entries = 0;
        bool stopped = false;
        Walk(root, "", 0);
        return new(text.ToString(), folders, files, warnings);

        void Walk(string path, string prefix, int depth)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (depth >= 128) { text.AppendLine(prefix + "[Maximale Tiefe erreicht]"); warnings++; return; }
            if (stopped) return;
            try
            {
                // Bound both listing memory and displayed output for very large directories.
                var children = new DirectoryInfo(path).EnumerateFileSystemInfos()
                    .Take(5001).ToArray();
                bool truncated = children.Length > 5000;
                children = children.Take(5000).OrderByDescending(x => (x.Attributes & FileAttributes.Directory) != 0)
                    .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToArray();
                for (int i = 0; i < children.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (stopped) break;
                    if (++entries > 50000 || text.Length > 8_000_000)
                    {
                        text.AppendLine(prefix + "[Ausgabegrenze erreicht: 50.000 Einträge / ca. 8 Mio. Zeichen]");
                        warnings++; stopped = true; break;
                    }
                    var item = children[i];
                    bool last = i == children.Length - 1 && !truncated;
                    bool dir = (item.Attributes & FileAttributes.Directory) != 0;
                    bool link = (item.Attributes & FileAttributes.ReparsePoint) != 0;
                    if (dir) folders++; else files++;
                    text.Append(prefix).Append(last ? "└── " : "├── ").Append(item.Name.Replace('\r', ' ').Replace('\n', ' '));
                    if (dir) text.Append('/');
                    if (link) text.Append(" [Verknüpfung; nicht verfolgt]");
                    text.AppendLine();
                    if (dir && !link) Walk(item.FullName, prefix + (last ? "    " : "│   "), depth + 1);
                }
                if (truncated && !stopped) { text.AppendLine(prefix + "└── [Ordner auf 5.000 Einträge begrenzt]"); warnings++; }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or System.Security.SecurityException)
            {
                text.AppendLine(prefix + "[Nicht lesbar: " + ex.Message.Replace('\r', ' ').Replace('\n', ' ') + "]");
                warnings++;
            }
        }
    }
}
