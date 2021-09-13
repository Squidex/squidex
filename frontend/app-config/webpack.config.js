/* eslint-disable no-useless-escape */
/* eslint-disable global-require */

const webpack = require('webpack');
const path = require('path');

const appRoot = path.resolve(__dirname, '..');

function root() {
    // eslint-disable-next-line prefer-rest-params
    const newArgs = Array.prototype.slice.call(arguments, 0);

    // eslint-disable-next-line prefer-spread
    return path.join.apply(path, [appRoot].concat(newArgs));
}

const plugins = {
    // https://github.com/webpack-contrib/mini-css-extract-plugin
    MiniCssExtractPlugin: require('mini-css-extract-plugin'),
    // https://github.com/dividab/tsconfig-paths-webpack-plugin
    TsconfigPathsPlugin: require('tsconfig-paths-webpack-plugin'),
    // https://github.com/aackerman/circular-dependency-plugin
    CircularDependencyPlugin: require('circular-dependency-plugin'),
    // https://github.com/jantimon/html-webpack-plugin
    HtmlWebpackPlugin: require('html-webpack-plugin'),
    // https://webpack.js.org/plugins/terser-webpack-plugin/
    TerserPlugin: require('terser-webpack-plugin'),
    // https://www.npmjs.com/package/@ngtools/webpack
    NgToolsWebpack: require('@ngtools/webpack'),
    // https://github.com/NMFR/optimize-css-assets-webpack-plugin
    OptimizeCSSAssetsPlugin: require('optimize-css-assets-webpack-plugin'),
    // https://webpack.js.org/plugins/eslint-webpack-plugin/
    ESLintPlugin: require('eslint-webpack-plugin'),
    // https://github.com/webpack-contrib/stylelint-webpack-plugin
    StylelintPlugin: require('stylelint-webpack-plugin'),
    // https://www.npmjs.com/package/webpack-bundle-analyzer
    BundleAnalyzerPlugin: require('webpack-bundle-analyzer').BundleAnalyzerPlugin,
    // https://www.npmjs.com/package/@angular-devkit/build-optimizer
    BuildOptimizerWebpackPlugin: require('@angular-devkit/build-optimizer').BuildOptimizerWebpackPlugin,
    // https://webpack.js.org/plugins/copy-webpack-plugin/
    CopyPlugin: require('copy-webpack-plugin'),
    // https://www.npmjs.com/package/webpack-filter-warnings-plugin
    FilterWarningsPlugin: require('webpack-filter-warnings-plugin'),
};

