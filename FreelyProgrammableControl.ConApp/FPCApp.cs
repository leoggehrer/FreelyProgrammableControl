using CommonTool;
using FreelyProgrammableControl.Logic;

namespace FreelyProgrammableControl.ConApp
{
    /// <summary>
    /// Represents a console application that manages and interacts with an execution unit.
    /// </summary>
    /// <remarks>
    /// This class is responsible for starting and stopping the execution unit,
    /// displaying outputs and counters in the console, and providing a menu for user interaction.
    /// </remarks>
    internal class FPCApp : ConsoleApplication
    {
        private readonly ExecutionUnit executionUnit = new(16, 16);
        private int outputsHashCode = -1;
        private int countersHashCode = 0;
        /// <summary>
        /// Initializes a new instance of the <see cref="FPCApp"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor attaches an event handler to the <see cref="executionUnit"/>
        /// that runs a task to update the console output whenever the outputs or counters
        /// change. It captures the cursor position, changes the console text color,
        /// and prints the outputs and counters to the console.
        /// If an error occurs during this process, it logs the error message to the debug output.
        /// </remarks>
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
        /// <summary>
        /// Prints the header for the current document or output based on the solution name.
        /// This method overrides the base class implementation to provide specific header information.
        /// </summary>
        /// <remarks>
        /// The solution name is extracted from the current solution path using the
        /// <see cref="TemplatePath.GetSolutionNameFromPath(string)"/> method.
        /// The extracted solution name is then passed to another overload of the
        /// <see cref="PrintHeader(string)"/> method for actual printing.
        /// </remarks>
        protected override void PrintHeader()
        {
            string solutionNameFromPath = TemplatePath.GetSolutionNameFromPath(Application.GetCurrentSolutionPath());
            PrintHeader(solutionNameFromPath);
        }

        /// <summary>
        /// Creates an array of menu items for the execution unit.
        /// </summary>
        /// <returns>
        /// An array of <see cref="MenuItem"/> objects representing the menu items for the execution unit,
        /// including options to start and stop the execution unit, as well as options for each input.
        /// </returns>
        /// <remarks>
        /// The method generates menu items based on the current state of the execution unit.
        /// It includes options to start and stop the execution unit, which are displayed based on whether
        /// the execution unit is currently running. Additionally, it creates menu items for each input
        /// of the execution unit, allowing toggling of switches if applicable.
        /// </remarks>
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

        /// <summary>
        /// Starts the execution unit by loading the source code from a file and initiating the execution process.
        /// </summary>
        /// <remarks>
        /// This method reads all lines from the "Test.fpc" file and passes them to the execution unit for processing.
        /// It is expected that the file exists and is accessible. Any exceptions related to file access or reading
        /// should be handled elsewhere in the code.
        /// </remarks>
        private void StartExecutionUnit()
        {
            var lines = File.ReadAllLines("Test.fpc");

            executionUnit.LoadSource(lines);
            executionUnit.Start();
        }
        /// <summary>
        /// Stops the execution unit and pauses the current thread for a short duration.
        /// </summary>
        /// <remarks>
        /// This method calls the <see cref="ExecutionUnit.Stop"/> method to halt the execution unit,
        /// and then it sleeps the current thread for 500 milliseconds to allow for a smooth stop process.
        /// </remarks>
        private void StopExecutionUnit()
        {
            executionUnit.Stop();
            Thread.Sleep(500);
        }
    }
}
