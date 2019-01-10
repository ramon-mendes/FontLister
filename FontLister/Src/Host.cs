using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using SciterSharp;
using SciterSharp.Interop;
using FontLister.Data;

namespace FontLister
{
	class Host : BaseHost
	{
		public Host(SciterWindow wnd)
		{
			// Prepares SciterHost and then load the page
			var host = this;
			host.Setup(wnd);
			host.AttachEvh(new HostEvh());
			host.SetupPage("index.html");
			//host.DebugInspect();// >-- call it after SetupPage(), dont forget to run inspector.exe 
			wnd.Show();
		}

		// Things to do here:
		// -override OnLoadData() to customize or track resource loading
		// -override OnPostedNotification() to handle notifications generated with SciterHost.PostNotification()
	}

	class HostEvh : SciterEventHandler
	{
		protected override bool OnScriptCall(SciterElement se, string name, SciterValue[] args, out SciterValue result)
		{
			result = null;
			switch(name)
			{
				case "Host_DownloadFont":
					string savefolder = args[0].Get("");
					string family = args[1].Get("");
					SciterValue async_cbk = args[2];
					string str = async_cbk.ToString();

					Task.Run(() =>
					{
						bool res;
						try
						{
							GAPI.DownloadFont(family, savefolder);
							res = true;
						}
						catch(Exception)
						{
							res = false;
						}

						App.AppHost.InvokePost(() =>
						{
							async_cbk.Call(new SciterValue(res));
						});
					});

					return true;
			}
			return false;
		}
	}

	// This base class overrides OnLoadData and does the resource loading strategy
	// explained at http://misoftware.com.br/Bootstrap/Dev
	//
	// - in DEBUG mode: resources loaded directly from the file system
	// - in RELEASE mode: resources loaded from by a SciterArchive (packed binary data contained as C# code in ArchiveResource.cs)
	class BaseHost : SciterHost
	{
		protected static SciterArchive _archive = new SciterArchive();
		protected SciterWindow _wnd;
		private static string _rescwd;

		static BaseHost()
		{
#if DEBUG
			_rescwd = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace('\\', '/');
	#if OSX
			_rescwd += "/../../../../../res/";
	#else
			_rescwd += "/../../res/";
	#endif

			_rescwd = Path.GetFullPath(_rescwd).Replace('\\', '/');
			Debug.Assert(Directory.Exists(_rescwd));
#else
			_archive.Open(SciterAppResource.ArchiveResource.resources);
#endif
		}

		public void Setup(SciterWindow wnd)
		{
			_wnd = wnd;
			SetupWindow(wnd);
		}

		public void SetupPage(string page_from_res_folder)
		{
			string path = _rescwd + page_from_res_folder;
			Debug.Assert(File.Exists(path));

#if DEBUG
			string url = "file://" + path;
#else
			string url = "archive://app/" + page_from_res_folder;
#endif

			bool res = _wnd.LoadPage(url);
			Debug.Assert(res);
		}

		protected override SciterXDef.LoadResult OnLoadData(SciterXDef.SCN_LOAD_DATA sld)
		{
			if(sld.uri.StartsWith("archive://app/"))
			{
				// load resource from SciterArchive
				string path = sld.uri.Substring(14);
				byte[] data = _archive.Get(path);
				if(data!=null)
					SciterX.API.SciterDataReady(sld.hwnd, sld.uri, data, (uint) data.Length);
			}

			// call base to ensure LibConsole is loaded
			return base.OnLoadData(sld);
		}
	}
}