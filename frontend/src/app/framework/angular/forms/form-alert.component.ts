/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, input, numberAttribute } from '@angular/core';

@Component({
    selector: 'sqx-form-alert',
    styleUrls: ['./form-alert.component.scss'],
    templateUrl: './form-alert.component.html',
})
export class FormAlertComponent {
    /** Additional CSS classes for the alert element. */
    public readonly class = input('');

    /** The top margin as Bootstrap spacing level (0-5). */
    public readonly marginTop = input(2, { transform: numberAttribute });

    /** The bottom margin as Bootstrap spacing level (0-5). */
    public readonly marginBottom = input(4, { transform: numberAttribute });

    /** Renders the alert with a lighter background. */
    public readonly light = input(false, { transform: booleanAttribute });

    /** Renders the alert with a larger font and padding. */
    public readonly large = input(false, { transform: booleanAttribute });
}
