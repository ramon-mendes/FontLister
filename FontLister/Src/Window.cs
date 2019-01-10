using System;
using SciterSharp;

namespace FontLister
{
	public class Window : SciterWindow
	{
		public Window()
		{
			var wnd = this;
			wnd.CreateMainWindow(800, 600);
			wnd.CenterTopLevelWindow();
			wnd.Title = "FontLister";
			#if WINDOWS
			wnd.Icon = Properties.Resources.IconMain;
			#endif
		}
	}
}