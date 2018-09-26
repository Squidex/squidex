const webpack = require('webpack'),
 webpackMerge = require('webpack-merge'),
         path = require('path'),
      helpers = require('./helpers'),
 commonConfig = require('./webpack.config.js');

const plugins = {
    HtmlWebpackPlugin: require('html-webpack-plugin')
};

module.exports = webpackMerge(commonConfig, {
    /**
     * The entry point for the bundle
     * Our Angular.js app
     *
     * See: https://webpack.js.org/configuration/entry-context/
     */
    entry: {
        'shims': './app/shims.ts',
          'app': './app/app.ts'
    },

    plugins: [
        new plugins.HtmlWebpackPlugin({
            hash: true,
            chunks: ['shims', 'app'],
            chunksSortMode: 'manual',
            template: 'wwwroot/index.html'
        }),
        
        new plugins.HtmlWebpackPlugin({
            template: 'wwwroot/theme.html', hash: true, chunksSortMode: 'none', filename: 'theme.html'
        })
    ]
});