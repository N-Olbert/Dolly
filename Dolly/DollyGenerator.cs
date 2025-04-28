using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Dolly;


/*
 * Todo:
 * X Embed Microsoft.Bcl.HashCode correctly
 * X List, Array, IEnumrable
 * Dictionary
 * X Record
 * X Private setters
 * X Ctor
 * X Inheritance
 * X IgnoreAttribute
 * X Handle null values
 * X Structs
 * CloneConstructorAttribute
 * IClone
 * Move interfaces and attributes to dependency to simplify cross assemlby usage
 * KeyValuePair
 */

[Generator]
public partial class DollyGenerator : IIncrementalGenerator
{
    public const string ClonableAttribute = """
        using System;

        namespace Dolly
        {
            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
            public class ClonableAttribute : Attribute
            {
            }
        }
        """;

    public const string CloneIgnoreAttribute = """
        using System;

        namespace Dolly
        {
            [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
            public class CloneIgnoreAttribute : Attribute
            {
            }
        }
        """;

    public const string ClonableInterface = """
        using System;
        namespace Dolly
        {
            public interface IClonable<T> : ICloneable
            {
                T DeepClone();
                T ShallowClone();
            }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("ClonableAttribute.g.cs", ClonableAttribute);
            ctx.AddSource("CloneIgnoreAttribute.g.cs", CloneIgnoreAttribute);
            ctx.AddSource("IClonable.g.cs", ClonableInterface);
        });

        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName<Result<Model>>(
            fullyQualifiedMetadataName: "Dolly.ClonableAttribute",
            predicate: static (node, cancellationToken) => node is ClassDeclarationSyntax || node is StructDeclarationSyntax || node is RecordDeclarationSyntax,
            transform: static (context, cancellationToken) =>
            {
                var symbol = context.SemanticModel.GetDeclaredSymbol(context.TargetNode);
                if (symbol is INamedTypeSymbol namedTypeSymbol)
                {
                    var nullabilityEnabled = context.SemanticModel.GetNullableContext(context.TargetNode.SpanStart).HasFlag(NullableContext.Enabled);
                    if (Model.TryCreate(namedTypeSymbol, nullabilityEnabled, out var model, out var error))
                    {
                        return model;
                    }
                    else
                    {
                        return error;
                    }
                }
                return DiagnosticInfo.Create(Diagnostics.FailedToGetModelError, context.TargetNode);
            });

        context.RegisterSourceOutput(pipeline, static (context, result) =>
        {
            result.Handle(model =>
            {
                var sourceText = SourceTextConverter.ToSourceText(model);
                context.AddSource($"{model.Name}.g.cs", sourceText);
            },
            error =>
            {
                context.ReportDiagnostic(error.ToDiagnostic());
            });
        });
    }
}
