using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using MoreLinq;

namespace Internalizer
{
  static class AssemblyAnalyzer
  {
    public static IEnumerable<MemberReference> AnalyzeMembers(AssemblyDefinition mainass, IEnumerable<AssemblyDefinition> depasses)
    {
      var musages = from dep in depasses
                    from mref in dep.MainModule.GetMemberReferences()
                    where ((AssemblyNameReference)mref.DeclaringType.Scope).FullName == mainass.Name.FullName
                    let m = Scrutinize(mref)
                    where m != null
                    select m;

      return musages.DistinctBy(x => x.FullName).OrderBy(x => x.Name);
    }

    public static IEnumerable<TypeReference> AnalyzeTypes(AssemblyDefinition mainass, IEnumerable<AssemblyDefinition> depasses)
    {
      var tusages = from dep in depasses
                    from tref in dep.MainModule.GetTypeReferences()
                    where ((AssemblyNameReference)tref.Scope).FullName == mainass.Name.FullName
                    select tref;

      return tusages.DistinctBy(x => x.FullName).OrderBy(x => x.FullName);
    }

    static MemberReference Scrutinize(MemberReference mref)
    {
      var gentype = mref.DeclaringType as GenericInstanceType;
      if (gentype != null)
      {
        var methref = mref as MethodReference;
        if (methref != null)
        {
          return methref.Resolve();
        }
      }
      var genmeth = mref as MethodReference;
      if (genmeth != null && genmeth.HasGenericParameters)
      {
        return genmeth.Resolve();
      }
      return mref;
    }


  }
}
