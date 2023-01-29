' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Testing.Verifiers
Imports Microsoft.CodeAnalysis.VisualBasic.Testing

Partial Public NotInheritable Class VisualBasicCodeFixVerifier(Of TAnalyzer As {DiagnosticAnalyzer, New}, TCodeFix As {CodeFixProvider, New})

    Public Class Test
        Inherits VisualBasicCodeFixTest(Of TAnalyzer, TCodeFix, MSTestVerifier)

    End Class

End Class
