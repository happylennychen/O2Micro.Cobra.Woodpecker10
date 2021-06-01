using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cobra.Common;

namespace Cobra.Woodpecker10
{
    /// <summary>
    /// 数据结构定义
    ///     XX       XX        XX         XX
    /// --------  -------   --------   -------
    ///    保留   参数类型  寄存器地址   起始位
    /// </summary>
    public class ElementDefine
    {
        #region Chip Constant
        internal const UInt16 EF_MEMORY_SIZE = 0x10;
        internal const UInt16 EF_MEMORY_OFFSET = 0x10;
        internal const UInt16 EF_ATE_OFFSET = 0x10;
        internal const UInt16 EF_ATE_TOP = 0x17;
        internal const UInt16 ATE_CRC_OFFSET = 0x17;

        //internal const UInt16 EF_CFG = 0x16;
        internal const UInt16 EF_USR_OFFSET = 0x18;
        internal const UInt16 EF_USR_TOP = 0x1d;

        internal const UInt16 OP_USR_OFFSET = 0x28;
        internal const UInt16 OP_USR_TOP = 0x2d;
        internal const UInt16 OP_SW_MAPPING = 0x39;

        internal const UInt16 OP_MEMORY_SIZE = 0xFF;
        internal const Byte PARAM_HEX_ERROR = 0xFF;
        internal const Double PARAM_PHYSICAL_ERROR = -999999;
		
        internal const int RETRY_COUNTER = 15;
		internal const byte WORKMODE_OFFSET = 0x40;
        internal const byte MAPPINGDISABLE_OFFSET = 0x40;
        internal const UInt32 SectionMask = 0xFFFF0000;





        #region 温度参数GUID
        internal const UInt32 TemperatureElement = 0x00010000;
        internal const UInt32 TpRsense = TemperatureElement + 0x00;
        #endregion

        #region Efuse参数GUID
        internal const UInt32 EFUSEElement = 0x00031000;    //0x10~0x1f
        internal const UInt32 E_BAT_TYPE = 0x00031a07;
        internal const UInt32 E_OVP_TH = 0x00031900;
        internal const UInt32 E_DOT_TH = 0x00031802;
        internal const UInt32 E_OVR_HYS = 0x00031d04;
        internal const UInt32 E_UVR_HYS = 0x00031a04;
        internal const UInt32 E_UVP_TH = 0x00031a00;
        internal const UInt32 E_TRIM_RSVD = 0x00031007;
        #endregion

        #region Operation参数GUID
        internal const UInt32 OperationElement = 0x00030000;    //0x30~0xff
        internal const UInt32 MappingElement = 0x00032000;    //0x30~0xff

        internal const UInt32 O_BAT_TYPE = 0x00032a07;
        internal const UInt32 O_OVP_TH = 0x00032900;
        internal const UInt32 O_DOT_TH = 0x00032802;
        internal const UInt32 O_OVR_HYS = 0x00032d04;
        internal const UInt32 O_UVR_HYS = 0x00032a04;
        internal const UInt32 O_UVP_TH = 0x00032a00;
        internal const UInt32 O_TRIM_RSVD = 0x00032007;

        #endregion

        #region Virtual parameters
        internal const UInt32 VirtualElement = 0x000c0000;

        //internal const UInt32 E_DOT_E = 0x000c0001; //Efuse T Enable
        //internal const UInt32 O_DOT_E = 0x000c0002; //OP T Enable
        #endregion

        #region EFUSE操作常量定义
        internal const byte EFUSE_DATA_OFFSET = 0x10;
        internal const byte EFUSE_MAP_OFFSET = 0x20;
        internal const byte OPERATION_OFFSET = 0x30;

        // EFUSE operation code

        // EFUSE control registers' addresses
        internal const byte WORKMODE_REG = 0x40;
        //internal const byte EFUSE_TESTCTR_REG = 0x41;
        //internal const byte EFUSE_ATE_FROZEN_REG = 0x04;
        //internal const byte EFUSE_USER_FROZEN_REG = 0x07;

        // EFUSE Control Flags
        internal const byte ALLOW_WR_FLAG = 0x80;
        internal const byte EFUSE_FROZEN_FLAG = 0x80;
        internal const UInt16 EF_TOTAL_PARAMS = 25; //需要修改
        #endregion


        #endregion

        internal enum SUBTYPE : ushort
        {
            DEFAULT = 0,
            DOT_TH = 1,
            OVP = 2,
            CELL_NUM = 3,
            
            EXT_TEMP_TABLE = 40,
            INT_TEMP_REFER = 41
        }

        #region Local ErrorCode
        //internal const UInt32 IDS_ERR_DEM_POWERON_FAILED = LibErrorCode.IDS_ERR_SECTION_DYNAMIC_DEM + 0x0001;
        //internal const UInt32 IDS_ERR_DEM_POWEROFF_FAILED = LibErrorCode.IDS_ERR_SECTION_DYNAMIC_DEM + 0x0002;
        //internal const UInt32 IDS_ERR_DEM_POWERCHECK_FAILED = LibErrorCode.IDS_ERR_SECTION_DYNAMIC_DEM + 0x0003;
        //internal const UInt32 IDS_ERR_DEM_FROZEN_EFUSE = LibErrorCode.IDS_ERR_SECTION_DYNAMIC_DEM + 0x0004;
        internal const UInt32 IDS_ERR_DEM_FROZEN = LibErrorCode.IDS_ERR_SECTION_DYNAMIC_DEM + 0x0005;
        //internal const UInt32 IDS_ERR_DEM_BLOCK = LibErrorCode.IDS_ERR_SECTION_DYNAMIC_DEM + 0x0006;
        internal const UInt32 IDS_ERR_DEM_ONE_PARAM_DISABLE = LibErrorCode.IDS_ERR_SECTION_DYNAMIC_DEM + 0x0007;
        internal const UInt32 IDS_ERR_DEM_READ_BACK_CHECK_FAILED = LibErrorCode.IDS_ERR_SECTION_DYNAMIC_DEM + 0x0008;
        #endregion

        public enum EFUSE_MODE : ushort
        {
            NORMAL = 0,
            WRITE_MAP_CTRL = 0x01,
            PROGRAM = 0x02,
        }

        internal enum COMMAND : ushort
        {
            //MP_FROZEN_BIT_CHECK_PC = 9,
            //MP_FROZEN_BIT_CHECK = 10,
            //MP_DIRTY_CHIP_CHECK_PC = 11,
            //MP_DIRTY_CHIP_CHECK = 12,
            //MP_DOWNLOAD_PC = 13,
            //MP_DOWNLOAD = 14,
            //MP_READ_BACK_CHECK_PC = 15,
            //MP_READ_BACK_CHECK = 16,
            //GET_EFUSE_HEX_DATA = 17,  //不再使用此命令，与OZ77系列统一
            EFUSE_CONFIG_SAVE_EFUSE_HEX = 18,
            //MP_BIN_FILE_CHECK = 21,                   //检查bin文件的合法性
            REGISTER_CONFIG_READ = 22,
            REGISTER_CONFIG_WRITE = 23,
            EFUSE_CONFIG_READ = 24,
            EFUSE_CONFIG_WRITE = 25
        }

    }
}
