/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';

import * as Sortable from 'sortablejs';

@Directive({
    selector: '[sqxSortModel]'
})
export class SortedDirective implements OnDestroy, OnInit {
    private sortable: Sortable.Ref;

    @Input()
    public dragHandle = '.drag-handle';

    @Input('sqxSortModel')
    public sortModel: any[];

    @Output('sqxSorted')
    public sorted = new EventEmitter<any[]>();

    constructor(
        private readonly elementRef: ElementRef
    ) {
    }

    public ngOnDestroy() {
        if (this.sortable) {
            this.sortable.destroy();
        }
    }

    public ngOnInit() {
        this.sortable = Sortable.create(this.elementRef.nativeElement, {
            sort: true,
            animation: 150,

            onSort: (event: { oldIndex: number, newIndex: number }) => {
                if (this.sortModel && event.newIndex !== event.oldIndex) {
                    const newModel = [...this.sortModel];

                    const item = this.sortModel[event.oldIndex];

                    newModel.splice(event.oldIndex, 1);
                    newModel.splice(event.newIndex, 0, item);

                    this.sorted.emit(newModel);
                }
            },

            handle: this.dragHandle
        });
    }
}