//
// DummyDocumentContext.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2018 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.TypeSystem;

namespace MonoDevelop.Debugger.Adapter
{
	class DummyDocumentContext : DocumentContext
	{
		DocumentContext context;
		DefaultParsedDocument parsedDocument;

		public DummyDocumentContext (DocumentContext context, Ide.Gui.Document document)
		{
			this.context = context;
			parsedDocument = new DefaultParsedDocument (document.FilePath);
		}

		public override string Name => context.Name;

		public override Projects.Project Project => context?.Project;

		public override Document AnalysisDocument => context?.AnalysisDocument;

		public override ParsedDocument ParsedDocument => parsedDocument;

		public override void AttachToProject (Projects.Project project)
		{
			context?.AttachToProject (project);
		}

		public override OptionSet GetOptionSet ()
		{
			return context?.GetOptionSet ();
		}

		public override void ReparseDocument ()
		{
			context?.ReparseDocument ();
		}

		public override Task<ParsedDocument> UpdateParseDocument ()
		{
			return context?.UpdateParseDocument ();
		}

		public override T GetContent<T> ()
		{
			return context?.GetContent<T> ();
		}

		public override IEnumerable<T> GetContents<T> ()
		{
			return context?.GetContents<T> ();
		}
	}
}
