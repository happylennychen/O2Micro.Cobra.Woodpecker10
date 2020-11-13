using Cobra.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cobra.Woodpecker10
{
    public static class Utility
    {
        public static List<byte> GenerateRegisterList(ref TASKMessage msg)
        {
            Reg reg = null;
            byte baddress = 0;
            List<byte> OpReglist = new List<byte>();

            ParamContainer demparameterlist = msg.task_parameterlist;
            if (demparameterlist == null) return null;

            foreach (Parameter p in demparameterlist.parameterlist)
            {
                if ((p.guid & ElementDefine.SectionMask) == ElementDefine.VirtualElement)    //略过虚拟参数
                    continue;
                if (p == null) break;
                foreach (KeyValuePair<string, Reg> dic in p.reglist)
                {
                    reg = dic.Value;
                    baddress = (byte)reg.address;
                    if(OpReglist.Contains(baddress) == false)
                        OpReglist.Add(baddress);
                }
            }
            OpReglist = OpReglist.Distinct().ToList();
            return OpReglist;
        }
        public static List<Parameter> GenerateParameterList(ref TASKMessage msg)
        {
            List<Parameter> OpParamList = new List<Parameter>();

            ParamContainer demparameterlist = msg.task_parameterlist;
            if (demparameterlist == null) return null;

            foreach (Parameter p in demparameterlist.parameterlist)
            {
                if ((p.guid & ElementDefine.SectionMask) == ElementDefine.VirtualElement)    //略过虚拟参数
                    continue;
                if (p == null) break;
                OpParamList.Add(p);
            }
            OpParamList = OpParamList.Distinct().ToList();
            return OpParamList;
        }
    }
}
