using System;

namespace rz_frzbn.Singletons.utils
{
    public static class EnumToString{
        public static string makeString(this Enum e){
            return Enum.GetName(e.GetType(), e);
        }
    }

}