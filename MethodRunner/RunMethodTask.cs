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
using System.Xml;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace ReSharper.Plugins.TestTools.MethodRunner
{
  [Serializable]
  public class RunMethodTask : RemoteTask, IEquatable<RunMethodTask>
  {
    private const string ASSEMBLY_LOCATION = "assemblyLocation";
    private const string CLASS_NAME = "className";
    private const string METHOD_NAME = "methodName";
    private const string IS_CLASS_STATIC = "isClassStatic";
    private const string IS_METHOD_STATIC = "isMethodStatic";

    private readonly string myAssemblyLocation;
    private readonly string myClassName;
    private readonly string myMethodName;
    private readonly bool myIsClassStatic;
    private readonly bool myIsMethodStatic;

    public RunMethodTask(XmlElement element) : base(element)
    {
      myAssemblyLocation = GetXmlAttribute(element, ASSEMBLY_LOCATION);
      myClassName = GetXmlAttribute(element, CLASS_NAME);
      myMethodName = GetXmlAttribute(element, METHOD_NAME);
      myIsClassStatic = bool.Parse(GetXmlAttribute(element, IS_CLASS_STATIC));
      myIsMethodStatic = bool.Parse(GetXmlAttribute(element, IS_METHOD_STATIC));
    }

    public RunMethodTask(string runnerId, string assemblyLocation, string className, string methodName, bool isClassStatic, bool isMethodStatic) : base(runnerId)
    {
      myAssemblyLocation = assemblyLocation;
      myClassName = className;
      myMethodName = methodName;
      myIsClassStatic = isClassStatic;
      myIsMethodStatic = isMethodStatic;
    }

    public string AssemblyLocation
    {
      get { return myAssemblyLocation; }
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

    public override void SaveXml(XmlElement element)
    {
      base.SaveXml(element);
      SetXmlAttribute(element, ASSEMBLY_LOCATION, myAssemblyLocation);
      SetXmlAttribute(element, CLASS_NAME, myClassName);
      SetXmlAttribute(element, METHOD_NAME, myMethodName);
      SetXmlAttribute(element, IS_CLASS_STATIC, myIsClassStatic.ToString());
      SetXmlAttribute(element, IS_METHOD_STATIC, myIsMethodStatic.ToString());
    }

    public bool Equals(RunMethodTask other)
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