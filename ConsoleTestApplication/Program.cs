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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MongoDB.AsyncDriver;
using InsertOperation = MongoDB.AsyncDriver.InsertOpcodeOperation;

namespace ConsoleTestApplication
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                MainAsync().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception:");
                Console.WriteLine(ex);
            }

            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }

        private static async Task MainAsync()
        {
            var uri = "mongodb://localhost/?ping=10s";
            var settings = new ClusterSettings(uri);

            var logger = new Logger(Console.Out, new StreamWriter("log.txt"));
            settings = settings
                .WithClusterListener(logger)
                .WithMessageListener(logger);

            //var server = new MockServer();
            //var servers = new Dictionary<DnsEndPoint, IMockServer> { { settings.EndPoints[0], server } };
            //var connectionPoolFactory = new MockConnectionPoolFactory(servers);
            //var nodeSettings = new NodeSettings.Builder(settings.NodeSettings) { ConnectionPoolFactory = connectionPoolFactory };
            //settings = settings.WithNodeSettings(nodeSettings);

            using (var cluster = Cluster.Create(settings))
            using (var node = await cluster.GetWritableNodeAsync())
            using (var connection = await node.GetConnectionAsync())
            {
                Console.WriteLine("ConnectionId: {0}.", connection.ConnectionId);

                Console.WriteLine("Dropping test collection");
                var reply = await new DropCollectionOperation("test", "test").ExecuteAsync(connection);

                Console.WriteLine("Reading timeofday collection");
                var timeOfDayCollection = new Collection("admin", "timeofday");
                var cursor = await timeOfDayCollection.Find().ExecuteAsync(connection);
                while (await cursor.MoveNextAsync())
                {
                    Console.WriteLine(cursor.Current);
                }

                Console.WriteLine("Inserting documents in test collection");
                var testCollection = new Collection("test", "test");
                var documents = new[]
                {
                    new BsonDocument("x", 1),
                    new BsonDocument("x", 2)
                };
                reply = await testCollection.InsertUsingOpcode(documents)
                    .WithContinueOnError(true)
                    .WithMaxBatchCount(100)
                    .WithMaxBatchLength(10000)
                    .WithMaxDocumentSize(500000)
                    .ExecuteAsync(connection);

                Console.WriteLine("Reading test collection");
                cursor = await testCollection.Find().ExecuteAsync(connection);
                while (await cursor.MoveNextAsync())
                {
                    Console.WriteLine(cursor.Current);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(60));
            Console.WriteLine("Leaving async method");
        }
    }
}
