/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, input, numberAttribute } from '@angular/core';

@Component({
    selector: 'sqx-form-hint',
    styleUrls: ['./form-hint.component.scss'],
    templateUrl: './form-hint.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormHintComponent {
    /** Additional CSS classes for the hint element. */
    public readonly class = input('');

    /** The top margin as Bootstrap spacing level (0-5). */
    public readonly marginTop = input(0, { transform: numberAttribute });

    /** The bottom margin as Bootstrap spacing level (0-5). */
    public readonly marginBottom = input(0, { transform: numberAttribute });
}
