namespace MiLightNet.Controllers
{
    internal enum DeviceTypes : byte
    {
        BridgeLamp = 0x00,
        RGBW = 0x08,
        RGBWW = 0x07,
    }

    internal enum Commands : byte
    {
        Bridge_SetState = 0x03,
        Bridge_SetMode = 0x04,
        Bridge_SetModeSpeed = 0x03,
        Bridge_SetColor = 0x01,
        Bridge_SetWhite = 0x03,
        Brdige_SetBrightness = 0x02,

        RGBW_SetState = 0x04,
        RGBW_SetWhite = 0x05,
        RGBW_SetColor = 0x01,
        RGBW_SetSaturation = 0x02,
        RGBW_SetBrightness = 0x03,
        RGBW_SetMode = 0x06,
        RGBW_SetModeSpeed = 0x04,

        RGBWW_SetState = 0x03,
        RGBWW_SetBrightness = 0x02,
        RGBWW_SetColor = 0x01,
        RGBWW_SetWhite = 0x03,
        RGBWW_SetMode = 0x04,
        RGBWW_SetModeSpeed = 0x03,
    }

    internal enum CommandValues : int
    {
        Bridge_On = 0x03000000,
        Bridge_Off = 0x04000000,
        Bridge_ModeSpeedDec = 0x01000000,
        Bridge_ModeSpeedInc = 0x02000000,
        Bridge_White = 0x05000000,

        RGBW_On = 0x01000000,
        RGBW_Off = 0x02000000,
        RGBW_NightLight = 0x04050000,
        RGBW_ModeSpeedInc = 0x03000000,
        RGBW_ModeSpeedDec = 0x04000000,

        RGBWW_On = 0x01000000,
        RGBWW_Off = 0x02000000,
        RGBWW_ModeSpeedInc = 0x03000000,
        RGBWW_ModeSpeedDec = 0x04000000,
        RGBWW_White = 0x05000000,
        RGBWW_NightLight = 0x06000000,
    }
}
