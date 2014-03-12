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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.AsyncDriver;
using ConsoleTestApplication;

namespace ReplicaSetTestApplication
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                // var uri = "mongodb://username:password@thinkmate:10001/?clusterType=replicaSet;endPoint=thinkmate:10002;endPoint=thinkmate:10003";
                var uri2 = "mongodb://username:password@thinkmate:10001/?clusterType=replicaSet";
                var uri = "mongodb://10.253.76.8:29012/?clusterType=replicaSet&ping=1s";
                var settings = new ClusterSettings(uri);

                var logger = new Logger(Console.Out, new StreamWriter("log.txt"));
                settings = settings.WithClusterListener(logger).WithMessageListener(logger);

                using (var cluster = Cluster.Create(settings))
                {
                    MainAsync(cluster).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception");
                Console.WriteLine(ex);
            }

            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
        }

        private static async Task MainAsync(ICluster cluster)
        {
            var minimumRevision = 0;
            while (true)
            {
                var info = await cluster.GetInfoAsync(minimumRevision);
                Console.WriteLine("clusterInfo = {0}", info);
                minimumRevision = info.Revision + 1;
            }
        }
    }
}
