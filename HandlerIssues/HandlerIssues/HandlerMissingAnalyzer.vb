Imports System.Collections.Immutable

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public Class MissingHandlerAnalyzer
    Inherits DiagnosticAnalyzer

    Private Const Category As String = "Event Handler"
    Private Shared ReadOnly s_description As LocalizableString = New LocalizableResourceString(NameOf(My.Resources.HandlerClauseMissingDescription), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Private Shared ReadOnly s_messageFormat As LocalizableString = New LocalizableResourceString(NameOf(My.Resources.HandlerClauseMissingMessageFormat), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Private Shared ReadOnly s_title As LocalizableString = New LocalizableResourceString(NameOf(My.Resources.HandlerClauseMissingTitle), My.Resources.ResourceManager, GetType(My.Resources.Resources))
    Public Const DiagnosticId As String = "MissingHandler"
    Public Shared ReadOnly s_rule As New DiagnosticDescriptor(DiagnosticId, s_title, s_messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault:=True, description:=s_description)

    Public Overrides ReadOnly Property SupportedDiagnostics As ImmutableArray(Of DiagnosticDescriptor)
        Get
            Return ImmutableArray.Create(s_rule)
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

            '  We have handles clause
            If methodStatement.Modifiers.Count > 0 AndAlso Not methodStatement.Modifiers.Contains(SyntaxKind.PrivateKeyword) Then
                ' Function is not Private
                Exit Sub
            End If

            If methodStatement.ParameterList?.Parameters.Count = 2 Then
                ' Need to check parameter list
                ' Check if first parameter is sender as Object
                Dim parameter As ParameterSyntax = methodStatement.ParameterList?.Parameters.First
                Dim identifier As ModifiedIdentifierSyntax = parameter.Identifier
                If Not parameter.Identifier.Identifier.Text.Equals("sender", StringComparison.OrdinalIgnoreCase) Then
                    ' Looking for (sender as object,...
                    Exit Sub
                End If

                If parameter.AsClause Is Nothing Then
                    Exit Sub
                End If

                Dim type1 As PredefinedTypeSyntax = TryCast(parameter.AsClause.Type, PredefinedTypeSyntax)
                If type1 Is Nothing Then
                    Exit Sub
                End If

                If type1.Keyword.Text <> "Object" Then
                    Exit Sub
                End If
            End If
            Dim diagnostic As Diagnostic = Diagnostic.Create(s_rule,
                                     methodStatement.GetLocation,
                                     methodStatement.Identifier.Text)
            context.ReportDiagnostic(diagnostic)
        End If
    End Sub

    Public Overrides Sub Initialize(context As AnalysisContext)
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None)
        context.EnableConcurrentExecution()

        context.RegisterSyntaxNodeAction(AddressOf Me.AnalyzeSubStatement, SyntaxKind.SubStatement)
    End Sub

End Class
