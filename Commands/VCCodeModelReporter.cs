using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.VCCodeModel;
using Microsoft.VisualStudio.Shell;
using System.Windows.Forms;

namespace DRYCodeGen.Commands
{
    internal class VCCodeModelReporter
    {
        public VCCodeModelReporter(DTE DTE) { 
            this.DTE = DTE;
        }

        private void ReportStruct(List<VCCodeStruct> elements,List<string> results)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (var e in elements)
            {
                results.Add("struct: " + e.FullName);
                for (int i = 1; i <= e.Members.Count; i++)
                {
                    var kind = e.Members.Item(i).Kind;
                    if (kind == vsCMElement.vsCMElementVariable)
                    {
                        results.Add("\t\t " + e.Members.Item(i).Name);
                    }
                }
            }
        }

        private void ReportClass(List<VCCodeClass> elements,List<string> results)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (var e in elements)
            {
                results.Add("class: "+e.FullName);
                for (int i = 1; i <= e.Members.Count; i++)
                {
                    var kind = e.Members.Item(i).Kind;
                    if(kind == vsCMElement.vsCMElementVariable)
                    {
                        results.Add("\t\t " + e.Members.Item(i).Name);
                    }
                }
            }
        }

        private void ReportNamespace(List<VCCodeNamespace> elements,List<string> results) 
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (var e in elements)
            {
                ReportStruct(e.Structs.Cast<VCCodeStruct>().ToList(), results);
                ReportClass(e.Classes.Cast<VCCodeClass>().ToList(),results);
            }
        }

        public string Test()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                VCFileCodeModel vcFileCodeModel = (VCFileCodeModel)DTE?.ActiveDocument?.ProjectItem?.FileCodeModel;
                if (vcFileCodeModel != null)
                {
                    List<string> results = new List<string>();

                    ReportStruct(vcFileCodeModel.Structs.Cast<VCCodeStruct>().ToList(), results);
                    ReportClass(vcFileCodeModel.Classes.Cast<VCCodeClass>().ToList(), results);
                    ReportNamespace(vcFileCodeModel.Namespaces.Cast<VCCodeNamespace>().ToList(), results);
                    return string.Join("\n", results);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                DTE.StatusBar.Text = ex.StackTrace;
                MessageBox.Show(ex.Message);
            }
            return null;
        }


        private DTE DTE { get; }
    }
}
