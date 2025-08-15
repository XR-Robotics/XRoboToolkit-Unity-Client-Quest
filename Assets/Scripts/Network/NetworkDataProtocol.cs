using System;
using System.Collections.Generic;
using System.Text;

namespace XRoboToolkit.Network
{
    /// <summary>
    /// Custom network data protocol structure containing command, length, and data
    /// </summary>
    [Serializable]
    public class NetworkDataProtocol
    {
        public string command;
        public int length;
        public byte[] data;

        /// <summary>
        /// Default constructor that initializes empty protocol data
        /// </summary>
        public NetworkDataProtocol()
        {
            command = string.Empty;
            length = 0;
            data = new byte[0];
        }

        /// <summary>
        /// Constructor that creates protocol data with command and data payload
        /// Automatically calculates length from data array
        /// </summary>
        /// <param name="command">Command string for the protocol</param>
        /// <param name="data">Data payload as byte array</param>
        public NetworkDataProtocol(string command, byte[] data)
        {
            this.command = command ?? string.Empty;
            this.data = data ?? new byte[0];
            this.length = this.data.Length;
        }

        /// <summary>
        /// Constructor that creates protocol data with command, explicit length, and data payload
        /// Allows specifying length independently of data array size
        /// </summary>
        /// <param name="command">Command string for the protocol</param>
        /// <param name="length">Explicit length value for the protocol</param>
        /// <param name="data">Data payload as byte array</param>
        public NetworkDataProtocol(string command, int length, byte[] data)
        {
            this.command = command ?? string.Empty;
            this.length = length;
            this.data = data ?? new byte[0];
        }
    }
}
