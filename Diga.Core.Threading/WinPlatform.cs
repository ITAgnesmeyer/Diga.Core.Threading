using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Diga.Core.Api.Win32;
namespace Diga.Core.Threading
{
    internal class WinPlatform : IPlatformThreadingInterface
    {
       
        public static WinPlatform Instance { get; } 
        private static Thread _uiThread;
        private WndProc _wndProcDelegate;
        private ApiHandleRef _hWnd;
        private bool disposedValue;
        private  List<Delegate> Delegates { get; }
        private static readonly uint WM_DISPATCH_WORK_ITEM = WindowsMessages.WM_USER;
        private static readonly int SignalW = unchecked((int) 0xdeadbeaf);
        private static readonly int SignalL = 0x12345678;

        static WinPlatform()
        {
            _uiThread = Thread.CurrentThread;
            Instance = new WinPlatform();
            
        }

        private WinPlatform()
        {
            this.Delegates = new List<Delegate>();
            CreateMessageWindow();
        }
        private void CreateMessageWindow()
        {
            this._wndProcDelegate = WndProcIntern;
            WndclassEx wndClass = new WndclassEx
            {
                cbSize = Marshal.SizeOf<WndclassEx>(),
                lpfnWndProc = this._wndProcDelegate,
                hInstance = Kernel32.GetModuleHandle(null),
                lpszClassName = "DigaMessageWindow_" + Guid.NewGuid().ToString()
            };
            var atom = User32.RegisterClassEx(ref wndClass);
            if (atom == 0)
            {
                throw new Win32Exception();
            }

            this._hWnd =User32.CreateWindowEx(0, atom, null, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if (!this._hWnd.IsValid)
            {
                throw new Win32Exception();
            }

        }

        private IntPtr WndProcIntern(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_DISPATCH_WORK_ITEM && wParam.ToInt64() == SignalW && lParam.ToInt64() == SignalL)
            {
                Signaled?.Invoke(null);
            }
            
            if (msg == WindowsMessages.WM_QUERYENDSESSION)
            {
                if (ShutdownRequested != null)
                {
                    var e = new ShutdownRequestedEventArgs();
                    ShutdownRequested(this, e);
                    if (e.Cancel)
                    {
                        return IntPtr.Zero;
                    }
                }
            }

            return User32.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        [SecurityCritical]
        [HandleProcessCorruptedStateExceptions]
        public void RunLoop(CancellationToken cancellationToken)
        {
            try
            {
                var result = 0;
                while (!cancellationToken.IsCancellationRequested &&
                       (result = User32.GetMessage(out var msg, IntPtr.Zero, 0, 0)) > 0)
                {
                    User32.TranslateMessage(ref msg);
                    User32.DispatchMessage(ref msg);
                }

                if (result < 0)
                {
                    //?
                }

            }
            catch (Exception e)
            {
                Debug.Print("Error:" + e.Message);
            }

        }

        public IDisposable StartTimer(DispatcherPriority priority, TimeSpan interval, Action tick)
        {
            TimerProc timerDelegate = (hWnd, uMsg, nIDEvent, dwTime) => tick();
            UIntPtr handle = User32.SetTimer(IntPtr.Zero, UIntPtr.Zero, (uint)interval.TotalMilliseconds, timerDelegate);
            this.Delegates.Add(timerDelegate);
            return Disposable.Create(() =>
            {
                this.Delegates.Remove(timerDelegate);
                User32.KillTimer(IntPtr.Zero, handle);
            });
        }

        public void Signal(DispatcherPriority priority)
        {
           User32.PostMessage(this._hWnd, WM_DISPATCH_WORK_ITEM, new IntPtr(SignalW), new IntPtr(SignalL));
        }
        [SecurityCritical]
        [HandleProcessCorruptedStateExceptions]
        public void DoEvents()
        {
            try
            {
                while (User32.PeekMessage(out MSG msg, IntPtr.Zero, 0, 0, 0x0001 | 0x0002))
                {
                    User32.TranslateMessage(ref msg);
                    User32.DispatchMessage(ref msg);
                }

            }
            catch (AccessViolationException vex)
            {
                Debug.Print("AccessViolationException=>" + vex.StackTrace);
            }
#pragma warning disable 618
            catch (ExecutionEngineException e)
#pragma warning restore 618
            {
                Debug.Print("ExecutionEngineException=>" + e.StackTrace);
            }
            catch (Exception e)
            {
                Debug.Print("DoEvents Exception:" + e.Message);
            }
                

        }

        public bool InvokeRequired =>
            User32.GetWindowThreadProcessId(this._hWnd, out _) != Kernel32.GetCurrentThreadId();
        public bool CurrentThreadIsLoopThread => _uiThread == Thread.CurrentThread;
        

        public event Action<DispatcherPriority?> Signaled;
        public event EventHandler<ShutdownRequestedEventArgs> ShutdownRequested;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (this._hWnd.IsValid)
                {
                    User32.DestroyWindow(this._hWnd);
                    this._hWnd.Dispose();
                }
       
                disposedValue = true;
            }
        }

      

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}