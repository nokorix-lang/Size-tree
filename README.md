# SizeTree

Portable Windows-App in C# / .NET 10. Zeigt die Dateien und Unterordner eines ausgewählten Ordners als Textbaum an.

## Starten

1. Das ZIP vollständig entpacken.
2. `SizeTree.exe` doppelt anklicken.
3. **Ordner wählen …** anklicken und einen Ordner auswählen.
4. Die Ausgabe mit **Kopieren** übernehmen oder **Als TXT speichern …** als UTF-8-Datei exportieren.

Für Windows 10/11 (64 Bit, x64). Keine separate .NET-Installation und keine Administratorrechte nötig. Die App benötigt Lesezugriff auf den ausgewählten Ordner. Die EXE ist nicht signiert; Windows kann abhängig von den Sicherheitseinstellungen eine Warnung anzeigen. Die EXE stammt aus diesem Paket, es gibt keinen Installer.

## Verhalten

- Unterordner stehen vor Dateien; Namen sind alphabetisch sortiert.
- Versteckte Dateien werden ebenfalls angezeigt, sofern lesbar.
- Verknüpfungen/Junctions werden angezeigt, aber nicht verfolgt.
- Nicht lesbare Verzeichnisse werden mit einem Hinweis markiert.
- **Abbrechen** beendet das Einlesen; **Aktualisieren** liest den Ordner erneut ein.
- Schutzgrenzen: 50.000 Einträge insgesamt, 5.000 direkte Einträge pro Ordner, 128 Ebenen und ungefähr 8 Millionen Zeichen. Begrenzungen werden in der Ausgabe markiert. Zähler beziehen sich auf die angezeigten Einträge; die Ausgabe ist bei Begrenzungen unvollständig.
- Dateien werden ausschließlich aufgelistet, nicht geöffnet oder verändert. Die App benötigt keinen Netzwerkzugriff und sendet keine Daten.
- Die integrierte .NET-Laufzeit kann beim ersten Start native Dateien im temporären Benutzerverzeichnis entpacken; portable bedeutet hier ohne Installation, nicht ohne temporäre Dateien.

## Quellcode und eigener Build

Der vollständige Quellcode liegt unter `source/` im Download-Paket (im Repository direkt unter `src/`). Zum Bauen wird das .NET-10-SDK benötigt. In diesem Quellcodeverzeichnis:

```powershell
dotnet run --project tests/SizeTree.Checks -c Release
dotnet publish src/SizeTree/SizeTree.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -o artifacts/SizeTree-win-x64
```

Das Ergebnis liegt in `artifacts/SizeTree-win-x64/SizeTree.exe`. Der Build lässt sich dank `EnableWindowsTargeting` auch unter Linux erzeugen. Zum Ausführen der Oberfläche ist Windows nötig.

Die automatisierten Prüfungen testen die echte Dateisystemauflistung, Baumzeichen, Unicode, Sortierung, Zähler, Verknüpfungsschleifen, Abbruch und fehlende Verzeichnisse. Ein manueller Windows-Test bleibt erforderlich: EXE öffnen, Ordner auswählen, Ausgabe kopieren, TXT speichern und erneut einlesen. Die Windows-Oberfläche wurde in der Linux-Buildumgebung nicht ausgeführt.
