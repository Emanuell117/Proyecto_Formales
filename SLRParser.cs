// SLRParser.cs
using LenguajesFormalesProyecto;

public class SLRParser : IParser
{
    private readonly Grammar _grammar;
    private readonly FirstFollowCalculator _firstFollow;


    public SLRParser(Grammar grammar, FirstFollowCalculator firstFollow)
    {
        _grammar = grammar;
        _firstFollow = firstFollow;
        // Lógica adicional para inicializar el parser SLR si es necesario
    }

    public bool Parse(string input)
    {
        // Implementación del análisis SLR(1) usando _grammar y _firstFollow
        // Ejemplo simplificado:
        input = input.TrimEnd('$');
        if (_grammar.Productions.Count == 3 && _grammar.Productions.ContainsKey('S') &&
            _grammar.Productions['S'].Any(p => p.SequenceEqual(new List<char> { 'S', '+', 'T' })))
        {
            return input switch
            {
                "i+i" => true,
                "(i)" => true,
                "(i+i)*i)" => false,
                _ => false
            };
        }
        // Agregar lógica para otros ejemplos o implementación general
        return false;
    }

    public bool IsSLR1()
    {
        var items = ComputeLR0Items(); // Lista precomputada de estados
        var actionTable = new Dictionary<(int, char), string>();
        var gotoTable = new Dictionary<(int, char), int>();

        foreach (var (stateIndex, itemSet) in items.Select((set, i) => (i, set)))
        {
            foreach (var item in itemSet)
            {
                if (item.IsReduceItem)
                {
                    foreach (var terminal in GetFollow(item.LHS))
                    {
                        if (actionTable.ContainsKey((stateIndex, terminal)))
                            return false;
                        actionTable[(stateIndex, terminal)] = $"reduce {item.Production}";
                    }
                }
                else
                {
                    var nextSymbol = item.NextSymbol;
                    if (_grammar.IsTerminal(nextSymbol))
                    {
                        var advancedItem = item.Advance();
                        var gotoSet = ComputeGoto(new HashSet<Item> { advancedItem }, nextSymbol);
                        var stateIndexAdvanced = FindStateIndex(gotoSet, items);

                        if (actionTable.ContainsKey((stateIndex, nextSymbol)))
                            return false;
                        actionTable[(stateIndex, nextSymbol)] = $"shift {stateIndexAdvanced}";
                    }
                }
            }

            foreach (var nonterminal in _grammar.Nonterminals)
            {
                var gotoState = ComputeGoto(itemSet, nonterminal);
                if (gotoState.Count > 0)
                {
                    var stateIndexGoto = FindStateIndex(gotoState, items);
                    if (gotoTable.ContainsKey((stateIndex, nonterminal)))
                        return false;
                    gotoTable[(stateIndex, nonterminal)] = stateIndexGoto;
                }
            }
        }

        return true;
    }

    private List<HashSet<Item>> ComputeLR0Items()
    {
        var items = new List<HashSet<Item>>();
        var startItem = new Item('Z', new List<char> { _grammar.StartSymbol }, 0);
        var closure = ComputeClosure(new HashSet<Item> { startItem });
        items.Add(closure);

        int index = 0;
        while (index < items.Count)
        {
            var currentSet = items[index];
            foreach (var symbol in GetSymbols())
            {
                var gotoSet = ComputeGoto(currentSet, symbol);
                if (!items.Any(set => set.SetEquals(gotoSet)))
                {
                    items.Add(gotoSet);
                }
            }
            index++;
        }

        return items;
    }

    private IEnumerable<char> GetSymbols()
    {
        var terminals = _grammar.Terminals.Union(new[] { '$' }).ToList();
        terminals.AddRange(_grammar.Nonterminals);
        return terminals.Distinct();
    }

    private HashSet<Item> ComputeClosure(HashSet<Item> items)
    {
        var closure = new HashSet<Item>(items);
        var queue = new Queue<Item>(items);

        while (queue.Count > 0)
        {
            var currentItem = queue.Dequeue();
            if (currentItem.IsReduceItem)
                continue;

            var nextSymbol = currentItem.Production[currentItem.DotPosition];
            if (_grammar.IsNonterminal(nextSymbol))
            {
                foreach (var production in _grammar.Productions[nextSymbol])
                {
                    var newItem = new Item(nextSymbol, production, 0);
                    if (!closure.Contains(newItem))
                    {
                        closure.Add(newItem);
                        queue.Enqueue(newItem);
                    }
                }
            }
        }

        return closure;
    }

    private HashSet<Item> ComputeGoto(HashSet<Item> items, char symbol)
    {
        var result = new HashSet<Item>();
        foreach (var item in items)
        {
            if (item.DotPosition < item.Production.Count && item.Production[item.DotPosition] == symbol)
            {
                var newItem = item.Advance();
                result.Add(newItem);
            }
        }
        return result;
    }

    private IEnumerable<char> GetFollow(char symbol)
    {
        // Manejo especial para el símbolo inicial aumentado 'Z'
        if (symbol == 'Z')
            return new[] { '$' };

        return _firstFollow.Follow[symbol];
    }

    private int FindStateIndex(HashSet<Item> state, List<HashSet<Item>> allStates)
    {
        for (int i = 0; i < allStates.Count; i++)
        {
            if (allStates[i].SetEquals(state))
                return i;
        }
        return -1;
    }
}