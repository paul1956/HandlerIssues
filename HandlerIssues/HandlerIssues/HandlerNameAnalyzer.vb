' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Collections.Immutable

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class HandlerNameAnalyzer
    Inherits DiagnosticAnalyzer

    Private Const Category As String = "Naming"
    Private Shared ReadOnly Description As LocalizableString = New LocalizableResourceString(NameOf(My.Resources.SubNameWrongDescription), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Private Shared ReadOnly MessageFormat As LocalizableString = New LocalizableResourceString(NameOf(My.Resources.SubNameWrongMessageFormat), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Private Shared ReadOnly Title As LocalizableString = New LocalizableResourceString(NameOf(My.Resources.SubNameWrongTitle), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Public Const DiagnosticId As String = "HandlerNaming"
    Public Shared ReadOnly Rule As New DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault:=True, Description)

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(Rule)
        End Get
    End Property

    Private Sub AnalyzeSubStatement(context As SyntaxNodeAnalysisContext)
        If context.Node.ContainsDiagnostics Then
            Exit Sub
        End If

        If context.Node.Language <> LanguageNames.VisualBasic Then
            Exit Sub
        End If

        Dim methodStatement As MethodStatementSyntax = TryCast(context.Node, MethodStatementSyntax)
        Dim handlesClause As HandlesClauseSyntax = methodStatement.HandlesClause

        If handlesClause Is Nothing Then
            Exit Sub
        End If

        If handlesClause.Events.Count <> 1 Then
            Exit Sub
        End If
        Dim handlesClauseItem As HandlesClauseItemSyntax = handlesClause.Events(0)
        Dim classBlock As ClassBlockSyntax = methodStatement.FirstAncestorOfType(Of ClassBlockSyntax)
        Dim classStatement As ClassStatementSyntax = classBlock.ClassStatement
        If classStatement.ContainsDiagnostics Then
            Exit Sub
        End If

        Dim keywordEventContainer As KeywordEventContainerSyntax = TryCast(handlesClauseItem.EventContainer, KeywordEventContainerSyntax)
        Dim member As IdentifierNameSyntax = handlesClauseItem.EventMember
        Dim memberName As String = member.Identifier.Text
        Dim correctSubName As String
        If keywordEventContainer IsNot Nothing Then
            correctSubName = $"{classStatement.Identifier.Text}_{memberName}"
            If methodStatement.Identifier.Text = correctSubName Then
                Exit Sub
            End If
        Else
            Dim withEventsEventContainer As WithEventsEventContainerSyntax = TryCast(handlesClauseItem.EventContainer, WithEventsEventContainerSyntax)
            If withEventsEventContainer Is Nothing Then
                Exit Sub
            End If
            Dim memberToken As SyntaxToken = withEventsEventContainer.Identifier
            correctSubName = $"{memberToken.Text}_{memberName}"

            If methodStatement.Identifier.Text = correctSubName Then
                Exit Sub
            End If
        End If
        ' We have handles clause
        context.ReportDiagnostic(Diagnostic.Create(Rule,
                                 methodStatement.Identifier.GetLocation,
                                 correctSubName))

    End Sub

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None)
        context.EnableConcurrentExecution()

        context.RegisterSyntaxNodeAction(AddressOf Me.AnalyzeSubStatement, SyntaxKind.SubStatement)
    End Sub

End Class
