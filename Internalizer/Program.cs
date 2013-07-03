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
        if (type.IsPublic)
        {
          if (!types.ContainsKey(type.MetadataToken))
          {
            Console.WriteLine("I: " + type);
            type.IsPublic = false;
          }
          else
          {

          }
        }
      }

      File.Move(mainfile, mainfile + ".old");

      using (var key = File.OpenRead("DEVELOPMENT.snk"))
      {
        mainass.Write(mainfile, new WriterParameters { StrongNameKeyPair = new StrongNameKeyPair(key) });
      }

    }

    bool IsPublicOverride(MethodDefinition meth)
    {
      return meth.IsVirtual && meth.IsPublic;
    }



  }
}
