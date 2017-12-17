using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

namespace FileReplicator
{
	class Program
	{
		//0 - Live time (in minutes)
		static void Main(string[] args)
		{
			if (args.Count() == 0)
				return;

			var watchers = GetWatchers();
			Thread.Sleep(Convert.ToInt32(args[0])*60*1000);
		}

		private static void OnChanged(object source, FileSystemEventArgs e)
		{
			Thread.Sleep(1000);
			try
			{
				var paths = GetPaths();

				string fullFilename = e.FullPath;				
				string fileName = Path.GetFileName(fullFilename);
				string filePath = Path.GetDirectoryName(fullFilename);

				File.Move(fullFilename, paths[filePath] + "\\" + fileName);
			}
			catch(Exception)
			{ }
		}

		private static Dictionary<string, string> GetPaths()
		{
			var dictionary = new Dictionary<string, string>();

			dictionary[@"C:\Users\hurski\AppData\Roaming\MetaQuotes\Terminal\Common\Files\Tradevanguarda"] = @"C:\Projects\PublisherSubscriber\Tradevanguarda";
			//dictionary["D:\\Temp\\1"] = "D:\\Temp\\2";

			return dictionary;
		}

		private static List<FileSystemWatcher> GetWatchers()
		{
			var watchers = new List<FileSystemWatcher>();
			var keys = GetPaths().Keys;
			foreach (var key in keys)
			{
				watchers.Add(GetWatcher(key));
			}
			return watchers;
		}

		private static FileSystemWatcher GetWatcher(string path)
		{
			var watcher = new FileSystemWatcher();
			watcher.Path = path;
			watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
			//watcher.Filter = "*.txt";

			watcher.Changed += new FileSystemEventHandler(OnChanged);
			watcher.Created += new FileSystemEventHandler(OnChanged);

			// Begin watching.
			watcher.EnableRaisingEvents = true;

			return watcher;
		}
	}
}
