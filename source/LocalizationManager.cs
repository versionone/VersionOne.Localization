using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace VersionOne.Localization
{
	public class LocalizationManager
	{
		private readonly CultureInfo _defaultculture;
		private readonly IEnumerable<ITemplateProvider> _providers; 
		private readonly IDictionary _localizers;
		private readonly IDictionary<string, string> _overrides;

		public LocalizationManager(string defaultculture, ITemplateSetLoader loader, IDictionary<string, string> overrides,
			params string[] setnames)
			: this(
				new CultureInfo(defaultculture),
				setnames.Select(setname => new CompatibilityTemplateProvider(loader, setname)))
		{
			_overrides = overrides;
		}

		private LocalizationManager(CultureInfo defaultculture, ITemplateSetLoader loader, params string[] setnames)
			: this(
			defaultculture, 
			setnames.Select(setname => new CompatibilityTemplateProvider(loader, setname)))
		{}

		public LocalizationManager(CultureInfo defaultculture, IEnumerable<ITemplateProvider> providers)
		{
			_defaultculture = defaultculture;
			_providers = providers;
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
			Localizer loc = CreateLocalizer(null, _defaultculture.Name, _providers);
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
			Localizer loc = CreateLocalizer(fallbacklocalizer, culture.Name, _providers);
			return loc ?? fallbacklocalizer;
		}


		private static Localizer CreateLocalizer(Localizer fallback, string culture, IEnumerable<ITemplateProvider> providers)
		{
			Localizer loc = null;
			foreach (ITemplateProvider provider in providers)
			{
				try
				{
					using (ITemplateSet templates = provider.Load(culture))
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
					throw new TemplateSetLoadException(culture, provider.Name, e);
				}
			}

			return loc;
		}

		private class TemplateSetLoadException : ApplicationException
		{
			public TemplateSetLoadException(string culture, string name, Exception inner)
				: base(string.Format("Faied to load \"{1}\" template set for \"{0}\" culture.", culture, name), inner) { }
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
