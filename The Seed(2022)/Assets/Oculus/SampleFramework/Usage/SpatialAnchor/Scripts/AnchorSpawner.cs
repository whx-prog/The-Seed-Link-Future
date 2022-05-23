// Copyright (c) Facebook, Inc. and its affiliates. All Rights Reserved.

using UnityEngine;

/// <summary>
/// Spawns anchors at a specific transform.
/// </summary>
public class AnchorSpawner : MonoBehaviour
{

    /// <summary>
    /// Anchor Spawner singleton instance
    /// </summary>
    public static AnchorSpawner Instance;

    /// <summary>
    /// Anchor Prefab, used for spawning an anchor
    /// </summary>
    public GameObject AnchorPrefab { get { return anchorPrefab_; } }

    [SerializeField]
    private GameObject anchorPrefab_ = null;

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
        gameObject.AddComponent<SpatialAnchorSession>();
    }


    #region Anchor Management

    /// <summary>
    /// Spawns an anchor and parents it under the given transform
    /// </summary>
    /// <param name="transform"></param>
    public void PlaceAnchorAtTransform(Transform transform)
    {
        AnchorSession.Log("Placing anchor at pose: " + transform.ToOVRPose().ToPosef().ToString());

        var anchorHandle = AnchorSession.Instance.CreateSpatialAnchor(transform);
        if (anchorHandle == AnchorSession.kInvalidHandle)
        {
            AnchorSession.Log("Failed to create spatial anchor");
            return;
        }

        GameObject anchorObject = Instantiate(anchorPrefab_);
        Anchor anchor = anchorObject.GetComponent<Anchor>();
        if (anchor == null)
        {
            anchor = anchorObject.AddComponent<Anchor>();
        }
        anchor.SetAnchorHandle(anchorHandle);

        AnchorSession.Instance.handleToAnchor.Add(anchorHandle, anchor);
    }

    #endregion // Anchor Management
}
