// ReSharper disable InconsistentNaming
// ReSharper disable PossiblyUnassignedProperty

        var webpack = require('webpack'),
               path = require('path'),
  HtmlWebpackPlugin = require('html-webpack-plugin'),
  ExtractTextPlugin = require('extract-text-webpack-plugin'),
            helpers = require('./helpers');

module.exports = {
    /**
     * Options affecting the resolving of modules.
     *
     * See: http://webpack.github.io/docs/configuration.html#resolve
     */
    resolve: {
        /**
         * An array of extensions that should be used to resolve modules.
         *
         * See: http://webpack.github.io/docs/configuration.html#resolve-extensions
         */
        extensions: ['.js', '.ts', '.css', '.scss'],
        modules: [
            helpers.root('app'),
            helpers.root('app', 'theme'),
            helpers.root('app-libs'),
            helpers.root('node_modules')
        ],
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
                test: /\.ts$/,
                loaders: ['awesome-typescript', helpers.root('app-config', 'auto-loader') + '?[file].html=template&[file].scss=styles', 'tslint']
            }, {
                test: /\.html$/,
                loader: 'html'
            }, {
                test: /\.(woff|woff2|ttf|eot)(\?.*$|$)/,
                loader: 'file?name=assets/[name].[hash].[ext]'
            }, {
                test: /\.(png|jpe?g|gif|svg|ico)(\?.*$|$)/,
                loader: 'file?name=assets/[name].[hash].[ext]'
            }, {
                test: /\.css$/,
                loader: ExtractTextPlugin.extract({ fallbackLoader: 'style', loader: 'css?sourceMap' })
            }, {
                test: /\.scss$/,
                exclude: helpers.root('app', 'theme'),
                loaders: ['raw', 'sass']
            }
        ]
    },

    plugins: [
        new webpack.LoaderOptionsPlugin({
            options: {
                tslint: {
                    /**
                    * Run tslint in production build and fail if there is one warning.
                    * 
                    * See: https://github.com/wbuchwalter/tslint-loader
                    */
                    emitErrors: true,
                    /**
                    * Share the configuration file with the IDE
                    */
                    configuration: require('./../tslint.json')
                },
                htmlLoader: {
                    /**
                     * Define the root for images, so that we can use absolute urls
                     * 
                     * See: https://github.com/webpack/html-loader#Advanced_Options
                     */
                    root: helpers.root('app', 'images')
                },
                sassLoader: {
                    includePaths: [helpers.root('app', 'theme')]
                },
                context: '/'
            }
        }),

        new webpack.ContextReplacementPlugin(/moment[\/\\]locale$/, /en/),

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
};