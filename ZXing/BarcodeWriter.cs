﻿using ZXing.Common;

namespace ZXing
{
    internal class BarcodeWriter
    {
        public BarcodeFormat Format { get; set; }
        public EncodingOptions Options { get; set; }

        internal object Write(string skuIdName)
        {
            throw new NotImplementedException();
        }
    }
}