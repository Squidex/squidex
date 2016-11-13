// ReSharper disable InconsistentNaming
// ReSharper disable PossiblyUnassignedProperty
// ReSharper disable InconsistentNaming

var path = require('path');

var appRoot = path.resolve(__dirname, '..');

exports.root = function () {
    var newArgs = Array.prototype.slice.call(arguments, 0);

    return path.join.apply(path, [appRoot].concat(newArgs));
};

exports.removeLoaders = function (config, extensions) {
    var loaders = config.module.loaders;

    for (var i = 0; i < loaders.length; i += 1) {
        var loader = loaders[i];

        for (var j = 0; j < extensions.length; j += 1) {
            if (loader.test.source.indexOf(extensions[j]) >= 0) {
                loaders.splice(i, 1);
                i--;
                break;
            }
        }
    }
}