/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Location } from '@angular/common';
import { Component } from '@angular/core';
import { TitleComponent, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-not-found-page',
    styleUrls: ['./not-found-page.component.scss'],
    templateUrl: './not-found-page.component.html',
    imports: [
        TitleComponent,
        TranslatePipe,
    ],
})
export class NotFoundPageComponent {
    constructor(
        private readonly location: Location,
    ) {
    }

    public back() {
        this.location.back();
    }
}
