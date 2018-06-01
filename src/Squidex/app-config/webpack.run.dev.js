    var webpackMerge = require('webpack-merge'),
MiniCssExtractPlugin = require('mini-css-extract-plugin'),
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
                    loader: 'sass-loader?sourceMap', options: { includePaths: [helpers.root('app', 'theme')] }
                }],
                include: helpers.root('app', 'theme')
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
    ],

    devServer: {
        historyApiFallback: true, stats: 'minimal',
        headers: {
            'Access-Control-Allow-Origin': '*'
        }
    }
});