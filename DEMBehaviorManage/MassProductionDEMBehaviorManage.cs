using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using O2Micro.Cobra.Communication;
using O2Micro.Cobra.Common;
using System.IO;

namespace O2Micro.Cobra.Woodpecker10
{
    internal class MassProductionDEMBehaviorManage : DEMBehaviorManageBase
    {
        UInt16[] EFUSEUSRbuf = new UInt16[ElementDefine.EF_USR_TOP - ElementDefine.EF_USR_OFFSET + 1];
        public override UInt32 Command(ref TASKMessage msg)
        {
            UInt32 ret = LibErrorCode.IDS_ERR_SUCCESSFUL;

            switch ((ElementDefine.COMMAND)msg.sub_task)
            {
                case ElementDefine.COMMAND.MP_BIN_FILE_CHECK:
                    {
                        string binFileName = msg.sub_task_json;

                        var blist = SharedAPI.LoadBinFileToList(binFileName);
                        if (blist.Count == 0)
                            ret = LibErrorCode.IDS_ERR_DEM_LOAD_BIN_FILE_ERROR;
                        else
                            ret = CheckBinData(blist);
                        break;
                    }

                case ElementDefine.COMMAND.MP_FROZEN_BIT_CHECK_PC:
                    ret = PowerOn();
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                        return ret;

                    ret = FrozenBitCheck();
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                        return ret;

                    ret = PowerOff();
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                        return ret;
                    break;

                case ElementDefine.COMMAND.MP_FROZEN_BIT_CHECK:
                    ret = FrozenBitCheck();
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                        return ret;
                    break;

                case ElementDefine.COMMAND.MP_DIRTY_CHIP_CHECK_PC:
                    ret = PowerOn();
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                        return ret;

                    ret = DirtyChipCheck();
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                        return ret;

                    ret = PowerOff();
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                        return ret;
                    break;

                case ElementDefine.COMMAND.MP_DIRTY_CHIP_CHECK:
                    ret = DirtyChipCheck();
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                        return ret;
                    break;

                case ElementDefine.COMMAND.MP_DOWNLOAD_PC:
                    {
                        ret = DownloadWithPowerControl(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
#if debug
                        Thread.Sleep(1000);
#endif
                        break;
                    }

                case ElementDefine.COMMAND.MP_DOWNLOAD:
                    {
                        ret = DownloadWithoutPowerControl(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
#if debug
                        Thread.Sleep(1000);
#endif
                        break;
                    }
                case ElementDefine.COMMAND.MP_READ_BACK_CHECK_PC:
                    {
                        ret = PowerOn();
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;

                        ret = ReadBackCheck();
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;

                        ret = PowerOff();
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        break;
                    }
                case ElementDefine.COMMAND.MP_READ_BACK_CHECK:
                    {
                        ret = ReadBackCheck();
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        break;
                    }
            }
            return ret;
        }


        public uint CheckBinData(List<byte> blist)
        {
            UInt32 ret = LibErrorCode.IDS_ERR_SUCCESSFUL;
            int length = (ElementDefine.EF_USR_TOP - ElementDefine.EF_USR_OFFSET) + 1;
            length *= 2;    //一个字节地址，一个字节数值
            if (blist.Count != length)
            {
                ret = LibErrorCode.IDS_ERR_DEM_BIN_LENGTH_ERROR;
            }
            else
            {
                for (int i = ElementDefine.EF_USR_OFFSET, j = 1; i <= ElementDefine.EF_USR_TOP; i++, j++)
                {
                    if (blist[j * 2] != i)
                    {
                        ret = LibErrorCode.IDS_ERR_DEM_BIN_ADDRESS_ERROR;
                        break;
                    }
                }
            }
            return ret;
        }
        private bool isFrozen = false;
        private UInt32 FrozenBitCheck() //注意，这里没有把image里的Frozen bit置为1，记得在后面的流程中做这件事
        {
            SetWorkMode(ElementDefine.EFUSE_MODE.PROGRAM);
            UInt32 ret = LibErrorCode.IDS_ERR_SUCCESSFUL;
            byte pval1 = 0;
            byte cfg = 0;
            ret = ReadByte((byte)ElementDefine.EF_USR_TOP, ref pval1);

            if ((pval1 & 0x80) == 0x80)
            {
                isFrozen = true;
            }
            else
                isFrozen = false;

            if (isFrozen)
            {
                return LibErrorCode.IDS_ERR_DEM_FROZEN;
            }

            return ret;
        }

        private UInt32 DirtyChipCheck()
        {
#if dirty
            return LibErrorCode.IDS_ERR_DEM_DIRTYCHIP;
#else
            SetWorkMode(ElementDefine.EFUSE_MODE.PROGRAM);
            UInt32 ret = LibErrorCode.IDS_ERR_SUCCESSFUL;

            byte pval = 0;
            if (isFrozen == false)
            {
                for (byte index = (byte)ElementDefine.EF_USR_OFFSET; index <= (byte)ElementDefine.EF_USR_TOP; index++)
                {
                    ret = ReadByte(index, ref pval);
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                    {
                        return ret;
                    }
                    else if (pval != 0)
                    {
                        return LibErrorCode.IDS_ERR_DEM_DIRTYCHIP;
                    }
                }
                return ret;
            }
            return ret;
#endif
        }


        private UInt32 DownloadWithPowerControl(ref TASKMessage msg)
        {
            UInt32 ret = LibErrorCode.IDS_ERR_SUCCESSFUL;

            PrepareHexData();

            ret = SetWorkMode(ElementDefine.EFUSE_MODE.PROGRAM);
            if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
            {
                return ret;
            }

            ret = PowerOn();
            if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                return ret;


            for (byte badd = (byte)ElementDefine.EF_USR_OFFSET; badd <= (byte)ElementDefine.EF_USR_TOP; badd++)
            {
#if debug
                ret = LibErrorCode.IDS_ERR_SUCCESSFUL;
#else
                ret = parent.m_OpRegImg[badd].err;
#endif
                if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                {
                    return ret;
                }

#if debug
                EFUSEUSRbuf[badd - ElementDefine.EF_USR_OFFSET] = 0;
#else
                EFUSEUSRbuf[badd - ElementDefine.EF_USR_OFFSET] = parent.m_OpRegImg[badd].val;
#endif
                ret = WriteByte((byte)(badd), (byte)parent.m_OpRegImg[badd].val);
                parent.m_OpRegImg[(byte)(badd)].err = ret;
                if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                {
                    return ret;
                }
                //byte tmp = 0;
                //ret = ReadByte((byte)(badd), ref tmp);     //Issue 1746 workaround
                //if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                //{
                //    return ret;
                //}
            }

            ret = PowerOff();
            if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                return ret;

            ret = SetWorkMode(ElementDefine.EFUSE_MODE.NORMAL);
            if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
            {
                return ret;
            }

            return ret;
        }

        private UInt32 DownloadWithoutPowerControl(ref TASKMessage msg)
        {
            UInt32 ret = LibErrorCode.IDS_ERR_SUCCESSFUL;

            PrepareHexData();

            ret = SetWorkMode(ElementDefine.EFUSE_MODE.PROGRAM);
            if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
            {
                return ret;
            }

            for (byte badd = (byte)ElementDefine.EF_USR_OFFSET; badd <= (byte)ElementDefine.EF_USR_TOP; badd++)
            {
#if debug
                ret = LibErrorCode.IDS_ERR_SUCCESSFUL;
#else
                ret = parent.m_OpRegImg[badd].err;
#endif
                if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                {
                    return ret;
                }

#if debug
                EFUSEUSRbuf[badd - ElementDefine.EF_USR_OFFSET] = 0;
#else
                EFUSEUSRbuf[badd - ElementDefine.EF_USR_OFFSET] = parent.m_OpRegImg[badd].val;
#endif
                ret = WriteByte((byte)(badd), (byte)parent.m_OpRegImg[badd].val);
                parent.m_OpRegImg[(byte)(badd)].err = ret;
                if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                {
                    return ret;
                }
            }

            ret = SetWorkMode(ElementDefine.EFUSE_MODE.NORMAL);
            if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
            {
                return ret;
            }

            return ret;
        }

        private UInt32 ReadBackCheck()
        {
            UInt32 ret = LibErrorCode.IDS_ERR_SUCCESSFUL;
#if readback
            return LibErrorCode.IDS_ERR_DEM_BUF_CHECK_FAIL;
#else
            SetWorkMode(ElementDefine.EFUSE_MODE.PROGRAM);
            byte pval = 0;
            for (byte badd = (byte)ElementDefine.EF_USR_OFFSET; badd <= (byte)ElementDefine.EF_USR_TOP; badd++)
            {
                ret = ReadByte((byte)(badd), ref pval);
                if (pval != EFUSEUSRbuf[badd - ElementDefine.EF_USR_OFFSET])
                {
                    FolderMap.WriteFile("Read back check, address: 0x" + (badd).ToString("X2") + "\torigi value: 0x" + EFUSEUSRbuf[badd - ElementDefine.EF_USR_OFFSET].ToString("X2") + "\tread value: 0x" + pval.ToString("X2"));
                    return LibErrorCode.IDS_ERR_DEM_BUF_CHECK_FAIL;
                }
            }
            return ret;
#endif
        }

        protected UInt32 PowerOn()
        {
            UInt32 ret = 0;
            lock (m_lock)
            {
                ret = OnPowerOn();
            }
            return ret;
        }
        protected UInt32 PowerOff()
        {
            UInt32 ret = 0;
            lock (m_lock)
            {
                ret = OnPowerOff();
            }
            return ret;
        }
        private UInt32 OnPowerOn()
        {
            byte[] yDataIn = { 0x51 };
            byte[] yDataOut = { 0, 0 };
            ushort uOutLength = 2;
            ushort uWrite = 1;
            if (parent.m_Interface.SendCommandtoAdapter(yDataIn, ref yDataOut, ref uOutLength, uWrite))
            {
                if (uOutLength == 2 && yDataOut[0] == 0x51 && yDataOut[1] == 0x1)
                {
                    Thread.Sleep(200);	//sync with SP8G2
                    return LibErrorCode.IDS_ERR_SUCCESSFUL;
                }
                else
                    return ElementDefine.IDS_ERR_DEM_POWERON_FAILED;
            }
            return ElementDefine.IDS_ERR_DEM_POWERON_FAILED;
        }

        private UInt32 OnPowerOff()
        {
            byte[] yDataIn = { 0x52 };
            byte[] yDataOut = { 0, 0 };
            ushort uOutLength = 2;
            ushort uWrite = 1;
            if (parent.m_Interface.SendCommandtoAdapter(yDataIn, ref yDataOut, ref uOutLength, uWrite))
            {
                if (uOutLength == 2 && yDataOut[0] == 0x52 && yDataOut[1] == 0x2)
                {
                    return LibErrorCode.IDS_ERR_SUCCESSFUL;
                }
                else
                    return ElementDefine.IDS_ERR_DEM_POWEROFF_FAILED;
            }
            return ElementDefine.IDS_ERR_DEM_POWEROFF_FAILED;
        }

    }
}