// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using AdvancedInputFieldPlugin;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ModesController : MonoBehaviour
{
    [SerializeField] private AdvancedInputField resizeVerticalInputField;
    [SerializeField] private RectTransform ButtonVertical;
    [SerializeField] private Button EnableChatButton;
    private Canvas canvas;
    private Vector2 originalResizeVerticalPosition;
    //private Vector2 originalButtonPosition;
    private float keyboardHeight;
    public float offsetThing = -0.1f;
    public Canvas Canvas
    {
        get
        {
            if (canvas == null)
            {
                canvas = resizeVerticalInputField.GetComponentInParent<Canvas>();
            }

            return canvas;
        }
    }

    private void Start()
    {
#if (UNITY_ANDROID || UNITY_IOS)
        if (!Application.isEditor || Settings.SimulateMobileBehaviourInEditor)
        {
            NativeKeyboardManager.AddKeyboardHeightChangedListener(OnKeyboardHeightChanged);
        }
#endif
        originalResizeVerticalPosition = resizeVerticalInputField.RectTransform.anchoredPosition;
        Button VerticalButton = ButtonVertical.gameObject.GetComponent<Button>();

        VerticalButton.onClick.AddListener(
            () =>
            {
                resizeVerticalInputField.Clear();
                resizeVerticalInputField.ManualSelect();
            });

        EnableChatButton.onClick.AddListener(
            () =>
            {
                if (resizeVerticalInputField.RectTransform.anchoredPosition == originalResizeVerticalPosition)
                {
                    resizeVerticalInputField.ManualSelect();
                    Vector2 position = resizeVerticalInputField.RectTransform.anchoredPosition;
                    //Vector2 position1 = ButtonVertical.anchoredPosition;

                    float currentBottomY = GetAbsoluteBottomY(resizeVerticalInputField.RectTransform, offsetThing);
                    //float currentBottomY1 = GetAbsoluteBottomY(ButtonVertical, offsetThing);

                    float targetBottomY = keyboardHeight / Canvas.scaleFactor;
                    position.y += (targetBottomY - currentBottomY);
                    //position1.y += (targetBottomY - currentBottomY1);

                    resizeVerticalInputField.RectTransform.anchoredPosition = position;
                    //ButtonVertical.anchoredPosition = position1;
                }
            });
    }

    //private void Update()
    //{
    //    var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
    //    if (activeTouches.Count > 0)
    //    {
    //        UnityEngine.InputSystem.EnhancedTouch.Touch touch = activeTouches[0];
    //        switch (touch.phase)
    //        {
    //            case UnityEngine.InputSystem.TouchPhase.Began:
    //                break;
    //            case UnityEngine.InputSystem.TouchPhase.Moved:
    //                break;
    //            case UnityEngine.InputSystem.TouchPhase.Ended:
    //                break;
    //        }
    //    }
    //}
    public void OnResizeHorizontalSizeChanged(Vector2 size)
    {
        Debug.Log("OnResizeHorizontalSizeChanged: " + size);
    }

    public void OnResizeVerticalBeginEdit()//BeginEditReason reason
    {
        Debug.Log("OnResizeVerticalBeginEdit");
        ////
        //originalButtonPosition = ButtonVertical.anchoredPosition;
#if (UNITY_ANDROID || UNITY_IOS)
        if (!Application.isEditor || Settings.SimulateMobileBehaviourInEditor)
        {
            OnResizeVerticalSizeChanged(resizeVerticalInputField.Size); //Move to top of keyboard on mobile on begin edit
        }
#endif
    }

    public void OnResizeVerticalEndEdit(string result, EndEditReason reason)
    {
        Debug.Log("OnResizeVerticalEndEdit");
        resizeVerticalInputField.RectTransform.anchoredPosition = originalResizeVerticalPosition;
        //ButtonVertical.anchoredPosition = originalButtonPosition;

    }
    //public void OnResizeVerticalEndEdit2()
    //{
    //    Debug.Log("OnResizeVerticalEndEdit");
    //    resizeVerticalInputField.RectTransform.anchoredPosition = originalResizeVerticalPosition;
    //    ButtonVertical.anchoredPosition = originalButtonPosition;

    //}

    public void OnResizeVerticalSizeChanged(Vector2 size)
    {
        Debug.Log("OnResizeVerticalSizeChanged: " + size);
        if (!resizeVerticalInputField.Selected) { return; }

#if (UNITY_ANDROID || UNITY_IOS)
        if (!Application.isEditor || Settings.SimulateMobileBehaviourInEditor)
        {
            Vector2 position = resizeVerticalInputField.RectTransform.anchoredPosition;
            //Vector2 position1 = ButtonVertical.anchoredPosition;

            float currentBottomY = GetAbsoluteBottomY(resizeVerticalInputField.RectTransform, offsetThing);
            //float currentBottomY1 = GetAbsoluteBottomY(ButtonVertical, offsetThing);

            float targetBottomY = keyboardHeight / Canvas.scaleFactor;
            position.y += (targetBottomY - currentBottomY);
            //position1.y += (targetBottomY - currentBottomY1);

            resizeVerticalInputField.RectTransform.anchoredPosition = position;
            //ButtonVertical.anchoredPosition = position1;
        }
#endif
    }

    //public float GetAbsoluteBottomY(RectTransform rectTransform)
    //{
    //	Vector3[] corners = new Vector3[4];
    //	rectTransform.GetWorldCorners(corners);

    //	float bottomY = corners[0].y;
    //	float normalizedBottomY = 0;
    //	if(Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
    //	{
    //		normalizedBottomY = bottomY / Screen.height;
    //	}
    //	else
    //	{
    //		Camera camera = Canvas.worldCamera;
    //		normalizedBottomY = (bottomY + camera.orthographicSize) / (camera.orthographicSize * 2);
    //	}

    //	return (normalizedBottomY * Canvas.pixelRect.height) / Canvas.scaleFactor;
    //}
    public float GetAbsoluteBottomY(RectTransform rectTransform, float offset)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // Calculate bottom Y position and apply the offset
        float bottomY = corners[0].y + offset;
        float normalizedBottomY = 0;
        if (Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            normalizedBottomY = bottomY / Screen.height;
        }
        else
        {
            Camera camera = Canvas.worldCamera;
            normalizedBottomY = (bottomY + camera.orthographicSize) / (camera.orthographicSize * 2);
        }

        return (normalizedBottomY * Canvas.pixelRect.height) / Canvas.scaleFactor;
    }


    public void OnKeyboardHeightChanged(int keyboardHeight)
    {
        Debug.Log("OnKeyboardHeightChanged: " + keyboardHeight);
        this.keyboardHeight = keyboardHeight;

        if (resizeVerticalInputField.Selected)
        {
            Vector2 position = resizeVerticalInputField.RectTransform.anchoredPosition;
            //Vector2 position1 = ButtonVertical.anchoredPosition;

            float currentBottomY = GetAbsoluteBottomY(resizeVerticalInputField.RectTransform, offsetThing);
            //float currentBottomY1 = GetAbsoluteBottomY(ButtonVertical, offsetThing);

            float targetBottomY = keyboardHeight / Canvas.scaleFactor;
            position.y += (targetBottomY - currentBottomY);
            //position1.y += (targetBottomY - currentBottomY1);

            resizeVerticalInputField.RectTransform.anchoredPosition = position;
            //ButtonVertical.anchoredPosition = position1;
        }
    }
}
