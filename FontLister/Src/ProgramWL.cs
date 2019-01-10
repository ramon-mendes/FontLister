#if WINDOWS || GTKMONO
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SciterSharp;
using SciterSharp.Interop;

namespace FontLister
{
	class Program
	{
#if GTKMONO
		static Program()
		{
			// see https://github.com/google/google-api-dotnet-client/issues/554#issuecomment-115729788
			var httpasm = typeof(System.Net.Http.HttpClient).Assembly;
			var httpver = new Version(1, 5, 0, 0);

			AppDomain.CurrentDomain.AssemblyResolve += (s, a) => {
				var requestedAssembly = new System.Reflection.AssemblyName(a.Name);
				if (requestedAssembly.Name != "System.Net.Http" || requestedAssembly.Version != httpver)
					return null;

				return httpasm;
			};
		}
#endif

		[STAThread]
		static void Main(string[] args)
		{
			if(IntPtr.Size == 4)
			{
				Debug.Assert(false, "sciter.dll that comes bundled in FontLister is the x64 version, make sure to change it to the x86 version if building for x86 (Windows only)");
			}

#if WINDOWS
			// Sciter needs this for drag'n'drop support; STAThread is required for OleInitialize succeess
			int oleres = PInvokeWindows.OleInitialize(IntPtr.Zero);
			Debug.Assert(oleres == 0);
#endif
#if GTKMONO
			PInvokeGTK.gtk_init(IntPtr.Zero, IntPtr.Zero);
			Mono.Setup();
#endif

			/*
				NOTE:
				In Linux, if you are getting a System.TypeInitializationException below, it is because you don't have 'libsciter-gtk-64.so' in your LD_LIBRARY_PATH.
				Run 'sudo bash install-libsciter.sh' contained in this package to install it in your system.
			*/
			App.Run();
		}
	}
}
#endif