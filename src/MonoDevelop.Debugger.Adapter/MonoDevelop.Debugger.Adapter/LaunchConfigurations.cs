//
// LaunchConfigurations.cs
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
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.Debugger.Adapter
{
	class LaunchConfigurations
	{
		Dictionary<string, LaunchConfigurationCacheInfo> configurationsCache =
			new Dictionary<string, LaunchConfigurationCacheInfo> ();

		public IEnumerable<LaunchConfiguration> GetConfigurations (Document document)
		{
			if (!document.SupportsLaunchConfiguration ()) {
				return Enumerable.Empty<LaunchConfiguration> ();
			}

			return GetConfigurations (document.FileName);
		}

		public IEnumerable<LaunchConfiguration> GetConfigurations (FilePath scriptFileName)
		{
			LaunchConfigurationCacheInfo cacheInfo = GetExistingConfigurations (scriptFileName);
			if (cacheInfo != null) {
				if (cacheInfo.IsOutOfDate ()) {
					return ReadConfigurations (scriptFileName, cacheInfo.GetActiveConfigurationName ());
				}
				return cacheInfo.Configurations;
			}

			return ReadConfigurations (scriptFileName);
		}

		LaunchConfigurationCacheInfo GetExistingConfigurations (FilePath scriptFileName)
		{
			string directory = scriptFileName.ParentDirectory;
			LaunchConfigurationCacheInfo cacheInfo = null;
			if (configurationsCache.TryGetValue (directory, out cacheInfo)) {
				return cacheInfo;
			}
			return null;
		}

		public void SetActiveLaunchConfiguration (LaunchConfiguration config, Document document)
		{
			LaunchConfigurationCacheInfo cacheInfo = GetExistingConfigurations (document.FileName);
			if (cacheInfo != null) {
				foreach (var existingConfig in cacheInfo.Configurations) {
					existingConfig.IsActive = false;
				}
				config.IsActive = true;
			} else {
				LoggingService.LogWarning ("Launch configuration not found. Unable to set active configuration. '{0}'", document.FileName);
			}
		}

		List<LaunchConfiguration> ReadConfigurations (FilePath scriptFileName, string activeConfiguration = "None")
		{
			string directory = scriptFileName.ParentDirectory;
			try {
				var reader = new LaunchConfigurationsReader ();
				List<LaunchConfiguration> foundConfigurations = reader.Read (directory);
				if (foundConfigurations != null) {
					AddNoneConfiguration (foundConfigurations);
					MarkActiveConfiguration (foundConfigurations, activeConfiguration);

					var cacheInfo = new LaunchConfigurationCacheInfo (
						foundConfigurations,
						reader.FileName,
						reader.LastWriteTime.Value);
					configurationsCache[directory] = cacheInfo;

					return foundConfigurations;
				}
			} catch (Exception ex) {
				LoggingService.LogError ("Unable to read launch configurations.", ex);
			}

			return new List<LaunchConfiguration> ();
		}

		void AddNoneConfiguration (List<LaunchConfiguration> configurations)
		{
			var noneConfiguration = LaunchConfiguration.CreateNoneConfiguration ();
			configurations.Insert (0, noneConfiguration);
		}

		void MarkActiveConfiguration (IEnumerable<LaunchConfiguration> configurations, string name)
		{
			var matchedConfiguration = configurations.FirstOrDefault (configuration => StringComparer.OrdinalIgnoreCase.Equals (configuration.Name, name));
			if (matchedConfiguration != null) {
				matchedConfiguration.IsActive = true;
				return;
			}

			var defaultActiveConfiguration = configurations.FirstOrDefault ();
			if (defaultActiveConfiguration != null)
				defaultActiveConfiguration.IsActive = true;
		}

		public LaunchConfiguration GetActiveLaunchConfiguration (Document document)
		{
			if (!document.SupportsLaunchConfiguration ()) {
				return null;
			}

			return GetConfigurations (document.FileName)
				.FirstOrDefault (configuration => configuration.IsActive);
		}
	}
}
