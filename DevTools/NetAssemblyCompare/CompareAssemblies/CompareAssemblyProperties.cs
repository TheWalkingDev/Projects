using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using XLerator.Helpers;

namespace CompareAssemblies
{
    public class TypeAndProperty
    {
        public Type Type { get; set; }
        public PropertyInfo Property { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}.[{3}]", Property.ScopeString(), Property.PropertyType, Type.Name, Property.ToString());
        }
    }

    public class TypeAndPropertyPair
    {
        public TypeAndProperty First { get; set; }
        public TypeAndProperty Second { get; set; }
    }

    public static class PropertyHelper
    {
        public static bool DifferentScope(this PropertyInfo a, PropertyInfo b)
        {
            if(a.CanRead == b.CanRead){
                if(a.GetGetMethod().DifferentScope(b.GetGetMethod())){
                    return true;
                }
            }
            if (a.CanWrite == b.CanWrite)
            {
                if (a.GetSetMethod().DifferentScope(b.GetSetMethod()))
                {
                    return true;
                }
            }
            return false;
        }

        public static string ScopeString(this PropertyInfo a)
        {
            var m1 = a.CanRead ? a.GetGetMethod() : (a.CanWrite ? a.GetSetMethod() : null);
            if (m1.NotNull())
            {
                return m1.ScopeString();
            }
            return "";
        }


    }

    public class CompareAssemblyProperties
    {
        public static void Compare(IResultLog Log, Assembly source, Assembly target)
        {
            var queryTM1 = GetTypesAndProperties(source);
            var queryTM2 = GetTypesAndProperties(target);

            var queryTMPossibleAdded =
            from tm1 in queryTM1
            select new TypeAndPropertyPair
            {
                First = tm1,
                Second = QueryRelatedTM(queryTM2, tm1)
            };

            var queryTMPossibleDeleted =
            from tm2 in queryTM2
            select new TypeAndPropertyPair
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
                        if (pair.First.Property.CanRead != pair.Second.Property.CanRead)
                        {
                            // one can read while other cannot
                            return true;
                        }

                        if (pair.First.Property.CanWrite != pair.Second.Property.CanWrite)
                        {
                            // one can write while other cannot
                            return true;
                        }

                        if (pair.First.Property.DifferentScope(pair.Second.Property))
                        {
                            return true;
                        }

                        if (pair.First.Property.CanRead)
                        {
                            var m1info = pair.First.Property.GetGetMethod();
                            var m2info = pair.Second.Property.GetGetMethod();

                            if (CompareMethodsDifferent(m1info, m2info))
                            {
                                return true;
                            }
                        }

                        if (pair.First.Property.CanWrite)
                        {
                            var m1info = pair.First.Property.GetSetMethod();
                            var m2info = pair.Second.Property.GetSetMethod();

                            if (CompareMethodsDifferent(m1info, m2info))
                            {
                                return true;
                            }
                        }

                        return false;
                    });

            // Outputs

            Log.Context = "Properties";

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

        private static TypeAndProperty QueryRelatedTM(IEnumerable<TypeAndProperty> queryTM, TypeAndProperty othertm)
        {
            return (from candidate in queryTM
                    where
                        othertm.Type.AssemblyQualifiedName == candidate.Type.AssemblyQualifiedName
                        &&
                        othertm.Property.ToString() == candidate.Property.ToString()
                    select candidate).FirstOrDefault();
        }

        private static IEnumerable<TypeAndProperty> GetTypesAndProperties(Assembly assembly)
        {
            var query1 =
                from type in assembly.GetTypes()
                from property in type.GetProperties()
                select new TypeAndProperty { Type = type, Property = property };
            return query1;
        }
    }
}
