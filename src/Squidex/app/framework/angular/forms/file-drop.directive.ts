/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, EventEmitter, HostListener, Output, Renderer } from '@angular/core';

@Directive({
    selector: '[sqxFileDrop]'
})
export class FileDropDirective {
    private dragCounter = 0;

    @Output('sqxFileDrop')
    public drop = new EventEmitter<FileList>();

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    @HostListener('dragend', ['$event'])
    @HostListener('dragleave', ['$event'])
    public onDragEnd(event: DragDropEvent) {
        const hasFiles = this.hasFiles(event.dataTransfer.types);

        if (hasFiles) {
            this.dragEnd();
        }
    }

    @HostListener('dragenter', ['$event'])
    public onDragEnter(event: DragDropEvent) {
        const hasFiles = this.hasFiles(event.dataTransfer.types);

        if (hasFiles) {
            this.dragStart();
        }
    }

    @HostListener('dragover', ['$event'])
    public onDragOver(event: DragDropEvent) {
        const hasFiles = this.hasFiles(event.dataTransfer.types);

        if (hasFiles) {
            this.stopEvent(event);
        }
    }

    @HostListener('drop', ['$event'])
    public onDrop(event: DragDropEvent) {
        const hasFiles = this.hasFiles(event.dataTransfer.types);

        if (hasFiles) {
            this.drop.emit(event.dataTransfer.files);

            this.dragEnd(0);
            this.stopEvent(event);
        }
    }

    private stopEvent(event: Event) {
        event.preventDefault();
        event.stopPropagation();
    }

    private dragStart() {
        this.dragCounter++;

        if (this.dragCounter === 1) {
            this.renderer.setElementClass(this.element.nativeElement, 'drag', true);
        }
    }

    private dragEnd(number?: number ) {
        this.dragCounter = number || this.dragCounter - 1;

        if (this.dragCounter === 0) {
            this.renderer.setElementClass(this.element.nativeElement, 'drag', false);
        }
    }

    private hasFiles(types: any): boolean {
        if (!types) {
            return false;
        }

        if (isFunction(types.indexOf)) {
            return types.indexOf('Files') !== -1;
        } else if (isFunction(types.contains)) {
            return types.contains('Files');
        } else {
            return false;
        }
    }
}

function isFunction(obj: any): boolean {
    return !!(obj && obj.constructor && obj.call && obj.apply);
}

interface DragDropEvent extends MouseEvent {
    readonly dataTransfer: DataTransfer;
}