using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XLerator.Helpers
{
    public static class BoolHelpers
    {
        #region Simple True/False tests
        public static bool IsTrue(this bool obj)
        {
            return obj;
        }

        public static bool IsFalse(this bool obj)
        {
            return obj == false;
        }

        public static bool NotTrue(this bool obj)
        {
            return obj == false;
        }

        public static bool NotFalse(this bool obj)
        {
            return obj == true;
        }

        public static bool Not(this bool obj)
        {
            return !obj;
        }
        #endregion

        #region Actions on true, false, null, notNull
        public static void IfTrue(this bool value, Action action)
        {
            if (value)
            {
                action();
            }
        }

        public static void IfFalse(this bool value, Action action)
        {
            if (value == false)
            {
                action();
            }
        }

        public static void If(this bool value, Action actionIfTrue, Action actionIfFalse)
        {
            if (value)
            {
                actionIfTrue();
            } else {
                actionIfFalse();
            }
        }

        #region Null classes
        public static void IfNull<ObjT>(this ObjT value, Action action)
            where ObjT : class
        {
            if (value.IsNull())
            {
                action();
            }
        }

        public static void IfNotNull<ObjT>(this ObjT value, Action action)
            where ObjT : class
        {
            if (value.NotNull())
            {
                action();
            }
        }

        public static void IfNull<ObjT>(this ObjT value, Action actionIfNull, Action actionIfNotNull)
            where ObjT : class
        {
            if (value.IsNull())
            {
                actionIfNull();
            }
            else
            {
                actionIfNotNull();
            }
        }

        public static void IfNotNull<ObjT>(this ObjT value, Action actionIfNotNull, Action actionIfNull)
            where ObjT : class
        {
            if (value.NotNull())
            {
                actionIfNotNull();
            }
            else
            {
                actionIfNull();
            }
        }
        #endregion

        #region Null nullables

        public static void IfNull<ObjT>(this Nullable<ObjT> value, Action action)
            where ObjT : struct
        {
            if (value.IsNull())
            {
                action();
            }
        }

        public static void IfNotNull<ObjT>(this Nullable<ObjT> value, Action action)
            where ObjT : struct
        {
            if (value.NotNull())
            {
                action();
            }
        }

        public static void IfNull<ObjT>(this Nullable<ObjT> value, Action actionIfNull, Action actionIfNotNull)
            where ObjT : struct
        {
            if (value.IsNull())
            {
                actionIfNull();
            }
            else
            {
                actionIfNotNull();
            }
        }

        public static void IfNotNull<ObjT>(this Nullable<ObjT> value, Action actionIfNotNull, Action actionIfNull)
            where ObjT : struct
        {
            if (value.NotNull())
            {
                actionIfNotNull();
            }
            else
            {
                actionIfNull();
            }
        }
        #endregion
        #endregion


        #region Funcs on true, false, null, notNull
        public static T IfTrue<T>(this bool value, Func<T> action)
        {
            if (value)
            {
                return action();
            }
            return default(T);
        }

        public static T IfFalse<T>(this bool value, Func<T> action)
        {
            if (value == false)
            {
                return action();
            }
            return default(T);
        }

        public static T If<T>(this bool value, Func<T> actionIfTrue, Func<T> actionIfFalse)
        {
            if (value)
            {
                return actionIfTrue();
            }
            else
            {
                return actionIfFalse();
            }
        }

        #region Null classes

        public static T IfNull<ObjT, T>(this ObjT value, Func<T> action)
            where ObjT : class
        {
            if (value.IsNull())
            {
                return action();
            }
            return default(T);
        }

        public static T IfNotNull<ObjT, T>(this ObjT value, Func<T> action)
            where ObjT : class
        {
            if (value.NotNull())
            {
                action();
            }
            return default(T);
        }

        public static T IfNull<ObjT, T>(this ObjT value, Func<T> actionIfNull, Func<T> actionIfNotNull)
            where ObjT : class
        {
            if (value.IsNull())
            {
                return actionIfNull();
            }
            else
            {
                return actionIfNotNull();
            }
        }

        public static T IfNotNull<ObjT, T>(this ObjT value, Func<T> actionIfNotNull, Func<T> actionIfNull)
            where ObjT : class
        {
            if (value.NotNull())
            {
                return actionIfNotNull();
            }
            else
            {
                return actionIfNull();
            }
        }

        #endregion

        #region Null Nullables

        public static T IfNull<ObjT, T>(this Nullable<ObjT> value, Func<T> action)
            where ObjT : struct
        {
            if (value.IsNull())
            {
                return action();
            }
            return default(T);
        }

        public static T IfNotNull<ObjT, T>(this Nullable<ObjT> value, Func<T> action)
            where ObjT : struct
        {
            if (value.NotNull())
            {
                action();
            }
            return default(T);
        }

        public static T IfNull<ObjT, T>(this Nullable<ObjT> value, Func<T> actionIfNull, Func<T> actionIfNotNull)
            where ObjT : struct
        {
            if (value.IsNull())
            {
                return actionIfNull();
            }
            else
            {
                return actionIfNotNull();
            }
        }

        public static T IfNotNull<ObjT, T>(this Nullable<ObjT> value, Func<T> actionIfNotNull, Func<T> actionIfNull)
            where ObjT : struct
        {
            if (value.NotNull())
            {
                return actionIfNotNull();
            }
            else
            {
                return actionIfNull();
            }
        }

        #endregion

        #endregion

    }
}
