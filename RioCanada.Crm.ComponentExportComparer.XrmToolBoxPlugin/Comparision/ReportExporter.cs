using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace RioCanada.Crm.ComponentExportComparer.XrmToolBoxPlugin.Comparision
{
    /// <summary>
    /// Generates HTML and XLSX comparison reports from flat <see cref="ComparisionLineItem"/> data.
    /// No external NuGet packages are required: XLSX uses Open XML SpreadsheetML via System.IO.Packaging (GAC .NET 4.8).
    /// </summary>
    internal static class ReportExporter
    {
        // ──────────────────────────────────────────────────────────────────────
        //  Public entry points
        // ──────────────────────────────────────────────────────────────────────

        public static void ExportHtml(IEnumerable<ComparisionLineItem> items, string outputPath)
        {
            var rows = Flatten(items);
            var html = BuildHtml(rows);
            File.WriteAllText(outputPath, html, Encoding.UTF8);
        }

        public static void ExportXlsx(IEnumerable<ComparisionLineItem> items, string outputPath)
        {
            var rows = Flatten(items);
            BuildXlsx(rows, outputPath);
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Flatten: depth-first traversal, only leaf nodes (files)
        // ──────────────────────────────────────────────────────────────────────

        private static List<ReportRow> Flatten(IEnumerable<ComparisionLineItem> items, string parentPath = "")
        {
            var result = new List<ReportRow>();
            if (items == null) return result;

            foreach (var item in items)
            {
                var path = string.IsNullOrEmpty(parentPath) ? item.Name : parentPath + " / " + item.Name;

                if (item.Children != null && item.Children.Count > 0)
                {
                    result.AddRange(Flatten(item.Children, path));
                }
                else
                {
                    result.Add(new ReportRow
                    {
                        Component  = path,
                        Type       = item.ContentType ?? item.Type.ToString(),
                        Status     = StatusLabel(item.Status),
                        StatusCode = item.Status,
                        Checksum   = SourceChecksum(item),
                    });
                }
            }

            return result;
        }

        private static string SourceChecksum(ComparisionLineItem item)
        {
            if (item.SourceItem != null && !string.IsNullOrEmpty(item.SourceItem.Checksum))
                return item.SourceItem.Checksum;
            if (item.TargetItem != null && !string.IsNullOrEmpty(item.TargetItem.Checksum))
                return item.TargetItem.Checksum;
            return string.Empty;
        }

        private static string StatusLabel(ComparisionStatus s)
        {
            switch (s)
            {
                case ComparisionStatus.Unchanged:    return "Unchanged";
                case ComparisionStatus.Modified:     return "Modified";
                case ComparisionStatus.OnlyInSource: return "Only in source";
                case ComparisionStatus.OnlyInTarget: return "Only in target";
                default:                             return s.ToString();
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        //  HTML generation
        // ──────────────────────────────────────────────────────────────────────

        private static string BuildHtml(List<ReportRow> rows)
        {
            var summary = BuildSummary(rows);
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"UTF-8\">");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">");
            sb.AppendLine("<title>CRM Component Comparison Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(HtmlStyle());
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // ── Header ────────────────────────────────────────────────────────
            sb.AppendLine("<div class=\"header\">");
            sb.AppendLine("  <h1>&#128202; CRM Component Comparison Report</h1>");
            sb.AppendLine($"  <p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss} &nbsp;|&nbsp; Total items: {rows.Count}</p>");
            sb.AppendLine("</div>");

            // ── Summary cards ─────────────────────────────────────────────────
            sb.AppendLine("<div class=\"cards\">");
            sb.AppendLine(Card("Unchanged",    summary.Unchanged,    "#6c757d", "&#9679;"));
            sb.AppendLine(Card("Modified",     summary.Modified,     "#fd7e14", "&#9998;"));
            sb.AppendLine(Card("Only in Src",  summary.OnlyInSource, "#28a745", "&#43;"));
            sb.AppendLine(Card("Only in Tgt",  summary.OnlyInTarget, "#dc3545", "&#8722;"));
            sb.AppendLine("</div>");

            // ── Donut chart (pure SVG) ────────────────────────────────────────
            sb.AppendLine(BuildDonut(summary, rows.Count));

            // ── Bar chart (pure CSS) ──────────────────────────────────────────
            sb.AppendLine(BuildBarChart(summary, rows.Count));

            // ── Filter controls ───────────────────────────────────────────────
            sb.AppendLine("<div class=\"filter-bar\">");
            sb.AppendLine("  <label>Filter: <input type=\"text\" id=\"filterInput\" onkeyup=\"filterTable()\" placeholder=\"Search component...\"></label>");
            sb.AppendLine("  &nbsp;");
            sb.AppendLine("  <label>Status: <select id=\"statusFilter\" onchange=\"filterTable()\">");
            sb.AppendLine("    <option value=\"\">All</option>");
            sb.AppendLine("    <option>Unchanged</option><option>Modified</option>");
            sb.AppendLine("    <option>Only in source</option><option>Only in target</option>");
            sb.AppendLine("  </select></label>");
            sb.AppendLine("</div>");

            // ── Data table ────────────────────────────────────────────────────
            sb.AppendLine("<table id=\"reportTable\">");
            sb.AppendLine("<thead><tr><th>#</th><th>Component</th><th>Type</th><th>Status</th><th>Checksum (source)</th></tr></thead>");
            sb.AppendLine("<tbody>");

            int i = 1;
            foreach (var row in rows)
            {
                sb.AppendLine($"<tr class=\"row-{CssClass(row.StatusCode)}\">");
                sb.AppendLine($"  <td>{i++}</td>");
                sb.AppendLine($"  <td class=\"component-cell\" title=\"{HtmlEncode(row.Component)}\">{HtmlEncode(row.Component)}</td>");
                sb.AppendLine($"  <td>{HtmlEncode(row.Type)}</td>");
                sb.AppendLine($"  <td><span class=\"badge badge-{CssClass(row.StatusCode)}\">{HtmlEncode(row.Status)}</span></td>");
                sb.AppendLine($"  <td class=\"checksum\">{HtmlEncode(row.Checksum)}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // ── JS filter ─────────────────────────────────────────────────────
            sb.AppendLine("<script>");
            sb.AppendLine(HtmlScript());
            sb.AppendLine("</script>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static string Card(string label, int count, string color, string icon)
        {
            return $"<div class=\"card\" style=\"border-top:4px solid {color}\">"
                 + $"<div class=\"card-icon\" style=\"color:{color}\">{icon}</div>"
                 + $"<div class=\"card-count\" style=\"color:{color}\">{count}</div>"
                 + $"<div class=\"card-label\">{label}</div>"
                 + "</div>";
        }

        private static string BuildDonut(Summary s, int total)
        {
            if (total == 0) return string.Empty;

            double r = 70, cx = 90, cy = 90;
            double circumference = 2 * Math.PI * r;

            var segments = new[]
            {
                new { value = s.Modified,     color = "#fd7e14", label = "Modified" },
                new { value = s.OnlyInSource, color = "#28a745", label = "Only in source" },
                new { value = s.OnlyInTarget, color = "#dc3545", label = "Only in target" },
                new { value = s.Unchanged,    color = "#6c757d", label = "Unchanged" },
            };

            var sb = new StringBuilder();
            sb.AppendLine("<div class=\"charts-row\">");
            sb.AppendLine("<div class=\"chart-container\">");
            sb.AppendLine("<h3>Status Distribution</h3>");
            sb.AppendLine($"<svg viewBox=\"0 0 180 180\" width=\"180\" height=\"180\">");

            double offset = 0;
            foreach (var seg in segments)
            {
                if (seg.value == 0) continue;
                double pct = (double)seg.value / total;
                double dash = pct * circumference;
                double gap  = circumference - dash;
                sb.AppendLine($"<circle r=\"{r}\" cx=\"{cx}\" cy=\"{cy}\" fill=\"transparent\""
                    + $" stroke=\"{seg.color}\" stroke-width=\"28\""
                    + $" stroke-dasharray=\"{dash:F2} {gap:F2}\""
                    + $" stroke-dashoffset=\"{-offset:F2}\""
                    + $" transform=\"rotate(-90 {cx} {cy})\">"
                    + $"<title>{seg.label}: {seg.value}</title></circle>");
                offset += dash;
            }

            sb.AppendLine($"<text x=\"{cx}\" y=\"{cy}\" text-anchor=\"middle\" dominant-baseline=\"middle\" font-size=\"14\" font-weight=\"bold\">{total}</text>");
            sb.AppendLine("</svg>");
            sb.AppendLine("<div class=\"legend\">");
            foreach (var seg in segments)
                sb.AppendLine($"<span class=\"legend-item\"><span class=\"legend-dot\" style=\"background:{seg.color}\"></span>{seg.label} ({seg.value})</span>");
            sb.AppendLine("</div></div>");
            return sb.ToString();
        }

        private static string BuildBarChart(Summary s, int total)
        {
            if (total == 0) return "</div></div>";

            double maxVal = Math.Max(Math.Max(s.Modified, s.OnlyInSource), Math.Max(s.OnlyInTarget, s.Unchanged));
            if (maxVal == 0) return "</div></div>";

            var bars = new[]
            {
                new { label = "Unchanged",    value = s.Unchanged,    color = "#6c757d" },
                new { label = "Modified",     value = s.Modified,     color = "#fd7e14" },
                new { label = "Only in Src",  value = s.OnlyInSource, color = "#28a745" },
                new { label = "Only in Tgt",  value = s.OnlyInTarget, color = "#dc3545" },
            };

            var sb = new StringBuilder();
            sb.AppendLine("<div class=\"chart-container\">");
            sb.AppendLine("<h3>Component Counts</h3>");
            sb.AppendLine("<div class=\"bar-chart\">");
            foreach (var bar in bars)
            {
                double pct = (bar.value / maxVal) * 100.0;
                sb.AppendLine("<div class=\"bar-row\">");
                sb.AppendLine($"  <span class=\"bar-label\">{bar.label}</span>");
                sb.AppendLine($"  <div class=\"bar-track\">");
                sb.AppendLine($"    <div class=\"bar-fill\" style=\"width:{pct:F1}%;background:{bar.color}\">{bar.value}</div>");
                sb.AppendLine($"  </div>");
                sb.AppendLine("</div>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>"); // close charts-row
            return sb.ToString();
        }

        private static string HtmlStyle()
        {
            return @"
*{box-sizing:border-box;margin:0;padding:0}
body{font-family:'Segoe UI',sans-serif;font-size:13px;background:#f4f6f9;color:#333}
.header{background:#1565c0;color:#fff;padding:20px 30px}
.header h1{font-size:20px;margin-bottom:4px}
.header p{font-size:12px;opacity:.85}
.cards{display:flex;gap:16px;padding:20px 30px;flex-wrap:wrap}
.card{background:#fff;border-radius:8px;padding:16px 24px;min-width:140px;box-shadow:0 2px 6px rgba(0,0,0,.08);text-align:center}
.card-icon{font-size:22px;margin-bottom:4px}
.card-count{font-size:28px;font-weight:700}
.card-label{font-size:11px;color:#888;margin-top:2px}
.charts-row{display:flex;gap:24px;padding:0 30px 20px;flex-wrap:wrap;align-items:flex-start}
.chart-container{background:#fff;border-radius:8px;padding:16px;box-shadow:0 2px 6px rgba(0,0,0,.08)}
.chart-container h3{font-size:13px;margin-bottom:12px;color:#555}
.legend{display:flex;flex-direction:column;gap:4px;margin-top:8px;font-size:11px}
.legend-item{display:flex;align-items:center;gap:6px}
.legend-dot{width:10px;height:10px;border-radius:50%;flex-shrink:0}
.bar-chart{display:flex;flex-direction:column;gap:8px;min-width:280px}
.bar-row{display:flex;align-items:center;gap:8px}
.bar-label{width:90px;font-size:11px;text-align:right;flex-shrink:0}
.bar-track{flex:1;background:#eee;border-radius:4px;height:22px;overflow:hidden}
.bar-fill{height:100%;border-radius:4px;color:#fff;font-size:11px;font-weight:600;display:flex;align-items:center;justify-content:flex-end;padding-right:6px;min-width:28px;transition:width .4s}
.filter-bar{padding:10px 30px;display:flex;gap:16px;align-items:center;background:#fff;border-bottom:1px solid #ddd}
.filter-bar input,.filter-bar select{border:1px solid #ccc;border-radius:4px;padding:4px 8px;font-size:12px}
table{width:calc(100% - 60px);margin:20px 30px;border-collapse:collapse;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 6px rgba(0,0,0,.08)}
thead tr{background:#1565c0;color:#fff}
th{padding:10px 12px;text-align:left;font-size:12px;font-weight:600}
td{padding:8px 12px;font-size:12px;border-bottom:1px solid #f0f0f0;vertical-align:middle}
tr:hover td{background:#f8f9fa}
.component-cell{max-width:360px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.checksum{font-family:monospace;font-size:11px;color:#888}
.badge{display:inline-block;padding:2px 8px;border-radius:12px;font-size:11px;font-weight:600;color:#fff}
.badge-unchanged{background:#6c757d}
.badge-modified{background:#fd7e14}
.badge-only-in-source{background:#28a745}
.badge-only-in-target{background:#dc3545}
.row-modified td{background:#fff8f0}
.row-only-in-source td{background:#f0fff4}
.row-only-in-target td{background:#fff5f5}
";
        }

        private static string HtmlScript()
        {
            return @"
function filterTable(){
  var txt=(document.getElementById('filterInput').value||'').toLowerCase();
  var st=(document.getElementById('statusFilter').value||'').toLowerCase();
  var rows=document.querySelectorAll('#reportTable tbody tr');
  rows.forEach(function(r){
    var comp=(r.cells[1]?r.cells[1].textContent:'').toLowerCase();
    var status=(r.cells[3]?r.cells[3].textContent:'').toLowerCase();
    r.style.display=((!txt||comp.includes(txt))&&(!st||status.includes(st)))?'':'none';
  });
}
";
        }

        private static string CssClass(ComparisionStatus s)
        {
            switch (s)
            {
                case ComparisionStatus.Unchanged:    return "unchanged";
                case ComparisionStatus.Modified:     return "modified";
                case ComparisionStatus.OnlyInSource: return "only-in-source";
                case ComparisionStatus.OnlyInTarget: return "only-in-target";
                default:                             return "unchanged";
            }
        }

        private static string HtmlEncode(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        // ──────────────────────────────────────────────────────────────────────
        //  XLSX generation — Open XML SpreadsheetML via System.IO.Packaging
        //  No external NuGet package required (WindowsBase.dll, GAC .NET 4.0+)
        // ──────────────────────────────────────────────────────────────────────

        private static void BuildXlsx(List<ReportRow> rows, string outputPath)
        {
            using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
            {
                AddEntry(archive, "_rels/.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                    "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                    "</Relationships>");

                AddEntry(archive, "[Content_Types].xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                    "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                    "<Default Extension=\"xml\"  ContentType=\"application/xml\"/>" +
                    "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                    "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                    "<Override PartName=\"/xl/sharedStrings.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml\"/>" +
                    "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>" +
                    "</Types>");

                AddEntry(archive, "xl/workbook.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" " +
                    "xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                    "<sheets><sheet name=\"Comparison\" sheetId=\"1\" r:id=\"rId1\"/></sheets>" +
                    "</workbook>");

                AddEntry(archive, "xl/_rels/workbook.xml.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                    "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                    "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings\" Target=\"sharedStrings.xml\"/>" +
                    "<Relationship Id=\"rId3\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>" +
                    "</Relationships>");

                AddEntry(archive, "xl/styles.xml", BuildStylesXml());

                var (sheetXml, sharedStrings) = BuildSheetXml(rows);
                AddEntry(archive, "xl/sharedStrings.xml", BuildSharedStringsXml(sharedStrings));
                AddEntry(archive, "xl/worksheets/sheet1.xml", sheetXml);
            }
        }

        private static void AddEntry(ZipArchive archive, string entryName, string xml)
        {
            var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(xml);
        }

        private static (string sheetXml, List<string> sharedStrings) BuildSheetXml(List<ReportRow> rows)
        {
            var ss  = new List<string>();
            var sb  = new StringBuilder();

            // Style indices (defined in BuildStylesXml):
            // 0 = default, 1 = header, 2 = unchanged, 3 = modified, 4 = onlyInSource, 5 = onlyInTarget

            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
            sb.Append("<sheetViews><sheetView workbookViewId=\"0\" showGridLines=\"1\"><selection/></sheetView></sheetViews>");
            sb.Append("<sheetFormatPr defaultRowHeight=\"15\"/>");
            sb.Append("<cols>");
            sb.Append("<col min=\"1\" max=\"1\" width=\"6\"  customWidth=\"1\"/>"); // #
            sb.Append("<col min=\"2\" max=\"2\" width=\"55\" customWidth=\"1\"/>"); // Component
            sb.Append("<col min=\"3\" max=\"3\" width=\"20\" customWidth=\"1\"/>"); // Type
            sb.Append("<col min=\"4\" max=\"4\" width=\"18\" customWidth=\"1\"/>"); // Status
            sb.Append("<col min=\"5\" max=\"5\" width=\"36\" customWidth=\"1\"/>"); // Checksum
            sb.Append("</cols>");
            sb.Append("<sheetData>");

            // Header row
            sb.Append("<row r=\"1\">");
            AppendHeaderCell(sb, "A1", "# ",          ss, 1);
            AppendHeaderCell(sb, "B1", "Component",   ss, 1);
            AppendHeaderCell(sb, "C1", "Type",        ss, 1);
            AppendHeaderCell(sb, "D1", "Status",      ss, 1);
            AppendHeaderCell(sb, "E1", "Checksum (source)", ss, 1);
            sb.Append("</row>");

            for (int i = 0; i < rows.Count; i++)
            {
                var row  = rows[i];
                int r    = i + 2;
                int style = StyleIndex(row.StatusCode);

                sb.Append($"<row r=\"{r}\">");
                AppendNumberCell(sb, $"A{r}", i + 1, style);
                AppendStringCell(sb, $"B{r}", row.Component,  ss, style);
                AppendStringCell(sb, $"C{r}", row.Type,       ss, style);
                AppendStringCell(sb, $"D{r}", row.Status,     ss, style);
                AppendStringCell(sb, $"E{r}", row.Checksum,   ss, style);
                sb.Append("</row>");
            }

            sb.Append("</sheetData>");
            sb.Append("<autoFilter ref=\"A1:E1\"/>");
            sb.Append("</worksheet>");

            return (sb.ToString(), ss);
        }

        private static void AppendHeaderCell(StringBuilder sb, string addr, string value, List<string> ss, int style)
        {
            int idx = GetOrAddString(ss, value);
            sb.Append($"<c r=\"{addr}\" t=\"s\" s=\"{style}\"><v>{idx}</v></c>");
        }

        private static void AppendStringCell(StringBuilder sb, string addr, string value, List<string> ss, int style)
        {
            if (string.IsNullOrEmpty(value)) { sb.Append($"<c r=\"{addr}\" s=\"{style}\"/>"); return; }
            int idx = GetOrAddString(ss, value);
            sb.Append($"<c r=\"{addr}\" t=\"s\" s=\"{style}\"><v>{idx}</v></c>");
        }

        private static void AppendNumberCell(StringBuilder sb, string addr, int value, int style)
        {
            sb.Append($"<c r=\"{addr}\" s=\"{style}\"><v>{value}</v></c>");
        }

        private static int GetOrAddString(List<string> ss, string value)
        {
            int idx = ss.IndexOf(value);
            if (idx < 0) { idx = ss.Count; ss.Add(value); }
            return idx;
        }

        private static string BuildSharedStringsXml(List<string> ss)
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append($"<sst xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" count=\"{ss.Count}\" uniqueCount=\"{ss.Count}\">");
            foreach (var s in ss)
                sb.Append($"<si><t xml:space=\"preserve\">{XmlEncode(s)}</t></si>");
            sb.Append("</sst>");
            return sb.ToString();
        }

        private static string BuildStylesXml()
        {
            // Fonts: 0=default, 1=header (white bold)
            // Fills: 0=none, 1=gray(pattern), 2=header blue, 3=modified orange, 4=source green, 5=target red
            // Borders: 0=none, 1=thin all
            // CellXfs (style indices):
            //   0=default, 1=header, 2=unchanged, 3=modified, 4=onlyInSource, 5=onlyInTarget
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                + "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
                + "<fonts count=\"2\">"
                + "<font><sz val=\"11\"/><name val=\"Calibri\"/></font>"
                + "<font><b/><sz val=\"11\"/><color rgb=\"FFFFFFFF\"/><name val=\"Calibri\"/></font>"
                + "</fonts>"
                + "<fills count=\"6\">"
                + "<fill><patternFill patternType=\"none\"/></fill>"
                + "<fill><patternFill patternType=\"gray125\"/></fill>"
                + "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FF1565C0\"/></patternFill></fill>" // header blue
                + "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFFFF8F0\"/></patternFill></fill>" // modified light orange
                + "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFF0FFF4\"/></patternFill></fill>" // source light green
                + "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFFFF5F5\"/></patternFill></fill>" // target light red
                + "</fills>"
                + "<borders count=\"2\">"
                + "<border><left/><right/><top/><bottom/><diagonal/></border>"
                + "<border><left style=\"thin\"><color auto=\"1\"/></left><right style=\"thin\"><color auto=\"1\"/></right>"
                + "<top style=\"thin\"><color auto=\"1\"/></top><bottom style=\"thin\"><color auto=\"1\"/></bottom><diagonal/></border>"
                + "</borders>"
                + "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>"
                + "<cellXfs count=\"6\">"
                + "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"1\" xfId=\"0\" applyBorder=\"1\"/>"           // 0 default
                + "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\"/>" // 1 header
                + "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"1\" xfId=\"0\" applyBorder=\"1\"/>"           // 2 unchanged
                + "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"3\" borderId=\"1\" xfId=\"0\" applyFill=\"1\" applyBorder=\"1\"/>" // 3 modified
                + "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"4\" borderId=\"1\" xfId=\"0\" applyFill=\"1\" applyBorder=\"1\"/>" // 4 only in source
                + "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"5\" borderId=\"1\" xfId=\"0\" applyFill=\"1\" applyBorder=\"1\"/>" // 5 only in target
                + "</cellXfs>"
                + "</styleSheet>";
        }

        private static int StyleIndex(ComparisionStatus s)
        {
            switch (s)
            {
                case ComparisionStatus.Modified:     return 3;
                case ComparisionStatus.OnlyInSource: return 4;
                case ComparisionStatus.OnlyInTarget: return 5;
                default:                             return 2;
            }
        }

        private static string XmlEncode(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Summary helper
        // ──────────────────────────────────────────────────────────────────────

        private static Summary BuildSummary(List<ReportRow> rows)
        {
            return new Summary
            {
                Unchanged    = rows.Count(r => r.StatusCode == ComparisionStatus.Unchanged),
                Modified     = rows.Count(r => r.StatusCode == ComparisionStatus.Modified),
                OnlyInSource = rows.Count(r => r.StatusCode == ComparisionStatus.OnlyInSource),
                OnlyInTarget = rows.Count(r => r.StatusCode == ComparisionStatus.OnlyInTarget),
            };
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Internal models
        // ──────────────────────────────────────────────────────────────────────

        private class ReportRow
        {
            public string            Component  { get; set; }
            public string            Type       { get; set; }
            public string            Status     { get; set; }
            public ComparisionStatus StatusCode { get; set; }
            public string            Checksum   { get; set; }
        }

        private class Summary
        {
            public int Unchanged    { get; set; }
            public int Modified     { get; set; }
            public int OnlyInSource { get; set; }
            public int OnlyInTarget { get; set; }
        }
    }
}
