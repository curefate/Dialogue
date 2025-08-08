namespace DS.Console
{
    using System;
    using DS.Core;

    public class Interpreter : Executer
    {
        public Runtime Runtime { get; private set; } = new();
        protected readonly Compiler compiler = new();

        public override void ExecuteDialogue(Stmt_Dialogue instruction, Runtime runtime)
        {
            try
            {
                Console.WriteLine($"{(instruction.HasSpeaker ? instruction.SpeakerName + ": " : "")}{instruction.TextNode.Evaluate(runtime)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing dialogue: {ex.Message}[Ln {instruction.LineNum}, Fp {instruction.FilePath}]");
            }
        }

        public override void ExecuteMenu(Stmt_Menu instruction, Runtime runtime)
        {
            try
            {
                Console.WriteLine("=====================");
                Console.WriteLine("Menu:");
                int index = 0;
                foreach (var textNode in instruction.OptionTextNodes)
                {
                    Console.WriteLine($"{index++}: " + textNode.Evaluate(runtime));
                }
                Console.Write("Select an option (0-" + (instruction.OptionTextNodes.Count - 1) + "): ");
                var input = Console.ReadLine();
                int choice;
                while (string.IsNullOrEmpty(input) || !int.TryParse(input, out choice) || choice < 0 || choice >= instruction.OptionTextNodes.Count)
                {
                    Console.Write("Invalid choice. Please enter a number between 0 and " + (instruction.OptionTextNodes.Count - 1) + ": ");
                    input = Console.ReadLine();
                }
                Console.WriteLine("=====================");
                var selectedActions = instruction.Blocks[choice];
                runtime.Enqueue(selectedActions, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing menu: {ex.Message}[Ln {instruction.LineNum}, Fp {instruction.FilePath}]");
            }
        }

        public virtual void Run(string filePath, string startLabel = "start")
        {
            var script = compiler.Compile(filePath);
            if (script == null)
            {
                Console.WriteLine("Failed to compile script.");
                return;
            }

            Runtime.ClearLabels();
            Runtime.ClearQueue();
            Runtime.Read(script);

            Runtime.Load(startLabel);

            while (Runtime.HasNext)
            {
                var instruction = Runtime.Pop();
                Executer executer = this;
                try
                {
                    executer.Execute(instruction, Runtime);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break; // Stop execution on error
                }
            }
        }
    }
}