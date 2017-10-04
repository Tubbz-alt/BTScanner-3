using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BTScanner.HCICmds.ATTCmds;

namespace BTScanner
{
    class RxParser
    {
        private devUtils devUtils = new devUtils();
        private void DisplayRxCommand(Byte cmdType, UInt16 opcode, UInt16 eventOpCode,
                                      Byte length, Byte[] data)
        {
            string msg = String.Empty;
            string payloadStr = String.Empty;
            string addrStr = String.Empty;
            Byte[] addr = new Byte[6];
            uint i = 0;

            BinaryReader br = new BinaryReader(new MemoryStream(data));
            if (cmdType == (byte)HCICmds.PacketType.Event)
            {
                msg = String.Format("-Type\t\t: 0x{0:X2} ({1:S})\n-EventCode\t: 0x{2:X2} ({3:S})\n-Data Length\t: 0x{4:X2} ({5:D}) bytes(s)\n",
                                    cmdType, devUtils.GetPacketTypeStr(cmdType),
                                    opcode, devUtils.GetOpCodeName(opcode),
                                    length, length);
            }
            else
            {
                msg = String.Format("-Type\t\t: 0x{0:X2} ({1:S})\n-OpCode\t\t: 0x{2:X4} ({3:S})\n-Data Length\t: 0x{4:X2} ({5:D}) bytes(s)\n",
                                    cmdType, devUtils.GetPacketTypeStr(cmdType),
                                    opcode, devUtils.GetOpCodeName(opcode),
                                    length, length);
            }

            int index = 0;
            Byte tmpByte = 0;
            UInt16 tmpUInt16 = 0;
            //UInt16 handle = 0;
            UInt16 connHandle = 0;
            UInt32 tmpUInt32 = 0;
            string tmpStr = String.Empty;
            bool dataErr = false;
            Byte tmpPduLen = 0;



            switch (opcode)
            {
                #region HCI_COMMAND_COMPLETE_EVENT
                /***********************************************************/
                // HCI_COMMAND_COMPLETE_EVENT
                // 0x000E - HCI_CommandCompleteEvent
                case (UInt16)HCICmds.HCIEvtCode.HCI_CommandCompleteEvent:
                    {
                        devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                        msg += String.Format(" Packets\t\t: 0x{0:X2} ({1:D})\n",
                                             tmpByte, tmpByte);
                        UInt16 cmdOpCode = devUtils.Unload16Bits(data, ref index, ref dataErr);
                        msg += String.Format(" Opcode\t\t: 0x{0:X4} ({1:S})\n",
                                             cmdOpCode, devUtils.GetOpCodeName(cmdOpCode));
                        switch (cmdOpCode)
                        {
                            #region HCIStatus_ReadRSSI
                            case (UInt16)HCICmds.HCICmdOpcode.HCIStatus_ReadRSSI:
                                devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                msg += String.Format(" Status\t\t: 0x{0:X2} ({1:S})\n",
                                                     tmpByte, devUtils.GetStatusStr(tmpByte));
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Handle\t\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                msg += String.Format(" RSSI\t\t: 0x{0:X2} ({1:D})\n",
                                                     tmpByte, tmpByte);
                                break;
                            #endregion
                            default:
                                devUtils.BuildRawDataStr(data, ref msg, data.Length);
                                break;
                        }
                        break;
                    }
                #endregion

                #region HCI_NUM_COMPLETED_PACKETS_EVENT
                /***********************************************************/
                // HCI_NUM_COMPLETED_PACKETS_EVENT
                // 0x0013 - HCI_NumberOfCompletedPacketsEvent
                case (UInt16)HCICmds.HCIEvtCode.HCI_NumberOfCompletedPacketsEvent:
                    {
                        // Assume 1 complete packet for now 
                        if (length == 5)
                        {
                            devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                            msg += String.Format(" NumOfHandles\t: 0x{0:X2} ({1:D})\n",
                                                 tmpByte, tmpByte);
                            tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                            msg += String.Format(" ConnHandle\t: 0x{0:X4} ({1:D})\n",
                                                 tmpUInt16, tmpUInt16);
                            devUtils.Unload16Bits(data, ref index, ref tmpUInt16, ref dataErr);
                            msg += String.Format(" PktsCompleted\t: 0x{0:X4} ({1:D})\n",
                                                 tmpUInt16, tmpUInt16);
                        }
                        // Display raw data for now
                        devUtils.BuildRawDataStr(data, ref msg, data.Length);
                        break;

                    }
                #endregion

                #region HCI_LE_EXT_EVENT
                /***********************************************************/
                // HCI_LE_EXT_EVENT
                // 0x00FF -  HCI_LE_ExtEvent,                    
                case (UInt16)HCICmds.HCIEvtCode.HCI_LE_ExtEvent:
                    {
                        // determine the correct status type
                        string status;
                        // mask and shift to get csg/esg from the event opcode
                        UInt16 esg = (UInt16)((eventOpCode & 0x380));
                        esg = (UInt16)(esg >> 7);
                        Byte tmpEventStatus = devUtils.Unload8Bits(data, ref index, ref dataErr);
                        // if the esg == HCI
                        if (esg == 0)
                        {
                            status = devUtils.GetHCIExtStatusStr(tmpEventStatus);
                        }
                        else
                        {
                            status = devUtils.GetStatusStr(tmpEventStatus);
                        }
                        msg += String.Format(
                                " Event\t\t: 0x{0:X4} ({1:S})\n Status\t\t: 0x{2:X2} ({3:S})\n",
                                eventOpCode,
                                devUtils.GetOpCodeName(eventOpCode),
                                tmpEventStatus,
                                status);

                        /***********************************************************/
                        switch (eventOpCode)
                        {
                            // | Type (1) | 0xFF + Length | Event Opcode (2) | Data |
                            // Start with [1] as [0] is the status
                            #region HCIExt
                            ////////////////////////////////////////////////////////////////
                            // HCI EXT
                            /***********************************************************/
                            //  0x0400 "HCIExt_SetRxGainDone
                            case (UInt16)HCICmds.HCIEvtOpCode.HCIExt_SetRxGainDone:
                            /***********************************************************/
                            // 0x0401 HCIExt_SetTxPowerDone
                            case (UInt16)HCICmds.HCIEvtOpCode.HCIExt_SetTxPowerDone:
                            /***********************************************************/
                            // 0x0402 HCIExt_OnePktPerEvt
                            case (UInt16)HCICmds.HCIEvtOpCode.HCIExt_OnePktPerEvtDone:
                            /***********************************************************/
                            // 0x0403 HCIExt_ClkDivideOnHalt
                            case (UInt16)HCICmds.HCIEvtOpCode.HCIExt_ClkDivideOnHaltDone:
                            /***********************************************************/
                            // 0x0404 HCIExt_DelayPostProc
                            case (UInt16)HCICmds.HCIEvtOpCode.HCIExt_DelayPostProcDone:
                            /***********************************************************/
                            // 0x0405 HCIExt_Decrypt
                            case (UInt16)HCICmds.HCIEvtOpCode.HCIExt_DecryptDone:
                            /***********************************************************/
                            // 0x0406 HCIExt_SetLocalSupportedFeatures
                            case (UInt16)HCICmds.HCIEvtOpCode.HCIExt_SetLocalSupportedFeaturesDone:
                            /***********************************************************/
                            // 0x0407 HCIExt_SetFastTxRespTime
                            case (UInt16)HCICmds.HCIEvtOpCode.HCIExt_SetFastTxRespTimeDone:
                            /***********************************************************/
                            // 0x0408 HCIExt_ModemTestTxDone
                            case (UInt16)HCICmds.HCIEvtOpCode.HCIExt_ModemTestTxDone:
                            /***********************************************************/
                            // 0x0409 HCIExt_ModemHopTestTxDone
                            case (UInt16)HCICmds.HCIEvtOpCode.HCIExt_ModemHopTestTxDone:
                            /***********************************************************/
                            // 0x040A HCIExt_ModemTestRxDone
                            case (UInt16)HCICmds.HCIEvtOpCode.HCIExt_ModemTestRxDone:
                            /***********************************************************/
                            // 0x040B HCIExt_EndModemTestDone
                            case (UInt16)HCICmds.HCIEvtOpCode.HCIExt_EndModemTestDone:
                            /***********************************************************/
                            // 0x040C HCIExt_SetBDADDRDone
                            case (UInt16)HCICmds.HCIEvtOpCode.HCIExt_SetBDADDRCmdDone:
                                devUtils.Unload16Bits(data, ref index, ref tmpUInt16, ref dataErr);
                                msg += String.Format(" Cmd Opcode\t: 0x{0:X4} ({1:S})\n",
                                                     tmpUInt16, devUtils.GetOpCodeName(tmpUInt16));
                                break;
                            #endregion

                            #region L2CAP_CmdReject
                            ////////////////////////////////////////////////////////////////
                            // L2CAP
                            /***********************************************************/
                            // 0x0481 - L2CAP_CmdReject
                            case (UInt16)HCICmds.HCIEvtOpCode.L2CAP_CmdReject:
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Handle\t\t: 0x{0:X4} ({1:D})\n",
                                                      tmpUInt16, tmpUInt16);
                                devUtils.Unload16Bits(data, ref index, ref tmpUInt16, ref dataErr);
                                msg += String.Format(" RejectReason\t: 0x{0:X4} ({1:S})\n",
                                                     tmpUInt16, devUtils.GetL2CapRejectReasonsStr(tmpUInt16));
                                break;
                            #endregion

                            #region L2CAP_InfoRsp
                            /***********************************************************/
                            // 0x048B - L2CAP_InfoRsp
                            case (UInt16)HCICmds.HCIEvtOpCode.L2CAP_InfoRsp:
                                devUtils.Unload16Bits(data, ref index, ref tmpUInt16, ref dataErr);
                                msg += String.Format(" Cmd Opcode\t: 0x{0:X4} ({1:S})\n",
                                                     tmpUInt16, devUtils.GetOpCodeName(tmpUInt16));
                                break;
                            #endregion

                            #region L2CAP_ConnParamUpdateRsp
                            /***********************************************************/
                            // 0x0493 - L2CAP_ConnParamUpdateRsp
                            case (UInt16)HCICmds.HCIEvtOpCode.L2CAP_ConnParamUpdateRsp:
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Handle\t\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                devUtils.Unload16Bits(data, ref index, ref tmpUInt16, ref dataErr);
                                msg += String.Format(" Result\t\t: 0x{0:X4} ({1:S})\n",
                                                     tmpUInt16, devUtils.GetL2CapConnParamUpdateResultStr(tmpUInt16));
                                break;
                            #endregion

                            #region ATT_ErrorRsp
                            ////////////////////////////////////////////////////////////////
                            // ATT
                            /***********************************************************/
                            // 0x0501 ATT_ErrorRsp
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ErrorRsp:
                                //Byte tmpPduLen = 0;
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                Byte tmpReqOpCode = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ReqOpcode\t: 0x{0:X2} ({1:D})\n",
                                                     tmpReqOpCode, tmpReqOpCode);
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Handle\t\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                Byte tmpStatus = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ErrorCode\t: 0x{0:X2} ({1:S})\n",
                                                     tmpStatus, devUtils.GetShortErrorStatusStr(tmpStatus));
                                msg += String.Format("       \t\t: {0:S}\n",
                                                     devUtils.GetErrorStatusStr(tmpStatus));
                                /*if (tcDevice.SelectedIndex == (int)UiTabs.TAB_READ_WRITE)
                                {
                                  if ((tmpReqOpCode == ((UInt16)HCICmds.HCICmdOpcode.ATT_ReadReq & HCICmds.CmdRspReqOCodeMask)) || 
                                      (tmpReqOpCode == ((UInt16)HCICmds.HCICmdOpcode.ATT_ReadByTypeReq & HCICmds.CmdRspReqOCodeMask)))
                                  {
                                    tbReadStatus.Text = String.Format("{0:S}", 
                                                                      devUtils.GetShortErrorStatusStr(tmpStatus));
                                  }

                                  if (tmpReqOpCode == ((UInt16)HCICmds.HCICmdOpcode.ATT_WriteReq & HCICmds.CmdRspReqOCodeMask))
                                  {
                                    tbWriteStatus.Text = String.Format("{0:S}", 
                                                                       devUtils.GetShortErrorStatusStr(tmpStatus));
                                  }
                                }
                                 */

                                //handle_ATT_ErrorRsp(tmpUInt16);

                                break;
                            #endregion

                            #region ATT_ExchangeMTUReq
                            /***********************************************************/
                            // 0x0502 - ATT_ExchangeMTUReq
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ExchangeMTUReq:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ClientRxMTU\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                break;

                            /***********************************************************/
                            // 0x0503 - ATT_ExchangeMTURsp
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ExchangeMTURsp:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ServerRxMTU\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                break;
                            #endregion

                            #region ATT_FindInfoReq
                            /***********************************************************/
                            // 0x0504 - ATT_FindInfoReq
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_FindInfoReq:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" StartHandle\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" EndHandle\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                break;
                            /***********************************************************/
                            // 0x0505 - ATT_FindInfoRsp
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_FindInfoRsp:

                                if ((tmpPduLen = devUtils.UnloadMsgHeaderR(ref data, ref index,
                                                                          ref msg, ref dataErr, ref connHandle)) == 0)
                                {
                                    break; // end of message
                                }
                                Byte tmpFormat = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Format\t\t: 0x{0:X2} ({1:S})\n",
                                                     tmpFormat, devUtils.GetFindFormatStr(tmpFormat));
                                int dataLength = 4;  // 2 header bytes + 2 data
                                if (tmpFormat == 0x02)
                                {
                                    dataLength = 10;   // 2 header bytes + 8 data
                                }
                                int totalLength = length - index;


