using UnityEngine;
using UnityEngine.UI;
public class Dots : MonoBehaviour
{
    [SerializeField] private GameScrollRect GameScrollRect;
    [SerializeField] private GameObject DotPrefab;
    [SerializeField] private Transform DotHolder;
    private void Start()
    {
        float NumberOfPanels = GameScrollRect.Panels.Length;
        for (int i = 0; i < NumberOfPanels; i++)
        {
            if (i != 0 && i != NumberOfPanels - 1)
            {
                GameObject dot = Instantiate(DotPrefab, DotHolder);
                //save the list of image shits to change the color later.
            }
        }
    }
    
}
