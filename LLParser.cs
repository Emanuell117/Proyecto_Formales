using LenguajesFormalesProyecto;
using System;
using System.Collections.Generic;

public class LLParser : IParser
{
    private readonly Grammar _grammar;
    private readonly FirstFollowCalculator _firstFollow;
    private readonly Dictionary<char, Dictionary<char, List<char>>> _table = new();

    public LLParser(Grammar grammar, FirstFollowCalculator firstFollow)
    {
        _grammar = grammar;
        _firstFollow = firstFollow;
        BuildTable();
    }

    private void BuildTable()
    {
        foreach (var (nt, productions) in _grammar.Productions)
        {
            _table[nt] = new();
            foreach (var production in productions)
            {
                var first = GetFirst(production);
                foreach (var terminal in first)
                {
                    if (terminal != 'e')
                    {
                        if (_table[nt].ContainsKey(terminal))
                            throw new InvalidOperationException($"Conflicto en {nt} con {terminal}");
                        _table[nt][terminal] = production;
                    }
                }
                if (first.Contains('e'))
                {
                    foreach (var followTerminal in _firstFollow.Follow[nt])
                    {
                        // Verificar si ya existe una entrada para este terminal
                        if (_table[nt].ContainsKey(followTerminal))
                            throw new InvalidOperationException($"Conflicto en {nt} con {followTerminal}");
                        _table[nt][followTerminal] = new List<char> { 'e' };
                    }
                }
            }
        }
    }

    private HashSet<char> GetFirst(List<char> symbols)
    {
        var result = new HashSet<char>();
        if (symbols == null || symbols.Count == 0)
        {
            result.Add('e');
            return result;
        }

        bool canEpsilon = true;
        foreach (var symbol in symbols)
        {
            if (!canEpsilon) break;
            if (_grammar.IsTerminal(symbol))
            {
                result.Add(symbol);
                canEpsilon = false;
            }
            else if (symbol == 'e') // Tratar 'e' como ε
            {
                result.Add('e');
                canEpsilon = true;
            }
            else
            {
                var firstSet = _firstFollow.First[symbol];
                foreach (var s in firstSet)
                {
                    if (s != 'e')
                        result.Add(s);
                }
                canEpsilon = firstSet.Contains('e');
            }
        }
        if (canEpsilon)
            result.Add('e');
        return result;
    }

    public bool IsLL1()
    {
        try
        {
            BuildTable();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Parse(string input)
    {
        input += "$";
        var stack = new Stack<char>();
        stack.Push('$');
        stack.Push(_grammar.StartSymbol);

        int index = 0;
        while (stack.Count > 0)
        {
            char top = stack.Pop();
            char currentInput = index < input.Length ? input[index] : '$';

            if (top == '$')
            {
                return currentInput == '$';
            }

            if (_grammar.IsTerminal(top))
            {
                if (top == currentInput)
                {
                    index++;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (!_table.ContainsKey(top) || !_table[top].ContainsKey(currentInput))
                {
                    return false;
                }
                var production = _table[top][currentInput];
                if (production.Count == 1 && production[0] == 'e')
                {
                    continue;
                }
                else
                {
                    for (int i = production.Count - 1; i >= 0; i--)
                    {
                        stack.Push(production[i]);
                    }
                }
            }
        }
        return false;
    }
}
