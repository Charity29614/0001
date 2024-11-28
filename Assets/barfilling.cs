using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
public class barfilling : MonoBehaviour
{
    public Image Fillbar; // Reference to the Image component for the fill bar

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    // Coroutine to handle scene loading and progress bar filling
    IEnumerator LoadSceneAsync()
    {
        // Start loading the scene asynchronously
        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync("Game Scene");
        sceneLoadOperation.allowSceneActivation = true; // Don't activate the scene immediately

        // Update the progress bar while loading the scene
        while (!sceneLoadOperation.isDone)
        {
            // Get the loading progress (between 0 and 0.9)
            float progress = sceneLoadOperation.progress;

            // If the progress is less than 0.9, scale it to the range 0 to 1
            Fillbar.fillAmount = Mathf.Clamp01(progress / 0.9f);

            // If the progress reaches 0.9, consider the scene loading complete
            if (progress >= 0.9f)
            {
                // Ensure the progress bar shows full
                Fillbar.fillAmount = 1f;

                // Activate the scene after filling the progress bar to 100%
                sceneLoadOperation.allowSceneActivation = true;
            }

            // Wait until the next frame
            yield return null;
        }
    }
}