﻿/* Copyright 2013-2014 MongoDB Inc.
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
    public class UsernamePasswordCredential : ICredential
    {
        // fields
        private string _source;
        private string _password;
        private string _username;

        // constructors
        public UsernamePasswordCredential(string source, string username, string password)
        {
            _source = source;
            _username = username;
            _password = password;
        }

        // properties
        public string Password
        {
            get { return _password; }
        }

        public string Username
        {
            get { return _username; }
        }

        public string Source
        {
            get { return _source; }
        }
    }
}
