using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                    String result = response.Content.ReadAsStringAsync().Result;
                    GameID = (string)JsonConvert.DeserializeObject(result);

                }
                else
                {
                    Console.WriteLine("Error registering: " + response.StatusCode);
                    Console.WriteLine(response.ReasonPhrase);
                }
            }
        }

        private void View_WordEntered()
        {
            throw new NotImplementedException();
        }

        private void View_GameOver()
        {
            throw new NotImplementedException();
        }

        private async void View_RegisterPressed(string name, string address)
        {
            if(name == null || address == null)
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
                        userToken = (string)JsonConvert.DeserializeObject(result);
                        
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
