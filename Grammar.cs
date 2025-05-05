using System;
using System.Collections.Generic;

public class Grammar
{
    public HashSet<char> Nonterminals { get; set; } = new();
    public HashSet<char> Terminals { get; set; } = new();
    public Dictionary<char, List<List<char>>> Productions { get; set; } = new();
    public char StartSymbol { get; set; } = 'S';

    public void AddProduction(char lhs, List<List<char>> rhs)
    {
        Productions[lhs] = rhs;
        Nonterminals.Add(lhs); // Ensure LHS is added

        foreach (var production in rhs)
        {
            foreach (var symbol in production)
            {
                if (IsNonterminal(symbol))
                    Nonterminals.Add(symbol); // Add RHS nonterminals
            }
        }
    }

    public bool IsNonterminal(char c) => Nonterminals.Contains(c);
    public bool IsTerminal(char c) => !char.IsUpper(c) && c != 'e';
}
