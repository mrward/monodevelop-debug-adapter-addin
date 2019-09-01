//
// DebugAdapterTooltipProvider.cs
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

using System.Threading;
using System.Threading.Tasks;
using Mono.Addins;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Gui;
using Xwt;

namespace MonoDevelop.Debugger.Adapter
{
	/// <summary>
	/// HACK: The DebugValueTooltipProvider is internal and only works if a document has an associated
	/// ParsedDocument. So this tooltip provider gets the debug tooltip provider and if there
	/// is an associated launch configuration for the active file adds a dummy ParsedDocument to
	/// the DocumentContext when GetItem is called.
	/// </summary>
	class DebugAdapterTooltipProvider : TooltipProvider
	{
		TooltipProvider debugTooltipProvider;

		public DebugAdapterTooltipProvider ()
		{
			debugTooltipProvider = GetDebugTooltipProvider ();
		}

		static TooltipProvider GetDebugTooltipProvider ()
		{
			var nodes = AddinManager.GetExtensionNodes<TypeExtensionNode> ("/MonoDevelop/Ide/Editor/TooltipProviders");
			foreach (var node in nodes) {
				if (node.Id == "Debug") {
					return (TooltipProvider)node.CreateInstance ();
				}
			}

			return null;
		}

		public override Task<TooltipItem> GetItem (TextEditor editor, DocumentContext ctx, int offset, CancellationToken token = default (CancellationToken))
		{
			if (debugTooltipProvider == null || !DebuggingService.IsPaused || !IsEnabled ())
				return Task.FromResult<TooltipItem> (null);

			ctx = FixDocumentContext (ctx);

			var item = debugTooltipProvider.GetItem (editor, ctx, offset, token);
			return item;
		}

		DocumentContext FixDocumentContext (DocumentContext ctx)
		{
			if (ctx?.ParsedDocument != null) {
				return ctx;
			}

			return new DummyDocumentContext (ctx, IdeApp.Workbench.ActiveDocument);
		}

		bool IsEnabled ()
		{
			// DocumentContext is null so get the document information again.
			Document document = IdeApp.Workbench.ActiveDocument;
			if (document == null) {
				return false;
			}

			LaunchConfiguration configuration = DebugAdapterService.GetActiveLaunchConfiguration (document, allowNoneConfiguration: false);
			return configuration != null;
		}

		public override Components.Window CreateTooltipWindow (TextEditor editor, DocumentContext ctx, TooltipItem item, int offset, ModifierKeys modifierState)
		{
			if (debugTooltipProvider == null || !IsEnabled ()) {
				return null;
			}

			return debugTooltipProvider.CreateTooltipWindow (editor, ctx, item, offset, modifierState);
		}

		public override void ShowTooltipWindow (TextEditor editor, Components.Window tipWindow, TooltipItem item, ModifierKeys modifierState, int mouseX, int mouseY)
		{
			if (debugTooltipProvider != null) {
				debugTooltipProvider.ShowTooltipWindow (editor, tipWindow, item, modifierState, mouseX, mouseY);
			}
		}

		public override bool IsInteractive (TextEditor editor, Components.Window tipWindow)
		{
			if (debugTooltipProvider != null) {
				return debugTooltipProvider.IsInteractive (editor, tipWindow);
			}

			return false;
		}

		public override void Dispose ()
		{
			if (debugTooltipProvider != null) {
				debugTooltipProvider.Dispose ();
			}

			base.Dispose ();
		}
	}
}
