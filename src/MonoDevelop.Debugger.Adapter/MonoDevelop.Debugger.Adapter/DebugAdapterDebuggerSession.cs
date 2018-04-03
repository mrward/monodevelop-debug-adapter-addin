//
// DebugAdapterDebuggerSession.cs
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
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Mono.Debugging.Client;
using MonoDevelop.Core;
using MonoDevelop.Debugger.VsCodeDebugProtocol;

namespace MonoDevelop.Debugger.Adapter
{
	class DebugAdapterDebuggerSession : VSCodeDebuggerSession
	{
		DebugAdapterDebuggerStartInfo debugAdapterStartInfo;
		ProcessStartInfo processStartInfo;

		protected override void OnRun (DebuggerStartInfo startInfo)
		{
			debugAdapterStartInfo = (DebugAdapterDebuggerStartInfo)startInfo;
			processStartInfo = debugAdapterStartInfo.GetProcessStartInfo ();

			base.OnRun (startInfo);
		}

		protected override string GetDebugAdapterPath ()
		{
			return processStartInfo.FileName;
		}

		protected override string GetDebugAdapterArguments ()
		{
			return processStartInfo.Arguments;
		}

		protected override AttachRequest CreateAttachRequest (long processId)
		{
			throw new NotImplementedException ();
		}

		protected override InitializeRequest CreateInitRequest ()
		{
			base.protocolClient.LogMessage += ProtocolClientLogMessage;

			var initialize = new InitializeRequest ();
			initialize.Args.AdapterID = "Test";
			initialize.Args.ClientID = BrandingService.ApplicationName;
			initialize.Args.LinesStartAt1 = true;
			initialize.Args.ColumnsStartAt1 = true;
			initialize.Args.PathFormat = InitializeArguments.PathFormatValue.Path;
			initialize.Args.SupportsVariableType = true;
			initialize.Args.SupportsVariablePaging = false;
			initialize.Args.SupportsRunInTerminalRequest = true;
			initialize.Args.SupportsHandshakeRequest = false;

			return initialize;
		}

		protected override LaunchRequest CreateLaunchRequest (DebuggerStartInfo startInfo)
		{
			debugAdapterStartInfo = (DebugAdapterDebuggerStartInfo)startInfo;
			return new LaunchRequest (false, debugAdapterStartInfo.GetLaunchProperties ());
		}

		protected override void OnInitialized ()
		{
			OnStarted ();
			// Hack. Wait a bit for breakpoint responses.
			// Need a way to wait until the breakpoints have been configured before sending the
			// ConfigurationDone request.
			System.Threading.Thread.Sleep (1000);
			base.OnInitialized ();
		}

		void ProtocolClientLogMessage (object sender, LogEventArgs e)
		{
			OnLogMessage (e.Category, e.Message);
		}

		void OnLogMessage (LogCategory category, string message)
		{
			bool standardError = category == LogCategory.Warning;
			OnDebuggerOutput (standardError, message + Environment.NewLine);
		}

		protected override ProcessInfo[] OnGetProcesses ()
		{
			var processes = base.OnGetProcesses ();
			if (processes.Length > 0) {
				return processes;
			}

			return new [] {
				new ProcessInfo (1, Path.GetFileName (debugAdapterStartInfo.Command))
			};
		}
	}
}
