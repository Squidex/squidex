var webpack = require('webpack'),
    helpers = require('./helpers');

module.exports = {
    /**
     * Source map for Karma from the help of karma-sourcemap-loader &  karma-webpack
     *
     * Do not change, leave as is or it wont work.
     * See: https://github.com/webpack/karma-webpack#source-maps
     */
    devtool: 'inline-source-map',

    resolve: {
        /**
         * An array of extensions that should be used to resolve modules.
         *
         * See: http://webpack.github.io/docs/configuration.html#resolve-extensions
         */
        extensions: ['', '.ts', '.js'],
        root: [
            helpers.root('app'),
            helpers.root('app-libs')
        ]
    },

    /*
     * Options affecting the normal modules.
     *
     * See: http://webpack.github.io/docs/configuration.html#module
     */
    module: {
        preLoaders: [{
            test: /\.ts/,
            loader: helpers.root('app-config', 'auto-loader') + '?[file].html=template&[file].scss=styles',
        }],

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
                test: /\.ts$/,
                loader: 'awesome-typescript-loader'
            }, {
                test: /\.html$/,
                loader: 'html'
            }, {
                test: /\.(png|jpe?g|gif|svg|woff|woff2|ttf|eot|ico)(\?.*$|$)/,
                loader: 'null'
            }, {
                test: /\.css$/,
                loader: 'null'
            }, {
                test: /\.scss$/,
                exclude: helpers.root('app', 'theme'),
                loaders: ['raw', 'sass']
            }
        ]
    },
    
    sassLoader: {
        includePaths: [helpers.root('app', 'theme')]
    },

    plugins: [
        /**
         * Shim additional libraries
         * 
         * See: https://webpack.github.io/docs/shimming-modules.html
         */
        new webpack.ProvidePlugin({
            // Mouse trap handles shortcut management
            'Mousetrap': 'mousetrap/mousetrap'
        })
    ]
}