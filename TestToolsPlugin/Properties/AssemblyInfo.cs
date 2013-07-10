// Copyright 2013 Victor Kropp, JetBrains s.r.o.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System.Reflection;
using JetBrains.ActionManagement;
using JetBrains.Application.PluginSupport;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("TestTools plugin for ReSharper")]
[assembly: AssemblyDescription("Provides number of useful test-related functions")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Victor Kropp, JetBrains s.r.o.")]
[assembly: AssemblyProduct("ReSharper-TestToolsPlugin")]
[assembly: AssemblyCopyright("Copyright © Victor Kropp, JetBrains s.r.o., 2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: ActionsXml("CreateTestPlugin.Actions.xml")]

// The following information is displayed by ReSharper in the Plugins dialog
[assembly: PluginTitle("Test Tools Plugin")]
[assembly: PluginDescription("Provides number of useful test-related functions")]
[assembly: PluginVendor("Victor Kropp, JetBrains s.r.o.")]
