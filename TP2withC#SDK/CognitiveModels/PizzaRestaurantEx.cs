// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using TP2withSDK.Entities;
using TP2withSDK.Extensions;

namespace Microsoft.BotBuilderSamples
{
    //Extends the partial FlightBooking class with methods and properties that simplify accessing entities in the luis results
    public partial class PizzaRestaurant
    {
        public (int? NumberOfPlaces, string Date, string NumeroTelephoneClient, string NomClient, string Time) ReservationEntities
        {
            get
            {
                var numberOfPeopleValue = Entities?.Reservation?.FirstOrDefault()?.NumberOfPlaces?.FirstOrDefault();
                var dateValue = Entities?.Reservation?.FirstOrDefault()?.Date?.FirstOrDefault();
                var numTelClient = Entities?.Reservation?.FirstOrDefault()?.PhoneNumber?.FirstOrDefault();
                var timeValue = Entities?.Reservation?.FirstOrDefault()?.Time?.FirstOrDefault();
                var nameValue = Entities?.Reservation?.FirstOrDefault()?.Name?.FirstOrDefault();
                return (numberOfPeopleValue, dateValue, numTelClient, nameValue, timeValue);
            }
        }

        public (int? NumeroDeReservation, string RIEN) AnnulationEntities
        {
            get
            {
                var numReservation = Entities?.Annulation?.FirstOrDefault()?.NumeroDeReservation?.FirstOrDefault();
                return (numReservation, "");
            }
        }

        public CommandePizza PizzaOrderEntities
        {
            get
            {
                var type = Entities?.PizzaOrder?.FirstOrDefault()?.Type?.FirstOrDefault()?.FirstOrDefault();
                return new CommandePizza
                {
                    Client = new Client(),
                    Taille = Entities?.PizzaOrder?.FirstOrDefault()?.Size?.FirstOrDefault()?.FirstOrDefault()?.ToTaille() ?? default,
                    Type = type?.ToTypePizza() ?? default,
                    Croute = Entities?.PizzaOrder?.FirstOrDefault()?.Crust?.FirstOrDefault()?.FirstOrDefault()?.ToTypeCroute() ?? default,
                    Quantite = Entities?.PizzaOrder?.FirstOrDefault()?.Quanitity?.FirstOrDefault() ?? 0
                };
            }
        }
            
        public (int? NumeroCommande, string RIEN) OrderStatusEntities
        {
            get
            {
                var numReservation = Entities?.OrderStatus?.FirstOrDefault()?.NumeroCommande?.FirstOrDefault();
                return (numReservation, "");
            }
        }

        //public (string To, string Airport) ToEntities
        //{
        //    get
        //    {
        //        var toValue = Entities?._instance?.To?.FirstOrDefault()?.Text;
        //        var toAirportValue = Entities?.To?.FirstOrDefault()?.Airport?.FirstOrDefault()?.FirstOrDefault();
        //        return (toValue, toAirportValue);
        //    }
        //}

        // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
        // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
        //public string TravelDate
        //    => Entities.datetime?.FirstOrDefault()?.Expressions.FirstOrDefault()?.Split('T')[0];
    }
}
