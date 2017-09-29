using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tiota
{
    class devUtils
    {

        private const string ErrorStr = "Error";

        public string GetOpCodeName(UInt16 opCode)
        {
            HCICmds HCICmdsObj = new HCICmds();
            for (uint i = 0; i < HCICmdsObj.OpCodeLookupTable.Length / 2; i++)
            {
                if (HCICmdsObj.OpCodeLookupTable[i, 0] == String.Format("0x{0:X4}", opCode))
                {
                    return HCICmdsObj.OpCodeLookupTable[i, 1];
                }
            }
            return "Unknown Op Code";
        }


        // --------------------------------------------------------------------------------
        //  @fn       CheckLineLength()
        //  @brief    Checks and adds the necessary bytes to move to the next line.
        //  @param    string msg - the message line to process.
        //            uint lineIndex - the number of data bytes in the line
        //            bool addTabs - add two tabs after line break.
        //  @returns  None
        // --------------------------------------------------------------------------------
        public void CheckLineLength(ref string msg, uint lineIndex, bool addTabs)
        {
            if (((lineIndex + 1) % 16) == 0)  // limit dump string to 16 per line 
            {
                if (addTabs == true)
                    msg += "\n\t\t  ";
                else
                    msg += "\n";
            }
            return;
        }

        // --------------------------------------------------------------------------------
        //  @fn       BuildRawDataStr()
        //  @brief    Build a raw data string for display
        //  @param    Byte[] data - the data to process
        //            string msg - the message line to process.
        //            int length - the number of data bytes to process
        //  @returns  None
        // --------------------------------------------------------------------------------
        public void BuildRawDataStr(Byte[] data, ref string msg, int length)
        {
            if (length > 0)
            {
                string tempStr = String.Empty;
                for (uint i = 0; i < length; i++)
                {
                    tempStr += String.Format("{0:X2} ", data[i]);
                    CheckLineLength(ref tempStr, i, true);  // limit dump string to 16 per line
                }
                msg += String.Format(" Raw\t\t: {0:S}\n", tempStr);
            }
            return;
        }

        // danger this maybe dead
        // --------------------------------------------------------------------------------
        //  @fn       Opcode2String()
        //  @brief    Converts an opcode into a string
        //  @param    UInt16 opCode - the opcode to convert
        // --------------------------------------------------------------------------------
        public string Opcode2String(UInt16 opCode)
        {
            return string.Format("{0:X2} {1:X2}", (opCode & 0xFF), ((opCode >> 8) & 0xFF));
        }

        // --------------------------------------------------------------------------------
        //  @fn       String2BDA_LSBMSB()
        //  @brief    Convert from a BDA address from a string to a byte array
        //  @param    string BDAStr - the address to convert
        //  @returns  Byte[] - the converted address
        // --------------------------------------------------------------------------------
        public Byte[] String2BDA_LSBMSB(string BDAStr)
        {
            Byte[] BDAddr = new Byte[6];
            string[] splitStr = BDAStr.Split(new char[] { ' ', ':' });

            if (splitStr.Length == 6)
            {
                for (uint i = 0; i < 6; i++)
                {
                    try
                    {
                        BDAddr[5 - i] = Convert.ToByte(splitStr[i], 16);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
            return BDAddr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       String2Bytes_LSBMSB()
        //  @brief    Take a string and make it into a byte array.
        //            Remove any ':' or space found in the string to parse
        //  @param    string str - the string to parse
        //            Byte radix - the numerical base for the string conversion
        //  @returns  Byte[] - an array of bytes
        // --------------------------------------------------------------------------------
        public Byte[] String2Bytes_LSBMSB(string str, Byte radix)
        {
            string[] splitStr;// = str.Split(new char[] { ' ', ':' });
            Byte[] Bytes; //= new Byte[splitStr.Length];
            uint i = 0;

            if (radix != 0xFF)
            {
                splitStr = str.Split(new char[] { ' ', ':' });

                // determine actual length
                int actualLength = 0;
                for (i = 0; i < splitStr.Length; i++)
                {
                    if (splitStr[i].Length > 0)
                    {
                        actualLength++;
                    }
                }

                // allocate for only strings that have length
                Bytes = new Byte[actualLength];

                int byteIndex = 0;
                for (i = 0; i < splitStr.Length; i++)
                {
                    try
                    {
                        // only convert strings that have some length
                        if (splitStr[i].Length > 0)
                        {
                            Bytes[byteIndex++] = Convert.ToByte(splitStr[i], radix);
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            else
            {
                char[] chars = str.ToCharArray();
                Bytes = new Byte[chars.Length];

                for (i = 0; i < chars.Length; i++)
                {
                    Bytes[i] = (Byte)chars[i];
                }
            }
            return Bytes;
        }




        // --------------------------------------------------------------------------------
        //  @fn       String2UInt16_LSBMSB()
        //  @brief    Convert a string into an array of UInt16 values
        //  @param    string str - the string to convert
        //            Byte radix - numerical base 
        // --------------------------------------------------------------------------------
        public UInt16[] String2UInt16_LSBMSB(string str, Byte radix)
        {
            string[] splitStr;// = str.Split(new char[] { ' ', ':' });
            UInt16[] UInt16Array; //= new Byte[splitStr.Length];
            uint i = 0;

            if (radix != 0xFF)
            {
                splitStr = str.Split(new char[] { ' ', ':', ';' });
                UInt16Array = new UInt16[splitStr.Length];

                for (i = 0; i < splitStr.Length; i++)
                {
                    try
                    {
                        if (splitStr[i] != String.Empty)
                        {
                            UInt16Array[i] = Convert.ToUInt16(splitStr[i], radix);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            else
            {
                UInt16Array = null;
            }
            return UInt16Array;
        }

        // --------------------------------------------------------------------------------
        //  @fn       _GetByte16()
        //  @brief    Extract a byte from a 16 bit value
        //  @param    UInt16 value - the value to set
        //            Byte byteNumber - the byte number to set
        // --------------------------------------------------------------------------------
        public Byte _GetByte16(UInt16 value, Byte byteNumber)
        {
            return (Byte)((value >> (8 * byteNumber)) & 0xFF);
        }

        // --------------------------------------------------------------------------------
        //  @fn       _GetByte32()
        //  @brief    Extract a byte from a 32 bit value
        //  @param    UInt32 value - the value to set
        //            Byte byteNumber - the byte number to set
        // --------------------------------------------------------------------------------
        public Byte _GetByte32(UInt32 value, Byte byteNumber)
        {
            return (Byte)((value >> (8 * byteNumber)) & 0xFF);
        }

        // --------------------------------------------------------------------------------
        //  @fn       _GetByte64()
        //  @brief    Extract a byte from a 64 bit value
        //  @param    UInt64 value - the value to set
        //            Byte byteNumber - the byte number to set
        // --------------------------------------------------------------------------------
        public Byte _GetByte64(UInt64 value, Byte byteNumber)
        {
            return (Byte)((value >> (8 * byteNumber)) & 0xFF);
        }

        // --------------------------------------------------------------------------------
        //  @fn       _SetByte16()
        //  @brief    Set 16 bit value one byte at a time
        //  @param    Byte value - the byte to set
        //            Byte byteNumber - the byte number to set
        // --------------------------------------------------------------------------------
        public UInt16 _SetByte16(Byte value, Byte byteNumber)
        {
            return (UInt16)(value << (8 * byteNumber));
        }

        // --------------------------------------------------------------------------------
        //  @fn       _SetByte32()
        //  @brief    Set 32 bit value one byte at a time
        //  @param    Byte value - the byte to set
        //            Byte byteNumber - the byte number to set
        // --------------------------------------------------------------------------------
        public UInt32 _SetByte32(Byte value, Byte byteNumber)
        {
            return (UInt32)(value << (8 * byteNumber));
        }

        // --------------------------------------------------------------------------------
        //  @fn       _SetByte64()
        //  @brief    Set 64 bit value one byte at a time
        //  @param    Byte value - the byte to set
        //            Byte byteNumber - the byte number to set
        // --------------------------------------------------------------------------------
        public UInt64 _SetByte64(Byte value, Byte byteNumber)
        {
            return (UInt64)(value << (8 * byteNumber));
        }

        #region Loaders
        // --------------------------------------------------------------------------------
        //  @fn       LoadMsgHeader()
        //  @brief    Loads the message header
        //  @param    ref Byte[] data - the destination data
        //            ref int index - the destination data index.
        //            Byte packetType - the message packet type;
        //            UInt16 opCode - the message op code
        //            Byte dataLength - the message data length;
        //  @returns  None
        // --------------------------------------------------------------------------------
        public void LoadMsgHeader(ref Byte[] data,
                                  ref int index,
                                  Byte packetType,
                                  UInt16 opCode,
                                  Byte dataLength)
        {
            try
            {
                data[index++] = packetType;
                Load16Bits(ref data, ref index, opCode);
                data[index++] = dataLength;
            }
            catch
            {
                /* MessageBox.Show("Load Msg Header Failed\nMessage Data Transfer Issue.\n",
                                 ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
                 * */
            }
            return;
        }

        // --------------------------------------------------------------------------------
        //  @fn       Load8Bits()
        //  @brief    Loads 8 bits into a destination array
        //  @param    ref Byte[] data - the destination data
        //            ref int index - the destination data index.
        //            UInt16 bits - the source data bits to move
        //  @returns  None
        // --------------------------------------------------------------------------------
        public void Load8Bits(ref Byte[] data,
                              ref int index,
                              Byte bits)
        {
            try
            {
                data[index++] = bits;
            }
            catch
            {
                //MessageBox.Show("Load 8 Bits Failed\nMessage Data Transfer Issue.\n", ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return;
        }

        // --------------------------------------------------------------------------------
        //  @fn       Load16Bits()
        //  @brief    Loads 16 bits into a destination array
        //  @param    ref Byte[] data - the destination data
        //            ref int index - the destination data index.
        //            UInt16 bits - the source data bits to move
        //  @returns  None
        // --------------------------------------------------------------------------------
        public void Load16Bits(ref Byte[] data,
                               ref int index,
                               UInt16 bits)
        {
            try
            {
                data[index++] = _GetByte16(bits, 0);
                data[index++] = _GetByte16(bits, 1);
            }
            catch
            {
                //MessageBox.Show("Load 16 Bits Failed\nMessage Data Transfer Issue.\n",ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return;
        }

        // --------------------------------------------------------------------------------
        //  @fn       Load32Bits()
        //  @brief    Loads 32 bits into a destination array
        //  @param    ref Byte[] data - the destination data
        //            ref int index - the destination data index.
        //            UInt32 bits - the source data bits to move
        //  @returns  None
        // --------------------------------------------------------------------------------
        public void Load32Bits(ref Byte[] data,
                               ref int index,
                               UInt32 bits)
        {
            try
            {
                data[index++] = _GetByte32(bits, 0);
                data[index++] = _GetByte32(bits, 1);
                data[index++] = _GetByte32(bits, 2);
                data[index++] = _GetByte32(bits, 3);
            }
            catch
            {
             //   MessageBox.Show("Load 32 Bits Failed\nMessage Data Transfer Issue.\n",
               //                 ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return;
        }

        // --------------------------------------------------------------------------------
        //  @fn       LoadDataBytes()
        //  @brief    Move data bytes into a a destination array
        //  @param    ref Byte[] data - the destination data
        //            ref int index - the destination data index.
        //            Byte [] sourceData - the source data bytes to move
        //  @returns  None
        // --------------------------------------------------------------------------------
        public void LoadDataBytes(ref Byte[] data,
                                  ref int index,
                                  Byte[] sourceData)
        {
            try
            {
                if (sourceData != null)
                {
                    int startIndex = index;
                    for (int i = startIndex;
                         i < (startIndex + (sourceData.Length));
                         i++)
                    {
                        // prevent destination bad access
                        if (data.Length <= i)
                        {
                            break;
                        }
                        data[i] = sourceData[i - startIndex];
                        index++;
                    }
                }
            }
            catch
            {
                //MessageBox.Show("Load Data Bytes\nMessage Data Transfer Issue.\n",
                  //              ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion // Unloaders
        #region Unloaders
        // --------------------------------------------------------------------------------
        //  @fn       UnloadATTEventMsgHeader()
        //  @brief    Unloads an ATT event msg header
        //  @param    ref Byte[] data - the destination data
        //            ref int index - the destination data index.
        //            ref bool dataErr - true = error and false = no error
        //  @returns  Byte pduLen the remaining length of the message
        // --------------------------------------------------------------------------------
        public Byte UnloadMsgHeader(ref Byte[] data,
                                    ref int index,
                                    ref string msg,
                                    ref bool dataErr)
        {
            Byte tmpPduLen = 0x00;
            UInt16 tmpUInt16 = 0;
            try
            {
                tmpUInt16 = Unload16Bits(data, ref index, ref dataErr);
                msg += String.Format(" ConnHandle\t: 0x{0:X4} ({1:D})\n",
                                     tmpUInt16, tmpUInt16);
                tmpPduLen = Unload8Bits(data, ref index, ref dataErr);
                msg += String.Format(" PduLen\t\t: 0x{0:X2} ({1:D})\n",
                                     tmpPduLen, tmpPduLen);
            }
            catch
            {
                //MessageBox.Show("Unload 8 Bits Failed\nMessage Data Transfer Issue.\n",
                  //              ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataErr = true;
            }
            return tmpPduLen;
        }

        // --------------------------------------------------------------------------------
        //  @fn       UnloadATTEventMsgHeader()
        //  @brief    Unloads an ATT event msg header
        //  @param    ref Byte[] data - the destination data
        //            ref int index - the destination data index.
        //            ref bool dataErr - true = error and false = no error
        //  @returns  Byte pduLen the remaining length of the message
        // --------------------------------------------------------------------------------
        public Byte UnloadMsgHeaderR(ref Byte[] data,
                                    ref int index,
                                    ref string msg,
                                    ref bool dataErr,
                                    ref ushort connHandle)
        {
            Byte tmpPduLen = 0x00;
            
            try
            {
                connHandle = Unload16Bits(data, ref index, ref dataErr);
                msg += String.Format(" ConnHandle\t: 0x{0:X4} ({1:D})\n",
                                     connHandle, connHandle);
                tmpPduLen = Unload8Bits(data, ref index, ref dataErr);
                msg += String.Format(" PduLen\t\t: 0x{0:X2} ({1:D})\n",
                                     tmpPduLen, tmpPduLen);
            }
            catch
            {
                //MessageBox.Show("Unload 8 Bits Failed\nMessage Data Transfer Issue.\n",
                  //              ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataErr = true;
            }
            return tmpPduLen;
        }





        // --------------------------------------------------------------------------------
        //  @fn       Unload8Bits()
        //  @brief    Unloads 8 bits from a source array
        //  @param    Byte[] data - the source array
        //            ref int index - the source data index.
        //            ref Byte bits - the destination data bits
        //            ref bool dataErr - true = error and false = no error
        //  @returns  Byte - the same value as bits
        // --------------------------------------------------------------------------------
        public Byte Unload8Bits(Byte[] data,
                                ref int index,
                                ref Byte bits,
                                ref bool dataErr)
        {
            try
            {
                bits = data[index++];
            }
            catch
            {
                //     MessageBox.Show("Unload 8 Bits Failed\nMessage Data Transfer Issue.\n",
                //                      ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataErr = true;
            }
            return bits;
        }

        // --------------------------------------------------------------------------------
        //  @fn       Unload8Bits()
        //  @brief    Unloads 8 bits from a source array
        //  @param    Byte[] data - the source array
        //            ref int index - the source data index.
        //            ref bool dataErr - true = error and false = no error
        //  @returns  Byte - the same value as bits
        // --------------------------------------------------------------------------------
        public Byte Unload8Bits(Byte[] data,
                                ref int index,
                                ref bool dataErr)
        {
            Byte bits = 0;
            return Unload8Bits(data, ref index, ref bits, ref dataErr);
        }

        // --------------------------------------------------------------------------------
        //  @fn       Unload16Bits()
        //  @brief    Unloads 16 bits from a source array 
        //  @param    Byte[] data - the source array 
        //            ref int index - the source data index. 
        //            ref UInt16 bits - the destination data bits 
        //            ref bool dataErr - true = error and false = no error
        //  @returns  UInt16 - the same value as bits
        // --------------------------------------------------------------------------------
        public UInt16 Unload16Bits(Byte[] data,
                                   ref int index,
                                   ref UInt16 bits,
                                   ref bool dataErr)
        {
            try
            {
                bits = _SetByte16(data[index++], 0);
                bits += _SetByte16(data[index++], 1);
            }
            catch
            {
                //MessageBox.Show("Unload 16 Bits Failed\nMessage Data Transfer Issue.\n",
                  //              ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataErr = true;
            }
            return bits;
        }

        // --------------------------------------------------------------------------------
        //  @fn       Unload16Bits()
        //  @brief    Unloads 16 bits from a source array 
        //  @param    Byte[] data - the source array 
        //            ref int index - the source data index. 
        //            ref bool dataErr - true = error and false = no error
        //  @returns  UInt16 - the same value as bits
        // --------------------------------------------------------------------------------
        public UInt16 Unload16Bits(Byte[] data,
                                   ref int index,
                                   ref bool dataErr)
        {
            UInt16 bits = 0;
            return Unload16Bits(data, ref index, ref bits, ref dataErr);
        }

        // --------------------------------------------------------------------------------
        //  @fn       Unload32Bits()
        //  @brief    Unloads 32 bits from a source array 
        //  @param    Byte[] data - the source array 
        //            ref int index - the source data index. 
        //            ref UInt32 bits - the destination data bits 
        //            ref bool dataErr - true = error and false = no error
        //  @returns  UInt32 - the same value as bits 
        // --------------------------------------------------------------------------------
        public UInt32 Unload32Bits(Byte[] data,
                                   ref int index,
                                   ref UInt32 bits,
                                   ref bool dataErr)
        {
            try
            {
                bits = _SetByte32(data[index++], 0);
                bits += _SetByte32(data[index++], 1);
                bits += _SetByte32(data[index++], 2);
                bits += _SetByte32(data[index++], 3);
            }
            catch
            {
                //MessageBox.Show("Unload 32 Bits Failed\nMessage Data Transfer Issue.\n",
                       //         ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataErr = true;
            }
            return bits;
        }

        // --------------------------------------------------------------------------------
        //  @fn       Unload32Bits()
        //  @brief    Unloads 32 bits from a source array 
        //  @param    Byte[] data - the source array 
        //            ref int index - the source data index. 
        //            ref bool dataErr - true = error and false = no error
        //  @returns  UInt32 - the same value as bits 
        // --------------------------------------------------------------------------------
        public UInt32 Unload32Bits(Byte[] data,
                                   ref int index,
                                   ref bool dataErr)
        {
            UInt32 bits = 0;
            return Unload32Bits(data, ref index, ref bits, ref dataErr);
        }

        // --------------------------------------------------------------------------------
        //  @fn       Unload64Bits()
        //  @brief    Unloads 64 bits from a source array 
        //  @param    Byte[] data - the source array 
        //            ref int index - the source data index. 
        //            ref UInt64 bits - the destination data bits 
        //            ref bool dataErr - true = error and false = no error
        //  @returns  UInt64 - the same value as bits 
        // --------------------------------------------------------------------------------
        public UInt64 Unload64Bits(Byte[] data,
                                   ref int index,
                                   ref UInt64 bits,
                                   ref bool dataErr)
        {
            try
            {
                bits = _SetByte64(data[index++], 0);
                bits += _SetByte64(data[index++], 1);
                bits += _SetByte64(data[index++], 2);
                bits += _SetByte64(data[index++], 3);
                bits += _SetByte64(data[index++], 4);
                bits += _SetByte64(data[index++], 5);
                bits += _SetByte64(data[index++], 6);
                bits += _SetByte64(data[index++], 7);
            }
            catch
            {
               // MessageBox.Show("Unload 64 Bits Failed\nMessage Data Transfer Issue.\n",
                 //               ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataErr = true;
            }
            return bits;
        }

        // --------------------------------------------------------------------------------
        //  @fn       Unload64Bits()
        //  @brief    Unloads 64 bits from a source array 
        //  @param    Byte[] data - the source array 
        //            ref int index - the source data index. 
        //            ref bool dataErr - true = error and false = no error
        //  @returns  UInt64 - the same value as bits 
        // --------------------------------------------------------------------------------
        public UInt64 Unload64Bits(Byte[] data,
                                   ref int index,
                                   ref bool dataErr)
        {
            UInt64 bits = 0;
            return Unload64Bits(data, ref index, ref bits, ref dataErr);
        }

        // --------------------------------------------------------------------------------
        //  @fn       UnloadDeviceAddr()
        //  @brief    Move data bytes into a string
        //  @param    Byte[] data - the source data
        //            ref Byte[]addr - destination array to store address     
        //            ref int index - the source data index.
        //            bool direction - direction of address (true = forward and false = backward)
        //            ref bool dataErr - true = error and false = no error
        //  @returns  string containing the device address
        // --------------------------------------------------------------------------------
        public string UnloadDeviceAddr(Byte[] data,
                                       ref Byte[] addr,
                                       ref int index,
                                       bool direction,
                                       ref bool dataErr)
        {
            string dataStr;
            dataStr = String.Empty;
            Byte tmpByte = 0x00;
            dataErr = false;

            try
            {
                /* device address */
                for (int i = 0; (i < 6) && (dataErr == false); i++)
                {
                    Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                    addr[i] = tmpByte;
                    if (direction == true) // forward
                    {
                        if (i != 5)
                        {
                            dataStr += String.Format("{0:X2}:", tmpByte);
                        }
                        else
                        {
                            dataStr += String.Format("{0:X2}", tmpByte);
                        }
                    }
                    else  // reverse
                    {
                        if (i != 0)
                        {
                            dataStr = String.Format("{0:X2}:", tmpByte) + dataStr;
                        }
                        else
                        {
                            dataStr = String.Format("{0:X2}", tmpByte) + dataStr;
                        }
                    }
                }
            }
            catch
            {
               // MessageBox.Show("Unload Device Address\nMessage Data Transfer Issue.\n",
                //                ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataErr = true;
            }
            return dataStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       UnloadColonData()
        //  @brief    Move data bytes into a string
        //  @param    Byte[] data - the source data
        //            ref int index - the source data index.
        //            int numBytes - the number of bytes to move
        //            ref bool dataErr - true = error and false = no error
        //  @returns  string containing the device address
        // --------------------------------------------------------------------------------
        public string UnloadColonData(Byte[] data,
                                      ref int index,
                                      int numBytes,
                                      ref bool dataErr)
        {
            string dataStr;
            dataStr = String.Empty;
            Byte tmpByte = 0x00;
            dataErr = false;

            try
            {
                /* device address */
                for (int i = 0; (i < numBytes) && (dataErr == false); i++)
                {
                    Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                    if (i != (numBytes - 1))
                    {
                        dataStr += String.Format("{0:X2}:", tmpByte);
                    }
                    else
                    {
                        dataStr += String.Format("{0:X2}", tmpByte);
                    }
                    if (i != numBytes - 1)
                    {
                        CheckLineLength(ref dataStr, (uint)i, true);  // limit dump string to 16 bytes per line 
                    }
                }
            }
            catch
            {
                //MessageBox.Show("Unload Colon Data\nMessage Data Transfer Issue.\n",
                 //               ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataErr = true;
            }
            return dataStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       UnloadHandleValueData()
        //  @brief    Move data bytes into a string of handle value pairs
        //  @param    Byte[] data - the source data
        //            ref int index - the source data index.
        //            int totalLength - the total length of the data
        //            int dataLength - the data length (value)
        //            ref string handleStr - a secondary string this one long line of data bytes only
        //            ref string valueStr - a secondary string this one long line of data bytes only
        //            ref bool dataErr - true = error and false = no error
        //  @returns  string containing the handle value pairs
        // --------------------------------------------------------------------------------
        public string UnloadHandleValueData(Byte[] data,
                                            ref int index,
                                            int totalLength,
                                            int dataLength,
                                            ref string handleStr,
                                            ref string valueStr,
                                            ref bool dataErr)
        {
            string msgStr = String.Empty;

            // parse out the handles and the values
            string dumpStr = String.Empty;
            valueStr = String.Empty;
            handleStr = String.Empty;
            UInt16 currentHandle = HCICmds.MaxUInt16;
            dataErr = false;
            int masterLength = totalLength;
            Byte tmpByte = 0x00;

            if (dataLength != 0)
            {
                while ((masterLength > 0) && (dataErr == false) && (masterLength >= (Byte)dataLength))
                {
                    try
                    {
                        // get the handle
                        currentHandle = HCICmds.MaxUInt16;
                        currentHandle = Unload16Bits(data, ref index, ref dataErr);

                        // step thru all the data bytes less the handle length
                        int maxIndex = dataLength - 2;
                        for (int i = 0; (i < maxIndex) && (dataErr == false); i++)
                        {
                            Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                            valueStr += String.Format("{0:X2} ", tmpByte);   // this string is one long line
                            if (i != (maxIndex - 1))
                            {
                                dumpStr += String.Format("{0:X2}:", tmpByte);    // this string is seperated into groups of 16
                            }
                            else
                            {
                                dumpStr += String.Format("{0:X2}", tmpByte);    // this string is seperated into groups of 16
                            }
                            CheckLineLength(ref dumpStr, (uint)i, true);  // limit dump string to 16 bytes per line 
                        }
                    }
                    catch
                    {
                        //MessageBox.Show("Unload Handle Value Data\nMessage Data Transfer Issue.\n",
                          //              ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        dataErr = true;
                    }

                    // build the output
                    // handle(s)
                    handleStr += String.Format("0x{0:X4} ", currentHandle);   // this string is one long line of handles
                    msgStr += String.Format(" Handle\t\t: 0x{0:X4}\n", currentHandle);
                    // value(s)
                    msgStr += String.Format(" Data\t\t: {0:S}\n", dumpStr);
                    dumpStr = String.Empty;

                    masterLength -= (Byte)dataLength;  // decrement the master length 
                }
            }
            return msgStr;
        }


        // --------------------------------------------------------------------------------
        //  @fn       UnloadHandleValueData()
        //  @brief    Move data bytes into a string of handle value pairs
        //  @param    Byte[] data - the source data
        //            ref int index - the source data index.
        //            int totalLength - the total length of the data
        //            int dataLength - the data length (value)
        //            ref string handleStr - a secondary string this one long line of data bytes only
        //            ref string valueStr - a secondary string this one long line of data bytes only
        //            ref bool dataErr - true = error and false = no error
        //  @returns  string containing the handle value pairs
        // --------------------------------------------------------------------------------

        public string UnloadThermometerHandleValueData(Byte[] data,
                                            ref int index,
                                            int totalLength,
                                            int dataLength,
                                            ref bool dataErr)
        {
            string msgStr = String.Empty;

            // parse out the handles and the values
            string dumpStr = String.Empty;

            UInt16 currentHandle = HCICmds.MaxUInt16;
            dataErr = false;
            int masterLength = totalLength;
            Byte tmpByte = 0x00;

            if (dataLength != 0)
            {
                while ((masterLength > 0) && (dataErr == false) && (masterLength >= (Byte)dataLength))
                {
                    try
                    {
                        // get the handle
                        currentHandle = HCICmds.MaxUInt16;
                        currentHandle = Unload16Bits(data, ref index, ref dataErr);

                        // step thru all the data bytes less the handle length
                        int maxIndex = dataLength - 2;
                        for (int i = 0; (i < maxIndex) && (dataErr == false); i++)
                        {
                            Unload8Bits(data, ref index, ref tmpByte, ref dataErr);

                            if (i != (maxIndex - 1))
                            {
                                dumpStr += String.Format("{0:X2}:", tmpByte);    // this string is seperated into groups of 16
                            }
                            else
                            {
                                dumpStr += String.Format("{0:X2}", tmpByte);    // this string is seperated into groups of 16
                            }
                            CheckLineLength(ref dumpStr, (uint)i, true);  // limit dump string to 16 bytes per line 
                        }
                    }
                    catch
                    {
                        //MessageBox.Show("Unload Handle Value Data\nMessage Data Transfer Issue.\n",
                          //              ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        dataErr = true;
                    }

                    // build the output
                    // handle(s)
                    msgStr += String.Format(" Handle\t\t: 0x{0:X4}\n", currentHandle);
                    // value(s)
                    msgStr += String.Format(" Data\t\t: {0:S}\n", dumpStr);

                    if (dumpStr == "00:81")
                    {

                    }

                    dumpStr = String.Empty;

                    masterLength -= (Byte)dataLength;  // decrement the master length 
                }
            }
            return msgStr;
        }









        // --------------------------------------------------------------------------------
        //  @fn       UnloadHandleValueData()
        //  @brief    Move data bytes into a string of handle value pairs
        //  @param    Byte[] data - the source data
        //            ref int index - the source data index.
        //            int totalLength - the total length of the data
        //            int dataLength - the data length (value)
        //            ref bool dataErr - true = error and false = no error
        //  @returns  string containing the handle value pairs
        // --------------------------------------------------------------------------------
        public string UnloadHandleValueData(Byte[] data,
                                            ref int index,
                                            int totalLength,
                                            int dataLength,
                                            ref bool dataErr)
        {
            string handleStr = String.Empty;
            string valueStr = String.Empty;

            return UnloadHandleValueData(data, ref index, totalLength, dataLength,
                                         ref handleStr, ref valueStr, ref dataErr);
        }

        // --------------------------------------------------------------------------------
        //  @fn       UnloadGATTHandleValueData()
        //  @brief    Move data bytes into a string of handle value pairs
        //  @param    Byte[] data - the source data
        //            ref int index - the source data index.
        //            int totalLength - the total length of the data
        //            int dataLength - the data length (value)
        //            ref string handleStr - a secondary string this one long line of data bytes only
        //            ref string valueStr - a secondary string this one long line of data bytes only
        //            ref bool dataErr - true = error and false = no error
        //  @returns  string containing the handle value pairs
        // --------------------------------------------------------------------------------
        public string UnloadGATTHandleValueData(Byte[] data,
                                                ref int index,
                                                int totalLength,
                                                int dataLength,
                                                ref string handleStr,
                                                ref string valueStr,
                                                ref bool dataErr)
        {
            string msgStr = String.Empty;

            // parse out the handles and the values
            string dumpStr = String.Empty;
            valueStr = String.Empty;
            handleStr = String.Empty;
            UInt16 currentHandle = HCICmds.MaxUInt16;
            dataErr = false;
            int masterLength = totalLength;
            Byte tmpByte = 0x00;

            if (dataLength != 0)
            {
                while ((masterLength > 0) && (dataErr == false) && (masterLength >= (Byte)dataLength))
                {
                    HCICmds.GATTReadByTypeData gData;
                    gData.properties = 0;
                    gData.handle = 0;
                    gData.uuid = 0;

                    try
                    {
                        // get the handle
                        currentHandle = HCICmds.MaxUInt16;
                        currentHandle = Unload16Bits(data, ref index, ref dataErr);

                        // step thru all the data bytes less the handle length
                        int maxIndex = dataLength - 2;

                        // get the gatt data struct
                        int indexSav = index;   // save the index
                        Unload8Bits(data, ref index, ref gData.properties, ref dataErr);
                        Unload16Bits(data, ref index, ref gData.handle, ref dataErr);
                        Unload16Bits(data, ref index, ref gData.uuid, ref dataErr);
                        index = indexSav; // restore the index

                        // process as regular data bytes
                        for (int i = 0; (i < maxIndex) && (dataErr == false); i++)
                        {
                            Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                            valueStr += String.Format("{0:X2} ", tmpByte);   // this string is one long line
                            if (i != (maxIndex - 1))
                            {
                                dumpStr += String.Format("{0:X2}:", tmpByte);    // this string is seperated into groups of 16
                            }
                            else
                            {
                                dumpStr += String.Format("{0:X2}", tmpByte);    // this string is seperated into groups of 16
                            }
                            CheckLineLength(ref dumpStr, (uint)i, true);  // limit dump string to 16 bytes per line 
                        }
                    }
                    catch
                    {
                        //MessageBox.Show("Unload Handle Value Data\nMessage Data Transfer Issue.\n",
                          //              ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        dataErr = true;
                    }

                    // build the output
                    // handle(s)
                    handleStr += String.Format("0x{0:X4} ", gData.handle);   // this string is one long line of handles
                    msgStr += String.Format(" Handle\t\t: 0x{0:X4}\n", currentHandle);
                    // value(s)
                    msgStr += String.Format(" Data\t\t: {0:S}\n", dumpStr);
                    dumpStr = String.Empty;

                    masterLength -= (Byte)dataLength;  // decrement the master length 
                }
            }
            return msgStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       UnloadHandleHandleValueData()
        //  @brief    Move data bytes into a string of handle handle value fields
        //  @param    Byte[] data - the source data
        //            ref int index - the source data index.
        //            int totalLength - the total length of the data
        //            int dataLength - the data length (value)
        //            ref bool dataErr - true = error and false = no error
        //  @returns  string containing the handle handle value fields
        // --------------------------------------------------------------------------------
        public string UnloadHandleHandleValueData(Byte[] data,
                                                  ref int index,
                                                  int totalLength,
                                                  int dataLength,
                                                  ref bool dataErr)
        {
            string msgStr = String.Empty;

            // parse out the handles and the values
            string dumpStr = String.Empty;
            UInt16 attrHandle = HCICmds.MaxUInt16;
            UInt16 endGroupHandle = HCICmds.MaxUInt16;
            dataErr = false;
            int masterLength = totalLength;
            Byte tmpByte = 0x00;

            if (dataLength != 0)
            {
                while ((masterLength > 0) && (dataErr == false) && (masterLength >= (Byte)dataLength))
                {
                    try
                    {
                        // get the first handle
                        attrHandle = HCICmds.MaxUInt16;
                        attrHandle = Unload16Bits(data, ref index, ref dataErr);

                        // get the first handle
                        endGroupHandle = HCICmds.MaxUInt16;
                        endGroupHandle = Unload16Bits(data, ref index, ref dataErr);

                        // step thru all the data bytes less the handle length
                        int maxIndex = dataLength - 4;
                        for (int i = 0; (i < maxIndex) && (dataErr == false); i++)
                        {
                            Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                            if (i != (maxIndex - 1))
                            {
                                dumpStr += String.Format("{0:X2}:", tmpByte);    // this string is seperated into groups of 16
                            }
                            else
                            {
                                dumpStr += String.Format("{0:X2}", tmpByte);    // this string is seperated into groups of 16
                            }
                            CheckLineLength(ref dumpStr, (uint)i, true);  // limit dump string to 16 bytes per line 
                        }
                    }
                    catch
                    {
                        //MessageBox.Show("Unload Handle Value Data\nMessage Data Transfer Issue.\n",
                        //                ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        dataErr = true;
                    }

                    // build the output
                    // handle(s)
                    msgStr += String.Format(" AttrHandle\t: 0x{0:X4}\n", attrHandle);
                    msgStr += String.Format(" EndGrpHandle\t: 0x{0:X4}\n", endGroupHandle);
                    // value(s)
                    msgStr += String.Format(" Value\t\t: {0:S}\n", dumpStr);
                    dumpStr = String.Empty;

                    masterLength -= (Byte)dataLength;  // decrement the master length 
                }
            }
            return msgStr;
        }

        #endregion  // Unloaders
        #region Message Strings
        // --------------------------------------------------------------------------------
        //  @fn       GetGapProfileStr()
        //  @brief    Get the Gap Profile string
        //  @param    Byte gapProfile the Gap Profile
        //  @returns  A string containing the GapProfile
        // --------------------------------------------------------------------------------
        public string GetGapProfileStr(Byte gapProfile)
        {
            string gapProfileStr = String.Empty;
            switch (gapProfile)
            {
                case (Byte)HCICmds.GAP_Profile.Broadcaster:
                    gapProfileStr = "Broadcaster";
                    break;
                case (Byte)HCICmds.GAP_Profile.Observer:
                    gapProfileStr = "Observer";
                    break;
                case (Byte)HCICmds.GAP_Profile.Peripheral:
                    gapProfileStr = "Peripheral";
                    break;
                case (Byte)HCICmds.GAP_Profile.Central:
                    gapProfileStr = "Central";
                    break;
                default:
                    gapProfileStr = "Unknown Gap Profile";
                    break;
            }
            return gapProfileStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapEnableDisableStr()
        //  @brief    Get the Gap Enable Disable string
        //  @param    Byte gapEnableDisable - the gap Enable Disable indicator
        //  @returns  A string containing the gap Enable Disable string
        // --------------------------------------------------------------------------------
        public string GetGapEnableDisableStr(Byte gapEnableDisable)
        {
            string gapEnableDisableStr = String.Empty;
            switch (gapEnableDisable)
            {
                case (Byte)HCICmds.GAP_EnableDisable.Disable:
                    gapEnableDisableStr = "Disable";
                    break;
                case (Byte)HCICmds.GAP_EnableDisable.Enable:
                    gapEnableDisableStr = "Enable";
                    break;
                default:
                    gapEnableDisableStr = "Unknown Gap EnableDisable";
                    break;
            }
            return gapEnableDisableStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapTrueFalseStr()
        //  @brief    Get the Gap True TrueFalseFalse string
        //  @param    Byte gapTrueFalse - the gap True False indicator
        //  @returns  A string containing the gap True False string
        // --------------------------------------------------------------------------------
        public string GetGapTrueFalseStr(Byte gapTrueFalse)
        {
            string gapTrueFalseStr = String.Empty;
            switch (gapTrueFalse)
            {
                case (Byte)HCICmds.GAP_TrueFalse.True:
                    gapTrueFalseStr = "True";
                    break;
                case (Byte)HCICmds.GAP_TrueFalse.False:
                    gapTrueFalseStr = "False";
                    break;
                default:
                    gapTrueFalseStr = "Unknown Gap TrueFalse";
                    break;
            }
            return gapTrueFalseStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapYesNoStr()
        //  @brief    Get the Gap Yes No string
        //  @param    Byte gapYesNo - the gap yes no indicator
        //  @returns  A string containing the gap yes no string
        // --------------------------------------------------------------------------------
        public string GetGapYesNoStr(Byte gapYesNo)
        {
            string gapYesNoStr = String.Empty;
            switch (gapYesNo)
            {
                case (Byte)HCICmds.GAP_YesNo.No:
                    gapYesNoStr = "No";
                    break;
                case (Byte)HCICmds.GAP_YesNo.Yes:
                    gapYesNoStr = "Yes";
                    break;
                default:
                    gapYesNoStr = "Unknown Gap Yes No";
                    break;
            }
            return gapYesNoStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetPacketTypeStr()
        //  @brief    Get the PacketType string
        //  @param    Byte packetType - the Packet Type
        //  @returns  A string containing the PacketType string
        // --------------------------------------------------------------------------------
        public string GetPacketTypeStr(Byte packetType)
        {
            string packetTypeStr = String.Empty;
            switch (packetType)
            {
                case (Byte)HCICmds.PacketType.Command:
                    packetTypeStr = "Command";
                    break;
                case (Byte)HCICmds.PacketType.AsyncData:
                    packetTypeStr = "Async Data";
                    break;
                case (Byte)HCICmds.PacketType.SyncData:
                    packetTypeStr = "Sync Data";
                    break;
                case (Byte)HCICmds.PacketType.Event:
                    packetTypeStr = "Event";
                    break;
                default:
                    packetTypeStr = "Unknown Packet Type";
                    break;
            }
            return packetTypeStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapDiscoveryModeStr()
        //  @brief    Get the Discovery Mode string
        //  @param    Byte discoveryMode the Discovery Mode
        //  @returns  A string containing the Discovery Mode
        // --------------------------------------------------------------------------------
        public string GetGapDiscoveryModeStr(Byte discoveryMode)
        {
            string discoveryModeStr = String.Empty;
            switch (discoveryMode)
            {
                case (Byte)HCICmds.GAP_DiscoveryMode.Nondiscoverable:
                    discoveryModeStr = "Nondiscoverable";
                    break;
                case (Byte)HCICmds.GAP_DiscoveryMode.General:
                    discoveryModeStr = "General";
                    break;
                case (Byte)HCICmds.GAP_DiscoveryMode.Limited:
                    discoveryModeStr = "Limited";
                    break;
                case (Byte)HCICmds.GAP_DiscoveryMode.All:
                    discoveryModeStr = "All";
                    break;
                default:
                    discoveryModeStr = "Unknown Discovery Mode";
                    break;
            }
            return discoveryModeStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapAddrTypeStr()
        //  @brief    Get the addr type string
        //  @param    Byte addrType the address type
        //  @returns  A string containing the address type
        // --------------------------------------------------------------------------------
        public string GetGapAddrTypeStr(Byte addrType)
        {
            string addrTypeStr = String.Empty;
            switch (addrType)
            {
                case (Byte)HCICmds.GAP_AddrType.Public:
                    addrTypeStr = "Public";
                    break;
                case (Byte)HCICmds.GAP_AddrType.Static:
                    addrTypeStr = "Static";
                    break;
                case (Byte)HCICmds.GAP_AddrType.PrivateNonResolve:
                    addrTypeStr = "PrivateNonResolve";
                    break;
                case (Byte)HCICmds.GAP_AddrType.PrivateResolve:
                    addrTypeStr = "PrivateResolve";
                    break;
                default:
                    addrTypeStr = "Unknown Addr Type";
                    break;
            }
            return addrTypeStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapIOCapsStr()
        //  @brief    Get the IO Caps string
        //  @param    Byte ioCaps - the IO Caps to look up
        //  @returns  A string containing the IO Caps string
        // --------------------------------------------------------------------------------
        public string GetGapIOCapsStr(Byte ioCaps)
        {
            string ioCapsStr = String.Empty;
            switch (ioCaps)
            {
                case (Byte)HCICmds.GAP_IOCaps.DisplayOnly:
                    ioCapsStr = "DisplayOnly";
                    break;
                case (Byte)HCICmds.GAP_IOCaps.DisplayYesNo:
                    ioCapsStr = "DisplayYesNo";
                    break;
                case (Byte)HCICmds.GAP_IOCaps.KeyboardOnly:
                    ioCapsStr = "KeyboardOnly";
                    break;
                case (Byte)HCICmds.GAP_IOCaps.NoInputNoOutput:
                    ioCapsStr = "NoInputNoOutput";
                    break;
                case (Byte)HCICmds.GAP_IOCaps.KeyboardDisplay:
                    ioCapsStr = "KeyboardDisplay";
                    break;
                default:
                    ioCapsStr = "Unknown Gap IO Caps";
                    break;
            }
            return ioCapsStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapParamIdStr()
        //  @brief    Get the Param Id string
        //  @param    Byte paramId - the Param Id to look up
        //  @returns  A string containing the Param Id string
        // --------------------------------------------------------------------------------
        public string GetGapParamIdStr(Byte paramId)
        {
            string newLineSpacer = "\n       \t\t  ";
            string paramIdStr = String.Empty;
            switch (paramId)
            {
                case (Byte)HCICmds.GAP_ParamId.TGAP_GEN_DISC_ADV_MIN:
                    paramIdStr = "Minimum time to remain advertising when in , " + newLineSpacer +
                                 "Discoverable mode (mSec). Setting this " + newLineSpacer +
                                 "parameter to 0 turns off the timer " + newLineSpacer +
                                 "(default). TGAP_GEN_DISC_ADV_MIN";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_LIM_ADV_TIMEOUT:
                    paramIdStr = "Maximum time to remain advertising, when in " + newLineSpacer +
                                 "Limited Discoverable mode (mSec). TGAP_LIM_ADV_TIMEOUT";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_GEN_DISC_SCAN:
                    paramIdStr = "Minimum time to perform scanning, when performing " + newLineSpacer +
                                 "General Discovery proc (mSec). TGAP_GEN_DISC_SCAN";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_LIM_DISC_SCAN:
                    paramIdStr = "Minimum time to perform scanning, when performing " + newLineSpacer +
                                 "Limited Discovery proc (mSec). TGAP_LIM_DISC_SCAN";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_EST_ADV_TIMEOUT:
                    paramIdStr = "Advertising timeout, when performing " + newLineSpacer +
                                 "Connection Establishment proc (mSec). " + newLineSpacer +
                                 "TGAP_CONN_EST_ADV_TIMEOUT";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_PARAM_TIMEOUT:
                    paramIdStr = "Link Layer connection parameter update " + newLineSpacer +
                                 "notification timer, connection parameter " + newLineSpacer +
                                 "update proc (mSec). TGAP_CONN_PARAM_TIMEOUT";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_LIM_DISC_ADV_INT_MIN:
                    paramIdStr = "Minimum advertising interval, when in limited " + newLineSpacer +
                                 "discoverable mode (mSec). TGAP_LIM_DISC_ADV_INT_MIN";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_LIM_DISC_ADV_INT_MAX:
                    paramIdStr = "Maximum advertising interval, when in limited " + newLineSpacer +
                                 "discoverable mode (mSec). TGAP_LIM_DISC_ADV_INT_MAX";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_GEN_DISC_ADV_INT_MIN:
                    paramIdStr = "Minimum advertising interval, when in General " + newLineSpacer +
                                 "discoverable mode (mSec). TGAP_GEN_DISC_ADV_INT_MIN";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_GEN_DISC_ADV_INT_MAX:
                    paramIdStr = "Maximum advertising interval, when in General " + newLineSpacer +
                                 "discoverable mode (mSec). TGAP_GEN_DISC_ADV_INT_MAX";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_ADV_INT_MIN:
                    paramIdStr = "Minimum advertising interval, when in Connectable " + newLineSpacer +
                                 "mode (mSec). TGAP_CONN_ADV_INT_MIN";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_ADV_INT_MAX:
                    paramIdStr = "Maximum advertising interval, when in Connectable " + newLineSpacer +
                                 "mode (mSec). TGAP_CONN_ADV_INT_MAX";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_SCAN_INT:
                    paramIdStr = "Scan interval used during Link Layer Initiating " + newLineSpacer +
                                 "state, when in Connectable mode (mSec). TGAP_CONN_SCAN_INT";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_SCAN_WIND:
                    paramIdStr = "Scan window used during Link Layer Initiating " + newLineSpacer +
                                 "state, when in Connectable mode (mSec). " + newLineSpacer +
                                 "TGAP_CONN_SCAN_WIND";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_HIGH_SCAN_INT:
                    paramIdStr = "Scan interval used during Link Layer Initiating " + newLineSpacer +
                                 "state, when in Connectable mode, high duty " + newLineSpacer +
                                 "scan cycle scan paramaters (mSec). TGAP_CONN_HIGH_SCAN_INT";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_HIGH_SCAN_WIND:
                    paramIdStr = "Scan window used during Link Layer Initiating " + newLineSpacer +
                                 "state, when in Connectable mode, high duty " + newLineSpacer +
                                 "scan cycle scan paramaters (mSec). TGAP_CONN_HIGH_SCAN_WIND";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_GEN_DISC_SCAN_INT:
                    paramIdStr = "Scan interval used during Link Layer Scanning " + newLineSpacer +
                                 "state, when in General Discovery " + newLineSpacer +
                                 "proc (mSec). TGAP_GEN_DISC_SCAN_INT";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_GEN_DISC_SCAN_WIND:
                    paramIdStr = "Scan window used during Link Layer Scanning " + newLineSpacer +
                                 "state, when in General Discovery " + newLineSpacer +
                                 "proc (mSec). TGAP_GEN_DISC_SCAN_WIND";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_LIM_DISC_SCAN_INT:
                    paramIdStr = "Scan interval used during Link Layer Scanning " + newLineSpacer +
                                 "state, when in Limited Discovery " + newLineSpacer +
                                 "proc (mSec). TGAP_LIM_DISC_SCAN_INT";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_LIM_DISC_SCAN_WIND:
                    paramIdStr = "Scan window used during Link Layer Scanning " + newLineSpacer +
                                 "state, when in Limited Discovery " + newLineSpacer +
                                 "proc (mSec). TGAP_LIM_DISC_SCAN_WIND";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_EST_ADV:
                    paramIdStr = "Advertising interval, when using Connection " + newLineSpacer +
                                 "Establishment proc (mSec). TGAP_CONN_EST_ADV";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_EST_INT_MIN:
                    paramIdStr = "Minimum Link Layer connection interval, " + newLineSpacer +
                                 "when using Connection Establishment " + newLineSpacer +
                                 "proc (mSec). TGAP_CONN_EST_INT_MIN";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_EST_INT_MAX:
                    paramIdStr = "Maximum Link Layer connection interval, " + newLineSpacer +
                                 "when using Connection Establishment " + newLineSpacer +
                                 "proc (mSec). TGAP_CONN_EST_INT_MAX";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_EST_SCAN_INT:
                    paramIdStr = "Scan interval used during Link Layer Initiating " + newLineSpacer +
                                 "state, when using Connection Establishment " + newLineSpacer +
                                 "proc (mSec). TGAP_CONN_EST_SCAN_INT";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_EST_SCAN_WIND:
                    paramIdStr = "Scan window used during Link Layer Initiating " + newLineSpacer +
                                 "state, when using Connection Establishment " + newLineSpacer +
                                 "proc (mSec). TGAP_CONN_EST_SCAN_WIND";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_EST_SUPERV_TIMEOUT:
                    paramIdStr = "Link Layer connection supervision timeout, " + newLineSpacer +
                                 "when using Connection Establishment " + newLineSpacer +
                                 "proc (mSec). TGAP_CONN_EST_SUPERV_TIMEOUT";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_EST_LATENCY:
                    paramIdStr = "Link Layer connection slave latency, when using " + newLineSpacer +
                                 "Connection Establishment proc (mSec) TGAP_CONN_EST_LATENCY";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_EST_MIN_CE_LEN:
                    paramIdStr = "Local informational parameter about min len " + newLineSpacer +
                                 "of connection needed, when using Connection" + newLineSpacer +
                                 " Establishment proc (mSec). TGAP_CONN_EST_MIN_CE_LEN";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_CONN_EST_MAX_CE_LEN:
                    paramIdStr = "Local informational parameter about max len " + newLineSpacer +
                                 "of connection needed, when using Connection " + newLineSpacer +
                                 "Establishment proc (mSec). TGAP_CONN_EST_MAX_CE_LEN";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_PRIVATE_ADDR_INT:
                    paramIdStr = "Minimum Time Interval between private " + newLineSpacer +
                                 "(resolvable) address changes. In minutes " + newLineSpacer +
                                 "(default 15 minutes) TGAP_PRIVATE_ADDR_INT";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_SM_TIMEOUT:
                    paramIdStr = "SM Message Timeout (milliseconds). " + newLineSpacer +
                                 "(default 30 seconds). TGAP_SM_TIMEOUT";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_SM_MIN_KEY_LEN:
                    paramIdStr = "SM Minimum Key Length supported " + newLineSpacer +
                                 "(default 7). TGAP_SM_MIN_KEY_LEN";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_SM_MAX_KEY_LEN:
                    paramIdStr = "SM Maximum Key Length supported " + newLineSpacer +
                                 "(default 16). TGAP_SM_MAX_KEY_LEN";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_GAP_TESTCODE:
                    paramIdStr = "GAP TestCodes - puts GAP into a " + newLineSpacer +
                                 "test mode TGAP_GAP_TESTCODE";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_SM_TESTCODE:
                    paramIdStr = "SM TestCodes - puts SM into a " + newLineSpacer +
                                 "test mode TGAP_SM_TESTCODE";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_AUTH_TASK_ID:
                    paramIdStr = "Task ID override for Task Authentication " + newLineSpacer +
                                 "control TGAP_AUTH_TASK_ID";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_PARAMID_MAX:
                    paramIdStr = "ID MAX-valid Parameter ID TGAP_PARAMID_MAX";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_GATT_TESTCODE:
                    paramIdStr = "GATT TestCodes - puts GATT into a test " + newLineSpacer +
                                 "mode (paramValue maintained by GATT) " + newLineSpacer +
                                 "TGAP_GATT_TESTCODE";
                    break;
                case (Byte)HCICmds.GAP_ParamId.TGAP_ATT_TESTCODE:
                    paramIdStr = "ATT TestCodes - puts ATT into a test mode " + newLineSpacer +
                                 "(paramValue maintained by ATT) TGAP_ATT_TESTCODE";
                    break;
                case (Byte)HCICmds.GAP_ParamId.SET_RX_DEBUG:
                    paramIdStr = "SET_RX_DEBUG";
                    break;
                case (Byte)HCICmds.GAP_ParamId.GET_MEM_USED:
                    paramIdStr = "GET_MEM_USED";
                    break;
                default:
                    paramIdStr = "Unknown Gap Param Id";
                    break;
            }
            return paramIdStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapTerminationReasonStr()
        //  @brief    Get the termination reason string
        //  @param    Byte termReason - the termination reason to look up
        //  @returns  A string containing the termination reason
        // --------------------------------------------------------------------------------
        public string GetGapTerminationReasonStr(Byte termReason)
        {
            string reasonStr = String.Empty;
            switch (termReason)
            {
                case (Byte)HCICmds.GAP_TerminationReason.GAP_LL_SUPERVISION_TIMEOUT_TERM:
                    reasonStr = "Supervisor Timeout";
                    break;
                case (Byte)HCICmds.GAP_TerminationReason.GAP_LL_PEER_REQUESTED_TERM:
                    reasonStr = "Peer Requested";
                    break;
                case (Byte)HCICmds.GAP_TerminationReason.GAP_LL_HOST_REQUESTED_TERM:
                    reasonStr = "Host Requested";
                    break;
                case (Byte)HCICmds.GAP_TerminationReason.GAP_LL_CONTROL_PKT_TIMEOUT_TERM:
                    reasonStr = "Control Packet Timeout";
                    break;
                case (Byte)HCICmds.GAP_TerminationReason.GAP_LL_CONTROL_PKT_INSTANT_PASSED_TERM:
                    reasonStr = "Control Packet Instant Passed";
                    break;
                case (Byte)HCICmds.GAP_TerminationReason.GAP_LL_LSTO_VIOLATION_TERM:
                    reasonStr = "LSTO Violation";
                    break;
                case (Byte)HCICmds.GAP_TerminationReason.GAP_LL_MIC_FAILURE_TERM:
                    reasonStr = "MIC Failure";
                    break;
                case (Byte)HCICmds.GAP_TerminationReason.GAP_LL_FAILED_TO_ESTABLISH:
                    reasonStr = "Failed To Establish";
                    break;
                default:
                    reasonStr = "Unknown Gap Termination Reason";
                    break;
            }
            return reasonStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapEventTypeStr()
        //  @brief    Get the Gap Event Type string
        //  @param    Byte eventType - the event type to look up
        //  @returns  A string containing the event type string
        // --------------------------------------------------------------------------------
        public string GetGapEventTypeStr(Byte eventType)
        {
            string eventTypeStr = String.Empty;
            switch (eventType)
            {
                case (Byte)HCICmds.GAP_EventType.GAP_EVENT_CONN_UNDIRECT_AD:
                    eventTypeStr = "Connectable undirected advertisement";
                    break;
                case (Byte)HCICmds.GAP_EventType.GAP_EVENT_CONN_DIRECT_AD:
                    eventTypeStr = "Connectable directed advertisement";
                    break;
                case (Byte)HCICmds.GAP_EventType.GAP_EVENT_DISCN_UNDIRECT_AD:
                    eventTypeStr = "Discoverable undirected advertisement";
                    break;
                case (Byte)HCICmds.GAP_EventType.GAP_EVENT_NON_CONN_UNDIRECT_AD:
                    eventTypeStr = "Non-connectable undirected advertisement";
                    break;
                case (Byte)HCICmds.GAP_EventType.GAP_EVENT_SCAN_RESPONSE:
                    eventTypeStr = "Scan Response";
                    break;
                default:
                    eventTypeStr = "Unknown Gap Event Type";
                    break;
            }
            return eventTypeStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetHciExtTxPowerStr()
        //  @brief    Get the Tx Power string
        //  @param    Byte txPower - the Tx Power to look up
        //  @returns  A string containing the Tx Power string
        // --------------------------------------------------------------------------------
        public string GetHciExtTxPowerStr(Byte txPower)
        {
            string txPowerStr = String.Empty;
            switch (txPower)
            {
                case (Byte)HCICmds.HCIExt_TxPower.HCI_EXT_TX_POWER_MINUS_23_DBM:
                    txPowerStr = "HCI_EXT_TX_POWER_MINUS_23_DBM";
                    break;
                case (Byte)HCICmds.HCIExt_TxPower.HCI_EXT_TX_POWER_MINUS_6_DBM:
                    txPowerStr = "HCI_EXT_TX_POWER_MINUS_6_DBM";
                    break;
                case (Byte)HCICmds.HCIExt_TxPower.HCI_EXT_TX_POWER_0_DBM:
                    txPowerStr = "HCI_EXT_TX_POWER_0_DBM";
                    break;
                case (Byte)HCICmds.HCIExt_TxPower.HCI_EXT_TX_POWER_4_DBM:
                    txPowerStr = "HCI_EXT_TX_POWER_4_DBM";
                    break;
                default:
                    txPowerStr = "Unknown Tx Power";
                    break;
            }
            return txPowerStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetHciExtRxGainStr()
        //  @brief    Get the Rx Gain string
        //  @param    Byte rx Gain - the Rx Gain to look up
        //  @returns  A string containing the Rx Gain string
        // --------------------------------------------------------------------------------
        public string GetHciExtRxGainStr(Byte rxGain)
        {
            string rxGainStr = String.Empty;
            switch (rxGain)
            {
                case (Byte)HCICmds.HCIExt_RxGain.HCI_EXT_RX_GAIN_STD:
                    rxGainStr = "HCI_EXT_RX_GAIN_STD";
                    break;
                case (Byte)HCICmds.HCIExt_RxGain.HCI_EXT_RX_GAIN_HIGH:
                    rxGainStr = "HCI_EXT_RX_GAIN_HIGH";
                    break;
                default:
                    rxGainStr = "Unknown Rx Gain";
                    break;
            }
            return rxGainStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetHciExtOnePktPerEvtCtrlStr()
        //  @brief    Get the One Pkt Per Evt Ctrl string
        //  @param    Byte control - the OnePktPerEvtCtrl to look up
        //  @returns  A string containing the OnePktPerEvtCtrl string
        // --------------------------------------------------------------------------------
        public string GetHciExtOnePktPerEvtCtrlStr(Byte control)
        {
            string controlStr = String.Empty;
            switch (control)
            {
                case (Byte)HCICmds.HCIExt_OnePktPerEvtCtrl.HCI_EXT_DISABLE_ONE_PKT_PER_EVT:
                    controlStr = "HCI_EXT_DISABLE_ONE_PKT_PER_EVT";
                    break;
                case (Byte)HCICmds.HCIExt_OnePktPerEvtCtrl.HCI_EXT_ENABLE_ONE_PKT_PER_EVT:
                    controlStr = "HCI_EXT_ENABLE_ONE_PKT_PER_EVT";
                    break;
                default:
                    controlStr = "Unknown One Pkt Per Evt Ctrl";
                    break;
            }
            return controlStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetHciExtClkDivideOnHaltCtrlStr()
        //  @brief    Get the Clk Divide On Halt Ctrl string
        //  @param    Byte control - the ClkDivideOnHaltCtrl to look up
        //  @returns  A string containing the ClkDivideOnHaltCtrl string
        // --------------------------------------------------------------------------------
        public string GetHciExtClkDivideOnHaltCtrlStr(Byte control)
        {
            string controlStr = String.Empty;
            switch (control)
            {
                case (Byte)HCICmds.HCIExt_ClkDivideOnHaltCtrl.HCI_EXT_DISABLE_CLK_DIVIDE_ON_HALT:
                    controlStr = "HCI_EXT_DISABLE_CLK_DIVIDE_ON_HALT";
                    break;
                case (Byte)HCICmds.HCIExt_ClkDivideOnHaltCtrl.HCI_EXT_ENABLE_CLK_DIVIDE_ON_HALT:
                    controlStr = "HCI_EXT_ENABLE_CLK_DIVIDE_ON_HALT";
                    break;
                default:
                    controlStr = "Unknown Clk Divide On Halt Ctrl";
                    break;
            }
            return controlStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetHciExtDelayPostProcCtrlStr()
        //  @brief    Get the Delay Post Proc Ctrl string
        //  @param    Byte control - the DelayPostProcCtrl to look up
        //  @returns  A string containing the DelayPostProcCtrl string
        // --------------------------------------------------------------------------------
        public string GetHciExtDelayPostProcCtrlStr(Byte control)
        {
            string controlStr = String.Empty;
            switch (control)
            {
                case (Byte)HCICmds.HCIExt_DelayPostProcCtrl.HCI_EXT_DISABLE_DELAY_POST_PROC:
                    controlStr = "HCI_EXT_DISABLE_DELAY_POST_PROC";
                    break;
                case (Byte)HCICmds.HCIExt_DelayPostProcCtrl.HCI_EXT_ENABLE_DELAY_POST_PROC:
                    controlStr = "HCI_EXT_ENABLE_DELAY_POST_PROC";
                    break;
                default:
                    controlStr = "Unknown Delay Post Proc Ctrl";
                    break;
            }
            return controlStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetHciExtSetFastTxRespTimeCtrlStr()
        //  @brief    Get the Set Fast Tx Resp Time Ctrl string
        //  @param    Byte control - the SetFastTxRespTimeCtrl to look up
        //  @returns  A string containing the SetFastTxRespTimeCtrl string
        // --------------------------------------------------------------------------------
        public string GetHciExtSetFastTxRespTimeCtrlStr(Byte control)
        {
            string controlStr = String.Empty;
            switch (control)
            {
                case (Byte)HCICmds.HCIExt_SetFastTxRespTimeCtrl.HCI_EXT_DISABLE_FAST_TX_RESP_TIME:
                    controlStr = "HCI_EXT_DISABLE_FAST_TX_RESP_TIME";
                    break;
                case (Byte)HCICmds.HCIExt_SetFastTxRespTimeCtrl.HCI_EXT_ENABLE_FAST_TX_RESP_TIME:
                    controlStr = "HCI_EXT_ENABLE_FAST_TX_RESP_TIME";
                    break;
                default:
                    controlStr = "Unknown Set Fast Tx Resp Time Ctrl";
                    break;
            }
            return controlStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetHciExtCwModeStr()
        //  @brief    Get the Cw Mode string
        //  @param    Byte cwMode - the Cw Mode to look up
        //  @returns  A string containing the Cw Mode string
        // --------------------------------------------------------------------------------
        public string GetHciExtCwModeStr(Byte cwMode)
        {
            string cwModeStr = String.Empty;
            switch (cwMode)
            {
                case (Byte)HCICmds.HCIExt_CwMode.HCI_EXT_TX_MODULATED_CARRIER:
                    cwModeStr = "HCI_EXT_TX_MODULATED_CARRIER";
                    break;
                case (Byte)HCICmds.HCIExt_CwMode.HCI_EXT_TX_UNMODULATED_CARRIER:
                    cwModeStr = "HCI_EXT_TX_UNMODULATED_CARRIER";
                    break;
                default:
                    cwModeStr = "Unknown Cw Mode";
                    break;
            }
            return cwModeStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetAttExecuteWriteFlagsStr()
        //  @brief    Get the ExecuteWriteFlags string
        //  @param    Byte executeWriteFlags - the Execute Write Flags to look up
        //  @returns  A string containing the Execute Write Flags string
        // --------------------------------------------------------------------------------
        public string GetAttExecuteWriteFlagsStr(Byte executeWriteFlags)
        {
            string executeWriteFlagsStr = String.Empty;
            switch (executeWriteFlags)
            {
                case (Byte)HCICmds.ATT_ExecuteWriteFlags.Cancel_all_prepared_writes:
                    executeWriteFlagsStr = "Cancel all prepared writes";
                    break;
                case (Byte)HCICmds.ATT_ExecuteWriteFlags.Immediately_write_all_pending_prepared_values:
                    executeWriteFlagsStr = "Immediately write all pending prepared values";
                    break;
                default:
                    executeWriteFlagsStr = "Unknown Execute Write Flags";
                    break;
            }
            return executeWriteFlagsStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetShortErrorStatusStr()
        //  @brief    Get the short event status name
        //  @param    Byte errorStatus - the error status to look up
        //  @returns  A string containing the short error status
        // --------------------------------------------------------------------------------
        public string GetShortErrorStatusStr(Byte errorStatus)
        {
            string errorStr = String.Empty;
            switch (errorStatus)
            {
                case (Byte)HCICmds.HCI_ErrorRspCodes.INVALID_HANDLE:
                    errorStr = "INVALID_HANDLE";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.READ_NOT_PERMITTED:
                    errorStr = "READ_NOT_PERMITTED";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.WRITE_NOT_PERMITTED:
                    errorStr = "WRITE_NOT_PERMITTED";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INVALID_PDU:
                    errorStr = "INVALID_PDU";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INSUFFICIENT_AUTHEN:
                    errorStr = "INSUFFICIENT_AUTHEN";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.UNSUPPORTED_REQ:
                    errorStr = "UNSUPPORTED_REQ";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INVALID_OFFSET:
                    errorStr = "INVALID_OFFSET";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INSUFFICIENT_AUTHOR:
                    errorStr = "INSUFFICIENT_AUTHOR";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.PREPARE_QUEUE_FULL:
                    errorStr = "PREPARE_QUEUE_FULL";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.ATTR_NOT_FOUND:
                    errorStr = "ATTR_NOT_FOUND";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.ATTR_NOT_LONG:
                    errorStr = "ATTR_NOT_LONG";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INSUFFICIENT_KEY_SIZE:
                    errorStr = "INSUFFICIENT_KEY_SIZE";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INVALID_SIZE:
                    errorStr = "INVALID_SIZE";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.UNLIKELY_ERROR:
                    errorStr = "UNLIKELY_ERROR";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INSUFFICIENT_ENCRYPTION:
                    errorStr = "INSUFFICIENT_ENCRYPTION";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.UNSUPPORTED_GRP_TYPE:
                    errorStr = "UNSUPPORTED_GRP_TYPE";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INSUFFICIENT_RESOURCES:
                    errorStr = "INSUFFICIENT_RESOURCES";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INVALID_VALUE:
                    errorStr = "INVALID_VALUE";
                    break;
                default:
                    errorStr = "Unknown Error Status";
                    break;
            }
            return errorStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetErrorStatusStr()
        //  @brief    Get the error status name
        //  @param    Byte errorStatus - the error status to look up
        //  @returns  A string containing the error status
        // --------------------------------------------------------------------------------
        public string GetErrorStatusStr(Byte errorStatus)
        {
            string errorStr = String.Empty;
            string newLineSpacer = "\n       \t\t  ";
            switch (errorStatus)
            {
                case (Byte)HCICmds.HCI_ErrorRspCodes.INVALID_HANDLE:
                    errorStr = "The attribute handle given was not" + newLineSpacer +
                               "valid on this server.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.READ_NOT_PERMITTED:
                    errorStr = "The attribute cannot be read.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.WRITE_NOT_PERMITTED:
                    errorStr = "The attribute cannot be written.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INVALID_PDU:
                    errorStr = "The attribute PDU was invalid.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INSUFFICIENT_AUTHEN:
                    errorStr = "The attribute requires authentication" + newLineSpacer +
                               "before it can be read or written.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.UNSUPPORTED_REQ:
                    errorStr = "Attribute server does not support the " + newLineSpacer +
                                "request received from the client.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INVALID_OFFSET:
                    errorStr = "Offset specified was past the end of " + newLineSpacer +
                               "the attribute.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INSUFFICIENT_AUTHOR:
                    errorStr = "The attribute requires authorization " + newLineSpacer +
                               "before it can be read or written.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.PREPARE_QUEUE_FULL:
                    errorStr = "Too many prepare writes have been queued.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.ATTR_NOT_FOUND:
                    errorStr = "No attribute found within the given " + newLineSpacer +
                               "attribute handle range";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.ATTR_NOT_LONG:
                    errorStr = "The attribute cannot be read or written " + newLineSpacer +
                               "using the Read Blob Request.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INSUFFICIENT_KEY_SIZE:
                    errorStr = "The Encryption Key Size used for " + newLineSpacer +
                               "encrypting this link is insufficient.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INVALID_SIZE:
                    errorStr = "The attribute value length is invalid " + newLineSpacer +
                               "for the operation.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.UNLIKELY_ERROR:
                    errorStr = "The attribute request that was requested " + newLineSpacer +
                               "has encountered an error that was unlikely, " + newLineSpacer +
                               "and therefore could not be completed as requested.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INSUFFICIENT_ENCRYPTION:
                    errorStr = "The attribute requires encryption before it " + newLineSpacer +
                               "can be read or written.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.UNSUPPORTED_GRP_TYPE:
                    errorStr = "The attribute type is not a supported grouping " + newLineSpacer +
                               "attribute as defined by a higher layer specification.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INSUFFICIENT_RESOURCES:
                    errorStr = "Insufficient Resources to complete the request.";
                    break;
                case (Byte)HCICmds.HCI_ErrorRspCodes.INVALID_VALUE:
                    errorStr = "Invaild Value.";
                    break;
                default:
                    errorStr = "Unknown Error Status";
                    break;
            }
            return errorStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetStatusStr()
        //  @brief    Get the status name
        //  @param    Byte status - the status to look up
        //  @returns  A string containing the status
        // --------------------------------------------------------------------------------
        public string GetStatusStr(Byte status)
        {
            string statusStr = String.Empty;
            switch (status)
            {
                case (Byte)HCICmds.HCI_StatusCodes.Success:
                    statusStr = "Success";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.Failure:
                    statusStr = "Failure";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.InvalidParameter:
                    statusStr = "InvalidParameter";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.InvalidTask:
                    statusStr = "InvalidTask";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.MsgBufferNotAvailable:
                    statusStr = "MsgBufferNotAvailable";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.InvalidMsgPointer:
                    statusStr = "InvalidMsgPointer";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.InvalidEventId:
                    statusStr = "InvalidEventId";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.InvalidInteruptId:
                    statusStr = "InvalidInteruptId";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.NoTimerAvail:
                    statusStr = "NoTimerAvail";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.NVItemUnInit:
                    statusStr = "NVItemUnInit";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.NVOpFailed:
                    statusStr = "NVOpFailed";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.InvalidMemSize:
                    statusStr = "InvalidMemSize";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.ErrorCommandDisallowed:
                    statusStr = "ErrorCommandDisallowed";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleNotReady:
                    statusStr = "Not ready to perform task";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleAlreadyInRequestedMode:
                    statusStr = "Already performing that task";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleIncorrectMode:
                    statusStr = "Not setup properly to perform that task";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleMemAllocError:
                    statusStr = "Memory allocation error occurred";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleNotConnected:
                    statusStr = "Can't perform function when not in a connection";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleNoResources:
                    statusStr = "There are no resource available";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.blePending:
                    statusStr = "Waiting";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleTimeout:
                    statusStr = "Timed out performing function";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleInvalidRange:
                    statusStr = "A parameter is out of range";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleLinkEncrypted:
                    statusStr = "The link is already encrypted";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleProcedureComplete:
                    statusStr = "The Procedure is completed";
                    break;

                // GAP Status Return Values - returned as bStatus_t
                case (Byte)HCICmds.HCI_StatusCodes.bleGAPUserCanceled:
                    statusStr = "The user canceled the task";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleGAPConnNotAcceptable:
                    statusStr = "The connection was not accepted";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleGAPBondRejected:
                    statusStr = "The bound information was rejected.";
                    break;

                // ATT Status Return Values - returned as bStatus_t
                case (Byte)HCICmds.HCI_StatusCodes.bleInvalidPDU:
                    statusStr = "The attribute PDU is invalid";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleInsufficientAuthen:
                    statusStr = "The attribute has insufficient authentication";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleInsufficientEncrypt:
                    statusStr = "The attribute has insufficient encryption";
                    break;
                case (Byte)HCICmds.HCI_StatusCodes.bleInsufficientKeySize:
                    statusStr = "The attribute has insufficient encryption key size";
                    break;

                // L2CAP Status Return Values - returned as bStatus_t
                case (Byte)HCICmds.HCI_StatusCodes.INVALID_TASK_ID:
                    statusStr = "Task ID isn't setup properly";
                    break;

                default:
                    statusStr = "Unknown Status";
                    break;
            }
            return statusStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetHCIExtStatusStr()
        //  @brief    Get the HCI Ext status name
        //  @param    Byte status - the status to look up
        //  @returns  A string containing the status
        // --------------------------------------------------------------------------------
        public string GetHCIExtStatusStr(Byte status)
        {
            string statusStr = String.Empty;
            switch (status)
            {
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_SUCCESS:
                    statusStr = "Success";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_UNKNOWN_HCI_CMD:
                    statusStr = "Unknown HCI Command";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_UNKNOWN_CONN_ID:
                    statusStr = "Unknown Connection Identifier";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_HW_FAILURE:
                    statusStr = "Hardware Failure";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_PAGE_TIMEOUT:
                    statusStr = "Page Timeout";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_AUTH_FAILURE:
                    statusStr = "Authentication Failure";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_PIN_KEY_MISSING:
                    statusStr = "PIN/Key Missing";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_MEM_CAP_EXCEEDED:
                    statusStr = "Memory Capacity Exceeded";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_CONN_TIMEOUT:
                    statusStr = "Connection Timeout";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_CONN_LIMIT_EXCEEDED:
                    statusStr = "Connection Limit Exceeded";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_SYNCH_CONN_LIMIT_EXCEEDED:
                    statusStr = "Synchronous Connection Limit To A Device Exceeded";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_ACL_CONN_ALREADY_EXISTS:
                    statusStr = "ACL Connection Already Exists";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_CMD_DISALLOWED:
                    statusStr = "Command Disallowed";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_CONN_REJ_LIMITED_RESOURCES:
                    statusStr = "Connection Rejected Due To Limited Resources";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_CONN_REJECTED_SECURITY_REASONS:
                    statusStr = "Connection Rejected Due To Security Reasons";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_CONN_REJECTED_UNACCEPTABLE_BDADDR:
                    statusStr = "Connection Rejected Due To Unacceptable BD_ADDR";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_CONN_ACCEPT_TIMEOUT_EXCEEDED:
                    statusStr = "Connection Accept Timeout Exceeded";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_UNSUPPORTED_FEATURE_PARAM_VALUE:
                    statusStr = "Unsupported Feature Or Parameter Value";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_INVALID_HCI_CMD_PARAMS:
                    statusStr = "Invalid HCI Command Parameters";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_REMOTE_USER_TERM_CONN:
                    statusStr = "Remote User Terminated Connection";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_REMOTE_DEVICE_TERM_CONN_LOW_RESOURCES:
                    statusStr = "Remote Device Terminated Connection Due To Low Resources";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_REMOTE_DEVICE_TERM_CONN_POWER_OFF:
                    statusStr = "Remote Device Terminated Connection Due To Power Off";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_CONN_TERM_BY_LOCAL_HOST:
                    statusStr = "Connection Terminated By Local Host";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_REPEATED_ATTEMPTS:
                    statusStr = "Repeated Attempts";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_PAIRING_NOT_ALLOWED:
                    statusStr = "Pairing Not Allowed";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_UNKNOWN_LMP_PDU:
                    statusStr = "Unknown LMP PDU";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_UNSUPPORTED_REMOTE_FEATURE:
                    statusStr = "Unsupported Remote or LMP Feature";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_SCO_OFFSET_REJ:
                    statusStr = "SCO Offset Rejected";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_SCO_INTERVAL_REJ:
                    statusStr = "SCO Interval Rejected";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_SCO_AIR_MODE_REJ:
                    statusStr = "SCO Air Mode Rejected";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_INVALID_LMP_PARAMS:
                    statusStr = "Invalid LMP Parameters";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_UNSPECIFIED_ERROR:
                    statusStr = "Unspecified Error";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_UNSUPPORTED_LMP_PARAM_VAL:
                    statusStr = "Unsupported LMP Parameter Value";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_ROLE_CHANGE_NOT_ALLOWED:
                    statusStr = "Role Change Not Allowed";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_LMP_LL_RESP_TIMEOUT:
                    statusStr = "LMP/LL Response Timeout";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_LMP_ERR_TRANSACTION_COLLISION:
                    statusStr = "LMP Error Transaction Collision";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_LMP_PDU_NOT_ALLOWED:
                    statusStr = "LMP PDU Not Allowed";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_ENCRYPT_MODE_NOT_ACCEPTABLE:
                    statusStr = "Encryption Mode Not Acceptable";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_LINK_KEY_CAN_NOT_BE_CHANGED:
                    statusStr = "Link Key Can Not be Changed";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_REQ_QOS_NOT_SUPPORTED:
                    statusStr = "Requested QoS Not Supported";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_INSTANT_PASSED:
                    statusStr = "Instant Passed";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_PAIRING_WITH_UNIT_KEY_NOT_SUPPORTED:
                    statusStr = "Pairing With Unit Key Not Supported";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_DIFFERENT_TRANSACTION_COLLISION:
                    statusStr = "Different Transaction Collision";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_RESERVED1:
                    statusStr = "Reserved";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_QOS_UNACCEPTABLE_PARAM:
                    statusStr = "QoS Unacceptable Parameter";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_QOS_REJ:
                    statusStr = "QoS Rejected";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_CHAN_ASSESSMENT_NOT_SUPPORTED:
                    statusStr = "Channel Assessment Not Supported";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_INSUFFICIENT_SECURITY:
                    statusStr = "Insufficient Security";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_PARAM_OUT_OF_MANDATORY_RANGE:
                    statusStr = "Parameter Out Of Mandatory Range";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_RESERVED2:
                    statusStr = "Reserved";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_ROLE_SWITCH_PENDING:
                    statusStr = "Role Switch Pending";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_RESERVED3:
                    statusStr = "Reserved";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_RESERVED_SLOT_VIOLATION:
                    statusStr = "Reserved Slot Violation";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_ROLE_SWITCH_FAILED:
                    statusStr = "Role Switch Failed";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_EXTENDED_INQUIRY_RESP_TOO_LARGE:
                    statusStr = "Extended Inquiry Response Too Large";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_SIMPLE_PAIRING_NOT_SUPPORTED_BY_HOST:
                    statusStr = "Simple Pairing Not Supported By Host";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_HOST_BUSY_PAIRING:
                    statusStr = "Host Busy - Pairing";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_CONN_REJ_NO_SUITABLE_CHAN_FOUND:
                    statusStr = "Connection Rejected Due To No Suitable Channel Found";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_CONTROLLER_BUSY:
                    statusStr = "Controller Busy";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_UNACCEPTABLE_CONN_INTERVAL:
                    statusStr = "Unacceptable Connection Interval";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_DIRECTED_ADV_TIMEOUT:
                    statusStr = "Directed Advertising Timeout";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_CONN_TERM_MIC_FAILURE:
                    statusStr = "Connection Terminated Due To MIC Failure";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_CONN_FAILED_TO_ESTABLISH:
                    statusStr = "Connection Failed To Be Established";
                    break;
                case (Byte)HCICmds.HCIExt_StatusCodes.HCI_ERR_MAC_CONN_FAILED:
                    statusStr = "MAC Connection Failed";
                    break;
                default:
                    statusStr = "Unknown HCI EXT Status";
                    break;
            }
            return statusStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetSigAuthStr()
        //  @brief    Get the signature authorizationn string
        //  @param    Byte sigAuth - the sig auth to look up
        //  @returns  A string containing the sig auth string
        // --------------------------------------------------------------------------------
        public string GetSigAuthStr(Byte sigAuth)
        {
            string sigAuthStr = String.Empty;
            switch (sigAuth)
            {
                case 0x00:
                    sigAuthStr = "The Authentication Signature is not included with the Write PDU.";
                    break;
                case 0x01:
                    sigAuthStr = "The included Authentication Signature is valid.";
                    break;
                case 0x02:
                    sigAuthStr = "The included Authentication Signature is not valid.";
                    break;
                default:
                    sigAuthStr = "Unknown Signature Authorization";
                    break;
            }
            return sigAuthStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetFindFormatStr()
        //  @brief    Get the Find Format string
        //  @param    Byte findFormat - the Find Format to look up
        //  @returns  A string containing the Find Format string
        // --------------------------------------------------------------------------------
        public string GetFindFormatStr(Byte findFormat)
        {
            string findFormatStr = String.Empty;
            switch (findFormat)
            {
                case (Byte)HCICmds.ATT_FindInfoFormat.HANDLE_BT_UUID_TYPE__handles_and_16_bit_Bluetooth_UUIDs:
                    findFormatStr = "A list of 1 or more handles with their 16-bit Bluetooth UUIDs";
                    break;
                case (Byte)HCICmds.ATT_FindInfoFormat.HANDLE_UUID_TYPE__handles_and_128_bit_UUIDs:
                    findFormatStr = "A list of 1 or more handles with their 128-bit UUIDs";
                    break;
                default:
                    findFormatStr = "Unknown Find Format";
                    break;
            }
            return findFormatStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapAuthenticatedCsrkStr()
        //  @brief    Get the GAP Authenticated Csrk string
        //  @param    Byte authCsrk - the GAP Authenticated Csrk to look up
        //  @returns  A string containing the GAP Authenticated Csrk string
        // --------------------------------------------------------------------------------
        public string GetGapAuthenticatedCsrkStr(Byte authCsrk)
        {
            string authCsrkStr = String.Empty;
            switch (authCsrk)
            {
                case (Byte)HCICmds.GAP_AuthenticatedCsrk.GAP_CSRK_NOT_AUTHENTICATED:
                    authCsrkStr = "CSRK is not authenticated";
                    break;
                case (Byte)HCICmds.GAP_AuthenticatedCsrk.GAP_CSRK_AUTHENTICATED:
                    authCsrkStr = "CSRK is authenticated";
                    break;
                default:
                    authCsrkStr = "Unknown GAP Authenticated Csrk";
                    break;
            }
            return authCsrkStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapBondParamIdStr()
        //  @brief    Get the GAP Bond Param Id string
        //  @param    UInt16 bondParamId - the GAP Bond Param Id to look up
        //  @returns  A string containing the GAP Bond Param Id string
        // --------------------------------------------------------------------------------
        public string GetGapBondParamIdStr(UInt16 bondParamId)
        {
            string bondParamIdStr = String.Empty;
            switch (bondParamId)
            {
                case (UInt16)HCICmds.GAP_BondParamId.GAPBOND_PAIRING_MODE:
                    bondParamIdStr = "GAPBOND_PAIRING_MODE";
                    break;
                case (UInt16)HCICmds.GAP_BondParamId.GAPBOND_INITIATE_WAIT:
                    bondParamIdStr = "GAPBOND_INITIATE_WAIT";
                    break;
                case (UInt16)HCICmds.GAP_BondParamId.GAPBOND_MITM_PROTECTION:
                    bondParamIdStr = "GAPBOND_MITM_PROTECTION";
                    break;
                case (UInt16)HCICmds.GAP_BondParamId.GAPBOND_IO_CAPABILITIES:
                    bondParamIdStr = "GAPBOND_IO_CAPABILITIES";
                    break;
                case (UInt16)HCICmds.GAP_BondParamId.GAPBOND_OOB_ENABLED:
                    bondParamIdStr = "GAPBOND_OOB_ENABLED";
                    break;
                case (UInt16)HCICmds.GAP_BondParamId.GAPBOND_OOB_DATA:
                    bondParamIdStr = "GAPBOND_OOB_DATA";
                    break;
                case (UInt16)HCICmds.GAP_BondParamId.GAPBOND_BONDING_ENABLED:
                    bondParamIdStr = "GAPBOND_BONDING_ENABLED";
                    break;
                case (UInt16)HCICmds.GAP_BondParamId.GAPBOND_KEY_DIST_LIST:
                    bondParamIdStr = "GAPBOND_KEY_DIST_LIST";
                    break;
                case (UInt16)HCICmds.GAP_BondParamId.GAPBOND_DEFAULT_PASSCODE:
                    bondParamIdStr = "GAPBOND_DEFAULT_PASSCODE";
                    break;
                case (UInt16)HCICmds.GAP_BondParamId.GAPBOND_ERASE_ALLBONDS:
                    bondParamIdStr = "GAPBOND_ERASE_ALLBONDS";
                    break;
                case (UInt16)HCICmds.GAP_BondParamId.GAPBOND_AUTO_FAIL_PAIRING:
                    bondParamIdStr = "GAPBOND_AUTO_FAIL_PAIRING";
                    break;
                case (UInt16)HCICmds.GAP_BondParamId.GAPBOND_AUTO_FAIL_REASON:
                    bondParamIdStr = "GAPBOND_AUTO_FAIL_REASON";
                    break;
                case (UInt16)HCICmds.GAP_BondParamId.GAPBOND_KEYSIZE:
                    bondParamIdStr = "GAPBOND_KEYSIZE";
                    break;
                default:
                    bondParamIdStr = "Unknown Gap Bond Param ID";
                    break;
            }
            return bondParamIdStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapAdventAdTypeStr()
        //  @brief    Get the Gap Advent Ad Type string
        //  @param    Byte adType - the Gap Advent Ad Type to look up
        //  @returns  A string containing the Gap Advent Ad Type string
        // --------------------------------------------------------------------------------
        public string GetGapAdventAdTypeStr(Byte adType)
        {
            string adTypeStr = String.Empty;
            switch (adType)
            {
                case (Byte)HCICmds.GAP_AvertAdType.GAPADVERT_SCAN_RSP_DATA:
                    adTypeStr = "SCAN_RSP data";
                    break;
                case (Byte)HCICmds.GAP_AvertAdType.GAPADVERT_ADVERTISEMENT_DATA:
                    adTypeStr = "Advertisement data";
                    break;
                default:
                    adTypeStr = "Unknown GAP Advent Ad Type";
                    break;
            }
            return adTypeStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapUiInputStr()
        //  @brief    Get the Gap Ui Input string
        //  @param    Byte uiInput - the Gap Ui Input to look up
        //  @returns  A string containing the Gap Ui Input string
        // --------------------------------------------------------------------------------
        public string GetGapUiInputStr(Byte uiInput)
        {
            string uiInputStr = String.Empty;
            switch (uiInput)
            {
                case (Byte)HCICmds.GAP_UiInput.DONT_ASK_TO_INPUT_PASSCODE:
                    uiInputStr = "Don’t ask user to input a passcode";
                    break;
                case (Byte)HCICmds.GAP_UiInput.ASK_TO_INPUT_PASSCODE:
                    uiInputStr = "Ask user to input a passcode";
                    break;
                default:
                    uiInputStr = "Unknown GAP UI Input";
                    break;
            }
            return uiInputStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapUiOutputStr()
        //  @brief    Get the Gap Ui Output string
        //  @param    Byte uiOutput - the Gap Ui Output to look up
        //  @returns  A string containing the Gap Ui Output string
        // --------------------------------------------------------------------------------
        public string GetGapUiOutputStr(Byte uiOutput)
        {
            string uiOutputStr = String.Empty;
            switch (uiOutput)
            {
                case (Byte)HCICmds.GAP_UiOutput.DONT_DISPLAY_PASSCODE:
                    uiOutputStr = "Don’t display passcode";
                    break;
                case (Byte)HCICmds.GAP_UiOutput.DISPLAY_PASSCODE:
                    uiOutputStr = "Display a passcode";
                    break;
                default:
                    uiOutputStr = "Unknown GAP UI Input";
                    break;
            }
            return uiOutputStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetUtilResetTypeStr()
        //  @brief    Get the Util Reset Type string
        //  @param    Byte resetType - the Util Reset Type to look up
        //  @returns  A string containing the Util Reset Type string
        // --------------------------------------------------------------------------------
        public string GetUtilResetTypeStr(Byte resetType)
        {
            string resetTypeStr = String.Empty;
            switch (resetType)
            {
                case (Byte)HCICmds.UTIL_ResetType.Hard_Reset:
                    resetTypeStr = "Hard Reset";
                    break;
                case (Byte)HCICmds.UTIL_ResetType.Soft_Reset:
                    resetTypeStr = "Soft Reset";
                    break;
                default:
                    resetTypeStr = "Unknown Util Reset Type";
                    break;
            }
            return resetTypeStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapChannelMapStr()
        //  @brief    Get the Gap Channel Map string
        //  @param    Byte channelMap - the Gap Channel Map to look up
        //  @returns  A string containing the Gap Channel Map string
        // --------------------------------------------------------------------------------
        public string GetGapChannelMapStr(Byte channelMap)
        {
            string newLineSpacer = "\n       \t\t  ";
            string channelMapStr = String.Empty;
            if (channelMap == 0x00)
            {
                channelMapStr = "Channel Map Bit Mask Is Not Set";
                return channelMapStr;
            }
            Byte tmpByte = 0;
            tmpByte = (Byte)HCICmds.GAP_ChannelMap.Channel_37;
            if ((channelMap & tmpByte) == tmpByte)
            {
                channelMapStr = "Channel 37";
            }
            tmpByte = (Byte)HCICmds.GAP_ChannelMap.Channel_38;
            if ((channelMap & tmpByte) == tmpByte)
            {
                if (!String.IsNullOrEmpty(channelMapStr))
                {
                    channelMapStr += newLineSpacer;
                }
                channelMapStr += "Channel 38";
            }
            tmpByte = (Byte)HCICmds.GAP_ChannelMap.Channel_39;
            if ((channelMap & tmpByte) == tmpByte)
            {
                if (!String.IsNullOrEmpty(channelMapStr))
                {
                    channelMapStr += newLineSpacer;
                }
                channelMapStr += "Channel 39";
            }
            if (String.IsNullOrEmpty(channelMapStr))
            {
                channelMapStr = "Unknown Gap Channel Map";
            }
            return channelMapStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapFilterPolicyStr()
        //  @brief    Get the Gap Filter Policy string
        //  @param    Byte filterPolicy - the Gap Filter Policy to look up
        //  @returns  A string containing the Gap Filter Policy string
        // --------------------------------------------------------------------------------
        public string GetGapFilterPolicyStr(Byte filterPolicy)
        {
            string newLineSpacer = "\n       \t\t  ";
            string filterPolicyStr = String.Empty;
            switch (filterPolicy)
            {
                case (Byte)HCICmds.GAP_FilterPolicy.GAP_FILTER_POLICY_ALL:
                    filterPolicyStr = "Allow scan requests from any, allow " + newLineSpacer +
                                      "connect request from any.";
                    break;
                case (Byte)HCICmds.GAP_FilterPolicy.GAP_FILTER_POLICY_WHITE_CON:
                    filterPolicyStr = "Allow scan requests from any, allow " + newLineSpacer +
                                      "connect request from white list only.";
                    break;
                case (Byte)HCICmds.GAP_FilterPolicy.GAP_FILTER_POLICY_WHITE_SCAN:
                    filterPolicyStr = "Allow scan requests from white list only, " + newLineSpacer +
                                      "allow connect request from any.";
                    break;
                case (Byte)HCICmds.GAP_FilterPolicy.GAP_FILTER_POLICY_WHITE:
                    filterPolicyStr = "Allow scan requests from white list only, " + newLineSpacer +
                                      "allow connect requests from white list only.";
                    break;
                default:
                    filterPolicyStr = "Unknown Gap Filter Policy";
                    break;
            }
            return filterPolicyStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapAuthReqStr()
        //  @brief    Get the Gap Auth Req string
        //  @param    Byte authReq - the Gap Auth Req to look up
        //  @returns  A string containing the Gap Auth Req string
        // --------------------------------------------------------------------------------
        public string GetGapAuthReqStr(Byte authReq)
        {
            string newLineSpacer = "\n       \t\t  ";
            string authReqStr = String.Empty;
            if (authReq == 0x00)
            {
                authReqStr = "Gap Auth Req Bit Mask Is Not Set";
                return authReqStr;
            }
            Byte tmpByte = 0;
            tmpByte = (Byte)HCICmds.GAP_AuthReq.Bonding;
            if ((authReq & tmpByte) == tmpByte)
            {
                authReqStr = "Bonding - exchange and save key information";
            }
            tmpByte = (Byte)HCICmds.GAP_AuthReq.Man_In_The_Middle;
            if ((authReq & tmpByte) == tmpByte)
            {
                if (!String.IsNullOrEmpty(authReqStr))
                {
                    authReqStr += newLineSpacer;
                }
                authReqStr += "Man-In-The-Middle protection";
            }
            if (String.IsNullOrEmpty(authReqStr))
            {
                authReqStr = "Unknown Gap Auth Req";
            }
            return authReqStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapKeyDiskStr()
        //  @brief    Get the Gap Key Disk string
        //  @param    Byte keyDisk - the Gap Key Disk to look up
        //  @returns  A string containing the Gap Key Disk string
        // --------------------------------------------------------------------------------
        public string GetGapKeyDiskStr(Byte keyDisk)
        {
            string newLineSpacer = "\n       \t\t  ";
            string keyDiskStr = String.Empty;
            if (keyDisk == 0x00)
            {
                keyDiskStr = "Gap Key Disk Bit Mask Is Not Set";
                return keyDiskStr;
            }
            Byte tmpByte = 0;
            tmpByte = (Byte)HCICmds.GAP_KeyDisk.Slave_Encryption_Key;
            if ((keyDisk & tmpByte) == tmpByte)
            {
                keyDiskStr = "Slave Encryption Key";
            }
            tmpByte = (Byte)HCICmds.GAP_KeyDisk.Slave_Identification_Key;
            if ((keyDisk & tmpByte) == tmpByte)
            {
                if (!String.IsNullOrEmpty(keyDiskStr))
                {
                    keyDiskStr += newLineSpacer;
                }
                keyDiskStr += "Slave Identification Key";
            }
            tmpByte = (Byte)HCICmds.GAP_KeyDisk.Slave_Signing_Key;
            if ((keyDisk & tmpByte) == tmpByte)
            {
                if (!String.IsNullOrEmpty(keyDiskStr))
                {
                    keyDiskStr += newLineSpacer;
                }
                keyDiskStr += "Slave Signing Key";
            }
            tmpByte = (Byte)HCICmds.GAP_KeyDisk.Master_Encryption_Key;
            if ((keyDisk & tmpByte) == tmpByte)
            {
                if (!String.IsNullOrEmpty(keyDiskStr))
                {
                    keyDiskStr += newLineSpacer;
                }
                keyDiskStr += "Master Encryption Key";
            }
            tmpByte = (Byte)HCICmds.GAP_KeyDisk.Master_Identification_Key;
            if ((keyDisk & tmpByte) == tmpByte)
            {
                if (!String.IsNullOrEmpty(keyDiskStr))
                {
                    keyDiskStr += newLineSpacer;
                }
                keyDiskStr += "Master Identification Key";
            }
            tmpByte = (Byte)HCICmds.GAP_KeyDisk.Master_Signing_Key;
            if ((keyDisk & tmpByte) == tmpByte)
            {
                if (!String.IsNullOrEmpty(keyDiskStr))
                {
                    keyDiskStr += newLineSpacer;
                }
                keyDiskStr += "Master Signing Key";
            }
            if (String.IsNullOrEmpty(keyDiskStr))
            {
                keyDiskStr = "Unknown Gap Key Disk";
            }
            return keyDiskStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetL2CapInfoTypesStr()
        //  @brief    Get the L2CAP Info Types string
        //  @param    UInt16 infoTypes - the L2Cap Info Types to look up
        //  @returns  A string containing the L2 Cap Info Types string
        // --------------------------------------------------------------------------------
        public string GetL2CapInfoTypesStr(UInt16 infoTypes)
        {
            string infoTypesStr = String.Empty;
            switch (infoTypes)
            {
                case (UInt16)HCICmds.L2CAP_InfoTypes.CONNECTIONLESS_MTU:
                    infoTypesStr = "CONNECTIONLESS_MTU";
                    break;
                case (UInt16)HCICmds.L2CAP_InfoTypes.EXTENDED_FEATURES:
                    infoTypesStr = "EXTENDED_FEATURES";
                    break;
                case (UInt16)HCICmds.L2CAP_InfoTypes.FIXED_CHANNELS:
                    infoTypesStr = "FIXED_CHANNELS";
                    break;
                default:
                    infoTypesStr = "Unknown L2Cap Info Types";
                    break;
            }
            return infoTypesStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetL2CapRejectReasonsStr()
        //  @brief    Get the L2CAP Reject Reasons string
        //  @param    UInt16 rejectReason - the L2Cap Reject Reasons to look up
        //  @returns  A string containing the L2 Cap RejectReasons string
        // --------------------------------------------------------------------------------
        public string GetL2CapRejectReasonsStr(UInt16 rejectReason)
        {
            string rejectReasonStr = String.Empty;
            switch (rejectReason)
            {
                case (UInt16)HCICmds.L2CAP_RejectReasons.L2CAP_REJECT_CMD_NOT_UNDERSTOOD:
                    rejectReasonStr = "Command not understood";
                    break;
                case (UInt16)HCICmds.L2CAP_RejectReasons.L2CAP_REJECT_SIGNAL_MTU_EXCEED:
                    rejectReasonStr = "Signaling MTU exceeded ";
                    break;
                case (UInt16)HCICmds.L2CAP_RejectReasons.L2CAP_REJECT_INVALID_CID:
                    rejectReasonStr = "Invalid CID in request";
                    break;
                default:
                    rejectReasonStr = "Unknown L2Cap Reject Reason";
                    break;
            }
            return rejectReasonStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetL2CapConnParamUpdateResultStr()
        //  @brief    Get the L2CAP Conn Param Update Result string
        //  @param    UInt16 updateResult - the L2Cap Conn Param Update Result to look up
        //  @returns  A string containing the L2 Cap Conn Param Update Result string
        // --------------------------------------------------------------------------------
        public string GetL2CapConnParamUpdateResultStr(UInt16 updateResult)
        {
            string updateResultStr = String.Empty;
            switch (updateResult)
            {
                case (UInt16)HCICmds.L2CAP_ConnParamUpdateResult.CONN_PARAMS_ACCEPTED:
                    updateResultStr = "CONN_PARAMS_ACCEPTED";
                    break;
                case (UInt16)HCICmds.L2CAP_ConnParamUpdateResult.CONN_PARAMS_REJECTED:
                    updateResultStr = "CONN_PARAMS_REJECTED";
                    break;
                default:
                    updateResultStr = "Unknown L2Cap Conn Param Update Result";
                    break;
            }
            return updateResultStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGattServiceUUIDStr()
        //  @brief    Get the Gatt Service UUID string
        //  @param    UInt16 serviceUUID - the Gatt Service UUID to look up
        //  @returns  A string containing the Gatt Service UUID string
        // --------------------------------------------------------------------------------
        public string GetGattServiceUUIDStr(UInt16 serviceUUID)
        {
            string serviceUUIDStr = String.Empty;
            switch (serviceUUID)
            {
                case (UInt16)HCICmds.GATT_ServiceUUID.PrimaryService:
                    serviceUUIDStr = "PrimaryService";
                    break;
                case (UInt16)HCICmds.GATT_ServiceUUID.SecondaryService:
                    serviceUUIDStr = "SecondaryService";
                    break;
                default:
                    serviceUUIDStr = "Unknown Gatt Service UUID";
                    break;
            }
            return serviceUUIDStr;
        }


        // --------------------------------------------------------------------------------
        //  @fn       GetGattPermissionsStr()
        //  @brief    Get the Gatt Permissions string
        //  @param    Byte permissions - the Gatt Permissions to look up
        //  @returns  A string containing the Gatt Permissions string
        // --------------------------------------------------------------------------------
        public string GetGattPermissionsStr(Byte permissions)
        {
            string newLineSpacer = "\n       \t\t  ";
            string permissionsStr = String.Empty;
            if (permissions == 0x00)
            {
                permissionsStr = "Gatt Permissions Bit Mask Is Not Set";
                return permissionsStr;
            }
            Byte tmpByte = 0;
            tmpByte = (Byte)HCICmds.GATT_Permissions.GATT_PERMIT_READ;
            if ((permissions & tmpByte) == tmpByte)
            {
                permissionsStr = "GATT_PERMIT_READ";
            }
            tmpByte = (Byte)HCICmds.GATT_Permissions.GATT_PERMIT_WRITE;
            if ((permissions & tmpByte) == tmpByte)
            {
                if (!String.IsNullOrEmpty(permissionsStr))
                {
                    permissionsStr += newLineSpacer;
                }
                permissionsStr += "GATT_PERMIT_WRITE";
            }
            tmpByte = (Byte)HCICmds.GATT_Permissions.GATT_PERMIT_AUTHEN_READ;
            if ((permissions & tmpByte) == tmpByte)
            {
                if (!String.IsNullOrEmpty(permissionsStr))
                {
                    permissionsStr += newLineSpacer;
                }
                permissionsStr += "GATT_PERMIT_AUTHEN_READ";
            }
            tmpByte = (Byte)HCICmds.GATT_Permissions.GATT_PERMIT_AUTHEN_WRITE;
            if ((permissions & tmpByte) == tmpByte)
            {
                if (!String.IsNullOrEmpty(permissionsStr))
                {
                    permissionsStr += newLineSpacer;
                }
                permissionsStr += "GATT_PERMIT_AUTHEN_WRITE";
            }
            tmpByte = (Byte)HCICmds.GATT_Permissions.GATT_PERMIT_AUTHOR_READ;
            if ((permissions & tmpByte) == tmpByte)
            {
                if (!String.IsNullOrEmpty(permissionsStr))
                {
                    permissionsStr += newLineSpacer;
                }
                permissionsStr += "GATT_PERMIT_AUTHOR_READ";
            }
            tmpByte = (Byte)HCICmds.GATT_Permissions.GATT_PERMIT_AUTHOR_WRITE;
            if ((permissions & tmpByte) == tmpByte)
            {
                if (!String.IsNullOrEmpty(permissionsStr))
                {
                    permissionsStr += newLineSpacer;
                }
                permissionsStr += "GATT_PERMIT_AUTHOR_WRITE";
            }
            if (String.IsNullOrEmpty(permissionsStr))
            {
                permissionsStr = "Unknown Gatt Permissions";
            }
            return permissionsStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapSMPFailureTypesStr()
        //  @brief    Get the Gap SMP Failure Types string
        //  @param    Byte failTypes - the Gap SMP Failure Types to look up
        //  @returns  A string containing the Gap SMP Failure Types string
        // --------------------------------------------------------------------------------
        public string GetGapSMPFailureTypesStr(Byte failTypes)
        {
            string failTypesStr = String.Empty;
            switch (failTypes)
            {
                case (Byte)HCICmds.GAP_SMPFailureTypes.SUCCESS:
                    failTypesStr = "SUCCESS";
                    break;
                case (Byte)HCICmds.GAP_SMPFailureTypes.SMP_PAIRING_FAILED_PASSKEY_ENTRY_FAILED:
                    failTypesStr = "SMP_PAIRING_FAILED_PASSKEY_ENTRY_FAILED";
                    break;
                case (Byte)HCICmds.GAP_SMPFailureTypes.SMP_PAIRING_FAILED_OOB_NOT_AVAIL:
                    failTypesStr = "SMP_PAIRING_FAILED_OOB_NOT_AVAIL";
                    break;
                case (Byte)HCICmds.GAP_SMPFailureTypes.SMP_PAIRING_FAILED_AUTH_REQ:
                    failTypesStr = "SMP_PAIRING_FAILED_AUTH_REQ";
                    break;
                case (Byte)HCICmds.GAP_SMPFailureTypes.SMP_PAIRING_FAILED_CONFIRM_VALUE:
                    failTypesStr = "SMP_PAIRING_FAILED_CONFIRM_VALUE";
                    break;
                case (Byte)HCICmds.GAP_SMPFailureTypes.SMP_PAIRING_FAILED_NOT_SUPPORTED:
                    failTypesStr = "SMP_PAIRING_FAILED_NOT_SUPPORTED";
                    break;
                case (Byte)HCICmds.GAP_SMPFailureTypes.SMP_PAIRING_FAILED_ENC_KEY_SIZE:
                    failTypesStr = "SMP_PAIRING_FAILED_ENC_KEY_SIZE";
                    break;
                case (Byte)HCICmds.GAP_SMPFailureTypes.SMP_PAIRING_FAILED_CMD_NOT_SUPPORTED:
                    failTypesStr = "SMP_PAIRING_FAILED_CMD_NOT_SUPPORTED";
                    break;
                case (Byte)HCICmds.GAP_SMPFailureTypes.SMP_PAIRING_FAILED_UNSPECIFIED:
                    failTypesStr = "SMP_PAIRING_FAILED_UNSPECIFIED";
                    break;
                case (Byte)HCICmds.GAP_SMPFailureTypes.SMP_PAIRING_FAILED_REPEATED_ATTEMPTS:
                    failTypesStr = "SMP_PAIRING_FAILED_REPEATED_ATTEMPTS";
                    break;
                case (Byte)HCICmds.GAP_SMPFailureTypes.bleTimeout:
                    failTypesStr = "bleTimeout";
                    break;
                default:
                    failTypesStr = "Unknown Gap SMP Failure Types";
                    break;
            }
            return failTypesStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapOobDataFlagStr()
        //  @brief    Get the Gap Oob Data Flag string
        //  @param    Byte dataFlag - the Gap Oob Data Flag to look up
        //  @returns  A string containing the Gap Oob Data Flag string
        // --------------------------------------------------------------------------------
        public string GetGapOobDataFlagStr(Byte dataFlag)
        {
            string dataFlagStr = String.Empty;
            switch (dataFlag)
            {
                case (Byte)HCICmds.GAP_OobDataFlag.Out_Of_Bounds_Data_Not_Available:
                    dataFlagStr = "Out-Of-Bounds (OOB) data is NOT available";
                    break;
                case (Byte)HCICmds.GAP_OobDataFlag.Out_Of_Bounds_Data_Available:
                    dataFlagStr = "Out-Of-Bounds (OOB) data is available";
                    break;
                default:
                    dataFlagStr = "Unknown Gap Oob Data Flag";
                    break;
            }
            return dataFlagStr;
        }

        // --------------------------------------------------------------------------------
        //  @fn       GetGapAdTypesStr()
        //  @brief    Get the Gap Ad Types string
        //  @param    Byte adTypes - the Gap Ad Types to look up
        //  @returns  A string containing the Gap Ad Types string
        // --------------------------------------------------------------------------------
        public string GetGapAdTypesStr(Byte adTypes)
        {
            string newLineSpacer = "\n       \t\t  ";
            string adTypesStr = String.Empty;
            switch (adTypes)
            {
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_FLAGS:
                    adTypesStr = "Flags: Discovery Mode";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_16BIT_MORE:
                    adTypesStr = "Service: More 16-bit UUIDs available";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_16BIT_COMPLETE:
                    adTypesStr = "Service: Complete list of 16-bit UUIDs";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_32BIT_MORE:
                    adTypesStr = "Service: More 32-bit UUIDs available";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_32BIT_COMPLETE:
                    adTypesStr = "Service: Complete list of 32-bit UUIDs";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_128BIT_MORE:
                    adTypesStr = "Service: More 128-bit UUIDs available";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_128BIT_COMPLETE:
                    adTypesStr = "Service: Complete list of 128-bit UUIDs";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_LOCAL_NAME_SHORT:
                    adTypesStr = "Shortened local name";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_LOCAL_NAME_COMPLETE:
                    adTypesStr = "Complete local name";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_POWER_LEVEL:
                    adTypesStr = "TX Power Level: 0xXX: -127 to +127 dBm";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_OOB_CLASS_OF_DEVICE:
                    adTypesStr = "Simple Pairing OOB Tag: Class of device" + newLineSpacer +
                                 " (3 octets)";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_OOB_SIMPLE_PAIRING_HASHC:
                    adTypesStr = "Simple Pairing OOB Tag: Simple Pairing " + newLineSpacer +
                                 "Hash C (16 octets)";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_OOB_SIMPLE_PAIRING_RANDR:
                    adTypesStr = "Simple Pairing OOB Tag: Simple Pairing " + newLineSpacer +
                                 "Randomizer R (16 octets)";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_SM_TK:
                    adTypesStr = "Security Manager TK Value";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_SM_OOB_FLAG:
                    adTypesStr = "Secutiry Manager OOB Flags";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_SLAVE_CONN_INTERVAL_RANGE:
                    adTypesStr = "Min and Max values of the connection interval " + newLineSpacer +
                                 "(2 octets Min, 2 octets Max) (0xFFFF indicates " + newLineSpacer +
                                 "no conn interval min or max)";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_SIGNED_DATA:
                    adTypesStr = "Signed Data field";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_SERVICES_LIST_16BIT:
                    adTypesStr = "Service Solicitation: list of 16-bit " + newLineSpacer +
                                 "Service UUIDs";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_SERVICES_LIST_128BIT:
                    adTypesStr = "Service Solicitation: list of 128-bit " + newLineSpacer +
                                 "Service UUIDs";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_SERVICE_DATA:
                    adTypesStr = "Service Data";
                    break;
                case (Byte)HCICmds.GAP_AdTypes.GAP_ADTYPE_MANUFACTURER_SPECIFIC:
                    adTypesStr = "Manufacturer Specific Data: first 2 octets " + newLineSpacer +
                                 "contain the Company Identifier Code " + newLineSpacer +
                                 "followed by the additional manufacturer " + newLineSpacer +
                                 "specific data";
                    break;
                default:
                    adTypesStr = "Unknown Gap Ad Types";
                    break;
            }
            return adTypesStr;
        }
        #endregion

        private void DeviceFormUtils_Load(object sender, EventArgs e)
        {

        }
    }
}
