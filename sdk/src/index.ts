/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import './style/index.scss';
import { renderOverlay } from './render';
import { getBaseUrl } from './utils';

const baseUrl = getBaseUrl();

function init() {
    renderOverlay(baseUrl);
    renderStyles();
}

function renderStyles() {
    if (!baseUrl) {
        return;
    }

    const styleElement = document.createElement('link');

    styleElement.rel = 'stylesheet';
    styleElement.href = `${baseUrl}/scripts/embed-sdk.css`;
    styleElement.type = 'text/css';

    document.head?.appendChild(styleElement);
}

if (document.readyState === 'complete') {
    init();
} else {
    window.addEventListener('load', () => init());
}