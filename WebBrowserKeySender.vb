Imports SensorySoftware.ComputerControl.Core
Imports SensorySoftware.Grids.Grid3.Modules.GridViewer.PlugIns.ComputerControl
Imports SensorySoftware.Grids.Grid3.Modules.GridViewer.PlugIns.WebBrowser
Imports SensorySoftware.Grids.Grid3.Modules.PlugIns.ComputerControl
Imports System
Imports System.Runtime.CompilerServices

Namespace SensorySoftware.Grids.Grid3.Modules.PlugIns.WebBrowser
    Friend Class WebBrowserKeySender
        Inherits KeySender
        Implements IWebBrowserKeySender, IKeySender

        Public Property WebBrowser As WebBrowserContainer

        Public Sub New(ByVal computerControl As IComputerControl)
            MyBase.New(computerControl)
        End Sub

        Public Overrides Sub KeyDown(ByVal virtualKeyCode As Integer, ByVal extendedKey As Boolean)
            Dim webBrowser As WebBrowserContainer = Me.WebBrowser

            If webBrowser Is Nothing Then
                Return
            End If

            webBrowser.KeyDown(virtualKeyCode)
        End Sub

        Public Sub KeyPress(ByVal virtualKeyCode As Integer)
            Dim webBrowser As WebBrowserContainer = Me.WebBrowser

            If webBrowser Is Nothing Then
                Return
            End If

            webBrowser.KeyPress(virtualKeyCode)
        End Sub

        Public Overrides Sub KeyUp(ByVal virtualKeyCode As Integer, ByVal extendedKey As Boolean)
            Dim webBrowser As WebBrowserContainer = Me.WebBrowser

            If webBrowser Is Nothing Then
                Return
            End If

            webBrowser.KeyUp(virtualKeyCode)
        End Sub

        Public Overrides Sub SendKeys(ByVal input As String, ByVal batchInput As Boolean)
            Dim str As String = KeySenderUtility.EscapeCarriageReturns(input)
            Dim webBrowser As WebBrowserContainer = Me.WebBrowser

            If webBrowser Is Nothing Then
                Return
            End If

            webBrowser.SendKeys(str, batchInput)
        End Sub
    End Class
End Namespace
