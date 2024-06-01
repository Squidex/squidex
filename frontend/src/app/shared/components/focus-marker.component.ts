/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, Input, Optional, Renderer2 } from '@angular/core';
import { map } from 'rxjs';
import { StringColorPipe } from '@app/framework';
import { CollaborationService, Subscriptions } from '@app/shared/internal';

@Component({
    standalone: true,
    selector: 'sqx-focus-marker',
    styleUrls: ['./focus-marker.component.scss'],
    templateUrl: './focus-marker.component.html',
    imports: [
        AsyncPipe,
        StringColorPipe,
    ],
})
export class FocusMarkerComponent {
    private readonly subscriptions = new Subscriptions();

    @Input({ required: true })
    public controlId!: string;

    public focusedUser =
        this.collaboration?.userChanges.pipe(
            map(u => u.find(x => x['focusedControl'] === this.controlId)?.user));

    constructor(
        private readonly renderer: Renderer2,
        @Optional() private readonly collaboration?: CollaborationService,
    ) {
    }

    public ngOnInit() {
        if (!this.collaboration) {
            return;
        }

        this.subscriptions.add(
            this.renderer.listen('window', 'click', (event: MouseEvent) => {
                this.updateFocusedControl(event, null);
            }));
    }

    public onClick(event: MouseEvent) {
        this.updateFocusedControl(event, this.controlId);
    }

    private updateFocusedControl(event: MouseEvent, controlId: string | null) {
        if (!this.collaboration) {
            return;
        }

        if ((event as any)['handled'] === true) {
            return;
        }

        this.collaboration.updateAwareness('focusedControl', controlId);

        (event as any)['handled'] = true;
    }
}
