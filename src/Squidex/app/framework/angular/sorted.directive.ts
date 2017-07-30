
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
    selector: '[sqxSorted]'
})
export class SortedDirective {
    private oldArray: any[];

    @Output('sqxSorted')
    public sorted = new EventEmitter<Array<any>>();

    constructor(
        sortableComponent: SortableComponent,
        sortableContainer: SortableContainer,
        sortableDragDropService: DragDropSortableService
    ) {
        const oldDragStartCallback = sortableComponent._onDragStartCallback.bind(sortableComponent);

        sortableComponent._onDragStartCallback = () => {
            oldDragStartCallback();

            this.oldArray = [...sortableContainer.sortableData];
        };

        const oldDropCallback = sortableComponent._onDropCallback.bind(sortableComponent);

        sortableComponent._onDropCallback = (event: Event) => {
            oldDropCallback(event);

            if (sortableDragDropService.isDragged) {
                const newArray = sortableContainer.sortableData;
                const oldArray = this.oldArray;

                if (newArray && oldArray && newArray.length === oldArray.length) {
                    for (let i = 0; i < oldArray.length; i++) {
                        if (oldArray[i] !== newArray[i]) {
                            this.sorted.emit(newArray);
                            break;
                        }
                    }
                }
            }
        };
    }
}
