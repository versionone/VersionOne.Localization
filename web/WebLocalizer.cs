using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using VersionOne.Localization;

namespace web
{
	internal class WebLocalizer : ILocalizerResolver
	{
		private LocalizationManager _manager;
		private readonly FileSystemWatcher _watcher;
		private readonly string _path;

		public WebLocalizer(string stringsPath)
		{
			_path = stringsPath;
			_watcher = new FileSystemWatcher(_path) { Filter = "*.txt", NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName };
			_watcher.Changed += Changed;
			_watcher.Deleted += Changed;
			_watcher.Renamed += Changed;
			_watcher.EnableRaisingEvents = true;
		}
		
		private void Changed(object sender, EventArgs args)
		{
			_manager = null;
		}

		private readonly string[] _setnames = new[] { "Base", "Custom" };

		private LocalizationManager Man 
		{ 
			get { return _manager ?? (_manager = new LocalizationManager("en", new FileTemplateSetLoader(_path), null, _setnames)); } 
		}

		private ILocalizerResolver Loc { get { return Man.GetLocalizer(CultureInfo.CurrentUICulture); } }

		public string Resolve(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
#if DEBUG
				var method = new StackFrame(1).GetMethod();
				throw new ArgumentException((key == null ? "NULL" : "Empty") + " localization tag from '" + method.DeclaringType.Name + "." + method.Name + "'.", "key");
#else
				return key;
#endif
			}

			return Loc.Resolve(key);
		}
	}
}
