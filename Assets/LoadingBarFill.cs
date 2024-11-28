using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
public class LoadingBarFill : MonoBehaviour
{
    public Slider LoadingBar;
    public TextMeshProUGUI ProgressText;
    private float tweenValue = 0;
    void Start()
    {
        AsyncOperation Operation = SceneManager.LoadSceneAsync("3Sections");
        Operation.allowSceneActivation = false;
        DOTween.To(() => tweenValue, x => tweenValue = x, 100f, 6f)
            .OnUpdate(() =>
            {
                LoadingBar.value = Mathf.RoundToInt(tweenValue);
                ProgressText.text = Mathf.RoundToInt(tweenValue).ToString() + "%";
            })
            .OnComplete(() =>
            {

                LoadingBar.value = 100f;
                ProgressText.text = "100%";
                Operation.allowSceneActivation = true;
            });
    }
}