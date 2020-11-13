using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Cobra.Common;

namespace Cobra.Woodpecker10
{
    public class RegisterConfigDEMDataManage:DEMDataManageBase
    {
        public RegisterConfigDEMDataManage(object pParent) : base(pParent)
        {
        }
        bool FromHexToPhy = false;
        //private void UpdateDOTE(ref Parameter pDOT_E)
        //{
        //    Parameter pDOT = new Parameter();
        //    switch (pDOT_E.guid)
        //    {
        //        case ElementDefine.E_DOT_E:
        //            pDOT = parent.parent.pE_DOT_TH;
        //            break;
        //        case ElementDefine.O_DOT_E:
        //            pDOT = parent.parent.pO_DOT_TH;
        //            break;
        //    }

        //    if (pDOT_E.phydata == 1)                     //pDOT_E.phydata是1的情况下，DOT变化了，那肯定是在读芯片, pDOT.hexdata已经是准确的了
        //    {
        //        if (pDOT.hexdata < 2)
        //        {
        //            pDOT_E.phydata = 1;
        //        }
        //        else
        //        {
        //            pDOT_E.phydata = 0;
        //        }
        //    }
        //    else if (pDOT_E.phydata == 0)               //pDOT_E.phydata是0的情况下，DOT变化了，有可能是读芯片，也可能是UI操作
        //    {
        //        //如果是读芯片，那么就还是直接使用hexdata
        //        if (FromHexToPhy)
        //        {
        //            if (pDOT.hexdata < 2)
        //            {
        //                pDOT_E.phydata = 1;
        //            }
        //            else
        //            {
        //                pDOT_E.phydata = 0;
        //            }
        //        }
        //        //*/
        //        //如果是UI操作，那么就什么都不用做
        //    }
        //}

