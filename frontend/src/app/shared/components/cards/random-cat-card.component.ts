/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';

@Component({
    selector: 'sqx-random-cat-card',
    styleUrls: ['./random-cat-card.component.scss'],
    templateUrl: './random-cat-card.component.html',
})
export class RandomCatCardComponent {
    protected get random() {
        return new Date().getTime();
    }
}
