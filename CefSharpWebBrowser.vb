Imports CefSharp
Imports CefSharp.Handler
Imports CefSharp.Internals
Imports CefSharp.WinForms
Imports SensorySoftware
Imports SensorySoftware.Common
Imports SensorySoftware.ComputerControl.Core.Keyboard
Imports SensorySoftware.Grids.Grid3
Imports SensorySoftware.Grids.Grid3.Framework.Locales
Imports SensorySoftware.Grids.Grid3.Framework.Translation
Imports SensorySoftware.Grids.Grid3.Modules.GridViewer.PlugIns.WebBrowser
Imports SensorySoftware.Grids.Grid3.Modules.PlugIns.Animation
Imports SensorySoftware.Grids.Storage
Imports SensorySoftware.Grids.Storage.Common
Imports SensorySoftware.Grids.Storage.PlugIns.WebBrowser.AccessibleApps.Models
Imports SensorySoftware.Grids.Storage.PlugIns.WebBrowser.AccessibleApps.Services.Abstract
Imports SensorySoftware.IO
Imports SensorySoftware.Logging
Imports SensorySoftware.Services
Imports SensorySoftware.Threading.Tasks
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

Namespace SensorySoftware.Grids.Grid3.Modules.PlugIns.WebBrowser
    Public NotInheritable Class CefSharpWebBrowser
        Inherits ChromiumWebBrowser
        Implements IWebBrowserControl, IDisposable, IContextMenuHandler, IDialogHandler, IDownloadHandler, IJsDialogHandler, IKeyboardHandler, ILifeSpanHandler, ILoadHandler

        Private ReadOnly Shared _javascriptExecutionTimeout As TimeSpan
        Private ReadOnly _container As WebBrowserContainer
        Private ReadOnly _injectGridScriptsOnFrameLoad As Boolean
        Private Const DevToolsUrl As String = "chrome-devtools://devtools/devtools_app.html"
        Private ReadOnly _attachments As Dictionary(Of String, List(Of String))
        Private ReadOnly _service As IAccessibleAppsService

        Public ReadOnly Property Url As Uri
            Get
                Dim uri As Uri = Nothing

                If Not Uri.TryCreate(Me.WebBrowser.get_Address(), 1, uri) Then
                    Return Nothing
                End If

                Return uri
            End Get
        End Property

        Private ReadOnly Property WebBrowser As IWebBrowser
            Get
                Return Me
            End Get
        End Property

        Private Shared Sub New()
            CefSharpWebBrowser._javascriptExecutionTimeout = TimeSpan.FromSeconds(10)
            Dim cefSetting As CefSettings = New CefSettings()
            cefSetting.set_LogSeverity(99)
            cefSetting.set_MultiThreadedMessageLoop(True)
            Dim cefCustomScheme As CefCustomScheme = New CefCustomScheme()
            cefCustomScheme.set_SchemeName("https")
            cefCustomScheme.set_SchemeHandlerFactory(New GridFontSchemeHandlerFactory())
            cefCustomScheme.set_DomainName("grid-fonts.com")
            cefCustomScheme.set_IsCorsEnabled(False)
            cefCustomScheme.set_IsCSPBypassing(True)
            cefCustomScheme.set_IsSecure(True)
            cefSetting.RegisterScheme(cefCustomScheme)
            cefSetting.RegisterScheme(AssetsSchemeHandlerFactory.CreateCustomScheme(KnownPictureLibraryIds.InteractiveLearning))
            cefSetting.RegisterScheme(AssetsSchemeHandlerFactory.CreateCustomScheme(KnownPictureLibraryIds.Symoji))
            Dim uiLocale As Locale = ServiceContainer.get_GlobalInstance().GetService(Of ITranslationService)().GetUiLocale(False)
            cefSetting.set_Locale(uiLocale.get_Name())
            cefSetting.get_CefCommandLineArgs().Add("autoplay-policy", "no-user-gesture-required")
            cefSetting.set_RootCachePath(GridApp.get_DataStore().GetPath(14))
            Cef.Initialize(cefSetting)
            CefSharpSettings.set_SubprocessExitIfParentProcessClosed(True)
        End Sub

        Public Sub New()
            Me.New(Nothing, False, Colors.White, False, "about:blank", True)
        End Sub

        Public Sub New(ByVal container As WebBrowserContainer, ByVal privateBrowsing As Boolean, ByVal backgroundColor As Color, ByVal Optional allowFileAccess As Boolean = False, ByVal Optional url As String = "about:blank", ByVal Optional injectGridScriptsOnFrameLoad As Boolean = True)
            MyBase.New(url, CefSharpWebBrowser.CreateRequestContext(privateBrowsing))
            Me._container = container
            Me._injectGridScriptsOnFrameLoad = injectGridScriptsOnFrameLoad
            Me.WebBrowser.set_DialogHandler(Me)
            Me.WebBrowser.set_DownloadHandler(Me)
            Me.WebBrowser.set_JsDialogHandler(Me)
            Me.WebBrowser.set_KeyboardHandler(Me)
            Me.WebBrowser.set_LifeSpanHandler(Me)
            Me.WebBrowser.set_LoadHandler(Me)
            Me.WebBrowser.set_MenuHandler(Me)
            Me.WebBrowser.set_RequestHandler(New CefSharpWebBrowser.CustomRequestHandler(Me))
            Me._attachments = New Dictionary(Of String, List(Of String))()
            MyBase.add_AddressChanged(New EventHandler(Of AddressChangedEventArgs)(Me, CefSharpWebBrowser.OnAddressChanged))
            MyBase.add_TitleChanged(New EventHandler(Of CefSharp.TitleChangedEventArgs)(Me, CefSharpWebBrowser.OnTitleChanged))
            MyBase.get_BrowserSettings().set_BackgroundColor((backgroundColor.get_A() << 24) Or (backgroundColor.get_R() << 16 ) Or (backgroundColor.get_G() << 8) Or backgroundColor.get_B())

            If allowFileAccess Then
                MyBase.get_BrowserSettings().set_FileAccessFromFileUrls(1)
                MyBase.get_BrowserSettings().set_UniversalAccessFromFileUrls(1)
            End If

            MyBase.add_IsBrowserInitializedChanged(New EventHandler(Me, CefSharpWebBrowser.OnIsBrowserInitializedChanged))
            Me._service = ServiceContainer.get_GlobalInstance().GetService(Of IAccessibleAppsService)()
        End Sub

        Private Sub OnBeforeContextMenu(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser, ByVal frame As IFrame, ByVal parameters As IContextMenuParams, ByVal model As IMenuModel)
            model.Clear()
        End Sub

        Private Function OnContextMenuCommand(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser, ByVal frame As IFrame, ByVal parameters As IContextMenuParams, ByVal commandId As CefMenuCommand, ByVal eventFlags As CefEventFlags) As Boolean
            If commandId <> 220 Then
                Return False
            End If

            WebBrowserExtensions.ShowDevTools(browser, Nothing, 0, 0)
            Return True
        End Function

        Private Sub OnContextMenuDismissed(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser, ByVal frame As IFrame)
        End Sub

        Private Function RunContextMenu(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser, ByVal frame As IFrame, ByVal parameters As IContextMenuParams, ByVal model As IMenuModel, ByVal callback As IRunContextMenuCallback) As Boolean
            Return False
        End Function

        Private Function OnFileDialog(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser, ByVal mode As CefFileDialogMode, ByVal flags As CefFileDialogFlags, ByVal title As String, ByVal defaultFilePath As String, ByVal acceptFilters As List(Of String), ByVal selectedAcceptFilter As Integer, ByVal callback As IFileDialogCallback) As Boolean
            Dim variable = Nothing  ' CefSharpWebBrowser.<>c__DisplayClass71_0 variable = null;
            Dim list As List(Of String) = New List(Of String)()

            If Me._attachments.ContainsKey(Me.Url.get_Host()) Then
                Dim item As List(Of String) = Me._attachments.get_Item(Me.Url.get_Host())

                If item IsNot Nothing Then
                    item.ForEach(New Action(Of String)(variable, Function(ByVal path As String)

                                                                     If File.Exists(path) Then
                                                                         Me.validPaths.Add(path)
                                                                     End If
                                                                 End Function))
                End If

                item.Clear()
            End If

            callback.[Continue](0, list)
            Return True
        End Function

        Private Sub OnBeforeDownload(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser, ByVal downloadItem As DownloadItem, ByVal callback As IBeforeDownloadCallback)
        End Sub

        Private Sub OnDownloadUpdated(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser, ByVal downloadItem As DownloadItem, ByVal callback As IDownloadItemCallback)
        End Sub

        Private Function OnBeforeUnloadDialog(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser, ByVal messageText As String, ByVal isReload As Boolean, ByVal callback As IJsDialogCallback) As Boolean
            Return True
        End Function

        Private Sub OnDialogClosed(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser)
        End Sub

        Private Function OnJSDialog(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser, ByVal originUrl As String, ByVal dialogType As CefJsDialogType, ByVal messageText As String, ByVal defaultPromptText As String, ByVal callback As IJsDialogCallback, ByRef suppressMessage As Boolean) As Boolean
            Dim eventHandler As EventHandler(Of JSDialogEventArgs) = Me.JSDialog

            If eventHandler IsNot Nothing Then
                eventHandler.Invoke(Me, New JSDialogEventArgs(messageText))
            Else
            End If

            suppressMessage = True
            Return False
        End Function

        Private Sub OnResetDialogState(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser)
        End Sub

        Private Function OnKeyEvent(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser, ByVal type As KeyType, ByVal windowsKeyCode As Integer, ByVal nativeKeyCode As Integer, ByVal modifiers As CefEventFlags, ByVal isSystemKey As Boolean) As Boolean
            Return False
        End Function

        Private Function OnPreKeyEvent(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser, ByVal type As KeyType, ByVal windowsKeyCode As Integer, ByVal nativeKeyCode As Integer, ByVal modifiers As CefEventFlags, ByVal isSystemKey As Boolean, ByRef isKeyboardShortcut As Boolean) As Boolean
            Dim domKeyEventArg As DomKeyEventArgs = New DomKeyEventArgs(CLng(Environment.get_TickCount()), CUInt(windowsKeyCode))

            If type <= 1 Then
                Dim eventHandler As EventHandler(Of DomKeyEventArgs) = Me.DomKeyDown

                If eventHandler IsNot Nothing Then
                    eventHandler.Invoke(Me, domKeyEventArg)
                Else
                End If
            ElseIf type = 2 Then
                Dim eventHandler1 As EventHandler(Of DomKeyEventArgs) = Me.DomKeyUp

                If eventHandler1 IsNot Nothing Then
                    eventHandler1.Invoke(Me, domKeyEventArg)
                Else
                End If
            End If

            Return domKeyEventArg.get_Handled()
        End Function

	Private Function DoClose(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser) As Boolean
            Dim flag As Boolean
            Dim mainFrame As IFrame = browser.get_MainFrame()

            Try

                If Not mainFrame.get_Url().Equals("chrome-devtools://devtools/devtools_app.html") Then
                    Return True
                Else
                    flag = False
                End If

            Finally

                If mainFrame IsNot Nothing Then
                    mainFrame.Dispose()
                End If
            End Try

            Return flag
        End Function

        Private Sub OnAfterCreated(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser)
        End Sub

        Private Sub OnBeforeClose(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser)
        End Sub

        Private Function OnBeforePopup(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser, ByVal frame As IFrame, ByVal targetUrl As String, ByVal targetFrameName As String, ByVal targetDisposition As WindowOpenDisposition, ByVal userGesture As Boolean, ByVal popupFeatures As IPopupFeatures, ByVal windowInfo As IWindowInfo, ByVal browserSettings As IBrowserSettings, ByRef noJavascriptAccess As Boolean, <Out> ByRef newBrowser As IWebBrowser) As Boolean
            If targetUrl IsNot Nothing Then
                browserControl.Load(targetUrl)
            End If

            newBrowser = Nothing
            Return True
        End Function

        Private Sub OnFrameLoadEnd(ByVal browserControl As IWebBrowser, ByVal frameLoadEndArgs As FrameLoadEndEventArgs)
            If frameLoadEndArgs.get_Frame().get_IsValid() AndAlso frameLoadEndArgs.get_Frame().get_IsMain() Then
                Dim eventHandler As EventHandler = Me.Navigated

                If eventHandler Is Nothing Then
                    Return
                End If

                eventHandler.Invoke(Me, EventArgs.Empty)
            End If
        End Sub

        Private Sub OnFrameLoadStart(ByVal browserControl As IWebBrowser, ByVal frameLoadStartArgs As FrameLoadStartEventArgs)
            If Not frameLoadStartArgs.get_Frame().get_IsValid() OrElse frameLoadStartArgs.get_Url().Equals("chrome-devtools://devtools/devtools_app.html") Then
                Return
            End If

            Try

                If Me._injectGridScriptsOnFrameLoad Then
                    Me.LoadGridScripts(frameLoadStartArgs.get_Frame())
                End If

                Dim navigatingEventArg As NavigatingEventArgs = New NavigatingEventArgs(New Uri(frameLoadStartArgs.get_Url()), frameLoadStartArgs.get_Frame().get_IsMain())
                Dim eventHandler As EventHandler(Of NavigatingEventArgs) = Me.Navigating

                If eventHandler IsNot Nothing Then
                    eventHandler.Invoke(Me, navigatingEventArg)
                Else
                End If

            Catch
            End Try
        End Sub

        Private Sub OnLoadError(ByVal browserControl As IWebBrowser, ByVal loadErrorArgs As LoadErrorEventArgs)
            If loadErrorArgs.get_Frame().get_IsValid() AndAlso loadErrorArgs.get_Frame().get_IsMain() Then

                If loadErrorArgs.get_ErrorCode() = -3 Then
                    Return
                End If

                Dim eventHandler As EventHandler = Me.MainFrameNavigationError

                If eventHandler Is Nothing Then
                    Return
                End If

                eventHandler.Invoke(Me, EventArgs.Empty)
            End If
        End Sub

        Private Sub OnLoadingStateChange(ByVal browserControl As IWebBrowser, ByVal loadingStateChangedArgs As LoadingStateChangedEventArgs)
        End Sub

        Public Sub CopySelection()
            WebBrowserExtensions.Copy(Me.WebBrowser)
        End Sub

        Private Shared Function CreateRequestContext(ByVal privateBrowsing As Boolean) As IRequestContext
            Dim requestContextSetting As RequestContextSettings = New RequestContextSettings()
            requestContextSetting.set_PersistSessionCookies(Not privateBrowsing)
            requestContextSetting.set_PersistUserPreferences(Not privateBrowsing)
            requestContextSetting.set_IgnoreCertificateErrors(False)
            Dim requestContextSetting1 As RequestContextSettings = requestContextSetting

            If Not privateBrowsing AndAlso GridApp.get_User() IsNot Nothing Then
                Dim archiveCollectionFolder As IArchiveFolder = GridApp.get_DataStore().GetArchiveCollectionFolder(3, GridApp.get_User())

                Try
                    requestContextSetting1.set_CachePath(archiveCollectionFolder.get_FullName())
                Finally

                    If archiveCollectionFolder IsNot Nothing Then
                        archiveCollectionFolder.Dispose()
                    End If
                End Try
            End If

            Return New RequestContext(requestContextSetting1)
        End Function

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If MyBase.get_IsDisposed() Then
                Return
            End If

            If disposing Then
                Dim parent As Control = MyBase.get_Parent()

                If parent IsNot Nothing Then
                    parent.get_Controls().Remove(Me)
                Else
                End If
            End If

            MyBase.Dispose(disposing)
            Dim requestContext As IRequestContext = MyBase.get_RequestContext()

            If requestContext Is Nothing Then
                Return
            End If

            requestContext.Dispose()
        End Sub

        Private Sub EvaluateAccessibleAppsJavaScript(ByVal frame As IFrame)
            Try
                Dim uri As Uri = New Uri(frame.get_Url())
            Catch
                Logger.get_Instance().Warn(String.Concat("Failed to create uri from frame Uri string property: ", frame.get_Url()))
                Return
            End Try

            For Each javaScriptResource As JavaScriptResource In Me._service.GetJavaScriptResources(New Uri(frame.get_Url()))
                frame.ExecuteJavaScriptAsync(javaScriptResource.get_JavaScript(), String.Concat("grid://grid/", javaScriptResource.get_Id()), 1)
            Next
        End Sub

        Public Function GetFrame(ByVal identifier As ULong) As IWebBrowserFrame
            Dim variable = Nothing ' CefSharpWebBrowser.<>c__DisplayClass58_0 variable = null;
            Return Nothing ' return this.GetFrameSafe(new Func<IFrame>(variable, () => this.<>4__this.GetBrowser().GetFrame((long)this.identifier)));
        End Function

        Public Function GetFrameIdentifiers() As IReadOnlyList(Of ULong)
            Dim frameIdentifiers As List(Of Long) = MyBase.GetBrowser().GetFrameIdentifiers()
            Dim u003cu003e9_590 As Func(Of Long, ULong) = Nothing ' Func<long, ulong> u003cu003e9_590 = CefSharpWebBrowser.<>c.<>9__59_0;

            If u003cu003e9_590 Is Nothing Then
		' u003cu003e9_590 = new Func<long, ulong>(CefSharpWebBrowser.<>c.<>9, (long x) => (ulong)x);
		' CefSharpWebBrowser.<>c.<>9__59_0 = u003cu003e9_590;
            End If

            Return Enumerable.ToList(Of ULong)(Enumerable.[Select](Of Long, ULong)(frameIdentifiers, u003cu003e9_590))
        End Function

        Private Function GetFrameSafe(ByVal frameCallback As Func(Of IFrame)) As IWebBrowserFrame
            Dim dummyBrowserFrame As IWebBrowserFrame
            Dim cefSharpFrame As IWebBrowserFrame

            Try
                Dim frame As IFrame = frameCallback.Invoke()

                If frame IsNot Nothing Then
                    cefSharpFrame = New CefSharpWebBrowser.CefSharpFrame(frame)
                Else
                    cefSharpFrame = Nothing
                End If

                dummyBrowserFrame = cefSharpFrame
            Catch exception As Exception When exception.get_Message().Contains("IsBrowserInitialized")
                Throw New JavaScriptExecutionException("Browser is not initialized.", exception)
            Catch objectDisposedException1 As ObjectDisposedException
                Dim objectDisposedException As ObjectDisposedException = objectDisposedException1
                Logger.get_Instance().ErrorFormat("Web browser disposed when trying to get the frame. Causes GE-40830: {0}", New Object() {objectDisposedException})
                dummyBrowserFrame = New DummyBrowserFrame()
            End Try

            Return dummyBrowserFrame
        End Function

        Public Function GetMainFrame() As IWebBrowserFrame
            Return Me.GetFrameSafe(New Func(Of IFrame)(Me, Function() WebBrowserExtensions.GetMainFrame(Me.WebBrowser)))
        End Function

        Public Function GoBack() As Boolean
            If Not Me.WebBrowser.get_CanGoBack() Then
                Return False
            End If

            WebBrowserExtensions.Back(Me.WebBrowser)
            Return True
        End Function

        Public Function GoForward() As Boolean
            If Not Me.WebBrowser.get_CanGoForward() Then
                Return False
            End If

            WebBrowserExtensions.Forward(Me.WebBrowser)
            Return True
        End Function

        Private Shared Function LoadAsync(ByVal browser As IWebBrowser, ByVal loadCallback As Action, ByVal Optional cleanupCallback As Action = Nothing) As Task
            Dim variable = Nothing ' CefSharpWebBrowser.<>c__DisplayClass94_0 variable = null;
            Dim taskCompletionSource As TaskCompletionSource(Of Boolean) = New TaskCompletionSource(Of Boolean)()
            Dim eventHandler As EventHandler(Of LoadingStateChangedEventArgs) = Nothing
            eventHandler = New EventHandler(Of LoadingStateChangedEventArgs)(variable, Function(ByVal sender As Object, ByVal args As LoadingStateChangedEventArgs)
                                                                                           Dim action As Action = Me.cleanupCallback

                                                                                           If action IsNot Nothing Then
                                                                                               action.Invoke()
                                                                                           Else
                                                                                           End If

                                                                                           If Not args.get_IsLoading() Then
                                                                                               Me.browser.remove_LoadingStateChanged(Me.handler)
                                                                                               TaskExtensions.TrySetResultAsync(Of Boolean)(Me.tcs, True)
                                                                                           End If
                                                                                       End Function)
            browser.add_LoadingStateChanged(eventHandler)
            loadCallback.Invoke()
            Return taskCompletionSource.get_Task()
        End Function

 Private Sub LoadGridScripts(ByVal frame As IFrame)
            Dim defaultInterpolatedStringHandler As DefaultInterpolatedStringHandler = New DefaultInterpolatedStringHandler(205, 2)
            defaultInterpolatedStringHandler.AppendLiteral(vbCrLf & "                window._grid_frameId = ")
            defaultInterpolatedStringHandler.AppendFormatted(Of Long)(frame.get_Identifier())
            defaultInterpolatedStringHandler.AppendLiteral(";" & vbCrLf & "                if (window.parent !== window) {" & vbCrLf & "                    window.parent.postMessage({ gridMessage: true, frameIdentifier: ")
            defaultInterpolatedStringHandler.AppendFormatted(Of Long)(frame.get_Identifier())
            defaultInterpolatedStringHandler.AppendLiteral(" }, '*');" & vbCrLf & "                }")
            frame.ExecuteJavaScriptAsync(defaultInterpolatedStringHandler.ToStringAndClear(), "about:blank", 1)
            Me.EvaluateAccessibleAppsJavaScript(frame)

            If Me._container Is Nothing Then

                For Each gridScript As WebBrowserScript In WebBrowserContainer.GridScripts
                    frame.ExecuteJavaScriptAsync(gridScript.Code, String.Concat("grid://grid/", gridScript.Name), 1)
                Next
            Else

                For Each script As WebBrowserScript In Me._container.GetScripts()
                    frame.ExecuteJavaScriptAsync(script.Code, String.Concat("grid://grid/", script.Name), 1)
                Next
            End If
        End Sub

        Public Sub Navigate(ByVal url As String)
            Me.WebBrowser.Load(url)
        End Sub

        Private Sub OnAddressChanged(ByVal sender As Object, ByVal e As AddressChangedEventArgs)
            Dim securityState As SecurityState = (If(e.get_Address().StartsWith("https://"), 2, 4))
            Dim eventHandler As EventHandler(Of SecurityStateChangedEventArgs) = Me.SecurityStateChanged

            If eventHandler Is Nothing Then
                Return
            End If

            eventHandler.Invoke(Me, New SecurityStateChangedEventArgs(securityState))
        End Sub

        Private Sub OnIsBrowserInitializedChanged(ByVal sender As Object, ByVal e As EventArgs)
            If MyBase.get_IsBrowserInitialized() Then
                MyBase.Invoke(New Action(Me, Function() MyBase.Focus()))
                Task.Run(New Action(Me, Function()
                                            Dim eventHandler As EventHandler = Me.Initialized

                                            If eventHandler Is Nothing Then
                                                Return
                                            End If

                                            eventHandler.Invoke(Me, EventArgs.Empty)
                                        End Function))
            End If
        End Sub

        Protected Overrides Sub OnMouseUp(ByVal e As MouseEventArgs)
            Dim eventHandler As EventHandler = Me.DomMouseUp

            If eventHandler IsNot Nothing Then
                eventHandler.Invoke(Me, EventArgs.Empty)
            Else
            End If

            MyBase.OnMouseUp(e)
        End Sub

        Private Sub OnTitleChanged(ByVal sender As Object, ByVal e As CefSharp.TitleChangedEventArgs)
            Dim eventHandler As EventHandler(Of SensorySoftware.Grids.Grid3.Modules.GridViewer.PlugIns.WebBrowser.TitleChangedEventArgs) = Me.TitleChanged

            If eventHandler Is Nothing Then
                Return
            End If

            eventHandler.Invoke(Me, New SensorySoftware.Grids.Grid3.Modules.GridViewer.PlugIns.WebBrowser.TitleChangedEventArgs(e.get_Title()))
        End Sub

        Public Sub Paste()
            WebBrowserExtensions.Paste(Me.WebBrowser)
        End Sub

        Public Sub Reload()
            WebBrowserExtensions.Reload(Me.WebBrowser)
        End Sub

        Public Sub ReloadWithBypass()
            WebBrowserExtensions.Reload(Me.WebBrowser, True)
        End Sub

        Public Sub SendKeyEvent(ByVal eventType As SensorySoftware.Grids.Grid3.Modules.GridViewer.PlugIns.WebBrowser.KeyEventType, ByVal keyCode As Integer, ByVal charCode As Integer, ByVal modifiers As ModifierKeys)
            Me.SendKeyEvent(eventType, keyCode, charCode, modifiers)
        End Sub

        Private Function get_Height() As Integer
            Return MyBase.get_Height()
        End Function

        Private Function get_Width() As Integer
            Return MyBase.get_Width()
        End Function

        Private Async Function GetZoomAsync() As Task(Of Single)
            Dim zoomLevelAsync As Single = (CSng(Await WebBrowserExtensions.GetZoomLevelAsync(Me.WebBrowser)) * 25F + 100F) / 100F
            Return zoomLevelAsync
        End Function

        Private Function LoadHtmlAsync(ByVal html As String) As Task
            Dim variable = Nothing ' CefSharpWebBrowser.<>c__DisplayClass93_0 variable = null;
            File.WriteAllText(Path.GetTempFileName(), html)
	    Return Nothing ' CefSharpWebBrowser.LoadAsync(this.WebBrowser, new Action(variable, () => this.<>4__this.WebBrowser.Load(this.tempFile)), new Action(variable, () => File.Delete(this.tempFile)));
        End Function

        Private Function LoadPageAsync(ByVal address As String) As Task
            Dim variable = Nothing ' CefSharpWebBrowser.<>c__DisplayClass92_0 variable = null;
            Return Nothing ' return CefSharpWebBrowser.LoadAsync(this.WebBrowser, new Action(variable, () => this.<>4__this.WebBrowser.Load(this.address)), null);
        End Function

        Private Sub set_Height(ByVal value As Integer)
            MyBase.set_Height(value)
        End Sub

        Private Sub set_Width(ByVal value As Integer)
            MyBase.set_Width(value)
        End Sub

        Private Sub SetZoom(ByVal value As Single)
            Dim single As Single = (value * 100F - 100F) / 25F
            WebBrowserExtensions.SetZoomLevel(Me.WebBrowser, CDbl(single))
        End Sub

        Private Function WaitUntilInitializedAsync() As Task
            Return TaskUtility.CompletedTask
        End Function

        Public Sub SetAttachment(ByVal attachment As String)
            If Not Enumerable.Contains(Of String)(Me._attachments.get_Keys(), Me.Url.get_Host()) Then
                Me._attachments.set_Item(Me.Url.get_Host(), New List(Of String)())
            End If

            Me._attachments.get_Item(Me.Url.get_Host()).Add(attachment)
        End Sub

        Public Sub SetScriptMessageHandler(ByVal messageHandler As ScriptMessageHandler)
            MyBase.get_JavascriptObjectRepository().Register("_gridBoundObject", messageHandler, True, Nothing)
        End Sub

        Public Sub [Stop]()
            WebBrowserExtensions.[Stop](Me.WebBrowser)
        End Sub

        Public Event BeforeLoadPage As EventHandler(Of NavigatingEventArgs)
        Public Event DomKeyDown As EventHandler(Of DomKeyEventArgs)
        Public Event DomKeyUp As EventHandler(Of DomKeyEventArgs)
        Public Event DomMouseUp As EventHandler
        Public Event FilterRequest As EventHandler(Of FilterRequestEventArgs)
        Public Event Initialized As EventHandler
        Public Event JSDialog As EventHandler(Of JSDialogEventArgs)
        Public Event MainFrameNavigationError As EventHandler
        Public Event Navigated As EventHandler
        Public Event Navigating As EventHandler(Of NavigatingEventArgs)
        Public Event SecurityStateChanged As EventHandler(Of SecurityStateChangedEventArgs)
        Public Event TitleChanged As EventHandler(Of SensorySoftware.Grids.Grid3.Modules.GridViewer.PlugIns.WebBrowser.TitleChangedEventArgs)


        Private NotInheritable Class CefSharpFrame
            Inherits IWebBrowserFrame
            Implements IDisposable

            Private ReadOnly _frame As IFrame
            Public ReadOnly Property Identifier As ULong

            Public ReadOnly Property IsFocused As Boolean
                Get
                    Return Me._frame.get_IsFocused()
                End Get
            End Property

            Public Property Name As String
                Get
                    Return get_Name()
                End Get
                Set(ByVal value As String)
                    set_Name(value)
                End Set
            End Property

            Private k__BackingField As String

            Public Function get_Name() As String 
                Return k__BackingField
            End Function

            Public Sub get_Name(Value As String)
                k__BackingField = Value
            End Sub

            Public ReadOnly Property Parent As IWebBrowserFrame

            Public Sub New(ByVal frame As IFrame)
                Dim cefSharpFrame As IWebBrowserFrame
                Me._frame = frame
                Dim parent As IFrame = frame.get_Parent()

                If parent Is Nothing OrElse Not parent.get_IsValid() Then
                    cefSharpFrame = Nothing
                Else
                    cefSharpFrame = New CefSharpWebBrowser.CefSharpFrame(parent)
                End If

                Me.Parent = cefSharpFrame
                Me.Identifier = CULng(Me._frame.get_Identifier())
                Me.Name = frame.get_Name()
            End Sub

            Public Sub Dispose()
                Dim parent As IWebBrowserFrame = Me.Parent

                If parent IsNot Nothing Then
                    parent.Dispose()
                Else
                End If

                Me._frame.Dispose()
            End Sub

            Public Overrides Function Equals(ByVal obj As Object) As Boolean
                Dim cefSharpFrame As CefSharpWebBrowser.CefSharpFrame = TryCast(obj, CefSharpWebBrowser.CefSharpFrame)

                If cefSharpFrame Is Nothing Then
                    Return False
                End If

                Return cefSharpFrame._frame.Equals(Me._frame)
            End Function

            Private Async Function EvaluateScriptAsync(ByVal script As String) As Task(Of JavascriptResponse)
                  ' CefSharpWebBrowser.CefSharpFrame.<EvaluateScriptAsync>d__17 variable = new CefSharpWebBrowser.CefSharpFrame.<EvaluateScriptAsync>d__17();
                  ' variable.<>t__builder = AsyncTaskMethodBuilder<JavascriptResponse>.Create();
                  ' variable.<>4__this = this;
                  ' variable.script = script;
                  ' variable.<>1__state = -1;
		  ' variable.<>t__builder.Start<CefSharpWebBrowser.CefSharpFrame.<EvaluateScriptAsync>d__17>(ref variable);
		  ' return variable.<>t__builder.get_Task();
		  Return Nothing
            End Function

            Public Async Function ExecuteJavaScriptAsync(ByVal script As String) As Task(Of JsonResponse)
                Dim jsonResponse As JsonResponse
        	Dim Result
        	' CefSharpWebBrowser.CefSharpFrame.<>c__DisplayClass14_0 result = new CefSharpWebBrowser.CefSharpFrame.<>c__DisplayClass14_0();

                If Me._frame.get_IsValid() Then
                    Dim javascriptResponse As JavascriptResponse = Await Me.EvaluateScriptAsync(script)

                    If javascriptResponse.get_Success() Then
                        result.result = javascriptResponse.get_Result()
                        ' jsonResponse = new JsonResponse(true, CefSharpWebBrowser.CefSharpFrame.<ExecuteJavaScriptAsync>g__getResult|14_0(ref result), null);
                    Else
                        jsonResponse = New JsonResponse(False, Nothing, javascriptResponse.get_Message())
                    End If
                Else
                    jsonResponse = New JsonResponse(False, Nothing, "Frame is no longer valid.")
                End If

                Return jsonResponse
            End Function

            Public Overrides Function GetHashCode() As Integer
                Return Me._frame.GetHashCode()
            End Function
        End Class

        Private NotInheritable Class CustomRequestHandler
            Inherits RequestHandler

            Private ReadOnly _parent As CefSharpWebBrowser

            Public Sub New(ByVal parent As CefSharpWebBrowser)
                Me._parent = parent
            End Sub

            Protected Overrides Function OnBeforeBrowse(ByVal browserControl As IWebBrowser, ByVal browser As IBrowser, ByVal frame As IFrame, ByVal request As IRequest, ByVal userGesture As Boolean, ByVal isRedirect As Boolean) As Boolean
                Dim uri As Uri = Nothing

                If Not Uri.TryCreate(request.get_Url(), 1, uri) Then
                    browser.Reload(False)
                    Return True
                End If

                Dim filterRequestEventArg As FilterRequestEventArgs = New FilterRequestEventArgs(uri)
                Dim filterRequest As EventHandler(Of FilterRequestEventArgs) = Me._parent.FilterRequest

                If filterRequest IsNot Nothing Then
                    filterRequest.Invoke(Me, filterRequestEventArg)
                Else
                End If

                If filterRequestEventArg.get_Cancel() Then
                    browser.Reload(False)
                    Return True
                End If

                Dim beforeLoadPage As EventHandler(Of NavigatingEventArgs) = Me._parent.BeforeLoadPage

                If beforeLoadPage IsNot Nothing Then
                    beforeLoadPage.Invoke(Me, New NavigatingEventArgs(uri, frame.get_IsMain()))
                Else
                End If

                Return MyBase.OnBeforeBrowse(browserControl, browser, frame, request, userGesture, isRedirect)
            End Function
        End Class    
    End Class
End Namespace
