const webpackConfig = require('./webpack.config');

module.exports = function calculateConfig(config) {
    const _config = {
        /*
         * Base path that will be used to resolve all patterns (e.g. files, exclude).
         */
        basePath: '',

        frameworks: ['jasmine', 'webpack'],

        /*
         * Load additional test shim to setup angular for testing.
         */
        files: [
            { pattern: './app-config/karma-test-shim.js', watched: false },
        ],

        preprocessors: {
            './app-config/karma-test-shim.js': ['webpack', 'sourcemap'],
        },

        /*
         * Load the files with webpack and use test configuration for it.
         */
        webpack: webpackConfig({ target: 'tests', coverage: true, jit: true }),

        webpackMiddleware: {
            stats: 'errors-only',
        },

        /*
         * Use a mocha style console reporter, html reporter and the code coverage reporter.
         */
        reporters: ['kjhtml', 'mocha', 'coverage-istanbul'],

        htmlReporter: {
            useCompactStyle: true,
            /*
             * Use the same folder like the html report for coverage reports.
             */
            outputFile: '_test-output/tests.html',

            /*
             * Group the output by test suite (describe), equivalent to mocha reporter.
             */
            groupSuites: true,
        },

        coverageIstanbulReporter: {
            // eslint-disable-next-line global-require
            dir: require('path').join(__dirname, '../_test-output/coverage'),

            reports: [
              'html',
              'lcovonly',
            ],

            fixWebpackSourcePaths: true,
          },

        /*
         * Disable continuous Integration mode, run only one time.
         */
        singleRun: true,

        customLaunchers: {
            ChromeCustom: {
                base: 'ChromeHeadless',
                /*
                 * We must disable the Chrome sandbox (Chrome's sandbox needs more permissions than Docker allows by default).
                 */
                flags: ['--no-sandbox'],
            },
        },

        /*
         * Run with chrome because phantom js does not provide all types.
         *
         * Available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
         */
        browsers: ['ChromeCustom'],
    };

    config.set(_config);
};
