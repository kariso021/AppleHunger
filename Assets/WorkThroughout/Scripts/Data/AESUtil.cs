using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class AESUtil
{
    // Node.js에서 사용한 Base64 키와 IV를 여기에 그대로 넣어주세요.
    private static readonly string base64Key = "8peBc9QJK+qI3V4fYLrkV+o7EqM7BunIWLjsXvJI0P0=";
    private static readonly string base64IV = "xzyxm5Vr8jvCHXo+PRROAQ==";

    private static readonly byte[] aesKey = Convert.FromBase64String(base64Key);
    private static readonly byte[] aesIV = Convert.FromBase64String(base64IV);

    public static string Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = aesKey;
        aes.IV = aesIV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        ICryptoTransform encryptor = aes.CreateEncryptor();
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(encrypted);
    }

    public static string Decrypt(string cipherTextBase64)
    {
        using Aes aes = Aes.Create();
        aes.Key = aesKey;
        aes.IV = aesIV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        ICryptoTransform decryptor = aes.CreateDecryptor();
        byte[] cipherBytes = Convert.FromBase64String(cipherTextBase64);
        byte[] decrypted = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(decrypted);
    }
}
