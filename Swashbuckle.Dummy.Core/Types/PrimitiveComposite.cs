using System;

namespace Swashbuckle.Dummy.Types
{
    public class PrimitiveComposite
    {
        public bool Boolean { get; set; }
        public byte Byte { get; set; }
        public sbyte SByte { get; set; }
        public short Int16 { get; set; }
        public ushort UInt16 { get; set; }
        public int Int32 { get; set; }
        public uint UInt32 { get; set; }
        public long Int64 { get; set; }
        public ulong UInt64 { get; set; }
        public float Single { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }
        public DateTime DateTime { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public Guid Guid { get; set; }
        public PrimitiveEnum Enum { get; set; }
        public char Char { get; set; }

        public bool? NullableBoolean { get; set; }
        public byte? NullableByte { get; set; }
        public sbyte? NullableSByte { get; set; }
        public short? NullableInt16 { get; set; }
        public ushort? NullableUInt16 { get; set; }
        public int? NullableInt32 { get; set; }
        public uint? NullableUInt32 { get; set; }
        public long? NullableInt64 { get; set; }
        public ulong? NullableUInt64 { get; set; }
        public float? NullableSingle { get; set; }
        public double? NullableDouble { get; set; }
        public decimal? NullableDecimal { get; set; }
        public DateTime? NullableDateTime { get; set; }
        public DateTimeOffset? NullableDateTimeOffset { get; set; }
        public TimeSpan? NullableTimeSpan { get; set; }
        public Guid? NullableGuid { get; set; }
        public PrimitiveEnum? NullableEnum { get; set; }
        public char? NullableChar { get; set; }
        public string String { get; set; }
    }
}
