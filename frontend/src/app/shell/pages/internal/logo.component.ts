/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, input } from '@angular/core';
import { LoaderComponent } from '@app/shared';

@Component({
    selector: 'sqx-logo',
    styleUrls: ['./logo.component.scss'],
    templateUrl: './logo.component.html',
    imports: [
        LoaderComponent,
    ],
})
export class LogoComponent {
    /** Shows a loading indicator instead of the logo. */
    public readonly isLoading = input(false, { transform: booleanAttribute });
}
