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

    if (!Object.keys(query).length) {
        return source;
    }

    var componentOffset = source.indexOf('@Component');

    if (componentOffset < 0) {
        componentOffset = source.indexOf('@Ng2.Component');
    }

    if (componentOffset < 0) {
        return source;
    }

    Object.keys(query).forEach(function (baggageFile) {
        var baggageVar = query[baggageFile];

        if ((typeof baggageVar === 'string' || baggageVar === true) && baggageFile !== 'noRequire') {
            baggageFile = applyPlaceholders(baggageFile, srcDirname, srcFilename);

            try {
                var stats = fs.statSync(path.resolve(srcDirpath, baggageFile));

                if (stats.isFile()) {                        
                    let replacement = null;

                    if (baggageVar === 'styles') {
                        if (query.noRequire) {
                            replacement = '[\'' + baggageFile + '\']';
                        } else {
                            replacement = '[require(\'./' + baggageFile + '\')]';
                        }
                    } else {
                        if (query.noRequire) {
                            replacement = '\'' + baggageFile + '\'';
                        } else {
                            replacement = 'require(\'./' + baggageFile + '\')';
                        }
                    }

                    var isReplaced = false;

                    source = source.replace(baggageVar, function (match, offset, full) {
                        if (isReplaced || offset <= componentOffset) {
                            return baggageVar;
                        } else {
                            isReplaced = true;

                            return baggageVar + ': ' + replacement;
                        }
                    });
                }
            } catch (e) { }
        }
    });

    return source;
};

module.exports = loadBaggage;