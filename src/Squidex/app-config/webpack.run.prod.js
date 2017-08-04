      var webpack = require('webpack'),
     webpackMerge = require('webpack-merge'),
ExtractTextPlugin = require('extract-text-webpack-plugin'),
        AotPlugin = require('@ngtools/webpack').AotPlugin,
        runConfig = require('./webpack.run.base.js'),
          helpers = require('./helpers');

var ENV = process.env.NODE_ENV = process.env.ENV = 'production';

helpers.removeLoaders(runConfig, ['scss', 'ts']);

module.exports = webpackMerge(runConfig, {
    devtool: 'source-map',

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
                use: ExtractTextPlugin.extract({ fallback: 'style-loader', use: 'css-loader?minimize!sass-loader?sourceMap' }),
                /*
                 * Do not include component styles
                 */
                include: helpers.root('app', 'theme'),
            }, {
                test: /\.scss$/,
                use: [{
                    loader: 'raw-loader'
                }, {
                    loader: 'sass-loader',
                    options: {
                        includePaths: [helpers.root('app', 'theme')]
                    }
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
        new webpack.NoEmitOnErrorsPlugin(),
        new webpack.DefinePlugin({ 'process.env': { 'ENV': JSON.stringify(ENV) } }),

        /*
         * Puts each bundle into a file and appends the hash of the file to the path.
         * 
         * See: https://github.com/webpack/extract-text-webpack-plugin
         */
        new ExtractTextPlugin('[name].css'),

        new webpack.optimize.UglifyJsPlugin({
            beautify: false,
            mangle: {
                screw_ie8: true, keep_fnames: true
            },
            compress: {
                screw_ie8: true, warnings: false
            },
            comments: false
        }),

        new AotPlugin({
            tsConfigPath: './tsconfig.json',
            entryModule: 'app/app.module#AppModule'
        }),
    ]
});