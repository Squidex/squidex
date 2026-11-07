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
    selector: 'sqx-forbidden-page',
    styleUrls: ['./forbidden-page.component.scss'],
    templateUrl: './forbidden-page.component.html',
    imports: [
        TitleComponent,
        TranslatePipe,
    ],
})
export class ForbiddenPageComponent {
    private readonly location = inject(Location);


    protected back() {
        this.location.back();
    }
}
