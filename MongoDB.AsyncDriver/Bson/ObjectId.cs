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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.AsyncDriver
{
    public struct ObjectId
    {
        #region static
        // static fields
        private static readonly ObjectId __empty = default(ObjectId);
        private static readonly int __machine = GetMachineHash();
        private static readonly short __pid = GetProcessId();
        private static int __incremement = (new Random()).Next();

        // static operators
        public static bool operator ==(ObjectId lhs, ObjectId rhs)
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(ObjectId lhs, ObjectId rhs)
        {
            return !(lhs == rhs);
        }

        // static properties
        public static ObjectId Empty
        {
            get { return __empty; }
        }

        // static methods
        private static int GetMachineHash()
        {
            var hostName = Environment.MachineName; // use instead of Dns.HostName so it will work offline
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(hostName));
            return (hash[0] << 16) + (hash[1] << 8) + hash[2]; // use first 3 bytes of hash
        }

        private static short GetProcessId()
        {
            try
            {
                return (short)GetProcessIdHelper(); // use low order two bytes only
            }
            catch (SecurityException)
            {
                return 0;
            }
        }

        /// Gets the current process id.  This method exists because of how CAS operates on the call stack, checking
        /// for permissions before executing the method.  Hence, if we inlined this call, the calling method would not execute
        /// before throwing an exception requiring the try/catch at an even higher level that we don't necessarily control.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetProcessIdHelper()
        {
            return Process.GetCurrentProcess().Id;
        }

        public static ObjectId NewObjectId()
        {
            return NewObjectId(DateTime.UtcNow);
        }

        public static ObjectId NewObjectId(DateTime dateTime)
        {
            return NewObjectId(ToTimestamp(dateTime));
        }

        public static ObjectId NewObjectId(int timestamp)
        {
            var increment = Interlocked.Increment(ref __incremement) & 0x00ffffff;
            return new ObjectId(timestamp, __machine, __pid, increment);
        }

        private static int ToTimestamp(DateTime dateTime)
        {
            return (int)Math.Floor((Utils.ToUniversalTime(dateTime) - Constants.UnixEpoch).TotalSeconds);
        }
        #endregion

        // fields
        private readonly int _timestamp;
        private readonly int _machine;
        private readonly short _pid;
        private readonly int _increment;

        // constructors
        public ObjectId(byte[] bytes)
        {
            _timestamp = (bytes[0] << 24) + (bytes[1] << 16) + (bytes[2] << 8) + bytes[3];
            _machine = (bytes[4] << 16) + (bytes[5] << 8) + bytes[6];
            _pid = (short)((bytes[7] << 8) + bytes[8]);
            _increment = (bytes[9] << 16) + (bytes[10] << 8) + bytes[11];
        }

        public ObjectId(byte[] bytes, int offset)
        {
            _timestamp = (bytes[offset + 0] << 24) + (bytes[offset + 1] << 16) + (bytes[offset + 2] << 8) + bytes[offset + 3];
            _machine = (bytes[offset + 4] << 16) + (bytes[offset + 5] << 8) + bytes[offset + 6];
            _pid = (short)((bytes[offset + 7] << 8) + bytes[offset + 8]);
            _increment = (bytes[offset + 9] << 16) + (bytes[offset + 10] << 8) + bytes[offset + 11];
        }

        public ObjectId(int timestamp, int machine, short pid, int increment)
        {
            if ((machine & 0xff000000) != 0)
            {
                throw new ArgumentException("Machine value must fit in 3 bytes.");
            }
            if ((increment & 0xff000000) != 0)
            {
                throw new ArgumentException("Increment value must fit in 3 bytes.");
            }
            _timestamp = timestamp;
            _machine = machine;
            _pid = pid;
            _increment = increment;
        }

        // properties
        public int Increment
        {
            get { return _increment; }
        }

        public int Machine
        {
            get { return _machine; }
        }

        public short Pid
        {
            get { return _pid; }
        }

        public int Timestamp
        {
            get { return _timestamp; }
        }

        // methods
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(ObjectId))
            {
                return false;
            }
            var rhs = (ObjectId)obj;
            return
                _timestamp == rhs._timestamp &&
                _machine == rhs._machine &&
                _pid == rhs._pid &&
                _increment == rhs._increment;
        }

        public override int GetHashCode()
        {
            return new Hasher().Hash(_timestamp).Hash(_machine).Hash(_pid).Hash(_increment).GetHashCode();
        }

        public byte[] ToByteArray()
        {
            var bytes = new byte[12];
            bytes[0] = (byte)(_timestamp >> 24);
            bytes[1] = (byte)(_timestamp >> 16);
            bytes[2] = (byte)(_timestamp >> 8);
            bytes[3] = (byte)(_timestamp);
            bytes[4] = (byte)(_machine >> 16);
            bytes[5] = (byte)(_machine >> 8);
            bytes[6] = (byte)(_machine);
            bytes[7] = (byte)(_pid >> 8);
            bytes[8] = (byte)(_pid);
            bytes[9] = (byte)(_increment >> 16);
            bytes[10] = (byte)(_increment >> 8);
            bytes[11] = (byte)(_increment);
            return bytes;
        }

        public override string ToString()
        {
            return string.Format("ObjectId('{0}')", Hex.ToString(ToByteArray()));
        }
    }
}
