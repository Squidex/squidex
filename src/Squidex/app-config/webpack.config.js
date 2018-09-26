const webpack = require('webpack'),
         path = require('path'),
      helpers = require('./helpers');

const plugins = {
    // https://github.com/webpack-contrib/mini-css-extract-plugin
    MiniCssExtractPlugin: require('mini-css-extract-plugin'),
    // https://github.com/dividab/tsconfig-paths-webpack-plugin
    TsconfigPathsPlugin: require('tsconfig-paths-webpack-plugin')
};

module.exports = {
    /**
     * Options affecting the resolving of modules.
     *
     * See: https://webpack.js.org/configuration/resolve/
     */
    resolve: {
        /**
         * An array of extensions that should be used to resolve modules.
         *
         * See: https://webpack.js.org/configuration/resolve/#resolve-extensions
         */
        extensions: ['.js', '.mjs', '.ts', '.css', '.scss'],
        modules: [
            helpers.root('app'),
            helpers.root('app', 'theme'),
            helpers.root('node_modules')
        ],

        plugins: [
            new plugins.TsconfigPathsPlugin()
        ]
    },

    /*
     * Options affecting the normal modules.
     *
     * See: https://webpack.js.org/configuration/module/
     */
    module: {
        /**
         * An array of Rules which are matched to requests when modules are created.
         *
         * See: https://webpack.js.org/configuration/module/#module-rules
         */
        rules: [{
            test: /\.mjs$/,
            type: "javascript/auto",
            include: [/node_modules/],
          },{
            test: /\.ts$/,
            use: [{
                loader: 'awesome-typescript-loader'
            }, {
                loader: 'angular-router-loader'
            }, {
                loader: 'angular2-template-loader'
            }, {
                loader: 'tslint-loader'
            }],
            exclude: [/node_modules/]
        }, {
            test: /\.ts$/,
            use: [{
                loader: 'awesome-typescript-loader'
            }],
            include: [/node_modules/]
        }, {
            test: /\.js\.flow$/,
            use: [{
                loader: 'ignore-loader'
            }],
            include: [/node_modules/]
        }, {
            test: /\.html$/,
            use: [{
                loader: 'raw-loader'
            }]
        }, {
            test: /\.(woff|woff2|ttf|eot)(\?.*$|$)/,
            use: [{
                loader: 'file-loader?name=assets/[name].[hash].[ext]'
            }]
        }, {
            test: /\.(png|jpe?g|gif|svg|ico)(\?.*$|$)/,
            use: [{
                loader: 'file-loader?name=assets/[name].[hash].[ext]'
            }]
        }, {
            test: /\.css$/,
            use: [
                plugins.MiniCssExtractPlugin.loader,
            {
                loader: 'css-loader'
            }]
        }, {
            test: /\.scss$/,
            use: [{
                loader: 'raw-loader'
            }, {
                loader: 'sass-loader', options: { includePaths: [helpers.root('app', 'theme')] }
            }],
            exclude: helpers.root('app', 'theme')
        }]
    },

    plugins: [
        /*
         * Puts each bundle into a file and appends the hash of the file to the path.
         * 
         * See: https://github.com/webpack-contrib/mini-css-extract-plugin
         */
        new plugins.MiniCssExtractPlugin('[name].css'),

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
                     * Define the root for images, so that we can use absolute url's
                     * 
                     * See: https://github.com/webpack/html-loader#Advanced_Options
                     */
                    root: helpers.root('app', 'images')
                },
                context: '/'
            }
        }),
        
        new webpack.ContextReplacementPlugin(/moment[\/\\]locale$/, /en/),

        /**
         * Shim additional libraries
         * 
         * See: https://webpack.js.org/plugins/provide-plugin/
         */
        new webpack.ProvidePlugin({
            // Mouse trap handles shortcut management
            'Mousetrap': 'mousetrap/mousetrap'
        })
    ]
};