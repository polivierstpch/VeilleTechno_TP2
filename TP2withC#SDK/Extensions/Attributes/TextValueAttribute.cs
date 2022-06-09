using System;

namespace TP2withSDK.Extensions.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TextValueAttribute : Attribute
    {
        public string Value { get; }

        public TextValueAttribute(string value)
        {
            Value = value;
        }
    }
}

