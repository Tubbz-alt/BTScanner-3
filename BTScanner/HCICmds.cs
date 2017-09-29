/*******************************************************************************
Filename:       HCICmds.cs
Author:         $Author: hphan $
Revised:        $Date: 2008-01-08 11:33:37 -0800 (Tue, 08 Jan 2008) $
Revision:       $Revision: 16162 $

Copyright (c) 2005-2010 by Texas Instruments Incorporated, All Rights Reserved.
Permission to use, reproduce, copy, prepare derivative works,
modify, distribute, perform, display or sell this software and/or
its documentation for any purpose is prohibited without the express
written consent of Texas Instruments Incorporated.
********************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;


namespace tiota
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    class Serialize : Attribute
    {
        public int number { get; set; }
    }

    public class HCISerializer
    {
        public Byte dataLength = 0;  // fixed length data only
        public UInt16 opCodeValue = 0;

        public byte[] GetBuffer()
        {

            SortedList<int, object> list = new SortedList<int, object>();
            byte[] buff = new byte[256];
            MemoryStream ms = new MemoryStream(buff);
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((byte)1);
            bw.Write(opCodeValue);
            bw.Write((byte)0); // Len

            Type ClassType = this.GetType();
            foreach (PropertyInfo prop in ClassType.GetProperties())
            {
                // for every property loop through all attributes
                foreach (Attribute att in prop.GetCustomAttributes(false))
                {
                    if (att.GetType() == typeof(Serialize))
                    {
                        Serialize serialize = (Serialize)att;
                        Object val = null;
                        switch (prop.MemberType)
                        {
                            case MemberTypes.Property:
                                val = prop.GetValue(this, null);
                                break;

                            case MemberTypes.Method:
                                val = prop.GetValue(this, null);
                                break;

                            case MemberTypes.Field:
                                val = prop.GetValue(this, null);
                                break;

                        }
                        list.Add(serialize.number, val);
                    }

                }
            }

            foreach (KeyValuePair<int, object> pair in list)
            {
                object val = pair.Value;
                Type t = val.GetType();
                if (t == typeof(Byte))
                {
                    bw.Write((byte)val);
                }
                else if (t == typeof(UInt16))
                {
                    bw.Write((UInt16)val);
                }

                else if (t == typeof(UInt32))
                {
                    bw.Write((UInt32)val);
                }

                else if (t == typeof(byte[]))
                {
                    bw.Write((byte[])val);
                }
                else
                {
                    bw.Write((byte)((int)val));
                }
            }

            long i = ms.Position;
            bw.Close();
            buff[3] = (byte)(i - 4);

            byte[] fix_buff = new byte[i];
            Array.Copy(buff, fix_buff, i);
            return fix_buff;
        }
    }

    public class HCICmds
    {
        #region Constants
        public string[,] OpCodeLookupTable = new string[,]
    { 
      ///////////
      // Events
      ///////////
      { "0x0001", "HCI_InquiryCompleteEvent"},
      { "0x0002", "HCI_InquiryResultEvent"},
      { "0x0003", "HCI_ConnectionCompleteEvent"},
      { "0x0004", "HCI_ConnectionRequestEvent"},
      { "0x0005", "HCI_DisconnectionCompleteEvent"},
      { "0x0006", "HCI_AuthenticationCompleteEvent"},
      { "0x0007", "HCI_RemoteNameRequestCompleteEvent"},
      { "0x0008", "HCI_EncryptionChangeEvent"},
      { "0x0009", "HCI_ChangeConnectionLinkKeyCompleteEvent"},
      { "0x000A", "HCI_MasterLinkKeyCompleteEvent"},
      { "0x000B", "HCI_ReadRemoteSupportedFeaturesCompleteEvent"},
      { "0x000C", "HCI_ReadRemoteVersionInformationCompleteEvent"},
      { "0x000D", "HCI_QoSSetupCompleteEvent"},
      { "0x000E", "HCI_CommandCompleteEvent"},
      { "0x000F", "HCI_CommandStatusEvent"},
      { "0x0010", "HCI_HardwareErrorEvent"},
      { "0x0011", "HCI_FlushOccurredEvent"},
      { "0x0012", "HCI_RoleChangeEvent"},
      { "0x0013", "HCI_NumberOfCompletedPacketsEvent"},
      { "0x0014", "HCI_ModeChangeEvent"},
      { "0x0015", "HCI_ReturnLinkKeysEvent"},
      { "0x0016", "HCI_PINCodeRequestEvent"},
      { "0x0017", "HCI_LinkKeyRequestEvent"},
      { "0x0018", "HCI_LinkKeyNotificationEvent"},
      { "0x0019", "HCI_LoopbackCommandEvent"},
      { "0x001A", "HCI_DataBufferOverflowEvent"},
      { "0x001B", "HCI_MaxSlotsChangeEvent"},
      { "0x001C", "HCI_ReadClockOffsetCompleteEvent"},
      { "0x001D", "HCI_ConnectionPacketTypeChangedEvent"},
      { "0x001E", "HCI_QoSViolationEvent"},
      { "0x001F", "HCI_PageScanModeChangeEvent"},
      { "0x0020", "HCI_PageScanRepetitionModeChangeEvent"},
      { "0x0021", "HCI_FlowSpecificationCompleteEvent"},
      { "0x0022", "HCI_InquiryResultWithRSSIEvent"},
      { "0x0023", "HCI_ReadRemoteExtendedFeaturesCompleteEvent"},
      { "0x002C", "HCI_SynchronousConnectionCompleteEvent"},
      { "0x002D", "HCI_SynchronousConnectionChangedEvent"},
      { "0x002E", "HCI_SniffSubratingEvent"},
      { "0x002F", "HCI_ExtendedInquiryResultEvent"},
      { "0x0030", "HCI_EncryptionKeyRefreshCompleteEvent"},
      { "0x0031", "HCI_IOCapabilityRequestEvent"},
      { "0x0032", "HCI_IOCapabilityResponseEvent"},
      { "0x0033", "HCI_UserConfirmationRequestEvent"},
      { "0x0034", "HCI_UserPasskeyRequestEvent"},
      { "0x0035", "HCI_RemoteOOBDataRequestEvent"},
      { "0x0036", "HCI_SimplePairingCompleteEvent"},
      { "0x0037", "HCI_RemoteOobResponseEvent"},
      { "0x0038", "HCI_LinkSupervisionTimeoutChangedEvent"},
      { "0x0039", "HCI_EnhancedFlushCompleteEvent"},
      { "0x003A", "HCI_SniffRequestEvent"},
      { "0x003B", "HCI_UserPasskeyNotificationEvent"},
      { "0x003C", "HCI_KeypressNotificationEvent"},
      { "0x003D", "HCI_RemoteHostSupportedFeaturesNotificationEvent"},
      { "0x0040", "HCI_PhysicalLinkCompleteEvent"},
      { "0x0041", "HCI_ChannelSelectedEvent"},
      { "0x0042", "HCI_DisconnectionPhysicalLinkCompleteEvent"},
      { "0x0043", "HCI_PhysicalLinkLossEarlyWarningEvent"},
      { "0x0044", "HCI_PhysicalLinkRecoveryEvent"},
      { "0x0045", "HCI_LogicalLinkCompleteEvent"},
      { "0x0046", "HCI_DisconnectionLogicalLinkCompleteEvent"},
      { "0x0047", "HCI_FlowSpecModifyCompleteEvent"},
      { "0x0048", "HCI_NumberOfCompletedDataBlocksEvent"},
      { "0x004C", "HCI_ShortRangeModeChangeCompleteEvent"},
      { "0x004D", "HCI_AMP_StatusChangeEvent"},
      { "0x0049", "HCI_AMP_StartTestEvent"},
      { "0x004A", "HCI_AMP_TestEndEvent"},
      { "0x004B", "HCI_AMP_ReceiverReportEvent"},
      { "0x003E", "HCI_LE_ConnectionCompleteEvent"},            // Subevent Code 0x01
      { "0x003E", "HCI_LE_AdvertisingReportEvent"},             // Subevent Code 0x02 
      { "0x003E", "HCI_LE_ConnectionUpdateCompleteEvent"},      // Subevent Code 0x03  
      { "0x003E", "HCI_LE_ReadRemoteUsedFeaturesCompleteEvent"},// Subevent Code 0x04     
      { "0x003E", "HCI_LE_LongTermKeyRequestEvent"},            // Subevent Code 0x05     
      { "0x00FF", "HCI_LE_ExtEvent"},                    

      /////////////////////
      // Extended Events 
      /////////////////////
      { "0x0400", "HCIExt_SetRxGainDone" },
      { "0x0401", "HCIExt_SetTxPowerDone" },
      { "0x0402", "HCIExt_OnePktPerEvtDone" },
      { "0x0403", "HCIExt_ClkDivideOnHaltDone" },
      { "0x0404", "HCIExt_DelayPostProcDone" },
      { "0x0405", "HCIExt_DecryptDone" },
      { "0x0406", "HCIExt_SetLocalSupportedFeaturesDone" },
      { "0x0407", "HCIExt_SetFastTxRespTimeDone" },
      { "0x0408", "HCIExt_ModemTestTxDone" },
      { "0x0409", "HCIExt_ModemHopTestTxDone" },
      { "0x040A", "HCIExt_ModemTestRxDone" },
      { "0x040B", "HCIExt_EndModemTestDone" },
      { "0x040C", "HCIExt_SetBDADDRCmdDone" },

      /////////////////////
      // L2CAP Events 
      /////////////////////
      { "0x0481", "L2CAP_CmdReject" },
      { "0x048B", "L2CAP_InfoRsp" },
      { "0x0493", "L2CAP_ConnParamUpdateRsp" }, 

      /////////////////////////////////
      // HCI Vendor Specific Events
      ////////////////////////////////
      { "0x0501", "ATT_ErrorRsp" },
      { "0x0502", "ATT_ExchangeMTUReq" },
      { "0x0503", "ATT_ExchangeMTURsp" },
      { "0x0504", "ATT_FindInfoReq" },
      { "0x0505", "ATT_FindInfoRsp" },
      { "0x0506", "ATT_FindByTypeValueReq" },
      { "0x0507", "ATT_FindByTypeValueRsp" },
      { "0x0508", "ATT_ReadByTypeReq" },
      { "0x0509", "ATT_ReadByTypeRsp" },
      { "0x050A", "ATT_ReadReq" },
      { "0x050B", "ATT_ReadRsp" },
      { "0x050C", "ATT_ReadBlobReq" },
      { "0x050D", "ATT_ReadBlobRsp" },
      { "0x050E", "ATT_ReadMultiReq" },
      { "0x050F", "ATT_ReadMultiRsp" },
      { "0x0510", "ATT_ReadByGrpTypeReq" },
      { "0x0511", "ATT_ReadByGrpTypeRsp" },
      { "0x0512", "ATT_WriteReq" },
      { "0x0513", "ATT_WriteRsp" },
      { "0x0516", "ATT_PrepareWriteReq" },
      { "0x0517", "ATT_PrepareWriteRsp" },
      { "0x0518", "ATT_ExecuteWriteReq" },
      { "0x0519", "ATT_ExecuteWriteRsp" },
      { "0x051B", "ATT_HandleValueNotification" },
      { "0x051D", "ATT_HandleValueIndication" },
      { "0x051E", "ATT_HandleValueConfirmation" },

      { "0x0600", "GAP_DeviceInitDone" },
      { "0x0601", "GAP_DeviceDiscoveryDone" },
      { "0x0602", "GAP_AdvertDataUpdate" },
      { "0x0603", "GAP_MakeDiscoverable" },
      { "0x0604", "GAP_EndDiscoverable" },
      { "0x0605", "GAP_EstablishLink" },
      { "0x0606", "GAP_TerminateLink" },
      { "0x0607", "GAP_LinkParamUpdate" },
      { "0x0608", "GAP_RandomAddressChange" },
      { "0x0609", "GAP_SignatureUpdate" },
      { "0x060A", "GAP_AuthenticationComplete" },
      { "0x060B", "GAP_PasskeyNeeded" },
      { "0x060C", "GAP_SlaveRequestedSecurity" },
      { "0x060D", "GAP_DeviceInformation" },
      { "0x060E", "GAP_BondComplete" },
      { "0x060F", "GAP_PairingRequested" },
      { "0x067F", "GAP_HCI_ExtentionCommandStatus" },

      /////////////////////////////////
      // HCI Extended Commands
      ////////////////////////////////
      { "0xFC00", "HCIExt_SetRxGain" },
      { "0xFC01", "HCIExt_SetTxPower" },
      { "0xFC02", "HCIExt_OnePktPerEvt" },
      { "0xFC03", "HCIExt_ClkDivideOnHalt" },
      { "0xFC04", "HCIExt_DelayPostProc" },
      { "0xFC05", "HCIExt_Decrypt" },
      { "0xFC06", "HCIExt_SetLocalSupportedFeatures" },
      { "0xFC07", "HCIExt_SetFastTxRespTime" },
      { "0xFC08", "HCIExt_ModemTestTx" },
      { "0xFC09", "HCIExt_ModemHopTestTx" },
      { "0xFC0A", "HCIExt_ModemTestRx" },
      { "0xFC0B", "HCIExt_EndModemTest" },
      { "0xFC0C", "HCIExt_SetBDADDRCmd" },

      ///////////////////////
      // L2CAP
      ///////////////////////
      { "0xFC8A", "L2CAP_InfoReq" },
      { "0xFC92", "L2CAP_ConnParamUpdateReq" }, 

      /////////////////////////////////
      // HCI Vendor Specific Commands
      ////////////////////////////////
      ///////////////////////
      // ATT
      ///////////////////////
      { "0xFD01", "ATT_ErrorRsp" },
      { "0xFD02", "ATT_ExchangeMTUReq" },
      { "0xFD03", "ATT_ExchangeMTURsp" },
      { "0xFD04", "ATT_FindInfoReq" },
      { "0xFD05", "ATT_FindInfoRsp" },
      { "0xFD06", "ATT_FindByTypeValueReq" },
      { "0xFD07", "ATT_FindByTypeValueRsp" },
      { "0xFD08", "ATT_ReadByTypeReq" },
      { "0xFD09", "ATT_ReadByTypeRsp" },
      { "0xFD0A", "ATT_ReadReq" },
      { "0xFD0B", "ATT_ReadReq" },
      { "0xFD0C", "ATT_ReadBlobReq" },
      { "0xFD0D", "ATT_ReadBlobRsp" },
      { "0xFD0E", "ATT_ReadMultiReq" },
      { "0xFD0F", "ATT_ReadMultiRsp" },
      { "0xFD10", "ATT_ReadByGrpTypeReq" },
      { "0xFD11", "ATT_ReadByGrpTypeRsp" },
      { "0xFD12", "ATT_WriteReq" },
      { "0xFD13", "ATT_WriteRsp" },
      { "0xFD16", "ATT_PrepareWriteReq" },
      { "0xFD17", "ATT_PrepareWriteRsp" },
      { "0xFD18", "ATT_ExecuteWriteReq" },
      { "0xFD19", "ATT_ExecuteWriteRsp" },
      { "0xFD1B", "ATT_HandleValueNotification" },
      { "0xFD1D", "ATT_HandleValueIndication" },
      { "0xFD1E", "ATT_HandleValueConfirmation" },

      ///////////////////////
      // GATT
      ///////////////////////
      { "0xFD88", "GATT_DiscCharsByUUID" },
      { "0xFD96", "GATT_WriteLong" },
      { "0xFDFC", "GATT_AddService" },
      { "0xFDFD", "GATT_DelService" },
      { "0xFDFE", "GATT_AddAttribute" }, 

      ///////////////////////
      // GAP
      ///////////////////////
      { "0xFE00", "GAP_DeviceInit" },
      { "0xFE03", "GAP_ConfigDeviceAddr" },
      { "0xFE04", "GAP_DeviceDiscoveryRequest" },
      { "0xFE05", "GAP_DeviceDiscoveryCancel" },
      { "0xFE06", "GAP_MakeDiscoverable" },
      { "0xFE07", "GAP_UpdateAdvertisingData" },
      { "0xFE08", "GAP_EndDiscoverable" },
      { "0xFE09", "GAP_EstablishLinkRequest" },
      { "0xFE0A", "GAP_TerminateLinkRequest" },
      { "0xFE0B", "GAP_Authenticate" },
      { "0xFE0C", "GAP_PasskeyUpdate" },
      { "0xFE0D", "GAP_SlaveSecurityRequest" },
      { "0xFE0E", "GAP_Signable" },
      { "0xFE0F", "GAP_Bond" },
      { "0xFE10", "GAP_TerminateAuth" },
      { "0xFE30", "GAP_SetParam" },
      { "0xFE31", "GAP_GetParam" },
      { "0xFE32", "GAP_ResolvePrivateAddr" },
      { "0xFE33", "GAP_SetAdvToken" },
      { "0xFE34", "GAP_RemoveAdvToken" },
      { "0xFE35", "GAP_UpdateAdvTokens" },
      { "0xFE36", "GAP_BondSetParam" },
      { "0xFE37", "GAP_BondGetParam" }, 

      ///////////////////////
      // UTIL
      ///////////////////////
      { "0xFE80", "UTIL_Reset" },
      { "0xFE81", "UTIL_NVRead" },
      { "0xFE82", "UTIL_NVWrite" }, 

      ///////////////////////
      // HCI Status Commands
      ///////////////////////
      { "0x1405", "HCIStatus_ReadRSSI" }

    };
        #endregion // Constants

        #region Enumerations
        public enum GAP_Profile
        {
            Broadcaster = 0x01,
            Observer = 0x02,
            Peripheral = 0x04,
            Central = 0x08
        };

        public enum GAP_EnableDisable
        {
            Disable = 0x00,
            Enable = 0x01,
        };

        public enum GAP_TrueFalse
        {
            False = 0x00,
            True = 0x01,
        };

        public enum GAP_YesNo
        {
            No = 0x00,
            Yes = 0x01,
        }

        public enum GAP_ChannelMap
        {
            Channel_37 = 0x00,     // Channel 37
            Channel_38 = 0x01,     // Channel 38 
            Channel_39 = 0x02,     // Channel 39 
        };

        public enum GAP_FilterPolicy
        {
            GAP_FILTER_POLICY_ALL = 0x00, // Allow scan requests from any, allow connect request from any.
            GAP_FILTER_POLICY_WHITE_SCAN = 0x01, // Allow scan requests from white list only, allow connect request from any.
            GAP_FILTER_POLICY_WHITE_CON = 0x02, // Allow scan requests from any, allow connect request from white list only.
            GAP_FILTER_POLICY_WHITE = 0x03  // Allow scan requests from white list only, allow connect requests from white list only.
        };

        public enum PacketType
        {
            Command = 0x01,
            AsyncData = 0x02,
            SyncData = 0x03,
            Event = 0x04,
        };

        /* Command opcodes */
        public enum HCICmdOpcode
        {
            // HCI Extended Commands
            HCIExt_SetRxGain = 0xFC00,
            HCIExt_SetTxPower = 0xFC01,
            HCIExt_OnePktPerEvt = 0xFC02,
            HCIExt_ClkDivideOnHalt = 0xFC03,
            HCIExt_DelayPostProc = 0xFC04,
            HCIExt_Decrypt = 0xFC05,
            HCIExt_SetLocalSupportedFeatures = 0xFC06,
            HCIExt_SetFastTxRespTime = 0xFC07,
            HCIExt_ModemTestTx = 0xFC08,
            HCIExt_ModemHopTestTx = 0xFC09,
            HCIExt_ModemTestRx = 0xFC0A,
            HCIExt_EndModemTest = 0xFC0B,
            HCIExt_SetBDADDRCmd = 0XFC0C,

            // L2CAP
            L2CAP_InfoReq = 0xFC8A,
            L2CAP_ConnParamUpdateReq = 0xFC92,

            // HCI Vendor specific Commands
            // ATT
            ATT_ErrorRsp = 0xFD01,
            ATT_ExchangeMTUReq = 0xFD02,
            ATT_ExchangeMTURsp = 0xFD03,
            ATT_FindInfoReq = 0xFD04,
            ATT_FindInfoRsp = 0xFD05,
            ATT_FindByTypeValueReq = 0xFD06,
            ATT_FindByTypeValueRsp = 0xFD07,
            ATT_ReadByTypeReq = 0xFD08,
            ATT_ReadByTypeRsp = 0xFD09,
            ATT_ReadReq = 0xFD0A,
            ATT_ReadRsp = 0xFD0B,
            ATT_ReadBlobReq = 0xFD0C,
            ATT_ReadBlobRsp = 0xFD0D,
            ATT_ReadMultiReq = 0xFD0E,
            ATT_ReadMultiRsp = 0xFD0F,
            ATT_ReadByGrpTypeReq = 0xFD10,
            ATT_ReadByGrpTypeRsp = 0xFD11,
            ATT_WriteReq = 0xFD12,
            ATT_WriteRsp = 0xFD13,
            ATT_PrepareWriteReq = 0xFD16,
            ATT_PrepareWriteRsp = 0xFD17,
            ATT_ExecuteWriteReq = 0xFD18,
            ATT_ExecuteWriteRsp = 0xFD19,
            ATT_HandleValueNotification = 0xFD1B,
            ATT_HandleValueIndication = 0xFD1D,
            ATT_HandleValueConfirmation = 0xFD1E,

            // GATT
            GATT_DiscCharsByUUID = 0xFD88,
            GATT_WriteLong = 0xFD96,
            GATT_AddService = 0xFDFC,
            GATT_DelService = 0xFDFD,
            GATT_AddAttribute = 0xFDFE,

            // GAP
            GAP_DeviceInit = 0xFE00,
            GAP_ConfigDeviceAddr = 0xFE03,
            GAP_DeviceDiscoveryRequest = 0xFE04,
            GAP_DeviceDiscoveryCancel = 0xFE05,
            GAP_MakeDiscoverable = 0xFE06,
            GAP_UpdateAdvertisingData = 0xFE07,
            GAP_EndDiscoverable = 0xFE08,
            GAP_EstablishLinkRequest = 0xFE09,
            GAP_TerminateLinkRequest = 0xFE0A,
            GAP_Authenticate = 0xFE0B,
            GAP_PasskeyUpdate = 0xFE0C,
            GAP_SlaveSecurityRequest = 0xFE0D,
            GAP_Signable = 0xFE0E,
            GAP_Bond = 0xFE0F,
            GAP_TerminateAuth = 0xFE10,
            GAP_SetParam = 0xFE30,
            GAP_GetParam = 0xFE31,
            GAP_ResolvePrivateAddr = 0xFE32,
            GAP_SetAdvToken = 0xFE33,
            GAP_RemoveAdvToken = 0xFE34,
            GAP_UpdateAdvTokens = 0xFE35,
            GAP_BondSetParam = 0xFE36,
            GAP_BondGetParam = 0xFE37,

            // Util
            UTIL_Reset = 0xFE80,
            UTIL_NVRead = 0xFE81,
            UTIL_NVWrite = 0xFE82,

            // HCI Status Commands
            HCIStatus_ReadRSSI = 0x1405
        };


        // Event opcodes
        public enum HCIEvtOpCode
        {
            // HCI Extended Commands
            HCIExt_SetRxGainDone = 0x0400,
            HCIExt_SetTxPowerDone = 0x0401,
            HCIExt_OnePktPerEvtDone = 0x0402,
            HCIExt_ClkDivideOnHaltDone = 0x0403,
            HCIExt_DelayPostProcDone = 0x0404,
            HCIExt_DecryptDone = 0x0405,
            HCIExt_SetLocalSupportedFeaturesDone = 0x0406,
            HCIExt_SetFastTxRespTimeDone = 0x0407,
            HCIExt_ModemTestTxDone = 0x0408,
            HCIExt_ModemHopTestTxDone = 0x0409,
            HCIExt_ModemTestRxDone = 0x040A,
            HCIExt_EndModemTestDone = 0x040B,
            HCIExt_SetBDADDRCmdDone = 0x040C,

            // L2CAP 
            L2CAP_CmdReject = 0x0481,
            L2CAP_InfoRsp = 0x048B,
            L2CAP_ConnParamUpdateRsp = 0x0493,

            // ATT
            ATT_ErrorRsp = 0x0501,
            ATT_ExchangeMTUReq = 0x0502,
            ATT_ExchangeMTURsp = 0x0503,
            ATT_FindInfoReq = 0x0504,
            ATT_FindInfoRsp = 0x0505,
            ATT_FindByTypeValueReq = 0x0506,
            ATT_FindByTypeValueRsp = 0x0507,
            ATT_ReadByTypeReq = 0x0508,
            ATT_ReadByTypeRsp = 0x0509,
            ATT_ReadReq = 0x050A,
            ATT_ReadRsp = 0x050B,
            ATT_ReadBlobReq = 0x050C,
            ATT_ReadBlobRsp = 0x050D,
            ATT_ReadMultiReq = 0x050E,
            ATT_ReadMultiRsp = 0x050F,
            ATT_ReadByGrpTypeReq = 0x0510,
            ATT_ReadByGrpTypeRsp = 0x0511,
            ATT_WriteReq = 0x0512,
            ATT_WriteRsp = 0x0513,
            ATT_PrepareWriteReq = 0x0516,
            ATT_PrepareWriteRsp = 0x0517,
            ATT_ExecuteWriteReq = 0x0518,
            ATT_ExecuteWriteRsp = 0x0519,
            ATT_HandleValueNotification = 0x051B,
            ATT_HandleValueIndication = 0x051D,
            ATT_HandleValueConfirmation = 0x051E,

            // GATT
            // none

            // GAP
            GAP_DeviceInitDone = 0x0600,
            GAP_DeviceDiscoveryDone = 0x0601,
            GAP_AdvertDataUpdate = 0x0602,
            GAP_MakeDiscoverable = 0x0603,
            GAP_EndDiscoverable = 0x0604,
            GAP_EstablishLink = 0x0605,
            GAP_TerminateLink = 0x0606,
            GAP_LinkParamUpdate = 0x0607,
            GAP_RandomAddressChange = 0x0608,
            GAP_SignatureUpdate = 0x0609,
            GAP_AuthenticationComplete = 0x060A,
            GAP_PasskeyNeeded = 0x060B,
            GAP_SlaveRequestedSecurity = 0x060C,
            GAP_DeviceInformation = 0x060D,
            GAP_BondComplete = 0x060E,
            GAP_PairingRequested = 0x060F,
            GAP_HCI_ExtentionCommandStatus = 0x067F
        };

        // Event codes
        public enum HCIEvtCode
        {
            // Events
            HCI_InquiryCompleteEvent = 0x0001,
            HCI_InquiryResultEvent = 0x0002,
            HCI_ConnectionCompleteEvent = 0x0003,
            HCI_ConnectionRequestEvent = 0x0004,
            HCI_DisconnectionCompleteEvent = 0x0005,
            HCI_AuthenticationCompleteEvent = 0x0006,
            HCI_RemoteNameRequestCompleteEvent = 0x0007,
            HCI_EncryptionChangeEvent = 0x0008,
            HCI_ChangeConnectionLinkKeyCompleteEvent = 0x0009,
            HCI_MasterLinkKeyCompleteEvent = 0x000A,
            HCI_ReadRemoteSupportedFeaturesCompleteEvent = 0x000B,
            HCI_ReadRemoteVersionInformationCompleteEvent = 0x000C,
            HCI_QoSSetupCompleteEvent = 0x000D,
            HCI_CommandCompleteEvent = 0x000E,
            HCI_CommandStatusEvent = 0x000F,
            HCI_HardwareErrorEvent = 0x0010,
            HCI_FlushOccurredEvent = 0x0011,
            HCI_RoleChangeEvent = 0x0012,
            HCI_NumberOfCompletedPacketsEvent = 0x0013,
            HCI_ModeChangeEvent = 0x0014,
            HCI_ReturnLinkKeysEvent = 0x0015,
            HCI_PINCodeRequestEvent = 0x0016,
            HCI_LinkKeyRequestEvent = 0x0017,
            HCI_LinkKeyNotificationEvent = 0x0018,
            HCI_LoopbackCommandEvent = 0x0019,
            HCI_DataBufferOverflowEvent = 0x001A,
            HCI_MaxSlotsChangeEvent = 0x001B,
            HCI_ReadClockOffsetCompleteEvent = 0x001C,
            HCI_ConnectionPacketTypeChangedEvent = 0x001D,
            HCI_QoSViolationEvent = 0x001E,
            HCI_PageScanModeChangeEvent = 0x001F,
            HCI_PageScanRepetitionModeChangeEvent = 0x0020,
            HCI_FlowSpecificationCompleteEvent = 0x0021,
            HCI_InquiryResultWithRSSIEvent = 0x0022,
            HCI_ReadRemoteExtendedFeaturesCompleteEvent = 0x0023,
            HCI_SynchronousConnectionCompleteEvent = 0x002C,
            HCI_SynchronousConnectionChangedEvent = 0x002D,
            HCI_SniffSubratingEvent = 0x002E,
            HCI_ExtendedInquiryResultEvent = 0x002F,
            HCI_EncryptionKeyRefreshCompleteEvent = 0x0030,
            HCI_IOCapabilityRequestEvent = 0x0031,
            HCI_IOCapabilityResponseEvent = 0x0032,
            HCI_UserConfirmationRequestEvent = 0x0033,
            HCI_UserPasskeyRequestEvent = 0x0034,
            HCI_RemoteOOBDataRequestEvent = 0x0035,
            HCI_SimplePairingCompleteEvent = 0x0036,
            HCI_RemoteOobResponseEvent = 0x0037,
            HCI_LinkSupervisionTimeoutChangedEvent = 0x0038,
            HCI_EnhancedFlushCompleteEvent = 0x0039,
            HCI_SniffRequestEvent = 0x003A,
            HCI_UserPasskeyNotificationEvent = 0x003B,
            HCI_KeypressNotificationEvent = 0x003C,
            HCI_RemoteHostSupportedFeaturesNotificationEvent = 0x003D,
            HCI_PhysicalLinkCompleteEvent = 0x0040,
            HCI_ChannelSelectedEvent = 0x0041,
            HCI_DisconnectionPhysicalLinkCompleteEvent = 0x0042,
            HCI_PhysicalLinkLossEarlyWarningEvent = 0x0043,
            HCI_PhysicalLinkRecoveryEvent = 0x0044,
            HCI_LogicalLinkCompleteEvent = 0x0045,
            HCI_DisconnectionLogicalLinkCompleteEvent = 0x0046,
            HCI_FlowSpecModifyCompleteEvent = 0x0047,
            HCI_NumberOfCompletedDataBlocksEvent = 0x0048,
            HCI_ShortRangeModeChangeCompleteEvent = 0x004C,
            HCI_AMP_StatusChangeEvent = 0x004D,
            HCI_AMP_StartTestEvent = 0x0049,
            HCI_AMP_TestEndEvent = 0x004A,
            HCI_AMP_ReceiverReportEvent = 0x004B,
#if WARNING_NOT_SUPPORTED
      HCI_LE_ConnectionCompleteEvent                      = 0x003E,  // Subevent Code 0x01
      HCI_LE_AdvertisingReportEvent                       = 0x003E,  // Subevent Code 0x02 
      HCI_LE_ConnectionUpdateCompleteEvent                = 0x003E,  // Subevent Code 0x03  
      HCI_LE_ReadRemoteUsedFeaturesCompleteEvent          = 0x003E,  // Subevent Code 0x04     
      HCI_LE_LongTermKeyRequestEvent"},                   = 0x003E,  // Subevent Code 0x05     
#else
            HCI_LE_SpecialSubEvent = 0x003E,  // see above
#endif
            HCI_LE_ExtEvent = 0x00FF
        };

        public enum GAP_DiscoveryMode
        {
            Nondiscoverable = 0x00,
            General = 0x01,
            Limited = 0x02,
            All = 0x03
        };

        public enum GAP_AddrType
        {
            Public = 0x00,
            Static = 0x01,
            PrivateNonResolve = 0x02,
            PrivateResolve = 0x03
        };

        public enum GAP_ConnHandle
        {
            Default = 0x0000,
            Init = 0xFFFE,
            All = 0xFFFF,
        };

        public enum GAP_IOCaps
        {
            DisplayOnly = 0x00,
            DisplayYesNo = 0x01,
            KeyboardOnly = 0x02,
            NoInputNoOutput = 0x03,
            KeyboardDisplay = 0x04,
        }

        public enum GAP_AuthReq  // bit fields
        {
            Bonding = 0x01,   // Bonding – exchange and save key information
            Man_In_The_Middle = 0x04    // Man-In-The-Middle protection
        }

        public enum GAP_KeyDisk  // bit fields 
        {
            Slave_Encryption_Key = 0x01,   // Slave Encryption Key
            Slave_Identification_Key = 0x02,   // Slave Identification Key
            Slave_Signing_Key = 0x04,   // Slave Signing Key
            Master_Encryption_Key = 0x08,   // Master Encryption Key
            Master_Identification_Key = 0x10,   // Master Identification Key
            Master_Signing_Key = 0x20,   // Master Signing Key
        }

        public enum GAP_ParamId
        {
            TGAP_GEN_DISC_ADV_MIN = 0x00,
            TGAP_LIM_ADV_TIMEOUT = 0x01,
            TGAP_GEN_DISC_SCAN = 0x02,
            TGAP_LIM_DISC_SCAN = 0x03,
            TGAP_CONN_EST_ADV_TIMEOUT = 0x04,
            TGAP_CONN_PARAM_TIMEOUT = 0x05,
            TGAP_LIM_DISC_ADV_INT_MIN = 0x06,
            TGAP_LIM_DISC_ADV_INT_MAX = 0x07,
            TGAP_GEN_DISC_ADV_INT_MIN = 0x08,
            TGAP_GEN_DISC_ADV_INT_MAX = 0x09,
            TGAP_CONN_ADV_INT_MIN = 0x0A,
            TGAP_CONN_ADV_INT_MAX = 0x0B,
            TGAP_CONN_SCAN_INT = 0x0C,
            TGAP_CONN_SCAN_WIND = 0x0D,
            TGAP_CONN_HIGH_SCAN_INT = 0x0E,
            TGAP_CONN_HIGH_SCAN_WIND = 0x0F,
            TGAP_GEN_DISC_SCAN_INT = 0x10,
            TGAP_GEN_DISC_SCAN_WIND = 0x11,
            TGAP_LIM_DISC_SCAN_INT = 0x12,
            TGAP_LIM_DISC_SCAN_WIND = 0x13,
            TGAP_CONN_EST_ADV = 0x14,
            TGAP_CONN_EST_INT_MIN = 0x15,
            TGAP_CONN_EST_INT_MAX = 0x16,
            TGAP_CONN_EST_SCAN_INT = 0x17,
            TGAP_CONN_EST_SCAN_WIND = 0x18,
            TGAP_CONN_EST_SUPERV_TIMEOUT = 0x19,
            TGAP_CONN_EST_LATENCY = 0x1A,
            TGAP_CONN_EST_MIN_CE_LEN = 0x1B,
            TGAP_CONN_EST_MAX_CE_LEN = 0x1C,
            TGAP_PRIVATE_ADDR_INT = 0x1D,
            TGAP_SM_TIMEOUT = 0x1E,
            TGAP_SM_MIN_KEY_LEN = 0x1F,
            TGAP_SM_MAX_KEY_LEN = 0x20,
            TGAP_GAP_TESTCODE = 0x21,
            TGAP_SM_TESTCODE = 0x22,
            TGAP_AUTH_TASK_ID = 0x23,
            TGAP_PARAMID_MAX = 0x24,
            TGAP_GATT_TESTCODE = 0x64,
            TGAP_ATT_TESTCODE = 0x65,
            SET_RX_DEBUG = 0xFE,
            GET_MEM_USED = 0xFF
        };

        // Termination Reason Code
        public enum GAP_TerminationReason
        {
            GAP_LL_SUPERVISION_TIMEOUT_TERM = 0x08,
            GAP_LL_PEER_REQUESTED_TERM = 0x13,
            GAP_LL_HOST_REQUESTED_TERM = 0x16,
            GAP_LL_CONTROL_PKT_TIMEOUT_TERM = 0x22,
            GAP_LL_CONTROL_PKT_INSTANT_PASSED_TERM = 0x28,
            GAP_LL_LSTO_VIOLATION_TERM = 0x3B,
            GAP_LL_MIC_FAILURE_TERM = 0x3D,
            GAP_LL_FAILED_TO_ESTABLISH = 0x3E
        }

        public enum GAP_EventType
        {
            GAP_EVENT_CONN_UNDIRECT_AD = 0x00, // Connectable undirected advertisement
            GAP_EVENT_CONN_DIRECT_AD = 0x01, // Connectable directed advertisement
            GAP_EVENT_DISCN_UNDIRECT_AD = 0x02, // Discoverable undirected advertisement
            GAP_EVENT_NON_CONN_UNDIRECT_AD = 0x03, // Non-connectable undirected advertisement
            GAP_EVENT_SCAN_RESPONSE = 0x04, // Scan Response
        }

        public enum GAP_AuthenticatedCsrk
        {
            GAP_CSRK_NOT_AUTHENTICATED = 0,  // CSRK is not authenticated
            GAP_CSRK_AUTHENTICATED = 1   // CSRK is authenticated
        }

        public enum GAP_BondParamId
        {
            GAPBOND_PAIRING_MODE = 0x400,
            GAPBOND_INITIATE_WAIT = 0x401,
            GAPBOND_MITM_PROTECTION = 0x402,
            GAPBOND_IO_CAPABILITIES = 0x403,
            GAPBOND_OOB_ENABLED = 0x404,
            GAPBOND_OOB_DATA = 0x405,
            GAPBOND_BONDING_ENABLED = 0x406,
            GAPBOND_KEY_DIST_LIST = 0x407,
            GAPBOND_DEFAULT_PASSCODE = 0x408,
            GAPBOND_ERASE_ALLBONDS = 0x409,
            GAPBOND_AUTO_FAIL_PAIRING = 0x40A,
            GAPBOND_AUTO_FAIL_REASON = 0x40B,
            GAPBOND_KEYSIZE = 0x40C,
        }

        public enum GAP_AvertAdType
        {
            GAPADVERT_SCAN_RSP_DATA = 0x00,  // SCAN_RSP data
            GAPADVERT_ADVERTISEMENT_DATA = 0x01   // Advertisement data
        }

        public enum GAP_AdTypes
        {
            GAP_ADTYPE_FLAGS = 0x01,  // Flags: Discovery Mode
            GAP_ADTYPE_16BIT_MORE = 0x02,  // Service: More 16-bit UUIDs available
            GAP_ADTYPE_16BIT_COMPLETE = 0x03,  // Service: Complete list of 16-bit UUIDs
            GAP_ADTYPE_32BIT_MORE = 0x04,  // Service: More 32-bit UUIDs available
            GAP_ADTYPE_32BIT_COMPLETE = 0x05,  // Service: Complete list of 32-bit UUIDs
            GAP_ADTYPE_128BIT_MORE = 0x06,  // Service: More 128-bit UUIDs available
            GAP_ADTYPE_128BIT_COMPLETE = 0x07,  // Service: Complete list of 128-bit UUIDs
            GAP_ADTYPE_LOCAL_NAME_SHORT = 0x08,  // Shortened local name
            GAP_ADTYPE_LOCAL_NAME_COMPLETE = 0x09,  // Complete local name
            GAP_ADTYPE_POWER_LEVEL = 0x0A,  // TX Power Level: 0xXX: -127 to +127 dBm
            GAP_ADTYPE_OOB_CLASS_OF_DEVICE = 0x0D,  // Simple Pairing OOB Tag: Class of device (3 octets)
            GAP_ADTYPE_OOB_SIMPLE_PAIRING_HASHC = 0x0E,  // Simple Pairing OOB Tag: Simple Pairing Hash C (16 octets)
            GAP_ADTYPE_OOB_SIMPLE_PAIRING_RANDR = 0x0F,  // Simple Pairing OOB Tag: Simple Pairing Randomizer R (16 octets)
            GAP_ADTYPE_SM_TK = 0x10,  // Security Manager TK Value
            GAP_ADTYPE_SM_OOB_FLAG = 0x11,  // Secutiry Manager OOB Flags
            GAP_ADTYPE_SLAVE_CONN_INTERVAL_RANGE = 0x12,  // Min and Max values of the connection interval (2 octets Min, 2 octets Max) (0xFFFF indicates no conn interval min or max)
            GAP_ADTYPE_SIGNED_DATA = 0x13,  // Signed Data field
            GAP_ADTYPE_SERVICES_LIST_16BIT = 0x14,  // Service Solicitation: list of 16-bit Service UUIDs
            GAP_ADTYPE_SERVICES_LIST_128BIT = 0x15,  // Service Solicitation: list of 128-bit Service UUIDs
            GAP_ADTYPE_SERVICE_DATA = 0x16,  // Service Data
            GAP_ADTYPE_MANUFACTURER_SPECIFIC = 0xFF   // Manufacturer Specific Data: first 2 octets contain the Company Identifier Code followed by the additional manufacturer specific data
        }

        public enum GAP_UiInput
        {
            DONT_ASK_TO_INPUT_PASSCODE = 0x00,  // Don’t ask user to input a passcode
            ASK_TO_INPUT_PASSCODE = 0x01   // Ask user to input a passcode
        }

        public enum GAP_UiOutput
        {
            DONT_DISPLAY_PASSCODE = 0x00, // Don’t display passcode
            DISPLAY_PASSCODE = 0x01  // Display a passcode
        }

        public enum ATT_ExecuteWriteFlags
        {
            Cancel_all_prepared_writes = 0x00,
            Immediately_write_all_pending_prepared_values = 0x01
        };

        public enum ATT_FindInfoFormat
        {
            HANDLE_BT_UUID_TYPE__handles_and_16_bit_Bluetooth_UUIDs = 0x01,
            HANDLE_UUID_TYPE__handles_and_128_bit_UUIDs = 0x02
        };

        public enum HCIExt_TxPower
        {
            HCI_EXT_TX_POWER_MINUS_23_DBM = 0x00,
            HCI_EXT_TX_POWER_MINUS_6_DBM = 0x01,
            HCI_EXT_TX_POWER_0_DBM = 0x02,
            HCI_EXT_TX_POWER_4_DBM = 0x03
        };

        public enum HCIExt_RxGain
        {
            HCI_EXT_RX_GAIN_STD = 0x00,
            HCI_EXT_RX_GAIN_HIGH = 0x01
        }

        public enum HCIExt_OnePktPerEvtCtrl
        {
            HCI_EXT_DISABLE_ONE_PKT_PER_EVT = 0x00,
            HCI_EXT_ENABLE_ONE_PKT_PER_EVT = 0x01
        }

        public enum HCIExt_ClkDivideOnHaltCtrl
        {
            HCI_EXT_DISABLE_CLK_DIVIDE_ON_HALT = 0x00,
            HCI_EXT_ENABLE_CLK_DIVIDE_ON_HALT = 0x01
        }

        public enum HCIExt_DelayPostProcCtrl
        {
            HCI_EXT_DISABLE_DELAY_POST_PROC = 0x00,
            HCI_EXT_ENABLE_DELAY_POST_PROC = 0x01
        }

        public enum HCIExt_SetFastTxRespTimeCtrl
        {
            HCI_EXT_DISABLE_FAST_TX_RESP_TIME = 0x00,
            HCI_EXT_ENABLE_FAST_TX_RESP_TIME = 0x01
        }

        public enum HCIExt_CwMode
        {
            HCI_EXT_TX_MODULATED_CARRIER = 0x00,
            HCI_EXT_TX_UNMODULATED_CARRIER = 0x01
        }

        public enum HCIExt_StatusCodes
        {
            HCI_SUCCESS = 0x00, // Success
            HCI_ERR_UNKNOWN_HCI_CMD = 0x01, // Unknown HCI Command
            HCI_ERR_UNKNOWN_CONN_ID = 0x02, // Unknown Connection Identifier
            HCI_ERR_HW_FAILURE = 0x03, // Hardware Failure
            HCI_ERR_PAGE_TIMEOUT = 0x04, // Page Timeout
            HCI_ERR_AUTH_FAILURE = 0x05, // Authentication Failure
            HCI_ERR_PIN_KEY_MISSING = 0x06, // PIN/Key Missing
            HCI_ERR_MEM_CAP_EXCEEDED = 0x07, // Memory Capacity Exceeded
            HCI_ERR_CONN_TIMEOUT = 0x08, // Connection Timeout
            HCI_ERR_CONN_LIMIT_EXCEEDED = 0x09, // Connection Limit Exceeded
            HCI_ERR_SYNCH_CONN_LIMIT_EXCEEDED = 0x0A, // Synchronous Connection Limit To A Device Exceeded
            HCI_ERR_ACL_CONN_ALREADY_EXISTS = 0x0B, // ACL Connection Already Exists
            HCI_ERR_CMD_DISALLOWED = 0x0C, // Command Disallowed
            HCI_ERR_CONN_REJ_LIMITED_RESOURCES = 0x0D, // Connection Rejected Due To Limited Resources
            HCI_ERR_CONN_REJECTED_SECURITY_REASONS = 0x0E, // Connection Rejected Due To Security Reasons
            HCI_ERR_CONN_REJECTED_UNACCEPTABLE_BDADDR = 0x0F, // Connection Rejected Due To Unacceptable BD_ADDR
            HCI_ERR_CONN_ACCEPT_TIMEOUT_EXCEEDED = 0x10, // Connection Accept Timeout Exceeded
            HCI_ERR_UNSUPPORTED_FEATURE_PARAM_VALUE = 0x11, // Unsupported Feature Or Parameter Value
            HCI_ERR_INVALID_HCI_CMD_PARAMS = 0x12, // Invalid HCI Command Parameters
            HCI_ERR_REMOTE_USER_TERM_CONN = 0x13, // Remote User Terminated Connection
            HCI_ERR_REMOTE_DEVICE_TERM_CONN_LOW_RESOURCES = 0x14, // Remote Device Terminated Connection Due To Low Resources
            HCI_ERR_REMOTE_DEVICE_TERM_CONN_POWER_OFF = 0x15, // Remote Device Terminated Connection Due To Power Off
            HCI_ERR_CONN_TERM_BY_LOCAL_HOST = 0x16, // Connection Terminated By Local Host
            HCI_ERR_REPEATED_ATTEMPTS = 0x17, // Repeated Attempts
            HCI_ERR_PAIRING_NOT_ALLOWED = 0x18, // Pairing Not Allowed
            HCI_ERR_UNKNOWN_LMP_PDU = 0x19, // Unknown LMP PDU
            HCI_ERR_UNSUPPORTED_REMOTE_FEATURE = 0x1A, // Unsupported Remote or LMP Feature
            HCI_ERR_SCO_OFFSET_REJ = 0x1B, // SCO Offset Rejected
            HCI_ERR_SCO_INTERVAL_REJ = 0x1C, // SCO Interval Rejected
            HCI_ERR_SCO_AIR_MODE_REJ = 0x1D, // SCO Air Mode Rejected
            HCI_ERR_INVALID_LMP_PARAMS = 0x1E, // Invalid LMP Parameters
            HCI_ERR_UNSPECIFIED_ERROR = 0x1F, // Unspecified Error
            HCI_ERR_UNSUPPORTED_LMP_PARAM_VAL = 0x20, // Unsupported LMP Parameter Value
            HCI_ERR_ROLE_CHANGE_NOT_ALLOWED = 0x21, // Role Change Not Allowed
            HCI_ERR_LMP_LL_RESP_TIMEOUT = 0x22, // LMP/LL Response Timeout
            HCI_ERR_LMP_ERR_TRANSACTION_COLLISION = 0x23, // LMP Error Transaction Collision
            HCI_ERR_LMP_PDU_NOT_ALLOWED = 0x24, // LMP PDU Not Allowed
            HCI_ERR_ENCRYPT_MODE_NOT_ACCEPTABLE = 0x25, // Encryption Mode Not Acceptable
            HCI_ERR_LINK_KEY_CAN_NOT_BE_CHANGED = 0x26, // Link Key Can Not be Changed
            HCI_ERR_REQ_QOS_NOT_SUPPORTED = 0x27, // Requested QoS Not Supported
            HCI_ERR_INSTANT_PASSED = 0x28, // Instant Passed
            HCI_ERR_PAIRING_WITH_UNIT_KEY_NOT_SUPPORTED = 0x29, // Pairing With Unit Key Not Supported
            HCI_ERR_DIFFERENT_TRANSACTION_COLLISION = 0x2A, // Different Transaction Collision
            HCI_ERR_RESERVED1 = 0x2B, // Reserved
            HCI_ERR_QOS_UNACCEPTABLE_PARAM = 0x2C, // QoS Unacceptable Parameter
            HCI_ERR_QOS_REJ = 0x2D, // QoS Rejected
            HCI_ERR_CHAN_ASSESSMENT_NOT_SUPPORTED = 0x2E, // Channel Assessment Not Supported
            HCI_ERR_INSUFFICIENT_SECURITY = 0x2F, // Insufficient Security
            HCI_ERR_PARAM_OUT_OF_MANDATORY_RANGE = 0x30, // Parameter Out Of Mandatory Range
            HCI_ERR_RESERVED2 = 0x31, // Reserved
            HCI_ERR_ROLE_SWITCH_PENDING = 0x32, // Role Switch Pending
            HCI_ERR_RESERVED3 = 0x33, // Reserved
            HCI_ERR_RESERVED_SLOT_VIOLATION = 0x34, // Reserved Slot Violation
            HCI_ERR_ROLE_SWITCH_FAILED = 0x35, // Role Switch Failed
            HCI_ERR_EXTENDED_INQUIRY_RESP_TOO_LARGE = 0x36, // Extended Inquiry Response Too Large
            HCI_ERR_SIMPLE_PAIRING_NOT_SUPPORTED_BY_HOST = 0x37, // Simple Pairing Not Supported By Host
            HCI_ERR_HOST_BUSY_PAIRING = 0x38, // Host Busy - Pairing
            HCI_ERR_CONN_REJ_NO_SUITABLE_CHAN_FOUND = 0x39, // Connection Rejected Due To No Suitable Channel Found
            HCI_ERR_CONTROLLER_BUSY = 0x3A, // Controller Busy
            HCI_ERR_UNACCEPTABLE_CONN_INTERVAL = 0x3B, // Unacceptable Connection Interval
            HCI_ERR_DIRECTED_ADV_TIMEOUT = 0x3C, // Directed Advertising Timeout
            HCI_ERR_CONN_TERM_MIC_FAILURE = 0x3D, // Connection Terminated Due To MIC Failure
            HCI_ERR_CONN_FAILED_TO_ESTABLISH = 0x3E, // Connection Failed To Be Established
            HCI_ERR_MAC_CONN_FAILED = 0x3F  // MAC Connection Failed
        }

        public enum HCI_StatusCodes
        {
            Success = 0x00,
            Failure = 0x01,
            InvalidParameter = 0x02,
            InvalidTask = 0x03,
            MsgBufferNotAvailable = 0x04,
            InvalidMsgPointer = 0x05,
            InvalidEventId = 0x06,
            InvalidInteruptId = 0x07,
            NoTimerAvail = 0x08,
            NVItemUnInit = 0x09,
            NVOpFailed = 0x0A,
            InvalidMemSize = 0x0B,
            ErrorCommandDisallowed = 0x0C,

            bleNotReady = 0x10,   // Not ready to perform task
            bleAlreadyInRequestedMode = 0x11,   // Already performing that task
            bleIncorrectMode = 0x12,   // Not setup properly to perform that task
            bleMemAllocError = 0x13,   // Memory allocation error occurred
            bleNotConnected = 0x14,   // Can't perform function when not in a connection
            bleNoResources = 0x15,   // There are no resource available
            blePending = 0x16,   // Waiting
            bleTimeout = 0x17,   // Timed out performing function
            bleInvalidRange = 0x18,   // A parameter is out of range
            bleLinkEncrypted = 0x19,   // The link is already encrypted
            bleProcedureComplete = 0x1A,   // The Procedure is completed

            // GAP Status Return Values - returned as bStatus_t
            bleGAPUserCanceled = 0x30,   // The user canceled the task
            bleGAPConnNotAcceptable = 0x31,   // The connection was not accepted
            bleGAPBondRejected = 0x32,   // The bound information was rejected.  

            // ATT Status Return Values - returned as bStatus_t
            bleInvalidPDU = 0x40,   // The attribute PDU is invalid
            bleInsufficientAuthen = 0x41,   // The attribute has insufficient authentication
            bleInsufficientEncrypt = 0x42,   // The attribute has insufficient encryption
            bleInsufficientKeySize = 0x43,   // The attribute has insufficient encryption key size

            // L2CAP Status Return Values - returned as bStatus_t
            INVALID_TASK_ID = 0xFF    // Task ID isn't setup properly
        }

        public enum HCI_ErrorRspCodes
        {
            INVALID_HANDLE = 0x01,
            READ_NOT_PERMITTED = 0x02,
            WRITE_NOT_PERMITTED = 0x03,
            INVALID_PDU = 0x04,
            INSUFFICIENT_AUTHEN = 0x05,
            UNSUPPORTED_REQ = 0x06,
            INVALID_OFFSET = 0x07,
            INSUFFICIENT_AUTHOR = 0x08,
            PREPARE_QUEUE_FULL = 0x09,
            ATTR_NOT_FOUND = 0x0A,
            ATTR_NOT_LONG = 0x0B,
            INSUFFICIENT_KEY_SIZE = 0x0C,
            INVALID_SIZE = 0x0D,
            UNLIKELY_ERROR = 0x0E,
            INSUFFICIENT_ENCRYPTION = 0x0F,
            UNSUPPORTED_GRP_TYPE = 0x10,
            INSUFFICIENT_RESOURCES = 0x11,
            INVALID_VALUE = 0x80,
        }

        public enum UTIL_ResetType
        {
            Hard_Reset = 0x00,   // Hard Reset
            Soft_Reset = 0x01    // Soft Reset
        }

        public enum L2CAP_InfoTypes
        {
            CONNECTIONLESS_MTU = 0x0001,  // CONNECTIONLESS_MTU
            EXTENDED_FEATURES = 0x0002,  // EXTENDED_FEATURES 
            FIXED_CHANNELS = 0x0003   // FIXED_CHANNELS    
        }

        public enum L2CAP_RejectReasons
        {
            L2CAP_REJECT_CMD_NOT_UNDERSTOOD = 0x0000,  // Command not understood
            L2CAP_REJECT_SIGNAL_MTU_EXCEED = 0x0001,  // Signaling MTU exceeded 
            L2CAP_REJECT_INVALID_CID = 0x0002   // Invalid CID in request
        }

        public enum L2CAP_ConnParamUpdateResult
        {
            CONN_PARAMS_ACCEPTED = 0x0000,  // CONN_PARAMS_ACCEPTED
            CONN_PARAMS_REJECTED = 0x0001   // CONN_PARAMS_REJECTED
        }

        public enum GATT_ServiceUUID
        {
            PrimaryService = 0x2800,     // PrimaryService 
            SecondaryService = 0x2801      // SecondaryService
        }

        public enum GATT_Permissions
        {
            GATT_PERMIT_READ = 0x01,       // GATT_PERMIT_READ
            GATT_PERMIT_WRITE = 0x02,       // GATT_PERMIT_WRITE
            GATT_PERMIT_AUTHEN_READ = 0x04,       // GATT_PERMIT_AUTHEN_READ
            GATT_PERMIT_AUTHEN_WRITE = 0x08,       // GATT_PERMIT_AUTHEN_WRITE
            GATT_PERMIT_AUTHOR_READ = 0x10,       // GATT_PERMIT_AUTHOR_READ
            GATT_PERMIT_AUTHOR_WRITE = 0x20,       // GATT_PERMIT_AUTHOR_WRITE
        }

        public enum GAP_SMPFailureTypes
        {
            SUCCESS = 0x00, // SUCCESS
            SMP_PAIRING_FAILED_PASSKEY_ENTRY_FAILED = 0x01, // SMP_PAIRING_FAILED_PASSKEY_ENTRY_FAILED
            SMP_PAIRING_FAILED_OOB_NOT_AVAIL = 0x02, // SMP_PAIRING_FAILED_OOB_NOT_AVAIL
            SMP_PAIRING_FAILED_AUTH_REQ = 0x03, // SMP_PAIRING_FAILED_AUTH_REQ
            SMP_PAIRING_FAILED_CONFIRM_VALUE = 0x04, // SMP_PAIRING_FAILED_CONFIRM_VALUE
            SMP_PAIRING_FAILED_NOT_SUPPORTED = 0x05, // SMP_PAIRING_FAILED_NOT_SUPPORTED
            SMP_PAIRING_FAILED_ENC_KEY_SIZE = 0x06, // SMP_PAIRING_FAILED_ENC_KEY_SIZE
            SMP_PAIRING_FAILED_CMD_NOT_SUPPORTED = 0x07, // SMP_PAIRING_FAILED_CMD_NOT_SUPPORTED
            SMP_PAIRING_FAILED_UNSPECIFIED = 0x08, // SMP_PAIRING_FAILED_UNSPECIFIED
            SMP_PAIRING_FAILED_REPEATED_ATTEMPTS = 0x09, // SMP_PAIRING_FAILED_REPEATED_ATTEMPTS
            bleTimeout = 0x17  // bleTimeout
        }

        public enum GAP_OobDataFlag
        {
            Out_Of_Bounds_Data_Not_Available = 0x00, // Out-Of-Bounds (OOB) data is NOT available
            Out_Of_Bounds_Data_Available = 0x01, // Out-Of-Bounds (OOB) data is available
        }

        #endregion // Enumeration
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CommandHeader
        {
            Byte packetType;
            UInt16 opCode;
            Byte dataLength;
        }
        public const Byte CmdHdrSize = 0x04;
        public const UInt16 CmdRspReqOCodeMask = 0x00FF;
        public const string ZeroXStr = "0x";
