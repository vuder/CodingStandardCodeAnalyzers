using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

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
    }
}