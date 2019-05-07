const webpack = require('webpack'),
 webpackMerge = require('webpack-merge'),
         path = require('path'),
      helpers = require('./helpers'),
 commonConfig = require('./webpack.config.js');

const plugins = {
    // https://github.com/jantimon/html-webpack-plugin
    HtmlWebpackPlugin: require('html-webpack-plugin')
};

module.exports = webpackMerge(commonConfig, {
    /**
     * The entry point for the bundle. Our Angular app.
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
            template: 'wwwroot/_theme.html', hash: true, chunksSortMode: 'none', filename: 'theme.html'
        })
    ]
});