using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenguajesFormalesProyecto
{
    // Item.cs
    public class Item
    {
        public char LHS { get; }
        public List<char> Production { get; }
        public int DotPosition { get; }

        public char NextSymbol => DotPosition < Production.Count ? Production[DotPosition] : 'ε';

        public Item(char lhs, List<char> production, int dotPosition)
        {
            LHS = lhs;
            Production = production;
            DotPosition = dotPosition;
        }

        public bool IsReduceItem => DotPosition == Production.Count;

        public Item Advance()
        {
            if (DotPosition >= Production.Count)
                throw new InvalidOperationException("No se puede avanzar más allá de la producción.");
            return new Item(LHS, Production, DotPosition + 1);
        }

        public override bool Equals(object obj)
        {
            if (obj is Item other)
                return LHS == other.LHS && Production.SequenceEqual(other.Production) && DotPosition == other.DotPosition;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LHS, string.Join("", Production), DotPosition);
        }
    }
}
