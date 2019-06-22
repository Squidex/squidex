const webpack = require('webpack'),
         path = require('path');

const appRoot = path.resolve(__dirname, '..');

function root() {
    var newArgs = Array.prototype.slice.call(arguments, 0);

    return path.join.apply(path, [appRoot].concat(newArgs));
};

const plugins = {
    // https://github.com/webpack-contrib/mini-css-extract-plugin
    MiniCssExtractPlugin: require('mini-css-extract-plugin'),
    // https://github.com/dividab/tsconfig-paths-webpack-plugin
    TsconfigPathsPlugin: require('tsconfig-paths-webpack-plugin'),
    // https://github.com/aackerman/circular-dependency-plugin
    CircularDependencyPlugin: require('circular-dependency-plugin'),
    // https://github.com/jantimon/html-webpack-plugin
    HtmlWebpackPlugin: require('html-webpack-plugin'),
    // https://github.com/mishoo/UglifyJS2/tree/harmony
    UglifyJsPlugin: require('uglifyjs-webpack-plugin'),
    // https://www.npmjs.com/package/@ngtools/webpack
    NgToolsWebpack: require('@ngtools/webpack'),
    // https://github.com/NMFR/optimize-css-assets-webpack-plugin
    OptimizeCSSAssetsPlugin: require("optimize-css-assets-webpack-plugin"),
    // https://github.com/jrparish/tslint-webpack-plugin
    TsLintPlugin: require('tslint-webpack-plugin')
};

