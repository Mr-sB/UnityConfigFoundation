using System;

namespace GameUtil.Config
{
    /// <summary>
    /// Add this Attribute to FieldConverter class convert methods.
    /// The method must be static, return type can not be void, and parameter is string.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class FieldConverterAttribute : Attribute
    {
    }
}