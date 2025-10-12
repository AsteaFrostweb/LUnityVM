using System;
using System.Security.Cryptography;
using System.Text;

public class UIDGenerator
{
    private static long _uidIndex = 0;
    private static readonly object _lock = new();
    private static readonly HashAlgorithm _sha = SHA256.Create();

    //Generate's a 64 Character UID
    public static string GenerateUID()
    {
        lock (_lock)
        {
            byte[] input = BitConverter.GetBytes(_uidIndex++);
            byte[] hash = _sha.ComputeHash(input);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}

