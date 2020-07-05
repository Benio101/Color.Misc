using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#nullable enable
namespace Color.Misc
{
	internal class Classifier
	:
		IClassifier
	{
		private          bool        IsClassificationRunning;
		private readonly IClassifier IClassifier;

		#pragma warning disable 67
		public event EventHandler<ClassificationChangedEventArgs>? ClassificationChanged;
		#pragma warning restore 67
		
		#pragma warning disable IDE0052 // Remove unread private members
		private readonly IClassificationType White;
		private readonly IClassificationType Silver;
		private readonly IClassificationType Gray;
		private readonly IClassificationType Dark;
		private readonly IClassificationType Black;
		private readonly IClassificationType Red;
		private readonly IClassificationType Red_dark;
		private readonly IClassificationType Orange;
		private readonly IClassificationType Orange_dark;
		private readonly IClassificationType Yellow;
		private readonly IClassificationType Yellow_dark;
		private readonly IClassificationType Lime;
		private readonly IClassificationType Lime_dark;
		private readonly IClassificationType Green;
		private readonly IClassificationType Green_dark;
		private readonly IClassificationType Turquoise;
		private readonly IClassificationType Turquoise_dark;
		private readonly IClassificationType Cyan;
		private readonly IClassificationType Cyan_dark;
		private readonly IClassificationType Blue;
		private readonly IClassificationType Blue_dark;
		private readonly IClassificationType Violet;
		private readonly IClassificationType Violet_dark;
		private readonly IClassificationType Purple;
		private readonly IClassificationType Purple_dark;
		private readonly IClassificationType Magenta;
		private readonly IClassificationType Magenta_dark;
		private readonly IClassificationType Rose;
		private readonly IClassificationType Rose_dark;
		#pragma warning restore IDE0052 // Remove unread private members

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

		internal Classifier
		(
			IClassificationTypeRegistryService Registry,
			IClassifier                        Classifier
		)
		{
			IsClassificationRunning = false;
			IClassifier             = Classifier;

			White          = Registry.GetClassificationType("Color.White");
			Silver         = Registry.GetClassificationType("Color.Silver");
			Gray           = Registry.GetClassificationType("Color.Gray");
			Dark           = Registry.GetClassificationType("Color.Dark");
			Black          = Registry.GetClassificationType("Color.Black");
			Red            = Registry.GetClassificationType("Color.Red");
			Red_dark       = Registry.GetClassificationType("Color.Red.Dark");
			Orange         = Registry.GetClassificationType("Color.Orange");
			Orange_dark    = Registry.GetClassificationType("Color.Orange.Dark");
			Yellow         = Registry.GetClassificationType("Color.Yellow");
			Yellow_dark    = Registry.GetClassificationType("Color.Yellow.Dark");
			Lime           = Registry.GetClassificationType("Color.Lime");
			Lime_dark      = Registry.GetClassificationType("Color.Lime.Dark");
			Green          = Registry.GetClassificationType("Color.Green");
			Green_dark     = Registry.GetClassificationType("Color.Green.Dark");
			Turquoise      = Registry.GetClassificationType("Color.Turquoise");
			Turquoise_dark = Registry.GetClassificationType("Color.Turquoise.Dark");
			Cyan           = Registry.GetClassificationType("Color.Cyan");
			Cyan_dark      = Registry.GetClassificationType("Color.Cyan.Dark");
			Blue           = Registry.GetClassificationType("Color.Blue");
			Blue_dark      = Registry.GetClassificationType("Color.Blue.Dark");
			Violet         = Registry.GetClassificationType("Color.Violet");
			Violet_dark    = Registry.GetClassificationType("Color.Violet.Dark");
			Purple         = Registry.GetClassificationType("Color.Purple");
			Purple_dark    = Registry.GetClassificationType("Color.Purple.Dark");
			Magenta        = Registry.GetClassificationType("Color.Magenta");
			Magenta_dark   = Registry.GetClassificationType("Color.Magenta.Dark");
			Rose           = Registry.GetClassificationType("Color.Rose");
			Rose_dark      = Registry.GetClassificationType("Color.Rose.Dark");
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
						@"(?<="
					+		@"^[ \f\t\v]*"
					+	@")"
					+	@"(?<Hash>#)"
				),
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
				),
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
				),
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
				),
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
				),
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
				),
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
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Identifier",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
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
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Command",
						MustBeClassifiedAs = new List<string>(){"preprocessor keyword"},
					},
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
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Command",
						MustBeClassifiedAs = new List<string>(){"cppMacro"},
					},
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
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Command",
						MustBeClassifiedAs = new List<string>(){"cppMacro"},
					},
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
				),
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
				),
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
				),
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
				),
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
				),
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
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Directive",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
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
				),
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Directive", Color = Gray},
					new Replacement(){Name = "Identifier", Color = Green},
				},
			});

			var Pragmas = new List<string>()
			{
				@"push_macro",
				@"pop_macro",
				@"region",
				@"endregion",
			};

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
					+		@"(?!"
					+			@"(?:" + string.Join(@"|", Pragmas.Select(Pragma => Pragma).ToArray()) + @")"
					+			Utils.IdentifierEndingBoundary
					+		")"
					+		@"(?<Pragma>" + Utils.Identifier + @")"
					+	@")?"
				),
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Directive", Color = Dark},
					new Replacement(){Name = "Pragma", Color = Gray},
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
				),
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Pragma", Color = Gray},
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
					+		@"pragma[ \f\t\v]+"
					+	@")"
					+	@"(?<Pragma>region)"
					+	Utils.IdentifierEndingBoundary
				),
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Pragma", Color = Gray},
				},
			});

			var PragmaOuterRegions = new List<Colors>()
			{
				new Colors(){Name = "Headers", Color = White},
				new Colors(){Name = "Meta", Color = White},

				new Colors(){Name = "Public", Color = Green},
				new Colors(){Name = "Protected", Color = Yellow},
				new Colors(){Name = "Private", Color = Red},
				
				new Colors(){Name = "(?:(?:Object|Function)-like macros|Macros)", Color = Purple},
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
					),
					Replacements = new List<Replacement>(){
						new Replacement(){Name = "Region", Color = PragmaOuterRegion.Color},
					},
				});

			var PragmaInnerRegions = new List<Colors>()
			{
				new Colors(){Name = "(?:(?:object|function)-like )?macro", Color = Purple},
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
					),
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
				),
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Region", Color = Silver},
				},
			});

			// #pragma endregion
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"^[ \f\t\v]*"
					+		@"#[ \f\t\v]*"
					+		@"pragma[ \f\t\v]+"
					+	@")"
					+	@"(?<Pragma>endregion)"
					+	Utils.IdentifierEndingBoundary
				),
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Pragma", Color = Dark},
				},
			});

			#endregion
			#region Comments

			// comments
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="
					+		@"(?<NotComment>^|.)"
					+	@")"
					+	@"(?<Slashes>//+)"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "NotComment",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
					new Condition(){
						Name = "Slashes",
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					},
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
					+		@"(?<NotComment>^|.)"
					+		@"(?<Slashes>//+)"
					+		@"\t"
					+	@")"
					+	@"(?<Code>.*)"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "NotComment",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
					new Condition(){
						Name = "Slashes",
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					},
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
					+		@"(?<NotComment>^|.)"
					+		@"(?<Slashes>//+)"
					+		@" *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>brief|details|note)"
					+	Utils.IdentifierEndingBoundary
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "NotComment",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
					new Condition(){
						Name = "Slashes",
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					},
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
					+		@"(?<NotComment>^|.)"
					+		@"(?<Slashes>//+)"
					+		@" *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>return|spare)"
					+	Utils.IdentifierEndingBoundary
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "NotComment",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
					new Condition(){
						Name = "Slashes",
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					},
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
					+		@"(?<NotComment>^|.)"
					+		@"(?<Slashes>//+)"
					+		@" *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>see)"
					+	Utils.IdentifierEndingBoundary
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "NotComment",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
					new Condition(){
						Name = "Slashes",
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					},
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
					+		@"(?<NotComment>^|.)"
					+		@"(?<Slashes>//+)"
					+		@" *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>bug)"
					+	Utils.IdentifierEndingBoundary
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "NotComment",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
					new Condition(){
						Name = "Slashes",
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					},
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
					+		@"(?<NotComment>^|.)"
					+		@"(?<Slashes>//+)"
					+		@" *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>todo)"
					+	Utils.IdentifierEndingBoundary
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "NotComment",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
					new Condition(){
						Name = "Slashes",
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					},
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
					+		@"(?<NotComment>^|.)"
					+		@"(?<Slashes>//+)"
					+		@" *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>hack)"
					+	Utils.IdentifierEndingBoundary
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "NotComment",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
					new Condition(){
						Name = "Slashes",
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					},
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
					+		@"(?<NotComment>^|.)"
					+		@"(?<Slashes>//+)"
					+		@" *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>param)"
					+	Utils.IdentifierEndingBoundary
					+	@"(?:"
					+		@" +"
					+		@"(?<Param>" + Utils.Identifier + @")"
					+	@")?"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "NotComment",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
					new Condition(){
						Name = "Slashes",
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					},
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
					+		@"(?<NotComment>^|.)"
					+		@"(?<Slashes>//+)"
					+		@" *"
					+	@")"
					+	@"(?<BackSlash>\\)"
					+	@"(?<Command>tparam)"
					+	Utils.IdentifierEndingBoundary
					+	@"(?:"
					+		@" +"
					+		@"(?<TParam>" + Utils.Identifier + @")"
					+	@")?"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "NotComment",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
					new Condition(){
						Name = "Command",
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					},
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
					+		@"(?<NotComment>^|.)"
					+		@"(?<Slashes>//+)"
					+		@" *"
					+	@")"
					+	@"(?<Mark>\>)"
					+	@"(?<Quote>.*)"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "NotComment",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
					new Condition(){
						Name = "Quote",
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					},
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Mark", Color = Dark},
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
						@"(?<="                       // Ensure `Slashes` is beginning of comment ∵
					+		@"(?<NotComment>^|.)"     // - there's nothing before `Slashes` xor a character before `Slashes` is not a comment.
					+		@"(?<Slashes>//+)"        // - `Slashes` is a comment.
					+		@" +"                     // Ensure comment is not a disabled code ∵ it does not begins with a tab.
					+		@"(?!>)"                  // Ensure comment is not a quote ∵ it does not begins with a closing chevron.
					+		@"[^`]*(?:`[^`]*`[^`]*)*" // Make sure reference is not inside an inline code ∵ it's not preceded by odd number of backticks.
					+	@")"
					+	Utils.IdentifierBeginningBoundary
					+	@"(?<Mark>" + Reference.Name + @")"
					+	@"(?<Reference>" + Utils.Identifier + @")"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "NotComment",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
					new Condition(){
						Name = "Slashes",
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					},
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Mark", Color = Dark},
					new Replacement(){Name = "Reference", Color = Reference.Color},
				},
			});

			// Comment: inline code
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<="                       // Ensure `Slashes` is beginning of comment ∵
					+		@"(?<NotComment>^|.)"     // - there's nothing before `Slashes` xor a character before `Slashes` is not a comment.
					+		@"(?<Slashes>//+)"        // - `Slashes` is a comment.
					+		@" +"                     // Ensure comment is not a disabled code ∵ it does not begins with a tab.
					+		@"(?!>)"                  // Ensure comment is not a quote ∵ it does not begins with a closing chevron.
					+		@"[^`]*(?:`[^`]*`[^`]*)*" // Make sure reference is not inside an inline code ∵ it's not preceded by odd number of backticks.
					+	@")"
					+	@"(?<OpeningBacktick>`)"
					+	@"(?:"
					+		@"(?<InlineCode>[^`]*)"
					+		@"(?<ClosingBacktick>`)?"
					+	@")?"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "NotComment",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment"},
					},
					new Condition(){
						Name = "Slashes",
						MustBeClassifiedAsAnyOf = new List<string>(){"comment", "XML Doc Comment"},
					},
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "OpeningBacktick", Color = Dark},
					new Replacement(){Name = "ClosingBacktick", Color = Dark},
					new Replacement(){Name = "InlineCode", Color = Silver},
				},
			});

			#endregion
			#region Attributes

			var Attributes = new List<Colors>()
			{
				new Colors(){Name = @"carries_dependency",        Color = Blue},
				new Colors(){Name = @"fallthrough",               Color = Violet},
				new Colors(){Name = @"likely",                    Color = Green},
				new Colors(){Name = @"maybe_unused",              Color = Yellow},
				new Colors(){Name = @"no_unique_address",         Color = Blue},
				new Colors(){Name = @"nodiscard",                 Color = Blue},
				new Colors(){Name = @"noreturn",                  Color = Rose},
				new Colors(){Name = @"optimize_for_synchronized", Color = Blue},
				new Colors(){Name = @"unlikely",                  Color = Red},
			};

			// Attributes
			foreach (var Attribute in Attributes)
				Colorizes.Add(new Colorize(){
					Regex = new Regex
					(
							@"(?<OpenBrackets>\[\[)"
						+	@"(?<Attribute>" + Attribute.Name + @")"
						+	@"(?<CloseBrackets>\]\])"
					),
					Conditions = new List<Condition>(){
						new Condition(){
							Name = "OpenBrackets",
							MustBeClassifiedAs = new List<string>(){"operator"},
						},
						new Condition(){
							Name = "CloseBrackets",
							MustBeClassifiedAs = new List<string>(){"operator"},
						},
					},
					Replacements = new List<Replacement>(){
						new Replacement(){Name = "OpenBrackets",  Color = Gray},
						new Replacement(){Name = "Attribute",     Color = Attribute.Color},
						new Replacement(){Name = "CloseBrackets", Color = Gray},
					},
				});

			// Attribute: deprecated
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
						@"(?<OpenBrackets>\[\[)"
					+	@"(?<Attribute>deprecated)"
					+	@"(?:"
					+		@"(?<OpenParens>\()"
					+		@"(?<OpenQuote>"")"
					+		@"(?<String>[^""]*)"
					+		@"(?<CloseQuote>"")"
					+		@"(?<CloseParens>\))"
					+	@")?"
					+	@"(?<CloseBrackets>\]\])"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "OpenBrackets",
						MustBeClassifiedAs = new List<string>(){"operator"},
					},
					new Condition(){
						Name = "CloseBrackets",
						MustBeClassifiedAs = new List<string>(){"operator"},
					},
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "OpenBrackets",  Color = Gray},
					new Replacement(){Name = "CloseBrackets", Color = Gray},
					new Replacement(){Name = "Attribute",     Color = Rose},
					new Replacement(){Name = "OpenParens",    Color = Gray},
					new Replacement(){Name = "CloseParens",   Color = Gray},
					new Replacement(){Name = "OpenQuote",     Color = Gray},
					new Replacement(){Name = "CloseQuote",    Color = Gray},
					new Replacement(){Name = "String",        Color = Green},
				},
			});

			#endregion
			#region Tokens

			var Keywords = new List<string>()
			{
				Utils.IdentifierBeginningBoundary + @"alignas"                  + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"alignof"                  + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"atomic_cancel"            + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"atomic_commit"            + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"atomic_noexcept"          + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"class"                    + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"concept"                  + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"const"                    + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"consteval"                + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"constexpr"                + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"constinit"                + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"const_cast"               + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"decltype"                 + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"default(?![ \f\t\v]*:)"   + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"dynamic_cast"             + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"enum"                     + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"explicit"                 + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"export"                   + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"extern"                   + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"final"                    + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"friend"                   + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"import"                   + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"inline"                   + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"module"                   + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"mutable"                  + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"namespace"                + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"noexcept"                 + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"operator"                 + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"override"                 + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"reflexpr"                 + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"reinterpret_cast"         + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"requires"                 + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"sizeof"                   + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"static"                   + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"static_assert"            + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"static_cast"              + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"struct"                   + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"synchronized"             + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"template"                 + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"this"                     + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"thread_local"             + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"transaction_safe"         + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"transaction_safe_dynamic" + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"typedef"                  + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"typeid"                   + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"typename"                 + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"union"                    + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"using"                    + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"virtual"                  + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"volatile"                 + Utils.IdentifierEndingBoundary,
			};

			// Keywords
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
					@"(?<Keyword>" + string.Join(@"|", Keywords.Select(Keyword => Keyword).ToArray()) + @")"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Keyword",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
					},
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Keyword", Color = Blue},
				},
			});

			var Types = new List<string>()
			{
				Utils.IdentifierBeginningBoundary + @"auto"     + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"bool"     + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"char"     + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"char8_t"  + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"char16_t" + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"char32_t" + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"double"   + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"float"    + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"int"      + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"long"     + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"short"    + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"signed"   + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"unsigned" + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"void"     + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"wchar_t"  + Utils.IdentifierEndingBoundary,
			};

			// Types
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
					@"(?<Type>" + string.Join(@"|", Types.Select(Type => Type).ToArray()) + @")"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Type",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
						MustBeClassifiedAs = new List<string>(){"keyword"},
					},
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Type", Color = Lime},
				},
			});

			var Flows = new List<string>()
			{
				Utils.IdentifierBeginningBoundary + @"break"     + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"case"      + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"catch"     + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"continue"  + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"co_await"  + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"co_return" + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"co_yield"  + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"default"   + @"(?=[ \f\t\v]*:)",
				Utils.IdentifierBeginningBoundary + @"do"        + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"else"      + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"for"       + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"goto"      + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"if"        + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"return"    + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"switch"    + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"throw"     + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"try"       + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"while"     + Utils.IdentifierEndingBoundary,
			};

			// Flows
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
					@"(?<Flow>" + string.Join(@"|", Flows.Select(Flow => Flow).ToArray()) + @")"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Flow",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
						MustBeClassifiedAsAnyOf = new List<string>(){"keyword", "cppControlKeyword"},
					},
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Flow", Color = Violet},
				},
			});

			var Statics = new List<string>()
			{
				Utils.IdentifierBeginningBoundary + @"false"   + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"nullptr" + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"true"    + Utils.IdentifierEndingBoundary,
			};

			// Statics
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
					@"(?<Static>" + string.Join(@"|", Statics.Select(Static => Static).ToArray()) + @")"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Static",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
						MustBeClassifiedAs = new List<string>(){"keyword"},
					},
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Static", Color = Green},
				},
			});

			var Importants = new List<string>()
			{
				Utils.IdentifierBeginningBoundary + @"delete" + Utils.IdentifierEndingBoundary,
				Utils.IdentifierBeginningBoundary + @"new"    + Utils.IdentifierEndingBoundary,
			};

			// Importants
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
					@"(?<Important>" + string.Join(@"|", Importants.Select(Important => Important).ToArray()) + @")"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Important",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
						MustBeClassifiedAs = new List<string>(){"keyword"},
					},
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Important", Color = Rose},
				},
			});

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

			// protected
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
					@"(?<Access>" + Utils.IdentifierBeginningBoundary + @"protected" + Utils.IdentifierEndingBoundary + @")"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Access",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
						MustBeClassifiedAs = new List<string>(){"keyword"},
					},
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Access", Color = Yellow},
				},
			});

			// public
			Colorizes.Add(new Colorize(){
				Regex = new Regex
				(
					@"(?<Access>" + Utils.IdentifierBeginningBoundary + @"public" + Utils.IdentifierEndingBoundary + @")"
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Access",
						CantBeClassifiedAs = new List<string>(){"comment", "XML Doc Comment", "string"},
						MustBeClassifiedAs = new List<string>(){"keyword"},
					},
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
				),
				Conditions = new List<Condition>(){
					new Condition(){
						Name = "Prefix",
						MustBeClassifiedAs = new List<string>(){"cppLocalVariable"},
					},
				},
				Replacements = new List<Replacement>(){
					new Replacement(){Name = "Prefix", Color = Gray},
				},
			});

			#endregion

			foreach (Colorize Colorize in Colorizes)
				foreach (Match Match in Colorize.Regex.Matches(Text))
				{
					if (Colorize.Conditions != null)
						foreach (var Condition in Colorize.Conditions)
						{
							var Start = Span.Start + Match.Groups[Condition.Name].Index;
							var Length = Match.Groups[Condition.Name].Length;

							if (Length == 0)
								continue;

							var ConditionSpan = new Span(Start, Length);
							var ConditionSnapshotSpan = new SnapshotSpan(Span.Snapshot, ConditionSpan);
							var Intersections = IClassifier.GetClassificationSpans(ConditionSnapshotSpan);

							foreach (var Intersection in Intersections)
							{
								var Classifications = Intersection.ClassificationType.Classification.Split(new[]{" - "}, StringSplitOptions.None);

								if (Condition.MustBeClassifiedAsAnyOf != null)
									if (Condition.MustBeClassifiedAsAnyOf.Count > 0)
										if (!Utils.IsClassifiedAs(Classifications, Condition.MustBeClassifiedAsAnyOf.ToArray()))
											goto SkipMatch;
							
								if (Condition.CantBeClassifiedAs != null)
									if (Condition.CantBeClassifiedAs.Count > 0)
										if (Utils.IsClassifiedAs(Classifications, Condition.CantBeClassifiedAs.ToArray()))
											goto SkipMatch;
					
								if (Condition.MustBeClassifiedAs != null)
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

#nullable disable