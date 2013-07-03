using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
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

      AssemblyAnalyzer.AnalyzeTypes(mainass, depasses).OrderBy(x => x.FullName).ForEach(Console.WriteLine);
      AssemblyAnalyzer.AnalyzeMembers(mainass, depasses).OrderBy(x => x.Name).ForEach(Console.WriteLine);

      // how to do?

    }



  }
}
