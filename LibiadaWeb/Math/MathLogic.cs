namespace LibiadaWeb.Math
{
    public static class MathLogic
    {
        public static bool NullableCompare(object first, object second)
        {
            if (first == null && second == null)
            {
                return true;
            }
            return first != null && first.Equals(second);
        } 
    }
}