        private void UpdateDOT(ref Parameter pDOT)
        {
            Parameter pDOT_E = new Parameter();
            switch (pDOT.guid)
            {
                case ElementDefine.O_DOT_TH:
                    pDOT_E = parent.parent.pO_DOT_TH;
                    break;
                case ElementDefine.E_DOT_TH:
                    pDOT_E = parent.parent.pE_DOT_TH;
                    break;
            }
            if (pDOT_E.phydata == 0)
            {
                //pDOT.phydata = 2;
            }
            else if (pDOT_E.phydata == 1)
            {
                //pDOT.phydata = 0;
            }
        }
        private void UpdateOVP(ref Parameter pOVP_TH)
        {
            Parameter pBAT_TYPE = new Parameter();
            switch (pOVP_TH.guid)
            {
                case ElementDefine.O_OVP_TH:
                    pBAT_TYPE = parent.parent.pO_BAT_TYPE;
                    break;
                case ElementDefine.E_OVP_TH:
                    pBAT_TYPE = parent.parent.pE_BAT_TYPE;
                    break;
            }
            if (pBAT_TYPE.phydata == 0)
            {
                pOVP_TH.offset = 3900;
                pOVP_TH.dbPhyMin = 4000;
                pOVP_TH.dbPhyMax = 4500;
                if (pOVP_TH.phydata < 4000)
                    pOVP_TH.phydata = 4000;
            }
            else if (pBAT_TYPE.phydata == 1)
            {
                pOVP_TH.offset = 3400;
                pOVP_TH.dbPhyMin = 3500;
                pOVP_TH.dbPhyMax = 4000;
                if (pOVP_TH.phydata > 4000)
                    pOVP_TH.phydata = 4000;
            }
        }
        private void UpdateOVR(ref Parameter pOVR)
        {
            Parameter pBAT_TYPE = new Parameter();
            Parameter pOVP = new Parameter();
            switch (pOVR.guid)
            {
                case ElementDefine.O_OVR_HYS:
                    pBAT_TYPE = parent.parent.pO_BAT_TYPE;
                    pOVP = parent.parent.pO_OVP_TH;
                    break;
                case ElementDefine.E_OVR_HYS:
                    pBAT_TYPE = parent.parent.pE_BAT_TYPE;
                    pOVP = parent.parent.pE_OVP_TH;
                    break;
            }
            if (pBAT_TYPE.phydata == 0)
            {
                if (pOVP.phydata >= 4050)
                {
                    if (!pOVR.itemlist.Contains("400mV"))
                    {
                        pOVR.itemlist.Add("400mV");
                    }
                }
                else
                {
                    if (pOVR.itemlist.Contains("400mV"))
                    {
                        pOVR.itemlist.Remove("400mV");
                        if (pOVR.phydata >= pOVR.itemlist.Count) pOVR.phydata = (pOVR.itemlist.Count - 1);
                    }
                }

            }
            else if (pBAT_TYPE.phydata == 1)
            {
                if (pOVP.phydata >= 3550)
                {
                    if (!pOVR.itemlist.Contains("400mV"))
                    {
                        pOVR.itemlist.Add("400mV");
                    }
                }
                else
                {
                    if (pOVR.itemlist.Contains("400mV"))
                    {
                        pOVR.itemlist.Remove("400mV");
                        if (pOVR.phydata >= pOVR.itemlist.Count) pOVR.phydata = (pOVR.itemlist.Count - 1);
                    }
                }
            }
        }
        private void UVRItemListAdjust(ref Parameter pUVR, byte num)
        {
            int diff = pUVR.itemlist.Count - num;
            if (diff > 0)
            {
                for (int i = diff; i > 0; i--)
                {
                    pUVR.itemlist.RemoveAt(pUVR.itemlist.Count - 1);
                }
            }
            else if (diff < 0)
            {
                for (int i = -diff; i > 0; i--)
                {
                    pUVR.itemlist.Add((pUVR.itemlist.Count * 100).ToString() + "mV");
                }
            }
        }
        private void UpdateUVR(ref Parameter pUVR)
        {
            Parameter pBAT_TYPE = new Parameter();
            Parameter pUVP = new Parameter();
            switch (pUVR.guid)
            {
                case ElementDefine.O_UVR_HYS:
                    pBAT_TYPE = parent.parent.pO_BAT_TYPE;
                    pUVP = parent.parent.pO_UVP_TH;
                    break;
                case ElementDefine.E_UVR_HYS:
                    pBAT_TYPE = parent.parent.pE_BAT_TYPE;
                    pUVP = parent.parent.pE_UVP_TH;
                    break;
            }
            int num = 16 - (int)pUVP.phydata;
            if (num > 8)
                num = 8;
            int diff = pUVR.itemlist.Count - num;
            if (pBAT_TYPE.phydata == 1)
            {
                if (diff > 0)
                {
                    for (int i = diff; i > 0; i--)
                    {
                        pUVR.itemlist.RemoveAt(pUVR.itemlist.Count - 1);
                        if (pUVR.phydata >= pUVR.itemlist.Count) pUVR.phydata = (pUVR.itemlist.Count - 1);
                    }
                }
                else if (diff < 0)
                {
                    for (int i = -diff; i > 0; i--)
                    {
                        pUVR.itemlist.Add(((pUVR.itemlist.Count + 1) * 100).ToString() + "mV");
                    }
                }
            }
            else if (pBAT_TYPE.phydata == 0)
            {
                for (int i = pUVR.itemlist.Count; i < 8; i++)
                {
                    pUVR.itemlist.Add(((i + 1) * 100).ToString() + "mV");
                }
            }
        }
        /// <summary>
        /// 更新参数ItemList
        /// </summary>
        /// <param name="p"></param>
        /// <param name="relatedparameters"></param>
        /// <returns></returns>
        public override void UpdateEpParamItemList(Parameter pTarget)
        {
            if (pTarget.errorcode != LibErrorCode.IDS_ERR_SUCCESSFUL)
                return;
            Parameter source = new Parameter();
            switch (pTarget.guid)
            {
                //case ElementDefine.E_DOT_E:
                //case ElementDefine.O_DOT_E:
                //    UpdateDOTE(ref pTarget);
                //    break;
                case ElementDefine.E_DOT_TH:
                    //UpdateThType(ref pTarget);
                    break;
                case ElementDefine.O_DOT_TH:
                    //UpdateThType(ref pTarget);
                    break;
                case ElementDefine.E_OVP_TH:
                case ElementDefine.O_OVP_TH:
                    UpdateOVP(ref pTarget);
                    break;
                case ElementDefine.O_OVR_HYS:
                case ElementDefine.E_OVR_HYS:
                    UpdateOVR(ref pTarget);
                    break;
                case ElementDefine.O_UVR_HYS:
                case ElementDefine.E_UVR_HYS:
                    UpdateUVR(ref pTarget);
                    break;
            }
            FromHexToPhy = false;
            return;
        }

