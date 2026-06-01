using PokiePawsDesk.Core;
using System;
using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace PokiePawsDesk.Tests
{
    public class ConverterTests
    {
        private readonly PriceConverter _price = new();
        private readonly DateOnlyConverter _date = new();

        [Fact]
        public void PriceConverter_Convert_Null_ReturnsEmptyString()
            => Assert.Equal("", _price.Convert(null, typeof(string), null, CultureInfo.InvariantCulture));

        [Fact]
        public void PriceConverter_Convert_Double_ReturnsTwoDecimalDotFormat()
            => Assert.Equal("12.50", _price.Convert(12.5, typeof(string), null, CultureInfo.InvariantCulture));

        [Fact]
        public void PriceConverter_Convert_Zero_ReturnsZeroFormatted()
            => Assert.Equal("0.00", _price.Convert(0.0, typeof(string), null, CultureInfo.InvariantCulture));

        [Fact]
        public void PriceConverter_ConvertBack_DotSeparator_ReturnsParsedDouble()
            => Assert.Equal(12.5, _price.ConvertBack("12.50", typeof(double), null, CultureInfo.InvariantCulture));

        [Fact]
        public void PriceConverter_ConvertBack_CommaSeparator_ReturnsParsedDouble()
            => Assert.Equal(12.5, _price.ConvertBack("12,50", typeof(double), null, CultureInfo.InvariantCulture));

        [Fact]
        public void PriceConverter_ConvertBack_InvalidInput_ReturnsUnsetValue()
            => Assert.Equal(DependencyProperty.UnsetValue, _price.ConvertBack("abc", typeof(double), null, CultureInfo.InvariantCulture));

        [Fact]
        public void PriceConverter_ConvertBack_EmptyString_ReturnsNull()
            => Assert.Null(_price.ConvertBack("", typeof(double), null, CultureInfo.InvariantCulture));

        [Fact]
        public void PriceConverter_ConvertBack_Null_ReturnsNull()
            => Assert.Null(_price.ConvertBack(null, typeof(double), null, CultureInfo.InvariantCulture));

        [Fact]
        public void DateOnlyConverter_Write_FormatsAsDateOnlyWithoutTime()
        {
            var buffer = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(buffer);
            _date.Write(writer, new DateTime(2024, 5, 15), new JsonSerializerOptions());
            writer.Flush();
            Assert.Equal("\"2024-05-15\"", Encoding.UTF8.GetString(buffer.WrittenSpan));
        }

        [Fact]
        public void DateOnlyConverter_Write_Null_WritesNullToken()
        {
            var buffer = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(buffer);
            _date.Write(writer, null, new JsonSerializerOptions());
            writer.Flush();
            Assert.Equal("null", Encoding.UTF8.GetString(buffer.WrittenSpan));
        }

        [Fact]
        public void DateOnlyConverter_Read_DateString_ParsesCorrectDate()
        {
            var json = "\"2024-05-15\""u8;
            var reader = new Utf8JsonReader(json);
            reader.Read();
            var result = _date.Read(ref reader, typeof(DateTime?), new JsonSerializerOptions());
            Assert.NotNull(result);
            Assert.Equal(new DateTime(2024, 5, 15), result!.Value.Date);
        }
    }
}