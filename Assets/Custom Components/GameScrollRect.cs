using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Game Scroll Rect", 37)]
    [SelectionBase]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    /// <summary>
    /// A component for making a child RectTransform scroll.
    /// </summary>
    /// <remarks>
    /// ScrollRect will not do any clipping on its own. Combined with a Mask component, it can be turned into a scroll view.
    /// </remarks>
    public class GameScrollRect : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup, IPointerClickHandler
    {
        /// <summary>
        /// A setting for which behavior to use when content moves beyond the confines of its container.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Scrollbar newScrollBar;
        ///
        ///     //Called when a button is pressed
        ///     public void Example(int option)
        ///     {
        ///         if (option == 0)
        ///         {
        ///             myScrollRect.movementType = ScrollRect.MovementType.Clamped;
        ///         }
        ///         else if (option == 1)
        ///         {
        ///             myScrollRect.movementType = ScrollRect.MovementType.Elastic;
        ///         }
        ///         else if (option == 2)
        ///         {
        ///             myScrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public enum MovementType
        {
            /// <summary>
            /// Unrestricted movement. The content can move forever.
            /// </summary>
            Unrestricted,

            /// <summary>
            /// Elastic movement. The content is allowed to temporarily move beyond the container, but is pulled back elastically.
            /// </summary>
            Elastic,

            /// <summary>
            /// Clamped movement. The content can not be moved beyond its container.
            /// </summary>
            Clamped,
        }

        /// <summary>
        /// Enum for which behavior to use for scrollbar visibility.
        /// </summary>
        public enum ScrollbarVisibility
        {
            /// <summary>
            /// Always show the scrollbar.
            /// </summary>
            Permanent,

            /// <summary>
            /// Automatically hide the scrollbar when no scrolling is needed on this axis. The viewport rect will not be changed.
            /// </summary>
            AutoHide,

            /// <summary>
            /// Automatically hide the scrollbar when no scrolling is needed on this axis, and expand the viewport rect accordingly.
            /// </summary>
            /// <remarks>
            /// When this setting is used, the scrollbar and the viewport rect become driven, meaning that values in the RectTransform are calculated automatically and can't be manually edited.
            /// </remarks>
            AutoHideAndExpandViewport,
        }

        [Serializable]
        /// <summary>
        /// Event type used by the ScrollRect.
        /// </summary>
        public class ScrollRectEvent : UnityEvent<Vector2> {}


        [SerializeField]
        private bool m_MobileScroll = false;
        public bool mobileScroll { get { return m_MobileScroll; } set { m_MobileScroll = value; } }

        [SerializeField]
        private bool m_Snap = false;
        public bool snap { get { return m_Snap; } set { m_Snap = value; } }

        [SerializeField]
        private bool m_GoBackToTop = true;
        public bool GoBackToTop { get { return m_GoBackToTop; } set { m_GoBackToTop = value; } }

        [SerializeField]
        private float m_PanelThreshHold = 11;
        public float PanelThreshHold { get { return m_PanelThreshHold; } set { m_PanelThreshHold = value; } }
        [SerializeField]
        private bool m_Horizontal = true;
        public bool horizontal { get { return m_Horizontal; } set { m_Horizontal = value; } }

        [SerializeField]
        private bool m_Vertical = true;
        public bool vertical { get { return m_Vertical; } set { m_Vertical = value; } }

        [SerializeField]
        private MovementType m_MovementType = MovementType.Elastic;

        /// <summary>
        /// The behavior to use when the content moves beyond the scroll rect.
        /// </summary>
        public MovementType movementType { get { return m_MovementType; } set { m_MovementType = value; } }

        [SerializeField]
        private float m_Elasticity = 0.1f;

        /// <summary>
        /// The amount of elasticity to use when the content moves beyond the scroll rect.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///
        ///     public void Start()
        ///     {
        ///         // assigns a new value to the elasticity of the scroll rect.
        ///         // The higher the number the longer it takes to snap back.
        ///         myScrollRect.elasticity = 3.0f;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public float elasticity { get { return m_Elasticity; } set { m_Elasticity = value; } }

        [SerializeField]
        private bool m_Inertia = true;
        public bool inertia { get { return m_Inertia; } set { m_Inertia = value; } }

        [SerializeField]
        private bool m_UseConstantInertiaSpeed = false;
        public bool UseConstatInertiaSpeed { get { return m_UseConstantInertiaSpeed; } set { m_UseConstantInertiaSpeed = value; } }

        [SerializeField]
        private float m_ConstantInertiaScrollSpeed = 100f;
        public float ConstantInertiaScrollSpeed { get { return m_ConstantInertiaScrollSpeed; } set { m_ConstantInertiaScrollSpeed = value; } }

        [SerializeField]
        private bool m_UseDecelerationRate = false;
        public bool useDecelerationRate { get { return m_UseDecelerationRate; } set { m_UseDecelerationRate = value; } }

        [SerializeField]
        private float m_DecelerationRate = 0.135f; // Only used when inertia is enabled
        public float decelerationRate { get { return m_DecelerationRate; } set { m_DecelerationRate = value; } }

        [SerializeField]
        private float m_ScrollSensitivity = 1.0f;
        public float scrollSensitivity { get { return m_ScrollSensitivity; } set { m_ScrollSensitivity = value; } }

        [SerializeField]
        private float m_ScrollMultiplier = 1.0f;
        public float scrollMultiplier { get { return m_ScrollMultiplier; } set { m_ScrollMultiplier = value; } }

        [SerializeField]
        private RectTransform m_Content;
        public RectTransform content { get { return m_Content; } set { m_Content = value; } }
        [SerializeField]
        private RectTransform m_Viewport;
        public RectTransform viewport { get { return m_Viewport; } set { m_Viewport = value; SetDirtyCaching(); } }

        [SerializeField] private RectTransform CanvasRectTransform;
        [SerializeField] private HorizontalLayoutGroup ViewPortHLG;
        [SerializeField] private HorizontalLayoutGroup ContentHLG;
        public Image[] Panels;
        [SerializeField] private Transform PanelHolder;
        //[SerializeField] private RectTransform SwipingImage;
        [SerializeField] private float ImageWidth = 1500;
        [SerializeField] private float SlideSpeed = 2;
        [SerializeField]
        private Scrollbar m_HorizontalScrollbar;

        /// <summary>
        /// Optional Scrollbar object linked to the horizontal scrolling of the ScrollRect.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Scrollbar newScrollBar;
        ///
        ///     public void Start()
        ///     {
        ///         // Assigns a scroll bar element to the ScrollRect, allowing you to scroll in the horizontal axis.
        ///         myScrollRect.horizontalScrollbar = newScrollBar;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public Scrollbar horizontalScrollbar
        {
            get
            {
                return m_HorizontalScrollbar;
            }
            set
            {
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                m_HorizontalScrollbar = value;
                if (m_Horizontal && m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
                SetDirtyCaching();
            }
        }

        [SerializeField]
        private Scrollbar m_VerticalScrollbar;
        public Scrollbar verticalScrollbar
        {
            get
            {
                return m_VerticalScrollbar;
            }
            set
            {
                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                m_VerticalScrollbar = value;
                if (m_Vertical && m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
                SetDirtyCaching();
            }
        }


        [SerializeField]
        private ScrollbarVisibility m_HorizontalScrollbarVisibility;

        /// <summary>
        /// The mode of visibility for the horizontal scrollbar.
        /// </summary>
        public ScrollbarVisibility horizontalScrollbarVisibility { get { return m_HorizontalScrollbarVisibility; } set { m_HorizontalScrollbarVisibility = value; SetDirtyCaching(); } }

        [SerializeField]
        private ScrollbarVisibility m_VerticalScrollbarVisibility;

        /// <summary>
        /// The mode of visibility for the vertical scrollbar.
        /// </summary>
        public ScrollbarVisibility verticalScrollbarVisibility { get { return m_VerticalScrollbarVisibility; } set { m_VerticalScrollbarVisibility = value; SetDirtyCaching(); } }

        [SerializeField]
        private float m_HorizontalScrollbarSpacing;

        /// <summary>
        /// The space between the scrollbar and the viewport.
        /// </summary>
        public float horizontalScrollbarSpacing { get { return m_HorizontalScrollbarSpacing; } set { m_HorizontalScrollbarSpacing = value; SetDirty(); } }

        [SerializeField]
        private float m_VerticalScrollbarSpacing;

        /// <summary>
        /// The space between the scrollbar and the viewport.
        /// </summary>
        public float verticalScrollbarSpacing { get { return m_VerticalScrollbarSpacing; } set { m_VerticalScrollbarSpacing = value; SetDirty(); } }

        [SerializeField]
        private ScrollRectEvent m_OnValueChanged = new ScrollRectEvent();

        /// <summary>
        /// Callback executed when the position of the child changes.
        /// </summary>
        /// <remarks>
        /// onValueChanged is used to watch for changes in the ScrollRect object.
        /// The onValueChanged call will use the UnityEvent.AddListener API to watch for
        /// changes.  When changes happen script code provided by the user will be called.
        /// The UnityEvent.AddListener API for UI.ScrollRect._onValueChanged takes a Vector2.
        ///
        /// Note: The editor allows the onValueChanged value to be set up manually.For example the
        /// value can be set to run only a runtime.  The object and script function to call are also
        /// provided here.
        ///
        /// The onValueChanged variable can be alternatively set-up at runtime.The script example below
        /// shows how this can be done.The script is attached to the ScrollRect object.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using UnityEngine.UI;
        ///
        /// public class ExampleScript : MonoBehaviour
        /// {
        ///     static ScrollRect scrollRect;
        ///
        ///     void Start()
        ///     {
        ///         scrollRect = GetComponent<ScrollRect>();
        ///         scrollRect.onValueChanged.AddListener(ListenerMethod);
        ///     }
        ///
        ///     public void ListenerMethod(Vector2 value)
        ///     {
        ///         Debug.Log("ListenerMethod: " + value);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public ScrollRectEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        // The offset from handle position to mouse down position
        private Vector2 m_PointerStartLocalCursor = Vector2.zero;
        protected Vector2 m_ContentStartPosition = Vector2.zero;

        private RectTransform m_ViewRect;

        protected RectTransform viewRect
        {
            get
            {
                if (m_ViewRect == null)
                    m_ViewRect = m_Viewport;
                if (m_ViewRect == null)
                    m_ViewRect = (RectTransform)transform;
                return m_ViewRect;
            }
        }

        protected Bounds m_ContentBounds;
        private Bounds m_ViewBounds;

        private Vector2 m_Velocity;
        private float storedVelocityX = 1f;
        private bool isSnapping = true; // Flag to control snapping execution
        private bool SnapToNextPanel = true;
        [System.NonSerialized] public float[] SnappingPoints = { 0, 0, 0 };
#if UNITY_EDITOR
        private float previousSizeDeltaY;
        private float previousImageWidth;
#endif
        /// <summary>
        /// The current velocity of the content.
        /// </summary>
        /// <remarks>
        /// The velocity is defined in units per second.
        /// </remarks>
        public Vector2 velocity { get { return m_Velocity; } set { m_Velocity = value; } }

        private bool m_Dragging;
#pragma warning disable CS0414
        private bool m_Scrolling;
#pragma warning restore CS0414

        private Vector2 m_PrevPosition = Vector2.zero;
        private Bounds m_PrevContentBounds;
        private Bounds m_PrevViewBounds;
        [NonSerialized]
        private bool m_HasRebuiltLayout = false;

        private bool m_HSliderExpand;
        private bool m_VSliderExpand;
        private float m_HSliderHeight;
        private float m_VSliderWidth;

        [System.NonSerialized] private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        private RectTransform m_HorizontalScrollbarRect;
        private RectTransform m_VerticalScrollbarRect;

        // field is never assigned warning
#pragma warning disable 649
        private DrivenRectTransformTracker m_Tracker;
#pragma warning restore 649

        protected GameScrollRect()
        {}

        /// <summary>
        /// Rebuilds the scroll rect data after initialization.
        /// </summary>
        /// <param name="executing">The current step in the rendering CanvasUpdate cycle.</param>
        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.Prelayout)
            {
                UpdateCachedData();
            }

            if (executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds();
                UpdateScrollbars(Vector2.zero);
                UpdatePrevData();

                m_HasRebuiltLayout = true;
            }
        }

        public virtual void LayoutComplete()
        {}

        public virtual void GraphicUpdateComplete()
        {}

        void UpdateCachedData()
        {
            Transform transform = this.transform;
            m_HorizontalScrollbarRect = m_HorizontalScrollbar == null ? null : m_HorizontalScrollbar.transform as RectTransform;
            m_VerticalScrollbarRect = m_VerticalScrollbar == null ? null : m_VerticalScrollbar.transform as RectTransform;

            // These are true if either the elements are children, or they don't exist at all.
            bool viewIsChild = (viewRect.parent == transform);
            bool hScrollbarIsChild = (!m_HorizontalScrollbarRect || m_HorizontalScrollbarRect.parent == transform);
            bool vScrollbarIsChild = (!m_VerticalScrollbarRect || m_VerticalScrollbarRect.parent == transform);
            bool allAreChildren = (viewIsChild && hScrollbarIsChild && vScrollbarIsChild);

            m_HSliderExpand = allAreChildren && m_HorizontalScrollbarRect && horizontalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_VSliderExpand = allAreChildren && m_VerticalScrollbarRect && verticalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_HSliderHeight = (m_HorizontalScrollbarRect == null ? 0 : m_HorizontalScrollbarRect.rect.height);
            m_VSliderWidth = (m_VerticalScrollbarRect == null ? 0 : m_VerticalScrollbarRect.rect.width);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_Horizontal && m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
            if (m_Vertical && m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            SetDirty();
            EnhancedTouchSupport.Enable();
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

            m_Dragging = false;
            m_Scrolling = false;
            m_HasRebuiltLayout = false;
            m_Tracker.Clear();
            m_Velocity = Vector2.zero;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            EnhancedTouchSupport.Disable();
            base.OnDisable();
        }

        /// <summary>
        /// See member in base class.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///
        ///     public void Start()
        ///     {
        ///         //Checks if the ScrollRect called "myScrollRect" is active.
        ///         if (myScrollRect.IsActive())
        ///         {
        ///             Debug.Log("The Scroll Rect is active!");
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public override bool IsActive()
        {
            return base.IsActive() && m_Content != null;
        }

        private void EnsureLayoutHasRebuilt()
        {
            if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Sets the velocity to zero on both axes so the content stops moving.
        /// </summary>
        public virtual void StopMovement()
        {
            m_Velocity = Vector2.zero;
        }

        public virtual void OnScroll(PointerEventData data)
        {
            if (!IsActive())
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (vertical && !horizontal)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    delta.y = delta.x;
                delta.x = 0;
            }
            if (horizontal && !vertical)
            {
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                    delta.x = delta.y;
                delta.y = 0;
            }

            if (data.IsScrolling())
                m_Scrolling = true;

            Vector2 position = m_Content.anchoredPosition;
            position += delta * m_ScrollSensitivity;
            if (m_MovementType == MovementType.Clamped)
                position += CalculateOffset(position - m_Content.anchoredPosition);

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            m_Velocity = Vector2.zero;
        }

        /// <summary>
        /// Handling for when the content is beging being dragged.
        /// </summary>
        ///<example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.EventSystems; // Required when using event data
        ///
        /// public class ExampleClass : MonoBehaviour, IBeginDragHandler // required interface when using the OnBeginDrag method.
        /// {
        ///     //Do this when the user starts dragging the element this script is attached to..
        ///     public void OnBeginDrag(PointerEventData data)
        ///     {
        ///         Debug.Log("They started dragging " + this.name);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        /// 


        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            //if (!Mathf.Approximately(content.anchoredPosition.x, SnappingPoints[0]) &&
            //    !Mathf.Approximately(content.anchoredPosition.x, SnappingPoints[1]) &&
            //    !Mathf.Approximately(content.anchoredPosition.x, SnappingPoints[2]))
            //{
            // Debug.Log("Checking snapping points...");

            bool isApproximate = false;

            for (int i = 0; i < SnappingPoints.Length; i++)
            {
                // Debug.Log($"Checking against Snapping Point {i}: {SnappingPoints[i]}");
                if (Mathf.Approximately(content.anchoredPosition.x, SnappingPoints[i]))
                {
                    isApproximate = true;
                    // Debug.Log($"Match found with Snapping Point {i}");
                    break;
                }
            }

            if (!isApproximate)
            {
                // Debug.Log("No approximate match found.");
                if (storedVelocityX != 0)
                {
                    m_Velocity = new Vector2(storedVelocityX, m_Velocity.y);
                    isSnapping = true;
                    // Debug.Log("Snapping started.");
                }
            }
            else
            {
                isSnapping = false;
            }



            //if (storedVelocityX != 0) 
            //{
            //    m_Velocity = new Vector2(storedVelocityX, m_Velocity.y);
            //}
            //    isSnapping = true;
            //}
        }
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!m_MobileScroll)
            {
                if (eventData.button != PointerEventData.InputButton.Left)
                    return;

                if (!IsActive())
                    return;

                UpdateBounds();

                m_PointerStartLocalCursor = Vector2.zero;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
                m_ContentStartPosition = m_Content.anchoredPosition;
                m_Dragging = true;
            }
        }

        /// <summary>
        /// Handling for when the content has finished being dragged.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.EventSystems; // Required when using event data
        ///
        /// public class ExampleClass : MonoBehaviour, IEndDragHandler // required interface when using the OnEndDrag method.
        /// {
        ///     //Do this when the user stops dragging this UI Element.
        ///     public void OnEndDrag(PointerEventData data)
        ///     {
        ///         Debug.Log("Stopped dragging " + this.name + "!");
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!m_MobileScroll)
            {
                if (eventData.button != PointerEventData.InputButton.Left)
                    return;

                m_Dragging = false;
            }
        }

        /// <summary>
        /// Handling for when the content is dragged.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.EventSystems; // Required when using event data
        ///
        /// public class ExampleClass : MonoBehaviour, IDragHandler // required interface when using the OnDrag method.
        /// {
        ///     //Do this while the user is dragging this UI Element.
        ///     public void OnDrag(PointerEventData data)
        ///     {
        ///         Debug.Log("Currently dragging " + this.name);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!m_MobileScroll)
            {
                if (!m_Dragging)
                    return;

                if (eventData.button != PointerEventData.InputButton.Left)
                    return;

                if (!IsActive())
                    return;

                Vector2 localCursor;
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
                    return;

                UpdateBounds();

                var pointerDelta = localCursor - m_PointerStartLocalCursor;
                pointerDelta *= m_ScrollMultiplier;
                Vector2 position = m_ContentStartPosition + pointerDelta;

                // Offset to get content into place in the view.
                Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
                position += offset;
                if (m_MovementType == MovementType.Elastic)
                {
                    if (offset.x != 0)
                        position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x);
                    if (offset.y != 0)
                        position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y);
                }

                SetContentAnchoredPosition(position);
            }
        }

        /// <summary>
        /// Sets the anchored position of the content.
        /// </summary>
        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            if (!m_Horizontal)
                position.x = m_Content.anchoredPosition.x;
            if (!m_Vertical)
                position.y = m_Content.anchoredPosition.y;

            if (position != m_Content.anchoredPosition)
            {
                m_Content.anchoredPosition = position;
                UpdateBounds();
            }
        }
        void ResizeContent()
        {
            float ContentWidthSize = (Panels.Length * ImageWidth) - ImageWidth * 2;
            m_Content.GetComponent<RectTransform>().sizeDelta = new Vector2(ContentWidthSize, CanvasRectTransform.sizeDelta.y);
        }

        void MakeTheContentNotScrollableForTheFirstAndLastPanels()
        {
            RectOffset newPadding = new RectOffset(ContentHLG.padding.left, ContentHLG.padding.right, ContentHLG.padding.top, ContentHLG.padding.bottom);
            newPadding.left = (int)-ImageWidth;
            newPadding.right = (int)-ImageWidth;
            ContentHLG.padding = newPadding;
        }
        void ParentThePanelsToTheContentAndResizeThem()
        {
            for (int i = 0; i < Panels.Length; i++)
            {
                var panel = Panels[i];
                //if (panel.transform.parent != m_Content) 
                panel.transform.SetParent(m_Content);
                panel.GetComponent<RectTransform>().sizeDelta = new Vector2(ImageWidth, CanvasRectTransform.sizeDelta.y);
                if (i != 0 && i != Panels.Length - 1)
                {
                    panel.transform.Find("Scroll View").GetComponent<RectTransform>().sizeDelta = new Vector2(CanvasRectTransform.sizeDelta.x, CanvasRectTransform.sizeDelta.y);
                }
            }
        }
        void SetSnappingPoints()
        {
            int FirstAddon = ((int)ImageWidth - (int)CanvasRectTransform.sizeDelta.x) / 2; // = 390
            int SnappingPointsINT = (int)Panels.Length - 2;
            SnappingPoints = new float[SnappingPointsINT];
            for (int i = 0; i < SnappingPointsINT; i++)
            {
                SnappingPoints[i] = (int)(-FirstAddon - i * ImageWidth);
                //Debug.Log(SnappingPoints[i]);
            }
            if (SnappingPoints.Length <= 1)
            {
                // Array is empty
                Debug.Log("The Panels Array needs to have more than 1 panel for Snapping to work");
            }
        }
        void MakeTheScrollViewStartFromTheMiddle()
        {
            int middleIndex;
            if (Panels.Length % 2 == 1) // Check if array length is odd
            {
                middleIndex = Panels.Length / 2; // Calculate the middle index
            }
            else // Array length is even
            {
                middleIndex = Panels.Length / 2 - 1; // Get the first element closest to the middle (in 6 elements = 3 instead of 4)
            }
            int elementsBeforeMiddle = middleIndex;
            float FirstAddon = (ImageWidth - CanvasRectTransform.sizeDelta.x) / 2;
            float CalculateMiddlePoint = -((elementsBeforeMiddle - 1) * ImageWidth) - FirstAddon;
            RectOffset newPadding1 = new RectOffset(ViewPortHLG.padding.left, ViewPortHLG.padding.right, ViewPortHLG.padding.top, ViewPortHLG.padding.bottom);
            newPadding1.left = (int)CalculateMiddlePoint;
            ViewPortHLG.padding = newPadding1;
        }
        void Begun(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
        {
            if (!IsActive()) return;
            UpdateBounds();
            m_PointerStartLocalCursor = Vector2.zero;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ViewRect, touch.screenPosition, Camera.main, out m_PointerStartLocalCursor);
            //m_ContentStartPosition = content.anchoredPosition;
            m_Dragging = true;
        }
        void Moved(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
        {
            if (!m_Dragging) return;
            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ViewRect, touch.screenPosition, Camera.main, out localCursor)) return;
            UpdateBounds();
            Vector2 pointerDelta = localCursor - m_PointerStartLocalCursor;
            pointerDelta *= m_ScrollMultiplier;

            Vector2 position = m_ContentStartPosition + pointerDelta;

            //Offset to get content into place in the view.
            Vector2 offseta = CalculateOffset(position - content.anchoredPosition);
            position += offseta;
            if (movementType == MovementType.Elastic)
            {
                if (offseta.x != 0)
                    position.x = position.x - RubberDelta(offseta.x, m_ViewBounds.size.x);
                if (offseta.y != 0)
                    position.y = position.y - RubberDelta(offseta.y, m_ViewBounds.size.y);
            }

            SetContentAnchoredPosition(position);

            float absoluteXPosition = Mathf.Round(touch.delta.x);
            if (absoluteXPosition != 0)
            {
                SnapToNextPanel = (absoluteXPosition >= PanelThreshHold || absoluteXPosition <= -PanelThreshHold);
            }
        }
        void Ended()
        {
            m_Dragging = false;
        }

        void Start()//protected override 
        {
            //base.Start();
#if UNITY_EDITOR
            previousSizeDeltaY = CanvasRectTransform.sizeDelta.y;
            previousImageWidth = ImageWidth;
#endif
            ResizeContent();
            MakeTheContentNotScrollableForTheFirstAndLastPanels();
            ParentThePanelsToTheContentAndResizeThem();
            MakeTheScrollViewStartFromTheMiddle();
            SetSnappingPoints();
    }
        void MoveHorizontally(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
        {
            if (!thresholdCrossed)
            {
                m_ContentStartPosition = content.anchoredPosition;
                m_PointerStartLocalCursor = Vector2.zero;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ViewRect, touch.screenPosition, Camera.main, out m_PointerStartLocalCursor);
                thresholdCrossed = true;
            }
            Moved(touch);
        }
        void MoveVertically(UnityEngine.InputSystem.EnhancedTouch.Touch touch, Image Panel)
        {
            if (!thresholdCrossed)
            {
                PanelScrollRect.m_ContentStartPosition = PanelScrollRect.content.anchoredPosition;
                PanelScrollRect.m_PointerStartLocalCursor = Vector2.zero;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(PanelScrollRect.m_ViewRect, touch.screenPosition, Camera.main, out PanelScrollRect.m_PointerStartLocalCursor);
                thresholdCrossed = true;
            }
            PanelScrollRect.Moved(touch);
        }
        private bool ExecuteElseOnce = true;
        private Vector2 totalTouchMovement = Vector2.zero;
        private bool thresholdCrossed = false;
        private bool xReached250 = false;
        private bool yReached250 = false;
        private Image ThisPanel = null;
        private ScrollRect1 PanelScrollRect;
        private Transform childTransform;
        Image previousPanel = null;
        void Update() 
        {
            ///down
#if UNITY_EDITOR
            if (EditorApplication.isPlaying || !Application.isEditor)
            {
                ExecuteElseOnce = true;
#else
            if (!Application.isEditor)
            {
                ExecuteElseOnce = true;
#endif

                //#if BuildingApplication
                //                if (EditorApplication.isPlaying || !Application.isEditor)
                //                {
                //                    ExecuteElseOnce = true;
                //#endif
                ///up//
#if UNITY_EDITOR
                if (CanvasRectTransform.sizeDelta.y != previousSizeDeltaY || ImageWidth != previousImageWidth)
                {
                    ResizeContent();
                    MakeTheContentNotScrollableForTheFirstAndLastPanels();
                    ParentThePanelsToTheContentAndResizeThem();
                    MakeTheScrollViewStartFromTheMiddle();
                    SetSnappingPoints();
                    previousSizeDeltaY = CanvasRectTransform.sizeDelta.y;
                    previousImageWidth = ImageWidth;
                }
#endif
                if (m_MobileScroll)
                {
                    bool isApproximate = false;
                    int matchedIndex = -1;
                    for (int i = 0; i < SnappingPoints.Length; i++)
                    {
                        if (Mathf.Approximately(content.anchoredPosition.x, SnappingPoints[i]))//starts from element 0 (4 is 3)
                        {
                            matchedIndex = i;
                            isApproximate = true;
                            isSnapping = false;
                            //Debug.Log($"Index: {matchedIndex}, Value: {SnappingPoints[i]}");
                            break;
                        }
                    }
                    if (ThisPanel != Panels[matchedIndex + 1])
                    {
                        int thisPanelIndex = System.Array.IndexOf(Panels, ThisPanel);
                        if (thisPanelIndex > 0 && thisPanelIndex < Panels.Length - 1)
                        {
                            previousPanel = ThisPanel;
                        }
                        //if (previousPanel != null) Debug.Log(previousPanel.gameObject.name);
                        ThisPanel = Panels[matchedIndex + 1];
                        if (m_GoBackToTop)
                        {
                            foreach (var panel in Panels)
                            {
                                int currentIndex = Array.IndexOf(Panels, panel);
                                if (panel != previousPanel && currentIndex != 0 && currentIndex != Panels.Length - 1)
                                {
                                    var scrollRect = panel.transform.Find("Scroll View")?.GetComponent<ScrollRect1>();
                                    if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
                                }
                            }
                        }
                        childTransform = ThisPanel.transform.Find("Scroll View");
                        if(childTransform != null) //&& childTransform.GetComponent<ScrollRect1>() != null && PanelScrollRect != childTransform.GetComponent<ScrollRect1>()
                            PanelScrollRect = childTransform.GetComponent<ScrollRect1>();
                    }
                    var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
                    if (activeTouches.Count > 0)
                    {
                        UnityEngine.InputSystem.EnhancedTouch.Touch touch = activeTouches[0];
                        switch (touch.phase)
                        {
                            case UnityEngine.InputSystem.TouchPhase.Began:
                                totalTouchMovement = Vector2.zero;
                                thresholdCrossed = false;
                                xReached250 = false;
                                yReached250 = false;
                                PanelScrollRect.Begun(touch);
                                Begun(touch);
                                break;
                            case UnityEngine.InputSystem.TouchPhase.Moved:
                                if (!xReached250 && !yReached250)
                                {
                                    if (Mathf.Abs(touch.delta.y) >= 5)
                                    {
                                        totalTouchMovement += new Vector2(Mathf.Abs(touch.delta.x / 12), Mathf.Abs(touch.delta.y));
                                    }
                                    else
                                    {
                                        totalTouchMovement += new Vector2(Mathf.Abs(touch.delta.x), Mathf.Abs(touch.delta.y));
                                    }

                                    if (totalTouchMovement.x > 10 && !yReached250)
                                    {
                                        xReached250 = true;
                                        yReached250 = false;
                                    }
                                    else if (totalTouchMovement.y > 10 && !xReached250)
                                    {
                                        yReached250 = true;
                                        xReached250 = false;
                                    }
                                }
                                if (!isApproximate)
                                {
                                    MoveHorizontally(touch);
                                }
                                else
                                {
                                    if (xReached250)// && PanelScrollRect.velocity.y == 0
                                    {
                                        MoveHorizontally(touch);
                                    }
                                    if (yReached250)
                                    {
                                        //Panels.Length we have 9
                                        //SnappingPoints we have 7          //skips element 0 and teh last element too. //matchedIndex+1
                                        MoveVertically(touch, ThisPanel);
                                    }
                                }
                                break;
                            case UnityEngine.InputSystem.TouchPhase.Ended:
                                Ended();
                                //childTransform = ThisPanel.transform.Find("Scroll View");
                                PanelScrollRect.Ended();
                                break;
                        }
                    }
                    
                    //down
                }
            //up
            }
            //down

            //#if BuildingApplication
            //            }
            //#endif          
#if UNITY_EDITOR
            else if (ExecuteElseOnce == true && (!EditorApplication.isPlaying || Application.isEditor))
            {
                for (int i = 0; i < Panels.Length; i++)
                {
                    var panel = Panels[i];
                    if (panel.transform.parent != PanelHolder)
                    {
                        panel.transform.SetParent(PanelHolder);
                        panel.GetComponent<RectTransform>().sizeDelta = new Vector2(ImageWidth, CanvasRectTransform.sizeDelta.y);
                    }

                }
                ExecuteElseOnce = false;
            }
#else
else if (ExecuteElseOnce == true && Application.isEditor)
            {
                for (int i = 0; i < Panels.Length; i++)
                {
                    var panel = Panels[i];
                    if (panel.transform.parent != PanelHolder)
                    {
                        panel.transform.SetParent(PanelHolder);
                        panel.GetComponent<RectTransform>().sizeDelta = new Vector2(ImageWidth, CanvasRectTransform.sizeDelta.y);
                    }

                }
                ExecuteElseOnce = false;
            }
#endif
            //#if BuildingApplication
            //            else if (ExecuteElseOnce == true && (!EditorApplication.isPlaying || Application.isEditor))
            //            {
            //                for (int i = 0; i < Panels.Length; i++)
            //                {
            //                    var panel = Panels[i];
            //                    if (panel.transform.parent != PanelHolder)
            //                    {
            //                        panel.transform.SetParent(PanelHolder);
            //                        panel.GetComponent<RectTransform>().sizeDelta = new Vector2(ImageWidth, CanvasRectTransform.sizeDelta.y);
            //                    }

            //                }
            //                ExecuteElseOnce = false;
            //            }
            //#endif
            //up

        }
        protected virtual void LateUpdate()
        {
            if (!m_Content)
                return;
            EnsureLayoutHasRebuilt();
            UpdateBounds();
            float deltaTime = Time.unscaledDeltaTime;
            Vector2 offset = CalculateOffset(Vector2.zero);
            if (deltaTime > 0.0f)
            {
                if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero))
                {
                    Vector2 position = m_Content.anchoredPosition;
                    for (int axis = 0; axis < 2; axis++)
                    {
                        // Apply spring physics if movement is elastic and content has an offset from the view.
                        //if (m_MovementType == MovementType.Elastic && offset[axis] != 0)
                        //{
                        //    float speed = m_Velocity[axis];
                        //    float smoothTime = m_Elasticity;
                        //    if (m_Scrolling)
                        //        smoothTime *= 3.0f;
                        //    position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis], m_Content.anchoredPosition[axis] + offset[axis], ref speed, smoothTime, Mathf.Infinity, deltaTime);
                        //    if (Mathf.Abs(speed) < 1) speed = 0;
                        //    m_Velocity[axis] = speed;
                        //}
                        // Else move content according to velocity with deceleration applied.
                        if (m_Inertia)
                        {
                            if (m_Snap)
                            {
                                float targetPosition = position.x;
                                //if (StartPositionContent.x == targetPosition) Debug.Log("hii");
                                if (isSnapping && SnapToNextPanel)
                                {
                                    float sign = Mathf.Sign(m_Velocity.x);
                                    float currentPosition = position.x;
                                    if (currentPosition > SnappingPoints[0])//first element
                                    {
                                        targetPosition = SnappingPoints[0];
                                    }
                                    else if (currentPosition < SnappingPoints[SnappingPoints.Length - 1])//last element
                                    {
                                        targetPosition = SnappingPoints[SnappingPoints.Length - 1];
                                    }
                                    else if (currentPosition <= SnappingPoints[0] && currentPosition >= SnappingPoints[SnappingPoints.Length - 1])
                                    {
                                        for (int i = 0; i < SnappingPoints.Length - 1; i++)
                                        {
                                            if (currentPosition <= SnappingPoints[i] && currentPosition > SnappingPoints[i + 1])
                                            {
                                                if (sign > 0)
                                                {
                                                    targetPosition = SnappingPoints[i];
                                                }
                                                else if (sign < 0)
                                                {
                                                    targetPosition = SnappingPoints[i + 1];
                                                }
                                                break; // Exit the loop once we've found the correct range
                                            }
                                        }
                                    }
                                    position.x = PeakMoveToward(currentPosition, targetPosition, SlideSpeed, deltaTime);
                                }
                                else if (isSnapping && !SnapToNextPanel)
                                {
                                    float currentPosition = position.x;
                                    if (position.x >= (SnappingPoints[0] + SnappingPoints[1]) / 2)
                                    {
                                        targetPosition = SnappingPoints[0];
                                        //L
                                    }
                                    else if (position.x < (SnappingPoints[SnappingPoints.Length - 2] + SnappingPoints[SnappingPoints.Length - 1]) / 2)
                                    {
                                        //R
                                        targetPosition = SnappingPoints[SnappingPoints.Length - 1];
                                    }
                                    else
                                    {
                                        for (int i = 1; i < SnappingPoints.Length - 1; i++)
                                        {
                                            if (position.x < (SnappingPoints[i - 1] + SnappingPoints[i]) / 2 &&
                                                position.x >= (SnappingPoints[i] + SnappingPoints[i + 1]) / 2)
                                            {
                                                targetPosition = SnappingPoints[i];
                                                break;
                                            }
                                        }
                                    }
                                    position.x = PeakMoveToward(currentPosition, targetPosition, SlideSpeed, deltaTime);
                                }
                                float epsilon = 0.001f; // Adjust this epsilon as per your requirement

                                if (Mathf.Abs(position.x - targetPosition) < epsilon)
                                {
                                    isSnapping = false;
                                }
                            }
                            else
                            {
                                if (m_UseDecelerationRate) { m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime); }
                                if (Mathf.Abs(m_Velocity[axis]) < 1) m_Velocity[axis] = 0;
                                position[axis] += m_Velocity[axis] * deltaTime;
                            }
                        }
                        // If we have neither elaticity or friction, there shouldn't be any velocity.
                        else
                        {
                            m_Velocity[axis] = 0;
                        }
                    }
                    //if (m_UseDecelerationRate) m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime);
                    //if (Mathf.Abs(m_Velocity[axis]) < 1) m_Velocity[axis] = 0;
                    //position[axis] += m_Velocity[axis] * deltaTime;
                    if (m_MovementType == MovementType.Clamped)
                    {
                        offset = CalculateOffset(position - m_Content.anchoredPosition);
                        position += offset;
                    }

                    SetContentAnchoredPosition(position);
                }

                //if (m_Dragging && m_Inertia)
                //{
                //    Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
                //    m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
                //}
                if (m_Dragging && m_Inertia)
                {
                    if (!m_Snap && !m_UseConstantInertiaSpeed)
                    {
                        Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
                        m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
                    }
                    else if (!m_Snap && m_UseConstantInertiaSpeed)
                    {
                        Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
                        m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
                        m_Velocity = m_Velocity.normalized * m_ConstantInertiaScrollSpeed;
                    }
                    else if (m_Snap)
                    {
                        Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
                        m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
                        m_Velocity = m_Velocity.normalized; //positive or negative one (or zero)
                        if (!Mathf.Approximately(m_Velocity.x, 0))
                        {
                            storedVelocityX = m_Velocity.x;
                        }
                        isSnapping = true;
                    }
                }
            }

            if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition)
            {
                UpdateScrollbars(offset);
                UISystemProfilerApi.AddMarker("ScrollRect.value", this);
                m_OnValueChanged.Invoke(normalizedPosition);
                UpdatePrevData();
            }
            UpdateScrollbarVisibility();
            m_Scrolling = false;
        }

        float PeakMoveToward(float before, float after, float speed, float dt)
        {
            // If the difference is 10 or less, snap to the target value instantly
            if (Mathf.Abs(before - after) <= 4f)
            {
                return after;
            }
            return after + (before - after) * Mathf.Exp(-speed * dt);
        }
        /// <summary>
        /// Helper function to update the previous data fields on a ScrollRect. Call this before you change data in the ScrollRect.
        /// </summary>
        protected void UpdatePrevData()
        {
            if (m_Content == null)
                m_PrevPosition = Vector2.zero;
            else
                m_PrevPosition = m_Content.anchoredPosition;
            m_PrevViewBounds = m_ViewBounds;
            m_PrevContentBounds = m_ContentBounds;
        }

        private void UpdateScrollbars(Vector2 offset)
        {
            if (m_HorizontalScrollbar)
            {
                if (m_ContentBounds.size.x > 0)
                    m_HorizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / m_ContentBounds.size.x);
                else
                    m_HorizontalScrollbar.size = 1;

                m_HorizontalScrollbar.value = horizontalNormalizedPosition;
            }

            if (m_VerticalScrollbar)
            {
                if (m_ContentBounds.size.y > 0)
                    m_VerticalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / m_ContentBounds.size.y);
                else
                    m_VerticalScrollbar.size = 1;

                m_VerticalScrollbar.value = verticalNormalizedPosition;
            }
        }

        /// <summary>
        /// The scroll position as a Vector2 between (0,0) and (1,1) with (0,0) being the lower left corner.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Vector2 myPosition = new Vector2(0.5f, 0.5f);
        ///
        ///     public void Start()
        ///     {
        ///         //Change the current scroll position.
        ///         myScrollRect.normalizedPosition = myPosition;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public Vector2 normalizedPosition
        {
            get
            {
                return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition);
            }
            set
            {
                SetNormalizedPosition(value.x, 0);
                SetNormalizedPosition(value.y, 1);
            }
        }

        /// <summary>
        /// The horizontal scroll position as a value between 0 and 1, with 0 being at the left.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Scrollbar newScrollBar;
        ///
        ///     public void Start()
        ///     {
        ///         //Change the current horizontal scroll position.
        ///         myScrollRect.horizontalNormalizedPosition = 0.5f;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public float horizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if ((m_ContentBounds.size.x <= m_ViewBounds.size.x) || Mathf.Approximately(m_ContentBounds.size.x, m_ViewBounds.size.x))
                    return (m_ViewBounds.min.x > m_ContentBounds.min.x) ? 1 : 0;
                return (m_ViewBounds.min.x - m_ContentBounds.min.x) / (m_ContentBounds.size.x - m_ViewBounds.size.x);
            }
            set
            {
                SetNormalizedPosition(value, 0);
            }
        }

        /// <summary>
        /// The vertical scroll position as a value between 0 and 1, with 0 being at the bottom.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class ExampleClass : MonoBehaviour
        /// {
        ///     public ScrollRect myScrollRect;
        ///     public Scrollbar newScrollBar;
        ///
        ///     public void Start()
        ///     {
        ///         //Change the current vertical scroll position.
        ///         myScrollRect.verticalNormalizedPosition = 0.5f;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>

        public float verticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if ((m_ContentBounds.size.y <= m_ViewBounds.size.y) || Mathf.Approximately(m_ContentBounds.size.y, m_ViewBounds.size.y))
                    return (m_ViewBounds.min.y > m_ContentBounds.min.y) ? 1 : 0;

                return (m_ViewBounds.min.y - m_ContentBounds.min.y) / (m_ContentBounds.size.y - m_ViewBounds.size.y);
            }
            set
            {
                SetNormalizedPosition(value, 1);
            }
        }

        private void SetHorizontalNormalizedPosition(float value) { SetNormalizedPosition(value, 0); }
        private void SetVerticalNormalizedPosition(float value) { SetNormalizedPosition(value, 1); }

        /// <summary>
        /// >Set the horizontal or vertical scroll position as a value between 0 and 1, with 0 being at the left or at the bottom.
        /// </summary>
        /// <param name="value">The position to set, between 0 and 1.</param>
        /// <param name="axis">The axis to set: 0 for horizontal, 1 for vertical.</param>
        protected virtual void SetNormalizedPosition(float value, int axis)
        {
            EnsureLayoutHasRebuilt();
            UpdateBounds();
            // How much the content is larger than the view.
            float hiddenLength = m_ContentBounds.size[axis] - m_ViewBounds.size[axis];
            // Where the position of the lower left corner of the content bounds should be, in the space of the view.
            float contentBoundsMinPosition = m_ViewBounds.min[axis] - value * hiddenLength;
            // The new content localPosition, in the space of the view.
            float newAnchoredPosition = m_Content.anchoredPosition[axis] + contentBoundsMinPosition - m_ContentBounds.min[axis];

            Vector3 anchoredPosition = m_Content.anchoredPosition;
            if (Mathf.Abs(anchoredPosition[axis] - newAnchoredPosition) > 0.01f)
            {
                anchoredPosition[axis] = newAnchoredPosition;
                m_Content.anchoredPosition = anchoredPosition;
                m_Velocity[axis] = 0;
                UpdateBounds();
            }
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private bool hScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.x > m_ViewBounds.size.x + 0.01f;
                return true;
            }
        }
        private bool vScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.y > m_ViewBounds.size.y + 0.01f;
                return true;
            }
        }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void CalculateLayoutInputHorizontal() {}

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void CalculateLayoutInputVertical() {}

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float minWidth { get { return -1; } }
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float preferredWidth { get { return -1; } }
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float flexibleWidth { get { return -1; } }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float minHeight { get { return -1; } }
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float preferredHeight { get { return -1; } }
        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual float flexibleHeight { get { return -1; } }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual int layoutPriority { get { return -1; } }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();
            UpdateCachedData();

            if (m_HSliderExpand || m_VSliderExpand)
            {
                m_Tracker.Add(this, viewRect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition);

                // Make view full size to see if content fits.
                viewRect.anchorMin = Vector2.zero;
                viewRect.anchorMax = Vector2.one;
                viewRect.sizeDelta = Vector2.zero;
                viewRect.anchoredPosition = Vector2.zero;

                // Recalculate content layout with this size to see if it fits when there are no scrollbars.
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VSliderExpand && vScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);

                // Recalculate content layout with this size to see if it fits vertically
                // when there is a vertical scrollbar (which may reflowed the content to make it taller).
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
            if (m_HSliderExpand && hScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(viewRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If the vertical slider didn't kick in the first time, and the horizontal one did,
            // we need to check again if the vertical slider now needs to kick in.
            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VSliderExpand && vScrollingNeeded && viewRect.sizeDelta.x == 0 && viewRect.sizeDelta.y < 0)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);
            }
        }

        /// <summary>
        /// Called by the layout system.
        /// </summary>
        public virtual void SetLayoutVertical()
        {
            UpdateScrollbarLayout();
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();
        }

        void UpdateScrollbarVisibility()
        {
            UpdateOneScrollbarVisibility(vScrollingNeeded, m_Vertical, m_VerticalScrollbarVisibility, m_VerticalScrollbar);
            UpdateOneScrollbarVisibility(hScrollingNeeded, m_Horizontal, m_HorizontalScrollbarVisibility, m_HorizontalScrollbar);
        }

        private static void UpdateOneScrollbarVisibility(bool xScrollingNeeded, bool xAxisEnabled, ScrollbarVisibility scrollbarVisibility, Scrollbar scrollbar)
        {
            if (scrollbar)
            {
                if (scrollbarVisibility == ScrollbarVisibility.Permanent)
                {
                    if (scrollbar.gameObject.activeSelf != xAxisEnabled)
                        scrollbar.gameObject.SetActive(xAxisEnabled);
                }
                else
                {
                    if (scrollbar.gameObject.activeSelf != xScrollingNeeded)
                        scrollbar.gameObject.SetActive(xScrollingNeeded);
                }
            }
        }

        void UpdateScrollbarLayout()
        {
            if (m_VSliderExpand && m_HorizontalScrollbar)
            {
                m_Tracker.Add(this, m_HorizontalScrollbarRect,
                    DrivenTransformProperties.AnchorMinX |
                    DrivenTransformProperties.AnchorMaxX |
                    DrivenTransformProperties.SizeDeltaX |
                    DrivenTransformProperties.AnchoredPositionX);
                m_HorizontalScrollbarRect.anchorMin = new Vector2(0, m_HorizontalScrollbarRect.anchorMin.y);
                m_HorizontalScrollbarRect.anchorMax = new Vector2(1, m_HorizontalScrollbarRect.anchorMax.y);
                m_HorizontalScrollbarRect.anchoredPosition = new Vector2(0, m_HorizontalScrollbarRect.anchoredPosition.y);
                if (vScrollingNeeded)
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), m_HorizontalScrollbarRect.sizeDelta.y);
                else
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(0, m_HorizontalScrollbarRect.sizeDelta.y);
            }

            if (m_HSliderExpand && m_VerticalScrollbar)
            {
                m_Tracker.Add(this, m_VerticalScrollbarRect,
                    DrivenTransformProperties.AnchorMinY |
                    DrivenTransformProperties.AnchorMaxY |
                    DrivenTransformProperties.SizeDeltaY |
                    DrivenTransformProperties.AnchoredPositionY);
                m_VerticalScrollbarRect.anchorMin = new Vector2(m_VerticalScrollbarRect.anchorMin.x, 0);
                m_VerticalScrollbarRect.anchorMax = new Vector2(m_VerticalScrollbarRect.anchorMax.x, 1);
                m_VerticalScrollbarRect.anchoredPosition = new Vector2(m_VerticalScrollbarRect.anchoredPosition.x, 0);
                if (hScrollingNeeded)
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                else
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0);
            }
        }

        /// <summary>
        /// Calculate the bounds the ScrollRect should be using.
        /// </summary>
        protected void UpdateBounds()
        {
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();

            if (m_Content == null)
                return;

            Vector3 contentSize = m_ContentBounds.size;
            Vector3 contentPos = m_ContentBounds.center;
            var contentPivot = m_Content.pivot;
            AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
            m_ContentBounds.size = contentSize;
            m_ContentBounds.center = contentPos;

            if (movementType == MovementType.Clamped)
            {
                // Adjust content so that content bounds bottom (right side) is never higher (to the left) than the view bounds bottom (right side).
                // top (left side) is never lower (to the right) than the view bounds top (left side).
                // All this can happen if content has shrunk.
                // This works because content size is at least as big as view size (because of the call to InternalUpdateBounds above).
                Vector2 delta = Vector2.zero;
                if (m_ViewBounds.max.x > m_ContentBounds.max.x)
                {
                    delta.x = Math.Min(m_ViewBounds.min.x - m_ContentBounds.min.x, m_ViewBounds.max.x - m_ContentBounds.max.x);
                }
                else if (m_ViewBounds.min.x < m_ContentBounds.min.x)
                {
                    delta.x = Math.Max(m_ViewBounds.min.x - m_ContentBounds.min.x, m_ViewBounds.max.x - m_ContentBounds.max.x);
                }

                if (m_ViewBounds.min.y < m_ContentBounds.min.y)
                {
                    delta.y = Math.Max(m_ViewBounds.min.y - m_ContentBounds.min.y, m_ViewBounds.max.y - m_ContentBounds.max.y);
                }
                else if (m_ViewBounds.max.y > m_ContentBounds.max.y)
                {
                    delta.y = Math.Min(m_ViewBounds.min.y - m_ContentBounds.min.y, m_ViewBounds.max.y - m_ContentBounds.max.y);
                }
                if (delta.sqrMagnitude > float.Epsilon)
                {
                    contentPos = m_Content.anchoredPosition + delta;
                    if (!m_Horizontal)
                        contentPos.x = m_Content.anchoredPosition.x;
                    if (!m_Vertical)
                        contentPos.y = m_Content.anchoredPosition.y;
                    AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
                }
            }
        }

        internal static void AdjustBounds(ref Bounds viewBounds, ref Vector2 contentPivot, ref Vector3 contentSize, ref Vector3 contentPos)
        {
            // Make sure content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when content is *larger* than view.
            // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when ContentSizeFitter is used on the content.
            Vector3 excess = viewBounds.size - contentSize;
            if (excess.x > 0)
            {
                contentPos.x -= excess.x * (contentPivot.x - 0.5f);
                contentSize.x = viewBounds.size.x;
            }
            if (excess.y > 0)
            {
                contentPos.y -= excess.y * (contentPivot.y - 0.5f);
                contentSize.y = viewBounds.size.y;
            }
        }

        private readonly Vector3[] m_Corners = new Vector3[4];
        private Bounds GetBounds()
        {
            if (m_Content == null)
                return new Bounds();
            m_Content.GetWorldCorners(m_Corners);
            var viewWorldToLocalMatrix = viewRect.worldToLocalMatrix;
            return InternalGetBounds(m_Corners, ref viewWorldToLocalMatrix);
        }

        internal static Bounds InternalGetBounds(Vector3[] corners, ref Matrix4x4 viewWorldToLocalMatrix)
        {
            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int j = 0; j < 4; j++)
            {
                Vector3 v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        private Vector2 CalculateOffset(Vector2 delta)
        {
            return InternalCalculateOffset(ref m_ViewBounds, ref m_ContentBounds, m_Horizontal, m_Vertical, m_MovementType, ref delta);
        }

        internal static Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Bounds contentBounds, bool horizontal, bool vertical, MovementType movementType, ref Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
            if (movementType == MovementType.Unrestricted)
                return offset;

            Vector2 min = contentBounds.min;
            Vector2 max = contentBounds.max;

            // min/max offset extracted to check if approximately 0 and avoid recalculating layout every frame (case 1010178)

            if (horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;

                float maxOffset = viewBounds.max.x - max.x;
                float minOffset = viewBounds.min.x - min.x;

                if (minOffset < -0.001f)
                    offset.x = minOffset;
                else if (maxOffset > 0.001f)
                    offset.x = maxOffset;
            }

            if (vertical)
            {
                min.y += delta.y;
                max.y += delta.y;

                float maxOffset = viewBounds.max.y - max.y;
                float minOffset = viewBounds.min.y - min.y;

                if (maxOffset > 0.001f)
                    offset.y = maxOffset;
                else if (minOffset < -0.001f)
                    offset.y = minOffset;
            }

            return offset;
        }

        /// <summary>
        /// Override to alter or add to the code that keeps the appearance of the scroll rect synced with its data.
        /// </summary>
        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        /// <summary>
        /// Override to alter or add to the code that caches data to avoid repeated heavy operations.
        /// </summary>
        protected void SetDirtyCaching()
        {
            if (!IsActive())
                return;

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);

            m_ViewRect = null;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirtyCaching();
        }

#endif
    }
}
