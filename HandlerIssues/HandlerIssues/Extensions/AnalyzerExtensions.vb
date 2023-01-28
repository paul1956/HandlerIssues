' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Runtime.CompilerServices

Imports Microsoft.CodeAnalysis

Public Module AnalyzerExtensions

    <Extension>
    Public Iterator Function AllBaseTypes(typeSymbol As INamedTypeSymbol) As IEnumerable(Of INamedTypeSymbol)
        Do While typeSymbol.BaseType IsNot Nothing
            Yield typeSymbol.BaseType
            typeSymbol = typeSymbol.BaseType
        Loop
    End Function

    <Extension>
    Public Iterator Function AllBaseTypesAndSelf(typeSymbol As INamedTypeSymbol) As IEnumerable(Of INamedTypeSymbol)
        Yield typeSymbol
        For Each b As INamedTypeSymbol In AllBaseTypes(typeSymbol)
            Yield b
        Next
    End Function

    <Extension>
    Public Function FirstAncestorOfType(Of T As SyntaxNode)(node As SyntaxNode) As T
        Dim currentNode As SyntaxNode = node
        Do
            Dim parent As SyntaxNode = currentNode.Parent
            If parent Is Nothing Then
                Exit Do
            End If

            Dim tParent As T = TryCast(parent, T)
            If tParent IsNot Nothing Then
                Return tParent
            End If

            currentNode = parent
        Loop

        Return Nothing
    End Function

    <Extension>
    Public Function FirstAncestorOfType(node As SyntaxNode, ParamArray types() As Type) As SyntaxNode
        Dim currentNode As SyntaxNode = node
        Do
            Dim parent As SyntaxNode = currentNode.Parent
            If parent Is Nothing Then
                Exit Do
            End If

            For Each type As Type In types
                If parent.GetType() Is type Then
                    Return parent
                End If
            Next type

            currentNode = parent
        Loop

        Return Nothing
    End Function

    <Extension>
    Public Function FirstAncestorOrSelfOfType(Of T As SyntaxNode)(node As SyntaxNode) As T
        Return CType(node.FirstAncestorOrSelfOfType(GetType(T)), T)
    End Function

    <Extension>
    Public Function FirstAncestorOrSelfOfType(node As SyntaxNode, ParamArray types() As Type) As SyntaxNode
        Dim currentNode As SyntaxNode = node
        Do
            If currentNode Is Nothing Then
                Exit Do
            End If

            For Each type As Type In types
                If currentNode.GetType() Is type Then
                    Return currentNode
                End If
            Next type

            currentNode = currentNode.Parent
        Loop

        Return Nothing
    End Function

    <Extension>
    Public Function GetAllMethodsIncludingFromInnerTypes(typeSymbol As INamedTypeSymbol) As IList(Of IMethodSymbol)
        Dim methods As List(Of IMethodSymbol) = typeSymbol.GetMembers().OfType(Of IMethodSymbol)().ToList()
        Dim innerTypes As IEnumerable(Of INamedTypeSymbol) = typeSymbol.GetMembers().OfType(Of INamedTypeSymbol)()
        For Each innerType As INamedTypeSymbol In innerTypes
            methods.AddRange(innerType.GetAllMethodsIncludingFromInnerTypes())
        Next

        Return methods
    End Function

    <Extension>
    Public Function IsAnyKind(displayPart As SymbolDisplayPart, ParamArray kinds() As SymbolDisplayPartKind) As Boolean
        For Each kind As SymbolDisplayPartKind In kinds
            If displayPart.Kind = kind Then
                Return True
            End If
        Next kind

        Return False
    End Function

End Module
