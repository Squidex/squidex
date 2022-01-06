/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, HostListener, Input } from '@angular/core';

@Directive({
    selector: '[sqxStopDrag]',
})
export class StopDragDirective {
    @Input('sqxStopDrag')
    public shouldStop: any = true;

    @HostListener('dragstart', ['$event'])
    public onDragStart(event: Event) {
        const shouldStop: any = this.shouldStop;

        if (shouldStop || shouldStop === '') {
            event.preventDefault();
        }
    }
}
