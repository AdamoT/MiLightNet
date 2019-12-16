namespace MiLightNet.Controllers.V6
{
    internal class MiLightMessageV6 : IMiLightMessage
    {
        #region IMiLightMessage

        public byte[] GetData() => _Data;

        public virtual void CalculateChecksum()
        {
            byte checkSum = 0x00;
            for (int i = _Data.Length - 12; i < _Data.Length - 1; ++i)
                checkSum += _Data[i];
            _Data[21] = checkSum;
        }

        #endregion IMiLightMessage

        #region Properties

        public static readonly MiLightMessageV6 IdsDiscoveryMsg = new DiscoverIDs();

        public byte ID1 { get { return _Data[5]; } set { _Data[5] = value; } }
        public byte ID2 { get => _Data[6]; set => _Data[6] = value; }
        public byte SequenceNo { get => _Data[8]; set => _Data[8] = value; }
        public DeviceTypes DeviceType { get => (DeviceTypes)_Data[13]; set => _Data[13] = (byte)value; }
        public Commands Command { get => (Commands)_Data[14]; set => _Data[14] = (byte)value; }
        public int Value
        {
            set
            {
                _Data[15] = (byte)((value >> 24) & 0xFF);
                _Data[16] = (byte)((value >> 16) & 0xFF);
                _Data[17] = (byte)((value >> 8) & 0xFF);
                _Data[18] = (byte)((value & 0xFF));
            }
        }
        public byte Zone { get => _Data[19]; set => _Data[19] = value; }

        #endregion Properties

        #region Protected Fields

        protected byte[] _Data = null;

        #endregion Protected Fields

        #region Constructors

        public MiLightMessageV6()
        {
            _Data = new byte[]
            {
                0x80,
                0x00,
                0x00,
                0x00,
                0x11,//Length
                0, //ID1 - 5
                0, //ID2 - 6
                0x00,
                0, //SequenceNo - 8
                0x00,
                0x31,//Data start
                0x00,
                0x00,
                0,//DeviceType - 13
                0,//Command - 14
                0,//Value - 15
                0,//Value - 16
                0,//Value - 17
                0,//Value - 18
                0,//Zone - 19
                0x00,
                0x00,//Checksum - 21
            };
        }

        #endregion Constructors

        #region Types

        private class DiscoverIDs : MiLightMessageV6
        {
            public DiscoverIDs()
            {
                _Data = new byte[] { 0x20, 0x00, 0x00, 0x00, 0x16, 0x02, 0x62, 0x3A, 0xD5, 0xED, 0xA3, 0x01, 0xAE, 0x08, 0x2D, 0x46, 0x61, 0x41, 0xA7, 0xF6, 0xDC, 0xAF, 0xD3, 0xE6, 0x00, 0x00, 0x1E };
            }

            public override void CalculateChecksum() { }
        }

        #endregion Types
    }
}
