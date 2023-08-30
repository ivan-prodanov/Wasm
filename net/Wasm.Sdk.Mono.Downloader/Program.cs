using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace MonoDownloader
{
    class Program
    {
		private static string RetreiveSDKFile(string sdkName, string sdkUri, string zipPath)
		{
			var tries = 3;

			while (--tries > 0)
			{
				try
				{
					var uri = new Uri(sdkUri);

					if (!uri.IsFile)
					{
						var client = new WebClient();
						var wp = WebRequest.DefaultWebProxy;
						wp.Credentials = CredentialCache.DefaultCredentials;
						client.Proxy = wp;

						Console.WriteLine($"Downloading {sdkName} to {zipPath}");
						client.DownloadFile(sdkUri, zipPath);

						return zipPath;
					}
					else
					{
						return uri.LocalPath;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine($"Failed to download Downloading {sdkName} to {zipPath}. Retrying... ({e.Message})");
				}
			}

			throw new Exception($"Failed to download {sdkName} to {zipPath}");
		}

		private static string GetMonoTempPath()
		{
			var path = Path.GetTempPath();
			Directory.CreateDirectory(path);

			return path;
		}

		static void Main(string[] args)
		{
			var sdkUri = Constants.DefaultDotnetRuntimeSdkUrl;

			var sdkName = Path.GetFileNameWithoutExtension(new Uri(sdkUri).AbsolutePath.Replace('/', Path.DirectorySeparatorChar));

			Console.WriteLine("NetCore-Wasm SDK: " + sdkName);
			string sdkPath = Path.Combine(GetMonoTempPath(), sdkName);
			if (args.Length > 0)
            {
				sdkPath = args[0];
            }
			Console.WriteLine("NetCore-Wasm SDK Path: " + sdkPath);

			if (!Directory.Exists(sdkPath))
			{
				var zipPath = sdkPath + ".zip";
				Console.WriteLine($"Using NetCore-Wasm SDK {sdkUri}");

				zipPath = RetreiveSDKFile(sdkName, sdkUri, zipPath);

				ZipFile.ExtractToDirectory(zipPath, sdkPath);
				Console.WriteLine($"Extracted {sdkName} to {sdkPath}");
			}
		}
    }
}
