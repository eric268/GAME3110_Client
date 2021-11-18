using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine;

public class GameSystemManager : MonoBehaviour
{
    public GameObject networkClient;

    //Toggles
    GameObject toggleLogIn, toggleCreateAccount;

    //Input Fields
    GameObject inputFieldUsername, inputFieldPassword, chatInputField, searchGameRoomInputField;

    //Buttons
    GameObject findGameSessionButton, mainMenuGameButton, leaderboardButton, leaderboardNamesText, leaderboardWinsText, chatScrollView, searchGameRoomButton, 
        chatInputFieldSubmitButton, replayDropDownButton, quitGameButton, buttonSubmit, clearReplayDropDownButton, leaveGameQueueButton, replayPlayButton,
        replayPauseButton, replayRestartButton, leaderboardMainMenuButton;

    //Text boxes
    GameObject nameTextBox, passwordTextBox, gameRoomNumberText, replayDropDownText, errorMessageText;
    public GameObject gameStatusText;

    GameObject replayDropDown, ticTacToeBoard;
    public ReplayRecorder replayRecorder;
    public Button[] ticTacToeButtonCellArray;
    public TextMeshProUGUI chatScrollViewText;

    string playersTicTacToeSymbol,opponentsTicTacToeSymbol, currentReplaySymbol;
    public string userName;

    public bool myTurnToMove = false, isWatchingReplay, recordingIsPaused = false, displayErrorScreenMessage = false, opponentsTurn = false, gameStarted = false;

    float playerTurnCounter = 0.0f, opponentTurnCounter = 0.0f, replayRecordingCounter = 0.0f, errorMessageTimer = 2.5f, errorMessageCounter = 0.0f;
    int numberOfTotalMovesMade = 0;

