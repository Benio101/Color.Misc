using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace Color.Misc
{
	[
		ContentType("C/C++"),
		Export(typeof(ITextViewCreationListener)),
		TextViewRole(PredefinedTextViewRoles.Editable)
	]

	internal class Refactor
	:
		ITextViewCreationListener
	{
		private ITextView TextView;
		private bool IsTextChanging;
		private static IClassifier Classifier;

		#pragma warning disable 649

		[Import]
		private readonly IClassifierAggregatorService ClassifierAggregatorService;

		#pragma warning restore 649

		public void TextViewCreated(ITextView TextView)
		{
			this.TextView = TextView;

			TextView.TextBuffer.Changed     += OnTextBufferChanged;
			TextView.TextBuffer.PostChanged += PostTextBufferChanged;

			Classifier = ClassifierAggregatorService.GetClassifier(this.TextView.TextBuffer);

			ThreadHelper.ThrowIfNotOnUIThread();
		}

		private void OnTextBufferChanged
		(
			object                      Sender,
			TextContentChangedEventArgs Event
		)
		{
			if (IsTextChanging) return;
			if (Event.Changes == null) return;

			ThreadHelper.ThrowIfNotOnUIThread();
			IsTextChanging = true;

			foreach (var Change in Event.Changes)
			{
				if
				(
						Change.OldLength == 0
					&&	Change.NewText   == "꞉"
				)
				{
					using (var Edit = Event.After.TextBuffer.CreateEdit())
					{
						Edit.Replace(new Span(Change.NewPosition, Change.NewText.Length), "");
						Edit.Apply();
					}

					var Line          = Event.After.GetLineFromPosition(Change.NewPosition);
					var LineNumber    = Event.After.GetLineNumberFromPosition(Change.NewPosition);
					var Intersections = Classifier.GetClassificationSpans(new SnapshotSpan(Line.Start, Line.End));

					foreach (var Intersection in Intersections)
					{
						Debug.Print(LineNumber + ": \"" + Intersection.Span.GetText() + "\": " + Intersection.ClassificationType.Classification);
					}
				}
			}
		}

		private void PostTextBufferChanged(object Sender, EventArgs Event)
		{
			IsTextChanging = false;
		}
	}
}