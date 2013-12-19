/* Copyright 2013-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public static class GuidConverter
    {
        // static methods
        public static Guid FromBytes(byte[] bytes, GuidRepresentation representation)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (bytes.Length != 16)
            {
                throw new ArgumentException("Byte array must be 16 bytes long.");
            }

            bytes = (byte[])bytes.Clone();
            switch (representation)
            {
                case GuidRepresentation.CSharpLegacy:
                    return new Guid(bytes);

                case GuidRepresentation.JavaLegacy:
                    Array.Reverse(bytes, 0, 8);
                    Array.Reverse(bytes, 8, 8);
                    Array.Reverse(bytes, 0, 4);
                    Array.Reverse(bytes, 4, 2);
                    Array.Reverse(bytes, 6, 2);
                    return new Guid(bytes);

                case GuidRepresentation.Standard:
                case GuidRepresentation.PythonLegacy:
                    Array.Reverse(bytes, 0, 4);
                    Array.Reverse(bytes, 4, 2);
                    Array.Reverse(bytes, 6, 2);
                    return new Guid(bytes);

                case GuidRepresentation.Unknown:
                    throw new NotSupportedException("Cannot convert byte array to Guid because representation is Unknown.");

                default:
                    throw new ArgumentException("Invalid GuidRepresentation.");
            }
        }

        public static byte[] ToBytes(Guid guid, GuidRepresentation representation)
        {
            var bytes = (byte[])guid.ToByteArray().Clone();
            switch (representation)
            {
                case GuidRepresentation.CSharpLegacy:
                    return bytes;

                case GuidRepresentation.JavaLegacy:
                    Array.Reverse(bytes, 0, 4);
                    Array.Reverse(bytes, 4, 2);
                    Array.Reverse(bytes, 6, 2);
                    Array.Reverse(bytes, 0, 8);
                    Array.Reverse(bytes, 8, 8);
                    return bytes;

                case GuidRepresentation.Standard:
                case GuidRepresentation.PythonLegacy:
                    Array.Reverse(bytes, 0, 4);
                    Array.Reverse(bytes, 4, 2);
                    Array.Reverse(bytes, 6, 2);
                    return bytes;

                case GuidRepresentation.Unknown:
                    throw new NotSupportedException("Cannot convert Guid to byte array because representation is Unknown.");

                default:
                    throw new ArgumentException("Invalid GuidRepresentation.");
            }
        }
    }
}
