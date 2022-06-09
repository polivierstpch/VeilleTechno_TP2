using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Dialogs;
using TP2withSDK.Entities;
using TP2withSDK.Extensions;

namespace TP2withSDK.Dialogs
{
    public class CommandeDialog : CancelAndHelpDialog
    {
        private const string NumTelPromptId = "NumTelPrompt";
        
        private readonly Data _pizzeriaData;
        
        private readonly List<string> _pizzaTypeChoices = EnumHelper.GetAllTextValues<TypePizza>(startIndex: 1);
        private readonly List<string> _crustTypeChoices = EnumHelper.GetAllTextValues<TypeCroute>(startIndex: 1);
        private readonly List<string> _sizeChoices = EnumHelper.GetAllTextValues<Taille>(startIndex: 1);

        public CommandeDialog(Data pizzeriaData) : base(nameof(CommandeDialog))
        {
            _pizzeriaData = pizzeriaData;
            
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new TextPrompt(NumTelPromptId, NumTelPromptValidatorAsync));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), QuantityPromptValidatorAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                TypeChoiceStepAsync,
                SizeChoiceStepAsync,
                CrustChoiceStepAsync,
                QuantityStepAsync,
                NameInputStepAsync,
                PhoneNumberStepAsync,
                ValidationCommandeStepAsync,
                FinalStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> TypeChoiceStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var commandePizza = (CommandePizza)stepContext.Options;

