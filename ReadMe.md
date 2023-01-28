# HandlerIssues

This vsix works with WinForms applications written in Visual Basic

It has 2 Analyzers and 2 CodeFixes

It detects handler subroutines that have names that don't match naming conventions.

The routine below should be called AboutBox1_Load if the Form is called AboutBox1
```
Private Sub About1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
```

There is a similar detector for Control handlers
```
Private Sub OKBut_Click(sender As Object, e As EventArgs) Handles OKButton.Click
```
The function about should be called OKButton_Click

It also detects Handler that are missing and offer a CodeFix to replace
```
Private Sub OKButton_Click(sender As Object, e As EventArgs)
```
This sometimes happen when WinForms designer has errors and deletes Handler Clause.
