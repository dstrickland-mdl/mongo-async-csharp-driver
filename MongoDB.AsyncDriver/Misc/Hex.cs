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
    public static class Hex
    {
        // static methods
        public static byte[] Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            byte[] bytes;
            if ((value.Length & 1) != 0)
            {
                value = "0" + value; // make length of value even
            }
            bytes = new byte[value.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string hex = value.Substring(2 * i, 2);
                try
                {
                    byte b = Convert.ToByte(hex, 16);
                    bytes[i] = b;
                }
                catch (FormatException ex)
                {
                    throw new FormatException(
                        string.Format("Invalid hex string {0}. Problem with substring {1} starting at position {2}",
                        value,
                        hex,
                        2 * i),
                        ex);
                }
            }

            return bytes;
        }

        public static string ToString(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            var sb = new StringBuilder(value.Length * 2);
            foreach (var b in value)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
    }
}
