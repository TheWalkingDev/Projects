using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using XLerator.Helpers;

namespace CompareAssemblies
{
    public class TypeAndField
    {
        public Type Type { get; set; }
        public FieldInfo Field { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}.[{3}]", Field.ScopeString(), Field.FieldType, Type.Name, Field.ToString());
        }
    }

    public class TypeAndFieldPair
    {
        public TypeAndField First { get; set; }
        public TypeAndField Second { get; set; }
    }

    public static class FieldHelper
    {
        public static bool DifferentScope(this FieldInfo a, FieldInfo b)
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

        public static string ScopeString(this FieldInfo a)
        {
            var sb = new StringBuilder();
            if (a.IsPrivate) sb.Append("private ");
            if (a.IsPublic) sb.Append("public ");
            if (a.IsStatic) sb.Append("static ");
            return sb.ToString();
        }


    }

    public class CompareAssemblyFields
    {
        public static void Compare(IResultLog Log, Assembly source, Assembly target)
        {
            var queryTM1 = GetTypesAndFields(source);
            var queryTM2 = GetTypesAndFields(target);

            var queryTMPossibleAdded =
            from tm1 in queryTM1
            select new TypeAndFieldPair
            {
                First = tm1,
                Second = QueryRelatedTM(queryTM2, tm1)
            };

            var queryTMPossibleDeleted =
            from tm2 in queryTM2
            select new TypeAndFieldPair
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
                        if (pair.First.Field.DifferentScope(pair.Second.Field))
                        {
                            return true;
                        }
                        return pair.First.Field.FieldType.AssemblyQualifiedName
                                !=
                                pair.Second.Field.FieldType.AssemblyQualifiedName;
                    });

            // Outputs
            Log.Context = "Fields";

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
            //Console.WriteLine("Total NonNull: {0}", queryTMSNonNull.Count());
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

        private static TypeAndField QueryRelatedTM(IEnumerable<TypeAndField> queryTM, TypeAndField othertm)
        {
            return (from candidate in queryTM
                    where
                        othertm.Type.AssemblyQualifiedName == candidate.Type.AssemblyQualifiedName
                        &&
                        othertm.Field.ToString() == candidate.Field.ToString()
                    select candidate).FirstOrDefault();
        }

        private static IEnumerable<TypeAndField> GetTypesAndFields(Assembly assembly)
        {
            var query1 =
                from type in assembly.GetTypes()
                from field in type.GetFields()
                select new TypeAndField { Type = type, Field = field };
            return query1;
        }
    }
}
