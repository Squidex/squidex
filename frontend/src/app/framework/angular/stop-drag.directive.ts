/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Directive, HostListener, Input } from '@angular/core';

@Directive({
    selector: '[sqxStopDrag]',
})
export class StopDragDirective {
    @Input({ alias: 'sqxStopDrag', transform: booleanAttribute })
    public shouldStop = true;

    @HostListener('dragstart', ['$event'])
    protected onDragStart(event: Event) {
        const shouldStop: any = this.shouldStop;

        if (shouldStop || shouldStop === '') {
            event.preventDefault();
        }
    }
}
