/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';

import Sortable = require('sortablejs');

@Directive({
    selector: '[sqxSortModel]'
})
export class SortedDirective implements OnDestroy, OnInit {
    private sortable: Sortable;

    @Input('sqxSortModel')
    public sortModel: any[];

    @Output('sqxSorted')
    public sorted = new EventEmitter<any[]>();

    constructor(
        private readonly elementRef: ElementRef
    ) {
    }

    public ngOnDestroy() {
        this.sortable.destroy();
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
            }
        });
    }
}