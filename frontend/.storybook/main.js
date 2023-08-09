/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

const customConfig = require('./../src/config/webpack.config');
module.exports = {
  stories: ["../src/**/*.stories.@(js|jsx|ts|tsx)"],
  addons: ["@storybook/addon-links", "@storybook/addon-essentials", "@storybook/addon-interactions"],
  framework: {
    name: "@storybook/angular",
    options: {}
  },
  webpackFinal: async config => {
    customConfig(config, {}, {});
    return config;
  },
  docs: {
    autodocs: true
  }
};