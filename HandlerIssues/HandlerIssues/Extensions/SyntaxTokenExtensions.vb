' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Runtime.CompilerServices

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic

Public Module SyntaxTokenExtensions

    <Extension>
    Friend Function Contains(tokens As SyntaxTokenList, kind As SyntaxKind) As Boolean
        Return tokens.Any(Function(m As SyntaxToken) m.IsKind(kind))
    End Function

End Module
