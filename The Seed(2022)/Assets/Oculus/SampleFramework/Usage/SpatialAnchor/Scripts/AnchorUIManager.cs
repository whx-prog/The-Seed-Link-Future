// Copyright (c) Facebook, Inc. and its affiliates. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages UI of anchor sample.
/// </summary>
public class AnchorUIManager : MonoBehaviour
{
    /// <summary>
    /// Anchor UI manager singleton instance
    /// </summary>
    public static AnchorUIManager Instance;

    /// <summary>
    /// Anchor Mode switches between create and select
    /// </summary>
    public enum AnchorMode { Create, Select };

    [SerializeField]
    private GameObject createModeButton_;
    [SerializeField]
    private GameObject selectModeButton_;

    [SerializeField]
    private Transform trackedDevice_;
    private Transform raycastOrigin_;
    private bool drawRaycast_ = false;
    [SerializeField]
    private LineRenderer lineRenderer_;

    private Anchor hoveredAnchor_;
    private Anchor selectedAnchor_;

    private AnchorMode mode_ = AnchorMode.Create;

    [SerializeField]
    private List<Button> buttonList_;

    private int menuIndex_ = 0;
    private Button selectedButton_;

    [SerializeField]
    private GameObject placementPreview_;

    [SerializeField]
    private Transform anchorPlacementTransform_;

    private delegate void PrimaryPressDelegate();
    private PrimaryPressDelegate primaryPressDelegate_;

    private bool isFocused_ = true;

    #region Monobehaviour Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        raycastOrigin_ = trackedDevice_;

        selectedButton_ = buttonList_[0];
        buttonList_[0].OnSelect(null);

        lineRenderer_.startWidth = 0.005f;
        lineRenderer_.endWidth = 0.005f;

        ToggleCreateMode();
    }

    private void Update()
    {
        if (drawRaycast_)
        {
            ControllerRaycast();
        }

        if (selectedAnchor_ == null)
        {
            // Refocus menu
            selectedButton_.OnSelect(null);
            isFocused_ = true;
        }

        HandleMenuNavigation();

        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            primaryPressDelegate_?.Invoke();
        }
    }

    #endregion // Monobehaviour Methods


    #region Menu UI Callbacks

    /// <summary>
    /// Create mode button pressed UI callback. Referenced by the Create button in the menu.
    /// </summary>
    public void OnCreateModeButtonPressed()
    {
        ToggleCreateMode();
        createModeButton_.SetActive(!createModeButton_.activeSelf);
        selectModeButton_.SetActive(!selectModeButton_.activeSelf);
    }

    /// <summary>
    /// Load anchors button pressed UI callback. Referenced by the Load Anchors button in the menu.
    /// </summary>
    public void OnLoadAnchorsButtonPressed()
    {
        AnchorSession.Instance.QueryAllLocalAnchors();
    }

    #endregion // Menu UI Callbacks


    #region Mode Handling

    private void ToggleCreateMode()
    {
        if (mode_ == AnchorMode.Select)
        {
            mode_ = AnchorMode.Create;
            EndSelectMode();
            StartPlacementMode();
        }
        else
        {
            mode_ = AnchorMode.Select;
            EndPlacementMode();
            StartSelectMode();
        }
    }

    private void StartPlacementMode()
    {
        ShowAnchorPreview();
        primaryPressDelegate_ = PlaceAnchor;
    }

    private void EndPlacementMode()
    {
        HideAnchorPreview();
        primaryPressDelegate_ = null;
    }

    private void StartSelectMode()
    {
        ShowRaycastLine();
        primaryPressDelegate_ = SelectAnchor;
    }

    private void EndSelectMode()
    {
        HideRaycastLine();
        primaryPressDelegate_ = null;
    }

    #endregion // Mode Handling


    #region Private Methods

    private void HandleMenuNavigation()
    {
        if (!isFocused_)
        {
            return;
        }
        if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickUp))
        {
            NavigateToIndexInMenu(false);
        }
        if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickDown))
        {
            NavigateToIndexInMenu(true);
        }
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
        {
            selectedButton_.OnSubmit(null);
        }
    }

    private void NavigateToIndexInMenu(bool moveNext)
    {
        if (moveNext)
        {
            menuIndex_++;
            if (menuIndex_ > buttonList_.Count - 1)
            {
                menuIndex_ = 0;
            }
        }
        else
        {
            menuIndex_--;
            if (menuIndex_ < 0)
            {
                menuIndex_ = buttonList_.Count - 1;
            }
        }
        selectedButton_.OnDeselect(null);
        selectedButton_ = buttonList_[menuIndex_];
        selectedButton_.OnSelect(null);
    }

    private void ShowAnchorPreview()
    {
        placementPreview_.SetActive(true);
    }

    private void HideAnchorPreview()
    {
        placementPreview_.SetActive(false);
    }

    private void PlaceAnchor()
    {
        AnchorSpawner.Instance.PlaceAnchorAtTransform(anchorPlacementTransform_);
    }

    private void ShowRaycastLine()
    {
        drawRaycast_ = true;
        lineRenderer_.gameObject.SetActive(true);
    }

    private void HideRaycastLine()
    {
        drawRaycast_ = false;
        lineRenderer_.gameObject.SetActive(false);
    }

    private void ControllerRaycast()
    {
        Ray ray = new Ray(raycastOrigin_.position, raycastOrigin_.TransformDirection(Vector3.forward));
        lineRenderer_.SetPosition(0, raycastOrigin_.position);
        lineRenderer_.SetPosition(1, raycastOrigin_.position + raycastOrigin_.TransformDirection(Vector3.forward) * 10f);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Anchor anchorObject = hit.collider.GetComponent<Anchor>();
            if (anchorObject != null)
            {
                lineRenderer_.SetPosition(1, hit.point);

                HoverAnchor(anchorObject);
                return;
            }
        }
        UnhoverAnchor();
    }

    private void HoverAnchor(Anchor anchor)
    {
        hoveredAnchor_ = anchor;
        hoveredAnchor_.OnHoverStart();
    }

    private void UnhoverAnchor()
    {
        if (hoveredAnchor_ == null)
        {
            return;
        }
        hoveredAnchor_.OnHoverEnd();
        hoveredAnchor_ = null;
    }

    private void SelectAnchor()
    {
        if (hoveredAnchor_ != null)
        {
            if (selectedAnchor_ != null)
            {
                // Deselect previous Anchor
                selectedAnchor_.OnSelect();
                selectedAnchor_ = null;
            }

            // Select new Anchor
            selectedAnchor_ = hoveredAnchor_;
            selectedAnchor_.OnSelect();

            // Defocus menu
            selectedButton_.OnDeselect(null);
            isFocused_ = false;
        }
        else
        {
            if (selectedAnchor_ != null)
            {
                // Deselect previous Anchor
                selectedAnchor_.OnSelect();
                selectedAnchor_ = null;

                // Refocus menu
                selectedButton_.OnSelect(null);
                isFocused_ = true;
            }
        }
    }

    #endregion // Private Methods

}
