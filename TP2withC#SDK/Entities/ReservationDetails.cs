using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TP2withSDK.Entities
{
    public class ReservationDetails
    {
        public int NumReservation { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public int NumberOfPlaces { get; set; }
        public Client Client { get; set; }
    }
}
