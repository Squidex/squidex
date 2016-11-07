// ReSharper disable InconsistentNaming
// ReSharper disable PossiblyUnassignedProperty

     var webpackMerge = require('webpack-merge'),
    ExtractTextPlugin = require('extract-text-webpack-plugin'),
         commonConfig = require('./webpack.common.js'),
              helpers = require('./helpers');

module.exports = webpackMerge(commonConfig, {
    /**
     * Developer tool to enhance debugging
     *
     * See: http://webpack.github.io/docs/configuration.html#devtool
     * See: https://github.com/webpack/docs/wiki/build-performance#sourcemaps
     */
    devtool: 'cheap-module-eval-source-map',

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
        loaders: [
            {
                test: /\.scss$/,
                include: helpers.root('app', 'theme'),
                loaders: ['style', 'css', 'sass?sourceMap']
            }
        ]
    },

    plugins: [
        new ExtractTextPlugin('[name].css')
    ],

    sassLoader: {
        includePaths: [helpers.root('app', 'theme')]
    },

    tslint: {
        /**
         * Run tslint in production build, but do not fail if there is a warning.
         * 
         * See: https://github.com/wbuchwalter/tslint-loader
         */
        failOnHint: false,
        /**
         * Share the configuration file with the IDE
         */
        configuration: require('./../tslint.json')
    },

    devServer: {
        historyApiFallback: true, stats: 'minimal'
    }
});