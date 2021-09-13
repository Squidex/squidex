/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
    selector: 'sqx-random-dog-card',
    styleUrls: ['./random-dog-card.component.scss'],
    templateUrl: './random-dog-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RandomDogCardComponent {
    public get random() {
        return new Date().getTime();
    }
}
