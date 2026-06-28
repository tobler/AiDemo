using System.Text.Json;
using AiDemo.Models;

namespace AiDemo.Services;

/// <summary>
/// Simulierter Materialkatalog - kommt später aus der Datenbank.
/// </summary>
public static class Materialkatalog
{
    public record Material(string Artikelnummer, string Name, string Kategorie, string Einheit, double PreisNetto, string Hersteller);

    public static readonly List<Material> Alle =
    [
        // Farben & Lacke
        new("F-001", "Wandfarbe Weiß Matt 10L",          "farbe",       "Eimer",  32.90, "Alpina"),
        new("F-002", "Wandfarbe Weiß Matt 2,5L",         "farbe",       "Eimer",  12.50, "Alpina"),
        new("F-003", "Silikonharzfarbe Außen 10L",        "farbe",       "Eimer",  54.90, "Caparol"),
        new("F-004", "Tiefengrund LF 10L",                "grundierung", "Eimer",  24.90, "Pufas"),
        new("F-005", "Acryl-Buntlack seidenmatt 0,75L",  "lack",        "Dose",   14.90, "Alpina"),
        new("F-006", "Holzschutzlasur 2,5L",              "lack",        "Dose",   29.90, "Bondex"),

        // Tapeten
        new("T-001", "Raufasertapete Klassik 33,5m",      "tapete",      "Rolle",   8.50, "Erfurt"),
        new("T-002", "Vliestapete Uni Weiß 10m",          "tapete",      "Rolle",  12.90, "Marburg"),
        new("T-003", "Glasfasertapete 25m",                "tapete",      "Rolle",  28.50, "Vitrulan"),
        new("T-004", "Tapetenkleister Normal 200g",        "kleber",      "Paket",   3.90, "Pufas"),
        new("T-005", "Tapetenkleister Spezial Vlies 200g", "kleber",     "Paket",   5.50, "Metylan"),

        // Bodenbeläge
        new("B-001", "Laminat Eiche Natur 2,49m²",        "laminat",     "Paket",  24.90, "Parador"),
        new("B-002", "Laminat Eiche Grau 2,49m²",         "laminat",     "Paket",  29.90, "Parador"),
        new("B-003", "Vinyl-Designboden Eiche 2,24m²",    "vinyl",       "Paket",  34.90, "Wineo"),
        new("B-004", "Trittschalldämmung 15m²",           "daemmung",    "Rolle",  19.90, "Selit"),
        new("B-005", "Sockelleiste Eiche 240cm",           "leiste",      "Stück",   4.90, "Parador"),
        new("B-006", "Bodenfliese Feinsteinzeug 60x60",   "fliese",      "m²",     28.50, "Villeroy&Boch"),
        new("B-007", "Fliesenkleber Flex 25kg",            "kleber",      "Sack",   18.90, "Sopro"),
        new("B-008", "Fugenmörtel Grau 5kg",               "fuge",        "Beutel",  8.90, "Sopro"),

        // Trockenbau
        new("D-001", "Gipskartonplatte 260x60cm 12,5mm",  "platte",      "Stück",   5.90, "Knauf"),
        new("D-002", "Gipskartonplatte Feuchtraum 260x60", "platte",     "Stück",   8.50, "Knauf"),
        new("D-003", "CW-Profil 75mm 260cm",              "profil",      "Stück",   3.20, "Knauf"),
        new("D-004", "UW-Profil 75mm 300cm",              "profil",      "Stück",   3.50, "Knauf"),
        new("D-005", "Mineralwolle 60mm WLG035 4,32m²",   "daemmung",    "Paket",  22.90, "Isover"),
        new("D-006", "Schnellbauschrauben 3,9x25 1000St", "schraube",    "Paket",  12.90, "Würth"),

        // Spachtel & Putz
        new("S-001", "Fugenspachtel 25kg",                 "spachtel",    "Sack",   14.90, "Knauf"),
        new("S-002", "Feinspachtelmasse 20kg",             "spachtel",    "Sack",   18.50, "Knauf"),
        new("S-003", "Bewehrungsstreifen 50mm 90m",        "band",        "Rolle",   4.90, "Knauf"),
        new("S-004", "Gipsputz MP75 30kg",                 "putz",        "Sack",   11.90, "Knauf"),

        // Werkzeug & Zubehör (Verbrauch)
        new("Z-001", "Abdeckfolie 4x5m",                  "abdeckung",   "Stück",   2.50, "Verschiedene"),
        new("Z-002", "Kreppband 50mm x 50m",              "klebeband",   "Rolle",   3.90, "Tesa"),
        new("Z-003", "Farbwalze 25cm",                     "werkzeug",    "Stück",   4.50, "Verschiedene"),
    ];
}

