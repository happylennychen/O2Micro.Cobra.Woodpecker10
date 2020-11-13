using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using Cobra.Communication;
using Cobra.Common;
using System.IO;

namespace Cobra.Woodpecker10
{
    internal class EFUSEConfigDEMBehaviorManage:DEMBehaviorManageBase
    {
        #region 基础服务功能设计
        public override UInt32 Command(ref TASKMessage msg)
        {
            UInt32 ret = LibErrorCode.IDS_ERR_SUCCESSFUL;

            switch ((ElementDefine.COMMAND)msg.sub_task)
            {
                case ElementDefine.COMMAND.EFUSE_CONFIG_READ:
                    {
                        if (msg.task_parameterlist.parameterlist.Count < ElementDefine.EF_TOTAL_PARAMS)
                            return ElementDefine.IDS_ERR_DEM_ONE_PARAM_DISABLE;
                        ret = SetWorkMode(ElementDefine.EFUSE_MODE.PROGRAM);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        msg.gm.message = "Please provide 7.2V power supply to Tref pin and limit its current to 150mA.";
                        msg.controlreq = COMMON_CONTROL.COMMON_CONTROL_SELECT;
                        if (!msg.controlmsg.bcancel) return LibErrorCode.IDS_ERR_DEM_USER_QUIT;
                        msg.percent = 40;
                        ret = GetRegisteInfor(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        msg.percent = 60;
                        ret = Read(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        msg.gm.message = "Please remove 7.2V power supply from Tref pin.";
                        msg.controlreq = COMMON_CONTROL.COMMON_CONTROL_SELECT;
                        if (!msg.controlmsg.bcancel) return LibErrorCode.IDS_ERR_DEM_USER_QUIT;
                        ret = SetWorkMode(ElementDefine.EFUSE_MODE.NORMAL);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        msg.percent = 80;
                        ret = ConvertHexToPhysical(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        break;
                    }
                case ElementDefine.COMMAND.EFUSE_CONFIG_WRITE:
                    {
                        if (msg.task_parameterlist.parameterlist.Count < ElementDefine.EF_TOTAL_PARAMS)
                            return ElementDefine.IDS_ERR_DEM_ONE_PARAM_DISABLE;
                        ret = SetWorkMode(ElementDefine.EFUSE_MODE.PROGRAM);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;

                        msg.gm.message = "Please provide 7.2V power supply to Tref pin and limit its current to 150mA.";
                        msg.controlreq = COMMON_CONTROL.COMMON_CONTROL_SELECT;
                        if (!msg.controlmsg.bcancel) return LibErrorCode.IDS_ERR_DEM_USER_QUIT;

                        if (isEFFrozen())
                        {
                            ret = ElementDefine.IDS_ERR_DEM_FROZEN;
                            return ret;
                        }
                        msg.percent = 30;
                        ret = GetRegisteInfor(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        msg.percent = 40;
                        ret = Read(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        msg.percent = 50;
                        ret = ConvertPhysicalToHex(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        PrepareHexData();
                        msg.percent = 60;
                        Dictionary<byte, ushort> pairs = StoreEFUSE();
                        ret = Write(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        Mapping();
                        if (!IsDownloadSuccessful(pairs))
                        {
                            ret = ElementDefine.IDS_ERR_DEM_READ_BACK_CHECK_FAILED;
                            return ret;
                        }
                        ret = Read(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        msg.gm.message = "Please remove 7.2V power supply from Tref pin.";
                        msg.controlreq = COMMON_CONTROL.COMMON_CONTROL_SELECT;
                        if (!msg.controlmsg.bcancel) return LibErrorCode.IDS_ERR_DEM_USER_QUIT;
                        ret = SetWorkMode(ElementDefine.EFUSE_MODE.NORMAL);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        msg.percent = 80;
                        ret = ConvertHexToPhysical(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        break;
                    }
                case ElementDefine.COMMAND.EFUSE_CONFIG_SAVE_EFUSE_HEX:
                    {
                        InitEfuseData();
                        ret = ConvertPhysicalToHex(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        PrepareHexData();
                        ret = GetEfuseHexData(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        FileStream hexfile = new FileStream(msg.sub_task_json, FileMode.Create);
                        StreamWriter hexsw = new StreamWriter(hexfile);
                        hexsw.Write(msg.sm.efusehexdata);
                        hexsw.Close();
                        hexfile.Close();

                        ret = GetEfuseBinData(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;

                        string binfilename = Path.Combine(Path.GetDirectoryName(msg.sub_task_json),
                            Path.GetFileNameWithoutExtension(msg.sub_task_json) + ".bin");

                        Encoding ec = Encoding.UTF8;
                        using (BinaryWriter bw = new BinaryWriter(File.Open(binfilename, FileMode.Create), ec))
                        {
                            foreach (var b in msg.sm.efusebindata)
                                bw.Write(b);

                            bw.Close();
                        }
                        break;
                    }
            }
            return ret;
        }

        private bool IsDownloadSuccessful(Dictionary<byte, ushort> pairs)
        {
            byte buf = 0;
            for (byte i = (byte)ElementDefine.OP_USR_OFFSET; i <= ElementDefine.OP_USR_TOP; i++)
            {
                ReadByte(i, ref buf);
                if (buf != pairs[(byte)(i - 0x10)])
                    return false;
            }
            return true;
        }

        private void Mapping()
        {
            byte tmp = 0;
            ReadByte((byte)ElementDefine.WORKMODE_REG, ref tmp);
            WriteByte((byte)ElementDefine.WORKMODE_REG, (byte)(tmp & 0xdf));

            ReadByte((byte)ElementDefine.OP_SW_MAPPING, ref tmp);
            WriteByte((byte)ElementDefine.OP_SW_MAPPING, (byte)(tmp | 0x08));
        }

        private Dictionary<byte, ushort> StoreEFUSE()
        {
            Dictionary<byte, ushort> output = new Dictionary<byte, ushort>();
            for (byte i = (byte)ElementDefine.EF_USR_OFFSET; i <= ElementDefine.EF_USR_TOP; i++)
            {
                output.Add(i, parent.m_OpRegImg[i].val);
            }
            return output;
        }

        private bool isEFFrozen()
        {
            byte tmp = 0;
            ReadByte((byte)ElementDefine.EF_USR_TOP, ref tmp);
            if ((tmp & 0x80) == 0x80)
                return true;
            else
                return false;
        }

        private void InitEfuseData()
        {
            for (ushort i = ElementDefine.EF_USR_OFFSET; i <= ElementDefine.EF_USR_TOP; i++)
            {
                parent.m_OpRegImg[i].err = 0;
                parent.m_OpRegImg[i].val = 0;
            }
        }

        private UInt32 GetEfuseHexData(ref TASKMessage msg)
        {
            string tmp = "";
            for (ushort i = ElementDefine.EF_USR_OFFSET; i <= ElementDefine.EF_USR_TOP; i++)
            {
                if (parent.m_OpRegImg[i].err != LibErrorCode.IDS_ERR_SUCCESSFUL)
                    return parent.m_OpRegImg[i].err;
                tmp += "0x" + i.ToString("X2") + ", " + "0x" + parent.m_OpRegImg[i].val.ToString("X2") + "\r\n";
            }
            msg.sm.efusehexdata = tmp;
            return LibErrorCode.IDS_ERR_SUCCESSFUL;
        }

        private UInt32 GetEfuseBinData(ref TASKMessage msg)
        {
            List<byte> tmp = new List<byte>();
            for (ushort i = ElementDefine.EF_USR_OFFSET; i <= ElementDefine.EF_USR_TOP; i++)
            {
                if (parent.m_OpRegImg[i].err != LibErrorCode.IDS_ERR_SUCCESSFUL)
                    return parent.m_OpRegImg[i].err;
                tmp.Add((byte)i);
                tmp.Add((byte)(parent.m_OpRegImg[i].val));
            }
            msg.sm.efusebindata = tmp;
            return LibErrorCode.IDS_ERR_SUCCESSFUL;
        }
        #endregion
    }
}