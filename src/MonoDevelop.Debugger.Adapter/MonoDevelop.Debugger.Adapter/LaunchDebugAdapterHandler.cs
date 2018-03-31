//
// LaunchDebugAdapterHandler.cs
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

using System;
using MonoDevelop.Components;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Dialogs;

namespace MonoDevelop.Debugger.Adapter
{
	class LaunchDebugAdapterHandler : CommandHandler
	{
		protected override void Run ()
		{
			try {
				OnLaunchDebugAdapter ();
			} catch (Exception ex) {
				LoggingService.LogError ("Failed to launch debug adapter", ex);
				MessageService.ShowError (ex.Message);
			}
		}

		void OnLaunchDebugAdapter ()
		{
			FilePath launchJsonFile = SelectLaunchJsonFile ();
			if (launchJsonFile.IsNotNull) {
				DebugAdapterService.LaunchAdapter (launchJsonFile);
			}
		}

		FilePath SelectLaunchJsonFile ()
		{
			var dialog = new OpenFileDialog ();
			dialog.Action = FileChooserAction.Open;
			dialog.Title = GettextCatalog.GetString ("Launch Debug Adapter");

			// 'launch.json' as a file filter does not work so use a wildcard version.
			dialog.AddFilter (GettextCatalog.GetString ("Launch Files (launch.json)"), "*launch*.json");
			dialog.AddFilter (GettextCatalog.GetString ("Json Files (*.json)"), "*.json");
			dialog.AddAllFilesFilter ();

			if (dialog.Run ()) {
				return dialog.SelectedFile;
			}

			return null;
		}
	}
}
