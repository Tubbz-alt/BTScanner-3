using NLog;
using System;
using System.Text;


namespace BTScanner
{
    /// <summary>
    /// Describe the Ti command header.
    /// The header in consist of 5 bytes
    /// This class just parse it mainly to get the opcode for the inner message
    /// The ToString() prints details about the header
    /// </summary>
    public class TiCommand
    {
        #region const
        public static readonly byte API_RECEIVE_MSG_TYPE_INDEX = 0;
        public static readonly byte API_RECEIVE_MSG_OPCODE_INDEX = 1;
        public static readonly byte API_RECEIVE_MSG_SIZE_INDEX = 2;
        public static readonly byte API_RECEIVE_HEADER_LENGTH = 5;
        public static readonly byte API_RECEIVE_MSG_MAX_SIZE = 255;

        //ti message typ's
        public static readonly byte TI_PROTOCOL_COMMAND_TYPE_MESSAGE = 1;
        public static readonly byte TI_PROTOCOL_EVENT_TYPE_MESSAGE = 4;
        #endregion

        #region members
        public byte type;
        public byte opCode;
        public byte length;
        public ushort eventOpCode;
        public byte[] data;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region CTOR
        public TiCommand(byte[] buffer)
        {
            try
            {
                type = buffer[API_RECEIVE_MSG_TYPE_INDEX];
                opCode = buffer[API_RECEIVE_MSG_OPCODE_INDEX];
                length = buffer[API_RECEIVE_MSG_SIZE_INDEX];

                eventOpCode = (ushort)(buffer[API_RECEIVE_MSG_SIZE_INDEX + 2] << 8);
                eventOpCode += buffer[API_RECEIVE_MSG_SIZE_INDEX + 1];

                data = new byte[length];
                Array.Copy(buffer, API_RECEIVE_HEADER_LENGTH + 1, data, 0, length);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Wrror during paring TiCommand");
            }
        }
        #endregion

        #region override functions
        public override string ToString()
        {
            return "Type is : " + type.ToString("X2") + " ,Opcode is : " + opCode.ToString("X2") + Environment.NewLine +
                    "Length is : " + length.ToString("X2") + " ,Event opcode is : " + eventOpCode.ToString("X4") + Environment.NewLine +
                    "Data is : " + BitConverter.ToString(data) + Environment.NewLine + " ---------------------------------" + Environment.NewLine;
        } 
        #endregion
    }




}
