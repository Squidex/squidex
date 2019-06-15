/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output } from '@angular/core';

import * as Sortable from 'sortablejs';

const DEFAULT_PROPS = { sort: true, animation: 150 };

@Directive({
    selector: '[sqxSortModel]'
})
export class SortedDirective implements OnChanges, OnDestroy, OnInit {
    private sortable: Sortable.Ref;

    @Input()
    public dragHandle = '.drag-handle';

    @Input('sqxSortModel')
    public sortModel: any[];

    @Output('sqxSort')
    public sort = new EventEmitter<any[]>();

    @Input('sqxSortDisabled')
    public isDisabled = false;

    constructor(
        private readonly elementRef: ElementRef
    ) {
    }

    public ngOnChanges() {
        if (this.sortable) {
            this.sortable.option('disabled', this.isDisabled);
        }
    }

    public ngOnDestroy() {
        if (this.sortable) {
            this.sortable.destroy();
        }
    }

    public ngOnInit() {
        this.sortable = Sortable.create(this.elementRef.nativeElement, {
            ...DEFAULT_PROPS,

            onSort: (event: { oldIndex: number, newIndex: number }) => {
                if (this.sortModel && event.newIndex !== event.oldIndex) {
                    const newModel = [...this.sortModel];

                    const item = this.sortModel[event.oldIndex];

                    newModel.splice(event.oldIndex, 1);
                    newModel.splice(event.newIndex, 0, item);

                    this.sort.emit(newModel);
                }
            },

            handle: this.dragHandle
        });

        this.sortable.option('disabled', this.isDisabled);
    }
}