module.exports = function calculateConfig(env) {
    const isProduction = env && env.production;
    const isAnalyzing = isProduction && env.analyze;
    const isDevServer = env.WEBPACK_SERVE;
    const isTestCoverage = env && env.coverage;
    const isTests = env && env.target === 'tests';

    const configFile = isTests ? 'tsconfig.spec.json' : 'tsconfig.app.json';

    // eslint-disable-next-line no-console
    console.log(`Use ${configFile}, Production: ${!!isProduction}`);

    const config = {
        mode: isProduction ? 'production' : 'development',

        /*
         * Source map for Karma from the help of karma-sourcemap-loader & karma-webpack.
         *
         * See: https://webpack.js.org/configuration/devtool/
         */
        devtool: isProduction ? false : 'inline-source-map',

        /*
         * Options affecting the resolving of modules.
         *
         * See: https://webpack.js.org/configuration/resolve/
         */
        resolve: {
            /*
             * An array of extensions that should be used to resolve modules.
             *
             * See: https://webpack.js.org/configuration/resolve/#resolve-extensions
             */
            extensions: ['.ts', '.js', '.mjs', '.css', '.scss'],
            modules: [
                root('app'),
                root('app', 'theme'),
                root('node_modules'),
            ],

            plugins: [
                new plugins.TsconfigPathsPlugin({
                    configFile,
                }),
            ],
        },

        /*
         * Options affecting the normal modules.
         *
         * See: https://webpack.js.org/configuration/module/
         */
        module: {
            /*
             * An array of Rules which are matched to requests when modules are created.
             *
             * See: https://webpack.js.org/configuration/module/#module-rules
             */
            rules: [{
                test: /\.mjs$/,
                type: 'javascript/auto',
                include: [/node_modules/],
            }, {
                // Mark files inside `@angular/core` as using SystemJS style dynamic imports.
                test: /[\/\\]@angular[\/\\]core[\/\\].+\.js$/,
                parser: { system: true },
                include: [/node_modules/],
            }, {
                test: /\.js\.flow$/,
                use: [{
                    loader: 'ignore-loader',
                }],
                include: [/node_modules/],
            }, {
                test: /\.map$/,
                use: [{
                    loader: 'ignore-loader',
                }],
                include: [/node_modules/],
            }, {
                test: /\.d\.ts$/,
                use: [{
                    loader: 'ignore-loader',
                }],
                include: [/node_modules/],
            }, {
                test: /\.(woff|woff2|ttf|eot)(\?.*$|$)/,
                use: [{
                    loader: 'file-loader',
                    options: {
                        name: '[name].[hash].[ext]',

                        // Store the assets in custom path because of fonts need relative urls.
                        outputPath: 'assets',

                        // Use custom public path as ./ is not supported by fonts.
                        publicPath: isDevServer ? undefined : 'assets',
                    },
                }],
            }, {
                test: /\.(png|jpe?g|gif|svg|ico)(\?.*$|$)/,
                use: [{
                    loader: 'file-loader',
                    options: {
                        name: '[name].[hash].[ext]',
                        // Store the assets in custom path because of fonts need relative urls.
                        outputPath: 'assets',
                    },
                }],
            }, {
                test: /\.css$/,
                use: [
                    plugins.MiniCssExtractPlugin.loader,
                    {
                        loader: 'css-loader',
                    }, {
                        loader: 'postcss-loader',
                    }],
            }, {
                test: /\.scss$/,
                use: [{
                    loader: 'raw-loader',
                }, {
                    loader: 'postcss-loader',
                }, {
                    loader: 'sass-loader',
                    options: {
                        additionalData: `
                            @import '_vars';
                            @import '_mixins';
                        `,
                        sassOptions: {
                            includePaths: [root('app', 'theme')],
                        },
                    },
                }],
                exclude: root('app', 'theme'),
            }],
        },

        performance: {
            hints: false,
        },

        plugins: [
            new plugins.FilterWarningsPlugin({
                exclude: /System.import/,
            }),

            /*
             * Always replace the context for the System.import in angular/core to prevent warnings.
             */
            new webpack.ContextReplacementPlugin(
                /\@angular(\\|\/)core(\\|\/)/,
                root('./app', '$_lazy_route_resources'),
                {},
            ),

            new plugins.NgToolsWebpack.AngularWebpackPlugin({
                tsconfig: configFile,
                // Load directly from file system and skip webpack.
                directTemplateLoading: true,

                // Only run in aot compiler in production.
                jitMode: !isProduction,
            }),

            /*
             * Puts each bundle into a file and appends the hash of the file to the path.
             *
             * See: https://github.com/webpack-contrib/mini-css-extract-plugin
             */
            new plugins.MiniCssExtractPlugin({
                filename: '[name].css',
            }),

            new webpack.LoaderOptionsPlugin({
                options: {
                    htmlLoader: {
                        /*
                         * Define the root for images, so that we can use absolute urls.
                         *
                         * See: https://github.com/webpack/html-loader#Advanced_Options
                         */
                        root: root('app', 'images'),
                    },
                    context: '/',
                },
            }),

            new plugins.StylelintPlugin({
                files: '**/*.scss',
            }),

            /*
             * Detect circular dependencies in app.
             *
             * See: https://github.com/aackerman/circular-dependency-plugin
             */
            new plugins.CircularDependencyPlugin({
                exclude: /([\\\/]node_modules[\\\/])|(ngfactory\.js$)/,
                // Add errors to webpack instead of warnings
                failOnError: true,
            }),

            /*
             * Copy lazy loaded libraries to output.
             */
            new plugins.CopyPlugin({
                patterns: [
                    { from: './node_modules/simplemde/dist', to: 'dependencies/simplemde' },

                    { from: './node_modules/tinymce/icons/default/icons.min.js', to: 'dependencies/tinymce/icons/default' },
                    { from: './node_modules/tinymce/plugins/advlist', to: 'dependencies/tinymce/plugins/advlist' },
                    { from: './node_modules/tinymce/plugins/code', to: 'dependencies/tinymce/plugins/code' },
                    { from: './node_modules/tinymce/plugins/image', to: 'dependencies/tinymce/plugins/image' },
                    { from: './node_modules/tinymce/plugins/link', to: 'dependencies/tinymce/plugins/link' },
                    { from: './node_modules/tinymce/plugins/lists', to: 'dependencies/tinymce/plugins/lists' },
                    { from: './node_modules/tinymce/plugins/media', to: 'dependencies/tinymce/plugins/media' },
                    { from: './node_modules/tinymce/plugins/paste', to: 'dependencies/tinymce/plugins/paste' },
                    { from: './node_modules/tinymce/skins', to: 'dependencies/tinymce/skins' },
                    { from: './node_modules/tinymce/themes/silver', to: 'dependencies/tinymce/themes/silver' },
                    { from: './node_modules/tinymce/tinymce.min.js', to: 'dependencies/tinymce' },

                    { from: './node_modules/tui-code-snippet/dist', to: 'dependencies/tui-calendar' },
                    { from: './node_modules/tui-calendar/dist', to: 'dependencies/tui-calendar' },

                    { from: './node_modules/ace-builds/src-min/ace.js', to: 'dependencies/ace/ace.js' },
                    { from: './node_modules/ace-builds/src-min/ext-language_tools.js', to: 'dependencies/ace/ext/language_tools.js' },
                    { from: './node_modules/ace-builds/src-min/ext-modelist.js', to: 'dependencies/ace/ext/modelist.js' },
                    { from: './node_modules/ace-builds/src-min/mode-*.js', to: 'dependencies/ace/[name][ext]' },
                    { from: './node_modules/ace-builds/src-min/snippets', to: 'dependencies/ace/snippets' },
                    { from: './node_modules/ace-builds/src-min/worker-*.js', to: 'dependencies/ace/[name][ext]' },

                    { from: './node_modules/leaflet-control-geocoder/dist/Control.Geocoder.css', to: 'dependencies/leaflet' },
                    { from: './node_modules/leaflet-control-geocoder/dist/Control.Geocoder.min.js', to: 'dependencies/leaflet' },
                    { from: './node_modules/leaflet/dist/leaflet.js', to: 'dependencies/leaflet' },
                    { from: './node_modules/leaflet/dist/leaflet.css', to: 'dependencies/leaflet' },
                    { from: './node_modules/leaflet/dist/images', to: 'dependencies/leaflet/images' },

                    { from: './node_modules/video.js/dist/video.min.js', to: 'dependencies/videojs' },
                    { from: './node_modules/video.js/dist/video-js.min.css', to: 'dependencies/videojs' },

                    { from: './node_modules/font-awesome/css/font-awesome.min.css', to: 'dependencies/font-awesome/css' },
                    { from: './node_modules/font-awesome/fonts', to: 'dependencies/font-awesome/fonts' },

                    { from: './node_modules/vis-network/standalone/umd/vis-network.min.js', to: 'dependencies' },
                ],
            }),
        ],

        devServer: {
            headers: {
                'Access-Control-Allow-Origin': '*',
            },
            historyApiFallback: true,
        },
    };

    if (!isTests) {
        /*
         * The entry point for the bundle. Our Angular app.
         *
         * See: https://webpack.js.org/configuration/entry-context/
         */
        config.entry = {
            shims: './app/shims.ts',
            style: './app/style.js',
              app: './app/app.ts',
        };

        if (isProduction) {
            config.output = {
                /*
                 * The output directory as absolute path (required).
                 *
                 * See: https://webpack.js.org/configuration/output/#output-path
                 */
                path: root('/build/'),

                publicPath: './build/',

                /*
                 * Specifies the name of each output file on disk.
                 *
                 * See: https://webpack.js.org/configuration/output/#output-filename
                 */
                filename: '[name].js',

                /*
                 * The filename of non-entry chunks as relative path inside the output.path directory.
                 *
                 * See: https://webpack.js.org/configuration/output/#output-chunkfilename
                 */
                chunkFilename: '[id].[fullhash].chunk.js',
            };
        } else {
            config.output = {
                filename: '[name].js',

                /*
                 * Set the public path, because we are running the website from another port (5000).
                 */
                publicPath: 'https://localhost:3000/',
            };
        }

        config.plugins.push(
            new plugins.HtmlWebpackPlugin({
                filename: 'index.html',
                hash: true,
                chunks: ['shims', 'app'],
                chunksSortMode: 'manual',
                template: root('app', 'index.html'),
            }),
        );

        config.plugins.push(
            new plugins.HtmlWebpackPlugin({
                filename: 'theme.html',
                hash: true,
                chunks: ['style'],
                chunksSortMode: 'none',
                template: root('app', '_theme.html'),
            }),
        );

        if (isProduction) {
            config.plugins.push(
                new plugins.ESLintPlugin({
                    files: [
                        './app/**/*.ts',
                    ],
                }),
            );
        }
    }

    if (isProduction) {
        config.optimization = {
            minimizer: [
                new plugins.TerserPlugin({
                    terserOptions: {
                        compress: true,
                        ecma: 5,
                        mangle: true,
                        output: {
                            comments: false,
                        },
                        safari10: true,
                    },
                    extractComments: true,
                }),

                new plugins.OptimizeCSSAssetsPlugin({}),
            ],
        };

        config.plugins.push(new plugins.BuildOptimizerWebpackPlugin());

        config.module.rules.push({
            test: /\.js$/,
            use: [{
                loader: '@angular-devkit/build-optimizer/webpack-loader',
                options: {
                    sourceMap: false,
                },
            }],
        });
    }

    if (isTestCoverage) {
        // Do not instrument tests.
        config.module.rules.push({
            test: /\.[jt]sx?$/,
            use: [{
                loader: '@ngtools/webpack',
            }],
            include: [/\.(e2e|spec)\.ts$/],
        });

        // Use instrument loader for all normal files.
        config.module.rules.push({
            test: /\.[jt]sx?$/,
            use: [{
                loader: 'istanbul-instrumenter-loader',
                options: {
                    esModules: true,
                },
            }, {
                loader: '@ngtools/webpack',
            }],
            exclude: [/\.(e2e|spec)\.ts$/],
        });
    } else {
        config.module.rules.push({
            test: /\.[jt]sx?$/,
            use: [{
                loader: '@ngtools/webpack',
            }],
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
                    loader: 'css-loader',
                }, {
                    loader: 'postcss-loader',
                }, {
                    loader: 'sass-loader',
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
                loader: 'style-loader',
            }, {
                loader: 'css-loader',
            }, {
                loader: 'postcss-loader',
            }, {
                loader: 'sass-loader',
                options: {
                    sourceMap: true,
                },
            }],
            // Do not include component styles.
            include: root('app', 'theme'),
        });
    }

    if (isAnalyzing) {
        config.plugins.push(new plugins.BundleAnalyzerPlugin());
    }

    return config;
};
