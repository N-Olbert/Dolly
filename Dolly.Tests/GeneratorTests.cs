using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Dolly.Tests;
public class GeneratorTests
{
    [Test]
    public async Task TestGenerator()
    {
        Compilation inputCompilation = CreateCompilation(@"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
");

        var generator = new DollyGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        // We can now assert things about the resulting compilation:
        await Assert.That(diagnostics).IsEmpty();
        await Assert.That(outputCompilation.SyntaxTrees).HasCount().EqualTo(2);
        await Assert.That(outputCompilation.GetDiagnostics()).IsEmpty();

        // Or we can look at the results directly:
        GeneratorDriverRunResult runResult = driver.GetRunResult();

        // The runResult contains the combined results of all generators passed to the driver
        await Assert.That(runResult.GeneratedTrees).HasCount().EqualToZero();
        await Assert.That(runResult.Diagnostics).IsEmpty();

        // Or you can access the individual results on a by-generator basis
        GeneratorRunResult generatorResult = runResult.Results[0];
        await Assert.That(generatorResult.Generator == generator).IsTrue();
        await Assert.That(generatorResult.Diagnostics).IsEmpty();
        await Assert.That(generatorResult.GeneratedSources).HasCount().EqualTo(1);
        await Assert.That(generatorResult.Exception).IsNull();
    }


    private static Compilation CreateCompilation(string source)
           => CSharpCompilation.Create("compilation",
               new[] { CSharpSyntaxTree.ParseText(source) },
               new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
               new CSharpCompilationOptions(OutputKind.ConsoleApplication));
}
