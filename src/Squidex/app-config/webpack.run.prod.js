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
         * See: http://webpack.github.io/docs/configuration.html#output-path
         */
        path: helpers.root('wwwroot/build/'),

        publicPath: '/build/',

        /**
         * Specifies the name of each output file on disk.
         * IMPORTANT: You must not specify an absolute path here!
         *
         * See: http://webpack.github.io/docs/configuration.html#output-filename
         */
        filename: '[name].[hash].js',

        /**
         * The filename of non-entry chunks as relative path
         * inside the output.path directory.
         *
         * See: http://webpack.github.io/docs/configuration.html#output-chunkfilename
         */
        chunkFilename: '[id].[hash].chunk.js'
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
                use: ExtractTextPlugin.extract({ 
                    fallbackLoader: 'style-loader', loader: 'css-loader!sass-loader?sourceMap' 
                }),
                include: helpers.root('app', 'theme'),
            }, {
                test: /\.scss$/,
                use: [{
                    loader: 'raw-loader'
                }, {
                    loader: helpers.root('app-config', 'clean-css-loader')
                }, {
                    loader: 'sass-loader'
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
        new ExtractTextPlugin('[name].[hash].css'),

        new webpack.optimize.UglifyJsPlugin({
            beautify: false,
            mangle: {
                screw_ie8: true,
                keep_fnames: true
            },
            compress: {
                warnings: false,
                screw_ie8: true
            },
            comments: false
        }),

        new AotPlugin({
            tsConfigPath: './tsconfig.json',
            entryModule: 'app/app.module#AppModule'
        }),
    ]
});