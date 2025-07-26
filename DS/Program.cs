using System;

public class Program
{


    public static void Main(string[] args)
    {
        Tester tester = new();
        tester.interpreter.OnDialogue = (dialogue) =>
        {
            Console.WriteLine($"{(dialogue.HasSpeaker ? dialogue.SpeakerName + ": " : "")}{dialogue.TextNode.Evaluate(tester.interpreter)}");
        };
        tester.interpreter.OnMenu = (menu) =>
        {
            Console.WriteLine("=====================");
            Console.WriteLine("Menu:");
            int index = 0;
            foreach (var textNode in menu.OptionTextNodes)
            {
                Console.WriteLine($"{index++}: " + textNode.Evaluate(tester.interpreter));
            }
            var input = Console.ReadLine();
            int choice;
            while (string.IsNullOrEmpty(input) || !int.TryParse(input, out choice) || choice < 0 || choice > menu.OptionTextNodes.Count)
            {
                Console.WriteLine("Invalid choice. Please enter a number between 0 and " + menu.OptionTextNodes.Count);
                input = Console.ReadLine();
            }
            return choice;
        };
        tester.interpreter.AddFunction<int, int, int>("Add", tester.Add);
        tester.interpreter.AddFunction<string>("Print", tester.Print);

        // Load and run the script
        Console.WriteLine("=======================================================");
        tester.interpreter.Load(tester.compiler.Compile(@"C:\Users\curef\Desktop\DS\DS\test2.txt"));
        Console.WriteLine("=======================================================");
        tester.interpreter.Run();
    }
}

public class Tester()
{
    public Interpreter interpreter = new();
    public Compiler compiler = new();

    public int Add(int a, int b)
    {
        return a + b;
    }

    public void Print(string message)
    {
        Console.WriteLine(message);
    }
}