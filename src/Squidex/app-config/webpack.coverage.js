
var webpackMerge = require('webpack-merge'),
            path = require('path'),
         helpers = require('./helpers'),
      testConfig = require('./webpack.test.js');

module.exports = webpackMerge(testConfig, {
    module: {
        loaders: [
            {
                test: /\.(js|ts)$/, 
                include: helpers.root('app'),
                exclude: [/\.(e2e|spec)\.ts$/],
                loader: 'istanbul-instrumenter-loader'
            }, {
                test: /\.(js|ts)$/,
                include: helpers.root('app'),
                exclude: [/\.(e2e|spec)\.ts$/],
                loader: helpers.root('app-config', 'fix-coverage-loader')
            }
        ]
    }
});