using System;
using System.Globalization;
using System.Resources;
using System.Reflection;
using System.IO;

namespace testResources
{
    /// <summary>
    /// приложение, содержащее несколько типов ресурсов:
    /// 1. app.Properties.Resources.resx
    /// 2. add files to project type Resources File
    ///    Resource1.resx - Build Action is Embedded Resource
    ///    Resource2.resx - Build Action is Resource
    /// 3. TextFile1.txt linked as Embedded Resource
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
        }

    }  // class
}
