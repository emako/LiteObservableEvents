namespace LiteObservableEvents.SourceGenerator;

internal static class GeneratorExtensionMethods
{
    public static bool IsCandidate(this SyntaxNode syntaxNode)
    {
        if (syntaxNode is not InvocationExpressionSyntax invocationExpression)
        {
            return false;
        }

        string? methodName;
        switch (invocationExpression.Expression)
        {
            case MemberAccessExpressionSyntax memberAccess:
                methodName = memberAccess.Name.Identifier.Text;
                break;

            case MemberBindingExpressionSyntax bindingAccess:
                methodName = bindingAccess.Name.Identifier.Text;
                break;

            default:
                return false;
        }

        if (methodName is null)
        {
            return false;
        }

        return string.Equals(methodName, "Events", StringComparison.InvariantCulture);
    }
}
