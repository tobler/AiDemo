# KI Demo - Anthropic AI Chat mit MCP Tools

Eine Blazor Server Demo-Anwendung, die zeigt wie man die Anthropic Claude API mit MCP (Model Context Protocol) Tools kombiniert, um einen intelligenten KI-Assistenten mit Dokumentenerstellung zu bauen.

## Features

- **Chat-Interface** - Konversation mit Claude inkl. Chat-History
- **Systemprompt-Verwaltung** - Konfigurierbar über die Web-Oberfläche
- **MCP Server Verwaltung** - Tools dynamisch hinzufügen, bearbeiten und konfigurieren
- **Materialkatalog** - 35+ Materialien mit Preisen (Farben, Tapeten, Bodenbeläge, Trockenbau)
- **Aufmaß-Berechnung** - Flächen und Volumen für Rechteck, Dreieck, Kreis, Raum (mit Abzügen)
- **Preiskalkulation** - Automatische Kalkulation mit Material- und Lohnkosten
- **Dokumentenerstellung** - PDF, Word (DOCX) und Excel (XLSX) direkt im Chat generieren

## Architektur

```
AiDemo/                     Blazor Server App
├── Components/Pages/
│   ├── Chat.razor          Chat-Interface mit Prompt-Eingabe
│   ├── SystemPromptPage    API-Key, Modell & Systemprompt
│   └── McpServersPage      MCP Server & Tool Verwaltung
├── Services/
│   ├── AnthropicChatService   Anthropic API mit Tool Use Loop
│   ├── ConfigurationService   Konfiguration (JSON-basiert)
│   ├── McpToolService         Tool-Ausführung & Materialkatalog
│   └── DocumentService        PDF/DOCX/Excel Generierung
└── Models/                    Datenmodelle

McpDemoServer/              Eigenständiger MCP Server (stdio)
├── AufmassTools.cs         berechne_aufmass, kalkuliere_preis,
│                           suche_material, material_katalog
└── Program.cs
```

## Voraussetzungen

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Anthropic API Key](https://console.anthropic.com/)

## Schnellstart

```bash
# Repository klonen
git clone https://github.com/tobler/AiDemo.git
cd AiDemo

# Packages wiederherstellen und starten
dotnet run --project AiDemo
```

Dann im Browser:

1. **Systemprompt-Seite** aufrufen und den Anthropic API-Key eintragen
2. Modell wählen (z.B. `claude-3-5-sonnet-latest`)
3. Zur **Chat-Seite** wechseln und loslegen

## Beispiel-Prompts

> Ich hätte gerne ein Angebot über Malerarbeiten für ein Wohnzimmer. Der Raum ist 5m lang, 4m breit und 2,50m hoch. Es gibt 2 Fenster (1,2m x 1,4m) und eine Tür (0,9m x 2,1m). Bitte erstelle das Angebot als PDF.

> Welche Bodenbeläge habt ihr im Sortiment und was kosten die?

> Berechne das Aufmaß für einen Raum: 7m x 5m x 2,80m mit 3 Fenstern und 2 Türen.

## Verfügbare Tools

| Tool | Beschreibung |
|------|-------------|
| `berechne_aufmass` | Flächen/Volumen-Berechnung (Rechteck, Dreieck, Kreis, Raum) |
| `kalkuliere_preis` | Preiskalkulation mit Material + Lohn (6 Leistungsarten) |
| `suche_material` | Materialkatalog durchsuchen |
| `material_katalog` | Kompletten Katalog oder Kategorie abrufen |
| `erstelle_pdf` | PDF-Dokument generieren |
| `erstelle_docx` | Word-Dokument generieren |
| `erstelle_excel` | Excel-Tabelle generieren |

## Materialpreise (Demo)

Der integrierte Katalog enthält realistische Beispielpreise für:

- **Farben & Lacke** - Alpina, Caparol, Pufas, Bondex
- **Tapeten** - Erfurt, Marburg, Vitrulan
- **Bodenbeläge** - Parador, Wineo, Villeroy&Boch, Sopro
- **Trockenbau** - Knauf, Isover, Würth
- **Spachtel & Putz** - Knauf

## Demo MCP Server

Der `McpDemoServer` ist ein eigenständiger MCP-Server, der über stdio kommuniziert:

```bash
dotnet run --project McpDemoServer
```

Er implementiert dieselben Tools wie die integrierte Version und kann als Referenz für eigene MCP-Server dienen.

## Technologien

- **ASP.NET Core Blazor Server** (.NET 10)
- **Anthropic Claude API** (Messages API mit Tool Use)
- **QuestPDF** - PDF-Generierung
- **ClosedXML** - Excel-Generierung
- **Open XML SDK** - Word-Generierung
- **ModelContextProtocol** - MCP Server SDK

## Lizenz

MIT
