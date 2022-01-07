/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ErrorDto } from '@app/framework/internal';

@Component({
    selector: 'sqx-form-error[error]',
    styleUrls: ['./form-error.component.scss'],
    templateUrl: './form-error.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormErrorComponent implements OnChanges {
    @Input()
    public error?: ErrorDto | null;

    @Input()
    public bubble?: boolean | null;

    @Input()
    public closeable?: boolean | null;

    public show = false;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['error']) {
            this.show = !!this.error;
        }
    }

    public close() {
        this.show = false;
    }
}
