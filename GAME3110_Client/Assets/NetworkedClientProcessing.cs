using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkedClientProcessing : MonoBehaviour
{
    ReplayRecorder loadedReplayRecording;
    static string opponentsSymbol;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    static public void ReceivedMessageFromServer(string msg)
    {
        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);
        if (signifier == ServertoClientSignifiers.LoginResponse)
        {
            int loginResultSignifier = int.Parse(csv[1]);

            Debug.Log("Login Result: " + loginResultSignifier);
            if (loginResultSignifier == LoginResponse.Success)
            {
                gameLogic.GetComponent<GameLogic>().userName = csv[2];
                gameLogic.GetComponent<GameLogic>().ChangeGameState(GameStates.MainMenu);
            }
            else if (loginResultSignifier == LoginResponse.FailureNameInUse)
            {
                gameLogic.GetComponent<GameLogic>().SetErrorDisplayMessage("Failed: Username in use");
                gameLogic.GetComponent<GameLogic>().UpdateLogInInputFields(true, true);
            }
            else if (loginResultSignifier == LoginResponse.FailureNameNotFound)
            {
                gameLogic.GetComponent<GameLogic>().SetErrorDisplayMessage("Failed: Username not found");
                gameLogic.GetComponent<GameLogic>().UpdateLogInInputFields(true, true);
            }
            else if (loginResultSignifier == LoginResponse.FailureIncorrectPassword)
            {
                gameLogic.GetComponent<GameLogic>().SetErrorDisplayMessage("Failed: Incorrect Password");
                gameLogic.GetComponent<GameLogic>().UpdateLogInInputFields(false, true);
            }

        }
        else if (signifier == ServertoClientSignifiers.GameSessionStarted)
        {
            gameLogic.GetComponent<GameLogic>().ChangeGameState(GameStates.PlayingTicTacToe);
            opponentsSymbol = (csv[1] == "X") ? "O" : "X";
            bool myTurn = (int.Parse(csv[2]) == 1) ? true : false;
            gameLogic.GetComponent<GameLogic>().InitGameSymbolsSetCurrentTurn(csv[1], opponentsSymbol, myTurn);
            gameLogic.GetComponent<GameLogic>().chatScrollViewText.text = "";
            gameLogic.GetComponent<GameLogic>().gameSessionID = int.Parse(csv[3]);

        }
        else if (signifier == ServertoClientSignifiers.OpponentPlayedAMove)
        {
            int cellNumberOfMovePlayed = int.Parse(csv[1]);
            gameLogic.GetComponent<GameLogic>().UpdateTicTacToeGridAfterMove(cellNumberOfMovePlayed);
            gameLogic.GetComponent<GameLogic>().myTurnToMove = true;
            gameLogic.GetComponent<GameLogic>().UpdatePlayersCurrentTurnText(true);
            gameLogic.GetComponent<GameLogic>().OpponentMadeMove(cellNumberOfMovePlayed);
        }
        else if (signifier == ServertoClientSignifiers.OpponentWon)
        {
            gameLogic.GetComponent<GameLogic>().UpdateGameStatusText(csv[1] + " Won!");
            gameLogic.GetComponent<GameLogic>().myTurnToMove = false;
            gameLogic.GetComponent<GameLogic>().GameOver();
        }
        else if (signifier == ServertoClientSignifiers.GameDrawn)
        {
            gameLogic.GetComponent<GameLogic>().UpdateGameStatusText("Game Drawn");
            gameLogic.GetComponent<GameLogic>().myTurnToMove = false;
            gameLogic.GetComponent<GameLogic>().GameOver();
        }
        else if (signifier == ServertoClientSignifiers.OpponentRestartedGame)
        {
            gameLogic.GetComponent<GameLogic>().ChangeGameState(GameStates.PlayingTicTacToe);
        }
        else if (signifier == ServertoClientSignifiers.LeaderboardShowRequest)
        {
            int numberOfPlayersToDisplay = int.Parse(csv[1]);
            int index = 2;

            for (int i = 0; i < numberOfPlayersToDisplay; i++)
            {
                int leaderboardPosition = (i + 1);
                string leaderboardPlayerResults = leaderboardPosition + ". " + csv[index++] + "\n";
                string leaderboardWinsResults = "Wins: " + csv[index++] + "\n";
                gameLogic.GetComponent<GameLogic>().AddPlayerToLeaderboardTextBox(leaderboardPlayerResults, leaderboardWinsResults);
            }
        }
        else if (signifier == ServertoClientSignifiers.SendPlayerChatToOpponent)
        {
            string message = "\n" + csv[1] + ": " + csv[2];
            gameLogic.GetComponent<GameLogic>().AddOpponenetMessageToChat(message);
        }
        else if (signifier == ServertoClientSignifiers.GetCellsOfTicTacToeBoard)
        {
            string requesterID = csv[1];
            string boardResults = gameLogic.GetComponent<GameLogic>().ConverCurrentTicTacToeBoardToString();

            gameLogic.GetComponent<GameLogic>().networkClient.GetComponent<NetworkedClient>()
                .SendMessageToServer(string.Join(",", ClientToSeverSignifiers.SendCellsOfTicTacToeBoardToServer.ToString(), requesterID, boardResults));
        }
        else if (signifier == ServertoClientSignifiers.SendTicTacToeCellsToObserver)
        {
            string boardResults = csv[1];
            gameLogic.GetComponent<GameLogic>().ChangeGameState(GameStates.PlayingTicTacToe);
            gameLogic.GetComponent<GameLogic>().PopulateObserverTicTacToeBoard(boardResults);
            gameLogic.GetComponent<GameLogic>().chatScrollViewText.text = "";
            gameLogic.GetComponent<GameLogic>().gameStatusText.GetComponent<TextMeshProUGUI>().text = "OBSERVER";
        }
        else if (signifier == ServertoClientSignifiers.UpdateObserverOnMoveMade)
        {
            int cellNumber = int.Parse(csv[1]);
            string symbol = csv[2];

            gameLogic.GetComponent<GameLogic>().UpdateObserverTicTacToeBoard(cellNumber, symbol);
            gameLogic.GetComponent<GameLogic>().UpdateObserverTurnDisplay(symbol);
        }
        else if (signifier == ServertoClientSignifiers.SendNumberOfSavedRecordings)
        {
            int numberOfRecordings = int.Parse(csv[1]);
            gameLogic.GetComponent<GameLogic>().UpdateRecordingDropdownMenu(numberOfRecordings);
        }
        else if (signifier == ServertoClientSignifiers.GameSessionSearchResponse)
        {
            Debug.Log("Game room search results");
            if (int.Parse(csv[1]) == GameRoomSearchResponse.SearchFailed)
                gameLogic.GetComponent<GameLogic>().SetErrorDisplayMessage("Game Room Not Found");
        }

        else if (signifier == ServertoClientSignifiers.RecordingStartingToBeSentToClient)
        {
            gameLogic.GetComponent<GameLogic>().replayRecorder = new ReplayRecorder();
        }

        else if (signifier == ServertoClientSignifiers.ServerSentRecordingUserName)
        {
            gameLogic.GetComponent<GameLogic>().replayRecorder.username = csv[1];
        }

        else if (signifier == ServertoClientSignifiers.ServerSentRecordedStartingSymbol)
        {
            gameLogic.GetComponent<GameLogic>().replayRecorder.startingSymbol = csv[1];
        }

        else if (signifier == ServertoClientSignifiers.ServerSentRecordedNumberOfTurns)
        {
            gameLogic.GetComponent<GameLogic>().replayRecorder.numberOfTurns = int.Parse(csv[1]);
        }

        else if (signifier == ServertoClientSignifiers.ServerSentRecordedTimeBetweenTurns)
        {
            for (int i = 1; i < csv.Length; i++)
            {
                gameLogic.GetComponent<GameLogic>().replayRecorder.timeBetweenTurnsArray.Add(float.Parse(csv[i]));
            }
        }
        else if (signifier == ServertoClientSignifiers.ServerSentRecordedIndexOfMoveLocation)
        {
            for (int i = 1; i < csv.Length; i++)
            {
                gameLogic.GetComponent<GameLogic>().replayRecorder.cellNumberOfTurn.Add(int.Parse(csv[i]));
            }
        }
        else if (signifier == ServertoClientSignifiers.RecordingFinishedSendingToClient)
        {
            gameLogic.GetComponent<GameLogic>().LoadAndBeginRecording();
        }
    }


        static public void SendMessageToServer(string msg)
        {
            networkedClient.SendMessageToServer(msg);
        }
        static public void ConnectionEvent()
        {
            Debug.Log("Network Connection Event!");
        }
        static public void DisconnectionEvent()
        {
            Debug.Log("Network Disconnection Event!");
        }
        static public bool IsConnectedToServer()
        {
            return networkedClient.IsConnected();
        }
        static public void ConnectToServer()
        {
            networkedClient.Connect();
        }
        static public void DisconnectFromServer()
        {
            networkedClient.Disconnect();
        }

        #region Setup
        static NetworkedClient networkedClient;
        static GameLogic gameLogic;

        static public void SetNetworkedClient(NetworkedClient NetworkedClient)
        {
            networkedClient = NetworkedClient;
        }
        static public NetworkedClient GetNetworkedClient()
        {
            return networkedClient;
        }
        static public void SetGameLogic(GameLogic GameLogic)
        {
            gameLogic = GameLogic;
        }

        #endregion
    }


    public static class ClientToSeverSignifiers
{
    public const int Login = 1;
    public const int CreateAccount = 2;
    public const int AddToGameSessionQueue = 3;
    public const int TicTacToePlay = 4;
    public const int TicTacToeMoveMade = 5;
    public const int GameOver = 6;
    public const int GameDrawn = 7;
    public const int RestartGame = 8;
    public const int ShowLeaderboard = 9;
    public const int PlayerSentMessageInChat = 10;
    public const int SearchGameRoomRequestMade = 11;
    public const int SendCellsOfTicTacToeBoardToServer = 12;

