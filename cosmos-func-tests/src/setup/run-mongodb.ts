import { MongoMemoryServer } from 'mongodb-memory-server';

let mongoDbServer: MongoMemoryServer;

export function startMongoDb() {
    console.log('[MongoDb] Starting in-memory server');

    mongoDbServer = new MongoMemoryServer({
        instance: {
            storageEngine: 'wiredTiger', port: 27017
        }
    });

    console.log('[MongoDb] Started in-memory server');
}

export function stopMongoDB() {
    if (mongoDbServer) {
        mongoDbServer.stop();

        console.log('[MongoDb] Stopped');
    }
}