                                //handle_ATT_FindInfoRsp(index, totalLength, data, connHandle);

                                msg += devUtils.UnloadHandleValueData(data, ref index, totalLength,
                                                                      dataLength, ref dataErr);


                                break;
                            #endregion

                            #region ATT_FindByTypeValueReq
                            /***********************************************************/
                            // 0x0506 - ATT_FindByTypeValueReq
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_FindByTypeValueReq:

                                //HCICmds.ATTCmds.ATT_FindByTypeValueReq FindbyTypeReq = new HCICmds.ATTCmds.ATT_FindByTypeValueReq();

                                // msg Header
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index, ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }

                                //start handle
                                //ATT_FindByTypeValueReq.startHandle = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                //msg += String.Format(" StartHandle\t: 0x{0:X4} ({1:D})\n",
                                //                     ATT_FindByTypeValueReq.startHandle, ATT_FindByTypeValueReq.startHandle);

                                //end Handle
                                //ATT_FindByTypeValueReq.endHandle = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                //msg += String.Format(" EndHandle\t: 0x{0:X4} ({1:D})\n",
                                //                     ATT_FindByTypeValueReq.endHandle, ATT_FindByTypeValueReq.endHandle);

                                //FindbyTypeReq.type
                                //[Description("Type (2 Bytes) - 'XX:XX' The UUID to find")]
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                //ATT_FindByTypeValueReq.type = String.Format("{0:X4}", tmpUInt16);
                                msg += String.Format(" Type\t\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);

                                //Value
                                //[Description("Value (x Bytes) - The attribute value to find")]
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                //ATT_FindByTypeValueReq.value = String.Format("{0:X4}", tmpUInt16); ;
                               // msg += String.Format(" Value\t\t: 0x{0:X4} ({1:D})\n",
                               //                      ATT_FindByTypeValueReq.value, ATT_FindByTypeValueReq.value);

                                /*
                                 * need update
                                msg += String.Format(" Value\t\t: {0:S}\n",
                                            devUtils.UnloadColonData(data, ref index,
                                    // (length + HCICmds.CmdHdrSize) - index,
                                                                                 2, //2 bytes
                                                                                  ref dataErr));

                                 */
                                //handle_ATT_FindByTypeValueReq();


                                break;

                            /***********************************************************/
                            // 0x0507 - ATT_FindByTypeValueRsp
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_FindByTypeValueRsp:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                for (i = 0; (i < tmpPduLen) && (i + 1 < tmpPduLen) && (dataErr == false); i += 2)
                                {
                                    msg += String.Format(" Handle\t\t: {0:X2}:{1:X2}\n",
                                                         devUtils.Unload8Bits(data, ref index, ref dataErr),
                                                         devUtils.Unload8Bits(data, ref index, ref dataErr));
                                }
                                break;
                            #endregion

                            #region ATT_ReadByTypeReq
                            /***********************************************************/
                            // 0x0508 - ATT_ReadByTypeReq
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ReadByTypeReq:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                //ATT_ReadByTypeReq.startHandle = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                //msg += String.Format(" StartHandle\t: 0x{0:X4} ({1:D})\n",
                                //                     ATT_ReadByTypeReq.startHandle, ATT_ReadByTypeReq.startHandle);
                               // ATT_ReadByTypeReq.endHandle = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" EndHandle\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);

                                //ATT_ReadByTypeReq.type = String.Format("{0:X4}", devUtils.Unload16Bits(data, ref index, ref dataErr));

                                //msg += String.Format(" Type\t\t: {0:S}\n", ATT_ReadByTypeReq.type);

                                /* replaced - need update
                                msg += String.Format(" Type\t\t: {0:S}\n",
                                                     devUtils.UnloadColonData(data, ref index,
                                                                              (length - HCICmds.EvtHdrSize - index) + 1,
                                                                              ref dataErr));
                                 */
                                //handle_ATT_ReadByTypeReq();

                                break;

                            /***********************************************************/
                            // 0x0509 - ATT_ReadByTypeRsp
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ReadByTypeRsp:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                dataLength = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Length\t\t: 0x{0:X2} ({1:D})\n",
                                                     dataLength, dataLength);
                                tmpPduLen--;  // account for the length byte  

