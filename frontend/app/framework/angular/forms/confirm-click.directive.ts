/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { Directive, EventEmitter, HostListener, Input, Output } from '@angular/core';
import { DialogService } from '@app/framework/internal';
import { Subscriber } from 'rxjs';
import { take } from 'rxjs/operators';

@Directive({
    selector: '[sqxConfirmClick]'
})
export class ConfirmClickDirective {
    @Input()
    public confirmTitle: string;

    @Input()
    public confirmText: string;

    @Input()
    public confirmRememberKey: string;

    @Input()
    public confirmRequired = true;

    @Output()
    public beforeClick = new EventEmitter();

    @Output('sqxConfirmClick')
    public clickConfirmed = new EventEmitter();

    constructor(
        private readonly dialogs: DialogService
    ) {
    }

    @HostListener('click', ['$event'])
    public onClick(event: Event) {
        if (this.confirmRequired &&
            this.confirmTitle &&
            this.confirmTitle.length > 0 &&
            this.confirmText &&
            this.confirmText.length > 0) {

            const observers = [...this.clickConfirmed.observers];

            this.beforeClick.emit();

            this.dialogs.confirm(this.confirmTitle, this.confirmText, this.confirmRememberKey).pipe(take(1))
                .subscribe(confirmed => {
                    if (confirmed) {
                        for (const observer of observers) {
                            const subscriber = observer as Subscriber<any>;

                            if (subscriber['destination'] && subscriber['destination'].next) {
                                subscriber['destination'].next(true);
                            }
                        }
                    }
                });
        } else {
            this.clickConfirmed.emit();
        }

        event.preventDefault();
    }
}
