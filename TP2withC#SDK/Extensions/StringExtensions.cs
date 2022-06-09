using System;
using System.Collections.Generic;
using TP2withSDK.Entities;

namespace TP2withSDK.Extensions
{
    public static class StringExtensions
    {
        private static readonly Dictionary<string, Taille> StringToTailleMap = new()
        {
            { Taille.Petit.GetTextValue().ToLower(), Taille.Petit },
            { Taille.Regulier.GetTextValue().ToLower(), Taille.Regulier },
            { Taille.Large.GetTextValue().ToLower(), Taille.Large },
            { Taille.ExtraLarge.GetTextValue().ToLower(), Taille.ExtraLarge }
        };

        private static readonly Dictionary<string, TypePizza> StringToTypePizzaMap = new()
        {
            { TypePizza.PepperoniFromage.GetTextValue().ToLower(), TypePizza.PepperoniFromage },
            { TypePizza.Garnie.GetTextValue().ToLower(), TypePizza.Garnie },
            { TypePizza.Hawaienne.GetTextValue().ToLower(), TypePizza.Hawaienne },
            { TypePizza.PouletBarbecue.GetTextValue().ToLower(), TypePizza.PouletBarbecue },
            { TypePizza.Vegetarienne.GetTextValue().ToLower(), TypePizza.Vegetarienne }
        };

        private static readonly Dictionary<string, TypeCroute> StringToTypeCrouteMap = new()
        {
            { TypeCroute.Mince.GetTextValue(), TypeCroute.Mince },
            { TypeCroute.Reguliere.GetTextValue(), TypeCroute.Reguliere },
            { TypeCroute.Farcie.GetTextValue(), TypeCroute.Farcie }
        };

        public static Taille ToTaille(this string value)
        {
            if (string.IsNullOrEmpty(value.Trim()))
                return Taille.Aucune;

            if (Enum.TryParse<Taille>(value, out var enumValue))
                return enumValue;

            return !StringToTailleMap.ContainsKey(value.ToLower()) ? Taille.Aucune : StringToTailleMap[value];
        }

        public static TypePizza ToTypePizza(this string value)
        {
            if (string.IsNullOrEmpty(value.Trim()))
                return default;

            if (Enum.TryParse<TypePizza>(value, out var enumValue))
                return enumValue;

            return !StringToTailleMap.ContainsKey(value.ToLower()) ? default : StringToTypePizzaMap[value];
        }

        public static TypeCroute ToTypeCroute(this string value)
        {
            if (string.IsNullOrEmpty(value.Trim()))
                return default;

            if (Enum.TryParse<TypeCroute>(value, out var enumValue))
                return enumValue;

            return !StringToTailleMap.ContainsKey(value.ToLower()) ? default : StringToTypeCrouteMap[value];
        }
    }
}

