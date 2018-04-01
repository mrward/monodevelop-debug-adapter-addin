﻿//
// LaunchConfigurationsReader.cs
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
using System.IO;
using MonoDevelop.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MonoDevelop.Debugger.Adapter
{
	class LaunchConfigurationsReader
	{
		public List<LaunchConfiguration> Read (string directory)
		{
			if (!Directory.Exists (directory))
				return null;

			string launchFileName = FindLaunchFile (directory);
			if (launchFileName == null)
				return null;

			FileName = launchFileName;
			LastWriteTime = File.GetLastWriteTime (launchFileName);

			return ReadFile ();
		}

		public FilePath FileName { get; private set; }
		public DateTime? LastWriteTime { get; private set; }

		static string FindLaunchFile (string directory)
		{
			string launchFileName = Path.Combine (directory, "launch.json");
			if (File.Exists (launchFileName)) {
				return launchFileName;
			}

			launchFileName = Path.Combine (directory, ".vscode", "launch.json");
			if (File.Exists (launchFileName)) {
				return launchFileName;
			}

			return null;
		}

		List<LaunchConfiguration> ReadFile ()
		{
			JObject jsonObject = ReadJsonFile ();
			return GetLaunchConfigurations (jsonObject);
		}

		JObject ReadJsonFile ()
		{
			using (var fileStream = File.OpenRead (FileName)) {
				using (var reader = new StreamReader (fileStream)) {
					using (var jsonReader = new JsonTextReader (reader)) {
						return JObject.Load (jsonReader);
					}
				}
			}
		}

		List<LaunchConfiguration> GetLaunchConfigurations (JObject jsonObject)
		{
			var configurations = new List<LaunchConfiguration> ();

			JProperty configurationsProperty = jsonObject.Property ("configurations");
			if (configurationsProperty != null) {
				foreach (var configuration in LaunchConfiguration.ReadConfigurations (FileName, configurationsProperty)) {
					if (configuration.IsValid ()) {
						configurations.Add (configuration);
					} else {
						LoggingService.LogInfo ("Invalid configuration {0} found in {1}", configuration.Name, FileName);
					}
				}
			} else {
				var configuration = LaunchConfiguration.Read (FileName, jsonObject);
				if (configuration.IsValid ()) {
					configurations.Add (configuration);
				} else {
					LoggingService.LogInfo ("Invalid configuration found in {0}", FileName);
				}
			}

			return configurations;
		}
	}
}
