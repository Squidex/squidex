/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { Directive, EventEmitter, HostListener, Input, OnDestroy, Output } from '@angular/core';
import { DialogService } from '@app/framework/internal';

@Directive({
    selector: '[sqxConfirmClick]'
})
export class ConfirmClickDirective implements OnDestroy {
    private isOpen = false;
    private isDestroyed = false;

    @Input()
    public confirmTitle: string;

    @Input()
    public confirmText: string;

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

    public ngOnDestroy() {
        this.isDestroyed = true;

        if (!this.isOpen) {
            this.clickConfirmed.complete();
        }
    }

    @HostListener('click', ['$event'])
    public onClick(event: Event) {
        if (this.confirmRequired &&
            this.confirmTitle &&
            this.confirmTitle.length > 0 &&
            this.confirmText &&
            this.confirmText.length > 0) {

            this.isOpen = true;

            this.beforeClick.emit();

            const subscription =
                this.dialogs.confirm(this.confirmTitle, this.confirmText)
                    .subscribe(confirmed => {
                        this.isOpen = false;

                        if (confirmed) {
                            this.clickConfirmed.emit();
                        }

                        subscription.unsubscribe();

                        if (this.isDestroyed) {
                            this.clickConfirmed.complete();
                        }
                    });
        } else {
            this.clickConfirmed.emit();
        }

        event.preventDefault();
    }
}
