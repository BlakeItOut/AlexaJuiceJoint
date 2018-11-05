using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using System;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace alexaJuiceJointDetroit
{
    public class Function
    {
        public static SmoothieResource GetResource()
        {
            SmoothieResource enUSResource = new SmoothieResource("en-US");
            enUSResource.SkillName = "Juice Joint";
            enUSResource.HelpMessage = "You can ask me for the names of the smoothies, ask about their ingredients, or even search for a smoothie by ingredients. If you want to exit, just say exit...What can I help you with ?";
            enUSResource.HelpReprompt = "What can I help you with?";
            enUSResource.StopMessage = "Goodbye!";
            enUSResource.LaunchMessage = "Welcome to Juice Joint. I know the smoothies and their ingredients at the Juice Joint. What would you like to know?";
            enUSResource.LaunchMessageReprompt = "Try asking me to tell you the smoothies.";
            enUSResource.AskMessage = " What else would you like to know?";
            enUSResource.Smoothies = new Dictionary<string, Smoothie>();
            enUSResource.Smoothies.Add("great gonzo", new Smoothie(new string[] { "blueberry", "pineapple", "ginger", "banana", "orange", "lemon", "coconut water" }, "great gonzo"));
            enUSResource.Smoothies.Add("maui waui", new Smoothie(new string[] { "avocado", "lime", "pineapple", "baby greens", "mango", "cilantro", "cayenne", "coconut water" }, "maui waui"));
            enUSResource.Smoothies.Add("atomic energy", new Smoothie(new string[] { "mango", "carrot", "tumeric", "ginger", "pineapple", "orange", "banana", "lemon", "coconut water" }, "atomic energy"));
            enUSResource.Smoothies.Add("sweetart", new Smoothie(new string[] { "blueberry", "raspberry", "strawberry", "orange", "kiwi", "banana", "coconut water", "honey" }, "sweetart"));
            enUSResource.Smoothies.Add("tutti-frutti", new Smoothie(new string[] { "strawberry", "banana", "pineapple", "raspberry" }, "tutti frutti"));
            enUSResource.Smoothies.Add("jungle juice", new Smoothie(new string[] { "pineapple", "mango", "baby greens", "banana", "coconut water" }, "jungle juice"));
            enUSResource.Smoothies.Add("the boss", new Smoothie(new string[] { "matcha", "mango", "avocado", "baby greens", "banana", "almond milk", "agave" }, "the boss"));
            enUSResource.Smoothies.Add("blue berry yum yum", new Smoothie(new string[] { "blueberry", "banana", "almond butter", "almond milk", "honey" }, "blue berry yum yum"));
            return enUSResource;
        }

        #region Conversation
        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="input"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            SkillResponse response = new SkillResponse();
            response.Response = new ResponseBody();
            response.Response.ShouldEndSession = false;
            IOutputSpeech innerResponse = null;
            var log = context.Logger;
            log.LogLine($"Skill Request Object:");
            log.LogLine(JsonConvert.SerializeObject(input));

            var resource = GetResource();

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"Default Launch made: Alexa, open Juice Joint");
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = resource.LaunchMessage;
            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = (IntentRequest)input.Request;

                switch (intentRequest.Intent.Name)
                {
                    case "AMAZON.CancelIntent":
                        log.LogLine($"AMAZON.CancelIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.StopIntent":
                        log.LogLine($"AMAZON.StopIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.HelpIntent":
                        log.LogLine($"AMAZON.HelpIntent: send HelpMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpMessage;
                        break;
                    case "GetSmoothies":
                        log.LogLine($"GetSmoothies: send smoothies");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = GetSmoothies(resource);
                        break;
                    case "GetIngredients":
                        log.LogLine($"GetIngredients: get smoothie slot");
                        response = GetIngredients(resource, intentRequest, input, response, log);
                        innerResponse = response.Response.OutputSpeech;
                        break;
                    case "FilterGetSmoothies":
                        log.LogLine($"FilterGetSmoothies: get ingredient slot");
                        response = FilterGetSmoothies(resource, intentRequest, input, response, log);
                        innerResponse = response.Response.OutputSpeech;
                        break;
                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpReprompt;
                        break;
                }
            }

            
            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";
            log.LogLine($"Skill Response Object...");
            log.LogLine(JsonConvert.SerializeObject(response));
            return response;
        }

        public SkillResponse FilterGetSmoothies(SmoothieResource resource, IntentRequest intentRequest, SkillRequest input, SkillResponse response, ILambdaLogger log)
        {
            switch (intentRequest.DialogState)
            {
                case DialogState.Started:
                    // Pre-fill slots: update the intent object with slot values for which
                    // you have defaults, then return Dialog.Delegate with this updated intent
                    // in the updatedIntent property.
                    log.LogLine($"GetSlot: Started");
                    response = ResponseBuilder.DialogDelegate(input.Session, intentRequest.Intent);
                    break;
                case DialogState.InProgress:
                    // return a Dialog.Delegate directive with no updatedIntent property.
                    log.LogLine($"GetSlot: InProgress");
                    response = ResponseBuilder.DialogDelegate(input.Session);
                    break;
                case DialogState.Completed:
                    // Dialog is now complete and all required slots should be filled,
                    // so call your normal intent handler. 
                    log.LogLine($"GetSlot: Completed");
                    IOutputSpeech innerResponse = null;
                    innerResponse = new PlainTextOutputSpeech();
                    (innerResponse as PlainTextOutputSpeech).Text = GetSmoothies(resource, (smoothie => smoothie.Ingredients.Contains(intentRequest.Intent.Slots["Ingredient"].Value)));
                    response.Response.OutputSpeech = innerResponse;
                    break;
                default:
                    // return a Dialog.Delegate directive with no updatedIntent property.
                    //response = ResponseBuilder.DialogElicitSlot(GetInnerResponse("What medicine will you be administering?"), "medicineName", input.Session, intentRequest.Intent);
                    log.LogLine($"GetSlot: Default.");
                    log.LogLine($"Input: {JsonConvert.SerializeObject(input)}");
                    log.LogLine($"Intent Request: {JsonConvert.SerializeObject(intentRequest)}");
                    response = ResponseBuilder.DialogDelegate(input.Session);
                    log.LogLine($"Response: {JsonConvert.SerializeObject(response)}");
                    break;
            }
            return response;
        }

        public SkillResponse GetIngredients(SmoothieResource resource, IntentRequest intentRequest, SkillRequest input, SkillResponse response, ILambdaLogger log)
        {
            switch (intentRequest.DialogState)
            {
                case DialogState.Started:
                    // Pre-fill slots: update the intent object with slot values for which
                    // you have defaults, then return Dialog.Delegate with this updated intent
                    // in the updatedIntent property.
                    log.LogLine($"GetSlot: Started");
                    response = ResponseBuilder.DialogDelegate(input.Session, intentRequest.Intent);
                    break;
                case DialogState.InProgress:
                    // return a Dialog.Delegate directive with no updatedIntent property.
                    log.LogLine($"GetSlot: InProgress");
                    response = ResponseBuilder.DialogDelegate(input.Session);
                    break;
                case DialogState.Completed:
                    // Dialog is now complete and all required slots should be filled,
                    // so call your normal intent handler. 
                    log.LogLine($"GetSlot: Completed");
                    IOutputSpeech innerResponse = null;
                    innerResponse = new PlainTextOutputSpeech();
                    (innerResponse as PlainTextOutputSpeech).Text = GetSmoothie(resource, intentRequest.Intent.Slots["Smoothie"].Value);
                    response.Response.OutputSpeech = innerResponse;
                    break;
                default:
                    // return a Dialog.Delegate directive with no updatedIntent property.
                    //response = ResponseBuilder.DialogElicitSlot(GetInnerResponse("What medicine will you be administering?"), "medicineName", input.Session, intentRequest.Intent);
                    log.LogLine($"GetSlot: Default.");
                    log.LogLine($"Input: {JsonConvert.SerializeObject(input)}");
                    log.LogLine($"Intent Request: {JsonConvert.SerializeObject(intentRequest)}");
                    response = ResponseBuilder.DialogDelegate(input.Session);
                    log.LogLine($"Response: {JsonConvert.SerializeObject(response)}");
                    break;
            }
            return response;
        }
        #endregion
        public static string CombineElements(string[] elements)
        {
            if (elements == null)
            {
                return null;
            }
            else if(elements.Length == 1)
            {
                return elements[0];
            }
            string CombinedElements = "";
            for (int i = 0; i < elements.Length-1; i++)
            {
                CombinedElements += $"{elements[i]}, ";
            }
            return $"{CombinedElements}and {elements.Last()}";
        }
        public static string GetSmoothie(SmoothieResource resource, string key) => CombineElements(resource.Smoothies[key].Ingredients);
        public static string GetSmoothies(SmoothieResource resource) => GetSmoothies(resource, (smoothie => true));
        public static string GetSmoothies(SmoothieResource resource, Func<Smoothie, bool> smoothieFilter) => CombineElements(resource.Smoothies.Values.Where(smoothieFilter).Select(smoothie => smoothie.PrintName).ToArray());

        public class SmoothieResource
        {
            public string Language { get; set; }
            public string SkillName { get; set; }
            public string LaunchMessage { get; set; }
            public string LaunchMessageReprompt { get; set; }
            public string AskMessage { get; set; }
            public Dictionary<string, Smoothie> Smoothies { get; set; }
            public string GetIngredientsMessage { get; set; }
            public string HelpMessage { get; set; }
            public string HelpReprompt { get; set; }
            public string StopMessage { get; set; }
            public SmoothieResource(string language)
            {
                Language = language;
            }
        }
        public class Smoothie
        {
            public Smoothie(string[] ingredients, string printName)
            {
                Ingredients = ingredients;
                PrintName = printName;
            }

            public string[] Ingredients { get; set; }
            public string PrintName { get; set; }
        }
    }
}
