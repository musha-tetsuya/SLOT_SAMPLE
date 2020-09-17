using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// 暗号化
/// </summary>
public static class CryptionUtility
{
    /// <summary>
    /// SHA-2ハッシュアルゴリズム
    /// </summary>
    private static readonly SHA256CryptoServiceProvider algorithm = new SHA256CryptoServiceProvider();
    /// <summary>
    /// 暗号化用バイト配列
    /// </summary>
    private static readonly byte[] cryptionBytes = Encoding.UTF8.GetBytes("f}op0U'8]%@hZ;7S");

    /// <summary>
    /// バイナリ暗号化/複合化
    /// </summary>
    public static byte[] Cryption(this byte[] bytes)
    {
        var resultBytes = new byte[bytes.Length];

        for (int i = 0; i < bytes.Length; i++)
        {
            resultBytes[i] = (byte)(bytes[i] ^ cryptionBytes[i % cryptionBytes.Length]);
        }

        return resultBytes;
    }

    /// <summary>
    /// ハッシュ文字列に変換
    /// </summary>
    public static string GetHashString(this string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        bytes = algorithm.ComputeHash(bytes);
        return BitConverter.ToString(bytes).Replace("-", null).ToLower();
    }
}
