//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace LptControlApp
//{
//    internal class AsciiEncoder
//    {
//    }
//}
public static class AsciiEncoder
{
    public static byte[] Encode(string ascii)
    {
        if (string.IsNullOrWhiteSpace(ascii))
            return Array.Empty<byte>();

        // Remove spaces and convert hex pairs
        ascii = ascii.Replace(" ", "").Trim();

        // If not hex, encode raw ASCII
        if (!IsHex(ascii))
            return System.Text.Encoding.ASCII.GetBytes(ascii);

        // Convert hex string to bytes
        int len = ascii.Length / 2;
        byte[] result = new byte[len];

        for (int i = 0; i < len; i++)
            result[i] = Convert.ToByte(ascii.Substring(i * 2, 2), 16);

        return result;
    }

    private static bool IsHex(string s)
    {
        foreach (char c in s)
            if (!Uri.IsHexDigit(c))
                return false;
        return true;
    }
}
