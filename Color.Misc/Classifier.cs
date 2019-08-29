using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Color.Misc
{
	internal class Classifier
		: IClassifier
	{
		private bool IsClassificationRunning;
		private readonly IClassifier IClassifier;

		#pragma warning disable 67
		public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
		#pragma warning restore 67
		
		private readonly IClassificationType cppLocalVariable_Dollar;

		internal Classifier(
			IClassificationTypeRegistryService Registry,
			IClassifier Classifier
		){
			IsClassificationRunning = false;
			IClassifier = Classifier;

			cppLocalVariable_Dollar = Registry.GetClassificationType("cppLocalVariable.Dollar");
		}

		public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan Span){
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

		private IList<ClassificationSpan> Classify(SnapshotSpan Span){
			IList<ClassificationSpan> Spans = new List<ClassificationSpan>();

			if (Span.IsEmpty) return Spans;
			var Text = Span.GetText();

			// $ sign.
			foreach (Match Match in new Regex(
				@"(?<Dollar>\$)?" + Utils.Identifier
			).Matches(Text))
			{
				var MatchedSpan = new SnapshotSpan(Span.Snapshot, new Span(
					Span.Start + Match.Groups["Dollar"].Index,
					Match.Groups["Dollar"].Length
				));

				var Intersections = IClassifier.GetClassificationSpans(MatchedSpan);
				foreach (var Intersection in Intersections){
					var Classifications = Intersection.ClassificationType.Classification.Split(
						new[]{" - "}, StringSplitOptions.None
					);

					// Comment must be classified as "cppLocalVariable".
					if (!Utils.IsClassifiedAs(Classifications, new[]{
						"cppLocalVariable"
					})){
						goto SkipVariable;
					}

					// Prevent matching attributes.
					if (Utils.IsClassifiedAs(Classifications, new[]{
						"Attribute"
					})){
						goto SkipVariable;
					}

					Spans.Add(new ClassificationSpan(new SnapshotSpan(
						Span.Snapshot, new Span(
							Span.Start + Match.Groups["Dollar"].Index,
							Match.Groups["Dollar"].Length
						)), cppLocalVariable_Dollar
					));
				}

				SkipVariable:;
			}

			return Spans;
		}
	}
}