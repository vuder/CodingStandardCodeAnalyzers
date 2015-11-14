using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace CodingStandardCodeAnalyzers {
    public static class CodeFixProviderHelper {
        public static async Task<Document> ReplaceNodesInDocumentAsync(this CodeFixProvider codeFixProvider, Document document, CancellationToken cancellationToken, params Tuple<SyntaxNode, SyntaxNode>[] nodes) {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            root = root.ReplaceNodes(nodes.Select(node => node.Item1), (nodeToReplace, node) => nodes.SingleOrDefault(n => n.Item1 == nodeToReplace).Item2);
            //TODO: check if needed.
            SyntaxNode formattedRoot = Formatter.Format(root, Formatter.Annotation, document.Project.Solution.Workspace);
            return document.WithSyntaxRoot(formattedRoot);
        }

        public static async Task<Document> ReplaceNodeInDocumentAsync(this CodeFixProvider codeFixProvider, Document document, CancellationToken cancellationToken, SyntaxNode oldNode, SyntaxNode newNode) {
            return await ReplaceNodesInDocumentAsync(codeFixProvider, document, cancellationToken, new Tuple<SyntaxNode, SyntaxNode>(oldNode, newNode));
        }

        public static async Task<Document> CheckNamespaceUsageAsync(this CodeFixProvider codeFixProvider, Document document, CancellationToken cancellationToken, string namespaceName) {
            if (string.IsNullOrWhiteSpace(namespaceName)) {
                return document;
            }
            namespaceName = namespaceName.Trim();
            var root = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            bool namespaceImported = root.DescendantNodes().OfType<UsingDirectiveSyntax>().Any(usingDirective => usingDirective.Name.ToString() == namespaceName);
            if (namespaceImported) {
                return document;
            }
            root = root.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName)));
            return document.WithSyntaxRoot(root);
        }

        public static async Task<RegisterCodeFixesAsyncContinuation<T>> RegisterCodeFixAsync<T>(this CodeFixContext context, string codeFixTitle, Func<Document, T, CancellationToken, Task<Document>> executeFixAsync) where T : SyntaxNode {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            Diagnostic diagnostic = context.Diagnostics.First();
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

            T statement = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<T>().First();
            RegisterCodeFix(context, codeFixTitle, executeFixAsync, statement, diagnostic);
            return new RegisterCodeFixesAsyncContinuation<T>(context, statement, diagnostic);
        }

        private static void RegisterCodeFix<T>(CodeFixContext context, string codeFixTitle, Func<Document, T, CancellationToken, Task<Document>> executeFixAsync, T statement, Diagnostic diagnostic) where T : SyntaxNode {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: codeFixTitle,
                    createChangedDocument: cancellationToken => executeFixAsync(context.Document, statement, cancellationToken),
                    equivalenceKey: codeFixTitle),
                diagnostic);
        }

        public static RegisterCodeFixesAsyncContinuation<T> RegisterAdditionalCodeFixes<T>(this RegisterCodeFixesAsyncContinuation<T> info, string codeFixTitle, Func<Document, T, CancellationToken, Task<Document>> executeFixAsync) where T : SyntaxNode {
            RegisterCodeFix(info.Context, codeFixTitle, executeFixAsync, info.Statement, info.Diagnostic);
            return info;
        }

        public class RegisterCodeFixesAsyncContinuation<T> where T : SyntaxNode {
            public CodeFixContext Context { get; }
            public T Statement { get; }
            public Diagnostic Diagnostic { get; }

            public RegisterCodeFixesAsyncContinuation(CodeFixContext context, T statement, Diagnostic diagnostic) {
                Context = context;
                Statement = statement;
                Diagnostic = diagnostic;
            }
        }
    }
}