// Copyright (c) Facebook, Inc. and its affiliates. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnchorSession : MonoBehaviour
{
    public static AnchorSession Instance;

    public const ulong kInvalidHandle = ulong.MaxValue;

    public enum StorageLocation
    {
        LOCAL = 0
    }

    public Dictionary<ulong, Anchor> handleToAnchor = new Dictionary<ulong, Anchor>();

    protected const int maxEvents = 5;
    protected const float eventPollingRate = 0.1f;

    protected GameObject anchorPrefab_ = null;

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        anchorPrefab_ = AnchorSpawner.Instance.AnchorPrefab;
    }

    public static void Log(string message)
    {
        const string tag = "SpatialAnchorsUnity: ";
        Debug.Log(tag + message);
    }

    protected static bool isFlagSet(uint bitset, uint flag)
    {
        return (bitset & flag) != 0;
    }

    public abstract ulong CreateSpatialAnchor(Transform T_UnityWorld_Anchor);

    public abstract void DestroyAnchor(ulong anchorHandle);

    public abstract void SaveAnchor(ulong anchorHandle, StorageLocation location);

    public abstract void EraseAnchor(ulong anchorHandle);

    public abstract void QueryAllLocalAnchors();
}