public class McpToolService
{
    private readonly DocumentService _documentService;

    public McpToolService(DocumentService documentService)
    {
        _documentService = documentService;
    }

    public async Task<(string result, GeneratedDocument? document)> ExecuteToolAsync(string toolName, JsonElement input)
    {
        return toolName switch
        {
            "berechne_aufmass" => (BerechneAufmass(input), null),
            "kalkuliere_preis" => (KalkulierePreis(input), null),
            "suche_material" => (SucheMaterial(input), null),
            "material_katalog" => (MaterialKatalogTool(input), null),
            "erstelle_pdf" => await ErstellePdf(input),
            "erstelle_docx" => await ErstelleDocx(input),
            "erstelle_excel" => await ErstelleExcel(input),
            _ => ($"Unbekanntes Tool: {toolName}", null)
        };
    }

    private static string BerechneAufmass(JsonElement input)
    {
        var typ = input.GetProperty("typ").GetString() ?? "rechteck";

        switch (typ.ToLower())
        {
            case "rechteck":
            {
                var laenge = GetDouble(input, "laenge");
                var breite = GetDouble(input, "breite");
                return JsonSerializer.Serialize(new
                {
                    typ, flaeche = Math.Round(laenge * breite, 2),
                    umfang = Math.Round(2 * (laenge + breite), 2), einheit = "m²"
                });
            }
            case "dreieck":
            {
                var laenge = GetDouble(input, "laenge");
                var hoehe = GetDouble(input, "hoehe");
                return JsonSerializer.Serialize(new
                {
                    typ, flaeche = Math.Round(0.5 * laenge * hoehe, 2), einheit = "m²"
                });
            }
            case "kreis":
            {
                var radius = GetDouble(input, "radius");
                return JsonSerializer.Serialize(new
                {
                    typ, flaeche = Math.Round(Math.PI * radius * radius, 2),
                    umfang = Math.Round(2 * Math.PI * radius, 2), einheit = "m²"
                });
            }
            case "raum":
            {
                var laenge = GetDouble(input, "laenge");
                var breite = GetDouble(input, "breite");
                var hoehe = GetDouble(input, "hoehe");
                var bodenflaeche = laenge * breite;
                var wandflaeche = 2 * (laenge + breite) * hoehe;
                var volumen = laenge * breite * hoehe;

                double abzugFlaeche = 0;
                if (input.TryGetProperty("abzuege", out var abzuegeElement))
                {
                    var abzuegeStr = abzuegeElement.GetString();
                    if (!string.IsNullOrEmpty(abzuegeStr))
                    {
                        var abzuege = JsonSerializer.Deserialize<List<JsonElement>>(abzuegeStr);
                        if (abzuege != null)
                        {
                            foreach (var abzug in abzuege)
                            {
                                if (abzug.TryGetProperty("laenge", out _) && abzug.TryGetProperty("breite", out _))
                                    abzugFlaeche += GetDouble(abzug, "laenge") * GetDouble(abzug, "breite");
                            }
                        }
                    }
                }

                return JsonSerializer.Serialize(new
                {
                    typ = "raum",
                    bodenflaeche = Math.Round(bodenflaeche, 2),
                    wandflaeche = Math.Round(wandflaeche, 2),
                    wandflaeche_netto = Math.Round(wandflaeche - abzugFlaeche, 2),
                    deckenflaeche = Math.Round(bodenflaeche, 2),
                    volumen = Math.Round(volumen, 2),
                    abzuege_gesamt = Math.Round(abzugFlaeche, 2),
                    einheit = "m²/m³"
                });
            }
            default:
                return JsonSerializer.Serialize(new { fehler = $"Unbekannter Typ: {typ}" });
        }
    }

