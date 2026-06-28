using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace McpDemoServer;

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

[McpServerToolType]
public static class AufmassTools
{
    [McpServerTool(Name = "berechne_aufmass")]
    [Description("Berechnet Aufmaße (Flächen, Volumen) aus gegebenen Maßen. Unterstützt Rechteck, Dreieck, Kreis und Raum-Berechnungen.")]
    public static string BerechneAufmass(
        [Description("Art der Berechnung: 'rechteck', 'dreieck', 'kreis', 'raum'")] string typ,
        [Description("Länge in Metern")] double laenge = 0,
        [Description("Breite in Metern")] double breite = 0,
        [Description("Höhe in Metern")] double hoehe = 0,
        [Description("Radius in Metern (für Kreis)")] double radius = 0,
        [Description("Abzüge als JSON-Array, z.B. [{\"typ\":\"rechteck\",\"laenge\":1.2,\"breite\":2.1}]")] string? abzuege = null)
    {
        switch (typ.ToLower())
        {
            case "rechteck":
                return JsonSerializer.Serialize(new
                {
                    typ,
                    flaeche = Math.Round(laenge * breite, 2),
                    umfang = Math.Round(2 * (laenge + breite), 2),
                    einheit = "m²"
                });

            case "dreieck":
                return JsonSerializer.Serialize(new
                {
                    typ,
                    flaeche = Math.Round(0.5 * laenge * hoehe, 2),
                    einheit = "m²"
                });

            case "kreis":
                return JsonSerializer.Serialize(new
                {
                    typ,
                    flaeche = Math.Round(Math.PI * radius * radius, 2),
                    umfang = Math.Round(2 * Math.PI * radius, 2),
                    einheit = "m²"
                });

            case "raum":
                var bodenflaeche = laenge * breite;
                var wandflaeche = 2 * (laenge + breite) * hoehe;
                var deckenflaeche = bodenflaeche;
                var volumen = laenge * breite * hoehe;

                double abzugFlaeche = 0;
                if (!string.IsNullOrEmpty(abzuege))
                {
                    var abzugList = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(abzuege);
                    if (abzugList != null)
                    {
                        foreach (var abzug in abzugList)
                        {
                            if (abzug.TryGetValue("laenge", out var aL) && abzug.TryGetValue("breite", out var aB))
                            {
                                abzugFlaeche += aL.GetDouble() * aB.GetDouble();
                            }
                        }
                    }
                }

                return JsonSerializer.Serialize(new
                {
                    typ,
                    bodenflaeche = Math.Round(bodenflaeche, 2),
                    wandflaeche = Math.Round(wandflaeche, 2),
                    wandflaeche_netto = Math.Round(wandflaeche - abzugFlaeche, 2),
                    deckenflaeche = Math.Round(deckenflaeche, 2),
                    volumen = Math.Round(volumen, 2),
                    abzuege_gesamt = Math.Round(abzugFlaeche, 2),
                    einheit = "m²/m³"
                });

            default:
                return JsonSerializer.Serialize(new { fehler = $"Unbekannter Typ: {typ}" });
        }
    }

