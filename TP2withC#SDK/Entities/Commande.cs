using TP2withSDK.Extensions.Attributes;

namespace TP2withSDK.Entities
{
    public enum TypePizza 
    {
        Aucune,
        [TextValue("Pepperoni Fromage")]
        PepperoniFromage,
        Garnie,
        [TextValue("Poulet Barbecue")]
        PouletBarbecue,
        [TextValue("Végétarienne")]
        Vegetarienne,
        [TextValue("Hawaïenne")]
        Hawaienne
    }

    public enum TypeCroute
    {
        Aucune,
        Mince,
        [TextValue("Regulière")]
        Reguliere,
        Farcie
    }

    public enum Taille
    {
        Aucune,
        Petit,
        [TextValue("Régulier")]
        Regulier,
        Large,
        [TextValue("Extra Large")]
        ExtraLarge
    }
    
    public class CommandePizza
    {
        public TypePizza Type;
        public TypeCroute Croute;
        public Taille Taille;
        public int Quantite;
        public int NumCommande { get; set; }
    }
}
