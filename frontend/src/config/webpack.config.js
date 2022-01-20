const plugins = {
    // https://webpack.js.org/plugins/eslint-webpack-plugin/
    ESLintPlugin: require('eslint-webpack-plugin'),
    // https://github.com/webpack-contrib/stylelint-webpack-plugin
    StylelintPlugin: require('stylelint-webpack-plugin'),
    // https://github.com/webpack-contrib/mini-css-extract-plugin
    MiniCssExtractPlugin: require('mini-css-extract-plugin'),
    // https://webpack.js.org/plugins/copy-webpack-plugin/
    CopyPlugin: require('copy-webpack-plugin'),
};

module.exports = (config, _, options) => {
    /*
     * Copy lazy loaded libraries to output.
     */
    config.plugins.push(new plugins.CopyPlugin({
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
    }));

    config.plugins.push(new plugins.StylelintPlugin({
        files: '**/*.scss',
    }));

    if (options.target === 'build') {
        config.plugins.push(
            new plugins.ESLintPlugin({
                files: [
                    './app/**/*.ts',
                ],
            }),
        );
    }

    const index = config.plugins.findIndex(x => x instanceof plugins.MiniCssExtractPlugin);

    if (index >= 0) {
        config.plugins.splice(index, 1);
    }
    
    config.plugins.push(new plugins.MiniCssExtractPlugin({
        filename: '[name].css',
    }));
    
    /*
     * Specifies the name of each output file on disk.
     *
     * See: https://webpack.js.org/configuration/output/#output-filename
     */
    config.output.filename = '[name].js';

    /*
     * The filename of non-entry chunks as relative path inside the output.path directory.
     *
     * See: https://webpack.js.org/configuration/output/#output-chunkfilename
     */
    config.output.chunkFilename = '[id].[fullhash].chunk.js';

    /* 
     * The filename for assets.
     */
    config.output.assetModuleFilename = 'assets/[hash][ext][query]';

    return config;
};