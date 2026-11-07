/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Directive, input } from '@angular/core';

@Directive({
    selector: '[sqxStopClick]',
    host: {
        '(click)': 'onClick($event)',
    },
})
export class StopClickDirective {
    /** Stops the propagation of click events when true. */
    public readonly shouldStop = input(true, { alias: 'sqxStopClick', transform: booleanAttribute });

    protected onClick(event: Event) {
        if (this.shouldStop()) {
            event.stopPropagation();
            event.stopImmediatePropagation();
        }
    }
}
