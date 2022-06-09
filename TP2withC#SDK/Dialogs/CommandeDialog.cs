using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.BotBuilderSamples.Dialogs;
using TP2withSDK.Entities;
using TP2withSDK.Extensions;

namespace TP2withSDK.Dialogs
{
    public class CommandeDialog : CancelAndHelpDialog
    {
        private const string TypeChoiceId = "PizzaTypeChoice";
        private const string CrustChoiceId = "PizzaCrustChoice";
        private const string SizeChoiceId = "PizzaSizeChoice";

        private List<string> _pizzaTypeChoices = EnumHelper.GetAllTextValues<TypePizza>(startIndex: 1);
        private List<string> _crustTypeChoices = EnumHelper.GetAllTextValues<TypeCroute>(startIndex: 1);
        private List<string> _sizeChoices = EnumHelper.GetAllTextValues<Taille>(startIndex: 1);

        public CommandeDialog() : base(nameof(CommandeDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), QuantityPromptValidatorAsync));
            AddDialog(new ChoicePrompt(TypeChoiceId));
            AddDialog(new ChoicePrompt(CrustChoiceId));
            AddDialog(new ChoicePrompt(SizeChoiceId));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
            SizeChoiceStepAsync,
            FinalStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SizeChoiceStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var commandePizza = (CommandePizza)stepContext.Options;

            if (commandePizza.Taille == Taille.Aucune)
            {
                return await stepContext.PromptAsync(SizeChoiceId, new PromptOptions
                {
                    Prompt = MessageFactory.Text("Veuillez choisir une taille de pizza :"),
                    RetryPrompt = MessageFactory.Text("Veuillez choisir une taille valide."),
                    Choices = ChoiceFactory.ToChoices(_sizeChoices),
                }, cancellationToken);
            }

            return await stepContext.NextAsync(commandePizza.Taille, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var numReservation = stepContext.Result;
            await stepContext.Context.SendActivityAsync($"La réservation #{numReservation} a été annulée. Veuillez réutiliser notre assistant pour une nouvelle réservation!");

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static Task<bool> QuantityPromptValidatorAsync(PromptValidatorContext<int> promptContext,
            CancellationToken cancellationToken)
        {
            var isValid = promptContext.Recognized.Succeeded &&
                          promptContext.Recognized.Value is > 0 and < 10;
            return Task.FromResult(isValid);
        }
    }
}

