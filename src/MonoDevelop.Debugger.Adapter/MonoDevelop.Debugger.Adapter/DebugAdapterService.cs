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

using System.Diagnostics;
using MonoDevelop.Core;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using System;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;

namespace MonoDevelop.Debugger.Adapter
{
	static class DebugAdapterService
	{
		static MonoDevelopDebugAdapterHost host;

		public static void LaunchAdapter (FilePath launchJsonFile)
		{
			var launchJson = MinimalLaunchJson.Read (launchJsonFile);

			Process process = StartDebugAdapterProcess (launchJson);
			if (process != null) {
				host = new MonoDevelopDebugAdapterHost (process);
				host.Start ();

				var initialize = new InitializeRequest ();
				initialize.Args.AdapterID = "Test";
				host.Protocol.SendRequest (initialize, OnCompleted, OnError);
			}
		}

		static void OnError (InitializeArguments args, ProtocolException ex)
		{
			host.Protocol.Stop ();
		}

		static void OnCompleted (InitializeArguments args, InitializeResponse response)
		{
			host.Protocol.Stop ();
		}

		static Process StartDebugAdapterProcess (MinimalLaunchJson launchJson)
		{
			var info = new ProcessStartInfo {
				FileName = "mono",
				Arguments = "\"" + launchJson.Adapter + "\"",
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			var process = new Process ();
			process.StartInfo = info;

			if (process.Start()) {
				return process;
			}
			return null;
		}
	}
}