                                // final safety check
                                if (dataLength == 0)
                                {
                                    break; // end of message
                                }
                                string handleStr = String.Empty;
                                string valueStr = String.Empty;

                                // unload the data differently for gatt
                                /*
                                 if ((tcDevice.SelectedIndex == (int)UiTabs.TAB_READ_WRITE) &&
                                    (cbReadType.SelectedIndex == (int)ReadSubProcedure.DISCOVER_CHARACTERISTIC_BY_UUID))
                                {
                                  msg += devUtils.UnloadGATTHandleValueData(data, ref index, tmpPduLen, dataLength,
                                                                            ref handleStr, ref valueStr, ref dataErr);

                                }
                                else
                                {
                                  msg += devUtils.UnloadHandleValueData(data, ref index, tmpPduLen, dataLength,
                                                                        ref handleStr, ref valueStr, ref dataErr);
                                }*/

                                // Load into the display
                                /*if (tcDevice.SelectedIndex == (int)UiTabs.TAB_READ_WRITE)
                                {
                                  tbReadValue.Tag = valueStr;

                                  if (rbASCIIRead.Checked)
                                  {
                                    tbReadValue.Text = devUtils.HexStr2UserDefinedStr(valueStr, StringType.ASCII);
                                  }
                                  else if (rbDecimalRead.Checked)
                                  {
                                    tbReadValue.Text = devUtils.HexStr2UserDefinedStr(valueStr, StringType.DEC);
                                  }
                                  else
                                  {
                                    tbReadValue.Text = devUtils.HexStr2UserDefinedStr(valueStr, StringType.HEX);
                                  }
                                  // load the handle into the handle box
                                  if (!String.IsNullOrEmpty(handleStr))
                                  {
                                    tbReadAttrHandle.Text = handleStr;
                                  }
                                }
                                 */
                                break;
                            #endregion

                            #region ATT_ReadReq
                            /***********************************************************/
                            // 0x050A - ATT_ReadReq
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ReadReq:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                             //   ATT_ReadReq.handle = devUtils.Unload16Bits(data, ref index, ref dataErr);
                              //  msg += String.Format(" Handle\t\t: 0x{0:X4} ({1:D})\n",
                               //                      ATT_ReadReq.handle, ATT_ReadReq.handle);


                                //handle_ATT_ReadReq();

                                break;
                            /***********************************************************/
                            // 0x050B - ATT_ReadRsp
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ReadRsp:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                payloadStr = String.Empty;
                                for (i = 0; (i < tmpPduLen) && (dataErr == false); i++)
                                {
                                    payloadStr += String.Format("{0:X2} ",
                                                                devUtils.Unload8Bits(data, ref index, ref dataErr));
                                }
                                payloadStr.Trim();
                                msg += String.Format(" Value\t\t: {0:S}\n", payloadStr);
                                /*if (tcDevice.SelectedIndex == (int)UiTabs.TAB_READ_WRITE)
                                {
                                  tbReadValue.Tag = payloadStr;

                                  if (rbASCIIRead.Checked)
                                  {
                                    tbReadValue.Text = devUtils.HexStr2UserDefinedStr(payloadStr, StringType.ASCII);
                                  }
                                  else if (rbDecimalRead.Checked)
                                  {
                                    tbReadValue.Text = devUtils.HexStr2UserDefinedStr(payloadStr, StringType.DEC);
                                  }
                                  else
                                  {
                                    tbReadValue.Text = devUtils.HexStr2UserDefinedStr(payloadStr, StringType.HEX);
                                  }
                                }
                                 */
                                break;
                            #endregion

                            #region ATT_ReadBlobReq
                            /***********************************************************/
                            // 0x050C - ATT_ReadBlobReq
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ReadBlobReq:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Handle\t\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Offset\t\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                break;

                            /***********************************************************/
                            // 0x050D - ATT_ReadBlobRsp
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ReadBlobRsp:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                msg += String.Format(" Value\t\t: {0:S}\n",
                                                     devUtils.UnloadColonData(data, ref index,
                                                                              (length - HCICmds.EvtHdrSize - index) + 1,
                                                                              ref dataErr));
                                break;
                            #endregion

                            #region ATT_ReadMultiReq
                            /***********************************************************/
                            // 0x050E - ATT_ReadMultiReq
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ReadMultiReq:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                for (i = 0; (i < tmpPduLen) && (dataErr == false); i++)
                                {
                                    payloadStr += String.Format("{0:X2} ",
                                                                devUtils.Unload8Bits(data, ref index, ref dataErr));
                                }
                                payloadStr.Trim();
                                msg += String.Format(" Handles\t\t: {0:S}\n", payloadStr);
                                break;

                            /***********************************************************/
                            // 0x050F - ATT_ReadMultiRsp
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ReadMultiRsp:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                for (i = 0; (i < tmpPduLen) && (dataErr == false); i++)
                                {
                                    payloadStr += String.Format("{0:X2} ",
                                                                devUtils.Unload8Bits(data, ref index, ref dataErr));
                                }
                                payloadStr.Trim();
                                msg += String.Format(" Values\t\t: {0:S}\n", payloadStr);

                                /*if (tcDevice.SelectedIndex == (int)UiTabs.TAB_READ_WRITE)
                                {
                                  tbReadValue.Tag = payloadStr;

                                  if (rbASCIIRead.Checked)
                                  {
                                    tbReadValue.Text = devUtils.HexStr2UserDefinedStr(payloadStr, StringType.ASCII);
                                  }
                                  else if (rbDecimalRead.Checked)
                                  {
                                    tbReadValue.Text = devUtils.HexStr2UserDefinedStr(payloadStr, StringType.DEC);
                                  }
                                  else
                                  {
                                    tbReadValue.Text = devUtils.HexStr2UserDefinedStr(payloadStr, StringType.HEX);
                                  }
                                }
                                 * */
                                break;
                            #endregion

                            #region ATT_ReadByGrpTypeReq
                            /***********************************************************/
                            // 0x0510 - ATT_ReadByGrpTypeReq
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ReadByGrpTypeReq:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" StartHandle\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" EndHandle\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                msg += String.Format(" GroupType\t: {0:S}\n",
                                                     devUtils.UnloadColonData(data, ref index,
                                                                              (length - HCICmds.EvtHdrSize - index) + 1,
                                                                              ref dataErr));
                                break;

