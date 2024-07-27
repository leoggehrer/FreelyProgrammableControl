using CommonTool;
using System.Diagnostics;

namespace FreelyProgrammableControl.ConApp
{
    internal class FPCApp : ConsoleApplication
    {
        Logic.ExecutionUnit executionUnit = new Logic.ExecutionUnit(16, 16);
        protected override void PrintHeader()
        {
            string solutionNameFromPath = TemplatePath.GetSolutionNameFromPath(Application.GetCurrentSolutionPath());
            PrintHeader(solutionNameFromPath);
        }

        protected override MenuItem[] CreateMenuItems()
        {
            var mnuIdx = 0;
            var menuItems = new List<MenuItem>
            {
                CreateMenuSeparator(),
                new()
                {
                    Key = $"{++mnuIdx}",
                    Text = ToLabelText("Start", "Start the execution unit."),
                    Action = (self) => StartExecutionUnit(),
                    IsDisplayed = !executionUnit.IsRunning,
                },
                new()
                {
                    Key = $"{++mnuIdx}",
                    Text = ToLabelText("Stop", "Stop the execution unit."),
                    Action = (self) => StopExecutionUnit(),
                    IsDisplayed = executionUnit.IsRunning,
                },
                CreateMenuSeparator(),
            };

            for (int i = 0;i < executionUnit.Outputs.Length;i++)
            {
                menuItems.Add(new()
                {
                    Key = $"{++mnuIdx}",
                    Text = ToLabelText($"{executionUnit.Outputs[i]}", $"{executionUnit.Inputs[i]}"),
                    Action = (self) =>
                    {
                        var idx = Convert.ToInt32(self.Params["idx"]);

                        if (executionUnit.Inputs[idx] is Logic.Switch sw)
                        {
                            sw.Toggle();
                        }
                    },
                    IsDisplayed = true,
                    Params = new() { { "idx", i } },
                });
            }

            return [..menuItems];
        }

        private void StartExecutionUnit()
        {
            var lines = File.ReadAllLines("Test.fpc");

            executionUnit.LoadSource(lines);
            executionUnit.Start();
        }
        private void StopExecutionUnit()
        {
            executionUnit.Stop();
        }
    }
}
