' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.CSharp.Testing
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Testing.Verifiers

Partial Public NotInheritable Class CSharpCodeFixVerifier(Of TAnalyzer As {DiagnosticAnalyzer, New}, TCodeFix As {CodeFixProvider, New})

    Public Class Test
        Inherits CSharpCodeFixTest(Of TAnalyzer, TCodeFix, MSTestVerifier)

        Public Sub New()
            Me.SolutionTransforms.Add(
                Function(solution, projectId)
                    Dim compilationOptions = solution.GetProject(projectId).CompilationOptions
                    compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                        compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings))
                    solution = solution.WithProjectCompilationOptions(projectId, compilationOptions)

                    Return solution
                End Function)
        End Sub

    End Class

End Class
