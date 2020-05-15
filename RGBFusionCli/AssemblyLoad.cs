using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Linq;

namespace AssemblyLoad
{
    public static class AssemblyLoad
    {
        private static List<string> RGBFusionAssemblies;
        private static string _RGBFusionDirectory;
        public static void InitAssemblyDirectory(string RGBFusionDirectory)
        {
            RGBFusionAssemblies = Directory.GetFiles(RGBFusionDirectory, "*.dll", SearchOption.TopDirectoryOnly).ToList();
            AppDomain currentDomain = AppDomain.CurrentDomain;
            _RGBFusionDirectory = RGBFusionDirectory;
            currentDomain.AssemblyResolve += new ResolveEventHandler(currentDomain_AssemblyResolve);
        }



        public static Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //This handler is called only when the common language runtime tries to bind to the assembly and fails.

            //Retrieve the list of referenced assemblies in an array of AssemblyName.
            Assembly MyAssembly, objExecutingAssemblies;
            string strTempAssmbPath = _RGBFusionDirectory;
            objExecutingAssemblies = Assembly.GetExecutingAssembly();
            AssemblyName[] arrReferencedAssmbNames = objExecutingAssemblies.GetReferencedAssemblies();

            //Loop through the array of referenced assembly names.
            foreach (AssemblyName strAssmbName in arrReferencedAssmbNames)
            {
                //Check for the assembly names that have raised the "AssemblyResolve" event.
                if (RGBFusionAssemblies.Count(s => s.ToLower().Contains(strAssmbName.Name.ToLower() + ".dll")) > 0)
                {
                    //Build the path of the assembly from where it has to be loaded.
                    //The following line is probably the only line of code in this method you may need to modify:
                    if (!strTempAssmbPath.EndsWith("\\")) strTempAssmbPath += "\\";
                    strTempAssmbPath += args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll";
                    break;
                }

            }
            //Load the assembly from the specified path.
            MyAssembly = Assembly.LoadFrom(strTempAssmbPath);

            //Return the loaded assembly.
            return MyAssembly;
        }

    }
}
