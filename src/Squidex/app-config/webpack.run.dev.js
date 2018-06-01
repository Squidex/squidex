     var webpackMerge = require('webpack-merge'),
    ExtractTextPlugin = require('extract-text-webpack-plugin'),
            runConfig = require('./webpack.run.base.js'),
              helpers = require('./helpers');

module.exports = webpackMerge(runConfig, {
    mode: 'development',

    output: {
        filename: '[name].js',
        // Set the public path, because we are running the website from another port (5000)
        publicPath: 'http://localhost:3000/'
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
                use: [{
                    loader: 'style-loader'
                }, {
                    loader: 'css-loader'
                }, {
                    loader: 'sass-loader?sourceMap',
                    options: {
                        includePaths: [helpers.root('app', 'theme')]
                    }
                }],
                include: helpers.root('app', 'theme')
            }
        ]
    },

    plugins: [
        new ExtractTextPlugin('[name].css')
    ],

    devServer: {
        historyApiFallback: true, stats: 'minimal',
        headers: {
            'Access-Control-Allow-Origin': '*'
        }
    }
});