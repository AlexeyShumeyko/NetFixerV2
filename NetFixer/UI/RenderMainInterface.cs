using Spectre.Console;

namespace NetFixer.UI
{
    public static class RenderMainInterface
    {
        private static readonly Color _mainColor = Color.SteelBlue1;
        private static readonly Color _disabledColor = Color.Grey;
        private static readonly Style _ruleStyle = Style.Parse("grey dim");

        private static readonly FigletText _title = new FigletText("NetFix")
            .Color(_mainColor)
            .Centered();

        private static readonly Text _version = new Text("v1.0.0", new Style(_disabledColor))
            .Centered();

        public static void MainInterface()
        {
            var header = new Rows(
                _title,
                _version,
                new Rule().RuleStyle(_ruleStyle)
            );

            AnsiConsole.Write(header);
        }
    }
}
