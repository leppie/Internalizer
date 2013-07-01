using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Internalizer
{
  class Program
  {
    static void Main(string[] args)
    {
      var mainfile = args[0];

      var rest = new List<string>(args.Skip(1).SelectMany(GetFilesFromArg).Except(new [] { mainfile }));

      var mainass = AssemblyDefinition.ReadAssembly(mainfile);

      var usages = from depfile in rest
                   select AssemblyDefinition.ReadAssembly(depfile)
                   into dep
                   from mref in dep.MainModule.GetMemberReferences()
                   where ((AssemblyNameReference) mref.DeclaringType.Scope).FullName == mainass.Name.FullName
                   let m = Scrutinize(mref)
                   where m != null
                   select m.ToString();

      foreach (var use in usages.Distinct().OrderBy(x => x))
      {
        Console.WriteLine(use);
      }
    }

    static MemberReference Scrutinize(MemberReference mref)
    {
      var gentype = mref.DeclaringType as GenericInstanceType;
      if (gentype != null)
      {
        var methref = mref as MethodReference;
        if (methref != null)
        {
          var genref = gentype.Resolve().Methods.SingleOrDefault(x => x.Name == mref.Name && x.Parameters.Count == methref.Parameters.Count);
          return genref;
        }
      }
      return mref;
    }

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
  }
}
