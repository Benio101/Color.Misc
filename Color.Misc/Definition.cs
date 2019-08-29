using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Color.Misc
{
	internal static class Definitions
	{
		// > The field is never used
		// Reason The field is used by MEF.
		#pragma warning disable 169
		#pragma warning disable IDE0051

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("cppLocalVariable.Dollar")]
		private static readonly ClassificationTypeDefinition
		Definition_cppLocalVariable_Dollar;

		#pragma warning restore IDE0051
		#pragma warning restore 169
	}
}