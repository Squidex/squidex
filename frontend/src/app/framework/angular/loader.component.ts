/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, numberAttribute } from '@angular/core';

@Component({
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
