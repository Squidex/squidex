/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { timer } from 'rxjs';

import {
    DialogModel,
    DialogRequest,
    DialogService,
    fadeAnimation,
    Notification,
    StatefulComponent,
    Tooltip
} from '@app/framework/internal';

interface State {
    // The pending dialog request.
    dialogRequest?: DialogRequest | null;

    // The list of open notifications.
    notifications: ReadonlyArray<Notification>;

    // The current tooltip.
    tooltip?: Tooltip | null;
}

@Component({
    selector: 'sqx-dialog-renderer',
    styleUrls: ['./dialog-renderer.component.scss'],
    templateUrl: './dialog-renderer.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DialogRendererComponent extends StatefulComponent<State> implements OnInit {
    public dialogView = new DialogModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly dialogs: DialogService
    ) {
        super(changeDetector, { notifications: [] });
    }

    public ngOnInit() {
        this.own(
            this.dialogView.isOpen.subscribe(isOpen => {
                if (!isOpen) {
                    this.finishRequest(false);
                }
            }));

        this.own(
            this.dialogs.notifications.subscribe(notification => {
                this.next(s => ({
                    ...s,
                    notifications: [...s.notifications, notification]
                }));

                if (notification.displayTime > 0) {
                    this.own(timer(notification.displayTime).subscribe(() => {
                        this.close(notification);
                    }));
                }
            }));

        this.own(
            this.dialogs.dialogs
                .subscribe(dialogRequest => {
                    this.cancel();

                    this.dialogView.show();

                    this.next(s => ({ ...s, dialogRequest }));
                }));

        this.own(
            this.dialogs.tooltips
                .subscribe(tooltip => {
                    if (tooltip.text) {
                        this.next(s => ({ ...s, tooltip }));
                    } else if (!this.snapshot.tooltip || tooltip.target === this.snapshot.tooltip.target) {
                        this.next(s => ({ ...s, tooltip: null }));
                    }
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
        this.next(s => {
            if (s.dialogRequest) {
                s.dialogRequest.complete(value);
            }

            return { ...s, dialogRequest: null };
        });
    }

    public close(notification: Notification) {
        this.next(s => ({ ...s, notifications: s.notifications.filter(n => notification !== n) }));
    }
}