                            /***********************************************************/
                            // 0x0511 - ATT_ReadByGrpTypeRsp
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ReadByGrpTypeRsp:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                Byte tmpLen = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Length\t\t: 0x{0:X2} ({1:D})\n",
                                                     tmpLen, tmpLen);
                                if (tmpLen == 0)
                                {
                                    break; // end of message
                                }
                                dataLength = tmpLen;  // 2 header bytes + 2 header bytes
                                totalLength = (length - HCICmds.EvtHdrSize - index) + 1;
                                msg += String.Format(" DataList\t:\n{0:S}\n",
                                                     devUtils.UnloadHandleHandleValueData(data,
                                                                                          ref index,
                                                                                          totalLength,
                                                                                          dataLength,
                                                                                          ref dataErr));
                                break;
                            #endregion

                            #region ATT_WriteReq
                            /***********************************************************/
                            // 0x0512 - ATT_WriteReq
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_WriteReq:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                tmpByte = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Signature\t: 0x{0:X2} ({1:S})\n",
                                                     tmpByte, devUtils.GetSigAuthStr(tmpByte));
                                tmpByte = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Command\t: 0x{0:X2} ({1:S})\n",
                                                     tmpByte, devUtils.GetGapYesNoStr(tmpByte));
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Handle\t\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                payloadStr.Trim();
                                msg += String.Format(" Value\t\t: {0:S}\n",
                                                     devUtils.UnloadColonData(data, ref index,
                                                                              (length - HCICmds.EvtHdrSize - index) + 1,
                                                                              ref dataErr));
                                break;

                            /***********************************************************/
                            // 0x0513 - ATT_WriteRsp
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_WriteRsp:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                break;
                            #endregion

                            #region ATT_PrepareWriteReq
                            /***********************************************************/
                            // 0x0516 - ATT_PrepareWriteReq
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_PrepareWriteReq:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Handle\t\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Offset\t\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                msg += String.Format(" Value\t\t: {0:S}\n",
                                                     devUtils.UnloadColonData(data, ref index,
                                                                              (length - HCICmds.EvtHdrSize - index) + 1,
                                                                              ref dataErr));
                                break;

                            /***********************************************************/
                            // 0x0517 - ATT_PrepareWriteRsp
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_PrepareWriteRsp:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Handle\t\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Offset\t\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                msg += String.Format(" Value\t\t: {0:S}\n",
                                                     devUtils.UnloadColonData(data, ref index,
                                                                              (length - HCICmds.EvtHdrSize - index) + 1,
                                                                              ref dataErr));
                                break;
                            #endregion

                            #region ATT_ExecuteWriteReq
                            /***********************************************************/
                            // 0x0518 - ATT_ExecuteWriteReq
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ExecuteWriteReq:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                msg += String.Format(" Flages\t\t: 0x{0:X2}\n",
                                                     devUtils.Unload8Bits(data, ref index, ref dataErr));
                                break;

                            /***********************************************************/
                            // 0x0519 - ATT_ExecuteWriteRsp
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_ExecuteWriteRsp:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                break;
                            #endregion

                            #region ATT_HandleValueNotification
                            /***********************************************************/
                            // 0x051B - ATT_HandleValueNotification
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_HandleValueNotification:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Handle\t\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                msg += String.Format(" Value\t\t: {0:S}\n",
                                                     devUtils.UnloadColonData(data, ref index,
                                                                              (length - HCICmds.EvtHdrSize - index) + 1,
                                                                              ref dataErr));

                                //handleNotification(tmpUInt16, data);

                                break;
                            #endregion

                            #region ATT_HandleValueIndication
                            /***********************************************************/
                            // 0x051D - ATT_HandleValueIndication
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_HandleValueIndication:
                                try
                                {
                                    if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                              ref msg, ref dataErr)) == 0)
                                    {
                                        break; // end of message
                                    }
                                    tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                    msg += String.Format(" Handle\t\t: 0x{0:X4} ({1:D})\n",
                                                         tmpUInt16, tmpUInt16);

                                    msg += String.Format(" Value\t\t: {0:S}\n",
                                                         devUtils.UnloadColonData(data, ref index,
                                                                                  (length - HCICmds.EvtHdrSize - index) + 1,
                                                                                  ref dataErr));
                                }
                                catch
                                {
//                                    MessageBox.Show("Message Data Conversion Issue.\n", ErrorStr, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    dataErr = true;
                                }


                                //handleIndication(handle, data);

                                break;
                            #endregion

                            #region ATT_HandleValueConfirmation
                            /***********************************************************/
                            // 0x051E - ATT_HandleValueConfirmation
                            case (UInt16)HCICmds.HCIEvtOpCode.ATT_HandleValueConfirmation:
                                if ((tmpPduLen = devUtils.UnloadMsgHeader(ref data, ref index,
                                                                          ref msg, ref dataErr)) == 0)
                                {
                                    break; // end of message
                                }
                                break;
                            #endregion

                            #region GAP_DeviceInitDone
                            /***********************************************************/
                            // 0x0600 - GAP_DeviceInitDone
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_DeviceInitDone:
                                // device address - needs to be displayed in reverse
                                tmpStr = devUtils.UnloadDeviceAddr(data, ref addr, ref index,
                                                                   false, ref dataErr);
                                msg += String.Format(" DevAddr\t\t: {0:S}\n", tmpStr);
                                //OnBDAdressNotify(tmpStr); // send address to the main gui for display

                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" DataPktLen\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                tmpByte = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" NumDataPkts\t: 0x{0:X2} ({1:D})\n",
                                                     tmpByte, tmpByte);
                                msg += String.Format(" IRK\t\t: {0:S}\n",
                                                     devUtils.UnloadColonData(data, ref index,
                                                                              HCICmds.GAPCmds.GAP_DeviceInit.irkSize,
                                                                              ref dataErr));
                                msg += String.Format(" CSRK\t\t: {0:S}\n",
                                                     devUtils.UnloadColonData(data, ref index,
                                                                              HCICmds.GAPCmds.GAP_DeviceInit.csrkSize,
                                                                              ref dataErr));

                                break;

                            #endregion

                            #region GAP_DeviceDiscoveryDone
                            ////////////////////////////////////////////////////////////////
                            // GAP
                            /***********************************************************/
                            // 0x0601 - GAP_DeviceDiscoveryDone
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_DeviceDiscoveryDone:
                                Byte tmpNumDevs = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" NumDevs\t: 0x{0:X2} ({1:D})\n",
                                                     tmpNumDevs, tmpNumDevs);
                                if (tmpNumDevs > 0)
                                {
                                    for (i = 0; (i < tmpNumDevs) && (dataErr == false); i++)
                                    {
                                        msg += String.Format(" Device #{0:D}\n", i);
                                        Byte tmpEvtType = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                        msg += String.Format(" EventType\t: 0x{0:X2} ({1:S})\n",
                                                             tmpEvtType, devUtils.GetGapEventTypeStr(tmpEvtType));
                                        devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                        msg += String.Format(" AddrType\t: 0x{0:X2} ({1:S})\n",
                                                             tmpByte, devUtils.GetGapAddrTypeStr(tmpByte));
                                        // device address - needs to be displayed in reverse
                                        msg += String.Format(" Addr\t\t: {0:S}\n",
                                                              devUtils.UnloadDeviceAddr(data, ref addr, ref index,
                                                                                        false, ref dataErr));
                                        // Add the device
                                        //AddSlaveDevice(1, addr);
                                    }
                                }
                                // stop the scan timer and the stop the event timer
                                //ShowProgress(false);
                                //StopTimer(EventType.Scan);

                                break;

                            /***********************************************************/
                            // 0x0602 - GAP_AdvertDataUpdate
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_AdvertDataUpdate:
                                devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                msg += String.Format(" AdType\t\t: 0x{0:X2} ({1:S})\n",
                                                     tmpByte, devUtils.GetGapAdventAdTypeStr(tmpByte));
                                break;
                            /***********************************************************/
                            // 0x0603 - GAP_MakeDiscoverable
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_MakeDiscoverable:
                            /***********************************************************/
                            // 0x0604 - GAP_EndDiscoverable
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_EndDiscoverable:
                                // nothing to do
                                break;
                            #endregion

                            #region GAP_EstablishLink
                            /***********************************************************/
                            // 0x0605 - GAP_EstablishLink
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_EstablishLink:
                                //ShowProgress(false);
                                //StopTimer(EventType.Establish);

                                tmpByte = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" DevAddrType\t: 0x{0:X2} ({1:S})\n",
                                                     tmpByte, devUtils.GetGapAddrTypeStr(tmpByte));
                                // device address - needs to be displayed in reverse
                                tmpStr = devUtils.UnloadDeviceAddr(data, ref addr, ref index,
                                                                   false, ref dataErr);
                                msg += String.Format(" DevAddr\t\t: {0:S}\n", tmpStr);
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ConnHandle\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);

                                if (tmpEventStatus == (byte)HCICmds.HCI_StatusCodes.Success)
                                {

                                    // link to device has been established
                                    /*
                                    connectData.connectBDA = tmpStr;
                                    connectData.connectHandle = tmpUInt16;
                                    sbConnection.Text = "Connected to:" + connectData.connectBDA;

                                    if (connectData.connectBDA != lastHTAddress)
                                    {

                                        lastHTAddress = connectData.connectBDA;

                                        //Send FindINfoReq to get handles
                                        HCICmds.ATTCmds.ATT_FindInfoReq ATT_FindInfoReq = new HCICmds.ATTCmds.ATT_FindInfoReq();
                                        ATT_FindInfoReq.connHandle = tmpUInt16;
                                        ATT_FindInfoReq.startHandle = 1;
                                        ATT_FindInfoReq.endHandle = 200;
                                        //SendATT_FindInfoReq(ATT_FindInfoReq);
                                    }
                                    else
                                    {//if auto enable, start measurement reports.
                                    }*/
                                }
                                
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ConnInterval\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ConnLatency\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ConnTimeout\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                tmpByte = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ClockAccuracy\t: 0x{0:X2} ({1:D})\n",
                                                     tmpByte, tmpByte);
                                break;
                            #endregion

                            #region GAP_TerminateLink
                            /***********************************************************/
                            // 0x0606 - GAP_TerminateLink
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_TerminateLink:
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ConnHandle\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                msg += String.Format(" Reason\t\t: 0x{0:X2} ({1:S})\n",
                                                     tmpByte, devUtils.GetGapTerminationReasonStr(tmpByte));

                                if (tmpEventStatus == (byte)HCICmds.HCI_StatusCodes.Success)
                                {
                                    //greg
                                    // link to device has been terminated
                                    //connectData.connectBDA = HCICmds.EmptyBDAStr;
                                    //connectData.connectHandle = 0;
                                    //OnConnectionNotify(connectData);  // update gui connect data
                                    //SetPairingStatus(PairingStatus.NOT_CONNECTED);
                                    //sbConnection.Text = "Not Connected";
                                    //txtThermometerConnection.ForeColor = Color.Red;
                                    //txtThermometerConnection.Text = "Not Connected";
                                    //txtBPConnected.ForeColor = Color.Red;
                                    //txtBPConnected.Text = "Not Connected";
                                    //timerAutoScan.Enabled = true;


                                }
                                break;
                            #endregion

                            #region GAP_LinkParamUpdate
                            /***********************************************************/
                            // 0x0607 - GAP_LinkParamUpdate
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_LinkParamUpdate:
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ConnHandle\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ConnInterval\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ConnLatency\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ConnTimeout\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                break;
                            #endregion

                            #region GAP_RandomAddressChange
                            /***********************************************************/
                            // 0x0608 - GAP_RandomAddressChange
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_RandomAddressChange:
                                devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                msg += String.Format(" AddrType\t: 0x{0:X2} ({1:S})\n",
                                                     tmpByte, devUtils.GetGapAddrTypeStr(tmpByte));
                                // device address - needs to be displayed in reverse
                                msg += String.Format(" NewRandAddr\t: {0:S}\n",
                                                     devUtils.UnloadDeviceAddr(data, ref addr, ref index,
                                                                               false, ref dataErr));
                                break;
                            #endregion

                            #region GAP_SignatureUpdate
                            /***********************************************************/
                            // 0x0609 - GAP_SignatureUpdate
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_SignatureUpdate:
                                devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                msg += String.Format(" AddrType\t: 0x{0:X2} ({1:S})\n",
                                                     tmpByte, devUtils.GetGapAddrTypeStr(tmpByte));
                                // device address - needs to be displayed in reverse
                                msg += String.Format(" DevAddr\t\t: {0:S}\n",
                                                     devUtils.UnloadDeviceAddr(data, ref addr, ref index,
                                                                               false, ref dataErr));
                                tmpUInt32 = devUtils.Unload32Bits(data, ref index, ref dataErr);
                                msg += String.Format(" SignCounter\t: 0x{0:X8} ({1:D})\n",
                                                     tmpUInt32, tmpUInt32);
                                break;
                            #endregion

                            #region GAP_AuthenticationComplete
                            /***********************************************************/
                            // 0x060A - GAP_AuthenticationComplete
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_AuthenticationComplete:
                                // unload event data into a data structure
                                HCICmds.GAPEvts.GAP_AuthenticationComplete GAP_AuthenticationComplete =
                                                          new HCICmds.GAPEvts.GAP_AuthenticationComplete();

                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ConnHandle\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                GAP_AuthenticationComplete.connHandle = tmpUInt16;

                                tmpByte = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" AuthState\t: 0x{0:X2} ({1:S})\n",
                                                     tmpByte, devUtils.GetGapAuthReqStr(tmpByte));
                                GAP_AuthenticationComplete.authState = tmpByte;

                                tmpByte = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" SecInf.Enable\t: 0x{0:X2} ({1:D})\n",
                                                     tmpByte, tmpByte);
                                GAP_AuthenticationComplete.secInfo_enable = (HCICmds.GAP_EnableDisable)tmpByte;

                                tmpByte = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" SecInf.LTKSize\t: 0x{0:X2} ({1:D})\n",
                                                     tmpByte, tmpByte);
                                GAP_AuthenticationComplete.secInfo_LTKsize = tmpByte;

                                // SecInf.LTK
                                tmpStr = devUtils.UnloadColonData(data, ref index,
                                                                  HCICmds.GAPEvts.GAP_AuthenticationComplete.secInfo_LTKSize,
                                                                  ref dataErr);
                                msg += String.Format(" SecInf.LTK\t: {0:S}\n", tmpStr);
                                GAP_AuthenticationComplete.secInfo_LTK = tmpStr;


                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" SecInf.DIV\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                GAP_AuthenticationComplete.secInfo_DIV = tmpUInt16;

                                tmpStr = devUtils.UnloadColonData(data, ref index,
                                                                  HCICmds.GAPEvts.GAP_AuthenticationComplete.secInfo_RANDSize,
                                                                  ref dataErr);
                                msg += String.Format(" SecInf.Rand\t: {0:S}\n", tmpStr);
                                GAP_AuthenticationComplete.secInfo_RAND = tmpStr;

                                tmpByte = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" DSInf.Enable\t: 0x{0:X2} ({1:D})\n",
                                                     tmpByte, tmpByte);
                                GAP_AuthenticationComplete.devSecInfo_enable = (HCICmds.GAP_EnableDisable)tmpByte;

                                tmpByte = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" DSInf.LTKSize\t: 0x{0:X2} ({1:D})\n",
                                                     tmpByte, tmpByte);
                                GAP_AuthenticationComplete.devSecInfo_LTKsize = tmpByte;

                                // DevSecInf.LTK
                                tmpStr = devUtils.UnloadColonData(data, ref index,
                                                                  HCICmds.GAPEvts.GAP_AuthenticationComplete.devSecInfo_LTKSize,
                                                                  ref dataErr);
                                msg += String.Format(" DSInf.LTK\t: {0:S}\n", tmpStr);
                                GAP_AuthenticationComplete.devSecInfo_LTK = tmpStr;

                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" DSInf.DIV\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                GAP_AuthenticationComplete.devSecInfo_DIV = tmpUInt16;

                                tmpStr = devUtils.UnloadColonData(data, ref index,
                                                                  HCICmds.GAPEvts.GAP_AuthenticationComplete.devSecInfo_RANDSize,
                                                                  ref dataErr);
                                msg += String.Format(" DSInf.Rand\t: {0:S}\n", tmpStr);
                                GAP_AuthenticationComplete.devSecInfo_RAND = tmpStr;

                                tmpByte = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" IdInfo.Enable\t: 0x{0:X2} ({1:D})\n",
                                                     tmpByte, tmpByte);
                                GAP_AuthenticationComplete.idInfo_enable = (HCICmds.GAP_EnableDisable)tmpByte;

                                // IndetityInfo.IRK
                                tmpStr = devUtils.UnloadColonData(data, ref index,
                                                                  HCICmds.GAPEvts.GAP_AuthenticationComplete.idInfo_IRKSize,
                                                                  ref dataErr);
                                msg += String.Format(" IdInfo.IRK\t: {0:S}\n", tmpStr);
                                GAP_AuthenticationComplete.idInfo_IRK = tmpStr;

                                // IndentityInfo.BD_ADDR
                                // device address - needs to be displayed in reverse
                                tmpStr = devUtils.UnloadDeviceAddr(data, ref addr, ref index,
                                                                   false, ref dataErr);
                                msg += String.Format(" IdInfo.BD_Addr\t: {0:S}\n", tmpStr);
                                GAP_AuthenticationComplete.idInfo_BdAddr = tmpStr;

                                tmpByte = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" SignInfo.Enable\t: 0x{0:X2} ({1:D})\n",
                                                     tmpByte, tmpByte);
                                GAP_AuthenticationComplete.signInfo_enable = (HCICmds.GAP_EnableDisable)tmpByte;

                                // SigningInfo.CSRK
                                tmpStr = devUtils.UnloadColonData(data, ref index,
                                                                  HCICmds.GAPEvts.GAP_AuthenticationComplete.signInfo_CSRKSize,
                                                                  ref dataErr);
                                msg += String.Format(" SignInfo.CSRK\t: {0:S}\n", tmpStr);
                                GAP_AuthenticationComplete.signInfo_CSRK = tmpStr;

                                tmpUInt32 = devUtils.Unload32Bits(data, ref index, ref dataErr);
                                msg += String.Format(" SignCounter\t: 0x{0:X8} ({1:D})\n",
                                                     tmpUInt32, tmpUInt32);
                                GAP_AuthenticationComplete.signCounter = tmpUInt32;
                                // end of unload


                               // lastGAP_AuthenticationComplete = GAP_AuthenticationComplete;                               // handle_GAP_AuthenticationComplete();

                                // handle pairing bond tab if selected
                                /*if (tcDevice.SelectedIndex == (int)UiTabs.TAB_PAIRING_BONDING)
                                {
                                  ShowProgress(false);
                                  StopTimer(EventType.PairBond);

                                  if (tmpEventStatus == (Byte)HCICmds.HCI_StatusCodes.bleTimeout)
                                  {  
                                    SetPairingStatus(PairingStatus.NOT_PAIRED);
                                  }
                                  else if (tmpEventStatus == (Byte)HCICmds.HCI_StatusCodes.MsgBufferNotAvailable)
                                  {  
                                    SetPairingStatus(PairingStatus.PASSKEY_INCORRECT);
                                  }                
                                  else if (tmpEventStatus == (Byte)HCICmds.HCI_StatusCodes.Success)                
                                  {
                                    // check bit 0
                                    tmpByte = (Byte)HCICmds.GAP_AuthReq.Bonding;
                                    if ((GAP_AuthenticationComplete.authState & tmpByte) == tmpByte)
                                    {
                                      SetPairingStatus(PairingStatus.DEVICES_PAIRED_BONDED);                  
                                    }
                                    else
                                    {
                                      SetPairingStatus(PairingStatus.DEVICES_PAIRED);                                    
                                    }
                                    // check bit 2
                                    tmpByte = (Byte)HCICmds.GAP_AuthReq.Man_In_The_Middle;                  
                                    if ((GAP_AuthenticationComplete.authState & tmpByte) == tmpByte)
                                    {
                                      SetAuthenticatedBond(true);
                                      SetAuthStatus(AuthStatus.CONNECTION_AUTHENTICATED);                  
                                    }
                                    else
                                    {
                                      SetAuthenticatedBond(false);
                                      SetAuthStatus(AuthStatus.NOT_AUTHENTICATED);                                    
                                    }                
                                    SetGapAuthCompleteInfo(GAP_AuthenticationComplete);
                                  }
                                }
                                 */
                                break;
                            #endregion

                            #region GAP_PasskeyNeeded
                            /***********************************************************/
                            // 0x060B - GAP_PasskeyNeeded
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_PasskeyNeeded:
                                // device address - needs to be displayed in reverse
                                msg += String.Format(" DevAddr\t\t: {0:S}\n",
                                                     devUtils.UnloadDeviceAddr(data, ref addr, ref index,
                                                                               false, ref dataErr));
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ConnHandle\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                msg += String.Format(" UiInput\t\t: 0x{0:X2} ({1:S})\n",
                                                     tmpByte, devUtils.GetGapUiInputStr(tmpByte));

                                // handle pairing bond tab if selected
                                /*
                                if (tcDevice.SelectedIndex == (int)UiTabs.TAB_PAIRING_BONDING)
                                {
                                  if(tmpByte == (Byte)HCICmds.GAP_UiInput.ASK_TO_INPUT_PASSCODE)
                                  {
                                    SetPairingStatus(PairingStatus.PASSKEY_NEEDED);          
                                  }
                                }
                                 * */

                                devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                msg += String.Format(" UiOutput\t\t: 0x{0:X2} ({1:S})\n",
                                                     tmpByte, devUtils.GetGapUiOutputStr(tmpByte));

                                // handle pairing bond tab if selected
                                /*if (tcDevice.SelectedIndex == (int)UiTabs.TAB_PAIRING_BONDING)
                                {
                                  UsePasskeySecurity((HCICmds.GAP_UiOutput)tmpByte);  // passkey display setting
                                  ShowProgress(false);
                                  StopTimer(EventType.PairBond);
                                }*/

                                //handle_GAP_PasskeyNeeded();
                                break;
                            #endregion

                            #region GAP_SlaveRequestedSecurity
                            /***********************************************************/
                            // 0x060C - GAP_SlaveRequestedSecurity
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_SlaveRequestedSecurity:
                                tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" ConnHandle\t: 0x{0:X4} ({1:D})\n",
                                                     tmpUInt16, tmpUInt16);
                                // device address - needs to be displayed in reverse
                                msg += String.Format(" DevAddr\t\t: {0:S}\n",
                                                     devUtils.UnloadDeviceAddr(data, ref addr, ref index,
                                                                               false, ref dataErr));
                                devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                msg += String.Format(" AuthReq\t\t: 0x{0:X2} ({1:D})\n",
                                                     tmpByte, tmpByte);
                                break;
                            #endregion

                            #region GAP_DeviceInformation
                            /***********************************************************/
                            // 0x060D - GAP_DeviceInformation
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_DeviceInformation:


                                Byte tmpEventType = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" EventType\t: 0x{0:X2} ({1:S})\n",
                                                     tmpEventType, devUtils.GetGapEventTypeStr(tmpEventType));
                                devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                msg += String.Format(" AddrType\t: 0x{0:X2} ({1:S})\n",
                                                     tmpByte, devUtils.GetGapAddrTypeStr(tmpByte));
                                // event type - needs to be displayed in reverse
                                msg += String.Format(" Addr\t\t: {0:S}\n",
                                                     devUtils.UnloadDeviceAddr(data, ref addr, ref index,
                                                                               false, ref dataErr));
                                tmpByte = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" Rssi\t\t: 0x{0:X2} ({1:D})\n",
                                                     tmpByte, tmpByte);
                                Byte tmpDataLen = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" DataLength\t: 0x{0:X2} ({1:D})\n",
                                                     tmpDataLen, tmpDataLen);
                                if (tmpDataLen == 0)
                                {
                                    break;  // end of message 
                                }
                                msg += String.Format(" Data\t\t: {0:S}\n",
                                                     devUtils.UnloadColonData(data, ref index,
                                                                              tmpDataLen,
                                                                              ref dataErr));

                                // If GAP device info event add the number
                                if ((tmpEventType == (Byte)HCICmds.GAP_EventType.GAP_EVENT_CONN_UNDIRECT_AD) ||
                                    (tmpEventType == (Byte)HCICmds.GAP_EventType.GAP_EVENT_SCAN_RESPONSE))
                                {
                                    //$$$$ AddSlaveDevice(1, addr);

                                }


                                //handle_GAP_DeviceInformation(addr, msg);


                                break;
                            #endregion

                            #region GAP_BondComplete
                            /***********************************************************/
                            // 0x060E - GAP_BondComplete
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_BondComplete:
                                {
                                    tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                    msg += String.Format(" ConnHandle\t: 0x{0:X4} ({1:D})\n",
                                                         tmpUInt16, tmpUInt16);

                                    // handle pairing bond tab if selected
                                    /*if (tcDevice.SelectedIndex == (int)UiTabs.TAB_PAIRING_BONDING)
                                    {
                                      ShowProgress(false);
                                      StopTimer(EventType.PairBond);

                                      if (tmpEventStatus == (Byte)HCICmds.HCI_StatusCodes.Success)
                                      {  
                                        SetPairingStatus(PairingStatus.DEVICES_PAIRED_BONDED);                  

                                        if (GetAuthenticationEnabled() == true)
                                        {
                                          SetAuthStatus(AuthStatus.CONNECTION_AUTHENTICATED);                  
                                        }
                                        else
                                        {
                                          SetAuthStatus(AuthStatus.NOT_AUTHENTICATED);                                    
                                        }                
                                      }
                                      else
                                      {  
                                        SetPairingStatus(PairingStatus.NOT_PAIRED);                  
                                        SetAuthStatus(AuthStatus.EMPTY);                                    
                                      }  
                                      PairBondUserInputControl();
                                    }*/
                                    break;
                                }
                            #endregion

                            #region GAP_PairingRequested
                            /***********************************************************/
                            // 0x060F - GAP_PairingRequested
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_PairingRequested:
                                {
                                    tmpUInt16 = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                    msg += String.Format(" ConnHandle\t: 0x{0:X4} ({1:D})\n",
                                                         tmpUInt16, tmpUInt16);
                                    devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                    msg += String.Format(" IOCap\t\t: 0x{0:X2} ({1:S})\n",
                                                         tmpByte, devUtils.GetGapIOCapsStr(tmpByte));
                                    devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                    msg += String.Format(" OobDataFlag\t: 0x{0:X2} ({1:S})\n",
                                                         tmpByte, devUtils.GetGapOobDataFlagStr(tmpByte));
                                    devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                    msg += String.Format(" AuthReq\t\t: 0x{0:X2} ({1:S})\n",
                                                         tmpByte, devUtils.GetGapAuthReqStr(tmpByte));
                                    tmpByte = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                    msg += String.Format(" MaxEncKeySiz\t: 0x{0:X4} ({1:D})\n",
                                                         tmpByte, tmpByte);
                                    devUtils.Unload8Bits(data, ref index, ref tmpByte, ref dataErr);
                                    msg += String.Format(" KeyDist\t\t: 0x{0:X2} ({1:S})\n",
                                                         tmpByte, devUtils.GetGapKeyDiskStr(tmpByte));
                                    break;
                                }
                            #endregion

                            #region GAP_HCI_ExtentionCommandStatus

                            ////////////////////////////////////////////////////////////////
                            // GAP HCI
                            /***********************************************************/
                            // 0x067F - GAP_HCI_ExtentionCommandStatus
                            case (UInt16)HCICmds.HCIEvtOpCode.GAP_HCI_ExtentionCommandStatus:
                                UInt16 cmdOpCode = devUtils.Unload16Bits(data, ref index, ref dataErr);
                                msg += String.Format(" OpCode\t\t: 0x{0:X4} ({1:S})\n",
                                                      cmdOpCode, devUtils.GetOpCodeName(cmdOpCode));
                                tmpDataLen = devUtils.Unload8Bits(data, ref index, ref dataErr);
                                msg += String.Format(" DataLength\t: 0x{0:X2} ({1:D})\n",
                                                     tmpDataLen, tmpDataLen);

                                switch (cmdOpCode)
                                {
                                    #region L2CAP_InfoReq,ATT_ErrorRsp,ATT_FindInfoReq
                                    /***********************************************************/
                                    // 0xFC8A - L2CAP_InfoReq
                                    case (UInt16)HCICmds.HCICmdOpcode.L2CAP_InfoReq:
                                    /***********************************************************/
                                    // 0xFC92 - L2CAP_ConnParamUpdateReq
                                    case (UInt16)HCICmds.HCICmdOpcode.L2CAP_ConnParamUpdateReq:

                                    /***********************************************************/
                                    // 0xFD01 - ATT_ErrorRsp
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ErrorRsp:
                                    /***********************************************************/
                                    // 0xFD02 - ATT_ExchangeMTUReq
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ExchangeMTUReq:
                                    /***********************************************************/
                                    // 0xFD03 - ATT_ExchangeMTURsp
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ExchangeMTURsp:
                                    /***********************************************************/
                                    // 0xFD04 - ATT_FindInfoReq
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_FindInfoReq:
                                    /***********************************************************/
                                    // 0xFD05 - ATT_FindInfoRsp
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_FindInfoRsp:
                                    /***********************************************************/
                                    // 0xFD06 - ATT_FindByTypeValueReq
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_FindByTypeValueReq:
                                    /***********************************************************/
                                    // 0xFD07 - ATT_FindByTypeValueRsp
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_FindByTypeValueRsp:
                                        // nothing to do
                                        break;
                                    #endregion

                                    #region ATT_ReadByTypeReq

                                    /***********************************************************/
                                    // 0xFD08 - ATT_ReadByTypeReq
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ReadByTypeReq:
                                        /*
                                        if (tbReadStatus.Text == "Reading...")
                                        {
                                            tbReadStatus.Text = String.Format("{0:S}",
                                                                              devUtils.GetStatusStr(tmpEventStatus));
                                        }*/
                                        break;

                                    /***********************************************************/
                                    // 0xFD09 - ATT_ReadByTypeRsp
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ReadByTypeRsp:
                                        // nothing to do
                                        break;
                                    #endregion

                                    #region ATT_ReadReq
                                    /***********************************************************/
                                    // 0xFD0A - ATT_ReadReq
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ReadReq:
                                        /*
                                        if (tbReadStatus.Text == "Reading...")
                                        {
                                            tbReadStatus.Text = String.Format("{0:S}",
                                                                              devUtils.GetStatusStr(tmpEventStatus));
                                        }*/
                                        break;

                                    /***********************************************************/
                                    // 0xFD0B - ATT_ReadRsp
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ReadRsp:
                                        break;
                                    #endregion

                                    #region ATT_ReadBlobReq
                                    /***********************************************************/
                                    // 0xFD0C - ATT_ReadBlobReq
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ReadBlobReq:
                                    /***********************************************************/
                                    // 0xFD0D - ATT_ReadBlobRsp
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ReadBlobRsp:
                                        // nothing to do
                                        break;
                                    #endregion

                                    #region ATT_ReadMultiReq
                                    /***********************************************************/
                                    // 0xFD0E - ATT_ReadMultiReq
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ReadMultiReq:
                                        break;

                                    /***********************************************************/
                                    // 0xFD0E - ATT_ReadMultiRsp
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ReadMultiRsp:
                                    #endregion

                                    #region ATT_ReadByGrpTypeReq
                                    /***********************************************************/
                                    // 0xFD10 - ATT_ReadByGrpTypeReq
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ReadByGrpTypeReq:
                                    /***********************************************************/
                                    // 0xFD11 - ATT_ReadByGrpTypeRsp
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ReadByGrpTypeRsp:
                                        // nothing to do
                                        break;
                                    #endregion

                                    #region ATT_WriteReq
                                    /***********************************************************/
                                    // 0xFD12 - ATT_WriteReq
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_WriteReq:

                                        /*if (tbWriteStatus.Text == "Writing...")
                                        {
                                          tbWriteStatus.Text = String.Format("{0:S}", 
                                                                             devUtils.GetStatusStr(tmpEventStatus));
                                        }*/
                                        break;

                                    /***********************************************************/
                                    // 0xFD13 - ATT_WriteRsp
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_WriteRsp:
                                    #endregion

                                    #region ATT_PrepareWriteReq, ATT_ExecuteWriteReq
                                    /***********************************************************/
                                    // 0xFD16 - ATT_PrepareWriteReq
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_PrepareWriteReq:
                                    /***********************************************************/
                                    // 0xFD17 - ATT_PrepareWriteRsp
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_PrepareWriteRsp:
                                    /***********************************************************/
                                    // 0xFD18 - ATT_ExecuteWriteReq
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ExecuteWriteReq:
                                    /***********************************************************/
                                    // 0xFD19 - ATT_ExecuteWriteRsp
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_ExecuteWriteRsp:
                                    #endregion

                                    #region ATT_HandleValueNotification
                                    /***********************************************************/
                                    // 0xFD1B - ATT_HandleValueNotification
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_HandleValueNotification:
                                    /***********************************************************/
                                    // 0xFD1D - ATT_HandleValueIndication
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_HandleValueIndication:
                                    /***********************************************************/
                                    // 0xFD1e - ATT_HandleValueConfirmation
                                    case (UInt16)HCICmds.HCICmdOpcode.ATT_HandleValueConfirmation:
                                        // nothing to do
                                        break;
                                    #endregion

                                    #region GATT_DiscCharsByUUID
                                    /***********************************************************/
                                    // 0xFD88 - GATT_DiscCharsByUUID
                                    case (UInt16)HCICmds.HCICmdOpcode.GATT_DiscCharsByUUID:
                                        break;
                                    #endregion

                                    #region GATT_WriteLong,GATT_AddService,GAP_ConfigDeviceAddr
                                    /***********************************************************/
                                    // 0xFD96 - GATT_WriteLong
                                    case (UInt16)HCICmds.HCICmdOpcode.GATT_WriteLong:
                                    /***********************************************************/
                                    // 0xFDFC - GATT_AddService
                                    case (UInt16)HCICmds.HCICmdOpcode.GATT_AddService:
                                    /***********************************************************/
                                    // 0xFDFD - GATT_DelService
                                    case (UInt16)HCICmds.HCICmdOpcode.GATT_DelService:
                                    /***********************************************************/
                                    // 0xFDFE - GATT_AddAttribute
                                    case (UInt16)HCICmds.HCICmdOpcode.GATT_AddAttribute:
                                    #endregion

                                    #region GAP_DeviceInit,GAP_ConfigDeviceAddr
                                    /***********************************************************/
                                    // 0xFE00 - GAP_DeviceInit
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_DeviceInit:
                                    /***********************************************************/
                                    // 0xFE03 - GAP_ConfigDeviceAddr
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_ConfigDeviceAddr:
                                    /***********************************************************/
                                    // 0xFE04 - GAP_DeviceDiscoveryRequest
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_DeviceDiscoveryRequest:
                                    /***********************************************************/
                                    // 0xFE05 - GAP_DeviceDiscoveryCancel
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_DeviceDiscoveryCancel:
                                    /***********************************************************/
                                    // 0xFE06 - GAP_MakeDiscoverable
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_MakeDiscoverable:
                                    /***********************************************************/
                                    // 0xFE07 - GAP_UpdateAdvertisingData
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_UpdateAdvertisingData:
                                    /***********************************************************/
                                    // 0xFE08 - GAP_EndDiscoverable
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_EndDiscoverable:
                                        // nothing to do
                                        break;

                                    /***********************************************************/
                                    // 0xFE09 - GAP_EstablishLinkRequest
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_EstablishLinkRequest:
                                        //greg ShowProgress(false);
                                        //StopTimer(EventType.Establish);
                                        break;

                                    /***********************************************************/
                                    // 0xFE0A - GAP_TerminateLinkRequest
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_TerminateLinkRequest:
                                    /***********************************************************/
                                    // 0xFE0B - GAP_Authenticate
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_Authenticate:
                                    /***********************************************************/
                                    // 0xFE0C - GAP_PasskeyUpdate
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_PasskeyUpdate:
                                    /***********************************************************/
                                    // 0xFE0D - GAP_SlaveSecurityRequest
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_SlaveSecurityRequest:
                                    /***********************************************************/
                                    // 0xFE0F - GAP_Signable
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_Signable:
                                    /***********************************************************/
                                    // 0xFE0F - GAP_Bond
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_Bond:
                                    /***********************************************************/
                                    // 0xFE10 - GAP_TerminateAuth
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_TerminateAuth:
                                    /***********************************************************/
                                    // 0xFE30 - GAP_SetParam
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_SetParam:
                                        // nothing to do
                                        break;
                                    #endregion

                                    #region GAP_GetParam
                                    /***********************************************************/
                                    // 0xFE31 - GAP_GetParam
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_GetParam:
                                        if (tmpDataLen == 0)
                                        {
                                            break;  // end of message 
                                        }
                                        devUtils.Unload16Bits(data, ref index, ref tmpUInt16, ref dataErr);
                                        msg += String.Format(" ParamValue\t: 0x{0:X4} ({1:D})\n",
                                                               tmpUInt16, tmpUInt16);


                                        break;

                                    /***********************************************************/
                                    // 0xFE32 - GAP_ResolvePrivateAddr
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_ResolvePrivateAddr:
                                    /***********************************************************/
                                    // 0xFE33 - GAP_SetAdvToken   
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_SetAdvToken:
                                    /***********************************************************/
                                    // 0xFE34 - GAP_RemoveAdvToken
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_RemoveAdvToken:
                                    /***********************************************************/
                                    // 0xFE35 - GAP_UpdateAdvTokens
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_UpdateAdvTokens:
                                    /***********************************************************/
                                    // 0xFE36 - GAP_BondSetParam
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_BondSetParam:
                                    /***********************************************************/
                                    // 0xFE3 - GAP_BondGetParam
                                    case (UInt16)HCICmds.HCICmdOpcode.GAP_BondGetParam:
                                        // nothing to do
                                        break;
                                    #endregion

                                    #region UTIL
                                    /***********************************************************/
                                    // 0xF80 - UTIL_Reset
                                    case (UInt16)HCICmds.HCICmdOpcode.UTIL_Reset:
                                        // nothing to do
                                        break;
                                    /***********************************************************/
                                    // 0xFE81 - UTIL_NVRead
                                    case (UInt16)HCICmds.HCICmdOpcode.UTIL_NVRead:
                                        if (tmpDataLen == 0)
                                        {
                                            break;  // end of message 
                                        }
                                        msg += String.Format(" nvData\t\t: {0:S}\n",
                                                             devUtils.UnloadColonData(data, ref index,
                                                                                      tmpDataLen,
                                                                                      ref dataErr));
                                        break;
                                    /***********************************************************/
                                    // 0xFE82 - UTIL_NVWrite
                                    case (UInt16)HCICmds.HCICmdOpcode.UTIL_NVWrite:
                                        // nothing to do
                                        break;
                                    #endregion
                                    /***********************************************************/
                                    default:
                                        devUtils.BuildRawDataStr(data, ref msg, data.Length);
                                        break;
                                }
                                #endregion

                                break;

                            /***********************************************************/
                            default:
                                devUtils.BuildRawDataStr(data, ref msg, data.Length);
                                break;
                        }
                        break;
                    }
                /***********************************************************/
                default:
                    {
                        devUtils.BuildRawDataStr(data, ref msg, data.Length);
                        break;
                    }
            }
            #endregion

            if (dataErr == true)
            {
                //DisplayMessage(MessageType.Error,"Could Not Convert All The Data In The Following Message.\n");
            }
            //DisplayMessage(MessageType.Incoming, msg);

        }//Invoke not required

    }
}
