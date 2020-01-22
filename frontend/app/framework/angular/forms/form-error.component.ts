/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges } from '@angular/core';

import { ErrorDto } from '@app/framework/internal';

@Component({
    selector: 'sqx-form-error',
    template: `
        <ng-container *ngIf="show">
            <div [class.form-bubble]="bubble">
                <div class="form-alert form-alert-error" [class.closeable]="closeable">
                    <a class="form-alert-close" (click)="close()">
                        <i class="icon-close"></i>
                    </a>
                    <div [innerHTML]="error?.displayMessage | sqxMarkdown"></div>
                </div>
            </div>
        </ng-container>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormErrorComponent implements OnChanges {
    @Input()
    public error?: ErrorDto | null;

    @Input()
    public bubble = false;

    @Input()
    public closeable = false;

    public show: boolean;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['error']) {
            this.show = !!this.error;
        }
    }

    public close() {
        this.show = false;
    }
}