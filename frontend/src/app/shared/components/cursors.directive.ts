/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, inject } from '@angular/core';
import { CollaborationService } from '@app/shared/internal';

@Directive({
    selector: '[sqxCursors]',
    host: {
        '(window:mousemove)': 'onMouseMove($event)',
    },
})
export class CursorsDirective {
    private readonly collaboration = inject(CollaborationService);
    private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);

    protected onMouseMove(event: MouseEvent) {
        const rect = this.element.nativeElement.getBoundingClientRect();

        const x = event.pageX - rect.left;
        const y = event.pageY - rect.top;

        this.collaboration.updateAwareness('cursor', { x, y });
    }
}
