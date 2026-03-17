/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Preview } from "@storybook/angular";
import "./../src/styles.scss";

const preview: Preview = {
    parameters: {
        actions: {
            argTypesRegex: "^on[A-Z].*",
        },
        controls: {
            matchers: {
                color: /(background|color)$/i,
                date: /Date$/,
            },
        },
        docs: {
            inlineStories: true,
        },
    },
};

export default preview;
