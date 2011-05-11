using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XLerator.Helpers;

namespace System
{
    public static class SystemHelpers
    {
        public static bool NotNull<ObjT>(this ObjT obj) where ObjT : class
        {
            return obj != null;
        }
        public static bool IsNull<ObjT>(this ObjT obj) where ObjT : class
        {
            return obj == null;
        }
        public static bool NotNull<ObjT>(this Nullable<ObjT> obj) where ObjT : struct
        {
            return obj != null;
        }
        public static bool IsNull<ObjT>(this Nullable<ObjT> obj) where ObjT : struct
        {
            return obj == null;
        }

        public static T ValueOrDefault<T>(this object obj, T defaultValue)
        {
            if (obj.NotNull())
            {
                return (T)obj;
            }
            return defaultValue;
        }

        public static bool IsSameAs(this object first, object second)
        {
            if (first == second)
            {
                return true;
            }
            if (first.IsNull())
            {
                return false;
            }
            return first.Equals(second);
        }

        public static bool NotSameAs(this object first, object second)
        {
            return first.IsSameAs(second).Not();
        }
    }
}
