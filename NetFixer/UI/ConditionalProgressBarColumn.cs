using Spectre.Console;
using Spectre.Console.Rendering;

namespace NetFixer.UI
{
    public class ConditionalProgressBarColumn : ProgressColumn
    {
        public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
        {
            if (task.Description.Contains("HIDDEN_TASK"))
                return new Text("");

            return new ProgressBarColumn().Render(options, task, deltaTime);
        }
    }

    public class ConditionalPercentageColumn : ProgressColumn
    {
        public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
        {
            if (task.Description.Contains("HIDDEN_TASK"))
                return new Text("");

            return new PercentageColumn().Render(options, task, deltaTime);
        }
    }


}
