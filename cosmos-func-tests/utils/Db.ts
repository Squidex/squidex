const { MongoMemoryServer } = require('mongodb-memory-server');
const mydbcon = new MongoMemoryServer({
  instance: {
    storageEngine: 'wiredTiger',
    dbPath: '../cosmos-func-tests/database',
    port: 27017
  }
});
// tslint:disable-next-line: no-console
console.log('connected');

export { mydbcon };

