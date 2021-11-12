using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;

public class NetworkedClient : MonoBehaviour
{

    int connectionID;
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 25565;
    byte error;
    bool isConnected = false;
    int ourClientID;
    string opponentsSymbol;


    GameObject gameSystemManager;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (go.name == "GameSystemManager")
            {
                gameSystemManager = go;
            }
        }

            Connect();
    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.S))
          //  SendMessageToHost("Hello from client");

        UpdateNetworkConnection();
    }

    private void UpdateNetworkConnection()
    {
        if (isConnected)
        {
            int recHostID;
            int recConnectionID;
            int recChannelID;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

            switch (recNetworkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log("connected.  " + recConnectionID);
                    ourClientID = recConnectionID;
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    ProcessRecievedMsg(msg, recConnectionID);
                    //Debug.Log("got msg = " + msg);
                    break;
                case NetworkEventType.DisconnectEvent:
                    isConnected = false;
                    Debug.Log("disconnected.  " + recConnectionID);
                    break;
            }
        }
    }
    
    private void Connect()
    {

        if (!isConnected)
        {
            Debug.Log("Attempting to create connection");

            NetworkTransport.Init();

            ConnectionConfig config = new ConnectionConfig();
            reliableChannelID = config.AddChannel(QosType.Reliable);
            unreliableChannelID = config.AddChannel(QosType.Unreliable);
            HostTopology topology = new HostTopology(config, maxConnections);
            hostID = NetworkTransport.AddHost(topology, 0);
            Debug.Log("Socket open.  Host ID = " + hostID);

            connectionID = NetworkTransport.Connect(hostID, "192.168.2.13", socketPort, 0, out error); // server is local on network

            if (error == 0)
            {
                isConnected = true;

                Debug.Log("Connected, id = " + connectionID);

            }
        }
    }
    
    public void Disconnect()
    {
        NetworkTransport.Disconnect(hostID, connectionID, out error);
    }
    
    public void SendMessageToHost(string msg)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, connectionID, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }

    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg received = " + msg + ".  connection id = " + id);
        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);
        if (signifier == ServertoClientSignifiers.LoginResponse)
        {
            int loginResultSignifier = int.Parse(csv[1]);

            if (loginResultSignifier == LoginResponse.Success)
            {
                gameSystemManager.GetComponent<GameSystemManager>().ChangeGameState(GameStates.MainMenu);
                gameSystemManager.GetComponent<GameSystemManager>().userName = csv[2];
                gameSystemManager.GetComponent<GameSystemManager>().GetNumberOfSavedRecordingsFromServer();
            }

        }
        else if (signifier == ServertoClientSignifiers.GameSessionStarted)
        {
            gameSystemManager.GetComponent<GameSystemManager>().ChangeGameState(GameStates.PlayiongTicTacToe);

            opponentsSymbol = (csv[1] == "X") ? "O" : "X";
            bool myTurn = (int.Parse(csv[2]) == 1) ? true : false;
            gameSystemManager.GetComponent<GameSystemManager>().InitGameSymbolsSetCurrentTurn(csv[1], opponentsSymbol, myTurn);
            gameSystemManager.GetComponent<GameSystemManager>().chatScrollViewText.text = "";
            gameSystemManager.GetComponent<GameSystemManager>().gameSessionID = int.Parse(csv[3]);

        }
        else if (signifier == ServertoClientSignifiers.OpponentTicTacToePlay)
        {

        }
        else if (signifier == ServertoClientSignifiers.OpponentPlayedAMove)
        {
            int cellNumberOfMovePlayed = int.Parse(csv[1]);
            gameSystemManager.GetComponent<GameSystemManager>().UpdateTicTacToeGridAfterMove(cellNumberOfMovePlayed);
            gameSystemManager.GetComponent<GameSystemManager>().myTurnToMove = true;
            gameSystemManager.GetComponent<GameSystemManager>().UpdatePlayersCurrentTurnText(true);
            gameSystemManager.GetComponent<GameSystemManager>().OpponentMadeMove(cellNumberOfMovePlayed);
        }
        else if (signifier == ServertoClientSignifiers.OpponentWon)
        {
            gameSystemManager.GetComponent<GameSystemManager>().UpdateGameStatusText(csv[1] + " Won!");
            gameSystemManager.GetComponent<GameSystemManager>().myTurnToMove = false;
            gameSystemManager.GetComponent<GameSystemManager>().GameOver();
        }
        else if (signifier == ServertoClientSignifiers.GameDrawn)
        {
            gameSystemManager.GetComponent<GameSystemManager>().UpdateGameStatusText("Game Drawn");
            gameSystemManager.GetComponent<GameSystemManager>().myTurnToMove = false;
            gameSystemManager.GetComponent<GameSystemManager>().GameOver();
        }
        else if (signifier == ServertoClientSignifiers.OpponentRestartedGame)
        {
            gameSystemManager.GetComponent<GameSystemManager>().ChangeGameState(GameStates.PlayiongTicTacToe);
        }
        else if (signifier == ServertoClientSignifiers.LeaderboardShowRequest)
        {
            int numberOfPlayersToDisplay = int.Parse(csv[1]);
            int playerRanking = 1;
            for(int i = 2; i < (numberOfPlayersToDisplay*2) + 2; i += 2)
            {
                string leaderboardPlayerResults = playerRanking++ + ". " + csv[i]+ "\n";
                string leaderboardWinsResults = "Wins: " + csv[i + 1] + "\n";
                gameSystemManager.GetComponent<GameSystemManager>().AddPlayerToLeaderboardTextBox(leaderboardPlayerResults, leaderboardWinsResults);
            }
        }
        else if (signifier == ServertoClientSignifiers.SendPlayerChatToOpponent)
        {
            string message = "\n" + csv[1] + ": " + csv[2];
            gameSystemManager.GetComponent<GameSystemManager>().AddOpponenetMessageToChat(message);
        }
        else if (signifier == ServertoClientSignifiers.GetCellsOfTicTacToeBoard)
        {
            string requesterID = csv[1];
            string boardResults = gameSystemManager.GetComponent<GameSystemManager>().ConverCurrentTicTacToeBoardToString();
            gameSystemManager.GetComponent<GameSystemManager>().networkClient.GetComponent<NetworkedClient>()
                .SendMessageToHost(string.Join(",",ClientToSeverSignifiers.SendCellsOfTicTacToeBoardToServer.ToString(), requesterID, boardResults));
        }
        else if (signifier == ServertoClientSignifiers.SendTicTacToeCellsToObserver)
        {
            string boardResults = csv[1];
            gameSystemManager.GetComponent<GameSystemManager>().ChangeGameState(GameStates.PlayiongTicTacToe);
            gameSystemManager.GetComponent<GameSystemManager>().PopulateObserverTicTacToeBoard(boardResults);

            int i = 8;
            for (; i >= 0; i--)
            {
                if (boardResults[i] != 'B')
                    break;
            }

            string symbol = (boardResults[i] == 'X') ? "O" : "X";
            gameSystemManager.GetComponent<GameSystemManager>().UpdateObserverTurnDisplay(symbol);
        }
        else if (signifier == ServertoClientSignifiers.UpdateObserverOnMoveMade)
        {
            int cellNumber = int.Parse(csv[1]);
            string symbol = csv[2];
            symbol = (csv[2] == "X") ? "O" : "X";

            gameSystemManager.GetComponent<GameSystemManager>().UpdateObserverTicTacToeBoard(cellNumber, symbol);
            gameSystemManager.GetComponent<GameSystemManager>().UpdateObserverTurnDisplay(symbol);
        }
        else if (signifier == ServertoClientSignifiers.RecordingSentToClient)
        {
            gameSystemManager.GetComponent<GameSystemManager>().LoadAndBeginRecording(msg);
        }
        else if (signifier == ServertoClientSignifiers.SendNumberOfSavedRecordings)
        {
            int numberOfRecordings = int.Parse(csv[1]);
            gameSystemManager.GetComponent<GameSystemManager>().UpdateRecordingDropdownMenu(numberOfRecordings);
        }
    }

    public bool IsConnected()
    {
        return isConnected;
    }


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
    public const int RecordingSentToServer = 13;
    public const int RecordingRequestedFromServer = 14;
    public const int RequestNumberOfSavedRecordings = 15;
    public const int ClearRecordingOnServer = 16;
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
    public const int RecordingSentToClient = 14;
    public const int SendNumberOfSavedRecordings = 15;
    public const int ReloadDropDownMenu = 16;
}

public static class LoginResponse
{
    public const int Success = 1;

    public const int FailureNameInUse = 2;

    public const int FailureNameNotFound = 3;

    public const int FailureIncorrectPassword = 4;
}

