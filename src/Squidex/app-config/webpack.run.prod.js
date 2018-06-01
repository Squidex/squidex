         var webpack = require('webpack'),
        webpackMerge = require('webpack-merge'),
MiniCssExtractPlugin = require('mini-css-extract-plugin'),
      ngToolsWebpack = require('@ngtools/webpack'),
           runConfig = require('./webpack.run.base.js'),
             helpers = require('./helpers');

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
        rules: [
            {
                test: /\.scss$/,
                /*
                 * Extract the content from a bundle to a file
                 * 
                 * See: https://github.com/webpack-contrib/extract-text-webpack-plugin
                 */
                use: [
                    MiniCssExtractPlugin.loader,
                {
                    loader: 'css-loader'
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
                test: /\.ts/, 
                use: [{
                    loader: '@ngtools/webpack'
                }]
            }
        ]
    },

    plugins: [
        /*
         * Puts each bundle into a file and appends the hash of the file to the path.
         * 
         * See: https://github.com/webpack-contrib/mini-css-extract-plugin
         */
        new MiniCssExtractPlugin('[name].css'),

        new ngToolsWebpack.AngularCompilerPlugin({
            tsConfigPath: './tsconfig.json',
            entryModule: 'app/app.module#AppModule'
        }),
    ]
});