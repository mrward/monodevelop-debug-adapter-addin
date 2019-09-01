//
// DebugAdapterTextEditorExtension.cs
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
using Microsoft.CodeAnalysis.Text;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Editor.Extension;
using MonoDevelop.Ide.Gui;
using MonoDevelop.LanguageServer.Client;

namespace MonoDevelop.Debugger.Adapter
{
	class DebugAdapterTextEditorExtension : TextEditorExtension, IDebuggerExpressionResolver
	{
		[CommandUpdateHandler (DebugCommands.Debug)]
		void OnUpdateDebug (CommandInfo info)
		{
			if (!IsTextEditorExtensionEnabled ()) {
				info.Bypass = true;
				return;
			}

			if (DebuggingService.IsPaused) {
				info.Enabled = true;
				info.Text = GettextCatalog.GetString ("_Continue Debugging");
				return;
			}

			info.Enabled = !DebuggingService.IsDebugging;
		}

		bool IsTextEditorExtensionEnabled ()
		{
			Document document = IdeApp.Workbench.ActiveDocument;
			if (document == null) {
				return false;
			}

			LaunchConfiguration configuration = DebugAdapterService.GetActiveLaunchConfiguration (document, allowNoneConfiguration: false);
			if (configuration != null) {
				return true;
			}
			return false;
		}

		[CommandHandler (DebugCommands.Debug)]
		void OnDebug ()
		{
			if (DebuggingService.IsPaused) {
				DebuggingService.Resume ();
				return;
			}

			IdeApp.Workbench.SaveAll ();

			LaunchDebugAdapter ();
		}

		void LaunchDebugAdapter ()
		{
			Document document = IdeApp.Workbench.ActiveDocument;
			LaunchConfiguration configuration = DebugAdapterService.GetActiveLaunchConfiguration (document, allowNoneConfiguration: false);
			if (configuration != null) {
				var context = new LaunchContext (DocumentContext.Name);
				DebugAdapterService.LaunchAdapter (configuration, context);
			}
		}

		[CommandUpdateHandler (ProjectCommands.Stop)]
		void OnUpdateStop (CommandInfo info)
		{
			if (!IsTextEditorExtensionEnabled ()) {
				info.Bypass = true;
			}
		}

		[CommandHandler (ProjectCommands.Stop)]
		void OnStop ()
		{
			DebuggingService.Stop ();
		}

		public Task<DebugDataTipInfo> ResolveExpressionAsync (
			IReadonlyTextDocument editor,
			DocumentContext doc,
			int offset,
			CancellationToken cancellationToken)
		{
			if (!IsTextEditorExtensionEnabled ()) {
				return Task.FromResult (new DebugDataTipInfo ());
			}

			var location = editor.OffsetToLocation (offset);
			WordAtPosition wordAtPosition = editor.GetWordAtPosition (location.Line, location.Column);
			if (!wordAtPosition.IsEmpty) {
				int wordOffset = editor.LocationToOffset (location.Line, wordAtPosition.StartColumn);
				var span = new TextSpan (wordOffset, wordAtPosition.Length);
				var tipInfo = new DebugDataTipInfo (span, wordAtPosition.Text);
				return Task.FromResult (tipInfo);
			}

			return Task.FromResult (new DebugDataTipInfo ());
		}
	}
}
