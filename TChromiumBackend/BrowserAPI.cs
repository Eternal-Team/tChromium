using CefSharp;
using CefSharp.OffScreen;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TChromiumBackend
{
	public class BrowserAPI
	{
		private Browser browser;
		private IBrowserHost Host => browser.GetBrowserHost();
		public event Action<IntPtr, int, int> Paint;

		public BrowserAPI()
		{
			AppDomain.CurrentDomain.AssemblyResolve += Resolver;

			LoadApp();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void LoadApp()
		{
			string terrariaPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

			var settings = new CefSettings
			{
				BrowserSubprocessPath = Path.Combine(terrariaPath, Environment.Is64BitProcess ? "x64" : "x86", "CefSharp.BrowserSubprocess.exe"),
				RootCachePath = Path.Combine(terrariaPath, "tChromiumCache"),
				CachePath = Path.Combine(terrariaPath, "tChromiumCache"),
				WindowlessRenderingEnabled = true
			};
			settings.EnableAudio();

			Cef.Initialize(settings, false, browserProcessHandler: null);

			var browserSettings = new BrowserSettings
			{
				BackgroundColor = Cef.ColorSetARGB(255, 255, 255, 255),
				WindowlessFrameRate = 60
			};
			browser = new Browser(browserSettings)
			{
				LifeSpanHandler = new LifeSpanHandler()
			};

			browser.BrowserInitialized += Browser_BrowserInitialized;

			browser.Paint += Browser_Paint;
		}

		#region Events
		public void MouseMove(int x, int y)
		{
			Host.SendMouseMoveEvent(x, y, false, CefEventFlags.None);
		}

		public void MouseScroll(int x, int y, int dX, int dY)
		{
			Host.SendMouseWheelEvent(x, y, dX, dY, CefEventFlags.None);
		}

		public void MouseDown(int x, int y, int button)
		{
			Host.SendMouseClickEvent(x, y, (MouseButtonType)button, false, 1, CefEventFlags.None);
		}

		public void MouseUp(int x, int y, int button)
		{
			Host.SendMouseClickEvent(x, y, (MouseButtonType)button, true, 1, CefEventFlags.None);
		}

		public void KeyDown(int key)
		{
			KeyEvent keyEvent = new KeyEvent
			{
				WindowsKeyCode = key,
				Type = KeyEventType.KeyDown,
				IsSystemKey = false,
				Modifiers = CefEventFlags.None,
				FocusOnEditableField = true
			};

			Host.SendKeyEvent(keyEvent);
		}

		public void KeyUp(int key)
		{
			KeyEvent keyEvent = new KeyEvent
			{
				WindowsKeyCode = key,
				Type = KeyEventType.KeyUp,
				IsSystemKey = false,
				Modifiers = CefEventFlags.None,
				FocusOnEditableField = true
			};

			Host.SendKeyEvent(keyEvent);
		}

		public void KeyTyped(int key)
		{
			KeyEvent keyEvent = new KeyEvent
			{
				WindowsKeyCode = key,
				Type = KeyEventType.Char,
				IsSystemKey = false,
				Modifiers = CefEventFlags.None,
				FocusOnEditableField = true
			};

			Host.SendKeyEvent(keyEvent);
		}

		public void Focus()
		{
			Host.SendFocusEvent(true);
		}

		public void Unfocus()
		{
			Host.SendFocusEvent(false);
		}
		#endregion

		public void Load(string url)
		{
			browser.Load(url);
		}

		public void SetSize(int width, int height)
		{
			browser.Size = new Size(width, height);
		}

		public void Shutdown()
		{
			Cef.Shutdown();
		}

		private void Browser_Paint(object sender, OnPaintEventArgs e)
		{
			Paint?.Invoke(e.BufferHandle, e.Width, e.Height);
		}

		public void ChangeVolume(int level)
		{
			foreach (Process process in Process.GetProcesses())
			{
				if (!process.ProcessName.Contains("CefSharp")) continue;

				Debug.WriteLine(process.Id);
				VolumeMixer.SetApplicationVolume(process.Id, level);
			}
		}

		private void Browser_BrowserInitialized(object sender, EventArgs e)
		{
			browser.LoadHtml(@"


<!DOCTYPE html>
<html>
<body>

<iframe width='560' height='315' src='https://www.youtube.com/embed/662JyNLVpxE?&autoplay=1' frameborder='0' allowfullscreen></iframe>

</body>
</html>


");
		}

		private static Assembly Resolver(object sender, ResolveEventArgs args)
		{
			if (args.Name.StartsWith("CefSharp"))
			{
				string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
				string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, Environment.Is64BitProcess ? "x64" : "x86", assemblyName);

				return File.Exists(archSpecificPath) ? Assembly.LoadFile(archSpecificPath) : null;
			}

			return null;
		}
	}
}