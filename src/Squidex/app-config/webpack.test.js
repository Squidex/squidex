 var webpack = require('webpack'),
webpackMerge = require('webpack-merge'),
commonConfig = require('./webpack.config.js'),
     helpers = require('./helpers');

module.exports = webpackMerge(commonConfig, { 
    mode: 'development',

    /**
     * Source map for Karma from the help of karma-sourcemap-loader & karma-webpack
     *
     * Do not change, leave as is or it wont work.
     * See: https://webpack.js.org/configuration/devtool/
     */
    devtool: 'inline-source-map',
});