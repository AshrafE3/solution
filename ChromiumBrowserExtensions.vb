Imports CefSharp
Imports CefSharp.WinForms
Imports SensorySoftware.ComputerControl.Core.Keyboard
Imports SensorySoftware.Grids.Grid3.Modules.GridViewer.PlugIns.WebBrowser
Imports System
Imports System.Runtime.CompilerServices

Namespace SensorySoftware.Grids.Grid3.Modules.PlugIns.WebBrowser
    Module ChromiumBrowserExtensions
        Private Function GetModifiers(ByVal modifiers As ModifierKeys) As CefEventFlags
            Dim cefEventFlag As CefEventFlags = 0

            If modifiers.HasFlag(CType(1, ModifierKeys)) Then
                cefEventFlag = cefEventFlag Or 8
            End If

            If modifiers.HasFlag(CType(2, ModifierKeys)) Then
                cefEventFlag = cefEventFlag Or 4
            End If

            If modifiers.HasFlag(CType(4, ModifierKeys)) Then
                cefEventFlag = cefEventFlag Or 2
            End If

            Return cefEventFlag
        End Function

        <Extension()>
        Sub SendKeyEvent(ByVal browser As ChromiumWebBrowser, ByVal eventType As SensorySoftware.Grids.Grid3.Modules.GridViewer.PlugIns.WebBrowser.KeyEventType, ByVal keyCode As Integer, ByVal charCode As Integer, ByVal modifiers As ModifierKeys)
            Dim variable = Nothing ' ChromiumBrowserExtensions.<>c__DisplayClass0_0 variable = new ChromiumBrowserExtensions.<>c__DisplayClass0_0();
            variable.browser = browser
            variable.modifiers = modifiers

            Select Case eventType
                Case 0
                    ' ChromiumBrowserExtensions.<SendKeyEvent>g__SendCefKeyEvent|0_0(0, keyCode, ref variable);
                    Return
                Case 1
                     'ChromiumBrowserExtensions.<SendKeyEvent>g__SendCefKeyEvent|0_0(2, keyCode, ref variable);
                    Return
                Case 2
                    ' ChromiumBrowserExtensions.<SendKeyEvent>g__SendCefKeyEvent|0_0(0, keyCode, ref variable);
		    ' ChromiumBrowserExtensions.<SendKeyEvent>g__SendCefKeyEvent|0_0(2, keyCode, ref variable);
                    Return
                Case 3
                    ' ChromiumBrowserExtensions.<SendKeyEvent>g__SendCefKeyEvent|0_0(0, keyCode, ref variable);
                    ' ChromiumBrowserExtensions.<SendKeyEvent>g__SendCefKeyEvent|0_0(3, charCode, ref variable);
                    ' ChromiumBrowserExtensions.<SendKeyEvent>g__SendCefKeyEvent|0_0(2, keyCode, ref variable);                    
                    Return
                Case 4
                    ' ChromiumBrowserExtensions.<SendKeyEvent>g__SendCefKeyEvent|0_0(3, charCode, ref variable);
                    Return
                Case Else
                    Return
            End Select
        End Sub
    End Module
End Namespace
