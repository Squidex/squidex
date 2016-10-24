'use strict';

   var path = require('path'),
         fs = require('fs'),
loaderUtils = require('loader-utils'),
  SourceMap = require('source-map');

function capitalize(str) {
    return str.charAt(0).toUpperCase() + str.slice(1);
};

function applyPlaceholders(str, dirname, filename) {
    if (!str.length) {
        return str;
    }

    return str
        .split('[file]').join(filename)
        .split('[File]').join(capitalize(filename));
};

function loadBaggage(source, sourcemap) {
    var query = loaderUtils.parseQuery(this.query);

    var srcFilepath = this.resourcePath;
    var srcFilename = path.basename(srcFilepath, path.extname(srcFilepath));
    var srcDirpath = path.dirname(srcFilepath);
    var srcDirname = srcDirpath.split(path.sep).pop();

    this.cacheable();

    if (Object.keys(query).length) {
        var inject = '\n/* injects from baggage-loader */\n';

        Object.keys(query).forEach(function (baggageFile) {
            var baggageVar = query[baggageFile];

            if (typeof baggageVar === 'string' || baggageVar === true) {
                baggageFile = applyPlaceholders(baggageFile, srcDirname, srcFilename);

                try {
                    var stats = fs.statSync(path.resolve(srcDirpath, baggageFile));

                    if (stats.isFile()) {
                        if (baggageVar.length) {
                            inject += 'const ' + baggageVar + ' = ';
                        }

                        if (baggageVar === 'styles') {
                            inject += '[require(\'./' + baggageFile + '\')];\n';
                        } else {
                            inject += 'require(\'./' + baggageFile + '\');\n';
                        }
                    }
                } catch (e) { }
            }
        });

        inject += '\n';

        return inject + source;
    }

    return source;
};

module.exports = loadBaggage;