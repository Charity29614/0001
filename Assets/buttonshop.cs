using UnityEngine;
using UnityEngine.UI;
public class buttonshop : MonoBehaviour
{
    // Reference to the button component
    public Button myButton;

    void Start()
    {
        // Check if the button reference is assigned in the inspector
        if (myButton != null)
        {
            // Add the event listener to the button
            myButton.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("Button reference is not assigned!");
        }
    }

    // Function to call when the button is clicked
    void OnButtonClick()
    {
        Debug.Log("Button clicked!");
        // You can add any functionality you want here.
    }
}