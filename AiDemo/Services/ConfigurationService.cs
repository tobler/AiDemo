using System.Text.Json;
using AiDemo.Models;

namespace AiDemo.Services;

public class ConfigurationService
{
    private readonly string _configPath;
    private AppConfig _config = new();
    private readonly object _lock = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ConfigurationService(IWebHostEnvironment env)
    {
        // Immer im Projektverzeichnis speichern, nicht im bin-Ordner
        _configPath = Path.Combine(env.ContentRootPath, "Data", "config.json");
        Console.WriteLine($"[ConfigService] Config-Pfad: {_configPath}");
        LoadConfig();
    }

    public AppConfig GetConfig()
    {
        lock (_lock) return _config;
    }

    public event Action? OnConfigChanged;

    public void SaveConfig(AppConfig config)
    {
        lock (_lock)
        {
            _config = config;
            Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
            File.WriteAllText(_configPath, JsonSerializer.Serialize(_config, JsonOptions));
            Console.WriteLine($"[ConfigService] Config gespeichert: API-Key={(!string.IsNullOrEmpty(config.ApiKey) ? "gesetzt" : "leer")}, Model={config.Model}");
        }
        OnConfigChanged?.Invoke();
    }

    public void UpdateApiKey(string apiKey)
    {
        _config.ApiKey = apiKey;
        SaveConfig(_config);
    }

    public void UpdateSystemPrompt(string prompt)
    {
        _config.SystemPrompt = prompt;
        SaveConfig(_config);
    }

    public void UpdateModel(string model)
    {
        _config.Model = model;
        SaveConfig(_config);
    }

    public void AddMcpServer(McpServerConfig server)
    {
        _config.McpServers.Add(server);
        SaveConfig(_config);
    }

    public void UpdateMcpServer(McpServerConfig server)
    {
        var index = _config.McpServers.FindIndex(s => s.Id == server.Id);
        if (index >= 0)
        {
            _config.McpServers[index] = server;
            SaveConfig(_config);
        }
    }

    public void RemoveMcpServer(string id)
    {
        _config.McpServers.RemoveAll(s => s.Id == id);
        SaveConfig(_config);
    }

    private void LoadConfig()
    {
        if (File.Exists(_configPath))
        {
            var json = File.ReadAllText(_configPath);
            _config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
            Console.WriteLine($"[ConfigService] Config geladen: API-Key={(!string.IsNullOrEmpty(_config.ApiKey) ? "gesetzt" : "leer")}, Model={_config.Model}");
        }
        else
        {
            Console.WriteLine($"[ConfigService] Keine config.json gefunden, erstelle Default...");
            _config = CreateDefaultConfig();
            SaveConfig(_config);
        }
    }

