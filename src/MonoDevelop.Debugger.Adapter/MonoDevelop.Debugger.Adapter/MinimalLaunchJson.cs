//
// MinimalLaunchJson.cs
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
using System.IO;
using MonoDevelop.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MonoDevelop.Debugger.Adapter
{
	/// <summary>
	/// { 
	///     "type": "mock",
	///     "request": "launch",
	///     "program": "c:\\path\\to\\your\\folder\\MockDebug.txt"
	/// }
	/// </summary>
	class MinimalLaunchJson
	{
		Dictionary<string, JToken> configurationProperties;

		[JsonProperty ("type")]
		public string Type { get; set; }

		[JsonProperty ("request")]
		public string Request { get; set; }

		[JsonProperty ("program")]
		public string Program { get; set; }

		[JsonProperty ("$adapter")]
		public string Adapter { get; set; }

		[JsonIgnore]
		public Dictionary<string, JToken> ConfigurationProperties {
			get { return configurationProperties; }
		}

		public static MinimalLaunchJson Read (FilePath fileName)
		{
			string json = File.ReadAllText (fileName);

			var launchJson = JsonConvert.DeserializeObject<MinimalLaunchJson> (json);

			launchJson.Program = FixRelativePath (fileName.ParentDirectory, launchJson.Program);
			launchJson.Adapter = FixRelativePath (fileName.ParentDirectory, launchJson.Adapter);

			launchJson.configurationProperties = new Dictionary<string, JToken> ();
			launchJson.configurationProperties.Add ("program", new JValue (launchJson.Program));

			return launchJson;
		}

		static string FixRelativePath (FilePath baseDirectory, string fileName)
		{
			if (string.IsNullOrEmpty (fileName)) {
				return fileName;
			}

			if (Path.IsPathRooted (fileName)) {
				return fileName;
			}

			return Path.Combine (baseDirectory, fileName);
		}
	}
}
