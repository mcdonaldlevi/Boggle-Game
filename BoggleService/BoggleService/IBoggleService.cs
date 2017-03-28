﻿using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Boggle
{
    [ServiceContract]
    public interface IBoggleService
    {
        /// <summary>
        /// Sends back index.html as the response body.
        /// </summary>
        [WebGet(UriTemplate = "/api")]
        Stream API();

        /// <summary>
        /// Returns the nth word from dictionary.txt.  If there is
        /// no nth word, responds with code 403. This is a demo;
        /// you can delete it.
        /// </summary>
        [WebGet(UriTemplate = "/word?index={n}")]
        string WordAtIndex(int n);

        [WebInvoke(Method = "POST", UriTemplate = "/users")]
        string CreateUser(UserInfo user);

        [WebInvoke(Method = "POST", UriTemplate = "/games")]
        string JoinGame(JoinGameInfo user);

        //[WebInvoke(Method = "PUT", UriTemplate = "/games")]
        //string CancelJoinRequest(UserInfo user);

        //[WebInvoke(Method = "PUT", UriTemplate = "/games/{GameID}")]
        //string PlayWord(UserInfo user);

        //[WebInvoke(Method = "GET", UriTemplate = "/games/{GameID}")]
        //string GameStatus(UserInfo user);
    }
}
