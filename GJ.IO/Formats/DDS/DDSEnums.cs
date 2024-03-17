﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDSLib
{
    public static class DDSEnums
    {
        public enum DXGIFormat : uint
        {
            UNKNOWN = 0,
            R32G32B32A32_TYPELESS = 1,
            R32G32B32A32_FLOAT = 2,
            R32G32B32A32_UINT = 3,
            R32G32B32A32_SINT = 4,
            R32G32B32_TYPELESS = 5,
            R32G32B32_FLOAT = 6,
            R32G32B32_UINT = 7,
            R32G32B32_SINT = 8,
            R16G16B16A16_TYPELESS = 9,
            R16G16B16A16_FLOAT = 10,
            R16G16B16A16_UNORM = 11,
            R16G16B16A16_UINT = 12,
            R16G16B16A16_SNORM = 13,
            R16G16B16A16_SINT = 14,
            R32G32_TYPELESS = 15,
            R32G32_FLOAT = 16,
            R32G32_UINT = 17,
            R32G32_SINT = 18,
            R32G8X24_TYPELESS = 19,
            D32_FLOAT_S8X24_UINT = 20,
            R32_FLOAT_X8X24_TYPELESS = 21,
            X32_TYPELESS_G8X24_UINT = 22,
            R10G10B10A2_TYPELESS = 23,
            R10G10B10A2_UNORM = 24,
            R10G10B10A2_UINT = 25,
            R11G11B10_FLOAT = 26,
            R8G8B8A8_TYPELESS = 27,
            R8G8B8A8_UNORM = 28,
            R8G8B8A8_UNORM_SRGB = 29,
            R8G8B8A8_UINT = 30,
            R8G8B8A8_SNORM = 31,
            R8G8B8A8_SINT = 32,
            R16G16_TYPELESS = 33,
            R16G16_FLOAT = 34,
            R16G16_UNORM = 35,
            R16G16_UINT = 36,
            R16G16_SNORM = 37,
            R16G16_SINT = 38,
            R32_TYPELESS = 39,
            D32_FLOAT = 40,
            R32_FLOAT = 41,
            R32_UINT = 42,
            R32_SINT = 43,
            R24G8_TYPELESS = 44,
            D24_UNORM_S8_UINT = 45,
            R24_UNORM_X8_TYPELESS = 46,
            X24_TYPELESS_G8_UINT = 47,
            R8G8_TYPELESS = 48,
            R8G8_UNORM = 49,
            R8G8_UINT = 50,
            R8G8_SNORM = 51,
            R8G8_SINT = 52,
            R16_TYPELESS = 53,
            R16_FLOAT = 54,
            D16_UNORM = 55,
            R16_UNORM = 56,
            R16_UINT = 57,
            R16_SNORM = 58,
            R16_SINT = 59,
            R8_TYPELESS = 60,
            R8_UNORM = 61,
            R8_UINT = 62,
            R8_SNORM = 63,
            R8_SINT = 64,
            A8_UNORM = 65,
            R1_UNORM = 66,
            R9G9B9E5_SHAREDEXP = 67,
            R8G8_B8G8_UNORM = 68,
            G8R8_G8B8_UNORM = 69,
            BC1_TYPELESS = 70,
            BC1_UNORM = 71,
            BC1_UNORM_SRGB = 72,
            BC2_TYPELESS = 73,
            BC2_UNORM = 74,
            BC2_UNORM_SRGB = 75,
            BC3_TYPELESS = 76,
            BC3_UNORM = 77,
            BC3_UNORM_SRGB = 78,
            BC4_TYPELESS = 79,
            BC4_UNORM = 80,
            BC4_SNORM = 81,
            BC5_TYPELESS = 82,
            BC5_UNORM = 83,
            BC5_SNORM = 84,
            B5G6R5_UNORM = 85,
            B5G5R5A1_UNORM = 86,
            B8G8R8A8_UNORM = 87,
            B8G8R8X8_UNORM = 88,
            R10G10B10_XR_BIAS_A2_UNORM = 89,
            B8G8R8A8_TYPELESS = 90,
            B8G8R8A8_UNORM_SRGB = 91,
            B8G8R8X8_TYPELESS = 92,
            B8G8R8X8_UNORM_SRGB = 93,
            BC6H_TYPELESS = 94,
            BC6H_UF16 = 95,
            BC6H_SF16 = 96,
            BC7_TYPELESS = 97,
            BC7_UNORM = 98,
            BC7_UNORM_SRGB = 99,
            AYUV = 100,
            Y410 = 101,
            Y416 = 102,
            NV12 = 103,
            P010 = 104,
            P016 = 105,
            OPAQUE_420 = 106,
            YUY2 = 107,
            Y210 = 108,
            Y216 = 109,
            NV11 = 110,
            AI44 = 111,
            IA44 = 112,
            P8 = 113,
            A8P8 = 114,
            B4G4R4A4_UNORM = 115,
            P208 = 130,
            V208 = 131,
            V408 = 132,
            SAMPLER_FEEDBACK_MIN_MIP_OPAQUE,
            SAMPLER_FEEDBACK_MIP_REGION_USED_OPAQUE,
            FORCE_UINT = 0xffffffff
        }
        [Flags]
        public enum DDSFlags : uint
        {
            UseCaps = 0x1,
            UseHeight = 0x2,
            UseWidth = 0x4,
            UsePitch = 0x8,
            UsePixelFormat = 0x1000,
            UseMipMapCount = 0x20000,
            UseLinearSize = 0x80000,
            UseDepth = 0x800000,
        }
        [Flags]
        public enum DDSPixelFormatFlags : uint
        {
            HasAlphaPixels = 0x1,
            HasAlpha = 0x2,
            HasFourCC = 0x4,
            HasRGB = 0x40,
            HasYUV = 0x200,
            HasLuminance = 0x20000,
        }
        [Flags]
        public enum DDSCaps : uint
        {
            Complex = 0x8,
            Texture = 0x1000,
            MipMap =  0x400000
        }
        [Flags]
        public enum DDSCaps2 : uint
        {
            CubeMap = 0x200,
            CubeMap_PositiveX = 0x400,
            CubeMap_NegativeX = 0x800,
            CubeMap_PositiveY = 0x1000,
            CubeMap_NegativeY = 0x2000,
            CubeMap_PositiveZ = 0x4000,
            CubeMap_NegativeZ = 0x8000,
            Volume = 0x200000,
        }
        [Flags]
        public enum DDSMiscFlags : uint
        {
            TextureCube = 4
        }
        [Flags]
        public enum DDSMiscFlags2 : uint
        {
            Unknown = 0,
            AlphaModeStraight = 1,
            AlphaModePremultiplied = 2,
            AlphaModeOpaque = 3,
            AlphaModeCustom = 4,
        }
        [Flags]
        public enum D3D10ResourceDimention : uint
        {
            Unknown = 0,
            Buffer = 1,
            Texture1D = 2,
            Texture2D = 3,
            Texture3D = 4,
        }
        public enum DDSFourCC : uint
        {
            DXT1 = 0x31545844,
            DXT2 = 0x32545844,
            DXT3 = 0x33545844,
            DXT4 = 0x34545844,
            DXT5 = 0x35545844,

            ATI1 = 0x31495441,
            ATI2 = 0x32495441,

            DX10 = 0x30315844,

            BC1 = 0x314342,
            BC1U = 0x55314342,
            BC2 = 0x324342,
            BC2U = 0x55324342,
            BC3 = 0x334342,
            BC3U = 0x55334342,
            BC4 = 0x344342,
            BC4U = 0x55344342,
            BC5 = 0x354342,
            BC5U = 0x55354342,
            BC6 = 0x364342,
            BC6U = 0x55364342,
            BC7 = 0x374342,
            BC7U = 553274342,
        }
    }
}
