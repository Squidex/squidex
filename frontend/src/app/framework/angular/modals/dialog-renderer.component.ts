/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */


import { Component, DestroyRef, effect, inject, signal, untracked } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { timer } from 'rxjs';
import { DialogModel, DialogRequest, DialogService, Notification, Tooltip } from '@app/framework/internal';
import { FocusOnInitDirective } from '../forms/focus-on-init.directive';
import { MarkdownDirective } from '../markdown.directive';
import { TranslatePipe } from '../pipes/translate.pipe';
import { ModalDialogComponent } from './modal-dialog.component';
import { ModalPlacementDirective } from './modal-placement.directive';
import { ModalDirective } from './modal.directive';
import { TooltipDirective } from './tooltip.directive';

@Component({
    selector: 'sqx-dialog-renderer',
    styleUrls: ['./dialog-renderer.component.scss'],
    templateUrl: './dialog-renderer.component.html',
    imports: [
        FocusOnInitDirective,
        MarkdownDirective,
        ModalDialogComponent,
        ModalDirective,
        ModalPlacementDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class DialogRendererComponent {
    private readonly destroyRef = inject(DestroyRef);
    private readonly dialogs = inject(DialogService);

    protected readonly dialogRequest = signal<DialogRequest | null | undefined>(undefined);

    protected readonly notifications = signal<ReadonlyArray<Notification>>([]);

    protected readonly tooltips = signal<ReadonlyArray<Tooltip>>([]);

    protected readonly dialogView = new DialogModel();

    constructor() {
        effect(() => {
            if (!this.dialogView.isOpen()) {
                untracked(() => this.finishRequest(false));
            }
        });

        this.dialogs.notifications.pipe(takeUntilDestroyed())
            .subscribe(notification => {
                this.notifications.update(x => [...x, notification]);

                if (notification.displayTime > 0) {
                    timer(notification.displayTime).pipe(takeUntilDestroyed(this.destroyRef))
                        .subscribe(() => {
                            this.close(notification);
                        });
                }
            });

        this.dialogs.dialogs.pipe(takeUntilDestroyed())
            .subscribe(dialogRequest => {
                this.cancel();

                this.dialogView.show();
                this.dialogRequest.set(dialogRequest);
            });

        this.dialogs.tooltips.pipe(takeUntilDestroyed())
            .subscribe(tooltip => {
                this.tooltips.update(tooltips => addTooltip(tooltips, tooltip));
            });
    }

    protected cancel() {
        this.finishRequest(false);

        this.dialogView.hide();
    }

    protected confirm() {
        this.finishRequest(true);

        this.dialogView.hide();
    }

    protected close(notification: Notification) {
        this.notifications.update(x => x.filter(n => notification !== n));
    }

    private finishRequest(value: boolean) {
        this.dialogRequest()?.complete(value);
        this.dialogRequest.set(null);
    }
}

function addTooltip(tooltips: ReadonlyArray<Tooltip>, tooltip: Tooltip) {
    if (tooltip.multiple || !tooltip.text) {
        tooltips = tooltips.filter(x => x.target !== tooltip.target);
    }

    if (!tooltip.text) {
        return tooltips;
    }

    if (tooltip.multiple) {
        return [tooltip, ...tooltips];
    }

    return [tooltip];
}
