using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SciterSharp;
using SciterSharp.Interop;

namespace FontLister
{
	class SciterMessages : SciterDebugOutputHandler
	{
		protected override void OnOutput(SciterSharp.Interop.SciterXDef.OUTPUT_SUBSYTEM subsystem, SciterSharp.Interop.SciterXDef.OUTPUT_SEVERITY severity, string text)
		{
			Console.WriteLine(text);// prints to console Sciter's debug output (works even if 'native debugging' is off)
		}
	}

	static class App
	{
		private static SciterMessages sm = new SciterMessages();
		public static SciterWindow AppWnd { get; private set; }
		public static Host AppHost { get; private set; }

		public static void Run()
		{
			AppWnd = new SciterWindow();

			var wnd = AppWnd;
			wnd.CreateMainWindow(800, 600);
			wnd.CenterTopLevelWindow();
			wnd.Title = "FontLister";
#if WINDOWS
			wnd.Icon = Properties.Resources.IconMain;
#endif

			// Prepares SciterHost and then load the page
			AppHost = new Host(AppWnd);

			Data.GAPI.Setup();

#if !OSX
			PInvokeUtils.RunMsgLoop();
#endif
		}
	}
}