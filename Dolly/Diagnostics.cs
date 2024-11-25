using Microsoft.CodeAnalysis;

namespace Dolly;

public static class Diagnostics
{
    public static readonly DiagnosticDescriptor NoValidConstructorError = 
        new (
            id: "DOLLYN001",
            title: "No valid constructor found",
            messageFormat: "Could not find a constructor that must contain the parameters '{0}' and no parameters that aren't included in '{1}'",
            category: "Dolly",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AbstractClassError =
        new(
            id: "DOLLYN002",
            title: "Abstract class not supported",
            messageFormat: "Abstract class {0} can't be cloned",
            category: "Dolly",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FailedToGetModelError =
        new(
            id: "DOLLYN003",
            title: "Failed to get Model",
            messageFormat: "Failed to get Model",
            category: "Dolly",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
}
