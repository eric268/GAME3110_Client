using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine;

public class GameSystemManager : MonoBehaviour
{
    GameObject inputFieldUsername, inputFieldPassword, buttonSubmit, toggleLogIn, toggleCreateAccount;
    GameObject networkClient;
    GameObject findGameSessionButton, mainMenuGameButton, restartGameButton, leaderboardButton, leaderboardNamesText, leaderboardWinsText;
    GameObject nameTextBox, passwordTextBox;
    GameObject ticTacToeBoard,gameStatusText;
    public Button[] ticTacToeButtonCellArray;
    string playersTicTacToeSymbol,opponentsTicTacToeSymbol;
    public bool myTurnToMove;
    int numberOfTotalMovesMade = 0;
    public string userName;
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
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.GameOver.ToString());
        //    networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.GameDrawn.ToString());
        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    ChangeGameState(GameStates.MainMenu);
        //}
        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    ChangeGameState(GameStates.WaitingForMatch);
        //}
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    ChangeGameState(GameStates.PlayiongTicTacToe);
        //}

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
                networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.TicTacToeMoveMade + "," + i);
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

}

public static class GameStates
{
    public const int Login = 1;
    public const int MainMenu = 2;
    public const int WaitingForMatch = 3;
    public const int PlayiongTicTacToe = 4;
    public const int Leaderboard = 5;
}