    public const int RequestNumberOfSavedRecordings = 13;
    public const int ClearRecordingOnServer = 14;
    public const int PlayerLeftGameRoom = 15;
    public const int PlayerHasLeftGameQueue = 16;

    public const int RecordingRequestedFromServer = 17;

    public const int BeginSendingRecording = 18;

    public const int SendRecordedPlayersUserName = 19;
    public const int SendRecordedNumberOfTurns = 20;
    public const int SendRecordedGamesStartingSymbol = 21;
    public const int SendRecordedGamesTimeBetweenTurns = 22;
    public const int SendRecordedGamesIndexOfMoveLocation = 23;

    public const int FinishedSendingRecordingToServer = 24;
    //public const int SendRecordingName

    //Recordings 
}

public static class ServertoClientSignifiers
{
    public const int LoginResponse = 1;
    public const int GameSessionStarted = 2;
    public const int OpponentTicTacToePlay = 3;
    public const int OpponentPlayedAMove = 4;
    public const int OpponentWon = 5;
    public const int GameDrawn = 6;
    public const int OpponentRestartedGame = 7;
    public const int LeaderboardShowRequest = 8;
    public const int SendPlayerChatToOpponent = 9;
    public const int SearchFoundValidGameRoom = 10;
    public const int GetCellsOfTicTacToeBoard = 11;
    public const int SendTicTacToeCellsToObserver = 12;
    public const int UpdateObserverOnMoveMade = 13;

    public const int SendNumberOfSavedRecordings = 14;
    public const int ReloadDropDownMenu = 15;
    public const int GameSessionSearchResponse = 16;

    public const int RecordingStartingToBeSentToClient = 17;

    public const int ServerSentRecordingUserName = 18;
    public const int ServerSentRecordedNumberOfTurns = 19;
    public const int ServerSentRecordedStartingSymbol = 20;
    public const int ServerSentRecordedTimeBetweenTurns = 21;
    public const int ServerSentRecordedIndexOfMoveLocation = 22;

    public const int RecordingFinishedSendingToClient = 23;
}

public static class LoginResponse
{
    public const int Success = 1;

    public const int FailureNameInUse = 2;

    public const int FailureNameNotFound = 3;

    public const int FailureIncorrectPassword = 4;
}

public static class GameRoomSearchResponse
{
    public const int SearchSucceeded = 1;
    public const int SearchFailed = 2;
}
