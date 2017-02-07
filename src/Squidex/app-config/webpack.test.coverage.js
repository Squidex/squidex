
var webpackMerge = require('webpack-merge'),
            path = require('path'),
         helpers = require('./helpers'),
      testConfig = require('./webpack.test.js');

     // console.log(JSON.stringify(testConfig, null, '\t'));

helpers.removeLoaders(testConfig, ['ts']);

module.exports = webpackMerge(testConfig, {
    module: {
        rules: [
            {
                test: /\.ts$/,
                use: [{
                    loader: 'awesome-typescript-loader'
                }],
                include: [/\.(e2e|spec)\.ts$/],
                
            }, {
                test: /\.ts$/,
                use: [{
                    loader: 'istanbul-instrumenter-loader'
                }, {
                    loader: helpers.root('app-config', 'fix-coverage-loader')
                }, {
                    loader: 'awesome-typescript-loader'
                }, {
                    loader: 'angular2-router-loader'
                }, {
                    loader: 'angular2-template-loader'
                }],
                exclude: [/\.(e2e|spec)\.ts$/]
            }
        ]
    }
});