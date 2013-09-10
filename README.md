VersionOne.Localization
=======================

Components
----------

```
VersionOne.Localization.ILocalizerResolver
```

```
VersionOne.Localization.Localizer
```

```
VersionOne.Localization.LocalizationManager
```

```
VersionOne.Localization.ITemplateSet
```

```
VersionOne.Localization.TextTemplateSet
```

```
VersionOne.Localization.Localizer
```

```
VersionOne.Localization.ITemplateSetLoader
```

```
VersionOne.Localization.FileTemplateSetLoader
```

WebLocalizer Sample Implementation
----------------------------------


Setting up current culture for each HTTP request, based on Accept-Language header sent by client browser.

```csharp
public class Global : System.Web.HttpApplication
{
	protected void Application_BeginRequest()
	{
		CultureInfo culture;
		try
		{
			culture = CultureInfo.CreateSpecificCulture(Request.UserLanguages[0]);
		}
		catch
		{
			culture = CultureInfo.InvariantCulture;
		}
		Thread.CurrentThread.CurrentCulture = culture;
		Thread.CurrentThread.CurrentUICulture = culture;
	}
}
```
