using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Bluehill.Analyzers;

[Generator(LanguageNames.CSharp)]
public sealed class DecoratorGenerator : IIncrementalGenerator {
    private const string EditorBrowsableNever = "[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]";

    private const string DecoratorMethodsSource = $"""
                                            namespace Decorators;

                                            {EditorBrowsableNever}
                                            public static partial class DecoratorMethods;
                                            """;

    private const string InterceptsLocationAttributeSource = """
                                                             namespace System.Runtime.CompilerServices {
                                                                 [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
                                                                 #pragma warning disable 9113
                                                                 file sealed class InterceptsLocationAttribute(int version, string data) : Attribute;
                                                             }
                                                             """;

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var methods = context.SyntaxProvider.ForAttributeWithMetadataName("Bluehill.DecoratorAttribute", (n, _) => n is MethodDeclarationSyntax,
            (c, _) => (IMethodSymbol)c.TargetSymbol).Collect();

        context.RegisterSourceOutput(context.CompilationProvider.Combine(methods), Generate);
    }

    private static void Generate(SourceProductionContext context, (Compilation, ImmutableArray<IMethodSymbol>) valueTuple) {
        var token = context.CancellationToken;
        var (compilation, symbols) = valueTuple;

        if (symbols.Any()) {
            context.AddSource("DecoratorMethods.g.cs", SourceText.From(DecoratorMethodsSource, Encoding.UTF8));
        }

        foreach (var symbol in symbols) {
            if (symbol.Parameters.Any()) {
                // Currently not supported
                continue;
            }

            if (symbol.TypeParameters.Any()) {
                // Also currently not supported
                continue;
            }

            if (!symbol.IsStatic) {
                // Also Also currently not supported
                continue;
            }

            if (GetMethodAccessibility(symbol) is not Accessibility.Public) {
                // Only public methods are supported
                continue;
            }

            var decoratorName = symbol.GetAttribute(MetadataName.Parse("Bluehill.DecoratorAttribute"))?.ConstructorArguments[0].Value?.ToString();

            if (decoratorName is null) {
                // DecoratorAttribute is not applied or decorator name is not provided or invalid
                continue;
            }

            var splitDecoratorName = decoratorName.Split('.');
            var decoratorMethod = compilation.GetTypeByMetadataName(string.Join(".", splitDecoratorName.Take(splitDecoratorName.Length - 1)))?.GetMembers()
                .OfType<IMethodSymbol>().FirstOrDefault(m => m.Name == splitDecoratorName[splitDecoratorName.Length - 1]);

            if (decoratorMethod is null) {
                // Decorator method not found
                continue;
            }

            StringBuilder sb = new(1024);

            sb.AppendLine("namespace Decorators {").AppendLine("    partial class DecoratorMethods {");

            if (symbol.IsStatic) {
                foreach (var model in compilation.SyntaxTrees.Select(t => compilation.GetSemanticModel(t))) {
                    foreach (var invocation in model.SyntaxTree.GetRoot(token).DescendantNodes().OfType<InvocationExpressionSyntax>()) {
                        if (model.GetSymbol(invocation) is IMethodSymbol method && SEC.Default.Equals(method, symbol)) {
#pragma warning disable RSEXPERIMENTAL002 // 형식은 평가 목적으로 제공되며, 이후 업데이트에서 변경되거나 제거될 수 있습니다. 계속하려면 이 진단을 표시하지 않습니다.
                            sb.Append("        ").AppendLine(model.GetInterceptableLocation(invocation, context.CancellationToken)!.GetInterceptsLocationAttributeSyntax());
#pragma warning restore RSEXPERIMENTAL002 // 형식은 평가 목적으로 제공되며, 이후 업데이트에서 변경되거나 제거될 수 있습니다. 계속하려면 이 진단을 표시하지 않습니다.
                        }
                    }
                }

                sb.AppendLine($"        {EditorBrowsableNever}")
                                .Append("        ").Append(ConstructInterceptMethodSignature(symbol)).AppendLine(" {")
                                .Append("            ").AppendLine($"{decoratorMethod.ToDisplayString(FqnFormat)}({symbol.ToDisplayString(FqnFormat)})();")
                                .AppendLine("        }");
            }

            sb.AppendLine("    }").AppendLine("}").AppendLine().AppendLine(InterceptsLocationAttributeSource);

            context.AddSource($"DecoratorMethods.{symbol.ToDisplayString(FqnFormat)}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }

    private static string ConstructInterceptMethodSignature(IMethodSymbol methodSymbol) {
        // Get method name
        var methodName = GetEscapedName(methodSymbol.ToDisplayString(FqnFormat)) + "Decorator";

        // Combine the pieces into a signature
        return $"public static {methodSymbol.ReturnType.ToDisplayString()} {methodName}()";
    }
}
