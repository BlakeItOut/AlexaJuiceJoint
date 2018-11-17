using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using System;
using Alexa.NET.Response.Directive;
using System.Text.RegularExpressions;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace alexaJuiceJointDetroit
{
    public class Function
    {
        #region Conversation
        private SkillResponse response = null;
        private ILambdaContext context = null;
        public static SmoothieResource resource = null;
        const string SMOOTHIES = "Smoothie";
        const string INGREDIENTS = "Ingredient";
        const string DAYOFWEEK = "DayOfTheWeek";
        
        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="input"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext ctx)
        {
            context = ctx;
            try
            {
                response = new SkillResponse();
                response.Response = new ResponseBody();
                response.Response.ShouldEndSession = false;
                response.Version = "1.0";

                resource = GetResource();

                if (input.GetRequestType() == typeof(LaunchRequest))
                {
                    ProcessLaunchRequest(response.Response);
                }
                else
                {
                    if (input.GetRequestType() == typeof(IntentRequest))
                    {
                        var intentRequest = (IntentRequest)input.Request;
                        if (IsDialogIntentRequest(intentRequest))
                        {
                            if (!IsDialogSequenceComplete(intentRequest))
                            {
                                CreateDelegateResponse();
                                return response;
                            }
                        }
                        if(!ProcessDialogRequest(intentRequest, response))
                        {
                            response.Response.OutputSpeech = ProcessIntentRequest(intentRequest);
                        }
                    }
                }
                Log(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Log($"error : {ex.Message}");
            }
            return null;   
        }

        private IOutputSpeech ProcessIntentRequest(IntentRequest intentRequest)
        {
            IOutputSpeech innerResponse = new PlainTextOutputSpeech();

            switch (intentRequest.Intent.Name)
            {
                case "GetSmoothies":
                    innerResponse = new SsmlOutputSpeech();
                    (innerResponse as SsmlOutputSpeech).Ssml = $"{GetSmoothies()}. {resource.AskMessage}";
                    break;
                case "GetAllHours":
                    innerResponse = new SsmlOutputSpeech();
                    (innerResponse as SsmlOutputSpeech).Ssml = $"{GetHours()} {resource.AskMessage}";
                    break;
                case "GetLocation":
                    innerResponse = new SsmlOutputSpeech();
                    (innerResponse as SsmlOutputSpeech).Ssml = $"{GetLocation()} {resource.AskMessage}";
                    break;
                case "OpenNow":
                    innerResponse = new SsmlOutputSpeech();
                    (innerResponse as SsmlOutputSpeech).Ssml = $"{OpenNow(intentRequest.Timestamp)} {resource.AskMessage}";
                    break;
                case BuiltInIntent.Cancel:
                case BuiltInIntent.Stop:
                    (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                    response.Response.ShouldEndSession = true;
                    break;
                case BuiltInIntent.Help:
                    (innerResponse as PlainTextOutputSpeech).Text = resource.HelpMessage;
                    break;
                default:
                    (innerResponse as PlainTextOutputSpeech).Text = resource.HelpReprompt;
                    break;
            }
            if(innerResponse.Type == "SSML")
            {
                BuildCard(resource.SkillName, (innerResponse as SsmlOutputSpeech).Ssml);
                (innerResponse as SsmlOutputSpeech).Ssml = SsmlDecorate((innerResponse as SsmlOutputSpeech).Ssml);
            }

            return innerResponse;
        }

        public static string GetHours()
        {
            return "The juice joint is open from 10 AM to 7 PM eastern standard time Monday through Friday and 10 AM to 5 PM eastern standard time on Saturday.";
        }

        public static string GetLocation()
        {
            return "The juice joint is located in the back of Marcus Market at 4614 2nd Avenue, Detroit, Michigan 48201.";
        }

        public static string OpenNow(DateTime now)
        {
            //TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            //DateTime now = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            now = now.AddHours(-5);
            string dayOfWeek = now.DayOfWeek.ToString().ToLower();
            if (!resource.Hours.ContainsKey(dayOfWeek))
            {
                return $"Sorry, it's not open today. {GetHours()}";
            }
            DateTime open = DateTime.Parse(resource.Hours[dayOfWeek].Open);
            DateTime close = DateTime.Parse(resource.Hours[dayOfWeek].Close);
            if (now < open)
            {
                return $"The juice joint is not open yet. It opens at {open.ToShortTimeString()} eastern standard time.";
            }
            else if (now > close)
            {
                return $"Sorry, it's closed now. {GetHours()}";
            }
            else
            {
                return $"The juice joint is open now! It closes at {close.ToShortTimeString()} eastern standard time.";
            }
        }

        private void BuildCard(string title, string output)
        {
            if (!String.IsNullOrEmpty(output))
            {
                output = Regex.Replace(output, @"<.*?>", "");
                response.Response.Card = new SimpleCard()
                {
                    Title = title,
                    Content = output,
                };
            };
        }

        private void Log(string text)
        {
            if(context != null)
            {
                context.Logger.LogLine(text);
            }
        }

        private bool ProcessDialogRequest(IntentRequest intentRequest, SkillResponse response)
        {
            string speech_message = String.Empty;
            bool processed = false;

            switch (intentRequest.Intent.Name)
            {
                case "GetIngredients":
                    speech_message = GetIngredients(intentRequest);
                    if (!String.IsNullOrEmpty(speech_message))
                    {
                        response.Response.OutputSpeech = new SsmlOutputSpeech();
                        (response.Response.OutputSpeech as SsmlOutputSpeech).Ssml = SsmlDecorate(speech_message);
                    }
                    processed = true;
                    break;
                case "FilterGetSmoothies":
                    speech_message = FilterGetSmoothies(intentRequest);
                    if (!String.IsNullOrEmpty(speech_message))
                    {
                        response.Response.OutputSpeech = new SsmlOutputSpeech();
                        (response.Response.OutputSpeech as SsmlOutputSpeech).Ssml = SsmlDecorate(speech_message);
                    }
                    processed = true;
                    break;
                case "GetHoursForDay":
                    speech_message = GetHoursForDay(intentRequest);
                    if (!String.IsNullOrEmpty(speech_message))
                    {
                        response.Response.OutputSpeech = new SsmlOutputSpeech();
                        (response.Response.OutputSpeech as SsmlOutputSpeech).Ssml = SsmlDecorate(speech_message);
                    }
                    processed = true;
                    break;
            }
            return processed;
        }

        private string GetHoursForDay(IntentRequest intentRequest)
        {
            string speech_message = String.Empty;
            if (intentRequest.Intent.Slots.ContainsKey(DAYOFWEEK))
            {
                Slot slot = null;
                if (intentRequest.Intent.Slots.TryGetValue(DAYOFWEEK, out slot))
                {
                    if (slot.Value != null && resource.Hours.ContainsKey(slot.Value.ToLower()))
                    {
                        Hour hour = resource.Hours[slot.Value.ToLower()];
                        speech_message = $"The hours for {slot.Value} are {hour.Open} to {hour.Close}. {resource.AskMessage}";
                    }
                    else
                    {
                        if (slot.Value == null)
                        {
                            speech_message = $"That is not a day of the week. {resource.AskMessage}";
                        }
                        else
                        {
                            speech_message = $"The juice joint is not open or the hours are varied on {slot.Value}. {resource.AskMessage}";
                        }
                    }
                }
            }
            return speech_message;
        }

        private void CreateDelegateResponse()
        {
            response.Response.Directives.Add(new DialogDelegate());
        }

        private void ProcessLaunchRequest(ResponseBody response)
        {
            IOutputSpeech innerResponse = new SsmlOutputSpeech();
            (innerResponse as SsmlOutputSpeech).Ssml = SsmlDecorate(resource.LaunchMessage);
            response.OutputSpeech = innerResponse;
            IOutputSpeech prompt = new PlainTextOutputSpeech();
            (prompt as PlainTextOutputSpeech).Text = resource.LaunchMessageReprompt;
            response.Reprompt = new Reprompt()
            {
                OutputSpeech = prompt
            };
        }

        private string SsmlDecorate(string speech)
        {
            return $"<speak>{speech}</speak>";
        }

        private bool IsDialogSequenceComplete(IntentRequest intentRequest)
        {
            if (intentRequest.DialogState == DialogState.Completed)
            {
                return true;
            }
            return false;
        }

        private bool IsDialogIntentRequest(IntentRequest intentRequest)
        {
            if (intentRequest.Intent.Slots == null)
            {
                return false;
            }
            return true;
        }

        public string FilterGetSmoothies(IntentRequest intentRequest)
        {
            string speech_message = string.Empty;
            var ingredients = resource.Smoothies.Values.SelectMany(smoothie => smoothie.Ingredients).Distinct().ToArray();

            if (intentRequest.Intent.Slots.ContainsKey(INGREDIENTS))
            {
                Slot slot = null;
                if (intentRequest.Intent.Slots.TryGetValue(INGREDIENTS, out slot))
                {
                    if (slot.Value != null && ingredients.Contains(slot.Value.ToLower()))
                    {
                        speech_message = $"The smoothies with {slot.Value} are {GetSmoothies(smoothie => smoothie.Ingredients.Contains(slot.Value))}. {resource.AskMessage}";
                    }
                    else
                    {
                        if(slot.Value == null)
                        {
                            speech_message = $"There is no smoothie in the database with that but you can always try asking in person as available ingredients change daily. {resource.AskMessage}";
                        }
                        else
                        {
                            speech_message = $"There is no smoothie in the database with {slot.Value} but you can always try asking in person as available ingredients change daily. {resource.AskMessage}";
                        }   
                    }
                }
            }
            return speech_message;
        }

        public string GetIngredients(IntentRequest intentRequest)
        {
            string speech_message = String.Empty;
            if (intentRequest.Intent.Slots.ContainsKey(SMOOTHIES))
            {
                Slot slot = null;
                if (intentRequest.Intent.Slots.TryGetValue(SMOOTHIES, out slot))
                {
                    if(slot.Value != null && resource.Smoothies.ContainsKey(slot.Value.ToLower()))
                    {
                        Smoothie smoothie = resource.Smoothies[slot.Value.ToLower()];
                        speech_message = $"The ingredients for {smoothie.PrintName} are {CombineElements(smoothie.Ingredients)}. {resource.AskMessage}";
                    }
                    else
                    {
                        if (slot.Value == null)
                        {
                            speech_message = $"There is no smoothie in the database called that. It may be an off-the-menu item so you will have to see in-person. {resource.AskMessage}";
                        }
                        else
                        {
                            speech_message = $"There is no smoothie in the database called {slot.Value}. It may be an off-the-menu item so you will have to see in-person. {resource.AskMessage}";
                        }
                    }
                }
            }
            return speech_message;
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
        public static string GetSmoothies() => GetSmoothies(smoothie => true);
        public static string GetSmoothies(Func<Smoothie, bool> smoothieFilter) => CombineElements(resource.Smoothies.Values.Where(smoothieFilter).Select(smoothie => smoothie.PrintName).ToArray());

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
            public Dictionary<string, Hour> Hours { get; set; } 

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
        public class Hour
        {
            public Hour(string open, string close)
            {
                Open = open;
                Close = close;
            }
            public string Open { get; set; }
            public string Close { get; set; }
        }
        public static SmoothieResource GetResource()
        {
            SmoothieResource enUSResource = new SmoothieResource("en-US");
            enUSResource.SkillName = "Juice Joint";
            enUSResource.HelpMessage = "You can ask me for the names of the smoothies, about their ingredients, what the hours are, if the juice joint is open now, or even search for a smoothie by ingredients. If you want to exit, just say exit...What can I help you with ?";
            enUSResource.HelpReprompt = " Names of smoothies, ingredients, smoothies by ingredient? Or just say exit to exit...What can I help you with next?";
            enUSResource.StopMessage = "Goodbye!";
            enUSResource.LaunchMessage = "Welcome to Juice Joint. I know the smoothies, their ingredients, and the hours of the Juice Joint. What would you like to know?";
            enUSResource.LaunchMessageReprompt = "Try asking me to tell you the smoothies.";
            enUSResource.AskMessage = "What else would you like to know?";
            enUSResource.Smoothies = new Dictionary<string, Smoothie>();
            enUSResource.Smoothies.Add("the boss", new Smoothie(new string[] { "matcha", "mango", "avocado", "baby greens", "banana", "almond milk", "agave" }, "the boss"));
            enUSResource.Smoothies.Add("summer blast", new Smoothie(new string[] { "raspberry", "lemon", "pineapple", "strawberry", "mango", "coconut water", "agave" }, "summer blast"));
            enUSResource.Smoothies.Add("pb and j", new Smoothie(new string[] { "strawberry", "peanut butter", "banana", "oat", "almond milk", "honey" }, "pb and j"));
            enUSResource.Smoothies.Add("jungle juice", new Smoothie(new string[] { "pineapple", "mango", "baby greens", "banana", "coconut water" }, "jungle juice"));
            enUSResource.Smoothies.Add("atomic energy", new Smoothie(new string[] { "mango", "carrot", "tumeric", "ginger", "pineapple", "orange", "banana", "lemon", "coconut water" }, "atomic energy"));
            enUSResource.Smoothies.Add("d's delight", new Smoothie(new string[] { "blueberry", "mango", "spinach", "ginger", "coconut water", "honey" }, "d's delight"));
            enUSResource.Smoothies.Add("blue nut", new Smoothie(new string[] { "cold brew coffee", "blueberry", "mango", "banana", "honey", "almond butter", "cinnamon", "almond milk" }, "blue nut"));
            enUSResource.Smoothies.Add("juicy fruit", new Smoothie(new string[] { "strawberry", "mango", "banana", "orange", "honey", "coconut water" }, "juicy fruit"));
            enUSResource.Smoothies.Add("basic bitch", new Smoothie(new string[] { "pumpkin puree", "pumpkin pie spice", "banana", "mango", "ginger", "almond butter", "almond milk", "honey" }, "basic bitch"));
            enUSResource.Smoothies.Add("the rihanna", new Smoothie(new string[] { "strawberry", "pineapple", "mango", "banana", "maca", "cayenne", "ginger", "coconut water", "honey" }, "the rihanna"));
            //enUSResource.Smoothies.Add("great gonzo", new Smoothie(new string[] { "blueberry", "pineapple", "ginger", "banana", "orange", "lemon", "coconut water" }, "great gonzo"));
            //enUSResource.Smoothies.Add("maui waui", new Smoothie(new string[] { "avocado", "lime", "pineapple", "baby greens", "mango", "cilantro", "cayenne", "coconut water" }, "maui waui"));
            //enUSResource.Smoothies.Add("sweetart", new Smoothie(new string[] { "blueberry", "raspberry", "strawberry", "orange", "kiwi", "banana", "coconut water", "honey" }, "sweetart"));
            //enUSResource.Smoothies.Add("tutti-frutti", new Smoothie(new string[] { "strawberry", "banana", "pineapple", "raspberry" }, "tutti frutti"));            
            //enUSResource.Smoothies.Add("blue berry yum yum", new Smoothie(new string[] { "blueberry", "banana", "almond butter", "almond milk", "honey" }, "blue berry yum yum"));
            enUSResource.Hours = new Dictionary<string, Hour>();
            enUSResource.Hours.Add("monday", new Hour("10:00 AM", "7:00 PM"));
            enUSResource.Hours.Add("tuesday", new Hour("10:00 AM", "7:00 PM"));
            enUSResource.Hours.Add("wednesday", new Hour("10:00 AM", "7:00 PM"));
            enUSResource.Hours.Add("thursday", new Hour("10:00 AM", "7:00 PM"));
            enUSResource.Hours.Add("friday", new Hour("10:00 AM", "7:00 PM"));
            enUSResource.Hours.Add("saturday", new Hour("10:00 AM", "5:00 PM"));
            enUSResource.Hours.Add("weekday", new Hour("10:00 AM", "7:00 PM"));
            return enUSResource;
        }
    }
}
