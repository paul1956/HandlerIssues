Imports System.Collections.Immutable
Imports System.Composition
Imports System.Threading

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeActions
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

<ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(AddHandlerCodeFixProvider)), [Shared]>
Public Class AddHandlerCodeFixProvider
    Inherits CodeFixProvider

    Public NotOverridable Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String)
        Get
            Return ImmutableArray.Create(MissingHandlerAnalyzer.DiagnosticId)
        End Get
    End Property

    Private Async Function AddMissingHandlersClauseAsync(document As Document, subStatement As MethodStatementSyntax, cancellationToken As CancellationToken) As Task(Of Solution)
        Dim oldRootTask As Task(Of SyntaxNode) = document.GetSyntaxRootAsync(cancellationToken)
        Dim subNamText() As String = subStatement.Identifier.Text.Split("_"c)
        Dim classBlock As ClassBlockSyntax = subStatement.FirstAncestorOfType(Of ClassBlockSyntax)
        Dim handlesClauseItem As HandlesClauseItemSyntax
        Dim eventMember As IdentifierNameSyntax = SyntaxFactory.IdentifierName(subNamText(1))
        If subNamText(0).Equals(classBlock.ClassStatement.Identifier.Text, StringComparison.InvariantCultureIgnoreCase) Then
            ' Need MyBase
            Dim myBaseToken As SyntaxToken = SyntaxFactory.Token(SyntaxKind.MyBaseKeyword)
            Dim keywordEventContainer As KeywordEventContainerSyntax = SyntaxFactory.KeywordEventContainer(myBaseToken)
            handlesClauseItem = SyntaxFactory.HandlesClauseItem(keywordEventContainer, eventMember)
        Else
            ' Simple KeywordEventContainerSyntax
            Dim eventContainer As WithEventsEventContainerSyntax = SyntaxFactory.WithEventsEventContainer(subNamText(0))
            handlesClauseItem = SyntaxFactory.HandlesClauseItem(eventContainer, eventMember)
        End If
        Dim spaceTrivia As SyntaxTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " ")
        Dim events As SeparatedSyntaxList(Of HandlesClauseItemSyntax) = SyntaxFactory.SingletonSeparatedList(Of HandlesClauseItemSyntax)(handlesClauseItem)
        Dim trailing As SyntaxTriviaList = SyntaxFactory.TriviaList(spaceTrivia)
        Dim handlesKeyword As SyntaxToken = SyntaxFactory.Token(SyntaxKind.HandlesKeyword, trailing, "Handles")
        Dim handlesClause As HandlesClauseSyntax = SyntaxFactory.HandlesClause(handlesKeyword, events).WithLeadingTrivia(spaceTrivia).WithTrailingTrivia(subStatement.GetTrailingTrivia)
        Dim newSub As MethodStatementSyntax = subStatement.WithoutTrailingTrivia.WithHandlesClause(handlesClause)
        ' Produce a new solution that has all references to that type renamed, including the declaration.
        Dim originalSolution As Solution = document.Project.Solution
        Dim optionSet As Options.OptionSet = originalSolution.Workspace.Options

        Dim oldRoot As SyntaxNode = Await oldRootTask
        Dim newRoot As SyntaxNode = oldRoot.ReplaceNode(subStatement, newSub)
        ' Produce a new solution that has all references to that type renamed, including the declaration.
        Return document.WithSyntaxRoot(oldRoot.ReplaceNode(subStatement, newSub)).Project.Solution
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
               title:=My.Resources.CodeFixResources._AddHandler,
               createChangedSolution:=Function(c) Me.AddMissingHandlersClauseAsync(context.Document, declaration, c),
               equivalenceKey:=My.Resources.CodeFixResources._AddHandler),
               diagnostic)
    End Function

End Class
