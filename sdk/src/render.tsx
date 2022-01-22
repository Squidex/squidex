/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { h, render } from 'preact';
import { OverlayContainer } from './components/overlay-container';

let element: HTMLDivElement | null = null;

export function renderOverlay(baseUrl: string | null | undefined) {
    if (!element) {
        element = document.body.appendChild(document.createElement('div'));
    }

    render(<OverlayContainer baseUrl={baseUrl} />, element)
}