    [McpServerTool(Name = "suche_material")]
    [Description("Durchsucht den Materialkatalog nach Materialien. Kann nach Kategorie, Name oder Artikelnummer filtern. Gibt Preise, Hersteller und verfügbare Materialien zurück.")]
    public static string SucheMaterial(
        [Description("Suchbegriff (Name, Kategorie oder Artikelnummer). Beispiele: 'farbe', 'laminat', 'Knauf', 'F-001'")] string suchbegriff,
        [Description("Optional: Kategorie filtern: 'farbe', 'grundierung', 'lack', 'tapete', 'kleber', 'laminat', 'vinyl', 'fliese', 'daemmung', 'leiste', 'fuge', 'platte', 'profil', 'schraube', 'spachtel', 'putz', 'band', 'abdeckung', 'klebeband', 'werkzeug'")] string? kategorie = null)
    {
        var suche = suchbegriff.ToLower();

        var treffer = Materialkatalog.Alle
            .Where(m =>
                m.Artikelnummer.Contains(suche, StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains(suche, StringComparison.OrdinalIgnoreCase) ||
                m.Kategorie.Contains(suche, StringComparison.OrdinalIgnoreCase) ||
                m.Hersteller.Contains(suche, StringComparison.OrdinalIgnoreCase))
            .Where(m => kategorie == null || m.Kategorie.Equals(kategorie, StringComparison.OrdinalIgnoreCase))
            .Select(m => new
            {
                artikelnummer = m.Artikelnummer,
                name = m.Name,
                kategorie = m.Kategorie,
                einheit = m.Einheit,
                preis_netto = m.PreisNetto,
                preis_brutto = Math.Round(m.PreisNetto * 1.19, 2),
                hersteller = m.Hersteller
            })
            .ToList();

        return JsonSerializer.Serialize(new
        {
            anzahl_treffer = treffer.Count,
            materialien = treffer
        });
    }

    [McpServerTool(Name = "material_katalog")]
    [Description("Gibt den kompletten Materialkatalog oder eine bestimmte Kategorie zurück. Nützlich um alle verfügbaren Materialien und Preise zu sehen.")]
    public static string MaterialKatalog(
        [Description("Optional: Nur eine bestimmte Kategorie anzeigen, z.B. 'farbe', 'laminat', 'tapete', 'trockenbau'. Ohne Angabe wird alles zurückgegeben.")] string? kategorie = null)
    {
        // Kategorie-Mapping für übergeordnete Gruppen
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
            {
                materialien = materialien.Where(m => unterKategorien.Contains(m.Kategorie));
            }
            else
            {
                materialien = materialien.Where(m =>
                    m.Kategorie.Equals(kat, StringComparison.OrdinalIgnoreCase));
            }
        }

        var result = materialien
            .GroupBy(m => m.Kategorie)
            .Select(g => new
            {
                kategorie = g.Key,
                materialien = g.Select(m => new
                {
                    artikelnummer = m.Artikelnummer,
                    name = m.Name,
                    einheit = m.Einheit,
                    preis_netto = m.PreisNetto,
                    preis_brutto = Math.Round(m.PreisNetto * 1.19, 2),
                    hersteller = m.Hersteller
                }).ToList()
            })
            .ToList();

        return JsonSerializer.Serialize(new
        {
            anzahl_kategorien = result.Count,
            anzahl_materialien = result.Sum(r => r.materialien.Count),
            katalog = result
        });
    }

