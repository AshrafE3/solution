Imports SensorySoftware.Common
Imports SensorySoftware.ComponentModel
Imports SensorySoftware.Grids.Grid3.Modules.GridViewer
Imports SensorySoftware.Grids.Grid3.Modules.GridViewer.PlugIns.Framework
Imports SensorySoftware.Grids.Grid3.Modules.GridViewer.PlugIns.WebBrowser
Imports System
Imports System.CodeDom.Compiler
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Runtime.CompilerServices
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Forms.Integration
Imports System.Windows.Interop
Imports System.Windows.Markup

Namespace SensorySoftware.Grids.Grid3.Modules.PlugIns.WebBrowser
    Public Class WebBrowserPageView
        Inherits ContentControl
        Implements IComponentConnector, IStyleConnector

        Private _viewModel As WebBrowserPageViewModel
        Private _keyboardEventHandler As BrowserInputEventHandler
        Private _handles As List(Of IntPtr) = New List(Of IntPtr)()
        Private _host As WindowsFormsHost
        Private _webBrowserContainer As WebBrowserContainer
        Private _contentLoaded As Boolean

        Public Sub New()
            Me.InitializeComponent()
        End Sub

        <DebuggerNonUserCode>
        <GeneratedCode("PresentationBuildTasks", "7.0.0.0")>
        Public Sub InitializeComponent()
            If Me._contentLoaded Then
                Return
            End If

            Me._contentLoaded = True
            Application.LoadComponent(Me, New Uri("/Grid 3;component/modules/plugins/webbrowser/webbrowserpageview.xaml", 2))
        End Sub

        Private Sub OnBrowserRecreated(ByVal sender As Object, ByVal e As EventArgs)
            Dim browser As CefSharpWebBrowser = CType(Me._webBrowserContainer.get_Browser(), CefSharpWebBrowser)
            Me._host.set_Child(browser)

            If Me._keyboardEventHandler IsNot Nothing Then
                Me._keyboardEventHandler.Dispose()
            End If

            Me._keyboardEventHandler = New BrowserInputEventHandler(CType(Me._webBrowserContainer.get_Browser(), CefSharpWebBrowser), Me)
        End Sub

        Private Sub OnHostLoaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Me._host = CType(sender, WindowsFormsHost)
            Me._viewModel = CType(MyBase.get_DataContext(), WebBrowserPageViewModel)
            Me._webBrowserContainer = Me._viewModel.get_PlugIn().get_WebBrowserContainer()

            If Me._webBrowserContainer Is Nothing Then
                Dim webBrowserPageViewModel As WebBrowserPageViewModel = Me._viewModel
                Dim u003cu003e9_60 As CreateWebBrowserControlDelegate = Nothing' CreateWebBrowserControlDelegate u003cu003e9_60 = WebBrowserPageView.<>c.<>9__6_0;

                If u003cu003e9_60 Is Nothing Then
                    Dim SomethingStrange = Nothing ' WebBrowserPageView.<>c.<>9
                    u003cu003e9_60 = New CreateWebBrowserControlDelegate(SomethingStrange, Function(ByVal container As WebBrowserContainer, ByVal privateBrowsing As Boolean, ByVal backgroundColor As Color) New CefSharpWebBrowser(container, privateBrowsing, backgroundColor, False, "about:blank", True))
                    'WebBrowserPageView.<>c.<>9__6_0 = u003cu003e9_60;
                End If

                Me._webBrowserContainer = New WebBrowserContainer(webBrowserPageViewModel, u003cu003e9_60, True)
                Me._webBrowserContainer.add_ControlCreated(New EventHandler(Me, AddressOf WebBrowserPageView.OnWebBrowserContainerLoaded))
            End If

            Me._webBrowserContainer.add_BrowserRecreated(New EventHandler(Me, AddressOf WebBrowserPageView.OnBrowserRecreated))
            Me.OnBrowserRecreated(Me, EventArgs.Empty)
            Me._handles.Add(Me._host.get_Handle())
            Me._viewModel.get_PlugIn().get_GridViewer().get_WindowHandles().Add(Me._host.get_Handle())
        End Sub

        Private Sub OnHostUnloaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
            If Me._keyboardEventHandler IsNot Nothing Then
                Me._keyboardEventHandler.Dispose()
            End If

            For Each _handle As IntPtr In Me._handles
                Me._viewModel.get_PlugIn().get_GridViewer().get_WindowHandles().Remove(_handle)
            Next

            Me._handles.Clear()

            If Me._webBrowserContainer IsNot Nothing Then
                Me._webBrowserContainer.remove_BrowserRecreated(New EventHandler(Me, AddressOf WebBrowserPageView.OnBrowserRecreated))
            End If

            Me._host.set_Child(Nothing)
            Me._host.Dispose()
        End Sub

        Private Sub OnWebBrowserContainerLoaded(ByVal sender As Object, ByVal e As EventArgs)
            Me._webBrowserContainer.remove_ControlCreated(New EventHandler(Me, AddressOf WebBrowserPageView.OnWebBrowserContainerLoaded))
            Me._viewModel.get_PlugIn().set_WebBrowserContainer(Me._webBrowserContainer)
        End Sub

        <DebuggerNonUserCode>
        <EditorBrowsable(_, _)>  ' JustDecompile was unable to locate the assembly where attribute parameters types are defined. Generating parameters values is impossible.
        <GeneratedCode("PresentationBuildTasks", "7.0.0.0")>
        Private Sub Connect(ByVal connectionId As Integer, ByVal target As Object)
            Me._contentLoaded = True
        End Sub

        <DebuggerNonUserCode>
        <EditorBrowsable(_, _)>  ' JustDecompile was unable to locate the assembly where attribute parameters types are defined. Generating parameters values is impossible.
        <GeneratedCode("PresentationBuildTasks", "7.0.0.0")>
        Private Sub Connect(ByVal connectionId As Integer, ByVal target As Object)
            If connectionId = 1 Then
                Dim eventSetter As EventSetter = New EventSetter()
                eventSetter.set_Event(FrameworkElement.LoadedEvent)
                eventSetter.set_Handler(New RoutedEventHandler(Me, AddressOf WebBrowserPageView.OnHostLoaded))
                (CType(target, Style)).get_Setters().Add(eventSetter)
                eventSetter = New EventSetter()
                eventSetter.set_Event(FrameworkElement.UnloadedEvent)
                eventSetter.set_Handler(New RoutedEventHandler(Me, AddressOf WebBrowserPageView.OnHostUnloaded))
                (CType(target, Style)).get_Setters().Add(eventSetter)
            End If
        End Sub
    End Class
End Namespace
