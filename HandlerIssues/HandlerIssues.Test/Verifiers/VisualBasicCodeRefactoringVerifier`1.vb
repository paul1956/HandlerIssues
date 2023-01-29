' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Threading

Imports Microsoft.CodeAnalysis.CodeRefactorings
Imports Microsoft.CodeAnalysis.Testing
Imports Microsoft.CodeAnalysis.Testing.ReferenceAssemblies

Partial Public NotInheritable Class VisualBasicCodeRefactoringVerifier(Of TCodeRefactoring As {CodeRefactoringProvider, New})

    Private Sub New()
        Throw New NotSupportedException()
    End Sub

    ''' <inheritdoc cref="CodeRefactoringVerifier(Of TCodeRefactoring, TTest, TVerifier).VerifyRefactoringAsync(String, String)"/>
    Public Shared Async Function VerifyRefactoringAsync(source As String, fixedSource As String) As Task
        Await VerifyRefactoringAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource)
    End Function

    ''' <inheritdoc cref="CodeRefactoringVerifier(Of TCodeRefactoring, TTest, TVerifier).VerifyRefactoringAsync(String, DiagnosticResult, String)"/>
    Public Shared Async Function VerifyRefactoringAsync(source As String, expected As DiagnosticResult, fixedSource As String) As Task
        Await VerifyRefactoringAsync(source, {expected}, fixedSource)
    End Function

    ''' <inheritdoc cref="CodeRefactoringVerifier(Of TCodeRefactoring, TTest, TVerifier).VerifyRefactoringAsync(String, DiagnosticResult(), String)"/>
    Public Shared Async Function VerifyRefactoringAsync(source As String, expected As DiagnosticResult(), fixedSource As String) As Task
        Dim test As New Test With
        {
        .TestCode = source,
        .FixedCode = fixedSource,
        .ReferenceAssemblies = NetFramework.Net472.WindowsForms
        }

        test.ExpectedDiagnostics.AddRange(expected)
        Await test.RunAsync(CancellationToken.None)
    End Function

End Class
