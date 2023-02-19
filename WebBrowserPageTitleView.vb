Imports System
Imports System.CodeDom.Compiler
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Markup

Namespace SensorySoftware.Grids.Grid3.Modules.PlugIns.WebBrowser
    Public Class WebBrowserPageTitleView
        Inherits UserControl
        Implements IComponentConnector

        Friend TitleTextBlock As TextBlock
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
            Application.LoadComponent(Me, New Uri("/Grid 3;component/modules/plugins/webbrowser/webbrowserpagetitleview.xaml", 2))
        End Sub

        <DebuggerNonUserCode>
        <EditorBrowsable(_, _)> ' JustDecompile was unable to locate the assembly where attribute parameters types are defined. Generating parameters values is impossible.
        <GeneratedCode("PresentationBuildTasks", "7.0.0.0")>
        Private Sub Connect(ByVal connectionId As Integer, ByVal target As Object)
            If connectionId <> 1 Then
                Me._contentLoaded = True
                Return
            End If

            Me.TitleTextBlock = CType(target, TextBlock)
        End Sub
    End Class
End Namespace
