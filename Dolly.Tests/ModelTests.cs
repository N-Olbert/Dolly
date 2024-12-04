namespace Dolly.Tests;

//todo: we do not need to specify sealed!
public class ModelTests
{
    [Test]
    [Arguments(ModelFlags.None, "partial class")]
    [Arguments(ModelFlags.Record, "partial record")]
    [Arguments(ModelFlags.Record | ModelFlags.Struct, "partial record struct")]
    [Arguments(ModelFlags.Record | ModelFlags.Struct | ModelFlags.ClonableBase, "partial record struct")] // Cannot occur
    [Arguments(ModelFlags.Record | ModelFlags.ClonableBase, "partial record")]
    [Arguments(ModelFlags.Struct, "partial struct")]
    [Arguments(ModelFlags.Struct | ModelFlags.ClonableBase, "partial struct")] // Cannot occur
    [Arguments(ModelFlags.ClonableBase, "partial class")]
    public async Task Modifiers(ModelFlags flags, string expected)
    {
        var model = new Model("test", "test", flags, [], []);
        var modifiers = model.GetModifiers();
        await Assert.That(modifiers).IsEqualTo(expected);
    }

    [Test]
    [Arguments(ModelFlags.None, "virtual")]
    [Arguments(ModelFlags.Struct, "")]
    [Arguments(ModelFlags.Struct | ModelFlags.ClonableBase, "override")] // Cannot occur
    [Arguments(ModelFlags.ClonableBase, "override")]
    public async Task MethodModifiers(ModelFlags flags, string expected)
    {
        var model = new Model("test", "test", flags, [], []);
        var modifiers = model.GetMethodModifiers().Trim();
        await Assert.That(modifiers).IsEqualTo(expected);
    }
}
