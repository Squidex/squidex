
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output, Renderer2 } from '@angular/core';

import * as dragula from 'dragula';

@Directive({
    selector: '[sqxSortModel]'
})
export class SortedDirective implements OnDestroy, OnInit {
    private drake: dragula.Drake;

    @Input('sqxSortModel')
    public sortModel: any[];

    @Input()
    public handleClass: string | null = null;

    @Output('sqxSorted')
    public sorted = new EventEmitter<any[]>();

    constructor(
        private readonly elementRef: ElementRef,
        private readonly renderer: Renderer2
    ) {
    }

    public ngOnDestroy() {
        this.drake.destroy();
    }

    public ngOnInit() {
        const handleClass = this.handleClass;

        this.drake = dragula([this.elementRef.nativeElement], {
            ignoreInputTextSelection: true,

            moves: (element, container, handle: HTMLElement) => {
                if (!handleClass) {
                    return true;
                }

                let current: HTMLElement | null = handle;

                while (current && current !== container) {
                    if (current.classList.contains(handleClass)) {
                        return true;
                    }

                    current = <any>current.parentNode;
                }

                return false;
            }
        });

        let dragIndex: number;
        let dropIndex: number;

        this.drake.on('dragend', (element: any, container: any) => {
            this.renderer.removeClass(element, 'sorting');
        });

        this.drake.on('drag', (element: any, container: any) => {
            this.renderer.addClass(element, 'sorting');

            dragIndex = this.domIndexOf(container, element);
        });

        this.drake.on('drop', (element: any, container: any) => {
            dropIndex = this.domIndexOf(container, element);

            if (this.sortModel && dragIndex !== dropIndex) {
                const newModel = [...this.sortModel];

                const item = this.sortModel[dragIndex];

                newModel.splice(dragIndex, 1);
                newModel.splice(dropIndex, 0, item);

                this.sorted.emit(newModel);
            }
        });
    }

    private domIndexOf(parent: any, child: any): any {
        return Array.prototype.indexOf.call(parent.children, child);
    }
}