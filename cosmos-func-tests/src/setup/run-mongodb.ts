import { MongoMemoryServer } from 'mongodb-memory-server';

let mongoDbServer: MongoMemoryServer;

export function startMongoDb() {
    console.log('[MONGODB] Starting in-memory server');

    mongoDbServer = new MongoMemoryServer({
        instance: {
            storageEngine: 'wiredTiger', port: 27017
        }
    });

    console.log('[MONGODB] Started in-memory server');
}

export function stopMongoDB() {
    if (mongoDbServer) {
        mongoDbServer.stop();

        console.log('[MONGODB] Stopped');
    }
}