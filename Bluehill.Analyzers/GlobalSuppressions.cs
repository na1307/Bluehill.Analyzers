// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded",
    Justification = "<보류 중>", Scope = "member", Target = "~F:Bluehill.Analyzers.BHAnalyzer.BaseUrl")]
[assembly: SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out",
    Justification = "False Positive", Scope = "member", Target =
        "~M:Bluehill.Analyzers.BH0003ProhibitPcpReassignmentAnalyzer.RegisterActions(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext)")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access",
    Justification = "<보류 중>", Scope = "member", Target = "~F:Bluehill.Analyzers.BH0004ToBH0006Analyzer.DiagnosticIdBH0005")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access",
    Justification = "<보류 중>", Scope = "member", Target = "~F:Bluehill.Analyzers.BH0004ToBH0006Analyzer.DiagnosticIdBH0006")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access",
    Justification = "<보류 중>", Scope = "member", Target = "~F:Bluehill.Analyzers.BH0010ToBH0011Analyzer.DiagnosticIdBH0011")]
