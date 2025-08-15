namespace Network
{
    public class NetUtils
    {
        /// <summary>
        /// Converts a byte value to its hexadecimal string representation
        /// </summary>
        /// <param name="value">Byte value to convert</param>
        /// <returns>Hexadecimal string in format "0xXX"</returns>
        public static string Get16String(byte value)
        {
            return "0x" + value.ToString("X2");
        }
    }
}