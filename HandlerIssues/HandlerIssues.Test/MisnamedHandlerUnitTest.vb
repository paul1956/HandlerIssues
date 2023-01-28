Imports Microsoft.CodeAnalysis.Testing
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports VerifyVB = HandlerIssues.Test.VisualBasicCodeFixVerifier(
    Of HandlerIssues.HandlerNameAnalyzer,
    HandlerIssues.FixHandlerNameCodeFixProvider)

Namespace HandlerIssues.Test

    <TestClass>
    Public Class MisnamedHandlerUnitTest

        'No diagnostics expected to show up
        <TestMethod>
        Public Async Function TestNoAction() As Task
            Dim test As String = ""
            Await VerifyVB.VerifyAnalyzerAsync(test)
        End Function

        'Diagnostic And CodeFix both triggered And checked for
        <TestMethod>
        Public Async Function TestSubHandlerName() As Task
            Dim test As String = "Imports System
Imports System.Windows.Forms

Public Class BGMiniWindow
    Inherits Form

    Private WithEvents BGTextBox As TextBox

    Public Sub New()
        MyBase.New
    End Sub

    Private Shared Function GetLastUpdateMessage() As String
        Return Nothing
    End Function

    Private Sub BGBox(sender As Object, e As EventArgs) Handles BGTextBox.GotFocus
        GetLastUpdateMessage()
    End Sub

End Class
"

            Dim fixTest As String = "Imports System
Imports System.Windows.Forms

Public Class BGMiniWindow
    Inherits Form

    Private WithEvents BGTextBox As TextBox

    Public Sub New()
        MyBase.New
    End Sub

    Private Shared Function GetLastUpdateMessage() As String
        Return Nothing
    End Function

    Private Sub BGTextBox_GotFocus(sender As Object, e As EventArgs) Handles BGTextBox.GotFocus
        GetLastUpdateMessage()
    End Sub

End Class
"

            Dim expected As DiagnosticResult = VerifyVB.Diagnostic(HandlerNameAnalyzer.DiagnosticId) _
                                                         .WithSpan(17, 17, 17, 22) _
                                                         .WithArguments("BGTextBox_GotFocus") _
                                                         .WithMessage("Handler name should be 'BGTextBox_GotFocus'.")

            Await VerifyVB.VerifyCodeFixAsync(test, expected, fixTest)

        End Function

        'Diagnostic And CodeFix both triggered And checked for
        <TestMethod>
        Public Async Function TestMyBaseHandlerName() As Task
            Dim test As String = "Imports System
Imports System.Windows.Forms

Public Class BGMiniWindow
    Inherits Form

    Private WithEvents ActiveInsulinTextBox As TextBox

    Public Sub New()
        MyBase.New
    End Sub

    Private Shared Function GetLastUpdateMessage() As String
        Return Nothing
    End Function

    Private Sub BGWindow_GotFocus(sender As Object, e As EventArgs) Handles MyBase.GotFocus
    End Sub

End Class
"

            Dim fixTest As String = "Imports System
Imports System.Windows.Forms

Public Class BGMiniWindow
    Inherits Form

    Private WithEvents ActiveInsulinTextBox As TextBox

    Public Sub New()
        MyBase.New
    End Sub

    Private Shared Function GetLastUpdateMessage() As String
        Return Nothing
    End Function

    Private Sub BGMiniWindow_GotFocus(sender As Object, e As EventArgs) Handles MyBase.GotFocus
    End Sub

End Class
"

            Dim expected As DiagnosticResult = VerifyVB.Diagnostic(HandlerNameAnalyzer.DiagnosticId) _
                                                         .WithSpan(17, 17, 17, 34) _
                                                         .WithArguments("BGMiniWindow_GotFocus") _
                                                         .WithMessage("Handler name should be 'BGMiniWindow_GotFocus'.")

            Await VerifyVB.VerifyCodeFixAsync(test, expected, fixTest)

        End Function

    End Class

End Namespace
