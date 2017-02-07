// ReSharper disable InconsistentNaming
// ReSharper disable PossiblyUnassignedProperty

     var webpackMerge = require('webpack-merge'),
    ExtractTextPlugin = require('extract-text-webpack-plugin'),
            runConfig = require('./webpack.run.base.js'),
              helpers = require('./helpers');

module.exports = webpackMerge(runConfig, {
    /**
     * Developer tool to enhance debugging
     *
     * See: http://webpack.github.io/docs/configuration.html#devtool
     * See: https://github.com/webpack/docs/wiki/build-performance#sourcemaps
     */
    devtool: 'cheap-module-source-map',

    //debug: true,

    output: {
        filename: '[name].js',
        // Set the public path, because we are running the website from another port (5000)
        publicPath: 'http://localhost:3000/'
    },
    
    /*
     * Options affecting the normal modules.
     *
     * See: http://webpack.github.io/docs/configuration.html#module
     */
    module: {
        /**
         * An array of automatically applied loaders.
         *
         * IMPORTANT: The loaders here are resolved relative to the resource which they are applied to.
         * This means they are not resolved relative to the configuration file.
         *
         * See: http://webpack.github.io/docs/configuration.html#module-loaders
         */
        rules: [
            {
                test: /\.scss$/,
                use: [{
                    loader: 'style-loader'
                }, {
                    loader: 'css-loader'
                }, {
                    loader: 'sass-loader?sourceMap',
                }],
                include: helpers.root('app', 'theme')
            }
        ]
    },

    plugins: [
        new ExtractTextPlugin('[name].css')
    ],

    devServer: {
        historyApiFallback: true, stats: 'minimal'
    }
});