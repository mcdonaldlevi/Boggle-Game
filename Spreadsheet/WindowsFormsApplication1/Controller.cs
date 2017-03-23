﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BoggleClient
{
    class Controller
    {
        private Form1 view;
        /// <summary>
        /// The token of the most recently registered user, or "0" if no user
        /// has ever registered
        /// </summary>
        private string userToken;
        /// <summary>
        /// stores a game ID
        /// </summary>
        private string GameID;

        public Controller(Form1 view)
        {
            this.view = view;

            view.RegisterPressed += View_RegisterPressed;
            view.GameOver += View_GameOver;
            view.WordEntered += View_WordEntered;
            view.JoinGamePressed += View_JoinGamePressed;
            view.CancelButtonPressed += View_CancelButtonPressed;
            view.GameStatusRequest += View_GameStatusRequest;
        }

        private async void View_GameStatusRequest(string address)
        {
            {
                using (HttpClient client = CreateClient(address))
                {
                    // Compose and send the request.
                    HttpResponseMessage gameStatsResponse = await client.GetAsync("games/" + GameID);

                    // Deal with the response
                    if (gameStatsResponse.IsSuccessStatusCode)
                    {
                        String result = gameStatsResponse.Content.ReadAsStringAsync().Result;
                        string gameStatusResult = gameStatsResponse.Content.ReadAsStringAsync().Result;
                        string timeLeft = JToken.Parse(gameStatusResult)["TimeLeft"].ToString();
                        string playerOneScore = JToken.Parse(gameStatusResult)["Player1"]["Score"].ToString();
                        string playerTwoScore = JToken.Parse(gameStatusResult)["Player2"]["Score"].ToString();
                        string letters = JToken.Parse(gameStatusResult)["Board"].ToString();
                        view.displayLetters(letters);
                    }


                    else
                    {
                        Console.WriteLine("Error getting Game Status: " + gameStatsResponse.StatusCode);
                        Console.WriteLine(gameStatsResponse.ReasonPhrase);
                    }
                }
            }
        }

        private async void View_CancelButtonPressed(string address)
        {
            using (HttpClient client = CreateClient(address))
            {

                // Create the parameter
                dynamic user = new ExpandoObject();
                user.UserToken = userToken;


                // Compose and send the request.

                StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("games", content);

                // Deal with the response
                if (response.IsSuccessStatusCode)
                {}
                else
                {
                    Console.WriteLine("Error Canceling: " + response.StatusCode);
                    Console.WriteLine(response.ReasonPhrase);
                }
            }
        
        }

        private async void View_JoinGamePressed(int timeLimit, string address)
        {
            using (HttpClient client = CreateClient(address))
            {

                // Create the parameter
                dynamic user = new ExpandoObject();
                user.UserToken = userToken;
                user.TimeLimit = timeLimit;


                // Compose and send the request.
                StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("games", content);

                // Deal with the response
                if (response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 202 )
                    {
                        String result = response.Content.ReadAsStringAsync().Result;
                        GameID = JToken.Parse(result)["GameID"].ToString();
                    }
                    else if ((int)response.StatusCode == 201)
                    {

                        String result = response.Content.ReadAsStringAsync().Result;
                        GameID = JToken.Parse(result)["GameID"].ToString();
                        view.startTime();
                        //HttpResponseMessage gameStatsResponse = await client.GetAsync("games/" + GameID);
                        //if(gameStatsResponse.IsSuccessStatusCode)
                        //{
                        //    string gameStatusResult = gameStatsResponse.Content.ReadAsStringAsync().Result;
                        //    string letters = JToken.Parse(gameStatusResult)["Board"].ToString();
                        //    view.displayLetters(letters);
                        //    view.startTime();
                        //}
                        //else
                        //{
                            
                        //}
                    }

                }
                else
                {
                    Console.WriteLine("Error registering: " + response.StatusCode);
                    Console.WriteLine(response.ReasonPhrase);
                }
            }
        }

        private async void View_WordEntered(string address, string word)
        {
            using (HttpClient client = CreateClient(address))
            {

                // Create the parameter
                dynamic user = new ExpandoObject();
                user.UserToken = userToken;
                user.Word = word;

                // Compose and send the request.

                StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("games/" + GameID, content);

                // Deal with the response
                if (response.IsSuccessStatusCode)
                {
                    String result = response.Content.ReadAsStringAsync().Result;
                    string wordScore = JToken.Parse(result)["Score"].ToString();

                }
                else
                {
                    Console.WriteLine("Error Inputing Word " + response.StatusCode);
                    Console.WriteLine(response.ReasonPhrase);
                }
            }
        }

        private void View_GameOver()
        {
            throw new NotImplementedException();
        }

        private async void View_RegisterPressed(string name, string address)
        {
            if (name == null || address == null)
            {
                throw new ArgumentNullException();
            }
            try
            {

                using (HttpClient client = CreateClient(address))
                {

                    // Create the parameter
                    dynamic user = new ExpandoObject();
                    user.Nickname = name;


                    // Compose and send the request.

                    StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("users", content);

                    // Deal with the response
                    if (response.IsSuccessStatusCode)
                    {
                        String result = response.Content.ReadAsStringAsync().Result;
                        userToken = JToken.Parse(result)["UserToken"].ToString();

                    }
                    else
                    {
                        Console.WriteLine("Error registering: " + response.StatusCode);
                        Console.WriteLine(response.ReasonPhrase);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }

        }
        private static HttpClient CreateClient(string address)
        {
            // Create a client whose base address is the GitHub server
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(address);

            // Tell the server that the client will accept this particular type of response data
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            // There is more client configuration to do, depending on the request.
            return client;
        }
    }
}

