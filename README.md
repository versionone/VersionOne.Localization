VersionOne.Localization
=======================

Components
----------

### VersionOne.Localization.ILocalizerResolver

Translates client-supplied localization `tag` into its corresponding localized value.

### VersionOne.Localization.Localizer

Implements `ILocalizerResolver` interface, caching localized output of each `tag`. If current `Localizer` instance does not define a translation for a given `tag`, it delegates resolution to a `fallback` instance configured via its constructor argument. If fallback localizer instance does not exist (as is in case of the default localizer instance), a `Trim()`'ed text of the tag itself is cached and returned as its localized value.

Each localization tag is treated as a stack of individual terms separated by tics `'` (terms themselves cannot contain tics). When translating a tag, `Localizer` attempts to resolve it from the most specific to the least specific form, until it finds a matching translation template defined. For example, translating a tag `foo'bar'baz` would cause the following resolution attempts until a matching translation template is found:

* `foo'bar'baz`
* `foo'bar`
* `foo`

Translation template body can contain nested localizer tags enclosed in curly braces `{` `}`. Curly braces must be doubled up `{{` `}}` in order to include them in translation output. Nested localizer tags can be literal, or built up out of current tag's terms by specifying their zero-based index. All localizer tags nested in a translation template are resolved against current `Localizer` instance, and their translated output is substituted in the result. Translation templates matching nested localizer tags can themselves nest other localizer tags, and are resolved recursively until no further nesting is encountered.

For example, resolving `StoriesInCurrent'Iteration` tag against this set of translation templates

```
Story=Backlog Item
Iteration=Sprint
Plural={1}s
StoriesInCurrent={{0}} {Plural'Story} in current {1}
```

produces `{0} Backlog Items in current Sprint` output.

`Localizer` maintains a map of tags and their corresponding translation templates. Map content is managed via `Add()` and `Remove()` methods.

Please refer to https://github.com/versionone/VersionOne.Localization/blob/master/tests/LocalizerTester.cs for more information.

### VersionOne.Localization.LocalizationManager

Please refer to https://github.com/versionone/VersionOne.Localization/blob/master/tests/SetOverrideTester.cs and https://github.com/versionone/VersionOne.Localization/blob/master/tests/CulturalTester.cs for more information.

### VersionOne.Localization.ITemplateSet

Represents a set of localizer translation templates. Each individual template defines a specific translation for a single localization tag. Template set instances implement `IDisposable` and, thus, must be properly disposed of.

### VersionOne.Localization.TextTemplateSet

Implementation of `ITemplateSet` interface for text files. Each line in a text file represents a single template, with tag and its corresponding translation separated by the equal `=` sign. The translation value following equal sign is optional. Lines starting with a semicolon `;` are treated as comments. If a localizer tag needs to contain equal sign, it must be doubled up `==`. An underscore `_` at the end of the line allows translation value to span multiple lines, preserving the line breaks.

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

Defines a mechanism for retrieving a specific localizer template set `setname` for a given `culture`.

### VersionOne.Localization.FileTemplateSetLoader

Implements `ITemplateSetLoader` interface for building localizer template sets from a set of text files (formatted according to `TextTemplateSet` specification above) located in a folder specified by `path` constructor argument. For each requested culture and set name combination, it either produces a `ITemplateSet` instance (backed by `TextTemplateSet`) from a file named `<culture>.<setname>.txt`, or yields `null` if such file does not exist.

Sample Web Application Implementation
----------------------------------

### WebLocalizer

Localization service that implements `ILocalizerResolver` interface for a typical web application, normally configured as a singleton via DI container. It lazily creates an instance of `LocalizationManager` configured for `en` default culture with `Base` and `Custom` template sets, and uses an instance of `FileTemplateSetLoader` to load localizer template sets from text files in a folder on disk specified by `stringsPath` constructor argument. It implements `ILocalizerResolver.Resolve()` method by delegating to a `Localizer` instance produced by `LocalizationManager` for the current UI culture specified via `CultureInfo.CurrentUICulture`. It uses an instance of `FileSystemWatcher` configured to watch over localizer template set files under `stringsPath` folder, and discarding current `LocalizationManager` instance upon detecting any changes, thus allowing localization templates to be updated at run-time without a web application restart.

### NoopLocalizer

A stub `ILocalizerResolver` implementation, returning original `tag` verbatim as localized output value.

### web application

This sample web application uses a global variable to keep a singleton `WebLocalizer` instance (created by `Application_Start()` event handler) configured for `Strings` folder under current web application root.

Current culture is set up for each HTTP request by `Application_BeginRequest()` event handler, based on `Accept-Language` header sent by client browser.

When `noloc` query string parameter is present, sample application uses an instance of `NoopLocalizer` instead of `WebLocalizer` to satisfy all localization requests, effectively disabling localization for the duration of current HTTP request. It is a useful debugging technique to expose hard-coded text literals that need to be - and had not been - localized.

A default route is set up to treat entire sub-path under application root as a localizer `tag` value. Alternatively, localizer tag can be supplied via `tag` query string parameter.

A web request for `~/<some-tag>` or `~/?tag=<some-tag>` produces following JSON output:

```json
{
  tag: "<some-tag>",
  value: "<localized-value>",
  locale: "<current-locale>"
}
```
