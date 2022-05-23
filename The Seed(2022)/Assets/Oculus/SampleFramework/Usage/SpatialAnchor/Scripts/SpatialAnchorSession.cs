// Copyright (c) Facebook, Inc. and its affiliates. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles calls to and from Spatial Anchor SDK
/// </summary>
public class SpatialAnchorSession : AnchorSession
{
    public Dictionary<ulong, ulong> locateAnchorRequest = new Dictionary<ulong, ulong>();
    private const string numUuids = "numUuids";

    private void Start()
    {
        // Bind Spatial Anchor API callbacks
        OVRManager.SpatialEntityStorageSave += SpatialAnchorSaved;
        OVRManager.SpatialEntityQueryResults += SpatialEntityQueryResults;
        OVRManager.SpatialEntityQueryComplete += SpatialEntityQueryComplete;
        OVRManager.SpatialEntityStorageErase += SpatialEntityStorageErase;
        OVRManager.SpatialEntitySetComponentEnabled += SpatialEntitySetComponentEnabled;
    }

    private void OnDisable()
    {
        // Unbind Spatial Anchor API callbacks
        OVRManager.SpatialEntityStorageSave -= SpatialAnchorSaved;
        OVRManager.SpatialEntityQueryResults -= SpatialEntityQueryResults;
        OVRManager.SpatialEntityQueryComplete -= SpatialEntityQueryComplete;
        OVRManager.SpatialEntityStorageErase -= SpatialEntityStorageErase;
        OVRManager.SpatialEntitySetComponentEnabled -= SpatialEntitySetComponentEnabled;
    }

    // ComponentEnabled callback
    private void SpatialEntitySetComponentEnabled(UInt64 requestId, bool result, OVRPlugin.SpatialEntityComponentType componentType, ulong space)
    {
        if (locateAnchorRequest.ContainsKey(requestId) && !handleToAnchor.ContainsKey(locateAnchorRequest[requestId]))
        {
            CreateAnchorGameobject(locateAnchorRequest[requestId]);
        }
    }

    // Save callback
    private void SpatialAnchorSaved(UInt64 requestId, UInt64 space, bool result, OVRPlugin.SpatialEntityUuid uuid)
    {
        Log("SpatialAnchorSaved requestId: " + requestId + " space: " + space + " result: " + result + " uuid: " + GetUuidString(uuid));

        // Write uuid of saved anchor to file
        if (!PlayerPrefs.HasKey(numUuids))
        {
            PlayerPrefs.SetInt(numUuids, 0);
        }
        int playerNumUuids = PlayerPrefs.GetInt(numUuids);
        PlayerPrefs.SetString("uuid" + playerNumUuids, GetUuidString(uuid));
        PlayerPrefs.SetInt(numUuids, ++playerNumUuids);

        // Toggle local save icon
        if (handleToAnchor.ContainsKey(space))
        {
            handleToAnchor[space].ShowSaveIcon();
        }
    }

    // Erase callback
    private void SpatialEntityStorageErase(UInt64 requestId, bool result, OVRPlugin.SpatialEntityUuid uuid, OVRPlugin.SpatialEntityStorageLocation location)
    {
        Log("SpatialEntityStorageErase requestID: " + requestId + " result: " + result + " uuid: " + GetUuidString(uuid) + " location: " + location);
    }

    // QueryResult callback
    private void SpatialEntityQueryResults(UInt64 requestId, int numResults, OVRPlugin.SpatialEntityQueryResult[] results)
    {
        for (int i = 0; i < numResults; i++)
        {
            var uuid = results[i].uuid;
            var space = results[i].space;
            Log("SpatialEntityQueryResult requestId: " + requestId + " space: " + space + " uuid: " + GetUuidString(uuid));
            tryEnableComponent(space, OVRPlugin.SpatialEntityComponentType.Storable);
            tryEnableComponent(space, OVRPlugin.SpatialEntityComponentType.Locatable);
        }
    }

    // QueryComplete callback
    private void SpatialEntityQueryComplete(UInt64 requestId, bool result, int numFound)
    {
        Log("SpatialEntityQueryComplete requestId: " + requestId + " result: " + result + " numFound: " + numFound);
    }

