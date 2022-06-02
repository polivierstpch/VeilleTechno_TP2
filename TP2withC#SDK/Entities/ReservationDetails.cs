using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TP2withSDK.Entities
{
    public class ReservationDetails
    {
        public DateTime Time { get; set; }
        public int NumberOfPlaces { get; set; }
        public Client Client { get; set; }
        public string Specification { get; set; }
        public string Notes { get; set; }
    }

    public class Client
    {
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
    }
}
