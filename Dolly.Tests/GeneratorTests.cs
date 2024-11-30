using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dolly.Tests;
public class GeneratorTests
{
    private Model GetModel(string code, Func<string, bool>? filter = null)
    {
        var compilation = CreateCompilation(code, true);
        var diagnostics = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error);
        if (diagnostics.Any())
        {
            throw new Exception("Failed to compile code, errors:" + string.Join(", ", diagnostics));
        }

        var syntaxTree = compilation.SyntaxTrees.Single(syntaxTree => syntaxTree.FilePath == "");

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        var allNodes = syntaxTree
            .GetRoot()
            .RecursiveFlatten(n => n.ChildNodes())
            .ToArray();

        var node = syntaxTree
            .GetRoot()
            .RecursiveFlatten(n => n.ChildNodes())
            .Single(node =>
            (node is ClassDeclarationSyntax classNode && (filter == null || filter(classNode.Identifier.Text))) ||
            (node is StructDeclarationSyntax structNode && (filter == null || filter(structNode.Identifier.Text))));
        var symbol = semanticModel.GetDeclaredSymbol(node);
        if (symbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (Model.TryCreate(namedTypeSymbol, true, out var model, out var error))
            {
                return model;
            }
            throw new Exception("Failed to create model, error: " + error.Descriptor.Description);
        }

