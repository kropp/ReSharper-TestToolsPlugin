using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace MethodRunner
{
  public class RunMethodTask : RemoteTask
  {
    private readonly string myClassName;
    private readonly string myMethodName;
    private readonly bool myIsClassStatic;
    private readonly bool myIsMethodStatic;

    public RunMethodTask(XmlElement element) : base(element)
    {
    }

    public RunMethodTask(string runnerId, string className, string methodName, bool isClassStatic, bool isMethodStatic) : base(runnerId)
    {
      myClassName = className;
      myMethodName = methodName;
      myIsClassStatic = isClassStatic;
      myIsMethodStatic = isMethodStatic;
    }

    public string ClassName
    {
      get { return myClassName; }
    }

    public string MethodName
    {
      get { return myMethodName; }
    }

    public bool IsClassStatic
    {
      get { return myIsClassStatic; }
    }

    public bool IsMethodStatic
    {
      get { return myIsMethodStatic; }
    }

    public override bool IsMeaningfulTask
    {
      get { return false; }
    }

    protected bool Equals(RunMethodTask other)
    {
      return string.Equals(myClassName, other.myClassName) && string.Equals(myMethodName, other.myMethodName) &&
        myIsClassStatic.Equals(other.myIsClassStatic) && myIsMethodStatic.Equals(other.myIsMethodStatic);
    }

    public override bool Equals(RemoteTask other)
    {
      return other is RunMethodTask && Equals((RunMethodTask) other);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != typeof (RunMethodTask)) return false;
      return Equals((RunMethodTask) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = myClassName.GetHashCode();
        hashCode = (hashCode*397) ^ myMethodName.GetHashCode();
        hashCode = (hashCode*397) ^ myIsClassStatic.GetHashCode();
        hashCode = (hashCode*397) ^ myIsMethodStatic.GetHashCode();
        return hashCode;
      }
    }
  }
}