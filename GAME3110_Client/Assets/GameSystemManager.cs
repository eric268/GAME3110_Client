using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameSystemManager : MonoBehaviour
{
    GameObject inputFieldUsername, inputFieldPassword, buttonSubmit, toggleLogIn, toggleCreateAccount;
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        
        foreach(GameObject go in allObjects)
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
      
        
        }
        buttonSubmit.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SubmitButtonPressed()
    {
        Debug.Log("Submit button pressed");
    }
}
