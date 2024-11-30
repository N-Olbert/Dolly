namespace Dolly.Tests;
public class MemberTests
{
    [Test]
    [Arguments("Value", MemberFlags.None, false, "Value")]
    [Arguments("Value", MemberFlags.None, true, "Value")]
    [Arguments("Value", MemberFlags.Clonable, false, "Value")]
    [Arguments("Value", MemberFlags.Clonable, true, "Value.DeepClone()")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.Enumerable, false, "Value.ToArray()")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.Enumerable, true, "Value.Select(item => item.DeepClone()).ToArray()")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.Enumerable | MemberFlags.NewCollection, false, "new (Value)")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.Enumerable | MemberFlags.NewCollection, true, "new (Value.Select(item => item.DeepClone()))")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.Enumerable | MemberFlags.NewCollection | MemberFlags.MemberNullable | MemberFlags.ElementNullable, false, "Value == null ? null : new (Value)")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.Enumerable | MemberFlags.NewCollection | MemberFlags.MemberNullable | MemberFlags.ElementNullable, true, "Value == null ? null : new (Value.Select(item => item?.DeepClone()))")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.Enumerable | MemberFlags.NewCollection | MemberFlags.ElementNullable, false, "new (Value)")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.Enumerable | MemberFlags.NewCollection | MemberFlags.ElementNullable, true, "new (Value.Select(item => item?.DeepClone()))")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.Enumerable | MemberFlags.MemberNullable, false, "Value?.ToArray()")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.Enumerable | MemberFlags.MemberNullable, true, "Value?.Select(item => item.DeepClone()).ToArray()")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.Enumerable | MemberFlags.MemberNullable | MemberFlags.ElementNullable, false, "Value?.ToArray()")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.Enumerable | MemberFlags.MemberNullable | MemberFlags.ElementNullable, true, "Value?.Select(item => item?.DeepClone()).ToArray()")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.Enumerable | MemberFlags.ElementNullable, false, "Value.ToArray()")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.Enumerable | MemberFlags.ElementNullable, true, "Value.Select(item => item?.DeepClone()).ToArray()")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.MemberNullable, false, "Value")]
    [Arguments("Value", MemberFlags.Clonable | MemberFlags.MemberNullable, true, "Value?.DeepClone()")]
    [Arguments("Value", MemberFlags.Enumerable, false, "Value.ToArray()")]
    [Arguments("Value", MemberFlags.Enumerable, true, "Value.ToArray()")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.NewCollection, false, "new (Value)")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.NewCollection, true, "new (Value)")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.NewCollection | MemberFlags.MemberNullable | MemberFlags.ElementNullable, false, "Value == null ? null : new (Value)")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.NewCollection | MemberFlags.MemberNullable | MemberFlags.ElementNullable, true, "Value == null ? null : new (Value)")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.NewCollection | MemberFlags.ElementNullable, false, "new (Value)")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.NewCollection | MemberFlags.ElementNullable, true, "new (Value)")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.MemberNullable, false, "Value?.ToArray()")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.MemberNullable, true, "Value?.ToArray()")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.MemberNullable | MemberFlags.ElementNullable, false, "Value?.ToArray()")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.MemberNullable | MemberFlags.ElementNullable, true, "Value?.ToArray()")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.ElementNullable, false, "Value.ToArray()")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.ElementNullable, true, "Value.ToArray()")]
    [Arguments("Value", MemberFlags.MemberNullable, false, "Value")]
    [Arguments("Value", MemberFlags.MemberNullable, true, "Value")]
    public async Task ToString(string name, MemberFlags flags, bool deepClone, string expected)
    {
        var member = new Member(name, false, flags);
        await Assert.That(member.ToString(deepClone)).IsEqualTo(expected);
    }
}
