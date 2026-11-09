/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Directive, input } from '@angular/core';

@Directive({
    selector: '[sqxStopDrag]',
    host: {
        '(dragstart)': 'onDragStart($event)',
    },
})
export class StopDragDirective {
    /** Prevents dragging the element when true. */
    public readonly shouldStop = input(true, { alias: 'sqxStopDrag', transform: booleanAttribute });

    protected onDragStart(event: Event) {
        if (this.shouldStop()) {
            event.preventDefault();
        }
    }
}
