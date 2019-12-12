﻿using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using System.Net.NetworkInformation;
using System.Threading;
using MiLightNet.Utils;
using System.IO;

namespace MiLightNet.Controllers.V6
{
    /// <summary>
    /// <see cref="http://www.limitlessled.com/dev/"/>
    /// </summary>
    public class MiLightControllerV6 : IMiLightController
    {
        #region IMiLightController

        public PhysicalAddress Mac { get; }
        public IPEndPoint EndPoint { get; private set; }

        #endregion IMiLightController

        #region Properties

        public int SendRepeatCount { get; set; } = 3;

        #endregion Properties

        #region Consts

        public const int DefaultV6Port = 5987;

        #endregion Consts

        #region Private Fields

        private UdpClient Udp
        {
            get
            {
                if(_Udp == null)
                {
                    _Udp = new UdpClient();
                    _Udp.Connect(EndPoint);
                }
                return _Udp;
            }
        }
        private UdpClient _Udp = null;

        private byte _SequenceNo = 0;
        private byte _Id1 = 0;
        private byte _Id2 = 0;
        private bool _IsInitialized = false;        

        private SemaphoreSlim _Semaphore = new SemaphoreSlim(1, 1);

        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// </summary>
        /// <param name="ipEndPoint">IP Endpoint of the controller</param>
        /// <param name="mac">Optional, mac address of the controller</param>
        public MiLightControllerV6(IPEndPoint ipEndPoint, PhysicalAddress mac = null)
        {
            EndPoint = ipEndPoint ?? throw new ArgumentNullException(nameof(ipEndPoint));
            Mac = mac ?? PhysicalAddress.None;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Turns the light ON or OFF
        /// </summary>
        /// <param name="zoneID">Zone</param>
        /// <param name="value">True to turn the light on, false to turn it off</param>
        /// <returns>Awaitable Task</returns>
        public async Task SetOnOff(MiLightZones zone, bool value)
        {
            var msg = new MiLightMessageV6();
            if (zone == MiLightZones.Bridge)
            {//Bridge
                msg.DeviceType = DeviceTypes.BridgeLamp;
                msg.Command = Commands.Bridge_SetState;
                msg.Zone = 0;
                msg.Value = (int)(value ? CommandValues.Bridge_On : CommandValues.Bridge_Off);
            }
            else if (zone < MiLightZones.NotZone)
            {//zone
                msg.DeviceType = DeviceTypes.RGBWW;
                msg.Command = Commands.RGBWW_SetState;
                msg.Zone = (byte)zone;
                msg.Value = (int)(value ? CommandValues.RGBWW_On : CommandValues.RGBWW_Off);
            }
            else throw new ArgumentOutOfRangeException(nameof(zone));

            await SendMsgValidateResponse(msg);
        }

        /// <summary>
        /// Sets hue value of color lights
        /// </summary>
        /// <param name="zoneID">Zone</param>
        /// <param name="value">Hue value (0 - 255)</param>
        /// <returns>Awaitable Task</returns>
        public async Task SetHue(MiLightZones zone, byte value)
        {
            var msg = new MiLightMessageV6()
            {
                Value = value | (value << 8) | (value << 16) | (value << 24),
            };
            if(zone == MiLightZones.Bridge)
            {
                msg.DeviceType = DeviceTypes.BridgeLamp;
                msg.Command = Commands.Bridge_SetColor;
                msg.Zone = 0;
            }
            else if(zone < MiLightZones.NotZone)
            {
                msg.DeviceType = DeviceTypes.RGBWW;
                msg.Command = Commands.RGBWW_SetColor;
                msg.Zone = (byte)zone;
            }
            await SendMsgValidateResponse(msg);
        }

        /// <summary>
        /// Sets the brightness of current light setup
        /// </summary>
        /// <param name="zoneID">Zone</param>
        /// <param name="value">Brightness in (0 - 100) Range </param>
        /// <returns>Awaitable Task</returns>
        public async Task SetBrightness(MiLightZones zone, byte value)
        {
            var msg = new MiLightMessageV6()
            {
                Value = value << 24,
            };

            if (zone == MiLightZones.Bridge)
            {
                msg.DeviceType = DeviceTypes.BridgeLamp;
                msg.Command = Commands.Brdige_SetBrightness;
                msg.Zone = 0;
            }
            else if (zone < MiLightZones.NotZone)
            {
                msg.DeviceType = DeviceTypes.RGBWW;
                msg.Command = Commands.RGBWW_SetBrightness;
                msg.Zone = (byte)zone;
            }
            else throw new ArgumentOutOfRangeException(nameof(zone));

            await SendMsgValidateResponse(msg);
        }

        /// <summary>
        /// Turns on white light
        /// </summary>
        /// <param name="zoneID">Zone</param>
        /// <returns>Awaitable Task</returns>
        public async Task SetWhite(MiLightZones zone)
        {
            var msg = new MiLightMessageV6();
            if (zone == MiLightZones.Bridge)
            {
                msg.DeviceType = DeviceTypes.BridgeLamp;
                msg.Command = Commands.Bridge_SetWhite;
                msg.Zone = 0;
                msg.Value = (int)CommandValues.Bridge_White;
            }
            else if (zone < MiLightZones.NotZone)
            {
                msg.DeviceType = DeviceTypes.RGBWW;
                msg.Command = Commands.RGBWW_SetWhite;
                msg.Zone = (byte)zone;
                msg.Value = (int)CommandValues.RGBWW_White;
            }
            else throw new ArgumentOutOfRangeException(nameof(zone));

            await SendMsgValidateResponse(msg);
        }

        /// <summary>
        /// Activates night mode
        /// </summary>
        /// <param name="zone">Zone</param>
        /// <returns>Awaitable Task</returns>
        public async Task SetNightMode(MiLightZones zone)
        {
            if (zone < MiLightZones.NotZone)
            {
                var msg = new MiLightMessageV6()
                {
                    DeviceType = DeviceTypes.RGBWW,
                    Command = Commands.RGBWW_SetState,
                    Zone = (byte)zone,
                    Value = (int)CommandValues.RGBWW_NightLight,
                };

                await SendMsgValidateResponse(msg);
            }
            else throw new ArgumentOutOfRangeException(nameof(zone));
        }

        /// <summary>
        /// Sets dynamic color/saturation change mode
        /// </summary>
        /// <param name="zone">Zone</param>
        /// <param name="value">Mode</param>
        /// <returns></returns>
        public async Task SetDynamicMode(MiLightZones zone, MiLightV6DynamicModes mode)
        {
            if (mode >= MiLightV6DynamicModes.Invalid)
                throw new ArgumentOutOfRangeException(nameof(mode));

            var msg = new MiLightMessageV6()
            {
                Value = (byte)(mode) << 24,
            };
            if (zone == MiLightZones.Bridge)
            {//TODO: This doesn't seem to work at all
                msg.DeviceType = DeviceTypes.BridgeLamp;
                msg.Command = Commands.Bridge_SetMode;
                msg.Zone = 0;
            }
            else if (zone < MiLightZones.NotZone)
            {
                msg.DeviceType = DeviceTypes.RGBWW;
                msg.Command = Commands.RGBWW_SetMode;
                msg.Zone = (byte)zone;
            }
            else throw new ArgumentOutOfRangeException(nameof(zone));

            await SendMsgValidateResponse(msg);
        }

        /// <summary>
        /// Changes mode changing speed
        /// </summary>
        /// <param name="zoneID">Zone</param>
        /// <param name="increase">True to increase the speed, false to decrease</param>
        /// <returns>Awaitable Task</returns>
        public async Task ChangeModeSpeed(MiLightZones zone, bool increase)
        {
            var msg = new MiLightMessageV6();
            if (zone == MiLightZones.Bridge)
            {
                msg.DeviceType = DeviceTypes.BridgeLamp;
                msg.Command = Commands.Bridge_SetModeSpeed;
                msg.Zone = 0;
                msg.Value = (int)(increase ? CommandValues.Bridge_ModeSpeedInc : CommandValues.Bridge_ModeSpeedDec);
            }
            else if(zone < MiLightZones.NotZone)
            {
                msg.DeviceType = DeviceTypes.RGBWW;
                msg.Command = Commands.RGBWW_SetModeSpeed;
                msg.Zone = (byte)zone;
                msg.Value = (int)((increase) ? CommandValues.RGBWW_ModeSpeedInc : CommandValues.RGBWW_ModeSpeedDec);
            }
            else throw new ArgumentOutOfRangeException(nameof(zone));

            await SendMsgValidateResponse(msg);
        }

        #endregion Public Methods
        
        #region Private Methods

        private async Task Initialize()
        {
            if (_IsInitialized)
                return;           

            try
            {
                await _Semaphore.WaitAsync();
                if (_IsInitialized)
                    return;

                var response = await SendMsgReceiveResponse(MiLightMessageV6.IdsDiscoveryMsg);

                _Id1 = response[19];
                _Id2 = response[20];

                _IsInitialized = true;
            }
            finally
            {
                _Semaphore.Release();
            }
        }

        private async Task SendMsgValidateResponse(MiLightMessageV6 msg)
        {
            if (!_IsInitialized)
                await Initialize();

            try
            {
                await _Semaphore.WaitAsync();
                msg.SequenceNo = _SequenceNo++;
                msg.ID1 = _Id1;
                msg.ID2 = _Id2;

                var response = await SendMsgReceiveResponse(msg);
                if (await ValidateResponse(msg, response))
                    return;
                else throw new System.IO.IOException("Failed to receive valid response");
            }
            finally
            {
                _Semaphore.Release();
            }
        }

        private async Task<byte[]> SendMsgReceiveResponse(MiLightMessageV6 msg)
        {
            msg.CalculateChecksum();

            for (int i = 0; i < SendRepeatCount; ++i)
            {
                try
                {
                    var result = await UdpUtils.ExchangeDatagram(Udp, msg.Data, TimeSpan.FromSeconds(1));
                    return result.Data;
                }
                catch(IOException)
                {
                    continue;
                }
            }

            throw new IOException("Failed to receive response");
        }

        private async Task<bool> ValidateResponse(IMiLightMessage msg, byte[] data)
        {
            if(data[data.Length - 1] != 0)
            {//Last byte should be zero
                await Initialize();
                return false;
            }

            return true;
        }

        #endregion Private Methods
    }
}