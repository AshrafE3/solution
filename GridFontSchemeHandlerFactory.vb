Imports CefSharp
Imports System
Imports System.Collections.Specialized
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Resources

Namespace SensorySoftware.Grids.Grid3.Modules.PlugIns.WebBrowser
    Friend NotInheritable Class GridFontSchemeHandlerFactory
        Inherits ISchemeHandlerFactory

        Public Sub New()
        End Sub

        Public Function Create(ByVal browser As IBrowser, ByVal frame As IFrame, ByVal schemeName As String, ByVal request As IRequest) As IResourceHandler
            Dim segments As String = (New Uri(request.get_Url())).get_Segments()(1)
            segments = segments.Substring(0, segments.get_Length() - ".g3ttf".get_Length())
            segments = WebUtility.UrlDecode(segments)
            Return New GridFontSchemeHandlerFactory.FontHandler(Application.GetResourceStream(New Uri(String.Concat("pack://application:,,,/Resources/Fonts/", segments, ".ttf"))).get_Stream())
        End Function

        Private NotInheritable Class FontHandler
            Inherits ResourceHandler

            Public Sub New(ByVal stream As Stream)
                MyBase.New("text/html", Nothing, False, Nothing)
                MyBase.set_Stream(stream)
                MyBase.get_Headers().set_Item("Access-Control-Allow-Origin", "*")
                MyBase.set_MimeType("application/octet-stream")
            End Sub
        End Class
    End Class
End Namespace
