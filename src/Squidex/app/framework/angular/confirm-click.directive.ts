/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, EventEmitter, HostListener, Input, OnDestroy, Output } from '@angular/core';

import { DialogService } from './../services/dialog.service';

class DelayEventEmitter<T> extends EventEmitter<T> {
    private delayedNexts: any[] | null = [];

    public delayEmit() {
        if (this.delayedNexts) {
            for (let callback of this.delayedNexts) {
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

    @Output('sqxConfirmClick')
    public clickConfirmed = new DelayEventEmitter();

    constructor(
        private readonly dialogService: DialogService
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
        if (this.confirmTitle &&
            this.confirmTitle.length > 0 &&
            this.confirmText &&
            this.confirmText.length > 0) {

            this.isOpen = true;

            let subscription =
                this.dialogService.confirm(this.confirmTitle, this.confirmText)
                    .subscribe(result => {
                        this.isOpen = false;

                        if (result) {
                            if (result) {
                                this.clickConfirmed.delayEmit();
                            }
                        }

                        subscription.unsubscribe();

                        if (this.isDestroyed) {
                            this.clickConfirmed.clear();
                        }
                    });
        } else {
            this.clickConfirmed.emit();
        }

        event.stopPropagation();
        event.preventDefault();
    }
}