using CefSharp;
using CefSharp.OffScreen;

namespace TChromiumBackend
{
	public class Browser : ChromiumWebBrowser
	{
		public Browser(BrowserSettings settings) : base(browserSettings: settings)
		{
		}

		public new IBrowser GetBrowser()
		{
			return base.GetBrowser();
		}
	}
}