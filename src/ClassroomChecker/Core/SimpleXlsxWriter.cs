using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Text;

namespace ClassroomChecker.Core
{
    public class XlsxRow
    {
        public bool IsHeader { get; set; }
        public List<string> Cells { get; set; }

        public XlsxRow(bool isHeader = false)
        {
            IsHeader = isHeader;
            Cells = new List<string>();
        }

        public XlsxRow(params string[] values)
        {
            IsHeader = false;
            Cells = new List<string>(values);
        }
    }

    public class XlsxSheet
    {
        public string Name { get; set; }
        public List<XlsxRow> Rows { get; set; }

        public XlsxSheet(string name)
        {
            Name = name;
            Rows = new List<XlsxRow>();
        }

        public void AddRow(params string[] values)
        {
            Rows.Add(new XlsxRow(values));
        }

        public void AddHeader(params string[] headers)
        {
            var row = new XlsxRow(true);
            row.Cells.AddRange(headers);
            Rows.Add(row);
        }
    }

    /// <summary>
    /// Lightweight, zero-dependency OpenXML (.xlsx) generator using built-in System.IO.Compression
    /// </summary>
    public class SimpleXlsxWriter
    {
        private readonly List<XlsxSheet> _sheets = new List<XlsxSheet>();

        public void AddSheet(XlsxSheet sheet)
        {
            if (sheet != null)
            {
                _sheets.Add(sheet);
            }
        }

        public void Save(string filePath)
        {
            if (_sheets.Count == 0)
            {
                throw new InvalidOperationException("No sheets to export.");
            }

            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                // 1. [Content_Types].xml
                WriteContentTypes(zip);

                // 2. _rels/.rels
                WriteRootRels(zip);

                // 3. xl/_rels/workbook.xml.rels
                WriteWorkbookRels(zip);

                // 4. xl/workbook.xml
                WriteWorkbookXml(zip);

                // 5. xl/styles.xml
                WriteStylesXml(zip);

                // 6. xl/worksheets/sheet{N}.xml
                for (int i = 0; i < _sheets.Count; i++)
                {
                    WriteWorksheetXml(zip, i + 1, _sheets[i]);
                }
            }
        }

        private void WriteContentTypes(ZipArchive zip)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.AppendLine("<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">");
            sb.AppendLine("  <Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>");
            sb.AppendLine("  <Default Extension=\"xml\" ContentType=\"application/xml\"/>");
            sb.AppendLine("  <Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>");
            sb.AppendLine("  <Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>");

            for (int i = 0; i < _sheets.Count; i++)
            {
                sb.AppendLine(string.Format("  <Override PartName=\"/xl/worksheets/sheet{0}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>", i + 1));
            }

            sb.AppendLine("</Types>");
            WriteZipEntry(zip, "[Content_Types].xml", sb.ToString());
        }

        private void WriteRootRels(ZipArchive zip)
        {
            string content = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>
</Relationships>";
            WriteZipEntry(zip, "_rels/.rels", content);
        }

        private void WriteWorkbookRels(ZipArchive zip)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.AppendLine("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");

            for (int i = 0; i < _sheets.Count; i++)
            {
                sb.AppendLine(string.Format("  <Relationship Id=\"rId{0}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{0}.xml\"/>", i + 1));
            }

            sb.AppendLine(string.Format("  <Relationship Id=\"rId{0}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>", _sheets.Count + 1));
            sb.AppendLine("</Relationships>");

