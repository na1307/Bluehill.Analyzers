using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Bluehill.Analyzers;

[Generator]
public sealed class DecoratorGenerator : IIncrementalGenerator {
    private const string decoratorAttributeSource = """
                                                    namespace System;

                                                    [AttributeUsage(AttributeTargets.Method)]
                                                    #pragma warning disable 9113
                                                    public sealed class DecoratorAttribute(string decoratorFQN) : Attribute;
                                                    """;

    private const string interceptsLocationAttributeSource = """
                                                             namespace System.Runtime.CompilerServices {
                                                                 [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
                                                                 #pragma warning disable 9113
                                                                 file sealed class InterceptsLocationAttribute(int version, string data) : Attribute;
                                                             }
                                                             """;

    private static readonly SymbolDisplayFormat fqnFormat = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, memberOptions: SymbolDisplayMemberOptions.IncludeContainingType);

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var compilationAndMethods = context.CompilationProvider.Combine(context.SyntaxProvider.ForAttributeWithMetadataName("System.DecoratorAttribute", (sn, t) => sn is MethodDeclarationSyntax, (c, t) => (MethodDeclarationSyntax)c.TargetNode).Collect());

        context.RegisterSourceOutput(compilationAndMethods, execute);
        context.RegisterPostInitializationOutput(context => context.AddSource("DecoratorAttribute.g.cs", SourceText.From(decoratorAttributeSource, Encoding.UTF8)));
    }

    private static void execute(SourceProductionContext context, (Compilation compilation, ImmutableArray<MethodDeclarationSyntax> methods) valueTuple) {
        var compilation = valueTuple.compilation;

        foreach (var method in valueTuple.methods) {
            var model = compilation.GetSemanticModel(method.SyntaxTree);
            var originalMethod = model.GetDeclaredSymbol(method) ?? throw new Exception("Original method couldn't be found!");

            if (originalMethod.TypeParameters.Any()) {
                // Currently not supported
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("DECORATOR002", "Placeholder", "Placeholder", "Usage", DiagnosticSeverity.Error, true), originalMethod.Locations[0]));
                continue;
            }

            if (!originalMethod.IsStatic) {
                // Also currently not supported
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("DECORATOR002", "Placeholder", "Placeholder", "Usage", DiagnosticSeverity.Error, true), originalMethod.Locations[0]));
                continue;
            }

            StringBuilder sb = new(5120);

            sb.AppendLine("namespace Decorators { internal sealed partial class DecoratorMethods {");

            var attribute = method.AttributeLists.SelectMany(al => al.Attributes.Where(a => isDecorator(model, a, context.CancellationToken))).Single();
            var ms = getMethod(compilation, model, attribute, context.CancellationToken);

            if (ms is null) {
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("DECORATOR001", "Placeholder", "Placeholder", "Usage", DiagnosticSeverity.Error, true), attribute.GetLocation()));

                continue;
            }

            if (originalMethod.IsStatic) {
                foreach (var call in findMethodCalls(originalMethod, model, model.SyntaxTree.GetRoot(context.CancellationToken))) {
#pragma warning disable RSEXPERIMENTAL002 // 형식은 평가 목적으로 제공되며, 이후 업데이트에서 변경되거나 제거될 수 있습니다. 계속하려면 이 진단을 표시하지 않습니다.
                    sb.AppendLine(model.GetInterceptableLocation(call, context.CancellationToken)!.GetInterceptsLocationAttributeSyntax());
#pragma warning restore RSEXPERIMENTAL002 // 형식은 평가 목적으로 제공되며, 이후 업데이트에서 변경되거나 제거될 수 있습니다. 계속하려면 이 진단을 표시하지 않습니다.
                }

                sb.AppendLine(constructInterceptMethodSignature(originalMethod))
                    .AppendLine("{")
                    .AppendLine($"{ms.ToDisplayString(fqnFormat)}({originalMethod.ToDisplayString(fqnFormat)})();")
                    .AppendLine("}");
            }

            sb.AppendLine("}}")
                .AppendLine(interceptsLocationAttributeSource);

            context.AddSource($"{originalMethod.ToDisplayString(fqnFormat)}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        static bool isDecorator(SemanticModel model, AttributeSyntax syntax, CancellationToken token) => model.GetSymbolInfo(syntax.Name, token).Symbol?.ToDisplayString(fqnFormat) == "System.DecoratorAttribute.DecoratorAttribute";

        static IMethodSymbol? getMethod(Compilation compilation, SemanticModel model, AttributeSyntax syntax, CancellationToken token) {
            if (syntax.ArgumentList?.Arguments.Any() != true) {
                throw new Exception("This attribute does not seem to be a DecoratorAttribute.");
            }

            var firstArgument = syntax.ArgumentList.Arguments[0];
            var cValue = model.GetConstantValue(firstArgument.Expression, token);

            if (!cValue.HasValue || cValue.Value is not string fqn) {
                return null;
            }

            var components = fqn.Split('.');

            if (components.Length < 2) {
                return null;
            }

            return compilation.GetTypeByMetadataName(string.Join(".", components.Take(components.Length - 1)))?.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(m => m.Name == components.Last());
        }

        static IEnumerable<InvocationExpressionSyntax> findMethodCalls(IMethodSymbol targetMethod, SemanticModel model, SyntaxNode root) {
            // Filter the invocations by matching their resolved method symbol to the target symbol
            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>()) {
                if (model.GetSymbolInfo(invocation).Symbol is IMethodSymbol symbol && SymbolEqualityComparer.Default.Equals(symbol.OriginalDefinition, targetMethod.OriginalDefinition)) {
                    yield return invocation;
                }
            }
        }

        static string constructInterceptMethodSignature(IMethodSymbol methodSymbol) {
            // Get method name
            var methodName = methodSymbol.Name + "Decorator";

            // Handle generic type parameters
            var typeParameters = methodSymbol.TypeParameters;
            var genericSuffix = typeParameters.Length > 0 ? $"<{string.Join(", ", typeParameters.Select(tp => tp.Name))}>" : string.Empty;

            // Get parameter list
            var parameters = methodSymbol.Parameters;
            var parameterList = string.Join(", ", parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));

            // Combine the pieces into a signature
            return $"public {(methodSymbol.IsStatic ? "static " : string.Empty)}{methodSymbol.ReturnType.ToDisplayString()} {methodName}{genericSuffix}({parameterList})";
        }
    }
}
