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
        private Data PizzeriaData;
        public AnnulerReservationDialog(Data data)
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
            PizzeriaData = data;
        }

        private async Task<DialogTurnResult> ConfirmationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            var numReservation = (int)stepContext.Options;

            var promptMessage = MessageFactory.Text("Vous désirez annuler votre réservation?");
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage}, cancellationToken);

        }


        private async Task<DialogTurnResult> NumReservationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if(!(bool)stepContext.Result)
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                var numReservation = (int)stepContext.Options;
                if (numReservation == -1)
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
            
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var numReservation = (int)stepContext.Result;

            var numReservationExiste = PizzeriaData.ListeReservations.Where(_ => _.NumReservation == numReservation).ToList().Count > 0;

            if (!numReservationExiste)
            {
                await stepContext.Context.SendActivityAsync($"La réservation #{numReservation} ne figure pas à nos dossier. Veuillez recommencer ou appeler au restaurant.");
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"La réservation #{numReservation} a été annulée. Veuillez réutiliser notre assistant pour une nouvelle réservation!");
            }
            
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static Task<bool> NumReservationPromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 99 && promptContext.Recognized.Value < 1000);
        }
    }
}