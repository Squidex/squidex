var path = require('path');

var appRoot = path.resolve(__dirname, '..');

exports.root = function () {
    var newArgs = Array.prototype.slice.call(arguments, 0);

    return path.join.apply(path, [appRoot].concat(newArgs));
};

exports.removeLoaders = function (config, extensions) {
    var rules = config.module.rules;

    for (var i = 0; i < rules.length; i += 1) {
        var rule = rules[i];

        for (var j = 0; j < extensions.length; j += 1) {
            if (rule.test.source.indexOf(extensions[j]) >= 0) {
                rules.splice(i, 1);
                i--;
                break;
            }
        }
    }
}