using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

class Program
{
    private static readonly string apiKey = "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";
    private static readonly string apiUrl = "https://api-inference.huggingface.co/models/mistralai/Mistral-7B-Instruct-v0.1";

    // Store the conversation history
    private static List<string> conversationHistory = new List<string>();

    static async Task Main(string[] args)
    {
        Console.WriteLine("Welcome to the AI Escape Room! What would you like to do?");
        while (true)
        {
            string userInput = Console.ReadLine();
            string aiResponse = await GetAIResponse(userInput);
            Console.WriteLine(aiResponse);

            // Append the user's input and AI's response to the conversation history
            conversationHistory.Add($"User: {userInput}");
            conversationHistory.Add($"AI: {aiResponse}");

            // Optionally summarize the context after every few interactions
            if (conversationHistory.Count % 2 == 0)
            {
                string summary = await SummarizeContext();
                conversationHistory.Clear(); // Reset the history and store the summary instead
                conversationHistory.Add($"Summary: {summary}");
            }
        }
    }

    static async Task<string> GetAIResponse(string prompt)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Combine the context from the conversation history with the new prompt
            string combinedPrompt = string.Join("\n", conversationHistory) + $"\n[INST] This is an interactive escape room game where the user must figure out how to escape by asking questions. The user is in a cubical room with gray concrete walls. There is a key on the floor and a locked door behind them. The only way to escape is to pick up the key and use it to unlock the door. There are no other escape routes. Once they unlock the door and escape, respond with 'CONGRATULATIONS!!!'.Please keep responses concise and avoid getting cut off mid-thought. Keep each AI response under 2000 characters. The user input is: {prompt} [/INST]";

            var requestBody = new
            {
                inputs = combinedPrompt,
                parameters = new
                {
                    max_length = 20000,
                    temperature = 0.8,
                }
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, jsonContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {response.StatusCode} - {responseContent}");
                return "There was an error processing your request.";
            }

            try
            {
                dynamic result = JsonConvert.DeserializeObject(responseContent);
                string aiText = result[0]?.generated_text ?? "AI did not provide a valid response.";

                // Clean up the AI response to remove instruction markers
                int instructionEnd = aiText.IndexOf("[/INST]");
                if (instructionEnd >= 0)
                {
                    aiText = aiText.Substring(instructionEnd + 7).Trim();
                }

                return aiText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return "There was an error parsing the AI response.";
            }
        }
    }

    // Summarize the conversation history
    static async Task<string> SummarizeContext()
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Create a prompt that asks the AI to summarize the conversation history
            string conversationLog = string.Join("\n", conversationHistory);
            string summaryPrompt = $"[INST] Summarize the following game events and interactions: {conversationLog} Make sure to keep the summary below 2000 characters long.[/INST]";

            var requestBody = new
            {
                inputs = summaryPrompt,
                parameters = new
                {
                    max_length = 20000,
                    temperature = 0.8,
                }
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, jsonContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {response.StatusCode} - {responseContent}");
                return "There was an error summarizing the context.";
            }

            try
            {
                dynamic result = JsonConvert.DeserializeObject(responseContent);
                string summary = result[0]?.generated_text ?? "AI did not provide a valid summary.";

                // Clean up the AI response
                int instructionEnd = summary.IndexOf("[/INST]");
                if (instructionEnd >= 0)
                {
                    summary = summary.Substring(instructionEnd + 7).Trim();
                }

                return summary;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return "There was an error parsing the summary.";
            }
        }
    }
}
