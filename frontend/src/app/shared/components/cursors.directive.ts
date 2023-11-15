/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, Renderer2 } from '@angular/core';
import { CollaborationService, Subscriptions } from '@app/shared/internal';

@Directive({
    selector: '[sqxCursors]',
    standalone: true,
})
export class CursorsDirective  {
    private readonly subscriptions = new Subscriptions();

    constructor(
        private readonly element: ElementRef<HTMLElement>,
        private readonly renderer: Renderer2,
        private readonly collaboration: CollaborationService,
    ) {
    }

    public ngOnInit() {
        const element = this.element.nativeElement;

        this.subscriptions.add(
            this.renderer.listen('window', 'mousemove', (event: MouseEvent) => {
                const rect = element.getBoundingClientRect();

                const x = event.pageX - rect.left;
                const y = event.pageY - rect.top;

                this.collaboration.updateAwareness('cursor', { x, y });
            }));
    }
}
