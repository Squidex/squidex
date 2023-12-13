/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Directive, HostListener, Input } from '@angular/core';

@Directive({
    selector: '[sqxStopClick]',
    standalone: true,
})
export class StopClickDirective {
    @Input({ alias: 'sqxStopClick', transform: booleanAttribute })
    public shouldStop: any = true;

    @HostListener('click', ['$event'])
    public onClick(event: Event) {
        const shouldStop: any = this.shouldStop;

        if (shouldStop || shouldStop === '') {
            event.stopPropagation();
            event.stopImmediatePropagation();
        }
    }
}
