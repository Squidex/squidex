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
        webpack: webpackConfig({ target: 'tests', jit: true }),

        webpackMiddleware: {
            stats: 'errors-only',
        },

        /*
         * Leave Jasmine Spec Runner output visible in browser.
         */
        client: {
            clearContext: false,
        },

        /*
         * Use a mocha style console reporter and html reporter.
         */
        reporters: ['kjhtml', 'mocha'],

        /*
         * Run with chrome to enable debugging.
         *
         * available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
         */
        browsers: ['Chrome'],
    };

    config.set(_config);
};
