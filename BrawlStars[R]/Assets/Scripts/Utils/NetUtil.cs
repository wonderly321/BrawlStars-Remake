using UnityEngine;
using System;
using System.Collections;

// 使用小端字节序进行 float/int/uint 和bytes直接的互转
public class NetUtil 
{
    public static uint BytesToUintLD(byte[] buf, int offset = 0) 
    {
        if (!BitConverter.IsLittleEndian) 
        {
            Array.Reverse(buf, offset, 4);
            uint res = BitConverter.ToUInt32(buf, offset);
            Array.Reverse(buf, offset, 4);
            return res;
        }
        return BitConverter.ToUInt32(buf, offset);
    }

    public static int BytesToIntLD(byte[] buf, int offset = 0) 
    {
        if (!BitConverter.IsLittleEndian) 
        {
            Array.Reverse(buf, offset, 4);
            int res = BitConverter.ToInt32(buf, offset);
            Array.Reverse(buf, offset, 4);
            return res;
        }
        return BitConverter.ToInt32(buf, offset);
    }

    public static float BytesToFloatLD(byte[] buf, int offset = 0) 
    {
        if (!BitConverter.IsLittleEndian) 
        {
            Array.Reverse(buf, offset, 4);
            float res = BitConverter.ToSingle(buf, offset);
            Array.Reverse(buf, offset, 4);
            return res;
        }
        return BitConverter.ToSingle(buf, offset);
    }

    public static byte[] UintToBytesLD(uint num) 
    {
        byte[] res = BitConverter.GetBytes(num);
        if (!BitConverter.IsLittleEndian) 
        {
            Array.Reverse(res);
        }
        return res;
    }

    public static byte[] IntToBytesLD(int num) 
    {
        byte[] res = BitConverter.GetBytes(num);
        if (!BitConverter.IsLittleEndian) 
        {
            Array.Reverse(res);
        }
        return res;
    }

    public static byte[] FloatToBytesLD(float num) 
    {
        byte[] res = BitConverter.GetBytes(num);
        if (!BitConverter.IsLittleEndian) 
        {
            Array.Reverse(res);
        }
        return res;
    }

}
