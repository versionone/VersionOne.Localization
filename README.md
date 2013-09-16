VersionOne.Localization
=======================

Components
----------


### VersionOne.Localization.ILocalizerResolver

Translates client-supplied localization ```tag``` into its corresponding localized value.

### VersionOne.Localization.Localizer

Implements ```ILocalizerResolver``` interface.

### VersionOne.Localization.LocalizationManager

### VersionOne.Localization.ITemplateSet

Represents a set of localizer translation templates. Each individual template defines a specific translation for a single localization tag.

### VersionOne.Localization.TextTemplateSet

Implementation of ```ITemplateSet``` interface for text files. Each line in a text file represents a single template, with tag and its corresponding translation separated by the equals ```=``` sign. The translation value following equals sign is optional. Lines starting with a semicolon ```;``` are treated as comments. If a localizer tag needs to contain equals sign, it must be doubled up ```==```. An underscore ```_``` at the end of the line allows translation value to span multiple lines, preserving the line breaks.

Here's an example of a template set text file illustrating these cases:

```
;Comment=Lines starting with semicolon are ignored
LongText=First line_
Second line
Empty=
Plural={1}s
Plural'Story=Stories
Command={1} {2}
Fixed=Never Change
This==has==equal==signs=Equal-sign free
```

### VersionOne.Localization.ITemplateSetLoader

### VersionOne.Localization.FileTemplateSetLoader

Sample Web Application Implementation
----------------------------------

### WebLocalizer

Localization service that implements ```ILocalizerResolver``` interface for a web application, to be configured as a singleton via DI container.

This sample web application uses a global variable to keep a singleton ```WebLocalizer``` instance configured for ```Strings``` folder under current web application root.

```csharp
public class Global : System.Web.HttpApplication
{
	protected void Application_Start()
	{
		_localizer = new WebLocalizer(Server.MapPath("~/Strings"));
	}

	private static WebLocalizer _localizer;
}
```

 Lazily creates an instance of ```LocalizationManager``` configured for ```en``` default culture with ```Base``` and ```Company``` template sets, and uses an instance of ```FileTemplateSetLoader``` to load localizer template sets from text files in a folder on disk specified by ```stringsPath``` constructor argument. Implements ```ILocalizerResolver.Resolve()``` method by delegating to a ```Localizer``` instance produced by ```LocalizationManager``` for the current UI culture specified via ```CultureInfo.CurrentUICulture```. Uses an instance of ```FileSystemWatcher``` pointed to ```stringsPath``` to watch over localization template set files, discarding current ```LocalizationManager``` instance upon detecting any changes, thus allowing localization templates to be updated at run-time without a web application restart.

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
