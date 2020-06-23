using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace Color.Misc
{
	internal static class Definitions
	{
		#pragma warning disable 169
		#pragma warning disable IDE0051

		// > The field is never used
		// Reason: The field is used by MEF.

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.White")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_White;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Silver")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Silver;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Gray")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Gray;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Dark")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Dark;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Black")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Black;


		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Red")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Red;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Orange")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Orange;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Yellow")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Yellow;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Lime")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Lime;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Green")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Green;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Turquoise")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Turquoise;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Cyan")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Cyan;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Blue")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Blue;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Violet")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Violet;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Purple")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Purple;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Magenta")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Magenta;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("Color.Rose")]
		private static readonly ClassificationTypeDefinition
		Definition_Color_Rose;

		#pragma warning restore IDE0051
		#pragma warning restore 169
	}
}