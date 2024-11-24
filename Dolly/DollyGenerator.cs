using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace Dolly;

/*
 * Todo:
 * X List, Array, IEnumrable
 * Dictionary
 * X Record
 * Private setters
 * Ctor
 * Inheritance
 * IgnoreAttribute
 * Private set only (should not be a problem)
 * Property without set
 * Handle null values
 * Structs
 */

[Generator]
public class DollyGenerator : IIncrementalGenerator
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

        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "Dolly.ClonableAttribute",
            predicate: static (node, cancellationToken) => node is ClassDeclarationSyntax,
            transform: static (context, cancellationToken) =>
            {
                if (context.TargetNode is ClassDeclarationSyntax classDeclaration)
                {
                    var model = context.SemanticModel.GetDeclaredSymbol(context.TargetNode);

                    if (model is INamedTypeSymbol namedTypeSymbol)
                    {
                        var properties = namedTypeSymbol.GetMembers().OfType<IPropertySymbol>()
                            .Where(p => p.SetMethod != null && !p.HasAttribute("CloneIgnoreAttribute"))
                            .Select(p => new Member(p.Name, p.Type))
                            .ToArray();
                        var fields = namedTypeSymbol.GetMembers().OfType<IFieldSymbol>()
                            .Where(f => !f.IsImplicitlyDeclared && !f.HasAttribute("CloneIgnoreAttribute"))
                            .Select(m => new Member(m.Name, m.Type))
                            .ToArray();


                        bool hasEmptyConstructor = namedTypeSymbol.Constructors.Any(c => c.Parameters.Length == 0);
                        

                        return new Model(
                            model.ContainingNamespace.ToCodeString(),
                            model.Name,
                            namedTypeSymbol.IsRecord,
                            properties,
                            fields,
                            hasEmptyConstructor
                        );
                    }
                }
                return null;
            });

        context.RegisterSourceOutput(pipeline, static (context, model) =>
        {
            if (model != null)
            {
                var sourceText = SourceText.From($$"""
                using System.Linq;
                namespace {{model.Namespace}};
                partial {{(model.IsRecord ? "record" : "class" )}} {{model.ClassName}} : IClonable<{{model.ClassName}}>
                {
                    {{(!model.HasEmptyConstructor ? $$"""private {{model.ClassName}}() {}""" : "")}}
                    object ICloneable.Clone() => this.Clone();

                    public {{model.ClassName}} Clone() =>
                        new {{model.ClassName}}()
                        {
                {{GenerateCloneMembers(model, true)}}
                        };

                    public {{model.ClassName}} ShallowClone() =>
                        new {{model.ClassName}}()
                        {
                {{GenerateCloneMembers(model, false)}}
                        };
                }
                """.Replace("\r\n", "\n"), Encoding.UTF8);

                context.AddSource($"{model.ClassName}.g.cs", sourceText);
            }
        });
    }

    private static string GenerateCloneMembers(Model model, bool deepClone) =>
        string.Join(",\n", 
            model.Fields.Concat(model.Properties)
            .Select(m => $"{new string(' ', 12)}{m.Name} = {m.ToString(deepClone)}"));

    private class Model(string @namespace, string @class, bool isRecord, Member[] properties, Member[] fields, bool hasEmptyConstructor)
    {
        public string Namespace { get; } = @namespace;
        public string ClassName { get; } = @class;
        public bool IsRecord { get; } = isRecord;
        public Member[] Properties { get; } = properties;
        public Member[] Fields { get; } = fields;
        public bool HasEmptyConstructor { get; } = hasEmptyConstructor;
    }

    private class Member
    {
        public string Name { get; }
        public bool IsClonable { get; }
        public Func<string, string>? Transform { get; }

        public Member(string name, ITypeSymbol symbol)
        {
            this.Name = name;
            if (TryGetTransform(symbol, out var transform, out var elementType))
            {
                Transform = transform;
                IsClonable = elementType.IsClonable();
            }
            else
            {
                IsClonable = symbol.IsClonable();
            }

        }

        private static bool TryGetTransform(ITypeSymbol symbol, out Func<string, string> transform, out ITypeSymbol elementType)
        {
            // Handle Array and IEnumerable<T>
            if (symbol is IArrayTypeSymbol arrayTypeSymbol)                
            {
                transform = (name) => $"{name}.Select(item => item.Clone()).ToArray()";
                elementType = arrayTypeSymbol.ElementType;
                return true;
            }
            if (symbol is INamedTypeSymbol namedSymbol && namedSymbol.IsGenericIEnumerable())
            {
                transform = (name) => $"{name}.Select(item => item.Clone()).ToArray()";
                elementType = namedSymbol.TypeArguments[0];
                return true;

            }

            // Handle types that implement IEnumerable<T> and take IEnumerable<T> as constructor parameter (ConcurrentQueue<T>, List<T>, ConcurrentStack<T> and LinkedList<T>)
            if (symbol is INamedTypeSymbol namedSymbol2 &&
            namedSymbol2.TryGetIEnumerableType(out var enumerableType) &&
            namedSymbol2.Constructors.Any(c =>
                c.Parameters.Length == 1 &&
                c.Parameters[0].Type.TryGetIEnumerableType(out var constructorEnumerableType) &&
                SymbolEqualityComparer.Default.Equals(enumerableType, constructorEnumerableType)))
            {
                transform = (name) => $"new {symbol.GetFullTypeName()}({name}.Select(item => item.Clone()))";
                elementType = enumerableType;
                return true;
            }
            transform = null!;
            elementType = null!;
            return false;
        }

        public string ToString(bool deepClone)
        {
            if (IsClonable && deepClone)
            {
                if (Transform != null)
                {
                    return Transform(Name);
                }
                return $"{Name}.Clone()";
            }

            return Name;
        }
    }
}
