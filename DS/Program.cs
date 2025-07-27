using System;
using DS.Core;

public class Program
{
    public static void Main(string[] args)
    {
        Tester tester = new();
        Interpreter interpreter = new();

        interpreter.Runtime.Functions.AddFunction<int, int, int>("Add", tester.Add);
        interpreter.Runtime.Functions.AddFunction<string>("Print", tester.Print);

        interpreter.Run(@"C:\Users\curef\Desktop\DS\DS\test.txt");
    }
}

public class Tester()
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    public void Print(string message)
    {
        Console.WriteLine(message);
    }
}