            if (commandePizza.Type == TypePizza.Aucune)
            {
                var choixPizza = string.Join("\n\n - ", _pizzaTypeChoices);
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text($"Voici le menu de pizza : \n\n - {choixPizza}"),
                    cancellationToken);
                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("Veuillez choisir votre type de pizza :"),
                    RetryPrompt = MessageFactory.Text("Veuillez choisir un type de pizza dans le menu."),
                    Choices = ChoiceFactory.ToChoices(_pizzaTypeChoices),
                }, cancellationToken);
            }

            var typeFoundChoice = new FoundChoice { Value = commandePizza.Type.GetTextValue() };
            return await stepContext.NextAsync(typeFoundChoice, cancellationToken);
        }
        
        private async Task<DialogTurnResult> SizeChoiceStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var commandePizza = (CommandePizza)stepContext.Options;
            commandePizza.Type = stepContext.Result is FoundChoice choice
                ? choice.Value.ToTypePizza()
                : (TypePizza)stepContext.Result;

            
            if (commandePizza.Taille == Taille.Aucune)
            {
                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("Veuillez choisir une taille de pizza :"),
                    RetryPrompt = MessageFactory.Text("Veuillez choisir une taille valide."),
                    Choices = ChoiceFactory.ToChoices(_sizeChoices),
                }, cancellationToken);
            }
            
            return await stepContext.NextAsync(commandePizza.Taille, cancellationToken);
        }

        private async Task<DialogTurnResult> CrustChoiceStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var commandePizza = (CommandePizza)stepContext.Options;
            commandePizza.Taille = stepContext.Result is FoundChoice choice
                ? choice.Value.ToTaille()
                : (Taille)stepContext.Result;
            
            if (commandePizza.Croute == TypeCroute.Aucune)
            {
                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("Veuillez choisir votre croûte de pizza :"),
                    RetryPrompt = MessageFactory.Text("Veuillez choisir une crôute valide."),
                    Choices = ChoiceFactory.ToChoices(_crustTypeChoices)
                }, cancellationToken);
            }

            return await stepContext.NextAsync(commandePizza.Croute, cancellationToken);
        }

        private async Task<DialogTurnResult> QuantityStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var commandePizza = (CommandePizza)stepContext.Options;
            commandePizza.Croute = stepContext.Result is FoundChoice choice
                ? choice.Value.ToTypeCroute()
                : (TypeCroute)stepContext.Result;
            
            if (commandePizza.Quantite <= 0)
            {
                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions
                {
                    Prompt = MessageFactory.Text("Combien de pizza(s) voulez-vous ?"),
                    RetryPrompt = MessageFactory.Text("Veuillez entrer un nombre de pizza entre 1 et 10.\n\n" +
                                                      "Pour assurer la fraîcheur maximale des produits, nous limitons le nombre de pizza à 10."),
                    Choices = ChoiceFactory.ToChoices(_crustTypeChoices)
                }, cancellationToken);
            }
            
            return await stepContext.NextAsync(commandePizza.Quantite, cancellationToken);
        }

        private async Task<DialogTurnResult> NameInputStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var commandePizza = (CommandePizza)stepContext.Options;
            commandePizza.Quantite = (int)stepContext.Result;

            var promptMessage = MessageFactory.Text("Puis-je s'il-vous plaît avoir votre nom? C'est pour mieux vous servir!");
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }
        
        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var commandePizza = (CommandePizza)stepContext.Options;
            commandePizza.Client.Name = (string)stepContext.Result;

            var promptMessage = MessageFactory.Text($"Merci {commandePizza.Client.Name.Split(" ").First()}!" +
                                                    " Il me faudrait ensuite votre numéro de téléphone, s'il-vous-plaît!");
            var retryMessage =
                MessageFactory.Text("Veuillez entrer un numéro de téléphone valide! (ex.: 555-555-5555)");
            return await stepContext.PromptAsync(NumTelPromptId, new PromptOptions
            {
                Prompt = promptMessage, 
                RetryPrompt = retryMessage
            }, cancellationToken);
        }
        
        private async Task<DialogTurnResult> ValidationCommandeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var commandePizza = (CommandePizza)stepContext.Options;
            commandePizza.Client.PhoneNumber = (string)stepContext.Result;

            var sb = new StringBuilder();

            sb.AppendLine("Vos informations de réservation sont les suivantes: \n");
            sb.AppendLine($" - Votre choix de pizza : {commandePizza.Type.GetTextValue()} \n");
            sb.AppendLine($" - Votre grandeur de pizza : {commandePizza.Taille.GetTextValue()} \n");
            sb.AppendLine($" - Votre croûte de pizza : {commandePizza.Croute.GetTextValue()} \n");
            sb.AppendLine($" - Quantité demandée : {commandePizza.Quantite} \n\n");
            sb.AppendLine("Renseignements fournis : \n");
            sb.AppendLine($" - Nom : {commandePizza.Client.Name} \n" );
            sb.AppendLine($" - Numéro de téléphone: {commandePizza.Client.PhoneNumber}");

            await stepContext.Context.SendActivityAsync(sb.ToString(), cancellationToken: cancellationToken);
            
            var promptMessage = MessageFactory.Text("Ces informations sont-elles correctes?", inputHint: InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var commandePizza = (CommandePizza)stepContext.Options;
            
            if (!(bool)stepContext.Result)
            {
                await stepContext.Context.SendActivityAsync("La commande à été annulée. Merci d'avoir utilisé le service de commande!", cancellationToken: cancellationToken);
            }
            else
            {
                commandePizza.NumCommande = GenerateCommandNumber();
                _pizzeriaData.ListeCommandes.Add(commandePizza);
                var messageFinal = MessageFactory.Text($"Merci {commandePizza.Client.Name.Split(" ").First()}!" + 
                                                       $" Votre numéro de commande est le #{commandePizza.NumCommande}! Veuillez le prendre en note!");
                await stepContext.Context.SendActivityAsync(messageFinal, cancellationToken);
            }
            return await stepContext.EndDialogAsync(commandePizza, cancellationToken);
        }

        private static Task<bool> QuantityPromptValidatorAsync(PromptValidatorContext<int> promptContext,
            CancellationToken cancellationToken)
        {
            var isValid = promptContext.Recognized.Succeeded &&
                          promptContext.Recognized.Value is > 0 and <= 10;
            return Task.FromResult(isValid);
        }
        
        private static Task<bool> NumTelPromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var regex = new Regex(@"([1]( |-))?[0-9]{3}( |-)[0-9]{3}-[0-9]{4}");
            var valide = regex.IsMatch(promptContext.Recognized.Value);
            return Task.FromResult(promptContext.Recognized.Succeeded && valide);
        }
        
        private static int GenerateCommandNumber()
        {
            var rd = new Random();
            return rd.Next(100, 999);
        }
    }
}

