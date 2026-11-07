/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */


import { ChangeDetectionStrategy, Component, effect, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subscription, timer } from 'rxjs';
import { DialogModel, DialogRequest, DialogService, Notification, Tooltip } from '@app/framework/internal';
import { FocusOnInitDirective } from '../forms/focus-on-init.directive';
import { MarkdownDirective } from '../markdown.directive';
import { TranslatePipe } from '../pipes/translate.pipe';
import { ModalDialogComponent } from './modal-dialog.component';
import { ModalPlacementDirective } from './modal-placement.directive';
import { ModalDirective } from './modal.directive';
import { TooltipDirective } from './tooltip.directive';

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

@Component({
    selector: 'sqx-dialog-renderer',
    styleUrls: ['./dialog-renderer.component.scss'],
    templateUrl: './dialog-renderer.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FocusOnInitDirective,
        FormsModule,
        MarkdownDirective,
        ModalDialogComponent,
        ModalDirective,
        ModalPlacementDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class DialogRendererComponent {
    public readonly dialogView = new DialogModel();
    public readonly dialogRequest = signal<DialogRequest | null | undefined>(undefined);
    public readonly notifications = signal<ReadonlyArray<Notification>>([]);
    public readonly tooltips = signal<ReadonlyArray<Tooltip>>([]);

    constructor(dialogs: DialogService) {
        effect(onCleanup => {
            const subscriptions = new Subscription();

            untracked(() => {
                subscriptions.add(
                    this.dialogView.isOpenChanges.subscribe(isOpen => {
                        if (!isOpen) {
                            this.finishRequest(false);
                        }
                    }));

                subscriptions.add(
                    dialogs.notifications.subscribe(notification => {
                        this.notifications.update(x => [...x, notification]);

                        if (notification.displayTime > 0) {
                            subscriptions.add(timer(notification.displayTime).subscribe(() => {
                                this.close(notification);
                            }));
                        }
                    }));

                subscriptions.add(
                    dialogs.dialogs.subscribe(dialogRequest => {
                        this.cancel();

                        this.dialogView.show();
                        this.dialogRequest.set(dialogRequest);
                    }));

                subscriptions.add(
                    dialogs.tooltips.subscribe(tooltip => {
                        this.tooltips.update(x => addTooltip(x, tooltip));
                    }));
            });

            onCleanup(() => subscriptions.unsubscribe());
        });
    }

    public cancel() {
        this.finishRequest(false);

        this.dialogView.hide();
    }

    public confirm() {
        this.finishRequest(true);

        this.dialogView.hide();
    }

    public close(notification: Notification) {
        this.notifications.update(x => x.filter(n => notification !== n));
    }

    private finishRequest(value: boolean) {
        this.dialogRequest()?.complete(value);
        this.dialogRequest.set(null);
    }
}
