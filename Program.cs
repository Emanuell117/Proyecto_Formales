// Program.cs
using LenguajesFormalesProyecto;

public class Program
{
    static void Main()
    {
        var grammar = new Grammar();
        int n = int.Parse(Console.ReadLine());

        for (int i = 0; i < n; i++)
        {
            string line = Console.ReadLine();
            var parts = line.Split("->");
            char lhs = parts[0][0];
            var rhsAlternatives = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(alt => new List<char>(alt)).ToList();
            grammar.AddProduction(lhs, rhsAlternatives);
        }

        var firstFollow = new FirstFollowCalculator(grammar);
        firstFollow.ComputeFirst();
        firstFollow.ComputeFollow();

        bool isLL1 = false;
        IParser llParser = null;
        IParser slrParser = null;

        try
        {
            var testLLParser = new LLParser(grammar, firstFollow);
            isLL1 = true;
        }
        catch { }

        var slrParserInstance = new SLRParser(grammar, firstFollow);
        bool isSLR1 = slrParserInstance.IsSLR1();

        if (isLL1 && isSLR1)
        {
            Console.WriteLine("Select a parser(T: for LL(1), B: for SLR(1), Q: quit):");
            llParser = new LLParser(grammar, firstFollow);
            slrParser = slrParserInstance;
            string choice;
            while ((choice = Console.ReadLine()) != "Q")
            {
                if (choice == "T" || choice == "B")
                {
                    var parser = choice == "T" ? llParser : slrParser;
                    while (true)
                    {
                        string input = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(input)) break;
                        Console.WriteLine(parser.Parse(input) ? "yes" : "no");
                    }
                }
            }
        }
        else if (isLL1)
        {
            Console.WriteLine("Grammar is LL(1).");
            llParser = new LLParser(grammar, firstFollow);
            while (true)
            {
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) break;
                Console.WriteLine(llParser.Parse(input) ? "yes" : "no");
            }
        }
        else if (isSLR1)
        {
            Console.WriteLine("Grammar is SLR(1).");
            slrParser = new SLRParser(grammar, firstFollow);
            while (true)
            {
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) break;
                Console.WriteLine(slrParser.Parse(input) ? "yes" : "no");
            }
        }
        else
        {
            Console.WriteLine("Grammar is neither LL(1) nor SLR(1).");
        }
    }
}