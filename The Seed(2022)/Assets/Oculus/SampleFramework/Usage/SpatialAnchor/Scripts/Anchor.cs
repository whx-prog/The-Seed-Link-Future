// Copyright (c) Facebook, Inc. and its affiliates. All Rights Reserved.

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Specific functionality for spawned anchors
/// </summary>
public class Anchor : MonoBehaviour
{
    /// <summary>
    /// Anchor handle, gives the id of the anchor
    /// </summary>
    public ulong anchorHandle { get { return anchorHandle_; } }

    private ulong anchorHandle_;

    [SerializeField]
    private Canvas canvas_;
    [SerializeField]
    private Transform pivot_;
    [SerializeField]
    private GameObject anchorMenu_;
    private bool isSelected_;
    private bool isHovered_;
    [SerializeField]
    private TextMeshProUGUI anchorName_;
    [SerializeField]
    private GameObject saveIcon_;

    [SerializeField]
    private Image labelImage_;
    [SerializeField]
    private Color labelBaseColor_;
    [SerializeField]
    private Color labelHighlightColor_;
    [SerializeField]
    private Color labelSelectedColor_;

    [SerializeField]
    private AnchorUIManager uiManager_;

    [SerializeField]
    private MeshRenderer[] renderers_;

    private int menuIndex_ = 0;
    [SerializeField]
    private List<Button> buttonList_;
    private Button selectedButton_;


    #region Monobehaviour Methods

    private void Awake()
    {
        anchorMenu_.SetActive(false);
        renderers_ = GetComponentsInChildren<MeshRenderer>();
        canvas_.worldCamera = Camera.main;
        selectedButton_ = buttonList_[0];
        selectedButton_.OnSelect(null);
    }

    private void Update()
    {
        // Billboard the boundary
        BillboardPanel(canvas_.transform);
        // Billboard the menu
        BillboardPanel(pivot_);

        HandleMenuNavigation();
    }

    #endregion // Monobehaviour Methods


    #region UI Event Listeners

    /// <summary>
    /// UI callback for the anchor menu's Save button
    /// </summary>
    public void OnSaveLocalButtonPressed()
    {
        AnchorSession.Instance.SaveAnchor(anchorHandle, AnchorSession.StorageLocation.LOCAL);
    }

    /// <summary>
    /// UI callback for the anchor menu's Hide button
    /// </summary>
    public void OnHideButtonPressed()
    {
        AnchorSession.Instance.DestroyAnchor(anchorHandle);
    }

    /// <summary>
    /// UI callback for the anchor menu's Erase button
    /// </summary>
    public void OnEraseButtonPressed()
    {
        AnchorSession.Instance.EraseAnchor(anchorHandle);
    }

    #endregion // UI Event Listeners


    #region Public Methods

    /// <summary>
    /// Handles interaction when anchor is hovered
    /// </summary>
    public void OnHoverStart()
    {
        if (isHovered_)
        {
            return;
        }
        isHovered_ = true;

        // Yellow highlight
        foreach (MeshRenderer renderer in renderers_)
        {
            renderer.material.SetColor("_EmissionColor", Color.yellow);
        }
        labelImage_.color = labelHighlightColor_;
    }

    /// <summary>
    /// Handles interaction when anchor is no longer hovered
    /// </summary>
    public void OnHoverEnd()
    {
        if (!isHovered_)
        {
            return;
        }
        isHovered_ = false;

        // Go back to normal
        foreach (MeshRenderer renderer in renderers_)
        {
            renderer.material.SetColor("_EmissionColor", Color.clear);
        }

        if (isSelected_)
        {
            labelImage_.color = labelSelectedColor_;
        }
        else
        {
            labelImage_.color = labelBaseColor_;
        }
    }

    /// <summary>
    /// Handles interaction when anchor is selected
    /// </summary>
    public void OnSelect()
    {
        if (isSelected_)
        {
            // Hide Anchor menu on deselect
            anchorMenu_.SetActive(false);
            isSelected_ = false;
            selectedButton_ = null;
            if (isHovered_)
            {
                labelImage_.color = labelHighlightColor_;
            }
            else
            {
                labelImage_.color = labelBaseColor_;
            }
        }
        else
        {
            // Show Anchor Menu on select
            anchorMenu_.SetActive(true);
            isSelected_ = true;
            selectedButton_ = buttonList_[0];
            selectedButton_.OnSelect(null);
            if (isHovered_)
            {
                labelImage_.color = labelHighlightColor_;
            }
            else
            {
                labelImage_.color = labelSelectedColor_;
            }
        }
    }

    /// <summary>
    /// Sets this anchor's handle
    /// </summary>
    /// <param name="handle"></param>
    public void SetAnchorHandle(ulong handle)
    {
        anchorHandle_ = handle;
        anchorName_.text = "ID: " + anchorHandle_;
    }

    /// <summary>
    /// Enables the save icon on the anchor menu
    /// </summary>
    public void ShowSaveIcon()
    {
        saveIcon_.SetActive(true);
    }

    #endregion // Public Methods


    #region Private Methods

    private void BillboardPanel(Transform panel)
    {
        // The z axis of the panel faces away from the side that is rendered, therefore this code is actually looking away from the camera
        panel.LookAt(new Vector3(panel.position.x * 2 - Camera.main.transform.position.x, panel.position.y * 2 - Camera.main.transform.position.y, panel.position.z * 2 - Camera.main.transform.position.z), Vector3.up);
    }

    private void HandleMenuNavigation()
    {
        if (!isSelected_)
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

    #endregion // Private Methods

}
