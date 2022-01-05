const path = require('path');

const appRoot = path.resolve(__dirname, '..');

function root() {
    var newArgs = Array.prototype.slice.call(arguments, 0);

    return path.join.apply(path, [appRoot].concat(newArgs));
}

function isDevServer() {
    return path.basename(require.main.filename) === 'webpack-dev-server.js';
}

module.exports = {
    root,
    isDevServer,
};