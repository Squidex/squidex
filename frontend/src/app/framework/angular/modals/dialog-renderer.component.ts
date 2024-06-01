/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */


import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { timer } from 'rxjs';
import { DialogModel, DialogRequest, DialogService, fadeAnimation, Notification, StatefulComponent, Subscriptions, Tooltip } from '@app/framework/internal';
import { FocusOnInitDirective } from '../forms/focus-on-init.directive';
import { MarkdownPipe } from '../pipes/markdown.pipe';
import { TranslatePipe } from '../pipes/translate.pipe';
import { SafeHtmlPipe } from '../safe-html.pipe';
import { ModalDialogComponent } from './modal-dialog.component';
import { ModalPlacementDirective } from './modal-placement.directive';
import { ModalDirective } from './modal.directive';
import { TooltipDirective } from './tooltip.directive';

interface State {
    // The pending dialog request.
    dialogRequest?: DialogRequest | null;

    // The list of open notifications.
    notifications: ReadonlyArray<Notification>;

    // The current tooltip.
    tooltips: ReadonlyArray<Tooltip>;
}

@Component({
    standalone: true,
    selector: 'sqx-dialog-renderer',
    styleUrls: ['./dialog-renderer.component.scss'],
    templateUrl: './dialog-renderer.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FocusOnInitDirective,
        FormsModule,
        MarkdownPipe,
        ModalDialogComponent,
        ModalDirective,
        ModalPlacementDirective,
        SafeHtmlPipe,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class DialogRendererComponent extends StatefulComponent<State> implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public dialogView = new DialogModel();

    constructor(
        private readonly dialogs: DialogService,
    ) {
        super({ notifications: [], tooltips: [] });
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.dialogView.isOpenChanges.subscribe(isOpen => {
                if (!isOpen) {
                    this.finishRequest(false);
                }
            }));

        this.subscriptions.add(
            this.dialogs.notifications.subscribe(notification => {
                this.next(s => ({
                    ...s,
                    notifications: [...s.notifications, notification],
                }));

                if (notification.displayTime > 0) {
                    this.subscriptions.add(timer(notification.displayTime).subscribe(() => {
                        this.close(notification);
                    }));
                }
            }));

        this.subscriptions.add(
            this.dialogs.dialogs
                .subscribe(dialogRequest => {
                    this.cancel();

                    this.dialogView.show();

                    this.next({ dialogRequest });
                }));

        this.subscriptions.add(
            this.dialogs.tooltips
                .subscribe(tooltip => {
                    this.next(s => {
                        let tooltips = s.tooltips;

                        if (tooltip.multiple || !tooltip.text) {
                            tooltips = tooltips.filter(x => x.target !== tooltip.target);
                        }

                        if (tooltip.text) {
                            if (tooltip.multiple) {
                                tooltips = [tooltip, ...tooltips];
                            } else {
                                tooltips = [tooltip];
                            }
                        }

                        return { ...s, tooltips };
                    });
                }));
    }

    public cancel() {
        this.finishRequest(false);

        this.dialogView.hide();
    }

    public confirm() {
        this.finishRequest(true);

        this.dialogView.hide();
    }

    private finishRequest(value: boolean) {
        this.snapshot.dialogRequest?.complete(value);

        this.next({ dialogRequest: null });
    }

    public close(notification: Notification) {
        this.next(s => ({
            ...s,
            notifications: s.notifications.filter(n => notification !== n),
        }));
    }
}
