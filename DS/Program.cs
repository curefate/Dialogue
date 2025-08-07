using DS.Console;

public class Program
{
    public static void Main(string[] args)
    {
        Tester tester = new();
        Interpreter interpreter = new();

        interpreter.Runtime.Functions.AddFunction<int, int, int>(tester.Add);
        interpreter.Runtime.Functions.AddFunction<string>(tester.Print);

        interpreter.Run(@"C:\Users\curef\Desktop\DS\DS\test6.txt", "a");
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