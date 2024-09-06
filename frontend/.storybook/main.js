/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

const CopyPlugin = require('copy-webpack-plugin');

module.exports = {
  stories: ["../src/**/*.stories.@(js|jsx|ts|tsx)"],
  addons: [
    "@storybook/addon-links",
    "@storybook/addon-essentials",
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

    config.resolve?.extensions?.push('.d.ts');
    return config;
  },
  docs: {
    autodocs: true
  }
};