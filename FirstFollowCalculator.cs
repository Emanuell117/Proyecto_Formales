using System;
using System.Collections.Generic;

public class FirstFollowCalculator
{
    private readonly Grammar _grammar;
    public Dictionary<char, HashSet<char>> First { get; } = new();
    public Dictionary<char, HashSet<char>> Follow { get; } = new();

    public FirstFollowCalculator(Grammar grammar)
    {
        _grammar = grammar;
        foreach (var nt in _grammar.Nonterminals)
        {
            First[nt] = new();
            Follow[nt] = new();
        }
    }

    public void ComputeFirst()
    {
        bool changed;
        do
        {
            changed = false;
            foreach (var (nt, productions) in _grammar.Productions)
            {
                foreach (var production in productions)
                {
                    changed |= AddFirstFromSequence(production, nt);
                }
            }
        } while (changed);
    }

    private bool AddFirstFromSequence(List<char> symbols, char nt)
    {
        bool added = false;
        bool prevCanEpsilon = true;
        foreach (var symbol in symbols)
        {
            if (!prevCanEpsilon) break;
            if (_grammar.IsTerminal(symbol))
            {
                added |= AddToSet(First[nt], symbol);
                prevCanEpsilon = false;
            }
            else if (symbol == 'e') // Tratar 'e' como ε
            {
                added |= AddToSet(First[nt], 'e');
                prevCanEpsilon = true; // Permite continuar con símbolos siguientes
            }
            else
            {
                var firstSet = First[symbol];
                foreach (var s in firstSet)
                {
                    if (s != 'e')
                        added |= AddToSet(First[nt], s);
                }
                prevCanEpsilon = firstSet.Contains('e');
            }
        }
        if (prevCanEpsilon)
            added |= AddToSet(First[nt], 'e');
        return added;
    }


    private bool AddToSet(HashSet<char> set, char value)
    {
        return set.Add(value);
    }

    public void ComputeFollow()
    {
        Follow[_grammar.StartSymbol].Add('$');
        bool changed;

        do
        {
            changed = false;

            foreach (var (nt, productions) in _grammar.Productions)
            {
                foreach (var production in productions)
                {
                    for (int i = 0; i < production.Count; i++)
                    {
                        char symbol = production[i];

                        if (_grammar.IsNonterminal(symbol))
                        {
                            List<char> beta = production.Skip(i + 1).ToList();
                            HashSet<char> firstBeta = GetFirst(beta);
                            bool canEpsilon = beta.Count == 0 || firstBeta.Contains('e');

                            // Agregar First(beta) - {e} al Follow(symbol]
                            foreach (var terminal in firstBeta)
                            {
                                if (terminal != 'e')
                                {
                                    changed |= AddToSet(Follow[symbol], terminal);
                                }
                            }

                            // Agregar Follow(nt) a Follow(symbol] solo si beta puede derivar ε
                            if (canEpsilon)
                            {
                                foreach (var followSymbol in Follow[nt])
                                {
                                    changed |= AddToSet(Follow[symbol], followSymbol);
                                }
                            }
                        }
                    }
                }
            }
        } while (changed);
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
            else if (symbol == 'e') // Caso especial para ε
            {
                result.Add('e');
                canEpsilon = true; // Continúa procesando símbolos siguientes
            }
            else
            {
                var firstSet = First[symbol];
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
}
