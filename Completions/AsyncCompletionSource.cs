using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DRYCodeGen.Completions
{
    internal class WriteItem
    {
        public int Number { get; }
        public string Name { get; }
        public string Value { get; }

        internal WriteItem(int number, string name, string value)
        {
            Number = number;
            Name = name;
            Value = value;
        }
    }
    internal class AsyncCompletionSource : IAsyncCompletionSource
    {
        private ITextStructureNavigatorSelectorService structureNavigatorSelectorService { get; }

        private readonly List<VCClass> classes = null;

        public AsyncCompletionSource(ITextStructureNavigatorSelectorService structureNavigatorSelectorService, List<VCClass> classes)
        {
            this.structureNavigatorSelectorService = structureNavigatorSelectorService;
            this.classes = classes;
        }

        private CompletionItem MakeItem(WriteItem element)
        {
            ImmutableArray<CompletionFilter> filters = ImmutableArray.Create(
                new CompletionFilter("Test", "T", null)
                );

            var item = new CompletionItem(
                displayText: element.Name,
                source: this,
                icon: null,
                filters: filters,
                suffix:"T",
                insertText:element.Value,
                sortText:"T",
                filterText:$"{element.Name}",
                attributeIcons:ImmutableArray<ImageElement>.Empty
                ) ;
            item.Properties.AddProperty(nameof(WriteItem), element);
            return item ;
        }


        public Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            try
            {
                //只作用于C/C++内容
                var contentType = session.TextView.TextBuffer.ContentType.TypeName;
                if(contentType !="C/C++")
                {
                    return Task.FromResult<CompletionContext>(null);
                }

                var lineStart = triggerLocation.GetContainingLine().Start;
                var span = new SnapshotSpan(lineStart, triggerLocation);
                var text = triggerLocation.Snapshot.GetText(span);
                int lineNumber = triggerLocation.GetContainingLineNumber();
                if(classes != null && text.EndsWith("__"))
                {
                    var prefix = text.Replace("__", "");
                    List<WriteItem> items = new List<WriteItem>();
                    foreach( var e in classes)
                    {
                        if (e == null) continue;
                        if(e.StartLine <= lineNumber && e.EndLine >= lineNumber)
                        {
                            items.Add(new WriteItem(lineNumber, e.Name, e.GetArchiveCode(prefix)));
                        }
                    }
                    session.Properties["LineNumber"] = lineNumber;
                    return Task.FromResult(
                        new CompletionContext(
                            items.Select(n=>MakeItem(n)).ToImmutableArray()
                            )
                        );
                }
            }
            catch
            {
                
            }
            return Task.FromResult<CompletionContext>(null);
        }

        public async Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
        {
            if(item.Properties.TryGetProperty<WriteItem>(nameof(WriteItem), out var writeItem))
            {
                var lineNumber = ((int)(session.Properties["LineNumber"]) + 1);
                return await Task.FromResult($"将在第{lineNumber}为{writeItem.Name}生成序列化/反序列化代码");
            }
            return Task.FromResult<CompletionContext>(null);
        }

        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            if (char.IsNumber(trigger.Character) ||
                //char.IsPunctuation(trigger.Character) ||
                trigger.Character == '\n' ||
                trigger.Reason == CompletionTriggerReason.Backspace ||
                trigger.Reason == CompletionTriggerReason.Deletion)
            {
                return CompletionStartData.DoesNotParticipateInCompletion;
            }

            var lineStart = triggerLocation.GetContainingLine().Start;
            var span = new SnapshotSpan(lineStart, triggerLocation);
            var text = triggerLocation.Snapshot.GetText(span);
            if (text.TrimStart().ToUpper() != "__")
            {
                return CompletionStartData.DoesNotParticipateInCompletion;
            }

            var tokenSpan = FindTokenSpanAtPosition(triggerLocation);
            return new CompletionStartData(CompletionParticipation.ProvidesItems, tokenSpan);
        }

        private SnapshotSpan FindTokenSpanAtPosition(SnapshotPoint triggerLocation)
        {
            ITextStructureNavigator navigator = structureNavigatorSelectorService.GetTextStructureNavigator(
                triggerLocation.Snapshot.TextBuffer);
            TextExtent extent = navigator.GetExtentOfWord(triggerLocation);
            if (triggerLocation.Position > 0 && (!extent.IsSignificant || !extent.Span.GetText().Any(c => char.IsLetterOrDigit(c))))
            {
                // Improves span detection over the default ITextStructureNavigation result
                extent = navigator.GetExtentOfWord(triggerLocation - 1);
            }

            var tokenSpan = triggerLocation.Snapshot.CreateTrackingSpan(extent.Span,
                SpanTrackingMode.EdgeInclusive);

            var snapShot = triggerLocation.Snapshot;
            var tokenText = tokenSpan.GetText(snapShot);
            if (string.IsNullOrWhiteSpace(tokenText))
            {
                return new SnapshotSpan(triggerLocation, 0);
            }
            return new SnapshotSpan(tokenSpan.GetStartPoint(snapShot),
                tokenSpan.GetEndPoint(snapShot));
        }
    }
}
