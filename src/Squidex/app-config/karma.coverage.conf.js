var webpackConfig = require('./webpack.test.coverage');

module.exports = function (config) {
    var _config = {
        /** 
         * Base path that will be used to resolve all patterns (e.g. files, exclude)
         */
        basePath: '',

        frameworks: ['jasmine'],

        /**
         * Load additional test shim to setup angular2 for testing
         */
        files: [
            { pattern: './app-config/karma-test-shim.js', watched: false }
        ],

        preprocessors: {
            './app-config/karma-test-shim.js': ['webpack', 'sourcemap'],
        },

        /**
         * Load the files with webpack and use test configuration for it.
         */
        webpack: webpackConfig,

        webpackMiddleware: {
            stats: 'errors-only'
        },

        webpackServer: {
            noInfo: true
        },

        /*
         * Use a mocha style console reporter, html reporter and the code coverage reporter
         */
        reporters: ['mocha', 'html', 'coverage'],

        // HtmlReporter configuration
        htmlReporter: {
            useCompactStyle: true,
            /** 
             * Use the same folder like the html report for coverage reports
             */
            outputFile: '_test-output/tests.html',

            /**
             * Group the output by test suite (describe), equivalent to mocha reporter
             */
            groupSuites: true
        },

        coverageReporter: {
            type: 'html',
            /** 
             * Use the same folder like the html report for coverage reports
             */
            dir: '_test-output/coverage'
        },

        /**
         * Disable continuous Integration mode, run only one time
         */
        singleRun: true,

        customLaunchers: {
            ChromeCustom: {
			    base: 'ChromeHeadless',
                // We must disable the Chrome sandbox (Chrome's sandbox needs more permissions than Docker allows by default)
                flags: ['--no-sandbox']
            }
          },

        /**
         * Run with chrome because phantom js does not provide all types, e.g. DragEvent
         * 
         * available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
         */
        browsers: ['ChromeCustom']
    };

    config.set(_config);
};