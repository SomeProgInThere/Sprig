using System.Collections;
using System.Collections.Immutable;

namespace Sprig.Code.Syntax;

public abstract class SeparatedSyntaxList {
    public abstract ImmutableArray<SyntaxNode> NodesWithSeperators();
}

public sealed class SeparatedSyntaxList<T>(ImmutableArray<SyntaxNode> nodesWithSeperators) 
    : SeparatedSyntaxList, 
      IEnumerable<T>
    where T : SyntaxNode {

    public int Count => (nodesWithSeperators.Length + 1) / 2;
    public T this[int index] => (T) nodesWithSeperators[index * 2];

    public SyntaxToken? Seperator(int index) {
        if (index == Count - 1)
            return null;
        
        return (SyntaxToken)nodesWithSeperators[index * 2 + 1];
    }

    public override ImmutableArray<SyntaxNode> NodesWithSeperators() => nodesWithSeperators;

    public IEnumerator<T> GetEnumerator() {
        for (var i = 0; i < Count; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}