    // Converts uuid to string in this format "e875df5e-3f47-4a8f-b0a5-9ca79280b27d"
    private string GetUuidString(OVRPlugin.SpatialEntityUuid uuid)
    {
        byte[] uuidData = new byte[16];
        BitConverter.GetBytes(uuid.Value_0).CopyTo(uuidData, 0);
        BitConverter.GetBytes(uuid.Value_1).CopyTo(uuidData, 8);
        return AnchorHelpers.UuidToString(uuidData);
    }

    // Enables specified component if not already enabled
    private void tryEnableComponent(ulong anchorHandle, OVRPlugin.SpatialEntityComponentType type)
    {
        bool enabled;
        bool changePending;
        bool success = OVRPlugin.SpatialEntityGetComponentEnabled(ref anchorHandle, type, out enabled, out changePending);
        if (!success)
        {
            Log("WARNING SpatialEntityGetComponentEnabled did not complete successfully");
        }

        if (enabled)
        {
            Log("Component of type: " + type + " already enabled for anchorHandle: " + anchorHandle);
        }
        else
        {
            ulong requestId = 0;
            OVRPlugin.SpatialEntitySetComponentEnabled(ref anchorHandle, type, true, 0, ref requestId);
            Log("Enabling component for anchorHandle: " + anchorHandle + " type: " + type + " requestId " + requestId);
            switch (type)
            {
                case OVRPlugin.SpatialEntityComponentType.Locatable:
                    locateAnchorRequest.Add(requestId, anchorHandle);
                    break;
                case OVRPlugin.SpatialEntityComponentType.Storable:
                    break;
                default:
                    Log("Tried to enable component that's not supported");
                    break;
            }
        }
    }

    // Create anchor gameobject with specified handle
    private void CreateAnchorGameobject(ulong anchorHandle)
    {
        // Create anchor gameobject
        GameObject anchorObject = Instantiate(anchorPrefab_);
        Anchor anchor = anchorObject.GetComponent<Anchor>();
        anchor.SetAnchorHandle(anchorHandle);

        // Add gameobject to dictionary so it can be tracked, toggle save icon
        handleToAnchor.Add(anchorHandle, anchor);
        handleToAnchor[anchorHandle].ShowSaveIcon();
    }

    void LateUpdate()
    {
        foreach (var handleAnchorPair in handleToAnchor)
        {
            var anchorHandle = handleAnchorPair.Key;
            var anchor = handleAnchorPair.Value;

            if (anchorHandle == kInvalidHandle)
            {
                Log("Error: AnchorHandle invalid in tracking loop!");
                return;
            }

            // Set anchor gameobject transform to pose returned from LocateSpace
            var pose = OVRPlugin.LocateSpace(ref anchorHandle, OVRPlugin.GetTrackingOriginType());
            anchor.transform.position = pose.ToOVRPose().position;
            anchor.transform.rotation = pose.ToOVRPose().orientation;
        }
    }

    public override ulong CreateSpatialAnchor(Transform T_UnityWorld_Anchor)
    {
        OVRPlugin.SpatialEntityAnchorCreateInfo createInfo = new OVRPlugin.SpatialEntityAnchorCreateInfo()
        {
            Time = OVRPlugin.GetTimeInSeconds(),
            BaseTracking = OVRPlugin.GetTrackingOriginType(),
            PoseInSpace = OVRExtensions.ToOVRPose(T_UnityWorld_Anchor, false).ToPosef()
        };

        ulong anchorHandle = AnchorSession.kInvalidHandle;
        if (OVRPlugin.SpatialEntityCreateSpatialAnchor(createInfo, ref anchorHandle))
        {
            Log("Spatial anchor created with handle: " + anchorHandle);
        }
        else
        {
            Log("OVRPlugin.SpatialEntityCreateSpatialAnchor failed");
        }

        tryEnableComponent(anchorHandle, OVRPlugin.SpatialEntityComponentType.Locatable);
        tryEnableComponent(anchorHandle, OVRPlugin.SpatialEntityComponentType.Storable);

        return anchorHandle;
    }


    public override void DestroyAnchor(ulong anchorHandle)
    {
        Log("DestroyAnchor called on anchorHandle: " + anchorHandle);

        // Destroy anchor gameObject
        if (handleToAnchor.ContainsKey(anchorHandle))
        {
            var anchorObject = handleToAnchor[anchorHandle].gameObject;
            handleToAnchor.Remove(anchorHandle);
            Destroy(anchorObject);
        }

        // Destroy anchor in memory
        if (!OVRPlugin.DestroySpace(ref anchorHandle))
        {
            Log("OVRPlugin.DestroySpace failed for anchorHandle " + anchorHandle);
        }
    }

