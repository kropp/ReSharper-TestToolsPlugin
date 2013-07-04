using System.Windows.Forms;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace MethodRunner
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

      MessageBox.Show("Run " + runMethodTask.ClassName + "." + runMethodTask.MethodName + " " + runMethodTask.IsClassStatic + " " + runMethodTask.IsMethodStatic);
    }
  }
}