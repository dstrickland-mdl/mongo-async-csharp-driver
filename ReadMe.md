---
## Note:

This is an experimental project. The concepts explored in this project may or may not ever appear in the official driver. This project serves 
as a playground where ideas can be explored with total freedom.

---

## Goals

1. Experiment with a 100% async implementation of a C# MongoDB Driver
2. Experiment with a simplified public API

## Design principles

### Async to the core

This driver is 100% async to the core. There are intentionally no synchronous versions of the async methods.

### Simplified public API

The public API is organized around 4 sets of classes:

1. Clusters (of various types)
2. Nodes
3. Connections
4. Operations (of which there are many, and you can write your own)

The basic problem that this simplified public API intends to solve is the ambiguity inherent in the following code:

    var cursor1 = collection.Find(query1);
    var cursor2 = collection.Find(query2);

The ambiguities here are several:

- when is the query actually executed (i.e, sent to the server)?
- which node is the query actually executed on?
- are both queries executed on the same node? using the same connection?

These ambiguities are resolved by applying complex hidden rules which are not easy to understand, and vary slightly from driver to driver.

The simplified public API defined in this experimental driver removes these ambiguities:

    using (var cluster = Cluster.CreateReplicaSet(uri))
    using (var node = await cluster.GetReadableNodeAsync(ReadPreference.Secondary))
    using (var connection = await node.GetConnectionAsync())
    {
        var collection = new Collection("test", "test");
        var query1 = ...;
        var query2 = ...;
        var cursor1 = await collection.Find(query1).ExecuteAsync(connection);
        var cursor2 = await collection.Find(query2).ExecuteAsync(connection);
    }

We know exactly when the query is executed: when ExecuteAsync is called. We know exactly which node the query is executed on and which connection
will be used, because we acquired the node and and connection values ourselves and passed the connection to the ExecuteAsync method.

### Extensive use of immutable objects

Almost all driver objects are immutable. They all follow the same pattern:

- a few constructors with parameters that are either required or commonly used (or some other factory pattern)
- methods named WithXyz which return a new immutable object "with" some values changed

For example:

    var operation = collection.Find(query).WithSkip(10).WithLimit(10);
    var cursor = await operation.ExecuteAsync(connection);

or alternatively, on a single line:

    var cursor = await collection.Find(query).WithSkip(10).WithLimit(10).ExecuteAsync(connection);

Here the Find extension method of the collection is returning an immutable FindOperation object which is then modified twice (returning new
immutable objects each time) to change some of the values.

### Shareable connections to reduce the number of connections required

Connections are implemented in such a way that they are shareable across multiple threads. When multiple threads send messages over the
same connection the messages are queued up and sent sequentially. Incoming messages are matched to message readers using the responseId in the reply.

In principle you could use just one connection, but the current version of the server processes incoming messages one at a time, therefore you will want
to use multiple connections so that the server can work on more than one operation at a time. The best number of connections to use will have
to be determined empirically. If a future version of the server supports processing incoming messages (on the same connection) in parallel
you will be able to use even fewer connections.

Some operations take a long time to complete, which (in the current version of the server) causes all operations queued up behind it (on the same
connection) to be blocked until the long running operation completes. The driver supports the notion of dedicated connections, so if you know that
you are going to be executing a resource intensive operation you have the option of executing it on a dedicated connection to avoid impacting other threads.