/*
        public struct EventHeader
        {
            Byte packetType;
            Byte eventCode;
            Byte dataLength;
        }*/
        public const Byte EvtHdrSize = 0x03;

        public struct GATTReadByTypeData
        {
            public Byte properties;
            public UInt16 handle;
            public UInt16 uuid;
        }

        public const string EmptyBDAStr = "00:00:00:00:00:00";
        public const string Empty16BytesStr = "00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00";
        public const string Empty8BytesStr = "00:00:00:00:00:00:00:00";
        public const string Empty2BytesStr = "00:00";

        public static readonly byte[] EmptyBDA = { 0, 0, 0, 0, 0, 0 };
        public static readonly byte[] Empty16Bytes = { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
        public static readonly byte[] Empty8Bytes = { 00, 00, 00, 00, 00, 00, 00, 00 };
        public static readonly byte[] Empty2Bytes = { 00, 00 };

        public const UInt16 MaxUInt16 = 0xFFFF;

        /***********************************************************/
        // COMMANDS
        /***********************************************************/

        #region HCI Extended Commands
        /***********************************************************/
        public class HCIExtCmds
        /***********************************************************/
        {
            /***********************************************************/
            public class HCIExt_SetRxGain
            {
                public string cmdName = "HCIExt_SetRxGain";
                public Byte dataLength = 0x01;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.HCIExt_SetRxGain;
                [Description("HCIExt_SetRxGain")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private HCIExt_RxGain _rxGain = HCICmds.HCIExt_RxGain.HCI_EXT_RX_GAIN_STD;
                [Description("Rx Gain (1 Byte) - Set the RF receiver gain")]
                [DefaultValueAttribute(HCICmds.HCIExt_RxGain.HCI_EXT_RX_GAIN_STD)]
                [Serialize(number =1)]
                public HCIExt_RxGain rxGain
                {
                    get { return _rxGain; }
                    set { _rxGain = value; }
                }
            }
            /***********************************************************/
            public class HCIExt_SetTxPower : HCISerializer
            {
                public HCIExt_SetTxPower() : base ()
                {
                    dataLength = 0x01;  // fixed length data only
                    opCodeValue = (UInt16)HCICmds.HCICmdOpcode.HCIExt_SetTxPower;
                }
                public string cmdName = "HCIExt_SetTxPower";
                [Description("HCIExt_SetTxPower")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private HCIExt_TxPower _txPower = HCICmds.HCIExt_TxPower.HCI_EXT_TX_POWER_0_DBM;
                [Description("Tx Power dBm (1 Byte) - Set the RF transmitter output power")]
                [DefaultValueAttribute(HCICmds.HCIExt_TxPower.HCI_EXT_TX_POWER_0_DBM)]
                [Serialize(number = 1)]
                public HCIExt_TxPower txPower
                {
                    get { return _txPower; }
                    set { _txPower = value; }
                }
            }

            /***********************************************************/
            public class HCIExt_OnePktPerEvt:HCISerializer
            {
                public HCIExt_OnePktPerEvt() : base ()
                {
                    dataLength = 0x01;  // fixed length data only
                    opCodeValue = (UInt16)HCICmds.HCICmdOpcode.HCIExt_OnePktPerEvt;
                }
                public string cmdName = "HCIExt_OnePktPerEvt";
                [Description("HCIExt_OnePktPerEvt")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private HCIExt_OnePktPerEvtCtrl _control = HCICmds.HCIExt_OnePktPerEvtCtrl.HCI_EXT_DISABLE_ONE_PKT_PER_EVT;
                [Description("Control (1 Byte) - Enable or disable allowing only one packet per event.")]
                [DefaultValueAttribute(HCICmds.HCIExt_OnePktPerEvtCtrl.HCI_EXT_DISABLE_ONE_PKT_PER_EVT)]
                [Serialize (number = 1)]
                public HCIExt_OnePktPerEvtCtrl control
                {
                    get { return _control; }
                    set { _control = value; }
                }
            }

            /***********************************************************/
            public class HCIExt_ClkDivideOnHalt : HCISerializer
            {
                public HCIExt_ClkDivideOnHalt() : base ()
                {
                    dataLength = 0x01;  // fixed length data only
                    opCodeValue = (UInt16)HCICmds.HCICmdOpcode.HCIExt_ClkDivideOnHalt;
                }
                public string cmdName = "HCIExt_ClkDivideOnHalt";
                [Description("HCIExt_ClkDivideOnHalt")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private HCIExt_ClkDivideOnHaltCtrl _control = HCICmds.HCIExt_ClkDivideOnHaltCtrl.HCI_EXT_DISABLE_CLK_DIVIDE_ON_HALT;
                [Description("Control (1 Byte) - Enable or disable clock division on halt.")]
                [DefaultValueAttribute(HCICmds.HCIExt_ClkDivideOnHaltCtrl.HCI_EXT_DISABLE_CLK_DIVIDE_ON_HALT)]
                [Serialize(number = 1)]
                public HCIExt_ClkDivideOnHaltCtrl control
                {
                    get { return _control; }
                    set { _control = value; }
                }
            }

            /***********************************************************/
            public class HCIExt_DelayPostProc : HCISerializer
            {
                public HCIExt_DelayPostProc() : base ()
                {
                    dataLength = 0x01;  // fixed length data only
                    opCodeValue = (UInt16)HCICmds.HCICmdOpcode.HCIExt_DelayPostProc;
                }
                public string cmdName = "HCIExt_DelayPostProc";
                [Description("HCIExt_DelayPostProc")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private HCIExt_DelayPostProcCtrl _control = HCICmds.HCIExt_DelayPostProcCtrl.HCI_EXT_DISABLE_DELAY_POST_PROC;
                [Description("Control (1 Byte) - Enable or disable delaying post processing (if possible).")]
                [DefaultValueAttribute(HCICmds.HCIExt_DelayPostProcCtrl.HCI_EXT_DISABLE_DELAY_POST_PROC)]
                [Serialize(number = 1)]
                public HCIExt_DelayPostProcCtrl control
                {
                    get { return _control; }
                    set { _control = value; }
                }
            }

            /***********************************************************/
            public class HCIExt_Decrypt : HCISerializer
            {
                public HCIExt_Decrypt() : base ()
                {
                    dataLength = 0x0;  // fixed length data only
                    opCodeValue = (UInt16)HCICmds.HCICmdOpcode.HCIExt_Decrypt;
                }
                public string cmdName = "HCIExt_Decrypt";
                [Description("HCIExt_Decrypt")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                public const Byte keySize = 16;
                private string _key = "BF:01:FB:9D:4E:F3:BC:36:D8:74:F5:39:41:38:68:4C";
                [Description("Key (16 Bytes) - 128 bit key for the decryption of the data")]
                [DefaultValueAttribute("BF:01:FB:9D:4E:F3:BC:36:D8:74:F5:39:41:38:68:4C")]
                public string key
                {
                    get { return _key; }
                    set { _key = value; }
                }

                public const Byte dataSize = 16;
                private string _data = "66:C6:C2:27:8E:3B:8E:05:3E:7E:A3:26:52:1B:AD:99";
                [Description("Data (16 Bytes) - 128 bit encrypted data to be decrypted")]
                [DefaultValueAttribute("66:C6:C2:27:8E:3B:8E:05:3E:7E:A3:26:52:1B:AD:99")]
                public string data
                {
                    get { return _data; }
                    set { _data = value; }
                }
            }

            /***********************************************************/
            public class HCIExt_SetLocalSupportedFeatures : HCISerializer
            {
                public HCIExt_SetLocalSupportedFeatures() : base ()
                {
                    dataLength = 0x08;  // fixed length data only
                    opCodeValue = (UInt16)HCICmds.HCICmdOpcode.HCIExt_SetLocalSupportedFeatures;
                }
                public string cmdName = "HCIExt_SetLocalSupportedFeatures";
                [Description("HCIExt_SetLocalSupportedFeatures")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                public const Byte localFeaturesSize = 8;
                private byte [] _localFeatures = { 01, 00, 00, 00, 00, 00, 00, 00 };
                [Description("Local Features (8 Bytes) - Set the Controller’s Local Supported Features.")]
                [DefaultValueAttribute("01:00:00:00:00:00:00:00")]
                [Serialize (number =1)]
                public byte [] localFeatures
                {
                    get { return _localFeatures; }
                    set { _localFeatures = value; }
                }
            }

            /***********************************************************/
            public class HCIExt_SetFastTxRespTime : HCISerializer
            {
                public HCIExt_SetFastTxRespTime() : base ()
                {
                    dataLength = 0x01;  // fixed length data only
                    opCodeValue = (UInt16)HCICmds.HCICmdOpcode.HCIExt_SetFastTxRespTime;
                }
                public string cmdName = "HCIExt_SetFastTxRespTime";
                [Description("HCIExt_SetFastTxRespTime")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private HCIExt_SetFastTxRespTimeCtrl _control = HCICmds.HCIExt_SetFastTxRespTimeCtrl.HCI_EXT_DISABLE_FAST_TX_RESP_TIME;
                [Description("Control (1 Byte) - Enable or disable the fast Tx response time feature.")]
                [DefaultValueAttribute(HCICmds.HCIExt_SetFastTxRespTimeCtrl.HCI_EXT_DISABLE_FAST_TX_RESP_TIME)]
                [Serialize(number = 1)]
                public HCIExt_SetFastTxRespTimeCtrl control
                {
                    get { return _control; }
                    set { _control = value; }
                }
            }

            /***********************************************************/
            public class HCIExt_ModemTestTx : HCISerializer
            {
                public HCIExt_ModemTestTx() : base ()
                {
                    dataLength = 0x02;  // fixed length data only
                    opCodeValue = (UInt16)HCICmds.HCICmdOpcode.HCIExt_ModemTestTx;
                }
                public string cmdName = "HCIExt_ModemTestTx";
                [Description("HCIExt_ModemTestTx")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private HCIExt_CwMode _cwMode = HCICmds.HCIExt_CwMode.HCI_EXT_TX_MODULATED_CARRIER;
                [Description("CW Mode (1 Byte) - Set Modem Test CW modulation.")]
                [DefaultValueAttribute(HCICmds.HCIExt_CwMode.HCI_EXT_TX_MODULATED_CARRIER)]
                [Serialize(number = 1)]
                public HCIExt_CwMode cwMode
                {
                    get { return _cwMode; }
                    set { _cwMode = value; }
                }

                private Byte _txRfChannel = 0x00;
                [Description("Tx RF Channel (1 Byte) - Channel Number 0 to 39")]
                [DefaultValueAttribute((Byte)0x00)]
                [Serialize(number = 2)]
                public Byte txRfChannel
                {
                    get { return _txRfChannel; }
                    set { _txRfChannel = value; }
                }
            }
            /***********************************************************/
            public class HCIExt_ModemHopTestTx : HCISerializer
            {
                public HCIExt_ModemHopTestTx() : base ()
                {
                    dataLength = 0x0;  // fixed length data only
                    opCodeValue = (UInt16)HCICmds.HCICmdOpcode.HCIExt_ModemHopTestTx;
                }
                public string cmdName = "HCIExt_ModemHopTestTx";
                [Description("HCIExt_ModemHopTestTx")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }
            }
            /***********************************************************/
            public class HCIExt_ModemTestRx
            {
                public string cmdName = "HCIExt_ModemTestRx";
                public Byte dataLength = 0x01;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.HCIExt_ModemTestRx;
                [Description("HCIExt_ModemTestRx")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }
                private Byte _rxRfChannel = 0x00;
                [Description("Rx RF Channel (1 Byte) - Channel Number 0 to 39")]
                [DefaultValueAttribute((Byte)0x00)]
                public Byte rxRfChannel
                {
                    get { return _rxRfChannel; }
                    set { _rxRfChannel = value; }
                }
            }
            /***********************************************************/
            public class HCIExt_EndModemTest
            {
                public string cmdName = "HCIExt_EndModemTest";
                public Byte dataLength = 0x00;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.HCIExt_EndModemTest;
                [Description("HCIExt_EndModemTest")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }
            }
            /***********************************************************/
            public class HCIExt_SetBDADDR
            {
                public string cmdName = "HCIExt_SetBDADDR";
                public Byte dataLength = 0x02;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.HCIExt_SetBDADDRCmd;
                [Description("HCIExt_SetBDADDR")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private string _bleDevAddr = HCICmds.EmptyBDAStr;
                [Description("BLE Device Address (6 Bytes) - Set this device’s BLE address")]
                [DefaultValueAttribute(HCICmds.EmptyBDAStr)]
                public string bleDevAddr
                {
                    get { return _bleDevAddr; }
                    set { _bleDevAddr = value; }
                }
            }
        }
        #endregion // HCI Status Commands

        #region L2CAP Commands
        /***********************************************************/
        public class L2CAPCmds
        /***********************************************************/
        {
            /***********************************************************/
            public class L2CAP_InfoReq
            {
                public string cmdName = "L2CAP_InfoReq";
                public Byte dataLength = 0x04;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.L2CAP_InfoReq;
                [Description("L2CAP_InfoReq")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private L2CAP_InfoTypes _infoType = L2CAP_InfoTypes.EXTENDED_FEATURES;
                [Description("Info Type (2 Bytes) - The type of implementation specific information being requested")]
                [DefaultValueAttribute(L2CAP_InfoTypes.EXTENDED_FEATURES)]
                public L2CAP_InfoTypes infoType
                {
                    get { return _infoType; }
                    set { _infoType = value; }
                }
            }

            /***********************************************************/
            public class L2CAP_ConnParamUpdateReq
            {
                public string cmdName = "L2CAP_ConnParamUpdateReq";
                public Byte dataLength = 0x0A;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.L2CAP_ConnParamUpdateReq;
                [Description("L2CAP_ConnParamUpdateReq")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private UInt16 _intervalMin = (UInt16)80;
                private const string _intervalMin_default = "80";
                [Description("Interval Min (2 Bytes) - The minimum value for the connection event interval")]
                [DefaultValueAttribute(typeof(UInt16), _intervalMin_default)]
                public UInt16 intervalMin
                {
                    get { return _intervalMin; }
                    set { _intervalMin = value; }
                }

                private UInt16 _intervalMax = (UInt16)160;
                private const string _intervalMax_default = "160";
                [Description("Interval Max (2 Bytes) - The maximum value for the connection event interval")]
                [DefaultValueAttribute(typeof(UInt16), _intervalMax_default)]
                public UInt16 intervalMax
                {
                    get { return _intervalMax; }
                    set { _intervalMax = value; }
                }

                private UInt16 _slaveLatency = (UInt16)0;
                private const string _slaveLatency_default = "0";
                [Description("Slave Latency (2 Bytes) - The slave latency parameter")]
                [DefaultValueAttribute(typeof(UInt16), _slaveLatency_default)]
                public UInt16 slaveLatency
                {
                    get { return _slaveLatency; }
                    set { _slaveLatency = value; }
                }

                private UInt16 _timeoutMultiplier = (UInt16)1000;
                private const string _timeoutMultiplier_default = "1000";
                [Description("Timeout Multiplier (2 Bytes) - The connection timeout parameter")]
                [DefaultValueAttribute(typeof(UInt16), _timeoutMultiplier_default)]
                public UInt16 timeoutMultiplier
                {
                    get { return _timeoutMultiplier; }
                    set { _timeoutMultiplier = value; }
                }
            }
        }
        #endregion // L2CAP Commands

        #region ATT Commands
        /***********************************************************/
        public class ATTCmds
        /***********************************************************/
        {
            #region ATT_ErrorRsp()
            /***********************************************************/
            public class ATT_ErrorRsp
            {
                public string cmdName = "ATT_ErrorRsp";
                public Byte dataLength = 0x06;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ErrorRsp;
                [Description("ATT_ErrorRsp")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private Byte _reqOpcode = 0x00;
                [Description("Req Opcode (1 Byte) - The request that generated this error response")]
                [DefaultValueAttribute((Byte)0x00)]
                public Byte reqOpcode
                {
                    get { return _reqOpcode; }
                    set { _reqOpcode = value; }
                }

                private UInt16 _handle = 0x0000;
                private const string _handle_default = "0x0000";
                [Description("Handle (2 Bytes) - The attribute handle that generated this error response")]
                [DefaultValueAttribute(typeof(UInt16), _handle_default)]
                public UInt16 handle
                {
                    get { return _handle; }
                    set { _handle = value; }
                }

                private HCI_ErrorRspCodes _errorCode = HCI_ErrorRspCodes.ATTR_NOT_FOUND;
                [Description("ErrorCode (1 Byte) - The reason why the request has generated an error response")]
                [DefaultValueAttribute(HCI_ErrorRspCodes.ATTR_NOT_FOUND)]
                public HCI_ErrorRspCodes errorCode
                {
                    get { return _errorCode; }
                    set { _errorCode = value; }
                }
            }
            #endregion // ATT_ErrorRsp()

            #region ATT_ExchangeMTUReq()
            /***********************************************************/
            public class ATT_ExchangeMTUReq
            {
                public string cmdName = "ATT_ExchangeMTUReq";
                public Byte dataLength = 0x04;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ExchangeMTUReq;
                [Description("ATT_ExchangeMTUReq")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private UInt16 _clientRxMTU = (UInt16)23;
                private const string _clientRxMTU_default = "23";
                [Description("Client Rx MTU (2 Bytes) - Attribute client receive MTU size")]
                [DefaultValueAttribute(typeof(UInt16), _clientRxMTU_default)]
                public UInt16 clientRxMTU
                {
                    get { return _clientRxMTU; }
                    set { _clientRxMTU = value; }
                }
            }
            #endregion // ATT_ExchangeMTUReq()

            #region ATT_ExchangeMTURsp()
            /***********************************************************/
            public class ATT_ExchangeMTURsp
            {
                public string cmdName = "ATT_ExchangeMTURsp";
                public Byte dataLength = 0x04;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ExchangeMTURsp;
                [Description("ATT_ExchangeMTURsp")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private UInt16 _serverRxMTU = (UInt16)23;
                private const string _serverRxMTU_default = "23";
                [Description("Server Rx MTU (2 Bytes) - Attribute server receive MTU size")]
                [DefaultValueAttribute(typeof(UInt16), _serverRxMTU_default)]
                public UInt16 serverRxMTU
                {
                    get { return _serverRxMTU; }
                    set { _serverRxMTU = value; }
                }
            }
            #endregion // ATT_ExchangeMTURsp()

            #region ATT_FindInfoReq()
            /***********************************************************/
            public class ATT_FindInfoReq
            {
                public string cmdName = "ATT_FindInfoReq";
                public Byte dataLength = 0x06;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_FindInfoReq;
                [Description("ATT_FindInfoReq")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private UInt16 _startHandle = 0x0001;
                private const string _startHandle_default = "0x0001";
                [Description("Start Handle (2 Bytes) - First requested handle number")]
                [DefaultValueAttribute(typeof(UInt16), _startHandle_default)]
                public UInt16 startHandle
                {
                    get { return _startHandle; }
                    set { _startHandle = value; }
                }

                private UInt16 _endHandle = HCICmds.MaxUInt16;
                private const string _endHandle_default = "0xFFFF";
                [Description("End Handle (2 Bytes) - Last requested handle number")]
                [DefaultValueAttribute(typeof(UInt16), _endHandle_default)]
                public UInt16 endHandle
                {
                    get { return _endHandle; }
                    set { _endHandle = value; }
                }
            }
            #endregion // ATT_FindInfoReq

            #region ATT_FindInfoRsp()
            /***********************************************************/
            public class ATT_FindInfoRsp
            {
                public string cmdName = "ATT_FindInfoRsp";
                public Byte dataLength = 0x03;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_FindInfoRsp;
                [Description("ATT_FindInfoRsp")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private ATT_FindInfoFormat _format = (ATT_FindInfoFormat)(Byte)HCICmds.ATT_FindInfoFormat.HANDLE_BT_UUID_TYPE__handles_and_16_bit_Bluetooth_UUIDs;
                [Description("Format (1 Byte) - The format of the information data")]
                [DefaultValueAttribute((ATT_FindInfoFormat)(Byte)HCICmds.ATT_FindInfoFormat.HANDLE_BT_UUID_TYPE__handles_and_16_bit_Bluetooth_UUIDs)]
                public ATT_FindInfoFormat format
                {
                    get { return _format; }
                    set { _format = value; }
                }

                private string _info = "00:00:00:00";
                [Description("Info (x Bytes) - The information data whose format is determined by the format field")]
                [DefaultValueAttribute("00:00:00:00")]
                public string info
                {
                    get { return _info; }
                    set { _info = value; }
                }
            }
            #endregion // ATT_FindInfoRsp

            #region ATT_FindByTypeValueReq()
            /***********************************************************/
            public class ATT_FindByTypeValueReq
            {
                public string cmdName = "ATT_FindByTypeValueReq";
                public Byte dataLength = 0x06;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_FindByTypeValueReq;
                [Description("ATT_FindByTypeValueReq")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private UInt16 _startHandle = 0x0001;
                private const string _startHandle_default = "0x0001";
                [Description("Start Handle (2 Bytes) - The start handle")]
                [DefaultValueAttribute(typeof(UInt16), _startHandle_default)]
                public UInt16 startHandle
                {
                    get { return _startHandle; }
                    set { _startHandle = value; }
                }

                private UInt16 _endHandle = HCICmds.MaxUInt16;
                private const string _endHandle_default = "0xFFFF";
                [Description("End Handle (2 Bytes) - The end handle")]
                [DefaultValueAttribute(typeof(UInt16), _endHandle_default)]
                public UInt16 endHandle
                {
                    get { return _endHandle; }
                    set { _endHandle = value; }
                }

                private string _type = HCICmds.Empty2BytesStr;
                [Description("Type (2 Bytes) - 'XX:XX' The UUID to find")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string type
                {
                    get { return _type; }
                    set { _type = value; }
                }

                private string _value = HCICmds.Empty2BytesStr;
                [Description("Value (x Bytes) - The attribute value to find")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string value
                {
                    get { return _value; }
                    set { _value = value; }
                }
            }
            #endregion // ATT_FindByTypeValueReq()

            #region ATT_FindByTypeValueRsp()
            /***********************************************************/
            public class ATT_FindByTypeValueRsp
            {
                public string cmdName = "ATT_FindByTypeValueRsp";
                public Byte dataLength = 0x02;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_FindByTypeValueRsp;
                [Description("ATT_FindByTypeValueRsp")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private string _handlesInfo = HCICmds.Empty2BytesStr;
                [Description("Handles Info (1 or more handles info) - 'XX:XX'...'XX:XX'")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string handlesInfo
                {
                    get { return _handlesInfo; }
                    set { _handlesInfo = value; }
                }
            }
            #endregion // ATT_FindByTypeValueRsp()

            #region ATT_ReadByTypeReq()
            /***********************************************************/
            public class ATT_ReadByTypeReq
            {
                public string cmdName = "ATT_ReadByTypeReq";
                public Byte dataLength = 0x06;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ReadByTypeReq;
                [Description("ATT_ReadByTypeReq")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private UInt16 _startHandle = 0x0001;
                private const string _startHandle_default = "0x0001";
                [Description("Start Handle (2 Bytes) - The start handle where values will be read")]
                [DefaultValueAttribute(typeof(UInt16), _startHandle_default)]
                public UInt16 startHandle
                {
                    get { return _startHandle; }
                    set { _startHandle = value; }
                }

                private UInt16 _endHandle = HCICmds.MaxUInt16;
                private const string _endHandle_default = "0xFFFF";
                [Description("End Handle (2 Bytes) - The end handle of where values will be read")]
                [DefaultValueAttribute(typeof(UInt16), _endHandle_default)]
                public UInt16 endHandle
                {
                    get { return _endHandle; }
                    set { _endHandle = value; }
                }

                private string _type = HCICmds.Empty2BytesStr;
                [Description("Type (2 or 16 Bytes) - 2 or 16 octet UUID")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string type
                {
                    get { return _type; }
                    set { _type = value; }
                }
            }
            #endregion // ATT_ReadByTypeReq()

            #region ATT_ReadByTypeRsp()
            /***********************************************************/
            public class ATT_ReadByTypeRsp
            {
                public string cmdName = "ATT_ReadByTypeRsp";
                public Byte dataLength = 0x03;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ReadByTypeRsp;
                [Description("ATT_ReadByTypeRsp")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private Byte _length = (Byte)0x00;
                [Description("Length (1 Byte) - The size of each attribute handle-value pair")]
                [DefaultValueAttribute((Byte)0x00)]
                public Byte length
                {
                    get { return _length; }
                    set { _length = value; }
                }

                private string _dataList = HCICmds.Empty2BytesStr;
                [Description("Data List (x Bytes) - A list of Attribute Data (handle-value pairs)")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string dataList
                {
                    get { return _dataList; }
                    set { _dataList = value; }
                }
            }
            #endregion // ATT_ReadByTypeRsp()

            #region ATT_ReadReq()
            /***********************************************************/
            public class ATT_ReadReq
            {
                public string cmdName = "ATT_ReadReq";
                public Byte dataLength = 0x04;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ReadReq;
                [Description("ATT_ReadReq")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private UInt16 _handle = 0x0000;
                private const string _handle_default = "0x0000";
                [Description("Handle (2 Bytes) - The handle of the attribute to be read")]
                [DefaultValueAttribute(typeof(UInt16), _handle_default)]
                public UInt16 handle
                {
                    get { return _handle; }
                    set { _handle = value; }
                }
            }
            #endregion // ATT_ReadReq

            #region ATT_ReadRsp()
            /***********************************************************/
            public class ATT_ReadRsp
            {
                public string cmdName = "ATT_ReadRsp";
                public Byte dataLength = 0x02;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ReadRsp;
                [Description("ATT_ReadRsp")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private string _value = HCICmds.Empty2BytesStr;
                [Description("Value (x Bytes) - The value of the attribute with the handle given")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string value
                {
                    get { return _value; }
                    set { _value = value; }
                }
            }
            #endregion // ATT_ReadRsp

            #region ATT_ReadBlobReq()
            /***********************************************************/
            public class ATT_ReadBlobReq
            {
                public string cmdName = "ATT_ReadBlobReq";
                public Byte dataLength = 0x06;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ReadBlobReq;
                [Description("ATT_ReadBlobReq")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private UInt16 _handle = 0x0000;
                private const string _handle_default = "0x0000";
                [Description("Handle (2 Bytes) - The handle of the attribute to be read")]
                [DefaultValueAttribute(typeof(UInt16), _handle_default)]
                public UInt16 handle
                {
                    get { return _handle; }
                    set { _handle = value; }
                }

                private UInt16 _offset = 0x0000;
                private const string _offset_default = "0x0000";
                [Description("Offset (2 Bytes) - The offset of the first octect to be read")]
                [DefaultValueAttribute(typeof(UInt16), _offset_default)]
                public UInt16 offset
                {
                    get { return _offset; }
                    set { _offset = value; }
                }
            }
            #endregion  // ATT_ReadBlobReq

            #region ATT_ReadBlobRsp()
            /***********************************************************/
            public class ATT_ReadBlobRsp
            {
                public string cmdName = "ATT_ReadBlobRsp";
                public Byte dataLength = 0x02;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ReadBlobRsp;
                [Description("ATT_ReadBlobRsp")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private string _value = HCICmds.Empty2BytesStr;
                [Description("Value (x Bytes) - The value of the attribute with the handle given")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string value
                {
                    get { return _value; }
                    set { _value = value; }
                }
            }
            #endregion  // ATT_ReadBlobRsp

            #region ATT_ReadMultiReq()
            /***********************************************************/
            public class ATT_ReadMultiReq
            {
                public string cmdName = "ATT_ReadMultiReq";
                public Byte dataLength = 0x02;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ReadMultiReq;
                [Description("ATT_ReadMultiReq")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private string _handles = "0x0000;0x0000";
                [Description("Handles (2 Bytes for each handle, seperated by ';') - The handles of the attributes")]
                [DefaultValueAttribute("0x0000;0x0000")]
                public string handles
                {
                    get { return _handles; }
                    set { _handles = value; }
                }
            }
            #endregion // ATT_ReadMultiReq

            #region ATT_ReadMultiRsp()
            /***********************************************************/
            public class ATT_ReadMultiRsp
            {
                public string cmdName = "ATT_ReadMultiRsp";
                public Byte dataLength = 0x02;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ReadMultiRsp;
                [Description("ATT_ReadMultiRsp")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private string _values = HCICmds.Empty2BytesStr;
                [Description("Values (x Bytes) - The values of the attribute with the handle given")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string values
                {
                    get { return _values; }
                    set { _values = value; }
                }
            }
            #endregion // ATT_ReadMultiRsp

            #region ATT_ReadByGrpTypeReq()
            /***********************************************************/
            public class ATT_ReadByGrpTypeReq
            {
                public string cmdName = "ATT_ReadByGrpTypeReq";
                public Byte dataLength = 0x06;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ReadByGrpTypeReq;
                [Description("ATT_ReadByGrpTypeReq")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private UInt16 _startHandle = 0x0000;
                private const string _startHandle_default = "0x0000";
                [Description("Start Handle (2 Bytes) - The start handle where values will be read")]
                [DefaultValueAttribute(typeof(UInt16), _startHandle_default)]
                public UInt16 startHandle
                {
                    get { return _startHandle; }
                    set { _startHandle = value; }
                }

                private UInt16 _endHandle = HCICmds.MaxUInt16;
                private const string _endHandle_default = "0xFFFF";
                [Description("End Handle (2 Bytes) - The end handle of where values will be read")]
                [DefaultValueAttribute(typeof(UInt16), _endHandle_default)]
                public UInt16 endHandle
                {
                    get { return _endHandle; }
                    set { _endHandle = value; }
                }

                private string _groupType = HCICmds.Empty2BytesStr;
                [Description("Group Type (2 or 16 Bytes) - 2 or 16 octet UUID")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string groupType
                {
                    get { return _groupType; }
                    set { _groupType = value; }
                }
            }
            #endregion // ATT_ReadByGrpTypeReq

            #region ATT_ReadByGrpTypeRsp()
            /***********************************************************/
            public class ATT_ReadByGrpTypeRsp
            {
                public string cmdName = "ATT_ReadByGrpTypeRsp";
                public Byte dataLength = 0x03;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ReadByGrpTypeRsp;
                [Description("ATT_ReadByGrpTypeRsp")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private Byte _length = 0x00;
                [Description("Length (1 Byte) - The size of each Attribute Data (attribute handle, end group handle and attribute value set)")]
                [DefaultValueAttribute((Byte)0x00)]
                public Byte length
                {
                    get { return _length; }
                    set { _length = value; }
                }

                private string _dataList = HCICmds.Empty2BytesStr;
                [Description("DataList (x Bytes) - 'XX:XX...' - A list of Attribute Data (attribute handle, end group handle and attribute value sets)")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string dataList
                {
                    get { return _dataList; }
                    set { _dataList = value; }
                }
            }
            #endregion // ATT_ReadByGrpTypeRsp

            #region ATT_WriteReq()
            /***********************************************************/
            public class ATT_WriteReq
            {
                public string cmdName = "ATT_WriteReq";
                public Byte dataLength = 0x06;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_WriteReq;
                [Description("ATT_WriteReq")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private GAP_YesNo _signature = GAP_YesNo.No;
                [Description("Signature (1 Byte) - Include Authentication Signature")]
                [DefaultValueAttribute(GAP_YesNo.No)]
                public GAP_YesNo signature
                {
                    get { return _signature; }
                    set { _signature = value; }
                }

                private GAP_YesNo _command = GAP_YesNo.No;
                [Description("Command (1 Byte) - This is the Write Command")]
                [DefaultValueAttribute(GAP_YesNo.No)]
                public GAP_YesNo command
                {
                    get { return _command; }
                    set { _command = value; }
                }

                private UInt16 _handle = 0x0000;
                private const string _handle_default = "0x0000";
                [Description("Handle (2 Bytes) - The handle of the attribute")]
                [DefaultValueAttribute(typeof(UInt16), _handle_default)]
                public UInt16 handle
                {
                    get { return _handle; }
                    set { _handle = value; }
                }

                private string _value = "00";
                [Description("Value (x Bytes)- The value of the attribute")]
                [DefaultValueAttribute("00")]
                public string value
                {
                    get { return _value; }
                    set { _value = value; }
                }
            }
            #endregion // ATT_WriteReq

            #region ATT_WriteRsp()
            /***********************************************************/
            public class ATT_WriteRsp
            {
                public string cmdName = "ATT_WriteRsp";
                public Byte dataLength = 0x02;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_WriteRsp;
                [Description("ATT_WriteRsp")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }
            }
            #endregion // ATT_WriteRsp

            #region ATT_PrepareWriteReq()
            /***********************************************************/
            public class ATT_PrepareWriteReq
            {
                public string cmdName = "ATT_PrepareWriteReq";
                public Byte dataLength = 0x06;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_PrepareWriteReq;
                [Description("ATT_PrepareWriteReq")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private UInt16 _handle = 0x0000;
                private const string _handle_default = "0x0000";
                [Description("Handle (2 Bytes) - The handle of the attribute to be written")]
                [DefaultValueAttribute(typeof(UInt16), _handle_default)]
                public UInt16 handle
                {
                    get { return _handle; }
                    set { _handle = value; }
                }

                private UInt16 _offset = 0x0000;
                private const string _offset_default = "0x0000";
                [Description("Offset (2 Bytes) - The offset of the first octet to be written")]
                [DefaultValueAttribute(typeof(UInt16), _offset_default)]
                public UInt16 offset
                {
                    get { return _offset; }
                    set { _offset = value; }
                }

                private string _value = HCICmds.Empty2BytesStr;
                [Description("Value (x Bytes) - Part of the value of the attribute to be written")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string value
                {
                    get { return _value; }
                    set { _value = value; }
                }
            }
            #endregion

            #region ATT_PrepareWriteRsp()
            /***********************************************************/
            public class ATT_PrepareWriteRsp
            {
                public string cmdName = "ATT_PrepareWriteRsp";
                public Byte dataLength = 0x06;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_PrepareWriteRsp;
                [Description("ATT_PrepareWriteRsp")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private UInt16 _handle = 0x0000;
                private const string _handle_default = "0x0000";
                [Description("Handle (2 Bytes) - The handle of the attribute to be written")]
                [DefaultValueAttribute(typeof(UInt16), _handle_default)]
                public UInt16 handle
                {
                    get { return _handle; }
                    set { _handle = value; }
                }

                private UInt16 _offset = 0x0000;
                private const string _offset_default = "0x0000";
                [Description("Offset (2 Bytes) - The offset of the first octet to be written")]
                [DefaultValueAttribute(typeof(UInt16), _offset_default)]
                public UInt16 offset
                {
                    get { return _offset; }
                    set { _offset = value; }
                }

                private string _value = HCICmds.Empty2BytesStr;
                [Description("Value (x Bytes) - Part of the value of the attribute to be written")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string value
                {
                    get { return _value; }
                    set { _value = value; }
                }
            }
            #endregion

            #region ATT_ExecuteWriteReq()
            /***********************************************************/
            public class ATT_ExecuteWriteReq
            {
                public string cmdName = "ATT_ExecuteWriteReq";
                public Byte dataLength = 0x03;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ExecuteWriteReq;
                [Description("ATT_ExecuteWriteReq")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private ATT_ExecuteWriteFlags _flags = ATT_ExecuteWriteFlags.Cancel_all_prepared_writes;
                [Description("Flags (1 Byte) - Cancel or Write all values in the queue from this client")]
                [DefaultValueAttribute(ATT_ExecuteWriteFlags.Cancel_all_prepared_writes)]
                public ATT_ExecuteWriteFlags flags
                {
                    get { return _flags; }
                    set { _flags = value; }
                }
            }
            #endregion

            #region ATT_ExecuteWriteRsp()
            /***********************************************************/
            public class ATT_ExecuteWriteRsp
            {
                public string cmdName = "ATT_ExecuteWriteRsp";
                public Byte dataLength = 0x02;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_ExecuteWriteRsp;
                [Description("ATT_ExecuteWriteRsp")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }
            }
            #endregion

            #region ATT_HandleValueNotification()
            /***********************************************************/
            public class ATT_HandleValueNotification
            {
                public string cmdName = "ATT_HandleValueNotification";
                public Byte dataLength = 0x05;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_HandleValueNotification;
                [Description("ATT_HandleValueNotification")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private GAP_YesNo _authenticated = GAP_YesNo.No;
                [Description("Authenticated (1 Byte) - Whether or not an authenticated link is required")]
                [DefaultValueAttribute(GAP_YesNo.No)]
                public GAP_YesNo authenticated
                {
                    get { return _authenticated; }
                    set { _authenticated = value; }
                }

                private UInt16 _handle = 0x0000;
                private const string _handle_default = "0x0000";
                [Description("Handle (2 Bytes) - The handle of the attribute")]
                [DefaultValueAttribute(typeof(UInt16), _handle_default)]
                public UInt16 handle
                {
                    get { return _handle; }
                    set { _handle = value; }
                }

                private string _value = "00";
                [Description("Value (x Bytes) - The value of the attribute")]
                [DefaultValueAttribute("00")]
                public string value
                {
                    get { return _value; }
                    set { _value = value; }
                }
            }
            #endregion // ATT_HandleValueNotification

            #region ATT_HandleValueIndication()
            /***********************************************************/
            public class ATT_HandleValueIndication
            {
                public string cmdName = "ATT_HandleValueIndication";
                public Byte dataLength = 0x05;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_HandleValueIndication;
                [Description("ATT_HandleValueIndication")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private GAP_YesNo _authenticated = GAP_YesNo.No;
                [Description("Authenticated (1 Byte) - Whether or not an authenticated link is required")]
                [DefaultValueAttribute(GAP_YesNo.No)]
                public GAP_YesNo authenticated
                {
                    get { return _authenticated; }
                    set { _authenticated = value; }
                }

                private UInt16 _handle = 0x0000;
                private const string _handle_default = "0x0000";
                [Description("Handle (2 Bytes) - The handle of the attribute")]
                [DefaultValueAttribute(typeof(UInt16), _handle_default)]
                public UInt16 handle
                {
                    get { return _handle; }
                    set { _handle = value; }
                }

                private string _value = "00";
                [Description("Value (x Bytes)- The value of the attribute")]
                [DefaultValueAttribute("00")]
                public string value
                {
                    get { return _value; }
                    set { _value = value; }
                }
            }
            #endregion // ATT_HandleValueIndication

            #region ATT_HandleValueConfirmation()
            /***********************************************************/
            public class ATT_HandleValueConfirmation
            {
                public string cmdName = "ATT_HandleValueConfirmation";
                public Byte dataLength = 0x02;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.ATT_HandleValueConfirmation;
                [Description("ATT_HandleValueConfirmation")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }
            }
            #endregion // ATT_HandleValueConfirmation
        }
        #endregion // ATT Commands

        #region GATT Commands
        /***********************************************************/
        public class GATTCmds
        /***********************************************************/
        {
            /***********************************************************/
            public class GATT_DiscCharsByUUID
            {
                public string cmdName = "GATT_DiscCharsByUUID";
                public Byte dataLength = 0x06;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GATT_DiscCharsByUUID;
                [Description("GATT_DiscCharsByUUID")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private UInt16 _startHandle = 0x0001;
                private const string _startHandle_default = "0x0001";
                [Description("Start Handle (2 Bytes) - The start handle where values will be read")]
                [DefaultValueAttribute(typeof(UInt16), _startHandle_default)]
                public UInt16 startHandle
                {
                    get { return _startHandle; }
                    set { _startHandle = value; }
                }

                private UInt16 _endHandle = HCICmds.MaxUInt16;
                private const string _endHandle_default = "0xFFFF";
                [Description("End Handle (2 Bytes) - The end handle of where values will be read")]
                [DefaultValueAttribute(typeof(UInt16), _endHandle_default)]
                public UInt16 endHandle
                {
                    get { return _endHandle; }
                    set { _endHandle = value; }
                }

                private string _type = HCICmds.Empty2BytesStr;
                [Description("Type (2 or 16 Bytes) - 2 or 16 octet UUID")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string type
                {
                    get { return _type; }
                    set { _type = value; }
                }
            }
            #region GATT_WriteLong()
            /***********************************************************/
            public class GATT_WriteLong
            {
                public string cmdName = "GATT_WriteLong";
                public Byte dataLength = 0x06;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GATT_WriteLong;
                [Description("GATT_WriteLong")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                private UInt16 _handle = 0x0000;
                private const string _handle_default = "0x0000";
                [Description("Handle (2 Bytes) - The handle of the attribute to be written")]
                [DefaultValueAttribute(typeof(UInt16), _handle_default)]
                public UInt16 handle
                {
                    get { return _handle; }
                    set { _handle = value; }
                }

                private UInt16 _offset = 0x0000;
                private const string _offset_default = "0x0000";
                [Description("Offset (2 Bytes) - The offset of the first octet to be written")]
                [DefaultValueAttribute(typeof(UInt16), _offset_default)]
                public UInt16 offset
                {
                    get { return _offset; }
                    set { _offset = value; }
                }

                private string _value = HCICmds.Empty2BytesStr;
                [Description("Value - The value of the attribute to be written")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string value
                {
                    get { return _value; }
                    set { _value = value; }
                }
            }
            #endregion

            #region GATT_AddService()
            /***********************************************************/
            public class GATT_AddService
            {
                public string cmdName = "GATT_AddService";
                public Byte dataLength = 0x04;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GATT_AddService;
                [Description("GATT_AddService")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private GATT_ServiceUUID _uuid = GATT_ServiceUUID.PrimaryService;
                [Description("UUID (2 Bytes)")]
                [DefaultValueAttribute(GATT_ServiceUUID.PrimaryService)]
                public GATT_ServiceUUID uuid
                {
                    get { return _uuid; }
                    set { _uuid = value; }
                }

                private UInt16 _numAttrs = 2;
                private const string _numAttrs_default = "2";
                [Description("Num Attrs (2 Bytes) - The number attributes in the service (including the service attribute)")]
                [DefaultValueAttribute(typeof(UInt16), _numAttrs_default)]
                public UInt16 numAttrs
                {
                    get { return _numAttrs; }
                    set { _numAttrs = value; }
                }
            }
            #endregion

            #region GATT_DelService()
            /***********************************************************/
            public class GATT_DelService
            {
                public string cmdName = "GATT_DelService";
                public Byte dataLength = 0x02;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GATT_DelService;
                [Description("GATT_DelService")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _handle = 0x0000;
                private const string _handle_default = "0x0000";
                [Description("Handle (2 Bytes) - The handle of the service to be deleted")]
                [DefaultValueAttribute(typeof(UInt16), _handle_default)]
                public UInt16 handle
                {
                    get { return _handle; }
                    set { _handle = value; }
                }
            }
            #endregion

            #region GATT_AddAttribute()
            /***********************************************************/
            public class GATT_AddAttribute
            {
                public string cmdName = "GATT_AddAttribute";
                public Byte dataLength = 0x01;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GATT_AddAttribute;
                [Description("GATT_AddAttribute")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private string _uuid = HCICmds.Empty2BytesStr;
                [Description("UUID (x Bytes) - The type of the attribute to be added")]
                [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
                public string uuid
                {
                    get { return _uuid; }
                    set { _uuid = value; }
                }

                // this is a bit field
                private Byte _permissions = 0x01;
                [Description("Permissions (1 Byte) - Bit mask - Attribute permissions")]
                [DefaultValueAttribute((Byte)0x01)]
                public Byte permissions
                {
                    get { return _permissions; }
                    set { _permissions = value; }
                }
            }
            #endregion
        }
        #endregion // GATT Commands

        #region GAP Commands
        /***********************************************************/
        public class GAPCmds
        /***********************************************************/
        {
            #region GAP_DeviceInit
            /***********************************************************/
            public class GAP_DeviceInit : HCISerializer
            {
                public GAP_DeviceInit() : base()
                {
                    dataLength = 0x06;  // fixed length data only
                    opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_DeviceInit;
                }

                public string cmdName = "GAP_DeviceInit";

                //public Byte dataLength = 0x06;  // fixed length data only
                //public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_DeviceInit;


                [Description("GAP_DeviceInit")]
                public string opCode
                {
                    get { return ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private GAP_Profile _profileRole = GAP_Profile.Central;
                [Description("Profile Role (1 Byte) - Bit Mask - GAP profile role")]
                [DefaultValueAttribute(GAP_Profile.Central)]
                [Category("ProfileRole")]
                [Serialize(number = 1)]
                public GAP_Profile profileRole
                {
                    get { return _profileRole; }
                    set { _profileRole = value; }
                }

                private Byte _maxScanResponses = 0x20;
                [Description("Max Scan Responses (1 Byte) - The maximun can responses we can receive during a device discovery.")]
                [DefaultValueAttribute((Byte)0x20)]
                [Serialize(number = 2)]
                public Byte maxScanResponses
                {
                    get { return _maxScanResponses; }
                    set { _maxScanResponses = value; }
                }

                public const Byte irkSize = 16;
                private byte[] _irk = HCICmds.Empty16Bytes;
                [Description("IRK (16 Bytes) - Identify Resolving Key - 0 if generate the key ")]
                [DefaultValueAttribute(HCICmds.Empty16BytesStr)]
                [Serialize(number = 3)]
                public byte[] irk
                {
                    get { return _irk; }
                    set { _irk = value; }
                }

                public const Byte csrkSize = 16;
                private byte[] _csrk = HCICmds.Empty16Bytes;
                [Description("CSRK (16 Bytes) - Connection Signature Resolving Key - 0 if generate the key ")]
                [DefaultValueAttribute(HCICmds.Empty16BytesStr)]
                [Serialize(number = 4)]
                public byte[] csrk
                {
                    get { return _csrk; }
                    set { _csrk = value; }
                }

                private UInt32 _signCounter = (UInt32)1;
                private const string _signCounter_default = "1";
                [Description("Signature Counter (4 Bytes) - 32 bit Signature Counter")]
                [DefaultValueAttribute(typeof(UInt32), _signCounter_default)]
                [Serialize(number = 5)]
                public UInt32 signCounter
                {
                    get { return _signCounter; }
                    set { _signCounter = value; }
                }
            }
        }
        #endregion  // GAP_DeviceInit

        #region GAP_ConfigDeviceAddr
        /***********************************************************/
        public class GAP_ConfigDeviceAddr
        {
            public string cmdName = "GAP_ConfigDeviceAddr";
            public Byte dataLength = 0x07;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_ConfigDeviceAddr;
            [Description("GAP_ConfigDeviceAddr")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private GAP_AddrType _addrType = GAP_AddrType.Public;
            [Description("Addr Type (1 Byte) - Address type")]
            [DefaultValueAttribute(GAP_AddrType.Public)]
            public GAP_AddrType addrType
            {
                get { return _addrType; }
                set { _addrType = value; }
            }

            private string _addr = HCICmds.EmptyBDAStr;
            [Description("Addr (6 Bytes) - BDA of the intended address")]
            [DefaultValueAttribute(HCICmds.EmptyBDAStr)]
            public string addr
            {
                get { return _addr; }
                set { _addr = value; }
            }
        }
        #endregion  // GAP_ConfigDeviceAddr

        #region GAP_DeviceDiscoveryRequest
        /***********************************************************/
        public class GAP_DeviceDiscoveryRequest : HCISerializer
        {
            public GAP_DeviceDiscoveryRequest() : base()
            {
                dataLength = 0x03;  // fixed length data only
                opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_DeviceDiscoveryRequest;
            }
            public string cmdName = "GAP_DeviceDiscoveryRequest";
            [Description("GAP_DeviceDiscoveryRequest")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private GAP_DiscoveryMode _mode = GAP_DiscoveryMode.All;
            [Description("Mode (1 Byte) - Discovery Mode")]
            [DefaultValueAttribute(GAP_DiscoveryMode.All)]
            [Category("Mode")]
            [Serialize(number = 1)]
            public GAP_DiscoveryMode mode
            {
                get { return _mode; }
                set { _mode = value; }
            }

            private GAP_EnableDisable _nameMode = GAP_EnableDisable.Enable;
            [Description("Name Mode (1 Byte) - Name Mode Enable/Disable")]
            [DefaultValueAttribute(GAP_EnableDisable.Enable)]
            [Category("NameMode")]
            [Serialize(number = 2)]
            public GAP_EnableDisable nameMode
            {
                get { return _nameMode; }
                set { _nameMode = value; }
            }

            private GAP_EnableDisable _whiteList = GAP_EnableDisable.Disable;
            [Description("White List (1 byte) - White List Enable/Disable - Enabled to only allow advertisements from devices in the white list.")]
            [DefaultValueAttribute(GAP_EnableDisable.Disable)]
            [Category("White List")]
            [Serialize(number = 3)]
            public GAP_EnableDisable whiteList
            {
                get { return _whiteList; }
                set { _whiteList = value; }
            }
        }
        #endregion  // GAP_DeviceDiscoveryRequest

        #region GAP_DeviceDiscoveryCancel
        /***********************************************************/
        public class GAP_DeviceDiscoveryCancel : HCISerializer
        {
            public GAP_DeviceDiscoveryCancel() : base()
            {
                dataLength = 0x00;  // fixed length data only
                opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_DeviceDiscoveryCancel;
            }
            public string cmdName = "GAP_DeviceDiscoveryCancel";

            [Description("GAP_DeviceDiscoveryCancel - Cancel the current device discovery")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }
        }
        #endregion  // GAP_DeviceDiscoveryCancel

        #region GAP_MakeDiscoverable
        /***********************************************************/
        public class GAP_MakeDiscoverable
        {
            public string cmdName = "GAP_MakeDiscoverable";
            public Byte dataLength = 0x09;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_MakeDiscoverable;
            [Description("GAP_MakeDiscoverable")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }
            private GAP_EventType _eventType = GAP_EventType.GAP_EVENT_CONN_UNDIRECT_AD;
            [Description("Event Type (1 Byte) - Advertising event type")]
            [DefaultValueAttribute(GAP_EventType.GAP_EVENT_CONN_UNDIRECT_AD)]
            public GAP_EventType eventType
            {
                get { return _eventType; }
                set { _eventType = value; }
            }

            private GAP_AddrType _initiatorAddrType = GAP_AddrType.Public;
            [Description("Initiator Address Type (1 Byte) - Address type")]
            [DefaultValueAttribute(GAP_AddrType.Public)]
            public GAP_AddrType initiatorAddrType
            {
                get { return _initiatorAddrType; }
                set { _initiatorAddrType = value; }
            }

            public const Byte initiatorAddrSize = 6;
            private string _initiatorAddr = HCICmds.EmptyBDAStr;
            [Description("Initiator's Address (6 Bytes) - BDA of the Initiator")]
            [DefaultValueAttribute(HCICmds.EmptyBDAStr)]
            public string initiatorAddr
            {
                get { return _initiatorAddr; }
                set { _initiatorAddr = value; }
            }

            // this is a bit field
            private Byte _channelMap = 0x07;
            [Description("Channel Map (1 Byte) - Bit mask - 0x07 all channels")]
            [DefaultValueAttribute((Byte)0x07)]
            public Byte channelMap
            {
                get { return _channelMap; }
                set { _channelMap = value; }
            }

            private GAP_FilterPolicy _filterPolicy = GAP_FilterPolicy.GAP_FILTER_POLICY_ALL;
            [Description("Filter Policy (1 Byte) - Filer Policy. Ignored when directed advertising is used.")]
            [DefaultValueAttribute(GAP_FilterPolicy.GAP_FILTER_POLICY_ALL)]
            public GAP_FilterPolicy filterPolicy
            {
                get { return _filterPolicy; }
                set { _filterPolicy = value; }
            }
        }
        #endregion  // GAP_MakeDiscoverable

        #region GAP_UpdateAdvertisingData
        /***********************************************************/
        public class GAP_UpdateAdvertisingData
        {
            public string cmdName = "GAP_UpdateAdvertisingData";
            public Byte dataLength = 0x02;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_UpdateAdvertisingData;
            [Description("GAP_UpdateAdvertisingData")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private GAP_AvertAdType _adType = GAP_AvertAdType.GAPADVERT_SCAN_RSP_DATA;
            [Description("Ad Type (1 Byte)")]
            [DefaultValueAttribute(GAP_AvertAdType.GAPADVERT_SCAN_RSP_DATA)]
            public GAP_AvertAdType adType
            {
                get { return _adType; }
                set { _adType = value; }
            }

            private Byte _dataLen = 0x00;
            [Description("DataLen (1 Byte) - The length of the data (0 - 31)")]
            [DefaultValueAttribute((Byte)0x00)]
            public Byte dataLen
            {
                get { return _dataLen; }
                set { _dataLen = value; }
            }

            private string _advertData = "02:01:06";
            [Description("Advert Data (x Bytes) - Raw Advertising Data")]
            [DefaultValueAttribute("02:01:06")]
            public string advertData
            {
                get { return _advertData; }
                set { _advertData = value; }
            }
        }
        #endregion  // GAP_UpdateAdvertisingData

        #region GAP_EndDiscoverable
        /***********************************************************/
        public class GAP_EndDiscoverable
        {
            public string cmdName = "GAP_EndDiscoverable";
            public Byte dataLength = 0x00;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_EndDiscoverable;
            [Description("GAP_EndDiscoverable")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }
        }
        #endregion  // GAP_UpdateAdvertisingData

        #region GAP_EstablishLinkRequest
        /***********************************************************/
        public class GAP_EstablishLinkRequest : HCISerializer
        {
            public GAP_EstablishLinkRequest() : base()
            {
                dataLength = 0x09;  // fixed length data only
                opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_EstablishLinkRequest;
            }
            public string cmdName = "GAP_EstablishLinkRequest";

            [Description("GAP_EstablishLinkRequest")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private GAP_EnableDisable _highDutyCycle = GAP_EnableDisable.Disable;
            [Description("High Duty Cycle (1 Byte) - A Central Device may use high duty cycle scan parameters in order to achieve low latency connection time with a Peripheral device using Directed Link Establishment.")]
            [DefaultValueAttribute(GAP_EnableDisable.Disable)]
            [Serialize(number = 1)]
            public GAP_EnableDisable highDutyCycle
            {
                get { return _highDutyCycle; }
                set { _highDutyCycle = value; }
            }

            private GAP_EnableDisable _whiteList = GAP_EnableDisable.Disable;
            [Description("White List (1 Byte)")]
            [DefaultValueAttribute(GAP_EnableDisable.Disable)]
            [Serialize(number = 2)]
            public GAP_EnableDisable whiteList
            {
                get { return _whiteList; }
                set { _whiteList = value; }
            }

            private GAP_AddrType _addrTypePeer = GAP_AddrType.Public;
            [Description("Addr Type (1 Byte) - Address type")]
            [DefaultValueAttribute(GAP_AddrType.Public)]
            [Serialize(number = 3)]
            public GAP_AddrType addrTypePeer
            {
                get { return _addrTypePeer; }
                set { _addrTypePeer = value; }
            }

            private byte[] _peerAddr = HCICmds.EmptyBDA;
            [Description("Peer's Address (6 Bytes) - BDA of the peer")]
            [DefaultValueAttribute(HCICmds.EmptyBDAStr)]
            [Serialize(number = 4)]
            public byte[] peerAddr
            {
                get { return _peerAddr; }
                set { _peerAddr = value; }
            }
        }
        #endregion  // GAP_EstablishLinkRequest

        #region GAP_TerminateLinkRequest
        /***********************************************************/
        public class GAP_TerminateLinkRequest : HCISerializer
        {
            public GAP_TerminateLinkRequest() : base()
            {
                dataLength = 0x02;  // fixed length data only
                opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_TerminateLinkRequest;
            }
            public string cmdName = "GAP_TerminateLinkRequest";
            [Description("GAP_TerminateLinkRequest")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
            private const string _connHandle_default = "0x0000";
            [Description("Connection Handle (2 Bytes) - Handle of the connection")]
            [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
            [Serialize(number = 1)]
            public UInt16 connHandle
            {
                get { return _connHandle; }
                set { _connHandle = value; }
            }
        }
        #endregion  // GAP_TerminateLinkRequest

        #region GAP_Authenticate
        /***********************************************************/
        public class GAP_Authenticate
        {
            public string cmdName = "GAP_Authenticate";
            public Byte dataLength = 0x1D;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_Authenticate;
            [Description("GAP_Authenticate")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
            private const string _connHandle_default = "0x0000";
            [Description("Connection Handle (2 Bytes) - Handle of the connection")]
            [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
            public UInt16 connHandle
            {
                get { return _connHandle; }
                set { _connHandle = value; }
            }

            private GAP_IOCaps _secReq_ioCaps = GAP_IOCaps.NoInputNoOutput;
            [Description("IOCaps (1 Byte) - Defines the values which are used when exchanging IO capabilities")]
            [DefaultValueAttribute(GAP_IOCaps.NoInputNoOutput)]
            public GAP_IOCaps secReq_ioCaps
            {
                get { return _secReq_ioCaps; }
                set { _secReq_ioCaps = value; }
            }

            private GAP_TrueFalse _secReq_oobAvailable = GAP_TrueFalse.False;
            [Description("OOB Available (1 Byte) - Enable if Out-of-band key available")]
            [DefaultValueAttribute(GAP_TrueFalse.False)]
            public GAP_TrueFalse secReq_oobAvailable
            {
                get { return _secReq_oobAvailable; }
                set { _secReq_oobAvailable = value; }
            }

            public const Byte secReq_oobSize = 16;
            private string _secReq_oob = "4d:9f:88:5a:6e:03:12:fe:00:00:00:00:00:00:00:00";
            [Description("OOB Key (16 Bytes) The OOB key value")]
            [DefaultValueAttribute("4d:9f:88:5a:6e:03:12:fe:00:00:00:00:00:00:00:00")]
            public string secReq_oob
            {
                get { return _secReq_oob; }
                set { _secReq_oob = value; }
            }

            // this is a bit field
            private Byte _secReq_authReq = (Byte)GAP_AuthReq.Bonding;
            [Description("Auth Req (1 Byte) - A bit field that indicates the requested security properties for STK and GAP bonding information.")]
            [DefaultValueAttribute((Byte)GAP_AuthReq.Bonding)]
            public Byte secReq_authReq
            {
                get { return _secReq_authReq; }
                set { _secReq_authReq = value; }
            }

            private Byte _secReq_maxEncKeySize = 16;
            [Description("Max Enc Key Size (16 Bytes) - This value defines the maximum encryption key size in octets\nthat the device can support.  Range: 7 to 16.")]
            [DefaultValueAttribute((Byte)16)]
            public Byte secReq_maxEncKeySize
            {
                get { return _secReq_maxEncKeySize; }
                set { _secReq_maxEncKeySize = value; }
            }

            // this is a bit field
            private Byte _secReq_keyDist = 63;
            [Description("Key Distribution (1 Byte) - The Key Distribution field indicates which keys will be distributed.")]
            [DefaultValueAttribute((Byte)63)]
            public Byte secReq_keyDist
            {
                get { return _secReq_keyDist; }
                set { _secReq_keyDist = value; }
            }

            private GAP_EnableDisable _pairReq_Enable = GAP_EnableDisable.Disable;
            [Description("Pairing Request (1 Byte) - Enable - if Pairing Request has already been received\nand to respond with a Pairing Response.\n This should only be used in a Peripheral device.")]
            [DefaultValueAttribute(GAP_EnableDisable.Disable)]
            public GAP_EnableDisable pairReq_Enable
            {
                get { return _pairReq_Enable; }
                set { _pairReq_Enable = value; }
            }

            private GAP_IOCaps _pairReq_ioCaps = GAP_IOCaps.NoInputNoOutput;
            [Description("IO Capabilities (1 Byte) - Defines the values which are used when exchanging IO capabilities")]
            [DefaultValueAttribute(GAP_IOCaps.NoInputNoOutput)]
            public GAP_IOCaps pairReq_ioCaps
            {
                get { return _pairReq_ioCaps; }
                set { _pairReq_ioCaps = value; }
            }

            private GAP_EnableDisable _pairReq_oobDataFlag = GAP_EnableDisable.Disable;
            [Description("OOB data Flag (1 Byte) - Enable if Out-of-band key available")]
            [DefaultValueAttribute(GAP_EnableDisable.Disable)]
            public GAP_EnableDisable pairReq_oobDataFlag
            {
                get { return _pairReq_oobDataFlag; }
                set { _pairReq_oobDataFlag = value; }
            }

            private Byte _pairReq_authReq = 0x01;
            [Description("Auth Req (1 Byte) - Bit field that indicates the requested security properties\nfor STK and GAP bonding information.")]
            [DefaultValueAttribute((Byte)0x01)]
            public Byte pairReq_authReq
            {
                get { return _pairReq_authReq; }
                set { _pairReq_authReq = value; }
            }

            private Byte _pairReq_maxEncKeySize = 16;
            [Description("Max Enc Key Size (1 Byte) - This value defines the maximun encryption key size in octects\nthat the device can support.")]
            [DefaultValueAttribute((Byte)16)]
            public Byte pairReq_maxEncKeySize
            {
                get { return _pairReq_maxEncKeySize; }
                set { _pairReq_maxEncKeySize = value; }
            }

            // this is a bit field
            private Byte _pairReq_keyDist = 63;
            [Description("Key Dist (1 Byte) - The Key Distribution field indicates which keys will be distributed.")]
            [DefaultValueAttribute((Byte)63)]
            public Byte pairReq_keyDist
            {
                get { return _pairReq_keyDist; }
                set { _pairReq_keyDist = value; }
            }
        }
        #endregion  // GAP_Authenticate

        #region GAP_PasskeyUpdate
        /***********************************************************/
        public class GAP_PasskeyUpdate
        {
            public string cmdName = "GAP_PasskeyUpdate";
            public Byte dataLength = 0x08;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_PasskeyUpdate;
            [Description("GAP_PasskeyUpdate")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
            private const string _connHandle_default = "0x0000";
            [Description("Connection Handle (2 Bytes) - Handle of the connection")]
            [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
            public UInt16 connHandle
            {
                get { return _connHandle; }
                set { _connHandle = value; }
            }

            public const Byte passKeySize = 6;
            private string _passKey = "000000";
            [Description("Pairing Passkey (6 Bytes) - string of numbers 0-9. '019655' is a value of 0x4CC7\n")]
            [DefaultValueAttribute("000000")]
            public string passKey
            {
                get { return _passKey; }
                set { _passKey = value; }
            }
        }
        #endregion // GAP_PasskeyUpdate

        #region GAP_SlaveSecurityRequest
        /***********************************************************/
        public class GAP_SlaveSecurityRequest
        {
            public string cmdName = "GAP_SlaveSecurityRequest";
            public Byte dataLength = 0x03;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_SlaveSecurityRequest;
            [Description("GAP_SlaveSecurityRequest")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
            private const string _connHandle_default = "0x0000";
            [Description("Connection Handle (2 Bytes) - Handle of the connection")]
            [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
            public UInt16 connHandle
            {
                get { return _connHandle; }
                set { _connHandle = value; }
            }

            // this is a bit field
            private Byte _authReq = (Byte)GAP_AuthReq.Bonding;
            [Description("AuthReq (1 Byte) - A bit field that indicates the requested security properties bonding information.")]
            [DefaultValueAttribute((Byte)GAP_AuthReq.Bonding)]
            public Byte authReq
            {
                get { return _authReq; }
                set { _authReq = value; }
            }
        }
        #endregion // GAP_SlaveSecurityRequest

        #region GAP_Signable
        /***********************************************************/
        public class GAP_Signable
        {
            public string cmdName = "GAP_Signable";
            public Byte dataLength = 0x07;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_Signable;
            [Description("GAP_Signable")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
            private const string _connHandle_default = "0x0000";
            [Description("Connection Handle (2 Bytes) - Handle of the connection")]
            [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
            public UInt16 connHandle
            {
                get { return _connHandle; }
                set { _connHandle = value; }
            }

            private GAP_AuthenticatedCsrk _authenticated = GAP_AuthenticatedCsrk.GAP_CSRK_NOT_AUTHENTICATED;
            [Description("Authenticated (1 Byte) - Is the signing information authenticated.")]
            [DefaultValueAttribute(GAP_AuthenticatedCsrk.GAP_CSRK_NOT_AUTHENTICATED)]
            public GAP_AuthenticatedCsrk authenticated
            {
                get { return _authenticated; }
                set { _authenticated = value; }
            }

            public const Byte csrkSize = 16;
            private string _csrk = HCICmds.Empty16BytesStr;
            [Description("CSRK (16 Bytes) - Connection Signature Resolving Key for the connected device")]
            [DefaultValueAttribute(HCICmds.Empty16BytesStr)]
            public string csrk
            {
                get { return _csrk; }
                set { _csrk = value; }
            }

            private UInt32 _signCounter = 0;
            private const string _signCounter_default = "0";
            [Description("Signature Counter (4 Bytes) - Sign Counter for the connected device")]
            [DefaultValueAttribute(typeof(UInt32), _signCounter_default)]
            public UInt32 signCounter
            {
                get { return _signCounter; }
                set { _signCounter = value; }
            }
        }
        #endregion // GAP_Signable

        #region GAP_Bond
        /***********************************************************/
        public class GAP_Bond
        {
            public string cmdName = "GAP_Bond";
            public Byte dataLength = 0x06;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_Bond;
            [Description("GAP_Bond")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
            private const string _connHandle_default = "0x0000";
            [Description("Connection Handle (2 Bytes) - Handle of the connection")]
            [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
            public UInt16 connHandle
            {
                get { return _connHandle; }
                set { _connHandle = value; }
            }

            private GAP_YesNo _authenticated = GAP_YesNo.No;
            [Description("Authenticated (1 Byte) - Yes if the bond was authenticated")]
            [DefaultValueAttribute(GAP_YesNo.No)]
            public GAP_YesNo authenticated
            {
                get { return _authenticated; }
                set { _authenticated = value; }
            }

            public const Byte secInfo_LTKLength = 16;
            private string _secInfo_LTK = "4d:9f:88:5a:6e:03:12:fe:00:00:00:00:00:00:00:00";
            [Description("secInfo_LTK (16 Bytes) - Long Term Key")]
            [DefaultValueAttribute("4d:9f:88:5a:6e:03:12:fe:00:00:00:00:00:00:00:00")]
            public string secInfo_LTK
            {
                get { return _secInfo_LTK; }
                set { _secInfo_LTK = value; }
            }

            private UInt16 _secInfo_DIV = 0x1111;
            private const string _secInfo_DIV_default = "0x1111";
            [Description("secInfo_DIV (2 Bytes) - Diversifier")]
            [DefaultValueAttribute(typeof(UInt16), _secInfo_DIV_default)]
            public UInt16 secInfo_DIV
            {
                get { return _secInfo_DIV; }
                set { _secInfo_DIV = value; }
            }

            public const Byte secInfo_RANDSize = 8;
            private string _secInfo_RAND = "11:22:33:44:55:66:77:88";
            [Description("secInfo_RAND (8 Bytes) - LTK Random pairing")]
            [DefaultValueAttribute("11:22:33:44:55:66:77:88")]
            public string secInfo_RAND
            {
                get { return _secInfo_RAND; }
                set { _secInfo_RAND = value; }
            }

            private Byte _secInfo_LTKSize = 16;
            [Description("secInfo_LTKSize (1 Byte) - LTK Key Size in bytes")]
            [DefaultValueAttribute((Byte)16)]
            public Byte secInfo_LTKSize
            {
                get { return _secInfo_LTKSize; }
                set { _secInfo_LTKSize = value; }
            }
        }
        #endregion // GAP_Bond

        #region GAP_TerminateAuth
        /***********************************************************/
        public class GAP_TerminateAuth
        {
            public string cmdName = "GAP_TerminateAuth";
            public Byte dataLength = 0x03;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_TerminateAuth;
            [Description("GAP_TerminateAuth")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private UInt16 _connHandle = (UInt16)GAP_ConnHandle.Default;
            private const string _connHandle_default = "0x0000";
            [Description("Connection Handle (2 Bytes) - Handle of the connection")]
            [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]
            public UInt16 connHandle
            {
                get { return _connHandle; }
                set { _connHandle = value; }
            }

            private GAP_SMPFailureTypes _reason = GAP_SMPFailureTypes.SMP_PAIRING_FAILED_AUTH_REQ;
            [Description("Reason (1 Byte) - Pairing Failed Message reason field.")]
            [DefaultValueAttribute(GAP_SMPFailureTypes.SMP_PAIRING_FAILED_AUTH_REQ)]
            public GAP_SMPFailureTypes reason
            {
                get { return _reason; }
                set { _reason = value; }
            }
        }
        #endregion // GAP_TerminateAuth

        #region GAP_SetParam
        /***********************************************************/
        public class GAP_SetParam : HCISerializer
        {
            public GAP_SetParam() : base()
            {
                dataLength = 0x03;  // fixed length data only
                opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_SetParam;
            }
            public string cmdName = "GAP_SetParam";
            [Description("GAP_SetParam")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private GAP_ParamId _paramId = GAP_ParamId.TGAP_GEN_DISC_ADV_MIN;
            [Description("Param Id (1 Byte) - GAP parameter ID")]
            [DefaultValueAttribute(GAP_ParamId.TGAP_GEN_DISC_ADV_MIN)]
            [Serialize(number = 1)]
            public GAP_ParamId paramId
            {
                get { return _paramId; }
                set { _paramId = value; }
            }

            private UInt16 _value = 0x0000;
            private const string _value_default = "0x0000";
            [Description("New Value (2 Bytes)")]
            [DefaultValueAttribute(typeof(UInt16), _value_default)]
            [Serialize(number = 2)]
            public UInt16 value
            {
                get { return _value; }
                set { _value = value; }
            }
        }
        #endregion // GAP_SetParam

        #region GAP_GetParam
        /***********************************************************/
        public class GAP_GetParam : HCISerializer
        {
            public GAP_GetParam() : base()
            {
                dataLength = 0x01;  // fixed length data only
                opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_GetParam;
            }
            public string cmdName = "GAP_GetParam";
            [Description("GAP_GetParam")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private GAP_ParamId _paramId = GAP_ParamId.TGAP_GEN_DISC_ADV_MIN;
            [Description("Param ID (1 Byte) - GAP parameter ID")]
            [DefaultValueAttribute(GAP_ParamId.TGAP_GEN_DISC_ADV_MIN)]
            [Category("ParamID")]
            [Serialize(number = 1)]
            public GAP_ParamId paramId
            {
                get { return _paramId; }
                set { _paramId = value; }
            }
        }
        #endregion  // GAP_GetParam

        #region GAP_ResolvePrivateAddr
        /***********************************************************/
        public class GAP_ResolvePrivateAddr
        {
            public string cmdName = "GAP_ResolvePrivateAddr";
            public Byte dataLength = 0x00;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_ResolvePrivateAddr;
            [Description("GAP_ResolvePrivateAddr")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            public const Byte irkSize = 16;
            private string _irk = HCICmds.Empty16BytesStr;
            [Description("IRK (16 Bytes) - Identity Resolving Key of the device your looking for")]
            [DefaultValueAttribute(HCICmds.Empty16BytesStr)]
            public string irk
            {
                get { return _irk; }
                set { _irk = value; }
            }

            public const Byte addrSize = 6;
            private string _addr = HCICmds.EmptyBDAStr;
            [Description("Address (6 Bytes) - Random Private address to resolve")]
            [DefaultValueAttribute(HCICmds.EmptyBDAStr)]
            public string addr
            {
                get { return _addr; }
                set { _addr = value; }
            }
        }
        #endregion  // GAP_ResolvePrivateAddr

        #region GAP_SetAdvToken
        /***********************************************************/
        public class GAP_SetAdvToken
        {
            public string cmdName = "GAP_SetAdvToken";
            public Byte dataLength = 0x02;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_SetAdvToken;
            [Description("GAP_SetAdvToken")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private GAP_AdTypes _adType = GAP_AdTypes.GAP_ADTYPE_FLAGS;
            [Description("Ad Type (1 Byte) - Advertisement Data Type")]
            [DefaultValueAttribute(GAP_AdTypes.GAP_ADTYPE_FLAGS)]
            public GAP_AdTypes adType
            {
                get { return _adType; }
                set { _adType = value; }
            }

            private Byte _advDataLen = 0;
            [Description("Adv Data Len (1 Byte) - Length (in octets) of advData")]
            [DefaultValueAttribute((Byte)0)]
            public Byte advDataLen
            {
                get { return _advDataLen; }
                set { _advDataLen = value; }
            }

            private string _advData = HCICmds.Empty2BytesStr;
            [Description("Adv Data (x Bytes) - Advertisement token data (over-the-air format).")]
            [DefaultValueAttribute(HCICmds.Empty2BytesStr)]
            public string advData
            {
                get { return _advData; }
                set { _advData = value; }
            }
        }
        #endregion  // GAP_SetAdvToken

        #region GAP_RemoveAdvToken
        /***********************************************************/
        public class GAP_RemoveAdvToken
        {
            public string cmdName = "GAP_RemoveAdvToken";
            public Byte dataLength = 0x01;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_RemoveAdvToken;
            [Description("GAP_RemoveAdvToken")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private GAP_AdTypes _adType = GAP_AdTypes.GAP_ADTYPE_FLAGS;
            [Description("Ad Type (1 Byte) - Advertisement Data Type")]
            [DefaultValueAttribute(GAP_AdTypes.GAP_ADTYPE_FLAGS)]
            public GAP_AdTypes adType
            {
                get { return _adType; }
                set { _adType = value; }
            }
        }
        #endregion  // GAP_RemoveAdvToken

        #region GAP_UpdateAdvTokens
        /***********************************************************/
        public class GAP_UpdateAdvTokens
        {
            public string cmdName = "GAP_UpdateAdvTokens";
            public Byte dataLength = 0x00;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_UpdateAdvTokens;
            [Description("GAP_UpdateAdvTokens")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }
        }
        #endregion  // GAP_UpdateAdvTokens

        #region GAP_BondSetParam
        /***********************************************************/
        public class GAP_BondSetParam
        {
            public string cmdName = "GAP_BondSetParam";
            public Byte dataLength = 0x03;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_BondSetParam;
            [Description("GAP_BondSetParam")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private GAP_BondParamId _paramId = GAP_BondParamId.GAPBOND_PAIRING_MODE;
            [Description("Param ID (1 Byte) - GAP Bond Parameter ID")]
            [DefaultValueAttribute(GAP_BondParamId.GAPBOND_PAIRING_MODE)]
            public GAP_BondParamId paramId
            {
                get { return _paramId; }
                set { _paramId = value; }
            }

            private Byte _length = 0x00;
            [Description("Param Length (1 Byte) - Length of the parameter")]
            [DefaultValueAttribute((Byte)0x00)]
            public Byte length
            {
                get { return _length; }
                set { _length = value; }
            }

            private string _value = "00";
            [Description("ParamData (x Bytes) - Param Data Field.  Ex. '02:FF' for 2 bytes")]
            [DefaultValueAttribute("00")]
            public string value
            {
                get { return _value; }
                set { _value = value; }
            }
        }
        #endregion // GAP_BondSetParam

        #region GAP_BondGetParam
        /***********************************************************/
        public class GAP_BondGetParam
        {
            public string cmdName = "GAP_BondGetParam";
            public Byte dataLength = 0x02;  // fixed length data only
            public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.GAP_BondGetParam;
            [Description("GAP_BondGetParam")]
            public string opCode
            {
                get { return ZeroXStr + opCodeValue.ToString("X4"); }
            }

            private GAP_BondParamId _paramId = GAP_BondParamId.GAPBOND_PAIRING_MODE;
            [Description("Param Id (1 Byte) GAP Bond Parameter ID")]
            [DefaultValueAttribute(GAP_BondParamId.GAPBOND_PAIRING_MODE)]
            public GAP_BondParamId paramId
            {
                get { return _paramId; }
                set { _paramId = value; }
            }
        }
        #endregion  // GAP_BondGetParam

        #endregion // GAP Cmds

        #region Util Commands
        /***********************************************************/
        public class UTILCmds
        /***********************************************************/
        {
            /***********************************************************/
            public class UTIL_Reset : HCISerializer
            {
                public UTIL_Reset() : base()
                {
                    dataLength = 0x01;  // fixed length data only
                    opCodeValue = (UInt16)HCICmds.HCICmdOpcode.UTIL_Reset;
                }
                public string cmdName = "UTIL_Reset";
                [Description("UTIL_Reset")]
                public string opCode
                {
                    get { return HCICmds.ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private HCICmds.UTIL_ResetType _resetType = HCICmds.UTIL_ResetType.Hard_Reset;
                [Description("Reset Type (1 Byte) - 0 = Hard and 1 = Soft ")]
                [DefaultValueAttribute(HCICmds.UTIL_ResetType.Hard_Reset)]
                [Serialize(number = 1)]
                public HCICmds.UTIL_ResetType resetType
                {
                    get { return _resetType; }
                    set { _resetType = value; }
                }
            }

            /***********************************************************/
            public class UTIL_NVRead
            {
                public string cmdName = "UTIL_NVRead";
                public Byte dataLength = 0x02;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.UTIL_NVRead;
                [Description("UTIL_NVRead")]
                public string opCode
                {
                    get { return HCICmds.ZeroXStr + opCodeValue.ToString("X4"); }
                }
                private Byte _nvId = 0;
                [Description("NV ID (1 Byte) - NV ID Number")]
                [DefaultValueAttribute((Byte)0)]
                public Byte nvId
                {
                    get { return _nvId; }
                    set { _nvId = value; }
                }

                private Byte _nvDataLen = 0;
                [Description("NV Data Len (1 Byte) - NV Data Length")]
                [DefaultValueAttribute((Byte)0)]
                public Byte nvDataLen
                {
                    get { return _nvDataLen; }
                    set { _nvDataLen = value; }
                }
            }

            /***********************************************************/
            public class UTIL_NVWrite
            {
                public string cmdName = "UTIL_NVWrite";
                public Byte dataLength = 0x02;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCICmdOpcode.UTIL_NVWrite;
                [Description("UTIL_NVWrite")]
                public string opCode
                {
                    get { return HCICmds.ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private Byte _nvId = 0;
                [Description("NV ID (1 Byte) - NV ID Number")]
                [DefaultValueAttribute((Byte)0)]
                public Byte nvId
                {
                    get { return _nvId; }
                    set { _nvId = value; }
                }

                private Byte _nvDataLen = 0;
                [Description("NV Data Len (1 Byte) - NV Data Length")]
                [DefaultValueAttribute((Byte)0)]
                public Byte nvDataLen
                {
                    get { return _nvDataLen; }
                    set { _nvDataLen = value; }
                }

                private string _nvData = "00";
                [Description("NV Data (x Bytes) - NV Data depends on the NV ID")]
                [DefaultValueAttribute("00")]
                public string nvData
                {
                    get { return _nvData; }
                    set { _nvData = value; }
                }
            }
        }
        #endregion // UTIL Commands

        #region HCI Status Commands
        /***********************************************************/
        public class HCIStatusCmds
        /***********************************************************/
        {
            /***********************************************************/
            public class HCIStatus_ReadRSSI : HCISerializer
            {
                public HCIStatus_ReadRSSI() : base()
                {
                    dataLength = 0x02;  // fixed length data only
                    opCodeValue = (UInt16)HCICmds.HCICmdOpcode.HCIStatus_ReadRSSI;
                }

                public string cmdName = "HCIStatus_ReadRSSI";
                [Description("HCIStatus_ReadRSSI")]
                public string opCode
                {
                    get { return HCICmds.ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _handle = (UInt16)HCICmds.GAP_ConnHandle.Default;
                private const string _handle_default = "0x0000";
                [Description("Handle (2 Bytes) - The handle")]
                [DefaultValueAttribute(typeof(UInt16), _handle_default)]
                [Serialize(number = 1)]
                public UInt16 handle
                {
                    get { return _handle; }
                    set { _handle = value; }
                }
            }
        }
        #endregion // HCI Status Commands

        #region Misc Commands
        /***********************************************************/
        public class MISCCmds
        /***********************************************************/
        {
            #region MISC_GenericCommand
            /***********************************************************/
            public class MISC_GenericCommand
            {
                public string cmdName = "MISC_GenericCommand";
                [Description("PacketType (1 Byte) -\n0x00 Command|0x01 - Async|0x02 - Sync|0x03 - Event")]
                public HCICmds.PacketType packetType
                {
                    get { return HCICmds.PacketType.Command; }
                }

                private string _opCode = "0x0000";
                [Description("Opcode (2 Bytes) - The opcode of the command\nFormat: 0x0000")]
                [DefaultValueAttribute("0x0000")]
                public string opCode
                {
                    get { return _opCode; }
                    set { _opCode = value; }
                }

                private Byte _dataLength = 0x00;
                [Description("DataLength (1 Byte) - The length of the data. This field is auto calculated when the command is sent")]
                [DefaultValueAttribute(0x00)]
                public Byte dataLength
                {
                    get { return _dataLength; }
                }

                private string _data = "00";
                [Description("Data (x Bytes) - The data")]
                [DefaultValueAttribute("00")]
                public string data
                {
                    get { return _data; }
                    set { _data = value; }
                }
            }
            #endregion GenericCommand

            #region MISC_RawTxMessage
            /***********************************************************/
            public class MISC_RawTxMessage
            {
                public string cmdName = "MISC_RawTxMessage";
                public Byte dataLength = 0x00;

                public const Byte minMsgSize = 4;
                private string _message = "00 00 00 00";
                [Description("Raw Tx Message (> 4 Bytes) - The Raw Tx Message")]
                [DefaultValueAttribute("00 00 00 00")]
                public string message
                {
                    get { return _message; }
                    set { _message = value; }
                }
            }
            #endregion MISC_RawTxMessage
        }
        #endregion Misc Commands

        /***********************************************************/
        // EVENTS
        /***********************************************************/

        #region GAP Events
        /***********************************************************/
        public class GAPEvts
        /***********************************************************/
        {
            /***********************************************************/
            public class GAP_AuthenticationComplete
            {
                public string cmdName = "GAP_AuthenticationComplete";
                public Byte dataLength = 0x11;  // fixed length data only
                public UInt16 opCodeValue = (UInt16)HCICmds.HCIEvtOpCode.GAP_AuthenticationComplete;
                [Description("GAP_AuthenticationComplete")]
                public string opCode
                {
                    get { return HCICmds.ZeroXStr + opCodeValue.ToString("X4"); }
                }

                private UInt16 _connHandle = (UInt16)HCICmds.GAP_ConnHandle.Default;
                private const string _connHandle_default = "0x0000";
                [Description("Connection Handle (2 Bytes) - The handle of the connection")]
                [DefaultValueAttribute(typeof(UInt16), _connHandle_default)]

                public UInt16 connHandle
                {
                    get { return _connHandle; }
                    set { _connHandle = value; }
                }

                // this is bit mask
                private Byte _authState = 0x00;
                [Description("Auth State (1 Byte)")]
                [DefaultValueAttribute(0x00)]
                public Byte authState
                {
                    get { return _authState; }
                    set { _authState = value; }
                }

                private HCICmds.GAP_EnableDisable _secInfo_enable = HCICmds.GAP_EnableDisable.Disable;
                [Description("Security Info Enable (1 Byte)")]
                [DefaultValueAttribute(HCICmds.GAP_EnableDisable.Disable)]
                public HCICmds.GAP_EnableDisable secInfo_enable
                {
                    get { return _secInfo_enable; }
                    set { _secInfo_enable = value; }
                }

                private Byte _secInfo_LTKsize = 0x00;
                [Description("Security Info LTK Size (1 Byte)")]
                [DefaultValueAttribute(0x00)]
                public Byte secInfo_LTKsize
                {
                    get { return _secInfo_LTKsize; }
                    set { _secInfo_LTKsize = value; }
                }

                public const int secInfo_LTKSize = 16;
                private string _secInfo_LTK = HCICmds.Empty16BytesStr;
                [Description("Security Info LTK (16 Byte)")]
                [DefaultValueAttribute(16)]
                public string secInfo_LTK
                {
                    get { return _secInfo_LTK; }
                    set { _secInfo_LTK = value; }
                }

                private UInt16 _secInfo_DIV = 0x0000;
                [Description("Security Info DIV (2 Bytes)")]
                [DefaultValueAttribute(0x0000)]
                public UInt16 secInfo_DIV
                {
                    get { return _secInfo_DIV; }
                    set { _secInfo_DIV = value; }
                }

                public const int secInfo_RANDSize = 8;
                private string _secInfo_RAND = HCICmds.Empty8BytesStr;
                [Description("Security Info RAND (8 Bytes)")]
                [DefaultValueAttribute(HCICmds.Empty8BytesStr)]
                public string secInfo_RAND
                {
                    get { return _secInfo_RAND; }
                    set { _secInfo_RAND = value; }
                }

                private HCICmds.GAP_EnableDisable _devSecInfo_enable = HCICmds.GAP_EnableDisable.Disable;
                [Description("Dev Security Info (1 Byte)")]
                [DefaultValueAttribute(HCICmds.GAP_EnableDisable.Disable)]
                public HCICmds.GAP_EnableDisable devSecInfo_enable
                {
                    get { return _devSecInfo_enable; }
                    set { _devSecInfo_enable = value; }
                }

                private Byte _devSecInfo_LTKsize = 0x00;
                [Description("Dev Security Info LTK Size (1 Byte)")]
                [DefaultValueAttribute(0x00)]
                public Byte devSecInfo_LTKsize
                {
                    get { return _devSecInfo_LTKsize; }
                    set { _devSecInfo_LTKsize = value; }
                }

                public const int devSecInfo_LTKSize = 16;
                private string _devSecInfo_LTK = HCICmds.Empty16BytesStr;
                [Description("Dev Security Info LTK (16 Byte)")]
                [DefaultValueAttribute(16)]
                public string devSecInfo_LTK
                {
                    get { return _devSecInfo_LTK; }
                    set { _devSecInfo_LTK = value; }
                }

                private UInt16 _devSecInfo_DIV = 0x0000;
                [Description("Dev Security Info DIV (2 Bytes)")]
                [DefaultValueAttribute(0x0000)]
                public UInt16 devSecInfo_DIV
                {
                    get { return _devSecInfo_DIV; }
                    set { _devSecInfo_DIV = value; }
                }

                public const int devSecInfo_RANDSize = 8;
                private string _devSecInfo_RAND = HCICmds.Empty8BytesStr;
                [Description("Dev Security Info RAND (8 Byte)")]
                [DefaultValueAttribute(8)]
                public string devSecInfo_RAND
                {
                    get { return _devSecInfo_RAND; }
                    set { _devSecInfo_RAND = value; }
                }

                private HCICmds.GAP_EnableDisable _idInfo_enable = HCICmds.GAP_EnableDisable.Disable;
                [Description("Identity Info Enable (1 Byte)")]
                [DefaultValueAttribute(HCICmds.GAP_EnableDisable.Disable)]
                public HCICmds.GAP_EnableDisable idInfo_enable
                {
                    get { return _idInfo_enable; }
                    set { _idInfo_enable = value; }
                }

                public const int idInfo_IRKSize = 16;
                private string _idInfo_IRK = HCICmds.Empty16BytesStr;
                [Description("Identity Info IRK (16 Bytes)")]
                [DefaultValueAttribute(16)]
                public string idInfo_IRK
                {
                    get { return _idInfo_IRK; }
                    set { _idInfo_IRK = value; }
                }

                public const int idInfo_BdAddrSize = 6;
                private string _idInfo_BdAddr = HCICmds.Empty8BytesStr;
                [Description("Identity Info BD Address (6 Bytes)")]
                [DefaultValueAttribute(6)]
                public string idInfo_BdAddr
                {
                    get { return _idInfo_BdAddr; }
                    set { _idInfo_BdAddr = value; }
                }

                private HCICmds.GAP_EnableDisable _signInfo_enable = HCICmds.GAP_EnableDisable.Disable;
                [Description("Signing Info Enable (1 Byte)")]
                [DefaultValueAttribute(HCICmds.GAP_EnableDisable.Disable)]
                public HCICmds.GAP_EnableDisable signInfo_enable
                {
                    get { return _signInfo_enable; }
                    set { _signInfo_enable = value; }
                }

                public const int signInfo_CSRKSize = 16;
                private string _signInfo_CSRK = HCICmds.Empty16BytesStr;
                [Description("Signing Info CSRK (16 Bytes)")]
                [DefaultValueAttribute(HCICmds.Empty16BytesStr)]
                public string signInfo_CSRK
                {
                    get { return _signInfo_CSRK; }
                    set { _signInfo_CSRK = value; }
                }

                private UInt32 _signCounter = 0x00000000;
                [Description("Sign Counter (4 Bytes)")]
                [DefaultValueAttribute(0x00000000)]
                public UInt32 signCounter
                {
                    get { return _signCounter; }
                    set { _signCounter = value; }
                }
            }
        }
        #endregion // GAP Events
    }


}