/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
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
        private readonly elementRef: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    @HostListener('dragleave', ['$event'])
    public onDragLeave(event: DragDropEvent) {
        this.dragEnd();
    }

    @HostListener('dragenter', ['$event'])
    public onDragEnter(event: DragDropEvent) {
        this.dragStart();
    }

    @HostListener('dragover', ['$event'])
    public onDragOver(event: DragDropEvent) {
        this.tryStopEvent(event);
    }

    @HostListener('drop', ['$event'])
    public onDrop(event: DragDropEvent) {
        this.drop.emit(event.dataTransfer.files);

        this.dragEnd(0);
        this.stopEvent(event);
    }

    private stopEvent(event: Event) {
        event.preventDefault();
        event.stopPropagation();
    }

    private dragStart() {
        this.dragCounter++;

        if (this.dragCounter === 1) {
            this.renderer.setElementClass(this.elementRef.nativeElement, 'drag', true);
        }
    }

    private dragEnd(number?: number ) {
        this.dragCounter = number || this.dragCounter - 1;

        if (this.dragCounter === 0) {
            this.renderer.setElementClass(this.elementRef.nativeElement, 'drag', false);
        }
    }

    private tryStopEvent(event: DragDropEvent) {
        const hasFiles = this.hasFiles(event.dataTransfer.types);

        if (!hasFiles) {
            return;
        }

        this.stopEvent(event);
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
};

interface DragDropEvent extends MouseEvent {
    readonly dataTransfer: DataTransfer;
}