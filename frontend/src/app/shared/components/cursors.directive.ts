/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, effect, ElementRef, inject, Renderer2 } from '@angular/core';
import { CollaborationService } from '@app/shared/internal';

@Directive({
    selector: '[sqxCursors]',
})
export class CursorsDirective {
    private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);
    private readonly renderer = inject(Renderer2);
    private readonly collaboration = inject(CollaborationService);

    constructor() {
        effect(onCleanup => {
            const element = this.element.nativeElement;

            const unlisten = this.renderer.listen('window', 'mousemove', (event: MouseEvent) => {
                const rect = element.getBoundingClientRect();

                const x = event.pageX - rect.left;
                const y = event.pageY - rect.top;

                this.collaboration.updateAwareness('cursor', { x, y });
            });

            onCleanup(unlisten);
        });
    }
}
