// ReSharper disable InconsistentNaming
// ReSharper disable PossiblyUnassignedProperty
// ReSharper disable InconsistentNaming

var path = require('path');

var appRoot = path.resolve(__dirname, '..');

exports.root = function () {
    var newArgs = Array.prototype.slice.call(arguments, 0);

    return path.join.apply(path, [appRoot].concat(newArgs));
};