using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using MoreLinq;

namespace Internalizer
{
  class Program
  {
    static IEnumerable<string> GetFilesFromArg(string arg)
    {
      if (File.Exists(arg))
      {
        yield return arg;
      }
      else
      {
        foreach (var file in Directory.GetFiles(".", arg))
        {
          yield return Path.GetFileName(file);
        }
      }
    }

    static void Main(string[] args)
    {

      var mainfile = args[0];

      var rest = args.Skip(1).SelectMany(GetFilesFromArg).Except(new [] { mainfile }).ToList();

      var mainass = AssemblyDefinition.ReadAssembly(mainfile);

      var depasses = rest.ConvertAll(AssemblyDefinition.ReadAssembly);

      var types = AssemblyAnalyzer.AnalyzeTypes(mainass, depasses).ToDictionary(x => x.MetadataToken);
      var members = AssemblyAnalyzer.AnalyzeMembers(mainass, depasses).ToDictionary(x => x.MetadataToken);

      types.Values.OrderBy(x => x.FullName).ForEach(Console.WriteLine);
      members.Values.OrderBy(x => x.Name).ForEach(Console.WriteLine);

      // how to do?

      foreach (var type in mainass.MainModule.GetTypes())
      {
        if (type.IsPublic && type.Name != "#" && !type.Namespace.StartsWith("record"))
        {
          if (!types.ContainsKey(type.MetadataToken))
          {
            Console.WriteLine("I: " + type);
            type.IsPublic = false;
          }
          else
          {
            //foreach (var meth in type.Methods)
            //{
            //  if (meth.IsPublic && !IsPublicOverride(meth))
            //  {
            //    if (!members.ContainsKey(meth.MetadataToken))
            //    {
            //      Console.WriteLine("M: " + meth);
            //      meth.IsAssembly = true;
            //    }
            //  }
            //}
          }
        }
      }

      File.Move(mainfile, mainfile + ".old");

      using (var key = File.OpenRead("DEVELOPMENT.snk"))
      {
        mainass.Write(mainfile, new WriterParameters { StrongNameKeyPair = new StrongNameKeyPair(key) });
      }

    }

    static bool IsPublicOverride(MethodDefinition meth)
    {
      return meth.IsVirtual && meth.IsPublic;
    }



  }
}
