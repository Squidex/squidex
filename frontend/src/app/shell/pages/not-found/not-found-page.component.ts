/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Location } from '@angular/common';
import { Component, inject } from '@angular/core';
import { TitleComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-not-found-page',
    styleUrls: ['./not-found-page.component.scss'],
    templateUrl: './not-found-page.component.html',
    imports: [
        TitleComponent,
        TranslatePipe,
    ],
})
export class NotFoundPageComponent {
    private readonly location = inject(Location);


    protected back() {
        this.location.back();
    }
}
