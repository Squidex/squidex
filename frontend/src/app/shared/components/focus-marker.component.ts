/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, input, Optional, Renderer2 } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { of } from 'rxjs';
import { StringColorPipe } from '@app/framework';
import { CollaborationService, Subscriptions } from '@app/shared/internal';

@Component({
    selector: 'sqx-focus-marker',
    styleUrls: ['./focus-marker.component.scss'],
    templateUrl: './focus-marker.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        StringColorPipe,
    ],
})
export class FocusMarkerComponent {
    private readonly subscriptions = new Subscriptions();
    private readonly users;

    /** The ID of the control, to show which other user has focused it. */
    public readonly controlId = input.required<string>();

    public readonly focusedUser = computed(() => {
        const controlId = this.controlId();

        return this.users().find(x => x['focusedControl'] === controlId)?.user;
    });

    constructor(
        renderer: Renderer2,
        @Optional() private readonly collaboration?: CollaborationService,
    ) {
        this.users = toSignal(collaboration?.userChanges ?? of([]), { initialValue: [] });

        if (!collaboration) {
            return;
        }

        this.subscriptions.add(
            renderer.listen('window', 'click', (event: MouseEvent) => {
                this.updateFocusedControl(event, null);
            }));
    }

    public onClick(event: MouseEvent) {
        this.updateFocusedControl(event, this.controlId());
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