    private static AppConfig CreateDefaultConfig()
    {
        return new AppConfig
        {
            Model = "claude-3-5-sonnet-latest",
            SystemPrompt = """
                Du bist der KI-Assistent der Firma Musterbau GmbH.
                Die Firma Musterbau GmbH ist ein Handwerksbetrieb, der sich auf Malerarbeiten,
                Bodenbeläge und Trockenbau spezialisiert hat.

                Wenn Du eine Anfrage nach einem Angebot oder Auftrag bekommst:
                1. Benutze "suche_material" oder "material_katalog" um passende Materialien und aktuelle Preise zu finden
                2. Benutze "berechne_aufmass" um die Flächen und Mengen zu berechnen
                3. Benutze "kalkuliere_preis" um die Gesamtpreise inkl. Material und Lohn zu kalkulieren
                4. Erstelle das Angebot als PDF, DOCX oder Excel-Datei mit den entsprechenden Tools

                Sei freundlich und professionell. Antworte auf Deutsch.
                Nenne immer die konkreten Materialien mit Artikelnummer, Hersteller und Preis.
                """,
            McpServers =
            [
                new McpServerConfig
                {
                    Name = "Aufmaß & Kalkulation",
                    Description = "Aufmaß-Berechnung, Materialkatalog und Preiskalkulation",
                    Enabled = true,
                    Tools =
                    [
                        new McpToolConfig
                        {
                            Name = "berechne_aufmass",
                            Description = "Berechnet Aufmaße (Flächen, Volumen) aus gegebenen Maßen. Unterstützt Rechteck, Dreieck, Kreis und zusammengesetzte Flächen.",
                            Parameters = new Dictionary<string, ToolParameter>
                            {
                                ["typ"] = new() { Type = "string", Description = "Art der Berechnung: 'rechteck', 'dreieck', 'kreis', 'raum'", Required = true },
                                ["laenge"] = new() { Type = "number", Description = "Länge in Metern", Required = false },
                                ["breite"] = new() { Type = "number", Description = "Breite in Metern", Required = false },
                                ["hoehe"] = new() { Type = "number", Description = "Höhe in Metern", Required = false },
                                ["radius"] = new() { Type = "number", Description = "Radius in Metern (für Kreis)", Required = false },
                                ["abzuege"] = new() { Type = "string", Description = "Abzüge als JSON-Array, z.B. [{\"typ\":\"rechteck\",\"laenge\":1.2,\"breite\":2.1}] für Fenster/Türen", Required = false }
                            }
                        },
                        new McpToolConfig
                        {
                            Name = "kalkuliere_preis",
                            Description = "Kalkuliert Preise basierend auf Aufmaßen und Leistungsart",
                            Parameters = new Dictionary<string, ToolParameter>
                            {
                                ["leistung"] = new() { Type = "string", Description = "Art der Leistung: 'malerarbeiten', 'tapezieren', 'bodenbelag_laminat', 'bodenbelag_fliesen', 'trockenbau', 'spachteln'", Required = true },
                                ["flaeche_qm"] = new() { Type = "number", Description = "Fläche in Quadratmetern", Required = true },
                                ["qualitaet"] = new() { Type = "string", Description = "Qualitätsstufe: 'standard', 'premium'", Required = false }
                            }
                        },
                        new McpToolConfig
                        {
                            Name = "suche_material",
                            Description = "Durchsucht den Materialkatalog nach Materialien. Kann nach Kategorie, Name oder Artikelnummer filtern.",
                            Parameters = new Dictionary<string, ToolParameter>
                            {
                                ["suchbegriff"] = new() { Type = "string", Description = "Suchbegriff (Name, Kategorie oder Artikelnummer), z.B. 'farbe', 'laminat', 'Knauf', 'F-001'", Required = true },
                                ["kategorie"] = new() { Type = "string", Description = "Optional: Kategorie filtern, z.B. 'farbe', 'laminat', 'tapete', 'platte', 'spachtel'", Required = false }
                            }
                        },
                        new McpToolConfig
                        {
                            Name = "material_katalog",
                            Description = "Gibt den kompletten Materialkatalog oder eine Kategorie zurück mit allen Preisen und Herstellern",
                            Parameters = new Dictionary<string, ToolParameter>
                            {
                                ["kategorie"] = new() { Type = "string", Description = "Optional: Kategorie, z.B. 'farben', 'tapeten', 'boden', 'trockenbau', 'spachtel', 'zubehoer'", Required = false }
                            }
                        }
                    ]
                },
                new McpServerConfig
                {
                    Name = "Dokument-Generator",
                    Description = "Erstellt Dokumente in verschiedenen Formaten",
                    Enabled = true,
                    Tools =
                    [
                        new McpToolConfig
                        {
                            Name = "erstelle_pdf",
                            Description = "Erstellt ein PDF-Dokument mit dem angegebenen Inhalt",
                            Parameters = new Dictionary<string, ToolParameter>
                            {
                                ["titel"] = new() { Type = "string", Description = "Titel des Dokuments", Required = true },
                                ["inhalt"] = new() { Type = "string", Description = "Inhalt des Dokuments (Markdown-Format wird unterstützt)", Required = true },
                                ["firma"] = new() { Type = "string", Description = "Firmenname für den Briefkopf", Required = false }
                            }
                        },
                        new McpToolConfig
                        {
                            Name = "erstelle_docx",
                            Description = "Erstellt ein Word-Dokument (DOCX) mit dem angegebenen Inhalt",
                            Parameters = new Dictionary<string, ToolParameter>
                            {
                                ["titel"] = new() { Type = "string", Description = "Titel des Dokuments", Required = true },
                                ["inhalt"] = new() { Type = "string", Description = "Inhalt des Dokuments", Required = true },
                                ["firma"] = new() { Type = "string", Description = "Firmenname für den Briefkopf", Required = false }
                            }
                        },
                        new McpToolConfig
                        {
                            Name = "erstelle_excel",
                            Description = "Erstellt eine Excel-Datei mit tabellarischen Daten",
                            Parameters = new Dictionary<string, ToolParameter>
                            {
                                ["titel"] = new() { Type = "string", Description = "Titel der Tabelle", Required = true },
                                ["spalten"] = new() { Type = "string", Description = "Spaltennamen als kommaseparierte Liste", Required = true },
                                ["zeilen"] = new() { Type = "string", Description = "Zeilen als JSON-Array von Arrays, z.B. [[\"Pos 1\",\"10\",\"25.00\"],[\"Pos 2\",\"5\",\"30.00\"]]", Required = true }
                            }
                        }
                    ]
                }
            ]
        };
    }
}