            WriteZipEntry(zip, "xl/_rels/workbook.xml.rels", sb.ToString());
        }

        private void WriteWorkbookXml(ZipArchive zip)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.AppendLine("<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">");
            sb.AppendLine("  <sheets>");

            for (int i = 0; i < _sheets.Count; i++)
            {
                string safeName = SecurityElement.Escape(_sheets[i].Name);
                sb.AppendLine(string.Format("    <sheet name=\"{0}\" sheetId=\"{1}\" r:id=\"rId{1}\"/>", safeName, i + 1));
            }

            sb.AppendLine("  </sheets>");
            sb.AppendLine("</workbook>");

            WriteZipEntry(zip, "xl/workbook.xml", sb.ToString());
        }

        private void WriteStylesXml(ZipArchive zip)
        {
            string content = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<styleSheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
  <fonts count=""2"">
    <font><sz val=""10.5""/><name val=""Phetsarath OT""/></font>
    <font><b/><sz val=""10.5""/><color rgb=""FFFFFFFF""/><name val=""Phetsarath OT""/></font>
  </fonts>
  <fills count=""3"">
    <fill><patternFill patternType=""none""/></fill>
    <fill><patternFill patternType=""gray125""/></fill>
    <fill><patternFill patternType=""solid""><fgColor rgb=""FF1E293B""/></patternFill></fill>
  </fills>
  <borders count=""2"">
    <border><left/><right/><top/><bottom/><diagonal/></border>
    <border>
      <left style=""thin""><color rgb=""FFE2E8F0""/></left>
      <right style=""thin""><color rgb=""FFE2E8F0""/></right>
      <top style=""thin""><color rgb=""FFE2E8F0""/></top>
      <bottom style=""thin""><color rgb=""FFE2E8F0""/></bottom>
    </border>
  </borders>
  <cellStyleXfs count=""1"">
    <xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0""/>
  </cellStyleXfs>
  <cellXfs count=""2"">
    <xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""1"" xfId=""0""/>
    <xf numFmtId=""0"" fontId=""1"" fillId=""2"" borderId=""1"" xfId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
  </cellXfs>
</styleSheet>";
            WriteZipEntry(zip, "xl/styles.xml", content);
        }

        private void WriteWorksheetXml(ZipArchive zip, int sheetIndex, XlsxSheet sheet)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.AppendLine("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");

            // Column widths
            sb.AppendLine("  <cols>");
            sb.AppendLine("    <col min=\"1\" max=\"1\" width=\"12\" customWidth=\"1\"/>");
            sb.AppendLine("    <col min=\"2\" max=\"2\" width=\"28\" customWidth=\"1\"/>");
            sb.AppendLine("    <col min=\"3\" max=\"50\" width=\"25\" customWidth=\"1\"/>");
            sb.AppendLine("  </cols>");

            sb.AppendLine("  <sheetData>");

            for (int r = 0; r < sheet.Rows.Count; r++)
            {
                var row = sheet.Rows[r];
                int rowNum = r + 1;
                sb.AppendLine(string.Format("    <row r=\"{0}\">", rowNum));

                for (int c = 0; c < row.Cells.Count; c++)
                {
                    string colLetter = GetColumnLetter(c + 1);
                    string cellRef = colLetter + rowNum;
                    int styleIdx = row.IsHeader ? 1 : 0;
                    string val = SecurityElement.Escape(row.Cells[c]);

                    sb.AppendLine(string.Format("      <c r=\"{0}\" s=\"{1}\" t=\"inlineStr\"><is><t>{2}</t></is></c>", cellRef, styleIdx, val));
                }

                sb.AppendLine("    </row>");
            }

            sb.AppendLine("  </sheetData>");
            sb.AppendLine("</worksheet>");

            WriteZipEntry(zip, string.Format("xl/worksheets/sheet{0}.xml", sheetIndex), sb.ToString());
        }

        private string GetColumnLetter(int colIndex)
        {
            string div = string.Empty;
            while (colIndex > 0)
            {
                int mod = (colIndex - 1) % 26;
                div = (char)('A' + mod) + div;
                colIndex = (colIndex - mod) / 26;
            }
            return div;
        }

        private void WriteZipEntry(ZipArchive zip, string path, string content)
        {
            var entry = zip.CreateEntry(path, CompressionLevel.Optimal);
            using (var stream = entry.Open())
            using (var sw = new StreamWriter(stream, Encoding.UTF8))
            {
                sw.Write(content);
            }
        }
    }
}
