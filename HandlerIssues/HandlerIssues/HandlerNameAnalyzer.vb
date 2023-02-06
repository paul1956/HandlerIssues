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

        Dim correctSubName As String
        correctSubName = GetCorrectSubName(methodStatement, handlesClauseItem)
        ' We have handles clause
        If String.IsNullOrWhiteSpace(correctSubName) Then
            Exit Sub
        End If
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
