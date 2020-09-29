# <img align="right" src="https://raw.githubusercontent.com/Benio101/Color.Misc/master/Color.Misc/Logo.ico"> Color.Misc
[Visual Studio](https://visualstudio.microsoft.com) extension: Color C++ Miscs.

## Status
| Branch | Build Status
| ---    | ---
| [`master`](https://github.com/Benio101/Color.Misc/tree/master) | [![Build status](https://ci.appveyor.com/api/projects/status/h64o7032q4a0pw67/branch/master?svg=true)](https://ci.appveyor.com/project/Benio101/color-misc/branch/master)

## Description
Extension allows colorizing certain C++ code parts using regular expressions.<br>
Extension works in files of `ContentType` `"C/C++"`, _eg_ `.cpp` or `.h` files.

## Usage
1. Edit region `rules` from function `Classifier.Classify` of a file `Classifier.cs`. Existing rules are left as an example.<br>
2. Either delete them all and add your own rules, or begin with editing existing ones.<br>
3. Finally, compile, test and (re)install output `.vsix` extension by running it.

### Rules
In order to add a regex rule, add another `Colorize` to `Colorizes` List in a `rules` region mentioned above.<br>
Structure of `Colorizes` is self-explaining, yet let's highlight it here:
```.cs
public struct Replacement
{
	public string Name;
	public IClassificationType Color;
}

public struct Condition
{
	public string Name;

	public List<string>? CantBeClassifiedAs;
	public List<string>? MustBeClassifiedAs;
	public List<string>? MustBeClassifiedAsAnyOf;
}

public struct Colorize
{
	public Regex Regex;
	public List<Condition>? Conditions;
	public List<Replacement> Replacements;
}

public struct Colors
{
	public string Name;
	public IClassificationType Color;
}
```

Note that classifications are not documented.<br>
I lost way too much time asking for the documentation.

## Colors
Finally, in order to change colors, edit them in `Definitions.cs`, `Style.cs`, `Default.cs` and `Classifier.cs`.<br>
Sorry, I didn't found any easy way to use them, _eg_ by regex RGB. Same as witch classifications, MSVC extension writing support is almost not existing. Let me not rant about it. It's enough that .NET sucks, aswell as all vsix api. I will never get my lost time back.

## Example
The following code adds a rule that will change color of all keywords `private` to `Red`.
Note that the keywords is being found by regex, it must be classified as `keyword` and cannot be part of neither a comment (_normal_: `comment`, triple slash _aka_ xml: `XML Doc Comment`) nor a string literal (`string`).
```.cs
// private
Colorizes.Add(new Colorize(){
	Regex = new Regex
	(
		@"(?<Access>" + Utils.IdentifierBeginningBoundary + @"private" + Utils.IdentifierEndingBoundary + @")"
	),
	Conditions = new List<Condition>(){
		new Condition(){
			Name = "Access",
			CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
			MustBeClassifiedAs = new List<string>(){"keyword"},
		},
	},
	Replacements = new List<Replacement>(){
		new Replacement(){Name = "Access", Color = Red},
	},
});
```

## Afterwords
Wish you best luck with creating your own regex rules.
