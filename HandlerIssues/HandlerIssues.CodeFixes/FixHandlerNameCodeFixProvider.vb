Imports System.Collections.Immutable
Imports System.Composition
Imports System.Threading

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeActions
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Rename
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

<ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(FixHandlerNameCodeFixProvider)), [Shared]>
Public Class FixHandlerNameCodeFixProvider
    Inherits CodeFixProvider

    Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
        Get
            Return ImmutableArray.Create(HandlerNameAnalyzer.DiagnosticId)
        End Get
    End Property

    Private Async Function FixHandlerNameAsync(document As Document, subStatement As MethodStatementSyntax, cancellationToken As CancellationToken) As Task(Of Solution)
        Dim identifierToken As SyntaxToken = subStatement.Identifier
        Dim handlesClause As HandlesClauseSyntax = subStatement.HandlesClause

        Dim handlesClauseItem As HandlesClauseItemSyntax = handlesClause.Events(0)
        Dim classBlock As ClassBlockSyntax = subStatement.FirstAncestorOfType(Of ClassBlockSyntax)

        Dim keywordEventContainer As KeywordEventContainerSyntax = TryCast(handlesClauseItem.EventContainer, KeywordEventContainerSyntax)
        Dim member As IdentifierNameSyntax = handlesClauseItem.EventMember
        Dim memberName As String = member.Identifier.Text
        Dim newName As String
        If keywordEventContainer IsNot Nothing Then
            Dim classStatement As ClassStatementSyntax = classBlock.ClassStatement
            Dim className As String = classStatement.Identifier.Text
            newName = $"{className}_{memberName}"
        Else
            Dim withEventsEventContainer As WithEventsEventContainerSyntax = TryCast(handlesClauseItem.EventContainer, WithEventsEventContainerSyntax)
            Dim memberToken As SyntaxToken = withEventsEventContainer.Identifier
            newName = $"{memberToken.Text}_{memberName}"
        End If

        ' Get the symbol representing the type to be renamed.
        Dim semanticModel As SemanticModel = Await document.GetSemanticModelAsync(cancellationToken)
        Dim typeSymbol As ISymbol = semanticModel.GetDeclaredSymbol(subStatement, cancellationToken)

        ' Produce a new solution that has all references to that type renamed, including the declaration.
        Dim originalSolution As Solution = document.Project.Solution
        Dim optionSet As Options.OptionSet = originalSolution.Workspace.Options
        Dim newSolution As Solution = Await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(False)

        ' Return the new solution with the now-uppercase type newName.
        Return newSolution
    End Function

    Public NotOverridable Overrides Function GetFixAllProvider() As FixAllProvider
        Return WellKnownFixAllProviders.BatchFixer
    End Function

    Public NotOverridable Overrides Async Function RegisterCodeFixesAsync(context As CodeFixContext) As Task
        Dim root As SyntaxNode = Await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(False)

        Dim diagnostic As Diagnostic = context.Diagnostics.First()
        Dim diagnosticSpan As TextSpan = diagnostic.Location.SourceSpan

        ' Find the type statement identified by the diagnostic.
        Dim declaration As MethodStatementSyntax = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType(Of MethodStatementSyntax)().First()

        ' Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
            title:=My.Resources.CodeFixResources.FixNamingViolation,
            createChangedSolution:=Function(c) Me.FixHandlerNameAsync(context.Document, declaration, c),
            equivalenceKey:=My.Resources.CodeFixResources.FixNamingViolation),
            diagnostic)
    End Function

End Class
