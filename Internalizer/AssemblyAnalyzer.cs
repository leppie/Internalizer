using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Rocks;
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

      // todo: add overriden methods too
      return musages.DistinctBy(x => x.FullName).
        SelectMany(WithBaseMembers).DistinctBy(x => x.FullName);
    }

    static IEnumerable<MemberReference> WithBaseMembers(MemberReference arg)
    {
      yield return arg;

      var meth = arg as MethodDefinition;

      if (meth != null)
      {
        var nmeth = meth.GetBaseMethod();
        if (nmeth == meth)
        {
          yield break;
        }
        meth = nmeth;
      }

      // no hit in ironscheme... O_o
      while (meth != null)
      {
        if (arg.DeclaringType.Scope == meth.DeclaringType.Scope)
        {
          yield return meth;

          var nmeth = meth.GetBaseMethod();
          if (nmeth == meth)
          {
            yield break;
          }
          meth = nmeth;
        }
        else
        {
          yield break;
        }
      }
    }

    public static IEnumerable<TypeDefinition> AnalyzeTypes(AssemblyDefinition mainass, IEnumerable<AssemblyDefinition> depasses)
    {
      var tusages = from dep in depasses
                    from tref in dep.MainModule.GetTypeReferences()
                    where ((AssemblyNameReference)tref.Scope).FullName == mainass.Name.FullName
                    select tref.Resolve();

      return tusages.DistinctBy(x => x.FullName).
        SelectMany(WithBaseTypes).DistinctBy(x => x.FullName);
    }

    static IEnumerable<TypeDefinition> WithBaseTypes(TypeDefinition arg)
    {
      yield return arg;

      var scope = arg.Scope;

      if (arg.BaseType == null)
      {
        yield break;
      }

      arg = arg.BaseType.Resolve();

      while (arg != null)
      {
        var argr = arg.Resolve();
        if (argr.Scope == scope)
        {
          yield return argr;
          
          if (argr.BaseType == null)
          {
            yield break;
          }

          arg = argr.BaseType.Resolve();
        }
        else
        {
          yield break;
        }
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
