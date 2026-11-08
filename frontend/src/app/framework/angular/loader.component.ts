/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, input, numberAttribute } from '@angular/core';

@Component({
    selector: 'sqx-loader',
    styleUrls: ['./loader.component.scss'],
    templateUrl: './loader.component.html',
})
export class LoaderComponent {
    /** The width and height of the spinner in pixels. */
    public readonly size = input(18, { transform: numberAttribute });

    /** The color scheme of the spinner. */
    public readonly color = input<'input' | 'theme' | 'white' | 'text'>('text');
}
