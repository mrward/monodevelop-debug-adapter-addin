//
// DebugAdapterService.cs
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
using MonoDevelop.Core;
using MonoDevelop.Debugger.Adapter.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.Debugger.Adapter
{
	static class DebugAdapterService
	{
		static LaunchConfigurations launchConfigurations = new LaunchConfigurations ();

		public static void LaunchAdapter (FilePath launchJsonFile)
		{
			var configuration = LaunchConfiguration.Read (launchJsonFile);
			LaunchAdapter (configuration, new LaunchContext ());
		}

		public static void LaunchAdapter (LaunchConfiguration configuration, LaunchContext context)
		{
			var debugAdapterCommand = new DebugAdapterExecutionCommand (configuration, context);

			IdeApp.ProjectOperations.DebugApplication (debugAdapterCommand);
		}

		public static IEnumerable<LaunchConfiguration> GetLaunchConfigurations (Document document)
		{
			return launchConfigurations.GetConfigurations (document);
		}

		public static void SetActiveLaunchConfiguration (LaunchConfiguration config, Document document)
		{
			launchConfigurations.SetActiveLaunchConfiguration (config, document);
		}

		public static LaunchConfiguration GetActiveLaunchConfiguration (Document document, bool allowNoneConfiguration = true)
		{
			var configuration = launchConfigurations.GetActiveLaunchConfiguration (document);

			if (!allowNoneConfiguration) {
				if (configuration?.Id == LaunchConfiguration.NoneConfigurationId) {
					return null;
				}
			}

			return configuration;
		}
	}
}