        public override void Physical2Hex(ref Parameter p)
        {
            UInt16 wdata = 0;
            double dtmp = 0;
            UInt32 ret = LibErrorCode.IDS_ERR_SUCCESSFUL;

            if (p == null) return;
            /*if (parent.fromCFG == true)
            {
                if (p.guid == ElementDefine.E_DOT)
                {
                    if (parent.pE_T_E.phydata == 1)
                        //parent.pE_DOT.hexdata = 0;
                        wdata = 0;
                }
                else if (p.guid == ElementDefine.O_DOT)
                {
                    if (parent.pO_T_E.phydata == 1)
                        //parent.pO_DOT.hexdata = 0;
                        wdata = 0;
                }
                ret = WriteToRegImg(p, wdata);
                if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                    WriteToRegImgError(p, ret);
            }*/
            switch ((ElementDefine.SUBTYPE)p.subtype)
            {                
				case ElementDefine.SUBTYPE.CELL_NUM:
                    wdata = (ushort)(p.phydata + 2);
                    ret = WriteToRegImg(p, wdata);
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                        WriteToRegImgError(p, ret);
                    break;
                case ElementDefine.SUBTYPE.OVP:
                    dtmp = p.phydata - p.offset;
                    wdata = (UInt16)((double)(dtmp * p.regref) / (double)p.phyref);
                    ret = WriteToRegImg(p, wdata);
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                        WriteToRegImgError(p, ret);
                    break;
                case ElementDefine.SUBTYPE.DOT_TH:
                    //Parameter pDOT_E = new Parameter();
                    //switch (p.guid)
                    //{
                    //    case ElementDefine.O_DOT_TH:
                    //        pDOT_E = parent.parent.pO_DOT_E;
                    //        break;
                    //    case ElementDefine.E_DOT_TH:
                    //        pDOT_E = parent.parent.pE_DOT_E;
                    //        break;
                    //}
                    //if (pDOT_E.phydata == 1)    //Disable
                    //{
                    //    wdata = 0;
                    //}
                    //else if (pDOT_E.phydata == 0)   //Enable
                    //{
                    //    wdata = (ushort)(p.phydata + 2);
                    //}
                    wdata = (ushort)(p.phydata + 1);
                    ret = WriteToRegImg(p, wdata);
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                        WriteToRegImgError(p, ret);
                    break;
                default:
                    dtmp = p.phydata - p.offset;
                    wdata = (UInt16)((double)(dtmp * p.regref) / (double)p.phyref);
                    ret = WriteToRegImg(p, wdata);
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                        WriteToRegImgError(p, ret);
                    break;
            }
        }

        /// <summary>
        /// 转换参数值类型从物理值到16进制值
        /// </summary>
        /// <param name="p"></param>
        /// <param name="relatedparameters"></param>
        public override void Hex2Physical(ref Parameter p)
        {
            UInt16 wdata = 0;
            double dtmp = 0;
            UInt32 ret = LibErrorCode.IDS_ERR_SUCCESSFUL;

            if (p == null) return;
            switch ((ElementDefine.SUBTYPE)p.subtype)
            {
                case ElementDefine.SUBTYPE.CELL_NUM:
                    ret = ReadFromRegImg(p, ref wdata);
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                    {
                        p.phydata = ElementDefine.PARAM_PHYSICAL_ERROR;
                        break;
                    }
                    if (wdata >= 2)
                        p.phydata = wdata - 2;
                    else
                        p.phydata = 0;
                    break;
                case ElementDefine.SUBTYPE.DOT_TH:
                    ret = ReadFromRegImg(p, ref wdata);
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                    {
                        p.phydata = ElementDefine.PARAM_PHYSICAL_ERROR;
                        break;
                    }
                    if (wdata >= 1)
                        p.phydata = wdata - 1;
                    else
                        p.phydata = 0;
                    break;
                case ElementDefine.SUBTYPE.OVP:
                    ret = ReadFromRegImg(p, ref wdata);
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                    {
                        p.phydata = ElementDefine.PARAM_PHYSICAL_ERROR;
                        break;
                    }
                    if (wdata < 0x0a)
                        wdata = 0x0a;
                    else if (wdata > 0x3c)
                        wdata = 0x3c;
                    dtmp = (double)((double)wdata * p.phyref / p.regref);
                    p.phydata = dtmp + p.offset;
                    break;
                default:
                    ret = ReadFromRegImg(p, ref wdata);
                    if (ret != LibErrorCode.IDS_ERR_SUCCESSFUL)
                    {
                        p.phydata = ElementDefine.PARAM_PHYSICAL_ERROR;
                        break;
                    }
                    dtmp = (double)((double)wdata * p.phyref / p.regref);
                    p.phydata = dtmp + p.offset;
                    break;
            }
            FromHexToPhy = true;
            /*if (parent.fromCFG == true)
            {
                byte tmp = 0;
                if (p.guid == ElementDefine.E_DOT)
                {
                    #region 根据DOT修改ENABLE
                    tmp = (byte)(parent.m_OpRegImg[0x18].val & 0x03);
                    if (tmp < 2)
                        parent.pE_T_E.phydata = 1;
                    else
                        parent.pE_T_E.phydata = 0;

                    #endregion
                }
                else if (p.guid == ElementDefine.O_DOT)
                {
                    #region 根据DOT修改ENABLE

                    tmp = (byte)(parent.m_OpRegImg[0x28].val & 0x03);
                    if (tmp < 2)
                        parent.pO_T_E.phydata = 1;
                    else
                        parent.pO_T_E.phydata = 0;

                    #endregion
                }
            }*/
        }

    }
}