    [McpServerTool(Name = "kalkuliere_preis")]
    [Description("Kalkuliert den Gesamtpreis für eine Leistung inkl. Material und Lohn. Nutzt die Materialpreise aus dem Katalog und rechnet Arbeitslohn dazu.")]
    public static string KalkulierePreis(
        [Description("Art der Leistung: 'malerarbeiten', 'tapezieren', 'bodenbelag_laminat', 'bodenbelag_fliesen', 'trockenbau', 'spachteln'")] string leistung,
        [Description("Fläche in Quadratmetern")] double flaeche_qm,
        [Description("Qualitätsstufe: 'standard', 'premium'")] string qualitaet = "standard")
    {
        // Lohnkosten pro m² je Leistung
        var lohnProQm = new Dictionary<string, double>
        {
            ["malerarbeiten"] = 8.00,
            ["tapezieren"] = 10.00,
            ["bodenbelag_laminat"] = 15.00,
            ["bodenbelag_fliesen"] = 25.00,
            ["trockenbau"] = 22.00,
            ["spachteln"] = 6.00
        };

        // Materialbedarf pro m² (Artikelnummer -> Menge pro m²)
        var materialBedarf = new Dictionary<string, List<(string artikelNr, double mengeProQm, string? premiumArtikelNr)>>
        {
            ["malerarbeiten"] =
            [
                ("F-001", 0.1, null),    // 1 Eimer 10L reicht für ~100m² => 0.1 Eimer/m²
                ("F-004", 0.05, null),   // Tiefengrund
                ("Z-001", 0.05, null),   // Abdeckfolie
                ("Z-002", 0.02, null),   // Kreppband
            ],
            ["tapezieren"] =
            [
                ("T-001", 0.03, "T-002"),  // Raufaser standard, Vlies premium (1 Rolle ~33m² => 0.03)
                ("T-004", 0.15, "T-005"),  // Kleister
                ("F-004", 0.05, null),     // Tiefengrund
            ],
            ["bodenbelag_laminat"] =
            [
                ("B-001", 0.4, "B-002"),  // Laminat (1 Paket = 2,49m² => 0.4 Pakete/m²)
                ("B-004", 0.067, null),   // Trittschalldämmung (1 Rolle = 15m²)
                ("B-005", 0.25, null),    // Sockelleisten (ca. 1 pro 4m Umfang, grob 0.25/m²)
            ],
            ["bodenbelag_fliesen"] =
            [
                ("B-006", 1.1, null),     // Fliesen inkl. Verschnitt
                ("B-007", 0.14, null),    // Fliesenkleber (1 Sack = ~7m²)
                ("B-008", 0.2, null),     // Fugenmörtel
            ],
            ["trockenbau"] =
            [
                ("D-001", 0.64, "D-002"), // GK-Platten (1 Platte = 1,56m²)
                ("D-003", 0.4, null),     // CW-Profile
                ("D-004", 0.15, null),    // UW-Profile
                ("D-005", 0.23, null),    // Mineralwolle (1 Paket = 4,32m²)
                ("D-006", 0.015, null),   // Schrauben
            ],
            ["spachteln"] =
            [
                ("S-001", 0.04, "S-002"), // Spachtelmasse
                ("S-003", 0.1, null),     // Bewehrungsstreifen
            ]
        };

        if (!lohnProQm.TryGetValue(leistung.ToLower(), out var lohn))
        {
            return JsonSerializer.Serialize(new { fehler = $"Unbekannte Leistung: {leistung}. Verfügbar: {string.Join(", ", lohnProQm.Keys)}" });
        }

        // Materialkosten berechnen
        var materialPositionen = new List<object>();
        double materialGesamt = 0;

        if (materialBedarf.TryGetValue(leistung.ToLower(), out var bedarf))
        {
            foreach (var (artikelNr, mengeProQm, premiumArtikelNr) in bedarf)
            {
                var aktuellerArtikel = (qualitaet == "premium" && premiumArtikelNr != null)
                    ? premiumArtikelNr
                    : artikelNr;

                var material = Materialkatalog.Alle.FirstOrDefault(m => m.Artikelnummer == aktuellerArtikel);
                if (material == null) continue;

                var menge = Math.Ceiling(mengeProQm * flaeche_qm * 10) / 10; // auf 0.1 aufrunden
                var posPreis = Math.Round(menge * material.PreisNetto, 2);
                materialGesamt += posPreis;

                materialPositionen.Add(new
                {
                    artikelnummer = material.Artikelnummer,
                    name = material.Name,
                    hersteller = material.Hersteller,
                    menge = menge,
                    einheit = material.Einheit,
                    einzelpreis = material.PreisNetto,
                    gesamtpreis = posPreis
                });
            }
        }

        var lohnGesamt = Math.Round(lohn * flaeche_qm, 2);
        if (qualitaet == "premium") lohnGesamt = Math.Round(lohnGesamt * 1.3, 2); // 30% Aufschlag premium

        var netto = Math.Round(materialGesamt + lohnGesamt, 2);
        var mwst = Math.Round(netto * 0.19, 2);

        return JsonSerializer.Serialize(new
        {
            leistung,
            qualitaet,
            flaeche_qm,
            material_positionen = materialPositionen,
            material_gesamt = materialGesamt,
            lohn_gesamt = lohnGesamt,
            netto,
            mwst_19_prozent = mwst,
            brutto = Math.Round(netto + mwst, 2)
        });
    }
}
