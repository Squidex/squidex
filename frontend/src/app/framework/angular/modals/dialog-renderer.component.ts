/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { timer } from 'rxjs';
import { DialogModel, DialogRequest, DialogService, fadeAnimation, Notification, StatefulComponent, Tooltip } from '@app/framework/internal';

interface State {
    // The pending dialog request.
    dialogRequest?: DialogRequest | null;

    // The list of open notifications.
    notifications: ReadonlyArray<Notification>;

    // The current tooltip.
    tooltips: ReadonlyArray<Tooltip>;
}

@Component({
    selector: 'sqx-dialog-renderer',
    styleUrls: ['./dialog-renderer.component.scss'],
    templateUrl: './dialog-renderer.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DialogRendererComponent extends StatefulComponent<State> implements OnInit {
    public dialogView = new DialogModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly dialogs: DialogService,
    ) {
        super(changeDetector, { notifications: [], tooltips: [] });
    }

    public ngOnInit() {
        this.own(
            this.dialogView.isOpenChanges.subscribe(isOpen => {
                if (!isOpen) {
                    this.finishRequest(false);
                }
            }));

        this.own(
            this.dialogs.notifications.subscribe(notification => {
                this.next(s => ({
                    ...s,
                    notifications: [...s.notifications, notification],
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

                    this.next({ dialogRequest });
                }));

        this.own(
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
