VersionOne.Localization
=======================

Components
----------


### VersionOne.Localization.ILocalizerResolver

### VersionOne.Localization.Localizer

### VersionOne.Localization.LocalizationManager

### VersionOne.Localization.ITemplateSet

Represents a set of localizer translation templates. Each individual template defines a specific trsnslation for a single localization tag.

### VersionOne.Localization.TextTemplateSet

Concrete implementation ITemplateSet interface for text files. Each line in a text file represents a single template, with tag and its corresponding translation separated by the equals ```=``` sign. The translation value following equalt sign is optional. Lines starting with a semicolon ```;``` are treated as comments. If a localizer tag needs to contain equals sign, it must be doubled up ```==```. An underscore ```_``` at the end of the line allows translation value to span multiple lines, preserving the line breaks.

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

### VersionOne.Localization.Localizer

### VersionOne.Localization.ITemplateSetLoader

### VersionOne.Localization.FileTemplateSetLoader

Sample Web Application Implementation
----------------------------------

### WebLocalizer

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
