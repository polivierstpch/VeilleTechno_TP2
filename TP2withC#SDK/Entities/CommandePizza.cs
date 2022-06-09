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
        public Client Client { get; set; }
        public TypePizza Type { get; set; }
        public TypeCroute Croute { get; set; }
        public Taille Taille { get; set; }
        public int Quantite { get; set; }
        public int NumCommande { get; set; }
    }
}
