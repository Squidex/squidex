
var webpackMerge = require('webpack-merge'),
            path = require('path'),
         helpers = require('./helpers'),
      testConfig = require('./webpack.test.js');

removeTsLoader();

module.exports = webpackMerge(testConfig, {
    module: {
        loaders: [
            {
                test: /\.ts$/,
                include: [/\.(e2e|spec)\.ts$/],
                loaders: ['awesome-typescript']
            },
            {
                test: /\.ts$/,
                exclude: [/\.(e2e|spec)\.ts$/],
                loaders: ['istanbul-instrumenter-loader', helpers.root('app-config', 'fix-coverage-loader'), 'awesome-typescript', helpers.root('app-config', 'auto-loader') + '?[file].html=template&[file].scss=styles']
            }
        ]
    }
});

function removeTsLoader() {
    var tsModuleIndex = -1;

    for (var i = 0, len = testConfig.module.loaders.length; i < len; i += 1) {
        if (testConfig.module.loaders[i].test.source.indexOf('.ts') > 0) {
            tsModuleIndex = i;
            break;
        }
    }

    if (tsModuleIndex >= 0) {
        testConfig.module.loaders.splice(tsModuleIndex, 1);
    }
}