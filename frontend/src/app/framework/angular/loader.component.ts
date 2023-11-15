/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable import/no-cycle */

import { ChangeDetectionStrategy, Component, Input, numberAttribute } from '@angular/core';

@Component({
    standalone: true,
    selector: 'sqx-loader',
    styleUrls: ['./loader.component.scss'],
    templateUrl: './loader.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoaderComponent {
    @Input({ transform: numberAttribute })
    public size = 18;

    @Input()
    public color: 'input' | 'theme' | 'white' | 'text' = 'text';
}
