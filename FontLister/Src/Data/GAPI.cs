using Google.Apis.Webfonts.v1;
using Google.Apis.Webfonts.v1.Data;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using SciterSharp;

namespace FontLister.Data
{
	public static class GAPI
	{
		private static string API_KEY = "AIzaSyCmfv8SVMoU_KqSRGhiz-Mzj0WWOzLoLY8";
		private static IList<Webfont> _fontlist;

		public static void Setup()
		{
			new Thread(() =>
			{
				var service = new WebfontsService(new Google.Apis.Services.BaseClientService.Initializer()
				{
					ApiKey = API_KEY
				});

				var request = service.Webfonts.List();
				request.Sort = WebfontsResource.ListRequest.SortEnum.Popularity;

				_fontlist = request.Execute().Items;// if you get an Exception here, plz disable your Firewall!
				Debug.Assert(_fontlist.Count > 0);

				// converts the Webfont list to JSON string
				string json = JsonConvert.SerializeObject(_fontlist);

				// converts the JSON string to SciterValue
				SciterValue sv = SciterValue.FromJSONString(json);

				// calls UI layer TIScript function with the font data
				App.AppHost.InvokePost(() =>
				{
					App.AppHost.CallFunction("View_OnFontList", sv);
					App.AppHost.EvalScript("view.Host_DownloadFont(\"D:/\", \"Open Sans\", function(res) {})");
				});
			}).Start();
		}

		public static void DownloadFont(string family, string savefolder)
		{
			var tmppath = Path.GetTempPath() + "FontLister_" + Guid.NewGuid().ToString() + "/";
			Directory.CreateDirectory(tmppath);

			var webfont = _fontlist.Single(wb => wb.Family == family);
			WebClient wc = new WebClient();

			foreach(var variant in webfont.Variants)
			{
				var variant_url = webfont.Files[variant];
				string ext = Path.GetExtension(variant_url);

				wc.DownloadFile(variant_url, tmppath + family + "-" + variant + ext);
			}

			ZipFile.CreateFromDirectory(tmppath, savefolder + "/" + family + ".zip");
		}
	}
}