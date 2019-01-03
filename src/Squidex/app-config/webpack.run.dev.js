const webpack = require('webpack'),
 webpackMerge = require('webpack-merge'),
         path = require('path'),
      helpers = require('./helpers'),
    runConfig = require('./webpack.run.base.js');

const plugins = {
    // https://github.com/jrparish/tslint-webpack-plugin
    TsLintPlugin: require('tslint-webpack-plugin')
};

module.exports = webpackMerge(runConfig, {
    mode: 'development',
    
    devtool: 'source-map',

    output: {
        filename: '[name].js',

        /**
         * Set the public path, because we are running the website from another port (5000).
         */
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
        rules: [{
            test: /\.scss$/,
            use: [{
                loader: 'style-loader'
            }, {
                loader: 'css-loader'
            }, {
                loader: 'sass-loader?sourceMap', options: { includePaths: [helpers.root('app', 'theme')] }
            }],
            include: helpers.root('app', 'theme')
        }]
    },

    plugins: [        
        new webpack.ContextReplacementPlugin(/\@angular(\\|\/)core(\\|\/)fesm5/, helpers.root('./src'), {}),

        new plugins.TsLintPlugin({
            files: ['./app/**/*.ts'],
            /**
             * Path to a configuration file.
             */
            config: helpers.root('tslint.json')
        })
    ],

    devServer: {
        headers: {
            'Access-Control-Allow-Origin': '*'
        },
        historyApiFallback: true
    }
});