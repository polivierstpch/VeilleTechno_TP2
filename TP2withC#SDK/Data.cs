using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TP2withSDK.Entities;

namespace TP2withSDK
{
    public class Data
    {
        public List<ReservationDetails> ListeReservations = new List<ReservationDetails>()
        {
            new ReservationDetails()
            {
                NumReservation = 777
            }
        };

        public List<Commande> ListeCommandes = new List<Commande>()
        {
            new Commande()
            {
                NumCommande = 222
            }
        };
    }
}
