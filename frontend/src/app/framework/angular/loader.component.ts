/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable import/no-cycle */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
    selector: 'sqx-loader',
    styleUrls: ['./loader.component.scss'],
    templateUrl: './loader.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoaderComponent {
    @Input()
    public size = 18;

    @Input()
    public color: 'input' | 'theme' | 'white' | 'text' = 'text';
}