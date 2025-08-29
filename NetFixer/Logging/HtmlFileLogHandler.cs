using NetFixer.Interfaces;
using System.Text;

namespace NetFixer.Logging
{
    public class HtmlFileLogHandler : ILog
    {
        private readonly StringBuilder _htmlContent = new();
        private readonly string _filePath;
        private readonly string _title;
        private int _sectionCounter = 0;

        public HtmlFileLogHandler(string title = "Network Diagnostic Log")
        {
            _title = title;
            var logDir = AppContext.BaseDirectory;
            var fileName = $"DiagnosticLog_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.html";

            _filePath = Path.Combine(logDir, fileName);
            InitializeHtml();
        }

        private void InitializeHtml()
        {
            _htmlContent.AppendLine("<!DOCTYPE html>");
            _htmlContent.AppendLine("<html lang=\"ru\">");
            _htmlContent.AppendLine("<head>");
            _htmlContent.AppendLine("    <meta charset=\"UTF-8\">");
            _htmlContent.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            _htmlContent.AppendLine($"    <title>{_title}</title>");
            _htmlContent.AppendLine("    <style>");
            _htmlContent.AppendLine("        body {");
            _htmlContent.AppendLine("            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;");
            _htmlContent.AppendLine("            margin: 0;");
            _htmlContent.AppendLine("            padding: 20px;");
            _htmlContent.AppendLine("            background-color: #f5f5f5;");
            _htmlContent.AppendLine("            color: #333;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .container {");
            _htmlContent.AppendLine("            width: 100%;");
            _htmlContent.AppendLine("            margin: 0 auto;");
            _htmlContent.AppendLine("            background: white;");
            _htmlContent.AppendLine("            border-radius: 8px;");
            _htmlContent.AppendLine("            box-shadow: 0 2px 10px rgba(0,0,0,0.1);");
            _htmlContent.AppendLine("            overflow: hidden;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .header {");
            _htmlContent.AppendLine("            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);");
            _htmlContent.AppendLine("            color: white;");
            _htmlContent.AppendLine("            padding: 20px;");
            _htmlContent.AppendLine("            text-align: center;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .content {");
            _htmlContent.AppendLine("            padding: 20px;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .log-entry {");
            _htmlContent.AppendLine("            margin-bottom: 8px;");
            _htmlContent.AppendLine("            padding: 8px 12px;");
            _htmlContent.AppendLine("            border-radius: 4px;");
            _htmlContent.AppendLine("            border-left: 4px solid #ddd;");
            _htmlContent.AppendLine("            background-color: #fafafa;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .log-entry.info { border-left-color: #2196F3; background-color: #E3F2FD; }");
            _htmlContent.AppendLine("        .log-entry.success { border-left-color: #4CAF50; background-color: #E8F5E8; }");
            _htmlContent.AppendLine("        .log-entry.error { border-left-color: #F44336; background-color: #FFEBEE; }");
            _htmlContent.AppendLine("        .log-entry.warning { border-left-color: #FF9800; background-color: #FFF3E0; }");
            _htmlContent.AppendLine("        .log-entry.debug { border-left-color: #9E9E9E; background-color: #F5F5F5; }");
            _htmlContent.AppendLine("        .log-entry.command { border-left-color: #9C27B0; background-color: #F3E5F5; }");
            _htmlContent.AppendLine("        .log-entry.highlight { border-left-color: #FFC107; background-color: #FFF8E1; }");
            _htmlContent.AppendLine("        .log-entry.stdout { border-left-color: #FF9800; background-color: #FFF3E0; }");
            _htmlContent.AppendLine("        .log-entry.stderr { border-left-color: #F44336; background-color: #FFEBEE; }");
            _htmlContent.AppendLine("        .log-entry.group { border-left-color: #3F51B5; background-color: #E8EAF6; }");
            _htmlContent.AppendLine("        .plugin-group {");
            _htmlContent.AppendLine("            margin: 30px 0;");
            _htmlContent.AppendLine("            padding: 20px;");
            _htmlContent.AppendLine("            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);");
            _htmlContent.AppendLine("            color: white;");
            _htmlContent.AppendLine("            border-radius: 12px;");
            _htmlContent.AppendLine("            box-shadow: 0 4px 20px rgba(0,0,0,0.15);");
            _htmlContent.AppendLine("            position: relative;");
            _htmlContent.AppendLine("            overflow: hidden;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .plugin-group::before {");
            _htmlContent.AppendLine("            content: '';");
            _htmlContent.AppendLine("            position: absolute;");
            _htmlContent.AppendLine("            top: 0;");
            _htmlContent.AppendLine("            left: 0;");
            _htmlContent.AppendLine("            right: 0;");
            _htmlContent.AppendLine("            height: 4px;");
            _htmlContent.AppendLine("            background: linear-gradient(90deg, #FFD700, #FFA500, #FF6347);");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .plugin-group h2 {");
            _htmlContent.AppendLine("            margin: 0 0 15px 0;");
            _htmlContent.AppendLine("            font-size: 24px;");
            _htmlContent.AppendLine("            font-weight: bold;");
            _htmlContent.AppendLine("            text-align: center;");
            _htmlContent.AppendLine("            text-shadow: 0 2px 4px rgba(0,0,0,0.3);");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .plugin-group .plugin-content {");
            _htmlContent.AppendLine("            background: rgba(255,255,255,0.1);");
            _htmlContent.AppendLine("            border-radius: 8px;");
            _htmlContent.AppendLine("            padding: 15px;");
            _htmlContent.AppendLine("            backdrop-filter: blur(10px);");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .plugin-divider {");
            _htmlContent.AppendLine("            height: 3px;");
            _htmlContent.AppendLine("            background: linear-gradient(90deg, transparent, #ddd, transparent);");
            _htmlContent.AppendLine("            margin: 20px 0;");
            _htmlContent.AppendLine("            border-radius: 2px;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .section {");
            _htmlContent.AppendLine("            margin: 20px 0;");
            _htmlContent.AppendLine("            padding: 15px;");
            _htmlContent.AppendLine("            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);");
            _htmlContent.AppendLine("            color: white;");
            _htmlContent.AppendLine("            border-radius: 8px;");
            _htmlContent.AppendLine("            font-size: 18px;");
            _htmlContent.AppendLine("            font-weight: bold;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .subsection {");
            _htmlContent.AppendLine("            margin: 15px 0;");
            _htmlContent.AppendLine("            padding: 10px 15px;");
            _htmlContent.AppendLine("            background-color: #f0f0f0;");
            _htmlContent.AppendLine("            border-left: 4px solid #2196F3;");
            _htmlContent.AppendLine("            border-radius: 4px;");
            _htmlContent.AppendLine("            font-size: 16px;");
            _htmlContent.AppendLine("            font-weight: bold;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .progress-bar {");
            _htmlContent.AppendLine("            width: 100%;");
            _htmlContent.AppendLine("            height: 20px;");
            _htmlContent.AppendLine("            background-color: #e0e0e0;");
            _htmlContent.AppendLine("            border-radius: 10px;");
            _htmlContent.AppendLine("            overflow: hidden;");
            _htmlContent.AppendLine("            margin: 10px 0;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .progress-fill {");
            _htmlContent.AppendLine("            height: 100%;");
            _htmlContent.AppendLine("            background: linear-gradient(90deg, #4CAF50, #8BC34A);");
            _htmlContent.AppendLine("            transition: width 0.3s ease;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .table {");
            _htmlContent.AppendLine("            width: 100%;");
            _htmlContent.AppendLine("            border-collapse: collapse;");
            _htmlContent.AppendLine("            margin: 15px 0;");
            _htmlContent.AppendLine("            background-color: white;");
            _htmlContent.AppendLine("            border-radius: 8px;");
            _htmlContent.AppendLine("            overflow: hidden;");
            _htmlContent.AppendLine("            box-shadow: 0 2px 10px rgba(0,0,0,0.1);");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .table th {");
            _htmlContent.AppendLine("            background-color: #f8f9fa;");
            _htmlContent.AppendLine("            padding: 12px;");
            _htmlContent.AppendLine("            text-align: left;");
            _htmlContent.AppendLine("            font-weight: bold;");
            _htmlContent.AppendLine("            border-bottom: 2px solid #dee2e6;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .table td {");
            _htmlContent.AppendLine("            padding: 12px;");
            _htmlContent.AppendLine("            border-bottom: 1px solid #dee2e6;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .table tr:hover {");
            _htmlContent.AppendLine("            background-color: #f8f9fa;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .code-block {");
            _htmlContent.AppendLine("            background-color: #2d3748;");
            _htmlContent.AppendLine("            color: #e2e8f0;");
            _htmlContent.AppendLine("            padding: 15px;");
            _htmlContent.AppendLine("            border-radius: 8px;");
            _htmlContent.AppendLine("            font-family: 'Courier New', monospace;");
            _htmlContent.AppendLine("            margin: 15px 0;");
            _htmlContent.AppendLine("            overflow-x: auto;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .link {");
            _htmlContent.AppendLine("            color: #2196F3;");
            _htmlContent.AppendLine("            text-decoration: none;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .link:hover {");
            _htmlContent.AppendLine("            text-decoration: underline;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .collapsible {");
            _htmlContent.AppendLine("            margin: 10px 0;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .collapsible-header {");
            _htmlContent.AppendLine("            background-color: #f8f9fa;");
            _htmlContent.AppendLine("            padding: 10px 15px;");
            _htmlContent.AppendLine("            cursor: pointer;");
            _htmlContent.AppendLine("            border: 1px solid #dee2e6;");
            _htmlContent.AppendLine("            border-radius: 4px;");
            _htmlContent.AppendLine("            font-weight: bold;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .collapsible-content {");
            _htmlContent.AppendLine("            padding: 15px;");
            _htmlContent.AppendLine("            border: 1px solid #dee2e6;");
            _htmlContent.AppendLine("            border-top: none;");
            _htmlContent.AppendLine("            border-radius: 0 0 4px 4px;");
            _htmlContent.AppendLine("            display: none;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .collapsible.active .collapsible-content {");
            _htmlContent.AppendLine("            display: block;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .timestamp { color: #666; font-size: 12px; font-family: 'Courier New', monospace; }");
            _htmlContent.AppendLine("        .type { font-weight: bold; margin-right: 8px; }");
            _htmlContent.AppendLine("        .type.info { color: #1976D2; }");
            _htmlContent.AppendLine("        .type.success { color: #388E3C; }");
            _htmlContent.AppendLine("        .type.error { color: #D32F2F; }");
            _htmlContent.AppendLine("        .type.warning { color: #F57C00; }");
            _htmlContent.AppendLine("        .type.debug { color: #616161; }");
            _htmlContent.AppendLine("        .type.command { color: #7B1FA2; }");
            _htmlContent.AppendLine("        .type.highlight { color: #F57F17; }");
            _htmlContent.AppendLine("        .type.stdout { color: #F57C00; }");
            _htmlContent.AppendLine("        .type.stderr { color: #D32F2F; }");
            _htmlContent.AppendLine("        .type.group { color: #3F51B5; }");
            _htmlContent.AppendLine("        .message { margin-left: 8px; }");
            _htmlContent.AppendLine("        .command-block { margin: 10px 0; padding: 12px; background-color: #f8f9fa; border-radius: 4px; border: 1px solid #e9ecef; }");
            _htmlContent.AppendLine("        .command-line { font-family: 'Courier New', monospace; background-color: #4a5568; color: #f7fafc; padding: 8px 12px; border-radius: 4px; margin-bottom: 8px; }");
            _htmlContent.AppendLine("        .output-line { font-family: 'Courier New', monospace; background-color: #f7fafc; color: #2d3748; padding: 4px 8px; border-radius: 2px; margin: 2px 0; }");
            _htmlContent.AppendLine("        .stdout-table { font-family: 'Courier New', monospace; background-color: #f7fafc; color: #2d3748; padding: 8px 12px; border-radius: 4px; margin: 8px 0; border: 1px solid #e2e8f0; }");
            _htmlContent.AppendLine("        .stdout-table .stdout-row { display: flex; margin: 2px 0; }");
            _htmlContent.AppendLine("        .stdout-table .stdout-label { font-weight: bold; min-width: 200px; color: #4a5568; }");
            _htmlContent.AppendLine("        .stdout-table .stdout-value { flex: 1; }");
            _htmlContent.AppendLine("        .error-line { font-family: 'Courier New', monospace; background-color: #fed7d7; color: #c53030; padding: 4px 8px; border-radius: 2px; margin: 2px 0; }");
            _htmlContent.AppendLine("        .exit-code { font-weight: bold; color: #666; }");
            _htmlContent.AppendLine("        .exit-code.success { color: #388E3C; }");
            _htmlContent.AppendLine("        .exit-code.error { color: #D32F2F; }");
            _htmlContent.AppendLine("        .separator { height: 2px; background: linear-gradient(90deg, transparent, #ddd, transparent); margin: 15px 0; }");
            _htmlContent.AppendLine("        .summary { margin-top: 20px; padding: 15px; background-color: #e8f5e8; border-radius: 4px; border-left: 4px solid #4CAF50; }");
            _htmlContent.AppendLine("        .group-banner {");
            _htmlContent.AppendLine("            margin: 20px 0;");
            _htmlContent.AppendLine("            padding: 15px;");
            _htmlContent.AppendLine("            background: linear-gradient(135deg, #4CAF50, #8BC34A);");
            _htmlContent.AppendLine("            color: white;");
            _htmlContent.AppendLine("            border-radius: 12px;");
            _htmlContent.AppendLine("            box-shadow: 0 4px 20px rgba(0,0,0,0.15);");
            _htmlContent.AppendLine("            position: relative;");
            _htmlContent.AppendLine("            overflow: hidden;");
            _htmlContent.AppendLine("            font-size: 20px;");
            _htmlContent.AppendLine("            font-weight: bold;");
            _htmlContent.AppendLine("            text-align: center;");
            _htmlContent.AppendLine("            text-shadow: 0 2px 4px rgba(0,0,0,0.3);");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("        .plugin-banner {");
            _htmlContent.AppendLine("            margin: 10px 0;");
            _htmlContent.AppendLine("            padding: 10px 15px;");
            _htmlContent.AppendLine("            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);");
            _htmlContent.AppendLine("            color: white;");
            _htmlContent.AppendLine("            border-radius: 8px;");
            _htmlContent.AppendLine("            box-shadow: 0 2px 10px rgba(0,0,0,0.1);");
            _htmlContent.AppendLine("            font-size: 18px;");
            _htmlContent.AppendLine("            font-weight: bold;");
            _htmlContent.AppendLine("            text-align: center;");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("    </style>");
            _htmlContent.AppendLine("    <script>");
            _htmlContent.AppendLine("        function toggleCollapsible(element) {");
            _htmlContent.AppendLine("            element.classList.toggle('active');");
            _htmlContent.AppendLine("        }");
            _htmlContent.AppendLine("    </script>");
            _htmlContent.AppendLine("</head>");
            _htmlContent.AppendLine("<body>");
            _htmlContent.AppendLine("    <div class=\"container\">");
            _htmlContent.AppendLine("        <div class=\"header\">");
            _htmlContent.AppendLine($"            <h1>{_title}</h1>");
            _htmlContent.AppendLine($"            <div class=\"timestamp\">Создан: {DateTime.Now:dd.MM.yyyy HH:mm:ss}</div>");
            _htmlContent.AppendLine("        </div>");
            _htmlContent.AppendLine("        <div class=\"content\">");
        }

        public void Info(string message) => WriteLogEntry("INFO", message, "info");
        public void Success(string message) => WriteLogEntry("SUCCESS", message, "success");
        public void Error(string message) => WriteLogEntry("ERROR", message, "error");
        public void Warning(string message) => WriteLogEntry("WARNING", message, "warning");
        public void Debug(string message) => WriteLogEntry("DEBUG", message, "debug");
        public void Highlight(string message) => WriteLogEntry("HIGHLIGHT", message, "highlight");
        public void Group(string message)
        {
            _htmlContent.AppendLine($"<div class=\"group-banner\">{EscapeHtml(message)}</div>");
        }

        public void StartPluginGroup(string pluginName)
        {
            _htmlContent.AppendLine($"<div class=\"plugin-banner\">{EscapeHtml(pluginName)}</div>");
        }

        public void Section(string title)
        {
            _sectionCounter++;
            _htmlContent.AppendLine($"            <div class=\"section\">");
            _htmlContent.AppendLine($"                {_sectionCounter}. {EscapeHtml(title)}");
            _htmlContent.AppendLine("            </div>");
        }

        public void SubSection(string title)
        {
            _htmlContent.AppendLine("            <div class=\"subsection\">");
            _htmlContent.AppendLine($"                {EscapeHtml(title)}");
            _htmlContent.AppendLine("            </div>");
        }

        public void Progress(string message, int percentage)
        {
            _htmlContent.AppendLine("            <div class=\"log-entry info\">");
            _htmlContent.AppendLine($"                <span class=\"timestamp\">{DateTime.Now:HH:mm:ss}</span>");
            _htmlContent.AppendLine("                <span class=\"type info\">[PROGRESS]</span>");
            _htmlContent.AppendLine($"                <span class=\"message\">{EscapeHtml(message)}</span>");
            _htmlContent.AppendLine("                <div class=\"progress-bar\">");
            _htmlContent.AppendLine($"                    <div class=\"progress-fill\" style=\"width: {percentage}%\"></div>");
            _htmlContent.AppendLine("                </div>");
            _htmlContent.AppendLine($"                <div style=\"text-align: center; margin-top: 5px; font-weight: bold;\">{percentage}%</div>");
            _htmlContent.AppendLine("            </div>");
        }

        public void Table(string[] headers, string[][] rows)
        {
            _htmlContent.AppendLine("            <table class=\"table\">");
            _htmlContent.AppendLine("                <thead>");
            _htmlContent.AppendLine("                    <tr>");
            foreach (var header in headers)
            {
                _htmlContent.AppendLine($"                        <th>{EscapeHtml(header)}</th>");
            }
            _htmlContent.AppendLine("                    </tr>");
            _htmlContent.AppendLine("                </thead>");
            _htmlContent.AppendLine("                <tbody>");
            foreach (var row in rows)
            {
                _htmlContent.AppendLine("                    <tr>");
                foreach (var cell in row)
                {
                    _htmlContent.AppendLine($"                        <td>{EscapeHtml(cell)}</td>");
                }
                _htmlContent.AppendLine("                    </tr>");
            }
            _htmlContent.AppendLine("                </tbody>");
            _htmlContent.AppendLine("            </table>");
        }

        public void CodeBlock(string code, string language = "text")
        {
            _htmlContent.AppendLine("            <div class=\"code-block\">");
            _htmlContent.AppendLine($"                <div style=\"margin-bottom: 10px; color: #a0aec0; font-size: 12px;\">// {language.ToUpper()}</div>");
            _htmlContent.AppendLine($"                <pre>{EscapeHtml(code)}</pre>");
            _htmlContent.AppendLine("            </div>");
        }

        public void Link(string text, string url)
        {
            _htmlContent.AppendLine($"            <a href=\"{EscapeHtml(url)}\" class=\"link\" target=\"_blank\">{EscapeHtml(text)}</a>");
        }

        public void Image(string altText, string imagePath)
        {
            _htmlContent.AppendLine($"            <img src=\"{EscapeHtml(imagePath)}\" alt=\"{EscapeHtml(altText)}\" style=\"max-width: 100%; height: auto; border-radius: 4px; margin: 10px 0;\">");
        }

        public void Collapsible(string title, string content)
        {
            _htmlContent.AppendLine("            <div class=\"collapsible\">");
            _htmlContent.AppendLine($"                <div class=\"collapsible-header\" onclick=\"toggleCollapsible(this.parentElement)\">");
            _htmlContent.AppendLine($"                    ▼ {EscapeHtml(title)}");
            _htmlContent.AppendLine("                </div>");
            _htmlContent.AppendLine("                <div class=\"collapsible-content\">");
            _htmlContent.AppendLine($"                    {EscapeHtml(content)}");
            _htmlContent.AppendLine("                </div>");
            _htmlContent.AppendLine("            </div>");
        }

        public void Command(string command, string output, string error, int exitCode)
        {
            // Команда
            _htmlContent.AppendLine("            <div class=\"log-entry command\">");
            _htmlContent.AppendLine($"                <span class=\"timestamp\">{DateTime.Now:HH:mm:ss}</span>");
            _htmlContent.AppendLine("                <span class=\"type command\">[COMMAND]</span>");
            _htmlContent.AppendLine($"                <span class=\"message\">{EscapeHtml(command)}</span>");
            _htmlContent.AppendLine("            </div>");

            // STDOUT
            if (!string.IsNullOrWhiteSpace(output))
            {
                var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    _htmlContent.AppendLine("            <div class=\"log-entry stdout\">");
                    _htmlContent.AppendLine($"                <span class=\"timestamp\">{DateTime.Now:HH:mm:ss}</span>");
                    _htmlContent.AppendLine("                <span class=\"type stdout\">[STDOUT]</span>");
                    _htmlContent.AppendLine("                <div class=\"stdout-table\">");

                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var trimmedLine = line.Trim();
                            if (trimmedLine.Contains(":"))
                            {
                                var parts = trimmedLine.Split(new[] { ':' }, 2);
                                if (parts.Length == 2)
                                {
                                    _htmlContent.AppendLine("                    <div class=\"stdout-row\">");
                                    _htmlContent.AppendLine($"                        <span class=\"stdout-label\">{EscapeHtml(parts[0].Trim())}:</span>");
                                    _htmlContent.AppendLine($"                        <span class=\"stdout-value\">{EscapeHtml(parts[1].Trim())}</span>");
                                    _htmlContent.AppendLine("                    </div>");
                                }
                                else
                                {
                                    _htmlContent.AppendLine($"                    <div class=\"output-line\">{EscapeHtml(trimmedLine)}</div>");
                                }
                            }
                            else
                            {
                                _htmlContent.AppendLine($"                    <div class=\"output-line\">{EscapeHtml(trimmedLine)}</div>");
                            }
                        }
                    }

                    var exitCodeClass = exitCode == 0 ? "success" : "error";
                    _htmlContent.AppendLine($"                    <div class=\"exit-code {exitCodeClass}\">Exit Code: {exitCode}</div>");

                    _htmlContent.AppendLine("                </div>");
                    _htmlContent.AppendLine("            </div>");
                }
            }

            // STDERR
            if (!string.IsNullOrWhiteSpace(error))
            {
                _htmlContent.AppendLine("            <div class=\"log-entry stderr\">");
                _htmlContent.AppendLine($"                <span class=\"timestamp\">{DateTime.Now:HH:mm:ss}</span>");
                _htmlContent.AppendLine("                <span class=\"type stderr\">[STDERR]</span>");
                _htmlContent.AppendLine($"                <span class=\"message\">{EscapeHtml(error)}</span>");
                _htmlContent.AppendLine("            </div>");
            }
        }

        private void WriteLogEntry(string type, string message, string cssClass)
        {
            _htmlContent.AppendLine($"            <div class=\"log-entry {cssClass}\">");
            _htmlContent.AppendLine($"                <span class=\"timestamp\">{DateTime.Now:HH:mm:ss}</span>");
            _htmlContent.AppendLine($"                <span class=\"type {cssClass}\">[{type}]</span>");
            _htmlContent.AppendLine($"                <span class=\"message\">{EscapeHtml(message)}</span>");
            _htmlContent.AppendLine("            </div>");
        }

        private string EscapeHtml(string text)
        {
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }

        public void SaveToFile()
        {
            _htmlContent.AppendLine("        </div>");
            _htmlContent.AppendLine("    </div>");
            _htmlContent.AppendLine("</body>");
            _htmlContent.AppendLine("</html>");

            File.WriteAllText(_filePath, _htmlContent.ToString(), Encoding.UTF8);
        }

        public string FilePath => _filePath;
    }
}
