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
    internal class RegisterConfigDEMBehaviorManage:DEMBehaviorManageBase
    {
        #region 基础服务功能设计
        public override UInt32 Command(ref TASKMessage msg)
        {
            UInt32 ret = LibErrorCode.IDS_ERR_SUCCESSFUL;

            switch ((ElementDefine.COMMAND)msg.sub_task)
            {
                case ElementDefine.COMMAND.REGISTER_CONFIG_READ:
                    {
                        if (msg.task_parameterlist.parameterlist.Count < ElementDefine.EF_TOTAL_PARAMS)
                            return ElementDefine.IDS_ERR_DEM_ONE_PARAM_DISABLE;
                        ret = SetWorkMode(ElementDefine.EFUSE_MODE.WRITE_MAP_CTRL);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        msg.percent = 40;
                        ret = GetRegisteInfor(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        msg.percent = 60;
                        ret = Read(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        msg.percent = 80;
                        ret = ConvertHexToPhysical(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        break;
                    }
                case ElementDefine.COMMAND.REGISTER_CONFIG_WRITE:
                    {
                        if (msg.task_parameterlist.parameterlist.Count < ElementDefine.EF_TOTAL_PARAMS)
                            return ElementDefine.IDS_ERR_DEM_ONE_PARAM_DISABLE;
                        ret = SetWorkMode(ElementDefine.EFUSE_MODE.WRITE_MAP_CTRL);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        if (!isOPEmpty())
                        {
                            ret = ElementDefine.IDS_ERR_DEM_FROZEN_OP;
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
                        msg.percent = 60;
                        ret = Write(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        msg.percent = 80;
                        ret = ConvertHexToPhysical(ref msg);
                        if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                            return ret;
                        break;
                    }
            }
            return ret;
        }

        private bool isOPEmpty()
        {
            byte tmp = 0;
            ReadByte(0x2d, ref tmp);
            if ((tmp & 0x80) == 0x80)
                return false;
            else
                return true;
        }
        #endregion
    }
}