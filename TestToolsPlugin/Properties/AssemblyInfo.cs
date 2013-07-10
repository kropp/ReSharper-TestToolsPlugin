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
[assembly: PluginTitle("Create Test Plugin")]
[assembly: PluginDescription("Creates NUnit test stub for tests on given class")]
[assembly: PluginVendor("Victor Kropp, JetBrains")]
