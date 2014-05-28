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

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace ReSharper.Plugins.TestTools.MethodRunner
{
  public class RunMethodRunner : RecursiveRemoteTaskRunner
  {
    public RunMethodRunner(IRemoteTaskServer server) : base(server)
    {
    }

    public override void ExecuteRecursive(TaskExecutionNode node)
    {
      var runMethodTask = node.RemoteTask as RunMethodTask;
      if (runMethodTask == null)
        return;

      var result = TaskResult.Skipped;
      var message = string.Empty;
      string output = null;

      try
      {
        var assembly = Assembly.LoadFrom(runMethodTask.AssemblyLocation);
        var type = assembly.GetType(runMethodTask.ClassName);

        if (runMethodTask.IsClassStatic)
        {
          var res = type.InvokeMember(runMethodTask.MethodName, BindingFlags.InvokeMethod, null, null, null);
          if (res != null)
            output = res.ToString();
        }
        else
        {
          var obj = Activator.CreateInstance(type);
          var flags = BindingFlags.InvokeMethod | BindingFlags.Public;
          if (runMethodTask.IsMethodStatic)
            flags |= BindingFlags.Static;
          else
            flags |= BindingFlags.Instance;
          var res = type.InvokeMember(runMethodTask.MethodName, flags, null, obj, null);
          if (res != null)
            output = res.ToString();
        }
        result = TaskResult.Success;
      }
      catch (Exception e)
      {
        var exceptions = TaskExecutor.ConvertExceptions(e, out message);
        Server.TaskException(runMethodTask, exceptions);
        result = TaskResult.Exception;
      }
      finally
      {
        if (!string.IsNullOrEmpty(output))
        {
          Server.TaskOutput(runMethodTask, output, TaskOutputType.STDOUT);
          Trace.WriteLine(output.FirstOrDefault().ToString());
        }
        Server.TaskFinished(runMethodTask, message, result);
      }
    }
  }
}