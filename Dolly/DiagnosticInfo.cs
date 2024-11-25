using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;

namespace Dolly;

internal sealed record DiagnosticInfo(DiagnosticDescriptor Descriptor, SyntaxTree? SyntaxTree, TextSpan TextSpan,
    EquatableArray<string> Arguments)
{
    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, ISymbol symbol, params string[] Arguments) =>
        new(descriptor, symbol.Locations.First().SourceTree, symbol.Locations.First().SourceSpan, Arguments);

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, SyntaxNode node, params string[] Arguments) =>
        new(descriptor, node.GetLocation().SourceTree, node.GetLocation().SourceSpan, Arguments);

    public Diagnostic ToDiagnostic() => 
        SyntaxTree == null ?
            Diagnostic.Create(Descriptor, Location.Create(SyntaxTree, TextSpan), Arguments.ToArray()) :
            Diagnostic.Create(Descriptor, Location.Create(SyntaxTree, TextSpan), Arguments.ToArray());
}
