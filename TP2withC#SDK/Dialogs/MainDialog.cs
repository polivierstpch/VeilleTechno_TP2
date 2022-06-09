// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using TP2withSDK.Dialogs;
using TP2withSDK.Entities;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly PizzaRestaurantRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(PizzaRestaurantRecognizer luisRecognizer, AjoutReservationDialog reservationDialog, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(reservationDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? $"Bienvenue chez TechPizza! Que voulez-vous faire aujourd'hui? Vous pouvez réserver en salle, modifier/annuler une réservation, faire une commande en ligne, connaître le status de votre commande ou donner votre avis.";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                // LUIS is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
                //return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
            }

            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            var luisResult = await _luisRecognizer.RecognizeAsync<PizzaRestaurant>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case PizzaRestaurant.Intent.Reserver:
                    var reservation = luisResult.ReservationEntities;
                    var reservationDetails = new ReservationDetails()
                    {
                        NumberOfPlaces = reservation.NumberOfPlaces > 0 ? reservation.NumberOfPlaces.Value : -1,
                        Date = reservation.Date,
                        Client = new Client() { PhoneNumber = reservation.NumeroTelephoneClient, Name = reservation.NomClient },
                        Time = reservation.Time
                    };

                    return await stepContext.BeginDialogAsync(nameof(AjoutReservationDialog), reservationDetails, cancellationToken);

                //case PizzaRestaurant.Intent.AnnulerReservation:
                //    var numReservation = luisResult.AnnulationEntities.NumeroDeReservation;
                //    return await stepContext.BeginDialogAsync(nameof(AnnulerReservationDialog), numReservation, cancellationToken);

                case PizzaRestaurant.Intent.StatusCommande:
                    var numReservation = luisResult.AnnulationEntities.NumeroDeReservation;
                    return await stepContext.BeginDialogAsync(nameof(OrderStatusDialog), numReservation, cancellationToken);

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Désolé, je n'ai pas compris ce que vous voulez faire (intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        // Shows a warning if the requested From or To cities are recognized as entities but they are not in the Airport entity list.
        // In some cases LUIS will recognize the From and To composite entities as a valid cities but the From and To Airport values
        // will be empty if those entity values can't be mapped to a canonical item in the Airport.
        //private static async Task ShowWarningForUnsupportedCities(ITurnContext context, PizzaRestaurant luisResult, CancellationToken cancellationToken)
        //{
        //    var unsupportedCities = new List<string>();

        //    var fromEntities = luisResult.FromEntities;
        //    if (!string.IsNullOrEmpty(fromEntities.From) && string.IsNullOrEmpty(fromEntities.Airport))
        //    {
        //        unsupportedCities.Add(fromEntities.From);
        //    }

        //    var toEntities = luisResult.ToEntities;
        //    if (!string.IsNullOrEmpty(toEntities.To) && string.IsNullOrEmpty(toEntities.Airport))
        //    {
        //        unsupportedCities.Add(toEntities.To);
        //    }

        //    if (unsupportedCities.Any())
        //    {
        //        var messageText = $"Sorry but the following airports are not supported: {string.Join(',', unsupportedCities)}";
        //        var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
        //        await context.SendActivityAsync(message, cancellationToken);
        //    }
        //}

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            // the Result here will be null.
            //if (stepContext.Result is BookingDetails result)
            //{
            //    // Now we have all the booking details call the booking service.

            //    // If the call to the booking service was successful tell the user.

            //    var timeProperty = new TimexProperty(result.TravelDate);
            //    var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
            //    var messageText = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
            //    var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            //    await stepContext.Context.SendActivityAsync(message, cancellationToken);
            //}

            // Restart the main dialog with a different message the second time around
            var promptMessage = "Que puis-je faire d'autre pour vous?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }
    }
}
