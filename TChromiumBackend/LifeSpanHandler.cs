using CefSharp;

namespace TChromiumBackend
{
	public class LifeSpanHandler : ILifeSpanHandler
	{
		public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
		{
			chromiumWebBrowser.Load(targetUrl);
			newBrowser = null;
			return true;
		}

		public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
		{
		}

		public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
		{
			return false;
		}

		public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
		{
		}
	}
}