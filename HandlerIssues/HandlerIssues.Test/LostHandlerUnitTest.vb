' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis.Testing
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports VerifyVB = HandlerIssues.Test.VisualBasicCodeFixVerifier(
    Of HandlerIssues.MissingHandlerAnalyzer,
    HandlerIssues.AddHandlerCodeFixProvider)

Namespace HandlerIssues.Test

    <TestClass>
    Public Class LostHandlerUnitTest

        'Diagnostic And CodeFix both triggered And checked for
        <TestMethod>
        Public Async Function TestLostHandler() As Task
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

    Private Sub BGTextBox_GotFocus(sender As Object, e As EventArgs)
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

            Dim expected As DiagnosticResult = VerifyVB.Diagnostic(MissingHandlerAnalyzer.DiagnosticId) _
                                                       .WithSpan(17, 5, 17, 69) _
                                                       .WithArguments("'BGTextBox_GotFocus") _
                                                       .WithMessage("Private Sub 'BGTextBox_GotFocus' does not have a Handles clause.")
            Await VerifyVB.VerifyAnalyzerAsync(test, expected)
            Await VerifyVB.VerifyCodeFixAsync(test, expected, fixTest)

        End Function

        'No diagnostics expected to show up
        <TestMethod>
        Public Async Function TestNoAction() As Task
            Dim test As String = ""
            Await VerifyVB.VerifyAnalyzerAsync(test)
        End Function

        'No diagnostics expected to show up
        <TestMethod>
        Public Async Function TestNoAction2() As Task
            Dim test As String = "Imports System
Imports System.Windows.Forms
Public Class BGMiniWindow
    Inherits Form

    Private Sub BGTextBox_GotFocus()
    End Sub

End Class
"

            Await VerifyVB.VerifyAnalyzerAsync(test)
        End Function

        'No diagnostics expected to show up
        <TestMethod>
        Public Async Function TestNoAction3() As Task
            Dim test As String = "Imports System
Imports System.Windows.Forms
Public Class BGMiniWindow
    Inherits Form

   Private Sub GotFocus1(sender As Object, e As EventArgs)
    End Sub

    Private Sub X()
        AddHandler MyBase.GotFocus, AddressOf Me.GotFocus1
    End Sub

End Class
"

            Await VerifyVB.VerifyAnalyzerAsync(test)
        End Function

        'No diagnostics expected to show up
        <TestMethod>
        Public Async Function TestNoAction4() As Task
            Dim test As String = "Imports System
Imports System.Windows.Forms
Public Class BGMiniWindow
    Inherits Form

   Private Sub GotFocus1(sender As Object, e As EventArgs)
    End Sub

    Private Sub X()
        AddHandler MyBase.GotFocus, AddressOf GotFocus1
    End Sub

End Class
"

            Await VerifyVB.VerifyAnalyzerAsync(test)
        End Function

    End Class

End Namespace
