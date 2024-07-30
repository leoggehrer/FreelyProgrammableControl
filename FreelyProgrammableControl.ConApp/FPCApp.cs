using CommonTool;
using FreelyProgrammableControl.Logic;

namespace FreelyProgrammableControl.ConApp
{
    internal class FPCApp : ConsoleApplication
    {
        private readonly ExecutionUnit executionUnit = new(16, 16);
        private int outputsHashCode = -1;
        private int countersHashCode = 0;
        public FPCApp()
        {
            executionUnit.Attach((s, e) =>
            {
                Task.Run(() =>
                {
                    if (outputsHashCode != executionUnit.Outputs.GetHashCode()
                        || countersHashCode != executionUnit.Counters.GetHashCode())
                    {
                        outputsHashCode = executionUnit.Outputs.GetHashCode();
                        countersHashCode = executionUnit.Counters.GetHashCode();
                        try
                        {
                            var (Left, Top) = GetCursorPosition();
                            var saveColor = ForegroundColor;

                            ForegroundColor = ConsoleColor.Yellow;
                            SetCursorPosition(0, Top + 2);
                            for (int i = 0; i < executionUnit.Outputs.Length; i++)
                            {
                                Print($"{i,6}");
                            }
                            PrintLine();
                            for (int i = 0; i < executionUnit.Outputs.Length; i++)
                            {
                                Print($"{executionUnit.Outputs[i].Value,6}");
                            }
                            PrintLine();
                            for (int i = 0; i < Math.Min(executionUnit.Outputs.Length, executionUnit.Counters.Length); i++)
                            {
                                Print($"{executionUnit.Counters.GetValue(i),6}");
                            }
                            SetCursorPosition(Left, Top);
                            ForegroundColor = saveColor;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in Attach: {ex.Message}");
                        }
                    }
                });
            });
        }
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
                    Key = $"{mnuIdx}",
                    Text = ToLabelText("Start", "Start the execution unit."),
                    Action = (self) => StartExecutionUnit(),
                    IsDisplayed = executionUnit.IsRunning == false,
                },
                new()
                {
                    Key = $"{mnuIdx}",
                    Text = ToLabelText("Stop", "Stop the execution unit."),
                    Action = (self) => StopExecutionUnit(),
                    IsDisplayed = executionUnit.IsRunning,
                },
                CreateMenuSeparator(),
            };

            for (int i = 0; i < executionUnit.Inputs.Length; i++)
            {
                menuItems.Add(new()
                {
                    Key = $"{++mnuIdx}",
                    Text = ToLabelText($"{executionUnit.Inputs[i].Label}", $"{executionUnit.Inputs[i].Value}"),
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
            return [.. menuItems.Union(CreateExitMenuItems())];
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
            Thread.Sleep(500);
        }
    }
}
