using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.BotBuilderSamples.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TP2withSDK.Dialogs
{
    public class SatisfactionDialog : CancelAndHelpDialog
    {
        private Data PizzeriaData;
        public SatisfactionDialog(Data data)
            : base(nameof(SatisfactionDialog))
        {
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), NumReservationPromptValidatorAsync));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ConfirmationStepAsync,
                NoteStepAsync,
                CommentaireStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
            PizzeriaData = data;
        }

        private async Task<DialogTurnResult> ConfirmationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var promptMessage = MessageFactory.Text("Vous désirez partager votre expérience chez PizzaTech?");
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);

        }


        private async Task<DialogTurnResult> NoteStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if (!(bool)stepContext.Result)
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                var promptMessage = MessageFactory.Text("Quel est votre niveau de satisfaction de 1 (insatisfait) à 10 (excellent)?", "Quel est votre niveau de satisfaction de 1 (insatisfait) à 10 (excellent)?");
                var retryPrompt = MessageFactory.Text("Inscrire une note entre 1 et 10", "Inscrire une note entre 1 et 10");
                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions { Prompt = promptMessage, RetryPrompt = retryPrompt }, cancellationToken);
            }

        }

        private async Task<DialogTurnResult> CommentaireStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var promptMessage = MessageFactory.Text("Votre opinion compte. Laissez nous un commentaire!", "Votre opinion compte. Laissez nous un commentaire!");
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);

        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            await stepContext.Context.SendActivityAsync($"Merci pour votre commentaire. PizzaTech travaille constamment à offrir la meillere expérience à ses clients.");

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static Task<bool> NumReservationPromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 && promptContext.Recognized.Value < 11);
        }
    }
}
