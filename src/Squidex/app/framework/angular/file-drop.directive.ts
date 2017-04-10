/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, EventEmitter, HostListener, Output } from '@angular/core';

@Directive({
    selector: '[sqxFileDrop]'
})
export class FileDropDirective {
    @Output('sqxFileDrop')
    public drop = new EventEmitter<FileList>();

    @HostListener('dragenter', ['$event'])
    public onDragEnter(event: DragDropEvent) {
        this.tryStopEvent(event);
    }

    @HostListener('dragover', ['$event'])
    public onDragOver(event: DragDropEvent) {
        this.tryStopEvent(event);
    }

    @HostListener('drop', ['$event'])
    public onDrop(event: DragDropEvent) {
        this.drop.emit(event.dataTransfer.files);

        this.stopEvent(event);
    }

    private stopEvent(event: Event) {
        event.preventDefault();
        event.stopPropagation();
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