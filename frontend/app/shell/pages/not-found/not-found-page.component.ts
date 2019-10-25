/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Location } from '@angular/common';
import { Component } from '@angular/core';

@Component({
    selector: 'sqx-not-found-page',
    template: `
        <sqx-title message="Not Found"></sqx-title>

        <div class="landing-page">
            <img class="splash-image" src="~/../squid.svg?title=Not Found&text=This%20is%20not%20the%20page%20you%20are%20looking%20for!&background=white&small" />

            <a href="#" (click)="back()">Back to previous page.</a>
        </div>
    `
})
export class NotFoundPageComponent {
    constructor(
        private readonly location: Location
    ) {
    }

    public back() {
        this.location.back();
    }
}