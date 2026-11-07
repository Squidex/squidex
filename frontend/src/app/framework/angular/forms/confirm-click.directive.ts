/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Directive, inject, input, output } from '@angular/core';
import { take } from 'rxjs';
import { DialogService } from '@app/framework/internal';

@Directive({
    selector: '[sqxConfirmClick]',
    host: {
        '(click)': 'onClick($event)',
    },
})
export class ConfirmClickDirective {
    private readonly dialogs = inject(DialogService);

    /** The title of the confirmation dialog. */
    public readonly confirmTitle = input<string | undefined | null>();

    /** The text of the confirmation dialog. */
    public readonly confirmText = input<string | undefined | null>();

    /** Shows the confirmation dialog before emitting the click. */
    public readonly confirmRequired = input(true, { transform: booleanAttribute });

    /** The key to remember the decision of the user, so the dialog is not shown again. */
    public readonly confirmRememberKey = input<string | undefined | null>();

    /** Emits before the confirmation dialog is shown. */
    public readonly beforeClick = output();

    /** Emits when the click is confirmed or no confirmation is required. */
    public readonly clickConfirmed = output<boolean | undefined>({ alias: 'sqxConfirmClick' });

    protected onClick(event: Event) {
        event.preventDefault();

        const title = this.confirmTitle();
        const text = this.confirmText();

        if (!this.confirmRequired() || !title || !text) {
            this.clickConfirmed.emit(undefined);
            return;
        }

        const listeners: ((value: boolean) => void)[] = [...((this.clickConfirmed as any)['listeners'] || [])];

        this.beforeClick.emit();

        const rememberKey = this.confirmRememberKey() || '';
        this.dialogs.confirm(title, text, rememberKey).pipe(take(1))
            .subscribe(confirmed => {
                if (confirmed) {
                    for (const listener of listeners) {
                        listener(true);
                    }
                }
            });
    }
}
