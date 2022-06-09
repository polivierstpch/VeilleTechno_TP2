using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples;
using Microsoft.BotBuilderSamples.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using TP2withSDK.Entities;

namespace TP2withSDK.Dialogs
{
    public class AjoutReservationDialog : CancelAndHelpDialog
    {
        private const string PlacesStepMsgText = "Pour combien de personnes voulez-vous réserver?";
        private Data PizzeriaData;
        public AjoutReservationDialog(Data data)
            : base(nameof(AjoutReservationDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new TextPrompt("Date", DatePromptValidatorAsync));
            AddDialog(new TextPrompt("Time", TimePromptValidatorAsync));
            AddDialog(new TextPrompt("Numtel", NumTelPromptValidatorAsync));
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), PlacesPromptValidatorAsync));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NbPersonnesStepAsync,
                DateStepAsync,
                TimeStepAsync,
                NomStepAsync,
                NumTelStepAsync,
                ConfirmationStepAsync,
                FinalStepAsync,
            }));
            PizzeriaData = data;
            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> NbPersonnesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reservationDetails = (ReservationDetails)stepContext.Options;
            await stepContext.Context.SendActivityAsync("Bienvenue dans l'assistant de réservation!");
            if (reservationDetails.NumberOfPlaces <= 0)
            {

                var promptMessage = MessageFactory.Text(PlacesStepMsgText, PlacesStepMsgText);
                var retryPrompt = MessageFactory.Text("Veuillez entrer une nombre de personnes valide", "Veuillez entrer une nombre de personnes valide");
                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions { Prompt = promptMessage, RetryPrompt = retryPrompt }, cancellationToken);
    
            }

            return await stepContext.NextAsync(reservationDetails.NumberOfPlaces, cancellationToken);
        }

        private async Task<DialogTurnResult> DateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reservationDetails = (ReservationDetails)stepContext.Options;

            reservationDetails.NumberOfPlaces = (int)stepContext.Result;

            if (string.IsNullOrEmpty(reservationDetails.Date))
            {
                var promptMessage = MessageFactory.Text("À quelle date voulez-vous réserver?", "À quelle date voulez-vous réserver?", InputHints.ExpectingInput);
                var retryPrompt = MessageFactory.Text("Veuillez entrer une date valide", "Veuillez entrer une date valide");
                return await stepContext.PromptAsync("Date", new PromptOptions { Prompt = promptMessage, RetryPrompt = retryPrompt }, cancellationToken);
            }

            return await stepContext.NextAsync(reservationDetails.Date, cancellationToken);
        }

        private async Task<DialogTurnResult> TimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reservationDetails = (ReservationDetails)stepContext.Options;

            reservationDetails.Date = (string)stepContext.Result;

            if (string.IsNullOrEmpty(reservationDetails.Time))
            {
                var promptMessage = MessageFactory.Text("À quelle heure voulez-vous réserver (entrez 17h, 18h, 19h ou 20h)?", "À quelle heure voulez-vous réserver (entrez 17h, 18h, 19h ou 20h)?", InputHints.ExpectingInput);
                var retryPrompt = MessageFactory.Text("Choisissez entre 17h, 18h, 19h ou 20h)?", "Choisissez entre 17h, 18h, 19h ou 20h)?", InputHints.ExpectingInput);
                return await stepContext.PromptAsync("Time", new PromptOptions { Prompt = promptMessage, RetryPrompt = retryPrompt }, cancellationToken);
            }

            return await stepContext.NextAsync(reservationDetails.Time, cancellationToken);
        }

        private async Task<DialogTurnResult> NomStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reservationDetails = (ReservationDetails)stepContext.Options;

            reservationDetails.Time = (string)stepContext.Result;

            if (string.IsNullOrEmpty(reservationDetails.Client.Name))
            {
                var promptMessage = MessageFactory.Text("À quel nom voulez-vous réserver?", "À quel nom voulez-vous réserver?", InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(reservationDetails.Client.Name, cancellationToken);
        }

        private async Task<DialogTurnResult> NumTelStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reservationDetails = (ReservationDetails)stepContext.Options;

            reservationDetails.Client.Name = (string)stepContext.Result;

            if (string.IsNullOrEmpty(reservationDetails.Client.PhoneNumber))
            {
                var promptMessage = MessageFactory.Text("Quel est votre numéro de téléphone (ex. 555-555-5555)?", "Quel est votre numéro de téléphone (ex. 555-555-5555)?", InputHints.ExpectingInput);
                var retryPrompt = MessageFactory.Text("Entrez un format de numéro de téléphone valide (ex.555-555-5555)?", "Entrez un format de numéro de téléphone valide (ex.555-555-5555)?", InputHints.ExpectingInput);
                return await stepContext.PromptAsync("Numtel", new PromptOptions { Prompt = promptMessage, RetryPrompt = retryPrompt }, cancellationToken);
            }

            return await stepContext.NextAsync(reservationDetails.Client.PhoneNumber, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reservationDetails = (ReservationDetails)stepContext.Options;
            reservationDetails.Client.PhoneNumber = (string)stepContext.Result;
            var message = String.Format("Vos informations de réservation sont les suivantes: \n\n Nombre de personnes: {0} \n\n Date: {1} \n\n Heure: {2} \n\n Nom: {3} \n\n Numéro de téléphone: {4}. Souhaitez-vous procéder à la réservation?",
                reservationDetails.NumberOfPlaces,
                reservationDetails.Date,
                reservationDetails.Time,
                reservationDetails.Client.Name,
                reservationDetails.Client.PhoneNumber);
            var promptMessage = MessageFactory.Text(message, message, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reservationDetails = (ReservationDetails)stepContext.Options;
            if (!(bool)stepContext.Result)
            {
                await stepContext.Context.SendActivityAsync("La réservation n'a pas été ajoutée. Veuillez recommencer.");
            }
            else
            {
                reservationDetails.NumReservation = GenerateNumReservation();
                PizzeriaData.ListeReservations.Add(reservationDetails);
                var messageFinal = String.Format($"Merci pour votre réservation! Votre numéro de réservation est le {reservationDetails.NumReservation}. Notez ce numéro si vous devez annuler votre réservation.");
                await stepContext.Context.SendActivityAsync(messageFinal);
            }
            return await stepContext.EndDialogAsync(reservationDetails, cancellationToken);
        }

        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }

        private static Task<bool> PlacesPromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 && promptContext.Recognized.Value < 20);
        }

        private static Task<bool> DatePromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            bool value;
            try
            {
                Convert.ToDateTime(promptContext.Recognized.Value, new CultureInfo("fr-ca"));
                value = true;
            }
            catch
            {
                value = false;
            }
            //var value = DateTime.TryParse(promptContext.Recognized.Value, out DateTime result);
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && value);
        }

        private static Task<bool> TimePromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valide = promptContext.Recognized.Value.ToLower() == "17h" || promptContext.Recognized.Value.ToLower() == "18h" || promptContext.Recognized.Value.ToLower() == "19h" || promptContext.Recognized.Value.ToLower() == "20h";
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && valide );
        }

        private static Task<bool> NumTelPromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var regex = new Regex(@"([1]( |-))?[0-9]{3}( |-)[0-9]{3}-[0-9]{4}");
            var valide = regex.IsMatch(promptContext.Recognized.Value);
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && valide);
        }

        private static int GenerateNumReservation()
        {
            var rd = new Random();
            return rd.Next(100, 999);
        }
    }
}
