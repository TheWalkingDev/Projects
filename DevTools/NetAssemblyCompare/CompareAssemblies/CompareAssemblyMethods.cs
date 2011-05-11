using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using XLerator.Helpers;

namespace CompareAssemblies
{
    public class TypeAndMethod
    {
        public Type Type { get; set; }
        public MethodInfo Method { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}.[{3}]", Method.ScopeString(), Method.ReturnType, Type.Name, Method.ToString());
        }
    }

    public class TypeAndMethodPair
    {
        public TypeAndMethod First { get; set; }
        public TypeAndMethod Second { get; set; }
    }

    public static class MethodHelper
    {
        public static bool DifferentScope(this MethodInfo a, MethodInfo b)
        {
            var same =
                (a.IsPrivate == b.IsPrivate)
                &&
                (a.IsPublic == b.IsPublic)
                &&
                (a.IsStatic == b.IsStatic)
                ;
            return same.IsFalse();
        }

        public static string ScopeString(this MethodInfo a)
        {
            var sb = new StringBuilder();
            if (a.IsPrivate) sb.Append("private ");
            if (a.IsPublic) sb.Append("public ");
            if (a.IsStatic) sb.Append("static ");
            return sb.ToString();
        }


    }

    public class CompareAssemblyMethods
    {
        public static void Compare(IResultLog Log, Assembly source, Assembly target)
        {
            var queryTM1 = GetTypesAndMethods(source);
            var queryTM2 = GetTypesAndMethods(target);

            var queryTMPossibleAdded =
            from tm1 in queryTM1
            select new TypeAndMethodPair
            {
                First = tm1,
                Second = QueryRelatedTM(queryTM2, tm1)
            };

            var queryTMPossibleDeleted =
            from tm2 in queryTM2
            select new TypeAndMethodPair
            {
                First = QueryRelatedTM(queryTM1, tm2),
                Second = tm2
            };

            var queryTMS =
            from pair in queryTMPossibleAdded.Union(queryTMPossibleDeleted).Distinct() select pair;

            var queryTMSNull = 
                from pair in queryTMS
                where
                    pair.First.IsNull()
                    ||
                    pair.Second.IsNull()
                select
                    pair;

            var queryTMSNonNull =
            from pair in queryTMS
            where
                pair.First.NotNull()
                &&
                pair.Second.NotNull()
            select
                pair;

            var queryDifferentBody =
                queryTMSNonNull
                .Where(pair =>
                    {
                        var m1info = pair.First.Method;
                        var m2info = pair.Second.Method;

                        if (m1info.DifferentScope(m2info))
                        {
                            return true;
                        }

                        return CompareMethodsDifferent(m1info, m2info);
                    });

            // Outputs

            Log.Context = "Methods";

            if (queryTMSNull.Count() > 0)
            {
                Log.WriteLine("Added or Deleted: {0}", queryTMSNull.Count());
                foreach (var pair in queryTMSNull)
                {
                    if (pair.First.IsNull())
                    {
                        Log.WriteLine("Added {0}", pair.Second);
                    }
                    if (pair.Second.IsNull())
                    {
                        Log.WriteLine("Deleted {0}", pair.First);
                    }
                }
            }

            if (queryDifferentBody.Count() > 0)
            {
                Log.WriteLine("Different: {0}", queryDifferentBody.Count());
                foreach (var pair in queryDifferentBody)
                {
                    Log.WriteLine("{0}", pair.First);
                }
            }
            //Log.WriteLine("Total NonNull: {0}", queryTMSNonNull.Count());
        }

        private static bool CompareMethodsDifferent(MethodInfo m1info, MethodInfo m2info)
        {
            var m1 = m1info.GetMethodBody();
            var m2 = m2info.GetMethodBody();
            if (m1 != null)
            {
                Debug.Assert(m2 != null);
                var m1body = m1.GetILAsByteArray();
                var m2body = m2.GetILAsByteArray();
                if (m1body.Length != m2body.Length)
                {
                    return true;
                }
                var r1set = m1body.ZIP(m2body, (a, b) => a == b);
                var r1setd = r1set.Distinct();
                var r1 = r1setd.Count() > 1;
                return r1;
            }
            return false;
        }

        private static TypeAndMethod QueryRelatedTM(IEnumerable<TypeAndMethod> queryTM, TypeAndMethod othertm)
        {
            return (from candidate in queryTM
                    where
                        othertm.Type.AssemblyQualifiedName == candidate.Type.AssemblyQualifiedName
                        &&
                        othertm.Method.ToString() == candidate.Method.ToString()
                    select candidate).FirstOrDefault();
        }

        private static IEnumerable<TypeAndMethod> GetTypesAndMethods(Assembly assembly)
        {
            var query1 =
                from type in assembly.GetTypes()
                from method in type.GetMethods()
                select new TypeAndMethod { Type = type, Method = method };
            return query1;
        }
    }
}
