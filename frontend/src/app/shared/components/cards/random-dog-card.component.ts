/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';

@Component({
    selector: 'sqx-random-dog-card',
    styleUrls: ['./random-dog-card.component.scss'],
    templateUrl: './random-dog-card.component.html',
})
export class RandomDogCardComponent {
    protected get random() {
        return new Date().getTime();
    }
}
