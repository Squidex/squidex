 var webpack = require('webpack'),
webpackMerge = require('webpack-merge'),
commonConfig = require('./webpack.config.js'),
     helpers = require('./helpers');

module.exports = webpackMerge(commonConfig, { 
    /**
     * Source map for Karma from the help of karma-sourcemap-loader & karma-webpack
     *
     * Do not change, leave as is or it wont work.
     * See: https://github.com/webpack/karma-webpack#source-maps
     */
    devtool: 'inline-source-map',
});