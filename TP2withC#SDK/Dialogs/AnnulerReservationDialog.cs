using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.BotBuilderSamples.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TP2withSDK.Entities;

namespace TP2withSDK.Dialogs
{
    public class AnnulerReservationDialog : CancelAndHelpDialog
    {
        public AnnulerReservationDialog()
            : base(nameof(AnnulerReservationDialog))
        {
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), NumReservationPromptValidatorAsync));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ConfirmationStepAsync,
                NumReservationStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ConfirmationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var numReservation = (int)stepContext.Options;

            var promptMessage = MessageFactory.Text("Vous désirez annuler votre réservation?");
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage}, cancellationToken);

            //return await stepContext.NextAsync(numReservation, cancellationToken);
        }


        private async Task<DialogTurnResult> NumReservationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var numReservation = (int)stepContext.Options;
            if(numReservation == -1)
            {
                var promptMessage = MessageFactory.Text("Quel est votre numéro de commande (100 à 999)?", "Quel est votre numéro de commande (100 à 999)?");
                var retryPrompt = MessageFactory.Text("Veuillez inscrire un numéro de commande entre 100 et 999", "Veuillez inscrire un numéro de commande entre 100 et 999");
                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions { Prompt = promptMessage, RetryPrompt = retryPrompt }, cancellationToken);
            }
            else if (numReservation < 100 || numReservation > 999)
            {

                var promptMessage = MessageFactory.Text("Le numéro de commande demandé est invalide. Veuillez inscrire un numéro de commande entre 100 et 999", "Le numéro de commande demandé est invalide. Veuillez inscrire un numéro de commande entre 100 et 999");
                var retryPrompt = MessageFactory.Text("Veuillez entrer une nombre de personnes valide", "Veuillez entrer une nombre de personnes valide");
                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions { Prompt = promptMessage, RetryPrompt = retryPrompt }, cancellationToken);

            }

            return await stepContext.NextAsync(numReservation, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var numReservation = stepContext.Options;
            //var reservationDetails = (ReservationDetails)stepContext.Options;
            var reservationDetails = new ReservationDetails();
            reservationDetails.Client.PhoneNumber = (string)stepContext.Result;
            var messageFinal = String.Format("Merci pour votre réservation! Votre numéro de réservation est le {0}. Notez ce numéro si vous devez annuler votre réservation. Vos informations: \n Nombre de personnes: {1} \n Date: {2} \n Heure: {3} \n Nom: {4} \n Numéro de téléphone: {5}"
                , reservationDetails.NumReservation,
                reservationDetails.NumberOfPlaces,
                reservationDetails.Date,
                reservationDetails.Time,
                reservationDetails.Client.Name,
                reservationDetails.Client.PhoneNumber);
            await stepContext.Context.SendActivityAsync(messageFinal);

            return await stepContext.EndDialogAsync(reservationDetails, cancellationToken);
        }

        private static Task<bool> NumReservationPromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 99 && promptContext.Recognized.Value < 1000);
        }
    }
}