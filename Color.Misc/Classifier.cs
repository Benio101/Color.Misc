using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Color.Misc
{
	internal class Classifier
	:
		IClassifier
	{
		private          bool        IsClassificationRunning;
		private readonly IClassifier IClassifier;

		#pragma warning disable 67
		public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
		#pragma warning restore 67
		
		#pragma warning disable IDE0052 // Remove unread private members
		private readonly IClassificationType White;
		private readonly IClassificationType Silver;
		private readonly IClassificationType Gray;
		private readonly IClassificationType Dark;
		private readonly IClassificationType Black;
		private readonly IClassificationType Red;
		private readonly IClassificationType Orange;
		private readonly IClassificationType Yellow;
		private readonly IClassificationType Lime;
		private readonly IClassificationType Green;
		private readonly IClassificationType Turquoise;
		private readonly IClassificationType Cyan;
		private readonly IClassificationType Blue;
		private readonly IClassificationType Violet;
		private readonly IClassificationType Purple;
		private readonly IClassificationType Magenta;
		private readonly IClassificationType Rose;
		#pragma warning restore IDE0052 // Remove unread private members

		public struct Replacement
		{
			public string Name;
			public IClassificationType Color;
		}

		public struct Condition
		{
			public string Name;
			public List<string> CantBeClassifiedAs;
			public List<string> MustBeClassifiedAs;
			public List<string> MustBeClassifiedAsAnyOf;
		}

		public struct Colorize
		{
			public Regex Regex;
			public List<Condition> Conditions;
			public List<Replacement> Replacements;
		}

		public struct Colors
		{
			public string Name;
			public IClassificationType Color;
		}

		internal Classifier
		(
			IClassificationTypeRegistryService Registry,
			IClassifier                        Classifier
		)
		{
			IsClassificationRunning = false;
			IClassifier             = Classifier;

			White     = Registry.GetClassificationType("Color.White");
			Silver    = Registry.GetClassificationType("Color.Silver");
			Gray      = Registry.GetClassificationType("Color.Gray");
			Dark      = Registry.GetClassificationType("Color.Dark");
			Black     = Registry.GetClassificationType("Color.Black");
			Red       = Registry.GetClassificationType("Color.Red");
			Orange    = Registry.GetClassificationType("Color.Orange");
			Yellow    = Registry.GetClassificationType("Color.Yellow");
			Lime      = Registry.GetClassificationType("Color.Lime");
			Green     = Registry.GetClassificationType("Color.Green");
			Turquoise = Registry.GetClassificationType("Color.Turquoise");
			Cyan      = Registry.GetClassificationType("Color.Cyan");
			Blue      = Registry.GetClassificationType("Color.Blue");
			Violet    = Registry.GetClassificationType("Color.Violet");
			Purple    = Registry.GetClassificationType("Color.Purple");
			Magenta   = Registry.GetClassificationType("Color.Magenta");
			Rose      = Registry.GetClassificationType("Color.Rose");
		}

		public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan Span)
		{
			if (IsClassificationRunning) return new List<ClassificationSpan>();

			try
			{
				IsClassificationRunning = true;
				return Classify(Span);
			}
			finally
			{
				IsClassificationRunning = false;
			}
		}

		private IList<ClassificationSpan> Classify(SnapshotSpan Span)
		{
			var Spans = new List<ClassificationSpan>();

			if (Span.IsEmpty)
				return Spans;

			var Text = Span.GetText();

			#pragma warning disable IDE0028 // Simplify collection initialization
			List<Colorize> Colorizes = new List<Colorize>();
			#pragma warning restore IDE0028 // Simplify collection initialization

			#region Directives

			// #
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"^[ \f\t\v]*"
					+	@"(?<Hash>#)"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Hash", Color = Dark},
				},
			});

			var Directives = new List<string>()
			{
				@"define",
				@"elif",
				@"else",
				@"endif",
				@"error",
				@"if",
				@"ifdef",
				@"ifndef",
				@"include",
				@"line",
				@"pragma",
				@"undef",
			};

			// #directive
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+	@")"
					+	@"(?<Directive>"
					+		@"(?!"
					+			@"(?:" + string.Join(@"|", Directives.Select(Directive => Directive).ToArray()) + @")"
					+			Utils.IdentifierEndingBoundary
					+		")"
					+		Utils.Identifier
					+	@")"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Directive", Color = Gray},
				},
			});

			// #define
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+	@")"
					+	@"(?<Directive>define)"
					+	Utils.IdentifierEndingBoundary
					+	@"(?:"
					+		@"[ \f\t\v]+"
					+		@"(?<Identifier>" + Utils.Identifier + @")"
					+		@"(?:"
					+			@"(?<OpenParenthese>\()"
					+			@"[^)]*"
					+			@"(?<CloseParenthese>\))?"
					+		@")?"
					+		@"(?<Definition>.*?(?=//|$))?"
					+	@")?"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Directive", Color = Rose},
					new Replacement(){Name = "Identifier", Color = Purple},
					new Replacement(){Name = "OpenParenthese", Color = Gray},
					new Replacement(){Name = "CloseParenthese", Color = Gray},
					new Replacement(){Name = "Definition", Color = Silver},
				},
			});

			// #define(): parameter
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+		@"define[ \f\t\v]+"
					+		Utils.Identifier + @"[ \f\t\v]*"
					+		@"\([^)]*"
					+	@")"
					+	@"(?<Param>" + Utils.Identifier + @")"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Param", Color = Cyan},
				},
			});

			// #define(): comma
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+		@"define[ \f\t\v]+"
					+		Utils.Identifier + @"[ \f\t\v]*"
					+		@"\([^)]*"
					+	@")"
					+	@"(?<Comma>,)"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Comma", Color = Gray},
				},
			});

			// #if, #elif
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+	@")"
					+	@"(?<Directive>(?:el)?if)"
					+	Utils.IdentifierEndingBoundary
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Directive", Color = Violet},
				},
			});

			// #if, #elif: identifier
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+		@"(?:el)?if"
					+		Utils.IdentifierEndingBoundary
					+		@".*"
					+	@")"
					+	@"(?!(?:defined|__has_cpp_attribute|__has_include)" + Utils.IdentifierEndingBoundary + @")"
					+	@"(?<!__has_include[ \f\t\v]*\([^)]*)"
					+	@"(?<Identifier>" + Utils.Identifier + @")"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Identifier",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Identifier", Color = Purple},
				},
			});

			// defined
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<Command>defined)"
					+	Utils.IdentifierEndingBoundary
					+	@"(?:"
					+		@"(?<OpenParenthese>\()"
					+		@"(?:"
					+			@"(?<Parameter>" + Utils.Identifier + @")"
					+			@"(?<CloseParenthese>\))?"
					+		@")?"
					+	@")?"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Command",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){"preprocessor keyword"},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Command", Color = Blue},
					new Replacement(){Name = "Parameter", Color = Purple},
					new Replacement(){Name = "OpenParenthese", Color = Gray},
					new Replacement(){Name = "CloseParenthese", Color = Gray},
				},
			});

			// __has_cpp_attribute
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<Command>__has_cpp_attribute)"
					+	Utils.IdentifierEndingBoundary
					+	@"(?:"
					+		@"(?<OpenParenthese>\()"
					+		@"(?:"
					+			@"(?<Parameter>" + Utils.Identifier + @")"
					+			@"(?<CloseParenthese>\))?"
					+		@")?"
					+	@")?"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Command",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){"cppMacro"},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Command", Color = Blue},
					new Replacement(){Name = "Parameter", Color = Purple},
					new Replacement(){Name = "OpenParenthese", Color = Gray},
					new Replacement(){Name = "CloseParenthese", Color = Gray},
				},
			});

			// __has_include
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<Command>__has_include)"
					+	Utils.IdentifierEndingBoundary
					+	@"(?:"
					+		@"(?<OpenParenthese>\()"
					+		@"(?:"
					+			@"(?<OpenPath>[""<])"
					+			@"(?<Path>[^"">]*)"
					+			@"(?<ClosePath>["">])"
					+			@"(?<CloseParenthese>\))?"
					+		@")?"
					+	@")?"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Command",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){"cppMacro"},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Command", Color = Blue},
					new Replacement(){Name = "Path", Color = Green},
					new Replacement(){Name = "OpenPath", Color = Gray},
					new Replacement(){Name = "ClosePath", Color = Gray},
					new Replacement(){Name = "OpenParenthese", Color = Gray},
					new Replacement(){Name = "CloseParenthese", Color = Gray},
				},
			});

			// #ifdef, #ifndef
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+	@")"
					+	@"(?<Directive>ifn?def)"
					+	Utils.IdentifierEndingBoundary
					+	@"(?:"
					+		@"[ \f\t\v]+"
					+		@"(?<Identifier>" + Utils.Identifier + @")"
					+	@")?"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Directive", Color = Violet},
					new Replacement(){Name = "Identifier", Color = Purple},
				},
			});

			// #else, #endif
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+	@")"
					+	@"(?<Directive>else|endif)"
					+	Utils.IdentifierEndingBoundary
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Directive", Color = Violet},
				},
			});

			// #include
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+	@")"
					+	@"(?<Directive>include)"
					+	Utils.IdentifierEndingBoundary
					+	@"(?![ \f\t\v]+""Nade/Directives/Index/[^""]+\.h"")"
					+	@"(?:"
					+		@"[ \f\t\v]+"
					+		@"(?<OpenPath>[""<])"
					+		@"(?<Path>[^"">]*)"
					+		@"(?<ClosePath>["">])"
					+	@")?"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Directive", Color = Turquoise},
					new Replacement(){Name = "Path", Color = Green},
					new Replacement(){Name = "OpenPath", Color = Gray},
					new Replacement(){Name = "ClosePath", Color = Gray},
				},
			});

			// #include "Nade/Directives/Index/
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+	@")"
					+	@"(?<Directive>"
					+		@"include[ \f\t\v]+"
					+		@"""Nade/Directives/Index/[^""]+\.h"""
					+	@")"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Directive", Color = Dark},
				},
			});

			// #undef
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+	@")"
					+	@"(?<Directive>undef)"
					+	Utils.IdentifierEndingBoundary
					+	@"(?:"
					+		@"[ \f\t\v]+"
					+		@"(?<Identifier>" + Utils.Identifier + @")"
					+	@")?"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Directive", Color = Rose},
					new Replacement(){Name = "Identifier", Color = Purple},
				},
			});

			// #error
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+	@")"
					+	@"(?<Directive>error)"
					+	Utils.IdentifierEndingBoundary
					+	@"(?:"
					+		@"[ \f\t\v]+"
					+		@"(?<Message>(.*?(?=//|$)))"
					+	@")?"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Directive",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Directive", Color = Rose},
					new Replacement(){Name = "Message", Color = Green},
				},
			});

			// #line
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+	@")"
					+	@"(?<Directive>line)"
					+	Utils.IdentifierEndingBoundary
					+	@"(?:"
					+		@"[ \f\t\v]+"
					+		@"(?<Identifier>" + Utils.Identifier + @")"
					+	@")?"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Directive", Color = Gray},
					new Replacement(){Name = "Identifier", Color = Green},
				},
			});

			// #pragma
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+	@")"
					+	@"(?<Directive>pragma)"
					+	Utils.IdentifierEndingBoundary
					+	@"(?:"
					+		@"[ \f\t\v]+"
					+		@"(?<Identifier>" + Utils.Identifier + @")"
					+	@")?"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Directive", Color = Dark},
					new Replacement(){Name = "Identifier", Color = Gray},
				},
			});

			// #pragma: push_macro, pop_macro
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+		@"pragma[ \f\t\v]+"
					+	@")"
					+	@"(?<Pragma>(?:push|pop)_macro)"
					+	Utils.IdentifierEndingBoundary
					+	@"(?:"
					+		@"[ \f\t\v]*"
					+		@"(?<PunctLeft>\([ \f\t\v]*"")"
					+		@"(?:"
					+			@"(?<Macro>" + Utils.Identifier + @")"
					+			@"(?<PunctRight>""[ \f\t\v]*\))?"
					+		@")?"
					+	@")?"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Pragma", Color = Silver},
					new Replacement(){Name = "Macro", Color = Purple},
					new Replacement(){Name = "PunctLeft", Color = Gray},
					new Replacement(){Name = "PunctRight", Color = Gray},
				},
			});

			// #pragma region
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+	@")"
					+	@"(?<Pragma>pragma)"
					+	Utils.IdentifierEndingBoundary
					+	@"(?:"
					+		@"[ \f\t\v]+"
					+		@"(?<Region>region)"
					+		Utils.IdentifierEndingBoundary
					+	@")?"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Region",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Pragma", Color = Dark},
					new Replacement(){Name = "Region", Color = Gray},
				},
			});

			var PragmaOuterRegions = new List<Colors>()
			{
				new Colors(){Name = "Headers", Color = White},
				new Colors(){Name = "Meta", Color = White},

				new Colors(){Name = "Public", Color = Green},
				new Colors(){Name = "Protected", Color = Yellow},
				new Colors(){Name = "Private", Color = Red},

				new Colors(){Name = "Macros", Color = Purple},
				new Colors(){Name = "Friends", Color = Blue},
				new Colors(){Name = "Usings", Color = Blue},
				new Colors(){Name = "Components", Color = Turquoise},
				new Colors(){Name = "Enums", Color = Green},
				new Colors(){Name = "Enum structs", Color = Green},
				new Colors(){Name = "Concepts", Color = Lime},
				new Colors(){Name = "Structs", Color = Lime},
				new Colors(){Name = "Classes", Color = Lime},
				new Colors(){Name = "Stores", Color = Yellow},
				new Colors(){Name = "Members", Color = Yellow},
				new Colors(){Name = "Properties", Color = Orange},
				new Colors(){Name = "Fields", Color = Red},
				new Colors(){Name = "Delegates", Color = Magenta},
				new Colors(){Name = "Specials", Color = Lime},
				new Colors(){Name = "Constructors", Color = Lime},
				new Colors(){Name = "Operators", Color = Yellow},
				new Colors(){Name = "Conversions", Color = Yellow},
				new Colors(){Name = "Overrides", Color = Yellow},
				new Colors(){Name = "Methods", Color = Yellow},
				new Colors(){Name = "Getters", Color = Orange},
				new Colors(){Name = "Setters", Color = Orange},
				new Colors(){Name = "Functions", Color = Red},
				new Colors(){Name = "Events", Color = Magenta},
			};

			// #pragma region: PragmaOuterRegions
			foreach (var PragmaOuterRegion in PragmaOuterRegions)
				Colorizes.Add(new Colorize(){
					Regex = new Regex
					(
							@"(?<="
						+		@"^[ \f\t\v]*"
						+		@"#[ \f\t\v]*"
						+		@"pragma[ \f\t\v]+"
						+		@"region[ \f\t\v]+"
						+	@")"
						+	@"(?<Region>" + PragmaOuterRegion.Name + ")"
						+	Utils.IdentifierEndingBoundary
						,	RegexOptions.Compiled
					),
					Conditions = new List<Condition>(){},
					Replacements = new List<Replacement>(){
						new Replacement(){Name = "Region", Color = PragmaOuterRegion.Color},
					},
				});

			var PragmaInnerRegions = new List<Colors>()
			{
				new Colors(){Name = "macro", Color = Purple},
				new Colors(){Name = "friend", Color = Blue},
				new Colors(){Name = "using", Color = Blue},
				new Colors(){Name = "component", Color = Turquoise},
				new Colors(){Name = "enum", Color = Green},
				new Colors(){Name = "enum struct", Color = Green},
				new Colors(){Name = "concept", Color = Lime},
				new Colors(){Name = "struct", Color = Lime},
				new Colors(){Name = "class", Color = Lime},
				new Colors(){Name = "store", Color = Yellow},
				new Colors(){Name = "member", Color = Yellow},
				new Colors(){Name = "property", Color = Orange},
				new Colors(){Name = "field", Color = Red},
				new Colors(){Name = "delegate", Color = Magenta},
				new Colors(){Name = "special", Color = Lime},
				new Colors(){Name = "constructor", Color = Lime},
				new Colors(){Name = "operator", Color = Yellow},
				new Colors(){Name = "conversion", Color = Yellow},
				new Colors(){Name = "override", Color = Yellow},
				new Colors(){Name = "method", Color = Yellow},
				new Colors(){Name = "getter", Color = Orange},
				new Colors(){Name = "setter", Color = Orange},
				new Colors(){Name = "function", Color = Red},
				new Colors(){Name = "event", Color = Magenta},

				new Colors(){Name = "namespace", Color = Gray},
			};

			// #pragma region: PragmaInnerRegions
			foreach (var PragmaInnerRegion in PragmaInnerRegions)
				Colorizes.Add(new Colorize(){
					Regex = new Regex
					(
							@"(?<="
						+		@"^[ \f\t\v]*"
						+		@"#[ \f\t\v]*"
						+		@"pragma[ \f\t\v]+"
						+		@"region[ \f\t\v]+"
						+	@")"
						+	@"(?<Region>" + PragmaInnerRegion.Name + @")"
						+	Utils.IdentifierEndingBoundary
						+	@"(?:"
						+		@"[ \f\t\v]+"
						+		@"(?<Name>(.*?(?=//|$)))"
						+	@")?"
						,	RegexOptions.Compiled
					),
					Conditions = new List<Condition>(){},
					Replacements = new List<Replacement>(){
						new Replacement(){Name = "Region", Color = Blue},
						new Replacement(){Name = "Name", Color = PragmaInnerRegion.Color},
					},
				});

			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+		@"pragma[ \f\t\v]+"
					+		@"region[ \f\t\v]+"
					+	@")"
					+	@"(?!"
					+		@"(?:"
					+			string.Join(@"|", PragmaOuterRegions.Concat(PragmaInnerRegions).ToList().Select(Region => Region.Name).ToArray())
					+		@")"
					+		Utils.IdentifierEndingBoundary
					+	@")"
					+	@"(?<Region>.*?(?=//|$))"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Region", Color = Silver},
				},
			});

			#endregion
			#region Comments

			// comments
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"^[ \f\t\v]*"
					+	@"(?<Slashes>//+)"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Slashes",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Slashes", Color = Dark},
				},
			});

			// commented out code
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"//+"
					+	@")"
					+	@"(?<Code>\t.*)"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Slashes",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Code", Color = Dark},
				},
			});

			// comment directive: meta
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"//+ *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>(?:brief|details|note))"
					+	Utils.IdentifierEndingBoundary
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Slashes",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "BackSlash", Color = Dark},
					new Replacement(){Name = "Command", Color = Blue},
				},
			});

			// comment directive: control
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"//+ *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>(?:return|spare))"
					+	Utils.IdentifierEndingBoundary
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Slashes",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "BackSlash", Color = Dark},
					new Replacement(){Name = "Command", Color = Violet},
				},
			});

			// comment directive: external
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"//+ *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>see)"
					+	Utils.IdentifierEndingBoundary
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Slashes",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "BackSlash", Color = Dark},
					new Replacement(){Name = "Command", Color = Magenta},
				},
			});

			// comment directive: error
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"//+ *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>bug)"
					+	Utils.IdentifierEndingBoundary
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Slashes",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "BackSlash", Color = Dark},
					new Replacement(){Name = "Command", Color = Red},
				},
			});

			// comment directive: warning
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"//+ *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>todo)"
					+	Utils.IdentifierEndingBoundary
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Slashes",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "BackSlash", Color = Dark},
					new Replacement(){Name = "Command", Color = Orange},
				},
			});

			// comment directive: note
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"//+ *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>hack)"
					+	Utils.IdentifierEndingBoundary
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Slashes",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "BackSlash", Color = Dark},
					new Replacement(){Name = "Command", Color = Yellow},
				},
			});

			// comment directive: param
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"//+ *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>param) +"
					+	@"(?<Param>" + Utils.Identifier + @")"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Slashes",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "BackSlash", Color = Dark},
					new Replacement(){Name = "Command", Color = Blue},
					new Replacement(){Name = "Param", Color = Cyan},
				},
			});

			// comment directive: tparam
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"//+ *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>tparam) +"
					+	@"(?<TParam>" + Utils.Identifier + @")"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Command",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "BackSlash", Color = Dark},
					new Replacement(){Name = "Command", Color = Blue},
					new Replacement(){Name = "TParam", Color = Lime},
				},
			});

			// comment: quote
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"//+"
					+	@")"
					+	@"(?<Mark>\>)"
					+	@"(?<Quote>.*)"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Quote",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Mark", Color = Gray},
					new Replacement(){Name = "Quote", Color = Green},
				},
			});

			var References = new List<Colors>()
			{
				new Colors(){Name = @":",  Color = Red},       // Static
				new Colors(){Name = @",",  Color = Orange},    // Property
				new Colors(){Name = @"\.", Color = Yellow},    // Member
				new Colors(){Name = @"\^", Color = Lime},      // Type
				new Colors(){Name = @"#",  Color = Green},     // Literal
				new Colors(){Name = @"\*", Color = Turquoise}, // Import
				new Colors(){Name = @"\&", Color = Cyan},      // Parameter
				new Colors(){Name = @"~",  Color = Blue},      // Meta
				new Colors(){Name = @"\?", Color = Violet},    // Control
				new Colors(){Name = @"%",  Color = Purple},    // Macro
				new Colors(){Name = @"\!", Color = Magenta},   // Event
				new Colors(){Name = @"\|", Color = Rose},      // Export
				new Colors(){Name = @"@",  Color = White},     // Local
			};

			// comment: references
			foreach (var Reference in References)
				Colorizes.Add(new Colorize(){
					Regex = new Regex
					(
							@"(?<="
						+		@"^[ \f\t\v]*"
						+		@"//+ +[^`]*(?:`[^`]*`[^`]*)*"
						+	@")"
						+	@"(?<Mark>" + Reference.Name + @")"
						+	@"(?<Reference>" + Utils.Identifier + @")"
						,	RegexOptions.Compiled
					),
					Conditions = new List<Condition>(){
						new Condition(){
							Name = "Reference",
							CantBeClassifiedAs = new List<string>(){},
							MustBeClassifiedAs = new List<string>(){},
							MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
						}
					},
					Replacements = new List<Replacement>(){
						new Replacement(){Name = "Mark", Color = Gray},
						new Replacement(){Name = "Reference", Color = Reference.Color},
					},
				});

			// comment: `inline code`
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"//+ +[^`]*(?:`[^`]*`[^`]*)*"
					+	@")"
					+	@"(?<MarkLeft>`)"
					+	@"(?:"
					+		@"(?<Code>[^`]*)"
					+		@"(?<MarkRight>`)?"
					+	@")?"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Reference",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "MarkLeft", Color = Dark},
					new Replacement(){Name = "MarkRight", Color = Dark},
					new Replacement(){Name = "Code", Color = Gray},
				},
			});

			#endregion
			#region Tokens

			var Keywords = new List<string>()
			{
				@"\balignas\b",
				@"\balignof\b",
				@"\batomic_cancel\b",
				@"\batomic_commit\b",
				@"\batomic_noexcept\b",
				@"\bclass\b",
				@"\bconcept\b",
				@"\bconst\b",
				@"\bconsteval\b",
				@"\bconstexpr\b",
				@"\bconstinit\b",
				@"\bconst_cast\b",
				@"\bdecltype\b",
				@"\bdefault(?![ \f\t\v]*:)\b",
				@"\bdynamic_cast\b",
				@"\benum\b",
				@"\bexplicit\b",
				@"\bexport\b",
				@"\bextern\b",
				@"\bfinal\b",
				@"\bfriend\b",
				@"\bimport\b",
				@"\binline\b",
				@"\bmodule\b",
				@"\bmutable\b",
				@"\bnamespace\b",
				@"\bnoexcept\b",
				@"\boperator\b",
				@"\boverride\b",
				@"\breflexpr\b",
				@"\breinterpret_cast\b",
				@"\brequires\b",
				@"\bsizeof\b",
				@"\bstatic\b",
				@"\bstatic_assert\b",
				@"\bstatic_cast\b",
				@"\bstruct\b",
				@"\bsynchronized\b",
				@"\btemplate\b",
				@"\bthis\b",
				@"\bthread_local\b",
				@"\btransaction_safe\b",
				@"\btransaction_safe_dynamic\b",
				@"\btypedef\b",
				@"\btypeid\b",
				@"\btypename\b",
				@"\bunion\b",
				@"\busing\b",
				@"\bvirtual\b",
				@"\bvolatile\b",
			};

			// Keywords
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<Keyword>" + string.Join(@"|", Keywords.Select(Keyword => Keyword).ToArray()) + @")"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Keyword",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
						MustBeClassifiedAs = new List<string>(){"keyword"},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Keyword", Color = Blue},
				},
			});

			var Types = new List<string>()
			{
				@"\bauto\b",
				@"\bbool\b",
				@"\bchar\b",
				@"\bchar8_t\b",
				@"\bchar16_t\b",
				@"\bchar32_t\b",
				@"\bdouble\b",
				@"\bfloat\b",
				@"\bint\b",
				@"\blong\b",
				@"\bshort\b",
				@"\bsigned\b",
				@"\bunsigned\b",
				@"\bvoid\b",
				@"\bwchar_t\b",
			};

			// Types
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<Type>" + string.Join(@"|", Types.Select(Type => Type).ToArray()) + @")"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Type",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
						MustBeClassifiedAs = new List<string>(){"keyword"},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Type", Color = Lime},
				},
			});

			var Flows = new List<string>()
			{
				@"\bbreak\b",
				@"\bcase\b",
				@"\bcatch\b",
				@"\bcontinue\b",
				@"\bco_await\b",
				@"\bco_return\b",
				@"\bco_yield\b",
				@"\bdefault\b(?=[ \f\t\v]*:)",
				@"\bdo\b",
				@"\belse\b",
				@"\bfor\b",
				@"\bgoto\b",
				@"\bif\b",
				@"\breturn\b",
				@"\bswitch\b",
				@"\bthrow\b",
				@"\btry\b",
				@"\bwhile\b",
			};

			// Flows
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<Flow>" + string.Join(@"|", Flows.Select(Flow => Flow).ToArray()) + @")"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Flow",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
						MustBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAsAnyOf = new List<string>(){"keyword", "cppControlKeyword"},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Flow", Color = Violet},
				},
			});

			var Statics = new List<string>()
			{
				@"\bfalse\b",
				@"\bnullptr\b",
				@"\btrue\b",
			};

			// Statics
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<Static>" + string.Join(@"|", Statics.Select(Static => Static).ToArray()) + @")"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Static",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
						MustBeClassifiedAs = new List<string>(){"keyword"},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Static", Color = Green},
				},
			});

			var Importants = new List<string>()
			{
				@"\bdelete\b",
				@"\bnew\b",
			};

			// Importants
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<Important>" + string.Join(@"|", Importants.Select(Important => Important).ToArray()) + @")"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Important",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
						MustBeClassifiedAs = new List<string>(){"keyword"},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Important", Color = Rose},
				},
			});

			// private
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<Access>\bprivate\b)"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Access",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
						MustBeClassifiedAs = new List<string>(){"keyword"},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Access", Color = Red},
				},
			});

			// protected
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<Access>\bprotected\b)"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Access",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
						MustBeClassifiedAs = new List<string>(){"keyword"},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Access", Color = Yellow},
				},
			});

			// public
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<Access>\bpublic\b)"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Access",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
						MustBeClassifiedAs = new List<string>(){"keyword"},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Access", Color = Green},
				},
			});

			#endregion
			#region Misc

			// Local variables that begins with `$`.
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						Utils.IdentifierBeginningBoundary
					+	@"(?<Prefix>\$)"
					,	RegexOptions.Compiled
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Prefix",
						CantBeClassifiedAs = new List<string>(){},
						MustBeClassifiedAs = new List<string>(){"cppLocalVariable"},
						MustBeClassifiedAsAnyOf = new List<string>(){},
					}
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Prefix", Color = Gray},
				},
			});

			#endregion

			foreach (Colorize Colorize in Colorizes)
				foreach (Match Match in Colorize.Regex.Matches(Text))
				{
					foreach (var Condition in Colorize.Conditions)
					{
						var Start = Span.Start + Match.Groups[Condition.Name].Index;
						var Length = Match.Groups[Condition.Name].Length;
						var ConditionSpan = new Span(Start, Length);
						var ConditionSnapshotSpan = new SnapshotSpan(Span.Snapshot, ConditionSpan);
						var Intersections = IClassifier.GetClassificationSpans(ConditionSnapshotSpan);

						foreach (var Intersection in Intersections)
						{
							var Classifications = Intersection.ClassificationType.Classification.Split(new[]{" - "}, StringSplitOptions.None);

							if (Condition.MustBeClassifiedAsAnyOf.Count > 0)
								if (!Utils.IsClassifiedAs(Classifications, Condition.MustBeClassifiedAsAnyOf.ToArray()))
									goto SkipMatch;
							
							if (Condition.CantBeClassifiedAs.Count > 0)
								if (Utils.IsClassifiedAs(Classifications, Condition.CantBeClassifiedAs.ToArray()))
									goto SkipMatch;
					
							foreach (string Classification in Condition.MustBeClassifiedAs)
								if (!Utils.IsClassifiedAs(Classifications, new[]{Classification}))
									goto SkipMatch;
						}
					}
				
					foreach (var Replacement in Colorize.Replacements)
					{
						var Start = Span.Start + Match.Groups[Replacement.Name].Index;
						var Length = Match.Groups[Replacement.Name].Length;
						var TextSpan = new Span(Start, Length);
						var SnapshotSpan = new SnapshotSpan(Span.Snapshot, TextSpan);
						var ClassificationSpan = new ClassificationSpan(SnapshotSpan, Replacement.Color);

						Spans.Add(ClassificationSpan);
					}

					SkipMatch:;
				}

			return Spans;
		}
	}
}