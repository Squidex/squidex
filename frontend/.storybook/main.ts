/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import type { StorybookConfig } from '@analogjs/storybook-angular';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import tsconfigPaths from 'vite-tsconfig-paths';
import { fileURLToPath } from 'url';
import { dirname, resolve } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const config: StorybookConfig = {
    stories: ['../src/**/*.stories.@(js|jsx|ts|tsx)'],
    addons: ['@storybook/addon-links'],
    framework: {
        name: '@analogjs/storybook-angular',
        options: {},
    },
    async viteFinal(config) {
        const { mergeConfig } = await import('vite');

        return mergeConfig(config, {
            plugins: [
                tsconfigPaths(),
                viteStaticCopy({
                    targets: [
                        {
                            src: './node_modules/ace-builds/src-min/*',
                            dest: 'dependencies/ace',
                        },
                    ],
                }),
            ],
            resolve: {
                extensions: [
                    '.ts',
                    '.tsx',
                    '.mjs',
                    '.js',
                    '.jsx',
                    '.json',
                    '.d.ts',
                ],
                alias: {
                    '@app/framework/internal': resolve(
                        __dirname,
                        '../src/app/framework/internal.ts',
                    ),
                    '@app/framework': resolve(
                        __dirname,
                        '../src/app/framework/index.ts',
                    ),
                    '@app/shared/internal': resolve(
                        __dirname,
                        '../src/app/shared/internal.ts',
                    ),
                    '@app/shared': resolve(
                        __dirname,
                        '../src/app/shared/index.ts',
                    ),
                },
            },
            css: {
                ...config.css,
                preprocessorOptions: {
                    scss: {
                        loadPaths: [resolve(__dirname, '../src/app/theme')],
                        includePaths: ['node_modules'],
                        quietDeps: true,
                        silenceDeprecations: ['color-functions', 'global-builtin', 'import', 'if-function'],
                    },
                },
            },
            optimizeDeps: {
                exclude: ['function-bind'],
            },
            ssr: {
                noExternal: [
                    '@angular/core',
                    '@angular/common',
                    '@angular/forms',
                    '@angular/router',
                ],
            },
        });
    },
};

export default config;