    public int gameSessionID;

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
            else if (go.name == "ClearReplayDropDownButton")
                clearReplayDropDownButton = go;
            else if (go.name == "LeaveGameQueueButton")
                leaveGameQueueButton = go;
            else if (go.name == "ErrorMessageText")
                errorMessageText = go;
            else if (go.name == "QuitGameButton")
                quitGameButton = go;
            else if (go.name == "ReplayPlayButton")
                replayPlayButton = go;
            else if (go.name == "ReplayPauseButton")
                replayPauseButton = go;
            else if (go.name == "ReplayRestartButton")
                replayRestartButton = go;
            else if (go.name == "LeaderboardMainMenuButton")
                leaderboardMainMenuButton = go;

        }

        buttonSubmit.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);
        toggleCreateAccount.GetComponent<Toggle>().onValueChanged.AddListener(ToggleCreateValueChanged);
        toggleLogIn.GetComponent<Toggle>().onValueChanged.AddListener(ToggleLogInValueChanged);

        findGameSessionButton.GetComponent<Button>().onClick.AddListener(FindGameSessionButtonPressed);
        mainMenuGameButton.GetComponent<Button>().onClick.AddListener(MainMenuGameButtonPressed);
        leaderboardButton.GetComponent<Button>().onClick.AddListener(ShowLeaderboardButtonPressed);
        searchGameRoomButton.GetComponent<Button>().onClick.AddListener(SearchGameRoomButtonPressed);
        ticTacToeButtonCellArray = ticTacToeBoard.GetComponentsInChildren<Button>();
        chatInputFieldSubmitButton.GetComponent<Button>().onClick.AddListener(ChatInputFieldSubmitButtonPressed);
        replayDropDownButton.GetComponent<Button>().onClick.AddListener(ReplayDropDownButtonPressed);
        clearReplayDropDownButton.GetComponent<Button>().onClick.AddListener(ClearReplayDropDownButtonPressed);
        leaveGameQueueButton.GetComponent<Button>().onClick.AddListener(LeaveGameQueueButtonPressed);
        quitGameButton.GetComponent<Button>().onClick.AddListener(QuitGameButtonPressed);
        replayPlayButton.GetComponent<Button>().onClick.AddListener(PlayReplayButtonPressed);
        replayPauseButton.GetComponent<Button>().onClick.AddListener(PauseReplayButtonPressed);
        replayRestartButton.GetComponent<Button>().onClick.AddListener(RestartReplayButtonPressed);
        leaderboardMainMenuButton.GetComponent<Button>().onClick.AddListener(MainMenuGameButtonPressed);
        errorMessageText.GetComponent<TextMeshProUGUI>().text = "";
        AddListenersToButtonCellArray();
        ChangeGameState(GameStates.Login);
    }

    // Update is called once per frame
    void Update()
    {
        if (displayErrorScreenMessage)
        {
            DisplayErrorMessage();
        }

        if (isWatchingReplay && !recordingIsPaused)
        {
            UpdateTicTacToeRecording();
        }

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
        Debug.Log("Submit Button Pressed");

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
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.AddToGameSessionQueue.ToString());
        ChangeGameState(GameStates.WaitingForMatch);
    }

    private void MainMenuGameButtonPressed()
    {
        ChangeGameState(GameStates.MainMenu);
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.PlayerLeftGameRoom.ToString());
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
        gameStatusText.GetComponent<TextMeshProUGUI>().text ="OBSERVER";
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
                GameOver();
                return true;
            }
            else if (numberOfTotalMovesMade == 9)
            {
                gameStatusText.GetComponent<TextMeshProUGUI>().text = "Game Drawn";
                networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.GameDrawn.ToString());
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
        int menuIndex = replayDropDown.GetComponent<TMP_Dropdown>().value;
        Debug.Log(replayDropDown.GetComponent<TMP_Dropdown>().options[menuIndex].text);
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(string.Join(",", ClientToSeverSignifiers.RecordingRequestedFromServer,userName, menuIndex));
        replayDropDown.GetComponent<TMP_Dropdown>().RefreshShownValue();
    }

    public void OpponentMadeMove(int cellNumberOfMovePlayed)
    {
        replayRecorder.cellNumberOfTurn[ReplayRecorder.turnNumber] = cellNumberOfMovePlayed;
        replayRecorder.timeBetweenTurnsArray[ReplayRecorder.turnNumber] = opponentTurnCounter;
        opponentTurnCounter = 0.0f;
        opponentsTurn = false;
        ReplayRecorder.turnNumber++;
    }

    public void GameOver()
    {
        gameStarted = false;
        replayRecorder.numberOfTurns = ReplayRecorder.turnNumber;
        replayRecorder.username = userName;
        //Send info to the server
        SendRecordingToServer();
    }

    void SendRecordingToServer()
    {
        string recordingPacket = string.Join(",", replayRecorder.username, replayRecorder.startingSymbol, replayRecorder.numberOfTurns);
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

    public void LoadAndBeginRecording(string recordingInfo)
    {

        string[] csv = recordingInfo.Split(',');
        //CSV 0 is the signifier so start at 1

        ChangeGameState(GameStates.PlayingTicTacToe);
        replayRecorder.username = (csv[1]);
        replayRecorder.startingSymbol = csv[2];
        replayRecorder.numberOfTurns = int.Parse(csv[3]);

        //There are numberOfTurns iterations for both time and cell location so need to double it
        int positionalCounter = 0;
        for (int i = 4; i < replayRecorder.numberOfTurns + 4; i++)
        {
            replayRecorder.timeBetweenTurnsArray[positionalCounter++] = float.Parse(csv[i]);
        }
        positionalCounter = 0;
        for (int i = replayRecorder.numberOfTurns + 4; i < (2* replayRecorder.numberOfTurns) + 4; i++)
        {
            replayRecorder.cellNumberOfTurn[positionalCounter++] =int.Parse(csv[i]);
        }
        isWatchingReplay = true;
        ReplayRecorder.turnNumber = 0;
        currentReplaySymbol = replayRecorder.startingSymbol;
        gameStatusText.GetComponent<TextMeshProUGUI>().text ="Replay";
        replayPlayButton.SetActive(true);
        replayPauseButton.SetActive(true);
        replayRestartButton.SetActive(true);
        chatScrollViewText.text = "";
    }

    void UpdateTicTacToeRecording()
    {
        //Debug.Log("Start replay");
        //Debug.Log("Counter: " + replayRecordingCounter);
        replayRecordingCounter += Time.deltaTime;


        if (replayRecordingCounter >= replayRecorder.timeBetweenTurnsArray[ReplayRecorder.turnNumber])
        {
            int cellToChange = replayRecorder.cellNumberOfTurn[ReplayRecorder.turnNumber];
            if (ReplayRecorder.turnNumber % 2 == 0)
            {
                ticTacToeButtonCellArray[cellToChange].GetComponentInChildren<TextMeshProUGUI>().text = replayRecorder.startingSymbol;
                currentReplaySymbol = (replayRecorder.startingSymbol == "X") ? "O" : "X";
                gameStatusText.GetComponent<TextMeshProUGUI>().text = "Replay";
            }
            else
            {
                string otherSymbol = (replayRecorder.startingSymbol == "X") ? "O" : "X";
                ticTacToeButtonCellArray[cellToChange].GetComponentInChildren<TextMeshProUGUI>().text = otherSymbol;
                currentReplaySymbol = replayRecorder.startingSymbol;
                gameStatusText.GetComponent<TextMeshProUGUI>().text = "Replay";
            }
            replayRecordingCounter = 0.0f;
            ReplayRecorder.turnNumber++;

            if (replayRecorder.numberOfTurns <= ReplayRecorder.turnNumber)
            {
                isWatchingReplay = false;
            }
        }
    }

    public void GetNumberOfSavedRecordingsFromServer()
    {
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(string.Join(",", ClientToSeverSignifiers.RequestNumberOfSavedRecordings, userName));
    }

    public void UpdateRecordingDropdownMenu(int numberOfSavedRecordings)
    {
        List <TMP_Dropdown.OptionData> clearOptions = new List<TMP_Dropdown.OptionData>();
        replayDropDown.GetComponent<TMP_Dropdown>().options = clearOptions;

        for (int i = 0; i < numberOfSavedRecordings; i++)
        {
            TMP_Dropdown.OptionData recordingSlot = new TMP_Dropdown.OptionData();
            recordingSlot.text = "Recording " + (i + 1);
            replayDropDown.GetComponent<TMP_Dropdown>().options.Add(recordingSlot);
        }
        replayDropDown.GetComponent<TMP_Dropdown>().RefreshShownValue();
    }
    public void PauseReplayButtonPressed()
    {
        recordingIsPaused = true;
    }
    public void PlayReplayButtonPressed()
    {
        recordingIsPaused = false;
    }

    public void RestartReplayButtonPressed()
    {
        ReplayRecorder.turnNumber = 0;
        replayRecordingCounter = 0;
        isWatchingReplay = true;
        recordingIsPaused = false;
        EraseAllTicTacToeCells();
    }


    void ClearReplayDropDownButtonPressed()
    {
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(string.Join(",", ClientToSeverSignifiers.ClearRecordingOnServer, userName));
    }

    void LeaveGameQueueButtonPressed()
    {
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(string.Join(",", ClientToSeverSignifiers.PlayerHasLeftGameQueue));
        ChangeGameState(GameStates.MainMenu);
    }

    public void SetErrorDisplayMessage(string message)
    {
        displayErrorScreenMessage = true;
        errorMessageText.GetComponent<TextMeshProUGUI>().text = message;
        errorMessageText.SetActive(true);
    }

    void DisplayErrorMessage()
    {
        errorMessageCounter += Time.deltaTime;
        if (errorMessageTimer <= errorMessageCounter)
        {
            errorMessageCounter = 0.0f;
            errorMessageText.GetComponent<TextMeshProUGUI>().text = "";
            displayErrorScreenMessage = false;
            errorMessageText.SetActive(false);
        }
    }

    public void UpdateLogInInputFields(bool resetUsernameField, bool resetPasswordField)
    {
        if (resetUsernameField)
            inputFieldUsername.GetComponent<InputField>().text = "";

        if (resetPasswordField)
            inputFieldPassword.GetComponent<InputField>().text = "";
    }

    void EraseAllTicTacToeCells()
    {
        for (int i = 0; i < 9; i++)
        {
            ticTacToeButtonCellArray[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
    }

    void QuitGameButtonPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
    }

    public void ChangeGameState(int newState)
    {
        Debug.Log("State Changed");
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
        leaveGameQueueButton.SetActive(false);
        errorMessageText.SetActive(false);
        quitGameButton.SetActive(false);
        leaderboardMainMenuButton.SetActive(false);
        replayPlayButton.SetActive(false);
        replayPauseButton.SetActive(false);
        replayRestartButton.SetActive(false);
        EraseAllTicTacToeCells();
        isWatchingReplay = false;
        ReplayRecorder.turnNumber = 0;
        opponentTurnCounter = 0.0f;
        playerTurnCounter = 0.0f;
        replayRecordingCounter = 0.0f;


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
            Debug.Log("Main Menu Loaded");
            findGameSessionButton.SetActive(true);
            leaderboardButton.SetActive(true);
            searchGameRoomInputField.SetActive(true);
            gameRoomNumberText.SetActive(true);
            searchGameRoomButton.SetActive(true);
            quitGameButton.SetActive(true);
            //Drop Down UI
            replayDropDown.SetActive(true);
            replayDropDownButton.SetActive(true);
            replayDropDownText.SetActive(true);

            GetNumberOfSavedRecordingsFromServer();
        }
        else if (newState == GameStates.WaitingForMatch)
        {
            Debug.Log("Waiting For Match Loaded");
            leaveGameQueueButton.SetActive(true);
        }
        else if (newState == GameStates.PlayingTicTacToe)
        {
            Debug.Log("Match against player Loaded");
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
            leaderboardMainMenuButton.SetActive(true);
            leaderboardNamesText.SetActive(true);
            leaderboardWinsText.SetActive(true);
            leaderboardNamesText.GetComponent<TextMeshProUGUI>().text = "\t  Leaderboard\n\n";
            leaderboardWinsText.GetComponent<TextMeshProUGUI>().text = "\n\n";
            networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.ShowLeaderboard.ToString());
        }
    }
}


public static class GameStates
{
    public const int Login = 1;
    public const int MainMenu = 2;
    public const int WaitingForMatch = 3;
    public const int PlayingTicTacToe = 4;
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
    public string username;

    public ReplayRecorder()
    {
        name = "";
        numberOfTurns = 0;
        startingSymbol = "";
        timeBetweenTurnsArray = new float[9];
        cellNumberOfTurn = new int[9];
    }
}