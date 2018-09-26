const webpack = require('webpack'),
 webpackMerge = require('webpack-merge'),
         path = require('path'),
      helpers = require('./helpers'),
    runConfig = require('./webpack.run.base.js');

const plugins = {
    // https://github.com/mishoo/UglifyJS2/tree/harmony
    UglifyJsPlugin: require('uglifyjs-webpack-plugin'),
    // https://www.npmjs.com/package/@ngtools/webpack
    NgToolsWebpack: require('@ngtools/webpack'),
    // https://github.com/webpack-contrib/mini-css-extract-plugin
    MiniCssExtractPlugin: require('mini-css-extract-plugin')
};
            
helpers.removeLoaders(runConfig, ['scss', 'ts']);

module.exports = webpackMerge(runConfig, {
    mode: 'production',

    output: {
        /**
         * The output directory as absolute path (required).
         *
         * See: https://webpack.js.org/configuration/output/#output-path
         */
        path: helpers.root('wwwroot/build/'),

        publicPath: '/build/',

        /**
         * Specifies the name of each output file on disk.
         * IMPORTANT: You must not specify an absolute path here!
         *
         * See: https://webpack.js.org/configuration/output/#output-filename
         */
        filename: '[name].js',

        /**
         * The filename of non-entry chunks as relative path
         * inside the output.path directory.
         *
         * See: https://webpack.js.org/configuration/output/#output-chunkfilename
         */
        chunkFilename: '[id].[hash].chunk.js'
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
            test: /\.scss$/,
            /*
             * Extract the content from a bundle to a file
             * 
             * See: https://github.com/webpack-contrib/extract-text-webpack-plugin
             */
            use: [
                plugins.MiniCssExtractPlugin.loader,
            {
                loader: 'css-loader', options: { minimize: true },
            }, {
                loader: 'sass-loader'
            }],
            /*
             * Do not include component styles
             */
            include: helpers.root('app', 'theme'),
        }, {
            test: /\.scss$/,
            use: [{
                loader: 'raw-loader'
            }, {
                loader: 'sass-loader', options: { includePaths: [helpers.root('app', 'theme')] }
            }],
            exclude: helpers.root('app', 'theme'),
        }, { 
            test: /(?:\.ngfactory\.js|\.ngstyle\.js|\.ts)$/,
            use: [{
                loader: '@ngtools/webpack'
            }]
        }]
    },

    plugins: [
        new plugins.NgToolsWebpack.AngularCompilerPlugin({
            entryModule: 'app/app.module#AppModule',
            sourceMap: false,
            skipSourceGeneration: false,
            tsConfigPath: './tsconfig.json'
        }),  
    ],

    optimization: {
        minimizer: [
            new plugins.UglifyJsPlugin({
                uglifyOptions: {
                    compress: false,
                    ecma: 6,
                    mangle: true
                }
            })
        ]
    },

    performance: {
        hints: false 
    }
});