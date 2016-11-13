var loaderUtils = require('loader-utils'),
       CleanCSS = require('clean-css');

function cleanCss(css) {
    this.cacheable();

    var loader = this;
    var callback = this.async();

    new CleanCSS().minify(css, function (err, minified) {
        if (err) {
            if (Array.isArray(err) && (err[0] != null)) {
                return callback(err[0]);
            } else {
                return callback(err);
            }
        }
        var warnings;
        if (((warnings = minified.warnings) != null ? warnings.length : void 0) !== 0) {
            warnings.forEach(function (msg) {
                loader.emitWarning(msg.toString());
            });
        }

        return callback(null, minified.styles, minified.sourceMap);
    });
};

module.exports = cleanCss;