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
    public class OrderStatusDialog : CancelAndHelpDialog
    {
        public OrderStatusDialog() 
            : base(nameof(OrderStatusDialog))
        {
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), OrderNumberPromptValidatorAsync));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
{
                OrderNumberPromptAsync,
                FinalStepAsync,
            }));
        }

        private async Task<DialogTurnResult> OrderNumberPromptAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if (!(bool)stepContext.Result)
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                var orderNumber = (int)stepContext.Options;
                if (orderNumber == -1)
                {
                    var promptMessage = MessageFactory.Text("Quel est votre numéro de commande (100 à 999)?", "Quel est votre numéro de commande (100 à 999)?");
                    var retryPrompt = MessageFactory.Text("Veuillez inscrire un numéro de commande entre 100 et 999", "Veuillez inscrire un numéro de commande entre 100 et 999");
                    return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions { Prompt = promptMessage, RetryPrompt = retryPrompt }, cancellationToken);
                }
                else if (orderNumber < 100 || orderNumber > 999)
                {

                    var promptMessage = MessageFactory.Text("Le numéro de commande demandé est invalide. Veuillez inscrire un numéro de commande entre 100 et 999", "Le numéro de commande demandé est invalide. Veuillez inscrire un numéro de commande entre 100 et 999");
                    var retryPrompt = MessageFactory.Text("Veuillez inscrire un numéro de commande entre 100 et 999", "Veuillez inscrire un numéro de commande entre 100 et 999");
                    return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions { Prompt = promptMessage, RetryPrompt = retryPrompt }, cancellationToken);

                }

                return await stepContext.NextAsync(orderNumber, cancellationToken);
            }

        }


        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var orderNumber = (int)stepContext.Result;

            var existingOrderNumber = PizzeriaData.ListeCommandes.Where(_ => _.NumCommande == orderNumber).ToList().Count > 0;

            if (!existingOrderNumber)
            {
                await stepContext.Context.SendActivityAsync($"La commande #{orderNumber} ne figure pas à nos dossier. Veuillez recommencer ou appeler au restaurant.");
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"La commande #{orderNumber} a été annulée. Veuillez réutiliser notre assistant pour une nouvelle réservation!");
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }


        private static Task<bool> OrderNumberPromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 99 && promptContext.Recognized.Value < 1000);
        }
    }
}