    private static string SucheMaterial(JsonElement input)
    {
        var suchbegriff = (input.GetProperty("suchbegriff").GetString() ?? "").ToLower();
        string? kategorie = null;
        if (input.TryGetProperty("kategorie", out var k))
            kategorie = k.GetString();

        var treffer = Materialkatalog.Alle
            .Where(m =>
                m.Artikelnummer.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase) ||
                m.Kategorie.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase) ||
                m.Hersteller.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase))
            .Where(m => kategorie == null || m.Kategorie.Equals(kategorie, StringComparison.OrdinalIgnoreCase))
            .Select(m => new
            {
                artikelnummer = m.Artikelnummer, name = m.Name, kategorie = m.Kategorie,
                einheit = m.Einheit, preis_netto = m.PreisNetto,
                preis_brutto = Math.Round(m.PreisNetto * 1.19, 2), hersteller = m.Hersteller
            })
            .ToList();

        return JsonSerializer.Serialize(new { anzahl_treffer = treffer.Count, materialien = treffer });
    }

    private static string MaterialKatalogTool(JsonElement input)
    {
        string? kategorie = null;
        if (input.TryGetProperty("kategorie", out var k))
            kategorie = k.GetString();

        var kategorieGruppen = new Dictionary<string, string[]>
        {
            ["farben"] = ["farbe", "grundierung", "lack"],
            ["tapeten"] = ["tapete", "kleber"],
            ["boden"] = ["laminat", "vinyl", "fliese", "daemmung", "leiste", "kleber", "fuge"],
            ["trockenbau"] = ["platte", "profil", "daemmung", "schraube"],
            ["spachtel"] = ["spachtel", "putz", "band"],
            ["zubehoer"] = ["abdeckung", "klebeband", "werkzeug"]
        };

        var materialien = Materialkatalog.Alle.AsEnumerable();
        if (!string.IsNullOrEmpty(kategorie))
        {
            var kat = kategorie.ToLower();
            if (kategorieGruppen.TryGetValue(kat, out var unterKategorien))
                materialien = materialien.Where(m => unterKategorien.Contains(m.Kategorie));
            else
                materialien = materialien.Where(m => m.Kategorie.Equals(kat, StringComparison.OrdinalIgnoreCase));
        }

        var result = materialien.GroupBy(m => m.Kategorie)
            .Select(g => new
            {
                kategorie = g.Key,
                materialien = g.Select(m => new
                {
                    artikelnummer = m.Artikelnummer, name = m.Name, einheit = m.Einheit,
                    preis_netto = m.PreisNetto, preis_brutto = Math.Round(m.PreisNetto * 1.19, 2),
                    hersteller = m.Hersteller
                }).ToList()
            }).ToList();

        return JsonSerializer.Serialize(new
        {
            anzahl_kategorien = result.Count,
            anzahl_materialien = result.Sum(r => r.materialien.Count),
            katalog = result
        });
    }

    private static string KalkulierePreis(JsonElement input)
    {
        var leistung = input.GetProperty("leistung").GetString() ?? "";
        var flaecheQm = GetDouble(input, "flaeche_qm");
        var qualitaet = "standard";
        if (input.TryGetProperty("qualitaet", out var q))
            qualitaet = q.GetString() ?? "standard";

        // Lohnkosten pro m²
        var lohnProQm = new Dictionary<string, double>
        {
            ["malerarbeiten"] = 8.00, ["tapezieren"] = 10.00,
            ["bodenbelag_laminat"] = 15.00, ["bodenbelag_fliesen"] = 25.00,
            ["trockenbau"] = 22.00, ["spachteln"] = 6.00
        };

        // Materialbedarf pro m² (Artikelnr, Menge/m², Premium-Alternative)
        var materialBedarf = new Dictionary<string, List<(string artikelNr, double mengeProQm, string? premiumArtikelNr)>>
        {
            ["malerarbeiten"] = [("F-001", 0.1, null), ("F-004", 0.05, null), ("Z-001", 0.05, null), ("Z-002", 0.02, null)],
            ["tapezieren"] = [("T-001", 0.03, "T-002"), ("T-004", 0.15, "T-005"), ("F-004", 0.05, null)],
            ["bodenbelag_laminat"] = [("B-001", 0.4, "B-002"), ("B-004", 0.067, null), ("B-005", 0.25, null)],
            ["bodenbelag_fliesen"] = [("B-006", 1.1, null), ("B-007", 0.14, null), ("B-008", 0.2, null)],
            ["trockenbau"] = [("D-001", 0.64, "D-002"), ("D-003", 0.4, null), ("D-004", 0.15, null), ("D-005", 0.23, null), ("D-006", 0.015, null)],
            ["spachteln"] = [("S-001", 0.04, "S-002"), ("S-003", 0.1, null)]
        };

        if (!lohnProQm.TryGetValue(leistung.ToLower(), out var lohn))
        {
            return JsonSerializer.Serialize(new
            {
                fehler = $"Unbekannte Leistung: {leistung}. Verfügbar: {string.Join(", ", lohnProQm.Keys)}"
            });
        }

        var materialPositionen = new List<object>();
        double materialGesamt = 0;

        if (materialBedarf.TryGetValue(leistung.ToLower(), out var bedarf))
        {
            foreach (var (artikelNr, mengeProQm, premiumArtikelNr) in bedarf)
            {
                var aktuellerArtikel = (qualitaet == "premium" && premiumArtikelNr != null) ? premiumArtikelNr : artikelNr;
                var material = Materialkatalog.Alle.FirstOrDefault(m => m.Artikelnummer == aktuellerArtikel);
                if (material == null) continue;

                var menge = Math.Ceiling(mengeProQm * flaecheQm * 10) / 10;
                var posPreis = Math.Round(menge * material.PreisNetto, 2);
                materialGesamt += posPreis;

                materialPositionen.Add(new
                {
                    artikelnummer = material.Artikelnummer, name = material.Name,
                    hersteller = material.Hersteller, menge, einheit = material.Einheit,
                    einzelpreis = material.PreisNetto, gesamtpreis = posPreis
                });
            }
        }

        var lohnGesamt = Math.Round(lohn * flaecheQm, 2);
        if (qualitaet == "premium") lohnGesamt = Math.Round(lohnGesamt * 1.3, 2);

        var netto = Math.Round(materialGesamt + lohnGesamt, 2);
        var mwst = Math.Round(netto * 0.19, 2);

        return JsonSerializer.Serialize(new
        {
            leistung, qualitaet, flaeche_qm = flaecheQm,
            material_positionen = materialPositionen,
            material_gesamt = materialGesamt,
            lohn_gesamt = lohnGesamt,
            netto, mwst_19_prozent = mwst, brutto = Math.Round(netto + mwst, 2)
        });
    }

    private async Task<(string, GeneratedDocument?)> ErstellePdf(JsonElement input)
    {
        var titel = input.GetProperty("titel").GetString() ?? "Dokument";
        var inhalt = input.GetProperty("inhalt").GetString() ?? "";
        var firma = "";
        if (input.TryGetProperty("firma", out var f)) firma = f.GetString() ?? "";
        var doc = await _documentService.CreatePdfAsync(titel, inhalt, firma);
        return ($"PDF-Dokument '{titel}' wurde erstellt ({doc.Data.Length} Bytes).", doc);
    }

    private async Task<(string, GeneratedDocument?)> ErstelleDocx(JsonElement input)
    {
        var titel = input.GetProperty("titel").GetString() ?? "Dokument";
        var inhalt = input.GetProperty("inhalt").GetString() ?? "";
        var firma = "";
        if (input.TryGetProperty("firma", out var f)) firma = f.GetString() ?? "";
        var doc = await _documentService.CreateDocxAsync(titel, inhalt, firma);
        return ($"Word-Dokument '{titel}' wurde erstellt ({doc.Data.Length} Bytes).", doc);
    }

    private async Task<(string, GeneratedDocument?)> ErstelleExcel(JsonElement input)
    {
        var titel = input.GetProperty("titel").GetString() ?? "Tabelle";
        var spaltenStr = input.GetProperty("spalten").GetString() ?? "";
        var zeilenStr = input.GetProperty("zeilen").GetString() ?? "[]";
        var spalten = spaltenStr.Split(',', StringSplitOptions.TrimEntries).ToList();
        var zeilen = JsonSerializer.Deserialize<List<List<string>>>(zeilenStr) ?? [];
        var doc = await _documentService.CreateExcelAsync(titel, spalten, zeilen);
        return ($"Excel-Datei '{titel}' wurde erstellt ({doc.Data.Length} Bytes).", doc);
    }

    private static double GetDouble(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var val)) return 0;
        return val.ValueKind switch
        {
            JsonValueKind.Number => val.GetDouble(),
            JsonValueKind.String => double.TryParse(val.GetString(), out var d) ? d : 0,
            _ => 0
        };
    }
}
