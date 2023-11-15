/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
    standalone: true,
    selector: 'sqx-random-cat-card',
    styleUrls: ['./random-cat-card.component.scss'],
    templateUrl: './random-cat-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RandomCatCardComponent {
    public get random() {
        return new Date().getTime();
    }
}
