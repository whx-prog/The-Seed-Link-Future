// Copyright (c) Facebook, Inc. and its affiliates. All Rights Reserved.

using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class AnchorHelpers
{
    private static Camera camera;

    // Caching main camera for performance
    private static Camera MainCamera
    {
        get
        {
            if (camera == null)
            {
                camera = Camera.main;
            }
            return camera;
        }
    }

    public static OVRPose getTransformUnityWorldFromDevice()
    {
        OVRPose T_UnityWorld_Device = MainCamera.transform.ToOVRPose();
        return T_UnityWorld_Device;
    }

    // Used to cast byte arrays into structs for the SPS event queue
    public static T ByteArrayToStruct<T>(byte[] bytes) where T : struct
    {
        T val;
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            val = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }
        finally
        {
            handle.Free();
        }
        return val;
    }

    // Converts Byte array to string with Uuid format "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX"
    public static string UuidToString(byte[] encodedMessage)
    {
        if (encodedMessage.Length != 16)
        {
            AnchorSession.Log("UuidToString failed because uuid byte array was incorrect length: " + encodedMessage.Length);
            return "";
        }

        StringBuilder message = new StringBuilder("", 36);
        var messageWithNoDashes = BitConverter.ToString(encodedMessage).ToLower().Replace('-'.ToString(), String.Empty);
        for (int iter = 0; iter < 32; iter++)
        {
            if (iter == 8 || iter == 12 || iter == 16 || iter == 20)
            {
                message.Append("-");
            }
            message.Append(messageWithNoDashes[iter]);
        }
        return message.ToString();
    }

    // Converts hex string with Uuid format "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX" to Byte array Uuid
    public static byte[] StringToUuid(string str)
    {
        byte[] uuid = new byte[16];
        str = str.Replace("-", "");
        if (str.Length != 32)
        {
            AnchorSession.Log("StringToUuid failed because string was incorrect length: " + str.Length);
            return uuid;
        }

        for (int i = 0; i < 16; ++i)
        {
            uuid[i] = (byte)(GetHexVal(str[2 * i]) * 16 + GetHexVal(str[(2 * i) + 1]));
        }
        return uuid;
    }

    private static int GetHexVal(char hex)
    {
        int val = (int)hex;
        if (val >= '0' && val <= '9')
        {
            return val - 48;
        }
        else if (val >= 'a' && val <= 'f')
        {
            return val - 87;
        }
        else if (val >= 'A' && val <= 'F')
        {
            return val - 55;
        }
        else
        {
            AnchorSession.Log("GetHexVal failed because of invalid hex character: " + hex);
            return 0;
        }
    }
}
