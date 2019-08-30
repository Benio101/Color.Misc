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
		[Name("cppLocalVariable.Dollar")]
		private static readonly ClassificationTypeDefinition
		Definition_cppLocalVariable_Dollar;

		#pragma warning restore IDE0051
		#pragma warning restore 169
	}
}