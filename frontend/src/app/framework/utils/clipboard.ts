/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Types } from './types';

export async function copyElementText(element: HTMLElement) {
    const text = Types.is(element, HTMLInputElement) || Types.is(element, HTMLTextAreaElement) ? element.value : element.innerText;

    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch {
        return false;
    }
}
