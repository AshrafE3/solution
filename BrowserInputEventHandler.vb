Imports SensorySoftware
Imports SensorySoftware.ComponentModel
Imports SensorySoftware.Grids.Grid3.Modules.GridViewer.PlugIns.WebBrowser
Imports System
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports System.Windows
Imports System.Windows.Forms
Imports System.Windows.Input
Imports System.Windows.Threading

Namespace SensorySoftware.Grids.Grid3.Modules.PlugIns.WebBrowser
    Friend Class BrowserInputEventHandler
        Inherits DisposableBase

        Private ReadOnly _host As UIElement
        Private ReadOnly Shared HandledKeys As List(Of Key)

        Private Shared Sub New()
            Dim list As List(Of Key) = New List(Of Key)()
            list.Add(90)
            list.Add(98)
            list.Add(99)
            list.Add(100)
            list.Add(101)
            BrowserInputEventHandler.HandledKeys = list
        End Sub

        Public Sub New(ByVal browser As CefSharpWebBrowser, ByVal host As UIElement)
            Dim variable = Nothing  ' BrowserInputEventHandler.<>c__DisplayClass1_0 variable = null;	
            Dim variable1 = Nothing ' BrowserInputEventHandler.<>c__DisplayClass1_1 variable1 = null;
            Me._host = host
            MyBase.AddDisposableEventHandler(Of EventHandler(Of DomKeyEventArgs))(New Action(Of EventHandler(Of DomKeyEventArgs))(variable, Function(ByVal h As EventHandler(Of DomKeyEventArgs)) CSharpImpl.__Assign(Me.browser.DomKeyDown, h)), New Action(Of EventHandler(Of DomKeyEventArgs))(variable, Function(ByVal h As EventHandler(Of DomKeyEventArgs)) CSharpImpl.__Assign(Me.browser.DomKeyDown, h)), New EventHandler(Of DomKeyEventArgs)(Me, AddressOf BrowserInputEventHandler.OnDomKeyDown))
            MyBase.AddDisposableEventHandler(Of EventHandler(Of DomKeyEventArgs))(New Action(Of EventHandler(Of DomKeyEventArgs))(variable, Function(ByVal h As EventHandler(Of DomKeyEventArgs)) CSharpImpl.__Assign(Me.browser.DomKeyUp, h)), New Action(Of EventHandler(Of DomKeyEventArgs))(variable, Function(ByVal h As EventHandler(Of DomKeyEventArgs)) CSharpImpl.__Assign(Me.browser.DomKeyUp, h)), New EventHandler(Of DomKeyEventArgs)(Me, AddressOf BrowserInputEventHandler.OnDomKeyUp))

            If browser.get_Parent() IsNot Nothing Then
                MyBase.AddDisposableEventHandler(Of System.Windows.Forms.MouseEventHandler)(New Action(Of System.Windows.Forms.MouseEventHandler)(variable1, Function(ByVal h As System.Windows.Forms.MouseEventHandler) Me.browserParent.add_MouseDown(h)), New Action(Of System.Windows.Forms.MouseEventHandler)(variable1, Function(ByVal h As System.Windows.Forms.MouseEventHandler) Me.browserParent.remove_MouseDown(h)), New System.Windows.Forms.MouseEventHandler(Me, AddressOf BrowserInputEventHandler.OnMouseDown))
                MyBase.AddDisposableEventHandler(Of System.Windows.Forms.MouseEventHandler)(New Action(Of System.Windows.Forms.MouseEventHandler)(variable1, Function(ByVal h As System.Windows.Forms.MouseEventHandler) Me.browserParent.add_MouseUp(h)), New Action(Of System.Windows.Forms.MouseEventHandler)(variable1, Function(ByVal h As System.Windows.Forms.MouseEventHandler) Me.browserParent.remove_MouseUp(h)), New System.Windows.Forms.MouseEventHandler(Me, AddressOf BrowserInputEventHandler.OnMouseUp))
            End If
        End Sub

        Private Sub OnDomKeyDown(ByVal sender As Object, ByVal e As DomKeyEventArgs)
            Me.RaiseEventFunction(Keyboard.PreviewKeyDownEvent, Keyboard.KeyDownEvent, e)
        End Sub

        Private Sub OnDomKeyUp(ByVal sender As Object, ByVal e As DomKeyEventArgs)
            Me.RaiseEventFunction(Keyboard.PreviewKeyUpEvent, Keyboard.KeyUpEvent, e)
        End Sub

        Private Sub OnMouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
            Me.RaiseEventFunction(Mouse.PreviewMouseDownEvent, e)
        End Sub

        Private Sub OnMouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
            Me.RaiseEventFunction(Mouse.PreviewMouseUpEvent, e)
        End Sub

        Private Sub RaiseEventFunction(ByVal previewEvent As RoutedEvent, ByVal routedEvent As RoutedEvent, ByVal e As DomKeyEventArgs)
            Me.[RaiseEvent](previewEvent, e)
            e.set_Handled(Me.RaiseEventFunction(routedEvent, e))
        End Sub

        Private Function RaiseEventFunction(ByVal routedEvent As RoutedEvent, ByVal e As DomKeyEventArgs) As Boolean            
            Dim variable = Nothing ' BrowserInputEventHandler.<>c__DisplayClass6_0 variable = null;
            Dim key As Key = KeyInterop.KeyFromVirtualKey(CInt(e.get_KeyCode()))
            Dim timestamp As Integer = CInt(e.get_Timestamp())
            Dim flag As Boolean = BrowserInputEventHandler.HandledKeys.Contains(key)
            Me._host.get_Dispatcher().InvokeAsync(New Action(variable, Function()
                                                                           Dim presentationSource As PresentationSource = PresentationSource.FromDependencyObject(Me._ < _ > 4__, Me._host)

                                                                           If presentationSource Is Nothing Then
                                                                               Return
                                                                           End If

                                                                           Dim keyEventArg As KeyEventArgs = New KeyEventArgs(Keyboard.get_PrimaryDevice(), presentationSource, Me.timestamp, Me.key)
                                                                           keyEventArg.set_RoutedEvent(Me.routedEvent)
									   ' this.<>4__this._host.RaiseEvent(keyEventArg);
                                                                       End Function))
            Return flag
        End Function

        Private Sub RaiseEventFunction(ByVal routedEvent As RoutedEvent, ByVal e As System.Windows.Forms.MouseEventArgs)
            Dim uIElement As UIElement = Me._host
            Dim primaryDevice As MouseDevice = Mouse.get_PrimaryDevice()
            Dim button As MouseButtons = e.get_Button()
            Dim mouseButtonEventArg As MouseButtonEventArgs = New MouseButtonEventArgs(primaryDevice, 0, EnumUtility.Parse(Of MouseButton)(button.ToString()))
            mouseButtonEventArg.set_RoutedEvent(routedEvent)
            mouseButtonEventArg.set_Source(Me)
            uIElement.[RaiseEvent](mouseButtonEventArg)
        End Sub

        Private Class CSharpImpl
            '<Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class
End Namespace
