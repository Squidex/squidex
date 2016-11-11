 var webpack = require('webpack'),
webpackMerge = require('webpack-merge'),
commonConfig = require('./webpack.config.js'),
     helpers = require('./helpers');

module.exports = webpackMerge(commonConfig, { });