        throw new Exception("Symbol is not of type INamedTypeSymbol");
    }

    [Test]
    public async Task SimpleClass()
    {
        var model = GetModel(@"
namespace Dolly;
[Clonable]
public partial class SimpleClass
{
    public string First { get; set; }
    public int Second { get; set; }
    [CloneIgnore]
    public float DontClone { get; set; }
}
");
        var expected = new Model("Dolly", "SimpleClass", ModelFlags.None, new Member[] {
            new Member("First", false, MemberFlags.None),
            new Member("Second", false, MemberFlags.None)
        }, EquatableArray<Member>.Empty());

        await Assert.That(model).IsEquivalentTo(expected);
    }

    [Test]
    public async Task SimpleStruct()
    {
        var model = GetModel(@"
namespace Dolly;
[Clonable]
public partial struct SimpleStruct
{
    public string First { get; set; }
    public int Second { get; set; }
    [CloneIgnore]
    public float DontClone { get; set; }
}
");
        var expected = new Model("Dolly", "SimpleStruct", ModelFlags.Struct, new Member[] {
            new Member("First", false, MemberFlags.None),
            new Member("Second", false, MemberFlags.None)
        }, EquatableArray<Member>.Empty());

        await Assert.That(model).IsEquivalentTo(expected);
    }

    [Test]
    public async Task Collections()
    {
        var model = GetModel(@"
using System.Collections.Generic;
namespace Dolly;
[Clonable]
public partial class SimpleClass
{
    public string First { get; set; }
    public int Second { get; set; }
    [CloneIgnore]
    public float DontClone { get; set; }
}

[Clonable]
public partial struct SimpleStruct
{
    public string First { get; set; }
    public int Second { get; set; }
    [CloneIgnore]
    public float DontClone { get; set; }
}

[Clonable]
public partial class ComplexClass
{
    public int[] IntArray { get; set; }
    public List<int> IntList { get; set; }
    public IEnumerable<int> IntIEnumerable { get; set; }

    public string[] StringArray { get; set; }
    public List<string> StringList { get; set; }
    public IEnumerable<string> StringIEnumerable { get; set; }

    public SimpleClass[] ReferenceArray { get; set; }
    public List<SimpleClass> ReferenceList { get; set; }
    public IEnumerable<SimpleClass> ReferenceIEnumerable { get; set; }

    public SimpleStruct[] ValueArray { get; set; }
    public List<SimpleStruct> ValueList { get; set; }
    public IEnumerable<SimpleStruct> ValueIEnumerable { get; set; }
}
", name => name == "ComplexClass");
        var expected = new Model("Dolly", "ComplexClass", ModelFlags.None, new Member[] {

            new Member("IntArray", false, MemberFlags.Enumerable),
            new Member("IntList", false, MemberFlags.Enumerable | MemberFlags.NewCollection),
            new Member("IntIEnumerable", false, MemberFlags.Enumerable),

            new Member("StringArray", false, MemberFlags.Enumerable),
            new Member("StringList", false, MemberFlags.Enumerable | MemberFlags.NewCollection),
            new Member("StringIEnumerable", false, MemberFlags.Enumerable),

            new Member("ReferenceArray", false, MemberFlags.Clonable | MemberFlags.Enumerable),
            new Member("ReferenceList", false, MemberFlags.Clonable | MemberFlags.Enumerable | MemberFlags.NewCollection),
            new Member("ReferenceIEnumerable", false, MemberFlags.Clonable | MemberFlags.Enumerable),

            new Member("ValueArray", false, MemberFlags.Clonable | MemberFlags.Enumerable),
            new Member("ValueList", false, MemberFlags.Clonable | MemberFlags.Enumerable | MemberFlags.NewCollection),
            new Member("ValueIEnumerable", false, MemberFlags.Clonable | MemberFlags.Enumerable)
        }, EquatableArray<Member>.Empty());

        await Assert.That(model).IsEquivalentTo(expected);
    }

    //[Test]
    //public async Task TestGenerator()
    //{
    //    Compilation inputCompilation = CreateCompilation();
    //    var syntaxTree = inputCompilation.SyntaxTrees.Single();

    //    var semanticModel = inputCompilation.GetSemanticModel(syntaxTree);

    //    var node = syntaxTree.GetRoot().RecursiveFlatten(n => n.ChildNodes()).OfType<ClassDeclarationSyntax>().Single();
    //    var symbol = semanticModel.GetDeclaredSymbol(node);
    //    if (symbol is INamedTypeSymbol namedTypeSymbol)
    //    {
    //        if (Model.TryCreate(symbol, true, out var model, out var error))
    //        {

    //        }
    //    }

    //    //todo: how do we set nullability when using generator
    //    var generator = new DollyGenerator();

    //    GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
    //    driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

    //    // We can now assert things about the resulting compilation:
    //    await Assert.That(diagnostics).IsEmpty();
    //    await Assert.That(outputCompilation.SyntaxTrees).HasCount().EqualTo(2);
    //    await Assert.That(outputCompilation.GetDiagnostics()).IsEmpty();

    //    // Or we can look at the results directly:
    //    GeneratorDriverRunResult runResult = driver.GetRunResult();

    //    // The runResult contains the combined results of all generators passed to the driver
    //    await Assert.That(runResult.GeneratedTrees).HasCount().EqualToZero();
    //    await Assert.That(runResult.Diagnostics).IsEmpty();

    //    // Or you can access the individual results on a by-generator basis
    //    GeneratorRunResult generatorResult = runResult.Results[0];
    //    await Assert.That(generatorResult.Generator == generator).IsTrue();
    //    await Assert.That(generatorResult.Diagnostics).IsEmpty();
    //    await Assert.That(generatorResult.GeneratedSources).HasCount().EqualTo(1);
    //    await Assert.That(generatorResult.Exception).IsNull();
    //}


    private static Compilation CreateCompilation(string source, bool addAttributes)
           => CSharpCompilation.Create("compilation",
               addAttributes ? [
                   CSharpSyntaxTree.ParseText(source),
                   CSharpSyntaxTree.ParseText(DollyGenerator.ClonableAttribute, path: "ClonableAttribute.g.cs"),
                   CSharpSyntaxTree.ParseText(DollyGenerator.CloneIgnoreAttribute, path: "CloneIgnoreAttribute.g.cs")
                   ] :
               [CSharpSyntaxTree.ParseText(source)],
               new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
               new CSharpCompilationOptions(
                   OutputKind.NetModule,
                   nullableContextOptions: NullableContextOptions.Enable));
}
