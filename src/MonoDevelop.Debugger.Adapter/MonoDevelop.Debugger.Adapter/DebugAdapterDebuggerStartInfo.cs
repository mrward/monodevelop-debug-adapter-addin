//
// DebugAdapterDebuggerStartInfo.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using Mono.Debugging.Client;
using MonoDevelop.Core;
using MonoDevelop.Core.Assemblies;
using MonoDevelop.Core.StringParsing;
using MonoDevelop.Debugger.Adapter.Commands;
using Newtonsoft.Json.Linq;

namespace MonoDevelop.Debugger.Adapter
{
	class DebugAdapterDebuggerStartInfo : DebuggerStartInfo
	{
		DebugAdapterExecutionCommand command;

		public DebugAdapterDebuggerStartInfo (DebugAdapterExecutionCommand command)
		{
			this.command = command;

			Command = command.Command;
			Arguments = command.Arguments;
			WorkingDirectory = command.WorkingDirectory;

			Adapter = command.LaunchConfiguration.Adapter;
			LaunchConfiguration = command.LaunchConfiguration;
		}

		public string Adapter { get; set; }

		public LaunchConfiguration LaunchConfiguration { get; private set; }
		public DebugAdapterExecutionCommand ExecutionCommand { get; private set; }

		public override string ToString ()
		{
			return $"Command={Command}, Arguments={Arguments}";
		}

		public ProcessStartInfo GetProcessStartInfo ()
		{
			string fileName = Adapter;
			string arguments = null;
			var stringTagModel = new LaunchConfigurationStringTagModel (command.Context);

			string monoPath = GetMonoPath ();
			if (monoPath != null) {
				fileName = monoPath;
				arguments = "\"" + ParseString (Adapter, stringTagModel) + "\"";
			}

			return new ProcessStartInfo {
				FileName = ParseString (fileName, stringTagModel),
				Arguments = ParseString (arguments, stringTagModel),
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
		}

		string GetMonoPath ()
		{
			if (AdapterIsDotNetExe ()) {
				var monoRuntime = Runtime.SystemAssemblyService.DefaultRuntime as MonoTargetRuntime;
				if (monoRuntime != null) {
					return monoRuntime.GetMonoExecutableForAssembly (Adapter);
				}
			}
			return null;
		}

		bool AdapterIsDotNetExe ()
		{
			FilePath fileName = Adapter;

			if (!fileName.IsNullOrEmpty) {
				return fileName.HasExtension (".exe") || fileName.HasExtension (".dll");
			}

			return false;
		}

		static string ParseString (string text, IStringTagModel stringTagModel)
		{
			if (string.IsNullOrEmpty (text))
				return string.Empty;

			return StringParserService.Parse (text, stringTagModel);
		}

		public Dictionary<string, JToken> GetLaunchProperties ()
		{
			var properties = new Dictionary<string, JToken> ();
			var stringTagModel = new LaunchConfigurationStringTagModel (command.Context);

			foreach (KeyValuePair<string, JToken> property in LaunchConfiguration.Properties) {
				var jvalue = property.Value as JValue;
				if (jvalue != null && jvalue.Type == JTokenType.String) {
					string value = ParseString (property.Value.ToString (), stringTagModel);
					properties [property.Key] = new JValue (value);
				} else {
					properties [property.Key] = property.Value;
				}
			}

			return properties;
		}
	}
}