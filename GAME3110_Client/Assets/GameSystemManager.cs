using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine;

public class GameSystemManager : MonoBehaviour
{
    GameObject inputFieldUsername, inputFieldPassword, chatInputField, buttonSubmit, toggleLogIn, toggleCreateAccount;
    GameObject replayDropDown;
    public GameObject networkClient;
    GameObject findGameSessionButton, mainMenuGameButton, restartGameButton, leaderboardButton, leaderboardNamesText, leaderboardWinsText, chatScrollView, searchGameRoomButton, chatInputFieldSubmitButton, replayDropDownButton;
    GameObject nameTextBox, passwordTextBox, gameRoomNumberText, replayDropDownText;
    GameObject ticTacToeBoard,gameStatusText;
    GameObject searchGameRoomInputField;
    public ReplayRecorder replayRecorder;
    public Button[] ticTacToeButtonCellArray;
    string playersTicTacToeSymbol,opponentsTicTacToeSymbol;
    public bool myTurnToMove = false, isWatchingReplay;
    public bool opponentsTurn = false, gameStarted = false;
    float playerTurnCounter = 0.0f, opponentTurnCounter = 0.0f;
    int numberOfTotalMovesMade = 0;
    public int gameSessionID;
    public string userName;
    public TextMeshProUGUI chatScrollViewText;
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (go.name == "Name Input Field")
                inputFieldUsername = go;
            else if (go.name == "Password Input Field")
                inputFieldPassword = go;
            else if (go.name == "Submit Button")
                buttonSubmit = go;
            else if (go.name == "CreateAccount Toggle")
                toggleCreateAccount = go;
            else if (go.name == "Login Toggle")
                toggleLogIn = go;
            else if (go.name == "networkClient")
                networkClient = go;
            else if (go.name == "FindGameSessionButton")
                findGameSessionButton = go;
            else if (go.name == "MainMenuButton")
                mainMenuGameButton = go;
            else if (go.name == "NameTextbox")
                nameTextBox = go;
            else if (go.name == "PasswordTextbox")
                passwordTextBox = go;
            else if (go.name == "TicTacToeBoard")
                ticTacToeBoard = go;
            else if (go.name == "GameStatusText")
                gameStatusText = go;
            else if (go.name == "RestartGameButton")
                restartGameButton = go;
            else if (go.name == "LeaderboardButton")
                leaderboardButton = go;
            else if (go.name == "LeaderboardNamesText")
                leaderboardNamesText = go;
            else if (go.name == "LeaderboardWinsText")
                leaderboardWinsText = go;
            else if (go.name == "ChatScrollView")
                chatScrollView = go;
            else if (go.name == "ChatInputField")
                chatInputField = go;
            else if (go.name == "ChatScrollViewText")
                chatScrollViewText = go.GetComponent<TextMeshProUGUI>();
            else if (go.name == "SearchGameRoomInputField")
                searchGameRoomInputField = go;
            else if (go.name == "SearchGameRoomButton")
                searchGameRoomButton = go;
            else if (go.name == "GameRoomNumberText")
                gameRoomNumberText = go;
            else if (go.name == "ChatInputFieldSubmitButton")
                chatInputFieldSubmitButton = go;
            else if (go.name == "ReplayDropDown")
                replayDropDown = go;
            else if (go.name == "ReplayDropDownText")
                replayDropDownText = go;
            else if (go.name == "ReplayDropDownButton")
                replayDropDownButton = go;



        }

        buttonSubmit.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);
        toggleCreateAccount.GetComponent<Toggle>().onValueChanged.AddListener(ToggleCreateValueChanged);
        toggleLogIn.GetComponent<Toggle>().onValueChanged.AddListener(ToggleLogInValueChanged);

        findGameSessionButton.GetComponent<Button>().onClick.AddListener(FindGameSessionButtonPressed);
        mainMenuGameButton.GetComponent<Button>().onClick.AddListener(MainMenuGameButtonPressed);
        restartGameButton.GetComponent<Button>().onClick.AddListener(RestartGameButtonPressed);
        leaderboardButton.GetComponent<Button>().onClick.AddListener(ShowLeaderboardButtonPressed);
        searchGameRoomButton.GetComponent<Button>().onClick.AddListener(SearchGameRoomButtonPressed);
        ticTacToeButtonCellArray = ticTacToeBoard.GetComponentsInChildren<Button>();
        chatInputFieldSubmitButton.GetComponent<Button>().onClick.AddListener(ChatInputFieldSubmitButtonPressed);
        replayDropDownButton.GetComponent<Button>().onClick.AddListener(ReplayDropDownButtonPressed);
        AddListenersToButtonCellArray();
        
        ChangeGameState(GameStates.Login);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (chatInputField.GetComponent<TMP_InputField>().text != "")
            {
                networkClient.GetComponent<NetworkedClient>().SendMessageToHost(string.Join(",",ClientToSeverSignifiers.PlayerSentMessageInChat, userName, chatInputField.GetComponent<TMP_InputField>().text));
                chatScrollViewText.text += "\n" + userName + ": " + chatInputField.GetComponent<TMP_InputField>().text;
                chatInputField.GetComponent<TMP_InputField>().text = "";
            }
            if (searchGameRoomInputField.GetComponent<TMP_InputField>().text != "")
            {
                networkClient.GetComponent<NetworkedClient>().SendMessageToHost(string.Join(",", ClientToSeverSignifiers.SearchGameRoomRequestMade, searchGameRoomInputField.GetComponent<TMP_InputField>().text));
                searchGameRoomInputField.GetComponent<TMP_InputField>().text = "";
            }
        }

        if (gameStarted)
        {
            Debug.Log("Recording Started");
            if (opponentsTurn)
            {
                opponentTurnCounter += Time.deltaTime;
            }
            else
                playerTurnCounter += Time.deltaTime;
        }
    }

    private void AddListenersToButtonCellArray()
    {
        foreach (Button button in ticTacToeButtonCellArray)
        {
            button.onClick.AddListener(ButtonCellPressed);
        }
    }


    public void SubmitButtonPressed()
    {
        string n = inputFieldUsername.GetComponent<InputField>().text;
        string p = inputFieldPassword.GetComponent<InputField>().text;

        if (toggleLogIn.GetComponent<Toggle>().isOn)
        {
            networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.Login + "," + n + "," + p);
        }
        else
        {
            networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.CreateAccount + "," + n + "," + p);
        }
    }

    private void ButtonCellPressed()
    {
        Button button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

        for (int i = 0; i < ticTacToeButtonCellArray.Length; i++)
        {
            if (button == ticTacToeButtonCellArray[i] && buttonText.text == "" && myTurnToMove == true)
            {
                opponentsTurn = true;
                replayRecorder.timeBetweenTurnsArray[ReplayRecorder.turnNumber] = playerTurnCounter;
                replayRecorder.cellNumberOfTurn[ReplayRecorder.turnNumber] = i;
                playerTurnCounter = 0.0f;
                ReplayRecorder.turnNumber++;

                numberOfTotalMovesMade++;
                Debug.Log("Number of moves made: " + numberOfTotalMovesMade);
                myTurnToMove = false;
                UpdatePlayersCurrentTurnText(myTurnToMove);
                buttonText.text = playersTicTacToeSymbol;
                networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.TicTacToeMoveMade + "," + i + "," + playersTicTacToeSymbol);
                if (CheckIfGameOver())
                {
                    Debug.Log("Printing Symbols");
                    for(int j = 0; j < 7; j +=3)
                    {
                        Debug.Log(ticTacToeButtonCellArray[j].GetComponentInChildren<TextMeshProUGUI>().text + "," + ticTacToeButtonCellArray[j+1].GetComponentInChildren<TextMeshProUGUI>().text + "," + ticTacToeButtonCellArray[j + 2].GetComponentInChildren<TextMeshProUGUI>().text);
                        
                    }
                }
                return;
            }
        }   
    }

    public void ToggleCreateValueChanged(bool val)
    {
        toggleLogIn.GetComponent<Toggle>().SetIsOnWithoutNotify(!val);
    }

    public void ToggleLogInValueChanged(bool val)
    {
        toggleCreateAccount.GetComponent<Toggle>().SetIsOnWithoutNotify(!val);
    }

    private void FindGameSessionButtonPressed()
    {
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.AddToGameSessionQueue + "");
        ChangeGameState(GameStates.WaitingForMatch);
    }

    private void MainMenuGameButtonPressed()
    {
        ChangeGameState(GameStates.MainMenu);
    }

    public void InitGameSymbolsSetCurrentTurn(string playerSymbol, string opponentSymbol, bool myTurn)
    {
        playersTicTacToeSymbol = playerSymbol;
        opponentsTicTacToeSymbol = opponentSymbol;
        myTurnToMove = myTurn;
        UpdatePlayersCurrentTurnText(myTurnToMove);
        gameStarted = true;
        if (myTurn == true)
        {
            replayRecorder.startingSymbol = playerSymbol;
            opponentsTurn = false;
        }
        else
        {
            opponentsTurn = true;
            replayRecorder.startingSymbol = opponentSymbol;
        }

    }

    public void UpdateTicTacToeGridAfterMove(int cellNumber)
    {
        numberOfTotalMovesMade++;
        ticTacToeButtonCellArray[cellNumber].GetComponentInChildren<TextMeshProUGUI>().text = opponentsTicTacToeSymbol;
    }

    public void UpdatePlayersCurrentTurnText(bool myTurn)
    {
        gameStatusText.GetComponent<TextMeshProUGUI>().text = (myTurn == true) ? "Your Move" : "Opponents Move";
    }

    public void UpdateObserverTurnDisplay(string symbol)
    {
        gameStatusText.GetComponent<TextMeshProUGUI>().text ="OBSERVER    " + symbol + " Turn";
    }

    private void ResetAllCellButtonTextValues()
    {
        foreach(Button button in ticTacToeButtonCellArray)
        {
            button.GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
    }

    public bool CheckIfGameOver()
    {
        //Earliest a game can be over is 5 moves so only start checking after the 5th move
        if (numberOfTotalMovesMade >= 5)
        {
            if (CheckIfGameWon())
            {
                gameStatusText.GetComponent<TextMeshProUGUI>().text = userName + " Won!";
                networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.GameOver.ToString() + "," + userName);
                restartGameButton.SetActive(true);
                GameOver();
                return true;
            }
            else if (numberOfTotalMovesMade == 9)
            {
                gameStatusText.GetComponent<TextMeshProUGUI>().text = "Game Drawn";
                networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.GameDrawn.ToString());
                restartGameButton.SetActive(true);
                GameOver();
                return true;
            }
        }
        return false;
    }

    bool CheckIfGameWon()
    {
        //Checks for rows having same symbol
        for (int i = 0; i < 7; i+=3)
        {
            string leftCell = ticTacToeButtonCellArray[i].GetComponentInChildren<TextMeshProUGUI>().text;
            string middleCell = ticTacToeButtonCellArray[i + 1].GetComponentInChildren<TextMeshProUGUI>().text;
            string rightCell = ticTacToeButtonCellArray[i + 2].GetComponentInChildren<TextMeshProUGUI>().text;

            if (leftCell != "" && leftCell == middleCell && leftCell == rightCell)
                return true;
        }
        //Checks for columns having same symbol
        for (int i = 0; i < 3; i++)
        {
            string topCell = ticTacToeButtonCellArray[i].GetComponentInChildren<TextMeshProUGUI>().text;
            string middleCell = ticTacToeButtonCellArray[i + 3].GetComponentInChildren<TextMeshProUGUI>().text;
            string bottomCell = ticTacToeButtonCellArray[i + 6].GetComponentInChildren<TextMeshProUGUI>().text;

            if (topCell != "" && topCell == middleCell && topCell == bottomCell)
                return true;
        }
        //Checks for diagonals
        string topLeftCorner = ticTacToeButtonCellArray[0].GetComponentInChildren<TextMeshProUGUI>().text;
        string middleGridCell = ticTacToeButtonCellArray[4].GetComponentInChildren<TextMeshProUGUI>().text;
        string topRightCorner = ticTacToeButtonCellArray[2].GetComponentInChildren<TextMeshProUGUI>().text;

        if (topLeftCorner != "" && topLeftCorner == middleGridCell & topLeftCorner == ticTacToeButtonCellArray[8].GetComponentInChildren<TextMeshProUGUI>().text)
            return true;
        if (topRightCorner != "" && topRightCorner == middleGridCell && topRightCorner == ticTacToeButtonCellArray[6].GetComponentInChildren<TextMeshProUGUI>().text)
            return true;

        return false;
    }

    public void UpdateGameStatusText(string gameText)
    {
        gameStatusText.GetComponent<TextMeshProUGUI>().text = gameText;
    }

    public void RestartGameButtonPressed()
    {
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.RestartGame.ToString());
        ChangeGameState(GameStates.PlayiongTicTacToe);
    }

    void ShowLeaderboardButtonPressed()
    {
        ChangeGameState(GameStates.Leaderboard);
    }

    public void AddPlayerToLeaderboardTextBox(string playerUsername, string playerWins)
    {
        leaderboardNamesText.GetComponent<TextMeshProUGUI>().text += playerUsername;
        leaderboardWinsText.GetComponent<TextMeshProUGUI>().text += playerWins;

    }
    public void AddOpponenetMessageToChat(string message)
    {
        chatScrollViewText.text += message;
    }

    public void ChangeGameState(int newState)
    {

        inputFieldUsername.SetActive(false);
        inputFieldPassword.SetActive(false);
        buttonSubmit.SetActive(false);
        toggleLogIn.SetActive(false);
        toggleCreateAccount.SetActive(false);
        findGameSessionButton.SetActive(false);
        mainMenuGameButton.SetActive(false);
        nameTextBox.SetActive(false);
        passwordTextBox.SetActive(false);
        ticTacToeBoard.SetActive(false);
        gameStatusText.SetActive(false);
        restartGameButton.SetActive(false);
        leaderboardButton.SetActive(false);
        leaderboardNamesText.SetActive(false);
        leaderboardWinsText.SetActive(false);
        chatScrollView.SetActive(false);
        chatInputField.SetActive(false);
        searchGameRoomInputField.SetActive(false);
        gameRoomNumberText.SetActive(false);
        searchGameRoomButton.SetActive(false);
        chatInputFieldSubmitButton.SetActive(false);
        replayDropDown.SetActive(false);
        replayDropDownButton.SetActive(false);
        replayDropDownText.SetActive(false);
        gameStarted = false;


        if (newState == GameStates.Login)
        {
            inputFieldUsername.SetActive(true);
            inputFieldPassword.SetActive(true);
            buttonSubmit.SetActive(true);
            toggleLogIn.SetActive(true);
            toggleCreateAccount.SetActive(true);
            nameTextBox.SetActive(true);
            passwordTextBox.SetActive(true);
        }
        else if (newState == GameStates.MainMenu)
        {
            findGameSessionButton.SetActive(true);
            leaderboardButton.SetActive(true);
            searchGameRoomInputField.SetActive(true);
            gameRoomNumberText.SetActive(true);
            searchGameRoomButton.SetActive(true);
            //Drop Down UI
            replayDropDown.SetActive(true);
            replayDropDownButton.SetActive(true);
            replayDropDownText.SetActive(true);
        }
        else if (newState == GameStates.WaitingForMatch)
        {
            mainMenuGameButton.SetActive(true);
        }
        else if (newState == GameStates.PlayiongTicTacToe)
        {
            numberOfTotalMovesMade = 0;
            mainMenuGameButton.SetActive(true);
            ticTacToeBoard.SetActive(true);
            gameStatusText.SetActive(true);
            chatScrollView.SetActive(true);
            chatInputField.SetActive(true);
            chatInputFieldSubmitButton.SetActive(true);
            ResetAllCellButtonTextValues();
            replayRecorder = new ReplayRecorder();
        }
        else if (newState == GameStates.Leaderboard)
        {
            mainMenuGameButton.SetActive(true);
            leaderboardNamesText.SetActive(true);
            leaderboardWinsText.SetActive(true);
            leaderboardNamesText.GetComponent<TextMeshProUGUI>().text = "\tLeaderboard\n\n";
            leaderboardWinsText.GetComponent<TextMeshProUGUI>().text = "\n\n";
            networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.ShowLeaderboard.ToString());
        }
    }

    public string ConverCurrentTicTacToeBoardToString()
    {
        string ans = "";

        for (int i = 0; i < ticTacToeButtonCellArray.Length; i++)
        {
            if (ticTacToeButtonCellArray[i].GetComponentInChildren<TextMeshProUGUI>().text == "")
            {
                ans += "B";

            }
            else
                ans += ticTacToeButtonCellArray[i].GetComponentInChildren<TextMeshProUGUI>().text;
        }
        return ans;
    }

    public void PopulateObserverTicTacToeBoard(string boardResult)
    {
        for(int i = 0; i < ticTacToeButtonCellArray.Length; i++)
        {
            if (boardResult[i] == 'X')
            {
                ticTacToeButtonCellArray[i].GetComponentInChildren<TextMeshProUGUI>().text = "X";
            }
            else if (boardResult[i] == 'O')
            {
                ticTacToeButtonCellArray[i].GetComponentInChildren<TextMeshProUGUI>().text = "O";
            }
            else if (boardResult[i] == 'B')
            {
                //Don't want to do anything here B is used to show a cell was blank
            }
        }
    }
    public void UpdateObserverTicTacToeBoard(int cellNumber, string symbol)
    {
        ticTacToeButtonCellArray[cellNumber].GetComponentInChildren<TextMeshProUGUI>().text = symbol;
    }

    public void SearchGameRoomButtonPressed()
    {
        if (searchGameRoomInputField.GetComponent<TMP_InputField>().text != "")
        {
            networkClient.GetComponent<NetworkedClient>().SendMessageToHost(string.Join(",", ClientToSeverSignifiers.SearchGameRoomRequestMade, searchGameRoomInputField.GetComponent<TMP_InputField>().text));
            searchGameRoomInputField.GetComponent<TMP_InputField>().text = "";
        }
    }
    void ChatInputFieldSubmitButtonPressed()
    {
        if (chatInputField.GetComponent<TMP_InputField>().text != "")
        {
            networkClient.GetComponent<NetworkedClient>().SendMessageToHost(string.Join(",", ClientToSeverSignifiers.PlayerSentMessageInChat, userName, chatInputField.GetComponent<TMP_InputField>().text));
            chatScrollViewText.text += "\n" + userName + ": " + chatInputField.GetComponent<TMP_InputField>().text;
            chatInputField.GetComponent<TMP_InputField>().text = "";
        }
    }
    void ReplayDropDownButtonPressed()
    {
        //Change to game play scene
        //Load in array containing 
            //First player's turn symbol
            //Time between turns
            //Cell number of turn

        //How to get index of selection
        int menuIndex = replayDropDown.GetComponent<TMP_Dropdown>().value;
        Debug.Log(menuIndex);

        //Index replay 

        List<TMP_Dropdown.OptionData> menuOptions = replayDropDown.GetComponent<TMP_Dropdown>().options;

        //Gives a list of existing options

        foreach (TMP_Dropdown.OptionData m in menuOptions)
            Debug.Log(m.text);
        
        TMP_Dropdown.OptionData add = new TMP_Dropdown.OptionData();
        add.text = "Eric's addition";
        menuOptions.Add(add);
        replayDropDown.GetComponent<TMP_Dropdown>().options = menuOptions;
        //Clear drop downs do this on start
        //List<TMP_Dropdown.OptionData> clearOptions = new List<TMP_Dropdown.OptionData>();
        //replayDropDown.GetComponent<TMP_Dropdown>().options = clearOptions;
    }

    public void OpponentMadeMove(int cellNumberOfMovePlayed)
    {
        replayRecorder.cellNumberOfTurn[ReplayRecorder.turnNumber] = cellNumberOfMovePlayed;
        replayRecorder.timeBetweenTurnsArray[ReplayRecorder.turnNumber] = opponentTurnCounter;
        opponentTurnCounter = 0.0f;
        opponentsTurn = false;
        ReplayRecorder.turnNumber++;
    }

    void GameOver()
    {
        //Can also call game over when one of the players leaves the game
        //Just send a message if a player selects the main menu button and check if game is over or not

        gameStarted = false;
        replayRecorder.numberOfTurns = ReplayRecorder.turnNumber;

        //Debug.Log("Replay name id: " + replayRecorder.gameID);
        //Debug.Log("Starting symbol: " + replayRecorder.startingSymbol);
        //Debug.Log("Number of turns: " + replayRecorder.numberOfTurns);
        
        //for (int i = 0; i < replayRecorder.numberOfTurns; i++)
        //{
        //    Debug.Log("Turn " + i + ": Cell Number : " + replayRecorder.cellNumberOfTurn[i] + "  Time: " + replayRecorder.timeBetweenTurnsArray[i]);
        //}

        //Get info from server as to which number it should be named
        // Add name 

        //Send info to the server
        SendInformationToServer();
    }

    void SendInformationToServer()
    {
        string recordingPacket = string.Join(",", replayRecorder.gameID, replayRecorder.startingSymbol, replayRecorder.numberOfTurns);
        for (int i = 0; i < replayRecorder.numberOfTurns; i++)
        {
            recordingPacket += "," + replayRecorder.timeBetweenTurnsArray[i];
        }
        for (int i = 0; i < replayRecorder.numberOfTurns; i++)
        {
            recordingPacket += "," + replayRecorder.cellNumberOfTurn[i];
        }
        Debug.Log(recordingPacket);

        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(string.Join(",", ClientToSeverSignifiers.RecordingSentToServer, recordingPacket));
    }
}

public static class GameStates
{
    public const int Login = 1;
    public const int MainMenu = 2;
    public const int WaitingForMatch = 3;
    public const int PlayiongTicTacToe = 4;
    public const int Leaderboard = 5;
}

public class ReplayRecorder
{
    public static int turnNumber = 0;
    public string name;
    public int numberOfTurns;
    public string startingSymbol;
    public float[] timeBetweenTurnsArray;
    public int[] cellNumberOfTurn;
    public float gameID;

    public ReplayRecorder()
    {
        name = "";
        numberOfTurns = 0;
        startingSymbol = "";
        timeBetweenTurnsArray = new float[9];
        cellNumberOfTurn = new int[9];
    }
}