﻿// xivModdingFramework
// Copyright © 2018 Rafael Gonzalez - All Rights Reserved
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace xivModdingFramework.Models.Enums
{
    /// <summary>
    /// Enum containing the Data Type for data entries in the Vertex Data Blocks
    /// </summary>
    public enum VertexDataType
    {
        Float1   = 0x0,
        Float2   = 0x1,
        Float3   = 0x2,
        Float4   = 0x3,
        Ubyte4   = 0x5,
        Short2   = 0x6,
        Short4   = 0x7,
        Ubyte4n  = 0x8,
        Short2n  = 0x9,
        Short4n  = 0xA,
        Ushort2n = 0xB,
        Ushort4n = 0xC,
        Half2    = 0xF,
        Half4    = 0x10,
        Unknown17 = 0x11
    }
}