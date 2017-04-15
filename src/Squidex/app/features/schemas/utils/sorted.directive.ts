
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, EventEmitter, Output } from '@angular/core';

import {
    SortableComponent,
    SortableContainer,
    DragDropSortableService
} from 'ng2-dnd';

@Directive({
    selector: '[sorted]'
})
export class SortedDirective {
    @Output()
    public sorted = new EventEmitter<Array<any>>();

    constructor(
        sortableComponent: SortableComponent,
        sortableContainer: SortableContainer,
        sortableDragDropService: DragDropSortableService
    ) {
        const oldCallback = sortableComponent._onDropCallback.bind(sortableComponent);

        sortableComponent._onDropCallback = (event: Event) => {
            oldCallback(event);

            if (sortableDragDropService.isDragged) {
                this.sorted.emit(sortableContainer.sortableData);
            }
        };
    }
}
