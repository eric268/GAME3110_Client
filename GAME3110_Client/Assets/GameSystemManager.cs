using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine;

public class GameSystemManager : MonoBehaviour
{
    GameObject inputFieldUsername, inputFieldPassword, chatInputField, buttonSubmit, toggleLogIn, toggleCreateAccount;
    public GameObject networkClient;
    GameObject findGameSessionButton, mainMenuGameButton, restartGameButton, leaderboardButton, leaderboardNamesText, leaderboardWinsText, chatScrollView;
    GameObject nameTextBox, passwordTextBox;
    GameObject ticTacToeBoard,gameStatusText;
    GameObject searchGameRoomInputField;
    public Button[] ticTacToeButtonCellArray;
    string playersTicTacToeSymbol,opponentsTicTacToeSymbol;
    public bool myTurnToMove = false;
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



        }

        buttonSubmit.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);
        toggleCreateAccount.GetComponent<Toggle>().onValueChanged.AddListener(ToggleCreateValueChanged);
        toggleLogIn.GetComponent<Toggle>().onValueChanged.AddListener(ToggleLogInValueChanged);

        findGameSessionButton.GetComponent<Button>().onClick.AddListener(FindGameSessionButtonPressed);
        mainMenuGameButton.GetComponent<Button>().onClick.AddListener(MainMenuGameButtonPressed);
        restartGameButton.GetComponent<Button>().onClick.AddListener(RestartGameButtonPressed);
        leaderboardButton.GetComponent<Button>().onClick.AddListener(ShowLeaderboardButtonPressed); 
        ticTacToeButtonCellArray = ticTacToeBoard.GetComponentsInChildren<Button>();
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
                return true;
            }
            else if (numberOfTotalMovesMade == 9)
            {
                gameStatusText.GetComponent<TextMeshProUGUI>().text = "Game Drawn";
                networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.GameDrawn.ToString());
                restartGameButton.SetActive(true);
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
            ResetAllCellButtonTextValues();
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
}

public static class GameStates
{
    public const int Login = 1;
    public const int MainMenu = 2;
    public const int WaitingForMatch = 3;
    public const int PlayiongTicTacToe = 4;
    public const int Leaderboard = 5;
}
