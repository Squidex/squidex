/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

const CopyPlugin = require('copy-webpack-plugin');

class FilterSassWarningsPlugin {
  apply(compiler) {
    compiler.hooks.done.tap('FilterSassWarningsPlugin', (stats) => {
      stats.compilation.warnings = stats.compilation.warnings.filter(warning => {
        const message = warning.message || warning.toString();
        return !message.includes('sass-loader');
      });
    });
  }
}

module.exports = {
  stories: ["../src/**/*.stories.@(js|jsx|ts|tsx)"],
  addons: [
    "@storybook/addon-links",
    "@storybook/addon-interactions"
  ],
  framework: {
    name: "@storybook/angular",
    options: {}
  },
  webpackFinal: async config => {
    /*
     * Copy lazy loaded libraries to output.
     */
    config.plugins.push(new CopyPlugin({
        patterns: [
            { from: './node_modules/ace-builds/src-min/', to: './dependencies/ace/' },
        ]
    }));

    config.plugins.push(new FilterSassWarningsPlugin());
    config.resolve?.extensions?.push('.d.ts');
    return config;
  },
  docs: {
    autodocs: true
  }
};