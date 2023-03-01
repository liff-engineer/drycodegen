using EnvDTE;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRYCodeGen.Completions
{
    [Export(typeof(IAsyncCompletionSourceProvider))]
    [Name("Test AsyncCompletion ")]
    [ContentType("code")]
    internal class AsyncCompletionSourceProvider : IAsyncCompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService navigatorSelectorService { get; set; }

        [Import]
        internal SVsServiceProvider ServiceProvider = null;

        public IAsyncCompletionSource GetOrCreate(ITextView textView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE DTE = (DTE)ServiceProvider.GetService(typeof(DTE));
            VCClassReader reader = new VCClassReader(DTE);
            var classes = reader.Run();
            return new AsyncCompletionSource(navigatorSelectorService, classes);
        }
    }
}
