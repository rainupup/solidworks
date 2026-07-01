using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using SolidWorks.ParametricAddin.Helpers;

namespace SolidWorks.ParametricAddin
{
    [ComVisible(true)]
    [Guid("B8E7F3D1-A2C4-4E5F-9A1B-3C6D8E0F4A2C")]
    [ProgId("SolidWorks.ParametricAddin.ComHost.SwAddin")]
    public class SwAddin : ISwAddin
    {
        private ISldWorks _swApp;
        private int _cookie;
        private Window _mainWindow;
        private Thread _wpfThread;

        public bool ConnectToSW(object ThisSW, int Cookie)
        {
            try
            {
                Logger.Info("ConnectToSW started");

                _swApp = (ISldWorks)ThisSW;
                _cookie = Cookie;
                Logger.Info("ISldWorks cast OK, cookie=" + Cookie);

                _swApp.SetAddinCallbackInfo2(0, this, _cookie);
                Logger.Info("SetAddinCallbackInfo2 OK");

                AddMenuItems();
                Logger.Info("AddMenuItems done");

                // Start WPF on a dedicated STA thread with its own Dispatcher loop.
                // This is required because SolidWorks' Win32 message pump is not
                // fully compatible with WPF's nested dispatcher frames (used by
                // modal dialogs like OpenFileDialog and OpenFolderDialog).
                _wpfThread = new Thread(WpfThreadEntry);
                _wpfThread.SetApartmentState(ApartmentState.STA);
                _wpfThread.IsBackground = true;
                _wpfThread.Start();

                Logger.Info("WPF thread started, ConnectToSW returning true");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("ConnectToSW failed", ex);
                return false;
            }
        }

        public bool DisconnectFromSW()
        {
            try
            {
                Logger.Info("DisconnectFromSW started");
                RemoveMenuItems();

                if (_mainWindow != null)
                {
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        if (Application.Current != null)
                            Application.Current.Shutdown();
                    });
                    _mainWindow = null;
                }

                _swApp = null;
                Logger.Info("DisconnectFromSW done");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("DisconnectFromSW failed", ex);
                return false;
            }
        }

        private void AddMenuItems()
        {
            try
            {
                _swApp.AddMenu(1, "参数化设计工具", 6);
                _swApp.AddMenuItem2(
                    (int)swDocumentTypes_e.swDocNONE,
                    _cookie,
                    "打开设计面板@参数化设计工具",
                    -1,
                    "ParametricDesign_Show",
                    "",
                    "ParametricDesign_Show_Help"
                );
            }
            catch { }
        }

        private void RemoveMenuItems()
        {
            try { _swApp.RemoveMenu(1, "参数化设计工具", ""); }
            catch { }
        }

        /// <summary>
        /// Entry point for the dedicated WPF STA thread.
        /// Creates the Application and Window, then runs the WPF message pump.
        /// </summary>
        private void WpfThreadEntry()
        {
            Logger.Info("WpfThreadEntry started");

            var app = new Application();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // CRITICAL: Catch all unhandled WPF dispatcher exceptions.
            // Without this, any exception in WPF event handlers or bindings
            // will terminate the entire SolidWorks process.
            app.DispatcherUnhandledException += (s, e) =>
            {
                Logger.Error("WPF DispatcherUnhandledException", e.Exception);
                e.Handled = true; // Prevent process termination!
            };

            // Also catch AppDomain-level unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                Logger.Error("AppDomain.UnhandledException", ex);
            };

            // Marshal the ISldWorks COM interface into this STA thread.
            ISldWorks swAppForWpf = null;
            try
            {
                swAppForWpf = _swApp;
                Logger.Info("ISldWorks marshaled to WPF thread OK");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to marshal ISldWorks to WPF thread", ex);
            }

            try
            {
                _mainWindow = CreateMainWindow(swAppForWpf);
                _mainWindow.Closed += (s, e) =>
                {
                    Logger.Info("Main window closed");
                    _mainWindow = null;
                    app.Shutdown();
                };

                Logger.Info("Showing main window");
                _mainWindow.Show();
                app.Run();
                Logger.Info("WPF app.Run() returned");
            }
            catch (Exception ex)
            {
                Logger.Error("Fatal error in WpfThreadEntry", ex);
            }
        }

        private Window CreateMainWindow(ISldWorks swApp)
        {
            var window = new Window
            {
                Title = "参数化设计工具",
                Width = 700,
                Height = 850,
                MinWidth = 500,
                MinHeight = 650,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Topmost = false,
            };

            var control = new TaskPane.MainTaskPaneControl();
            control.Initialize(swApp);

            window.Content = control;
            return window;
        }

        /// <summary>
        /// Shows the main WPF window. Can be called from any thread.
        /// </summary>
        public void ShowMainWindow()
        {
            if (_mainWindow != null)
            {
                _mainWindow.Dispatcher.Invoke(() =>
                {
                    _mainWindow.Show();
                    _mainWindow.Activate();
                });
            }
        }

        public void ParametricDesign_Show()
        {
            ShowMainWindow();
        }

        public int ParametricDesign_Show_Help()
        {
            return 1;
        }

        #region COM Registration

        [ComRegisterFunction]
        public static void Register(Type t)
        {
            try
            {
                string keyPath = $@"SOFTWARE\SolidWorks\Addins\{t.GUID:B}";

                using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(keyPath))
                {
                    key?.SetValue(null, 1);
                    key?.SetValue("Description", "SolidWorks Parametric Design Add-in");
                    key?.SetValue("Title", "参数化设计工具");
                }
            }
            catch
            {
                try
                {
                    string keyPath = $@"SOFTWARE\SolidWorks\Addins\{t.GUID:B}";
                    using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(keyPath))
                    {
                        key?.SetValue(null, 1);
                        key?.SetValue("Description", "SolidWorks Parametric Design Add-in");
                        key?.SetValue("Title", "参数化设计工具");
                    }
                }
                catch
                {
                    // Registration not possible
                }
            }
        }

        [ComUnregisterFunction]
        public static void Unregister(Type t)
        {
            try
            {
                Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(
                    $@"SOFTWARE\SolidWorks\Addins\{t.GUID:B}", false);
            }
            catch
            {
                try
                {
                    Microsoft.Win32.Registry.CurrentUser.DeleteSubKey(
                        $@"SOFTWARE\SolidWorks\Addins\{t.GUID:B}", false);
                }
                catch
                {
                    // Ignore
                }
            }
        }

        #endregion
    }
}
