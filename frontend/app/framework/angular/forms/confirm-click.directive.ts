/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { Directive, EventEmitter, HostListener, Input, OnDestroy, Output } from '@angular/core';

import { DialogService } from '@app/framework/internal';

class DelayEventEmitter<T> extends EventEmitter<T> {
    private delayedNexts: any[] | null = [];

    public delayEmit() {
        if (this.delayedNexts) {
            for (const callback of this.delayedNexts) {
                callback();
            }
        }
    }

    public clear() {
        this.delayedNexts = null;
    }

    public subscribe(generatorOrNext?: any, error?: any, complete?: any): any {
        if (this.delayedNexts) {
            this.delayedNexts.push(generatorOrNext);
        }

        return super.subscribe(generatorOrNext, error, complete);
    }
}

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

    @Output('sqxConfirmClick')
    public clickConfirmed = new DelayEventEmitter();

    constructor(
        private readonly dialogs: DialogService
    ) {
    }

    public ngOnDestroy() {
        this.isDestroyed = true;

        if (!this.isOpen) {
            this.clickConfirmed.clear();
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

            const subscription =
                this.dialogs.confirm(this.confirmTitle, this.confirmText)
                    .subscribe(result => {
                        this.isOpen = false;

                        if (result) {
                            this.clickConfirmed.delayEmit();
                        }

                        subscription.unsubscribe();

                        if (this.isDestroyed) {
                            this.clickConfirmed.clear();
                        }
                    });
        } else {
            this.clickConfirmed.emit();
        }

        event.preventDefault();
    }
}
