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

      var rest = new List<string>(args.Skip(1).SelectMany(GetFilesFromArg).Except(new [] { mainfile }));

      var mainass = AssemblyDefinition.ReadAssembly(mainfile);

      var depasses = rest.Select(AssemblyDefinition.ReadAssembly).ToList();

      AssemblyAnalyzer.AnalyzeTypes(mainass, depasses).ForEach(Console.WriteLine);
      AssemblyAnalyzer.AnalyzeMembers(mainass, depasses).ForEach(Console.WriteLine);
    }



  }
}
