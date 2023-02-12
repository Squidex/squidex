/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ErrorDto } from '@app/framework/internal';
import { TypedSimpleChanges } from './../helpers';

@Component({
    selector: 'sqx-form-error[error]',
    styleUrls: ['./form-error.component.scss'],
    templateUrl: './form-error.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormErrorComponent {
    @Input()
    public error?: ErrorDto | null;

    @Input()
    public bubble?: boolean | null;

    @Input()
    public closeable?: boolean | null;

    public show = false;

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.error) {
            this.show = !!this.error;
        }
    }

    public close() {
        this.show = false;
    }
}