    public override void EraseAnchor(ulong anchorHandle)
    {
        Log("EraseAnchor called on anchorHandle: " + anchorHandle);

        // Destroy anchor gameObject
        if (handleToAnchor.ContainsKey(anchorHandle))
        {
            Destroy(handleToAnchor[anchorHandle].gameObject);
            handleToAnchor.Remove(anchorHandle);
        }

        // Erase anchor from storage
        ulong eraseRequest = 0;
        if (!OVRPlugin.SpatialEntityEraseSpatialEntity(ref anchorHandle, OVRPlugin.SpatialEntityStorageLocation.Local, ref eraseRequest))
        {
            Log("OVRPlugin.SpatialEntityEraseSpatialEntity failed for anchorHandle " + anchorHandle);
        }
    }

    public void QueryAnchorByUuid()
    {
        // Get number of saved anchor uuids
        if (!PlayerPrefs.HasKey(numUuids))
        {
            PlayerPrefs.SetInt(numUuids, 0);
        }
        int playerNumUuids = PlayerPrefs.GetInt("numUuids");
        Log("numUuids to query with: " + numUuids + " uuids ");
        if (playerNumUuids == 0)
            return;


        OVRPlugin.SpatialEntityUuid[] uuidArr = new OVRPlugin.SpatialEntityUuid[playerNumUuids];
        for (int i = 0; i < playerNumUuids; ++i)
        {
            string uuidKey = "uuid" + i;
            string currentUuid = PlayerPrefs.GetString(uuidKey);
            Log("QueryAnchorByUuid: " + currentUuid);

            var byteArray = AnchorHelpers.StringToUuid(currentUuid);
            uuidArr[i] = new OVRPlugin.SpatialEntityUuid
            {
                Value_0 = BitConverter.ToUInt64(byteArray, 0),
                Value_1 = BitConverter.ToUInt64(byteArray, 8)
            };
        }

        var uuidInfo = new OVRPlugin.SpatialEntityFilterInfoIds
        {
            NumIds = playerNumUuids,
            Ids = uuidArr
        };

        var queryInfo = new OVRPlugin.SpatialEntityQueryInfo()
        {
            QueryType = OVRPlugin.SpatialEntityQueryType.Action,
            MaxQuerySpaces = 20,
            Timeout = 0,
            Location = OVRPlugin.SpatialEntityStorageLocation.Local,
            ActionType = OVRPlugin.SpatialEntityQueryActionType.Load,
            FilterType = OVRPlugin.SpatialEntityQueryFilterType.Ids,
            IdInfo = uuidInfo
        };

        ulong newReqId = 0;
        if (!OVRPlugin.SpatialEntityQuerySpatialEntity(queryInfo, ref newReqId))
        {
            Log("OVRPlugin.SpatialEntityQuerySpatialEntity failed");
        }
    }

    public override void QueryAllLocalAnchors()
    {
        Log("QueryAllLocalAnchors called");
        var queryInfo = new OVRPlugin.SpatialEntityQueryInfo()
        {
            QueryType = OVRPlugin.SpatialEntityQueryType.Action,
            MaxQuerySpaces = 20,
            Timeout = 0,
            Location = OVRPlugin.SpatialEntityStorageLocation.Local,
            ActionType = OVRPlugin.SpatialEntityQueryActionType.Load,
            FilterType = OVRPlugin.SpatialEntityQueryFilterType.None,
        };

        ulong newReqId = 0;
        if (!OVRPlugin.SpatialEntityQuerySpatialEntity(queryInfo, ref newReqId))
        {
            Log("OVRPlugin.SpatialEntityQuerySpatialEntity failed");
        }
    }

    public override void SaveAnchor(ulong anchorHandle, StorageLocation location)
    {
        Log("SaveAnchor called on anchorHandle: " + anchorHandle);
        ulong saveRequest = 0;
        if (!OVRPlugin.SpatialEntitySaveSpatialEntity(ref anchorHandle, OVRPlugin.SpatialEntityStorageLocation.Local, OVRPlugin.SpatialEntityStoragePersistenceMode.IndefiniteHighPri, ref saveRequest))
        {
            Log("OVRPlugin.SpatialEntitySaveSpatialEntity failed for anchorHandle " + anchorHandle + " location " + location);
        }
    }
}
