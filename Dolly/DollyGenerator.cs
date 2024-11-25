using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Dolly;


/*
 * Todo:
 * X List, Array, IEnumrable
 * Dictionary
 * X Record
 * X Private setters
 * X Ctor
 * X Inheritance
 * X IgnoreAttribute
 * Property without set
 * Handle null values
 * X Structs
 * CloneConstructorAttribute
 */

[Generator]
public partial class DollyGenerator : IIncrementalGenerator
{
    public const string ClonableAttribute = """
        using System;

        namespace Dolly
        {
            [AttributeUsage(AttributeTargets.Class)]
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
                /// <summary>
                /// Deep clone
                /// </summary>
                new T Clone();
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
            predicate: static (node, cancellationToken) => node is ClassDeclarationSyntax,
            transform: static (context, cancellationToken) =>
            {
                if (context.TargetNode is ClassDeclarationSyntax classDeclaration)
                {
                    var symbol = context.SemanticModel.GetDeclaredSymbol(context.TargetNode);

                    if (symbol is INamedTypeSymbol namedTypeSymbol)
                    {
                        if (Model.TryCreate(namedTypeSymbol, out var model, out var error))
                        {
                            return model;
                        }
                        else
                        {
                            return error;
                        }
                    }
                }                
                return DiagnosticInfo.Create(Diagnostics.FailedToGetModelError, context.TargetNode);
            });

        context.RegisterSourceOutput(pipeline, static (context, result) =>
        {
            result.Handle(model =>
            {
                var sourceText = SourceText.From($$"""
                using System.Linq;
                namespace {{model.Namespace}};
                partial {{model.GetModifiers()}} {{model.Name}} : IClonable<{{model.Name}}>
                {
                    {{(!model.HasClonableBaseClass ? "object ICloneable.Clone() => this.Clone();" : "")}}
                    public {{model.GetMethodModifiers()}}{{model.Name}} Clone() =>
                        new ()
                        {
                {{GenerateCloneMembers(model, true)}}
                        };

                    public {{model.GetMethodModifiers()}}{{model.Name}} ShallowClone() =>
                        new ()
                        {
                {{GenerateCloneMembers(model, false)}}
                        };
                }
                """.Replace("\r\n", "\n"), Encoding.UTF8);

                context.AddSource($"{model.Name}.g.cs", sourceText);
            },
            error =>
            {
                context.ReportDiagnostic(error.ToDiagnostic());
            });
        });
    }

    private static string GenerateCloneMembers(Model model, bool deepClone) =>
        string.Join(",\n", 
            model.Members
            .Select(m => $"{new string(' ', 12)}{m.Name} = {m.ToString(deepClone)}")
        );
}
