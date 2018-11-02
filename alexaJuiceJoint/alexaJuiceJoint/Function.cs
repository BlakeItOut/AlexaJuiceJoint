using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace alexaJuiceJoint
{
    public class Function
    {
        public static List<SmoothieResource> GetResources()
        {
            List<SmoothieResource> resources = new List<SmoothieResource>();
            SmoothieResource enUSResource = new SmoothieResource("en-US");
            enUSResource.SkillName = "Juice Joint";
            enUSResource.HelpMessage = "You can say tell me ingredients for a Smoothie, or, you can say exit...What can I help you with ?";
            enUSResource.HelpReprompt = "What can I help you with?";
            enUSResource.StopMessage = "Goodbye!";
            enUSResource.LaunchMessage = "Welcome to Juice Joint. I know the ingredients for the smoothies at the Juice Joint. What would you like to know?";
            enUSResource.LaunchMessageReprompt = "Try asking me to tell you ingredients.";
            enUSResource.AskMessage = " What else would you like to know?";
            enUSResource.Smoothies.Add("great gonzo", new Smoothie(new string[] { "Blueberry", "Pineapple", "Ginger", "Banana", "Orange", "Lemon", "Coconut Water" }, "Great Gonzo"));
            enUSResource.Smoothies.Add("maui waui", new Smoothie(new string [] {"Avocado", "Lime", "Pineapple", "Baby Greens", "Mango", "Cilantro", "Cayenne", "Coconut Water"}, "Maui Waui"));
            enUSResource.Smoothies.Add("atomic energy", new Smoothie(new string [] { "Mango", "Carrot", "Tumeric", "Ginger", "Pineapple", "Orange", "Banana", "Lemon", "Coconut Water" }, "Atomic Energy"));
            enUSResource.Smoothies.Add("sweetart", new Smoothie(new string [] { "Blueberry", "Raspberry", "Strawberry", "Orange", "Kiwi", "Banana", "Coconut Water", "Honey" }, "Sweetart"));
            enUSResource.Smoothies.Add("tutti-frutti", new Smoothie(new string[] { "Strawberry", "Banana", "Pineapple", "Raspberry" }, "Tutti Frutti"));
            enUSResource.Smoothies.Add("jungle juice", new Smoothie(new string[] { "Pineapple", "Mango", "Baby Greens", "Banana", "Coconut Water" }, "Jungle Juice"));
            enUSResource.Smoothies.Add("the boss", new Smoothie(new string[] { "Matcha", "Mango", "Avocado", "Baby Greens", "Banana", "Almond Milk", "Agave" }, "The Boss"));
            enUSResource.Smoothies.Add("blue berry yum yum", new Smoothie(new string[] { "Blueberry", "Banana", "Almond Butter", "Almond Milk", "Honey" }, "Blue Berry Yum Yum"));
            resources.Add(enUSResource);
            return resources;
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

            var allResources = GetResources();
            var resource = allResources.FirstOrDefault();

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
                        log.LogLine($"GetFactIntent sent: send new fact");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = GetSmoothies(resource);
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
        #endregion
        public static string CombineElements(string[] elements)
        {
            throw new NotImplementedException();
        }
        public static string GetSmoothies(SmoothieResource resource)
        {
            throw new NotImplementedException();
        }
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
            public SmoothieResource (string language)
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

            public string [] Ingredients { get; set; }
            public string PrintName { get; set; }
        }
    }
}
