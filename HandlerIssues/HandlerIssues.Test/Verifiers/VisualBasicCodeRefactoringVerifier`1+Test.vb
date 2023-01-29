' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis.CodeRefactorings
Imports Microsoft.CodeAnalysis.Testing.Verifiers
Imports Microsoft.CodeAnalysis.VisualBasic.Testing

Partial Public NotInheritable Class VisualBasicCodeRefactoringVerifier(Of TCodeRefactoring As {CodeRefactoringProvider, New})

    Public Class Test
        Inherits VisualBasicCodeRefactoringTest(Of TCodeRefactoring, MSTestVerifier)

    End Class

End Class
