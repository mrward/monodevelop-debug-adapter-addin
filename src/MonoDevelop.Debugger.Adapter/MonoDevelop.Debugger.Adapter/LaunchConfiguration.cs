//
// LaunchConfiguration.cs
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
	/// <summary>
	/// { 
	///     "type": "mock",
	///     "request": "launch",
	///     "program": "c:\\path\\to\\your\\folder\\MockDebug.txt"
	/// }
	/// </summary>
	class LaunchConfiguration
	{
		public static string NoneConfigurationId = "None";

		public static LaunchConfiguration CreateNoneConfiguration ()
		{
			return new LaunchConfiguration {
				Id = NoneConfigurationId,
				Name = GettextCatalog.GetString ("None")
			};
		}

		Dictionary<string, JToken> properties = new Dictionary<string, JToken> ();
		FilePath fileName;

		[JsonIgnore]
		public string Id { get; private set; }

		[JsonIgnore]
		public string Name { get; private set; }

		[JsonProperty ("$adapter")]
		public string Adapter { get; set; }

		[JsonProperty ("program")]
		public string Program { get; set; }

		[JsonProperty ("request")]
		public string Request { get; set; }

		[JsonProperty ("type")]
		public string Type { get; set; }

		[JsonIgnore]
		public bool IsActive { get; set; }

		[JsonIgnore]
		public Dictionary<string, JToken> Properties {
			get { return properties; }
		}

		public bool IsValid ()
		{
			return StringComparer.OrdinalIgnoreCase.Equals (Request, "launch") &&
				!string.IsNullOrEmpty (Adapter);
		}

		public static IEnumerable<LaunchConfiguration> ReadConfigurations (FilePath fileName, JProperty configurationsProperty)
		{
			var configurations = configurationsProperty.Value as JArray;
			if (configurations != null) {
				foreach (JToken token in configurations) {
					var jsonObject = token as JObject;
					if (jsonObject != null) {
						yield return Read (fileName, jsonObject);
					}
				}
			}
		}

		public static LaunchConfiguration Read (FilePath fileName)
		{
			string json = File.ReadAllText (fileName);
			JObject jsonObject = JObject.Parse (json);
			return Read (fileName, jsonObject);
		}

		public static LaunchConfiguration Read (FilePath fileName, JObject jsonObject)
		{
			var config = new LaunchConfiguration ();
			config.fileName = fileName;

			foreach (KeyValuePair<string, JToken> item in jsonObject) {
				config.AddProperty (item.Key, item.Value);
			}

			if (string.IsNullOrEmpty (config.Name)) {
				config.Name = config.Type;
				if (string.IsNullOrEmpty (config.Name)) {
					config.Name = "launch.json";
				}
			}

			// Special case 'node'.
			if (string.IsNullOrEmpty (config.Adapter)) {
				if (config.Type == "node") {
					config.Adapter = "node";
				}
			}

			return config;
		}

		void AddProperty (string name, JToken value)
		{
			properties [name] = value;

			if (StringComparer.OrdinalIgnoreCase.Equals (name, "program")) {
				Program = FixRelativePath (fileName.ParentDirectory, value.ToString ());
				properties [name] = new JValue (Program);
			} else if (StringComparer.OrdinalIgnoreCase.Equals (name, "$adapter")) {
				Adapter = FixRelativePath (fileName.ParentDirectory, value.ToString ());
				properties [name] = new JValue (Adapter);
			} else if (StringComparer.OrdinalIgnoreCase.Equals (name, "request")) {
				Request = value.ToString ();
			} else if (StringComparer.OrdinalIgnoreCase.Equals (name, "type")) {
				Type = value.ToString ();
			} else if (StringComparer.OrdinalIgnoreCase.Equals (name, "name")) {
				Name = value.ToString ();
			}
		}

		static string FixRelativePath (FilePath baseDirectory, string fileName)
		{
			if (string.IsNullOrEmpty (fileName)) {
				return fileName;
			}

			if (fileName.Contains ("${")) {
				return fileName;
			}

			if (Path.IsPathRooted (fileName)) {
				return fileName;
			}

			return Path.GetFullPath (Path.Combine (baseDirectory, fileName));
		}
	}
}
