/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, EventEmitter, HostListener, Input, Output } from '@angular/core';

import { DialogService } from './../services/dialog.service';

@Directive({
    selector: '[sqxConfirmClick]'
})
export class ConfirmClickDirective {
    @Input()
    public confirmTitle: string;

    @Input()
    public confirmText: string;

    @Output('sqxConfirmClick')
    public click = new EventEmitter();

    constructor(
        private readonly dialogService: DialogService
    ) {
    }

    @HostListener('click', ['$event'])
    public onClick(event: Event) {
        if (this.confirmTitle &&
            this.confirmTitle.length > 0 &&
            this.confirmText &&
            this.confirmText.length > 0) {

            let subscription =
                this.dialogService.confirm(this.confirmTitle, this.confirmText)
                    .subscribe(result => {
                        if (result) {
                            this.click.emit();
                        }

                        subscription.unsubscribe();
                    });
        } else {
            this.click.emit();
        }

        event.stopPropagation();
        event.preventDefault();
    }
}