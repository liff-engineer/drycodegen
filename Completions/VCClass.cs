using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCCodeModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRYCodeGen.Completions
{
    internal class VCClass
    {
        public string Name { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }

        public List<string> Arguments { get; set; }

        public VCClass(string name, int startLine, int endLine, List<string> arguments)
        {
            Name = name;
            StartLine = startLine;
            EndLine = endLine;
            Arguments = arguments;
        }

        public string GetArchiveCode(string header)
        {
            List<string> results = new List<string>();
            results.Add("template<typename Ar>");
            results.Add(header + "void Save(Ar& ar) const");
            results.Add(header + "{");
            foreach (var m in Arguments)
            {
                results.Add(header + $"\tar.Write(\"{m}\",{m});");
            }
            results.Add(header + "}");

            results.Add(header + "template<typename Ar>");
            results.Add(header + "void Read(const Ar& ar)");
            results.Add(header + "{");
            foreach (var m in Arguments)
            {
                results.Add(header + $"\tar.Read(\"{m}\",{m});");
            }
            results.Add(header + "}");
            return string.Join("\n", results);
        }
    }

    class VCClassReader
    {
        private DTE DTE { get; }
        public VCClassReader(DTE DTE)
        {
            this.DTE = DTE;
        }

        private void ReadStructs(List<VCCodeStruct> elements,List<VCClass> results)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (var element in elements)
            {
                List<string> strings = new List<string>();
                var members = element.Members;
                for (int i = 1; i <= members.Count; i++)
                {
                    var kind = members.Item(i).Kind;
                    if(kind == vsCMElement.vsCMElementVariable)
                    {
                        strings.Add(members.Item(i).Name);
                    }
                }

                results.Add(new VCClass(element.Name, element.StartPoint.Line, element.EndPoint.Line, strings));
            }
        }

        private void ReadClasses(List<VCCodeClass> elements, List<VCClass> results)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (var element in elements)
            {
                List<string> strings = new List<string>();
                var members = element.Members;
                for (int i = 1; i <= members.Count; i++)
                {
                    var kind = members.Item(i).Kind;
                    if (kind == vsCMElement.vsCMElementVariable)
                    {
                        strings.Add(members.Item(i).Name);
                    }
                }

                results.Add(new VCClass(element.Name, element.StartPoint.Line, element.EndPoint.Line, strings));
            }
        }

        private void ReadNamespaces(List<VCCodeNamespace> elements, List<VCClass> results)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (var element in elements)
            {
                ReadStructs(element.Structs.Cast<VCCodeStruct>().ToList(),results);
                ReadClasses(element.Classes.Cast<VCCodeClass>().ToList(),results);
            }
        }

        private void  ReadCodeModel(VCFileCodeModel e,List<VCClass> results)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ReadStructs(e.Structs.Cast<VCCodeStruct>().ToList(), results);
            ReadClasses(e.Classes.Cast<VCCodeClass>().ToList(),results);
            ReadNamespaces(e.Namespaces.Cast<VCCodeNamespace>().ToList(),results);
        }

        public List<VCClass> Run()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                VCFileCodeModel vcFileCodeModel = (VCFileCodeModel)DTE?.ActiveDocument?.ProjectItem?.FileCodeModel;
                if (vcFileCodeModel != null)
                {
                    List<VCClass> results = new List<VCClass>();
                    ReadCodeModel(vcFileCodeModel,results);
                    return results;
                }
            }
            catch
            {

            }
            return null;
        }
    }
}
