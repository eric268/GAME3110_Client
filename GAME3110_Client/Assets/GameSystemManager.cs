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
    GameObject findGameSessionButton, placeHolderGameButton;
    GameObject nameTextBox, passwordTextBox;
    GameObject ticTacToeBoard;
    Button[] ticTacToeButtonCellArray;
    string playersTicTacToeSymbol;
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
            else if (go.name == "PlaceholderGameButton")
                placeHolderGameButton = go;
            else if (go.name == "NameTextbox")
                nameTextBox = go;
            else if (go.name == "PasswordTextbox")
                passwordTextBox = go;
            else if (go.name == "TicTacToeBoard")
                ticTacToeBoard = go;

        }

        buttonSubmit.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);
        toggleCreateAccount.GetComponent<Toggle>().onValueChanged.AddListener(ToggleCreateValueChanged);
        toggleLogIn.GetComponent<Toggle>().onValueChanged.AddListener(ToggleLogInValueChanged);

        findGameSessionButton.GetComponent<Button>().onClick.AddListener(FindGameSessionButtonPressed);
        placeHolderGameButton.GetComponent<Button>().onClick.AddListener(PlaceHolderGameButtonPressed);

        ticTacToeButtonCellArray = ticTacToeBoard.GetComponentsInChildren<Button>();
        AddListenersToButtonCellArray();
        playersTicTacToeSymbol = "X";
        ChangeGameState(GameStates.Login);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ChangeGameState(GameStates.Login);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ChangeGameState(GameStates.MainMenu);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            ChangeGameState(GameStates.WaitingForMatch);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            ChangeGameState(GameStates.PlayiongTicTacToe);
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
        int i = 0;
        for (; i < ticTacToeButtonCellArray.Length; i++)
        {
            if (button == ticTacToeButtonCellArray[i])
            {
                break;
            }
        }

        if (buttonText.text == "")
        {
            buttonText.text = playersTicTacToeSymbol;
        }
        else
            Debug.Log("Cell already contains letter");
        
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

    private void PlaceHolderGameButtonPressed()
    {
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToSeverSignifiers.TicTacToePlay + "");
    }



    public void ChangeGameState(int newState)
    {
        //inputFieldUsername, inputFieldPassword, buttonSubmit, toggleLogIn, toggleCreateAccount;
        //findGameSessionButton, placeHolderGameButton;

        inputFieldUsername.SetActive(false);
        inputFieldPassword.SetActive(false);
        buttonSubmit.SetActive(false);
        toggleLogIn.SetActive(false);
        toggleCreateAccount.SetActive(false);
        findGameSessionButton.SetActive(false);
        placeHolderGameButton.SetActive(false);
        nameTextBox.SetActive(false);
        passwordTextBox.SetActive(false);
        ticTacToeBoard.SetActive(false);

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
        }
        else if (newState == GameStates.WaitingForMatch)
        {
            
        }
        else if (newState == GameStates.PlayiongTicTacToe)
        {
            placeHolderGameButton.SetActive(true);
            ticTacToeBoard.SetActive(true);
        }
    }

}

public static class GameStates
{
    public const int Login = 1;
    public const int MainMenu = 2;
    public const int WaitingForMatch = 3;
    public const int PlayiongTicTacToe = 4;
}
