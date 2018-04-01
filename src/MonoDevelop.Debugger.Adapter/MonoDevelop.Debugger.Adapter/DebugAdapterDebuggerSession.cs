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
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Mono.Debugging.Client;
using MonoDevelop.Core;

namespace MonoDevelop.Debugger.Adapter
{
	class DebugAdapterDebuggerSession : DebuggerSession
	{
		MonoDevelopDebugAdapterHost debugAdapterHost;
		InitializeResponse initializeResponse;

		protected override void OnAttachToProcess (long processId)
		{
			throw new NotImplementedException ();
		}

		protected override void OnContinue ()
		{
		}

		protected override void OnDetach ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnEnableBreakEvent (BreakEventInfo eventInfo, bool enable)
		{
		}

		protected override void OnFinish ()
		{
		}

		protected override ProcessInfo[] OnGetProcesses ()
		{
			return new [] {
				new ProcessInfo (1, "DebugAdapter")
			};
		}

		protected override Backtrace OnGetThreadBacktrace (long processId, long threadId)
		{
			throw new NotImplementedException ();
		}

		protected override ThreadInfo[] OnGetThreads (long processId)
		{
			return new [] {
				new ThreadInfo (processId, 1, "Main Thread", string.Empty)
			};
		}

		protected override BreakEventInfo OnInsertBreakEvent (BreakEvent breakEvent)
		{
			throw new NotImplementedException ();
		}

		protected override void OnNextInstruction ()
		{
		}

		protected override void OnNextLine ()
		{
		}

		protected override void OnRemoveBreakEvent (BreakEventInfo eventInfo)
		{
		}

		protected override void OnRun (DebuggerStartInfo startInfo)
		{
			if (HasExited)
				throw new UserException (GettextCatalog.GetString ("Debug adapter has exited."));

			try {
				var debugAdapterStartInfo = (DebugAdapterDebuggerStartInfo)startInfo;
				var process = new Process ();
				process.StartInfo = debugAdapterStartInfo.GetProcessStartInfo ();
				if (!process.Start ()) {
					LoggingService.LogError ("Failed to launch Debug Adapter. {0}", debugAdapterStartInfo);
					return;
				}

				debugAdapterHost = new MonoDevelopDebugAdapterHost (this, process);
				debugAdapterHost.Start ();

				OnInitialize ();
				OnStarted ();

				OnLaunch (debugAdapterStartInfo);
				OnConfigurationComplete ();
			} catch (Exception ex) {
				LoggingService.LogError ("DebugAdapterDebuggerSession.OnRun error.", ex);
			}
		}

		void OnInitialize ()
		{
			var initialize = new InitializeRequest ();
			initialize.Args.AdapterID = "Test";
			initializeResponse = debugAdapterHost.Protocol.SendRequestSync (initialize);
		}

		void OnConfigurationComplete ()
		{
			if (initializeResponse.SupportsConfigurationDoneRequest == true) {
				var configurationDoneRequest = new ConfigurationDoneRequest ();
				debugAdapterHost.Protocol.SendRequestSync (configurationDoneRequest);
			}
		}

		void OnLaunch (DebugAdapterDebuggerStartInfo startInfo)
		{
			var launchRequest = new LaunchRequest (false, startInfo.LaunchJson.ConfigurationProperties);
			debugAdapterHost.Protocol.SendRequestSync (launchRequest);
		}

		protected override void OnSetActiveThread (long processId, long threadId)
		{
		}

		protected override void OnStepInstruction ()
		{
		}

		protected override void OnStepLine ()
		{
		}

		protected override void OnStop ()
		{
		}

		protected override void OnUpdateBreakEvent (BreakEventInfo eventInfo)
		{
		}

		public override void Dispose ()
		{
			try {
				var host = debugAdapterHost;
				debugAdapterHost = null;
				if (host != null) {
					host.Protocol.Stop ();
				}
			} catch (Exception ex) {
				LoggingService.LogError ("DebugAdapterDebuggerSession.Dispose error", ex);
			}
			base.Dispose ();
		}

		protected override void OnExit ()
		{
			HasExited = true;

			try {
				var host = debugAdapterHost;
				debugAdapterHost = null;

				if (host != null) {
					var request = new DisconnectRequest ();
					host.Protocol.SendRequestSync (request);
					host.Protocol.Stop ();
				}
			} catch (Exception ex) {
				LoggingService.LogError ("DebugAdapterDebuggerSession.OnExit error.", ex);
			}
		}

		internal void OnLogMessage (LogCategory category, string message)
		{
			bool standardError = category == LogCategory.Warning;
			OnDebuggerOutput (standardError, message + Environment.NewLine);
		}

		internal void OnTerminated ()
		{
			var eventArgs = new TargetEventArgs (TargetEventType.TargetExited);
			OnTargetEvent (eventArgs);
		}

		internal void OnExited (int exitCode)
		{
			var eventArgs = new TargetEventArgs (TargetEventType.TargetExited) {
				ExitCode = exitCode
			};
			OnTargetEvent (eventArgs);
		}
	}
}