module.exports = function(env) {
    const isDevServer = path.basename(require.main.filename) === 'webpack-dev-server.js';
    const isProduction = env && env.production;
    const isTesting = env && env.target === 'tests';
    const isCoverage = env && env.coverage;
    const isJit = env && env.jit;

    const config = {
        mode: isProduction ? 'production' : 'development',

        /**
         * Source map for Karma from the help of karma-sourcemap-loader & karma-webpack.
         *
         * See: https://webpack.js.org/configuration/devtool/
         */
        devtool: isProduction ? undefined : (isTesting ? 'inline-source-map' : 'source-map'),

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
                root('app'),
                root('app', 'theme'),
                root('node_modules')
            ],

            plugins: [
                new plugins.TsconfigPathsPlugin()
            ]
        },

        /**
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
                include: [/node_modules/]
            }, {
                test: /[\/\\]@angular[\/\\]core[\/\\].+\.js$/,
                parser: { system: true },
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
                    loader: 'file-loader?name=[name].[hash].[ext]',
                    options: {
                        outputPath: 'assets', 
                        /*
                        * Use custom public path as ./ is not supported by fonts.
                        */
                        publicPath: isDevServer ? undefined : 'assets'
                    }
                }]
            }, {
                test: /\.(png|jpe?g|gif|svg|ico)(\?.*$|$)/,
                use: [{
                    loader: 'file-loader?name=[name].[hash].[ext]',
                    options: {
                        outputPath: 'assets'
                    }
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
                    loader: 'sass-loader', options: { includePaths: [root('app', 'theme')] }
                }],
                exclude: root('app', 'theme')
            }]
        },

        plugins: [
            new webpack.ContextReplacementPlugin(/\@angular(\\|\/)core(\\|\/)fesm5/, root('./app'), {}),
            new webpack.ContextReplacementPlugin(/moment[\/\\]locale$/, /en/),

            /**
             * Puts each bundle into a file and appends the hash of the file to the path.
             * 
             * See: https://github.com/webpack-contrib/mini-css-extract-plugin
             */
            new plugins.MiniCssExtractPlugin('[name].css'),

            new webpack.LoaderOptionsPlugin({
                options: {
                    htmlLoader: {
                        /**
                         * Define the root for images, so that we can use absolute urls.
                         * 
                         * See: https://github.com/webpack/html-loader#Advanced_Options
                         */
                        root: root('app', 'images')
                    },
                    context: '/'
                }
            }),

            /**
             * Detect circular dependencies in app.
             * 
             * See: https://github.com/aackerman/circular-dependency-plugin
             */
            new plugins.CircularDependencyPlugin({
                exclude: /([\\\/]node_modules[\\\/])|(ngfactory\.js$)/,
                // Add errors to webpack instead of warnings
                failOnError: true
            }),
        ],
        
        devServer: {
            headers: {
                'Access-Control-Allow-Origin': '*'
            },
            historyApiFallback: true
        }
    };

    if (!isTesting) {
        /**
         * The entry point for the bundle. Our Angular app.
         *
         * See: https://webpack.js.org/configuration/entry-context/
         */
        config.entry = {
            'shims': './app/shims.ts',
              'app': './app/app.ts'
        };

        if (isProduction) {
            config.output = {
                /**
                 * The output directory as absolute path (required).
                 *
                 * See: https://webpack.js.org/configuration/output/#output-path
                 */
                path: root('wwwroot/build/'),
        
                publicPath: './build/',
        
                /**
                 * Specifies the name of each output file on disk.
                 *
                 * See: https://webpack.js.org/configuration/output/#output-filename
                 */
                filename: '[name].js',
        
                /**
                 * The filename of non-entry chunks as relative path inside the output.path directory.
                 *
                 * See: https://webpack.js.org/configuration/output/#output-chunkfilename
                 */
                chunkFilename: '[id].[hash].chunk.js'
            };
        } else {
            config.output = {
                filename: '[name].js',
        
                /**
                 * Set the public path, because we are running the website from another port (5000).
                 */
                publicPath: 'http://localhost:3000/'
            };
        }

        config.plugins.push(
            new plugins.HtmlWebpackPlugin({
                hash: true,
                chunks: ['shims', 'app'],
                chunksSortMode: 'manual',
                template: 'wwwroot/index.html'
            })
        );

        config.plugins.push(
            new plugins.HtmlWebpackPlugin({
                template: 'wwwroot/_theme.html', hash: true, chunksSortMode: 'none', filename: 'theme.html'
            })
        );

        config.plugins.push(
            new plugins.TsLintPlugin({
                files: ['./app/**/*.ts'],
                /**
                 * Path to a configuration file.
                 */
                config: root('tslint.json'),
                /**
                 * Wait for linting and fail the build when linting error occur.
                 */
                waitForLinting: isProduction
            })
        );
    }

    if (isProduction) {
        config.optimization = {
            minimizer: [
                new plugins.UglifyJsPlugin({
                    uglifyOptions: {
                        compress: false,
                        ecma: 6,
                        mangle: true,
                        output: {
                            comments: false
                        }
                    },
                    extractComments: true
                }),

                new plugins.OptimizeCSSAssetsPlugin({})
            ]
        };

        config.performance = {
            hints: false 
        };
    }

    if (!isCoverage) {
        config.module.rules.push({
            test: /\.ts$/,
            use: [{
                loader: 'awesome-typescript-loader'
            }],
            exclude: [/node_modules/]
        })
    } else {
        config.module.rules.push({
            test: /\.ts$/,
            use: [{
                loader: 'ts-loader'
            }],
            include: [/\.(e2e|spec)\.ts$/],
        });

        // Use instrument loader for all normal builds.
        config.module.rules.push({
            test: /\.ts$/,
            use: [{
                loader: 'istanbul-instrumenter-loader'
            }, {
                loader: 'ts-loader'
            }],
            exclude: [/\.(e2e|spec)\.ts$/]
        });
    }

    if (isProduction) {
        config.module.rules.push({
            test: /\.scss$/,
            /*
             * Extract the content from a bundle to a file.
             * 
             * See: https://github.com/webpack-contrib/extract-text-webpack-plugin
             */
            use: [
                plugins.MiniCssExtractPlugin.loader,
            {
                loader: 'css-loader'
            }, {
                loader: 'sass-loader'
            }],
            /*
             * Do not include component styles.
             */
            include: root('app', 'theme'),
        });
    } else {
        config.module.rules.push({
            test: /\.scss$/,
            use: [{
                loader: 'style-loader'
            }, {
                loader: 'css-loader'
            }, {
                loader: 'sass-loader?sourceMap'
            }],
            /*
             * Do not include component styles.
             */
            include: root('app', 'theme')
        });
    }

    if (!isJit) {
        config.module.rules.push({ 
            test: /(?:\.ngfactory\.js|\.ngstyle\.js|\.ts)$/,
            use: [{
                loader: '@ngtools/webpack'
            }]
        });

        config.plugins.push(
            new plugins.NgToolsWebpack.AngularCompilerPlugin({
                entryModule: 'app/app.module#AppModule',
                sourceMap: !isProduction,
                skipSourceGeneration: false,
                tsConfigPath: './tsconfig.json'
            })
        );
    }

    return config;
};