using System.Text;

namespace ControlLibrary.Helpers
{
    internal class CryptographyHelper
    {
        // Demo, not for production use!
        internal static string Decrypt(string base64String)
        {
            // 1. Convert the Base64 string back to a byte array
            byte[] base64EncodedBytes = Convert.FromBase64String(base64String);

            // 2. Convert the byte array back to a string using the *same* encoding (UTF-8)
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        // Demo, not for production use!
        internal static string Encrypt(string plainText)
        {
            // 1. Convert the string to a byte array using UTF-8 encoding
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            // 2. Convert the byte array to a Base64 string
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
