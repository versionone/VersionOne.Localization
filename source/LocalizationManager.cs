using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace VersionOne.Localization
{
	public class LocalizationManager
	{
		private readonly CultureInfo _defaultculture;
		private readonly ITemplateSetLoader _loader;
		private readonly string[] _setnames;
		private readonly IDictionary _localizers;
		private readonly IDictionary<string, string> _overrides;

		public LocalizationManager(string defaultculture, ITemplateSetLoader loader, IDictionary<string, string> overrides, params string[] setnames)
			: this(new CultureInfo(defaultculture), loader, setnames)
		{
			_overrides = overrides;
		}

		private LocalizationManager(CultureInfo defaultculture, ITemplateSetLoader loader, params string[] setnames)
		{
			_defaultculture = defaultculture;
			_loader = loader;
			_setnames = setnames;
			_localizers = new Hashtable();
		}

		public Localizer GetLocalizer(string culture)
		{
			return GetLocalizer(new CultureInfo(culture));
		}

		public Localizer GetLocalizer (CultureInfo culture)
		{
			Localizer loc = LookupLocalizer(culture);
			if (loc == null)
				lock (_localizers.SyncRoot)
				{
					loc = LookupLocalizer(culture);
					if (loc == null)
					{
						if (culture.Equals(_defaultculture))
							loc = CreateDefaultLocalizer();
						else if (culture.IsNeutralCulture || culture.Equals(CultureInfo.InvariantCulture))
							loc = CreateLocalizer(culture, _defaultculture);
						else
							loc = CreateLocalizer(culture, culture.Parent);

						CacheLocalizer(culture, loc);
					}
				}
			return loc;
		}

		public Localizer DefaultLocalizer
		{
			get
			{
				return GetLocalizer(_defaultculture);
			}
		}

		private Localizer LookupLocalizer (CultureInfo culture)
		{
			return (Localizer) _localizers[culture];
		}
		private void CacheLocalizer (CultureInfo culture, Localizer loc)
		{
			_localizers.Add(culture, loc);
		}

		private Localizer CreateDefaultLocalizer ()
		{
			Localizer loc = CreateLocalizer(null, _defaultculture.Name, _loader, _setnames);
			if (loc != null)
			{
				if (_overrides != null)
					foreach (KeyValuePair<string, string> entry in _overrides)
						loc.Add(entry.Key, entry.Value);
				return loc;
			}
			return new Localizer(null);
		}

		private Localizer CreateLocalizer (CultureInfo culture, CultureInfo fallbackculture)
		{
			Localizer fallbacklocalizer = GetLocalizer(fallbackculture);
			Localizer loc = CreateLocalizer(fallbacklocalizer, culture.Name, _loader, _setnames);
			return loc ?? fallbacklocalizer;
		}


		private static Localizer CreateLocalizer (Localizer fallback, string culture, ITemplateSetLoader loader, params string[] setNames)
		{
			if (setNames == null)
				throw new ArgumentNullException("setNames");
			Localizer loc = null;
			foreach (string setname in setNames)
			{
				try
				{
					using (ITemplateSet templates = loader.Load(culture, setname))
					{
						if (templates != null)
						{
							if (loc == null)
								loc = new Localizer(fallback);
							FillLocalizer(loc, templates);
						}
					}
				}
				catch (Exception e)
				{
					throw new TemplateSetLoadException(culture, setname, e);
				}
			}
			return loc;
		}

		private class TemplateSetLoadException : ApplicationException
		{
			public TemplateSetLoadException(string culture, string setname, Exception inner)
				: base(string.Format("Faied to load \"{1}\" template set for \"{0}\" culture.", culture, setname), inner) { }
		}

		private static void FillLocalizer (Localizer loc, ITemplateSet templates)
		{
			for (Template t = templates.GetNextTemplate(); t != null; t = templates.GetNextTemplate())
			{
				if (string.IsNullOrEmpty(t.Translation))
					loc.Remove(t.Tag);
				else
					loc.Add(t.Tag, t.Translation);
			}
		}
	}
}
