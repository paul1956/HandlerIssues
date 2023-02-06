' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Public Module SupportFunctions

    Friend Function GetCorrectSubName(methodStatement As MethodStatementSyntax, handlesClauseItem As HandlesClauseItemSyntax) As String
        Dim correctSubName As String
        Dim memberName As String = handlesClauseItem?.EventMember?.Identifier.Text
        Dim classBlock As ClassBlockSyntax = methodStatement.FirstAncestorOfType(Of ClassBlockSyntax)
        If classBlock.ClassStatement.ContainsDiagnostics Then
            Return ""
        End If

        If TryCast(handlesClauseItem.EventContainer, KeywordEventContainerSyntax) IsNot Nothing Then
            correctSubName = $"{classBlock.ClassStatement.Identifier.Text}_{memberName}"
            If methodStatement.Identifier.Text = correctSubName Then
                Return ""
            End If
        Else
            If TryCast(handlesClauseItem.EventContainer, WithEventsEventContainerSyntax) Is Nothing Then
                Return ""
            End If
            Dim memberToken As SyntaxToken = TryCast(handlesClauseItem.EventContainer, WithEventsEventContainerSyntax).Identifier
            correctSubName = $"{memberToken.Text}_{memberName}"

            If methodStatement.Identifier.Text = correctSubName Then
                Return ""
            End If
        End If

        Return correctSubName
    End Function

End Module
