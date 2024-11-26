using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dolly.Tests;
public class MemberTests
{
    [Test]
    [Arguments("Value", MemberFlags.None, false, "Value")]
    [Arguments("Value", MemberFlags.None, true, "Value")]
    [Arguments("Value", MemberFlags.Clonable, false, "Value")]
    [Arguments("Value", MemberFlags.Clonable, true, "Value.Clone()")]
    [Arguments("Value", MemberFlags.Enumerable, false, "Value")]
    [Arguments("Value", MemberFlags.Enumerable, true, "Value")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.ArrayCompatible, false, "Value.ToArray()")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.ArrayCompatible, true, "Value.ToArray()")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.ArrayCompatible | MemberFlags.Clonable, false, "Value.ToArray()")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.ArrayCompatible | MemberFlags.Clonable, true, "Value.Select(item => item.Clone()).ToArray()")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.Clonable, false, "Value")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.Clonable, true, "Value.Select(item => item.Clone())")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.NewCompatible, false, "new (Value)")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.NewCompatible, true, "new (Value)")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.NewCompatible | MemberFlags.Clonable, false, "new (Value)")]
    [Arguments("Value", MemberFlags.Enumerable | MemberFlags.NewCompatible | MemberFlags.Clonable, true, "new (Value.Select(item => item.Clone()))")]
    [Arguments("Value", MemberFlags.NewCompatible, false, "new (Value)")]
    [Arguments("Value", MemberFlags.NewCompatible, true, "new (Value)")]
    public async Task ToString(string name, MemberFlags flags, bool deepClone, string expected)
    {
        var member = new Member(name, false, flags);
        await Assert.That(member.ToString(deepClone)).IsEqualTo(expected);
    }
}
