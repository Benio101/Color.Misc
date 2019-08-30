using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace Color.Misc
{
	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "cppLocalVariable.Dollar")]
	[Name("cppLocalVariable.Dollar")]
	[BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
	[UserVisible(true)]
	[Order(After = PredefinedClassificationTypeNames.Identifier)]
	[Order(After = "cppLocalVariable")]
	[Order(After = Priority.High)]
	internal sealed class Format_cppLocalVariable_Dollar
	:
		ClassificationFormatDefinition
	{
		public Format_cppLocalVariable_Dollar()
		{
			DisplayName = "C++ Local Variable: \"$\"";

			BackgroundCustomizable = false;
			ForegroundColor = Default.Colors.CommentPunct;
		}
	}
}