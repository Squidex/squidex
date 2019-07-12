/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:prefer-for-of

import { Directive, ElementRef, EventEmitter, HostListener, Input, Output, Renderer2 } from '@angular/core';

import { Types } from '@app/framework/internal';

const ImageTypes = [
    'image/jpeg',
    'image/png',
    'image/jpg',
    'image/gif'
];

@Directive({
    selector: '[sqxDropFile]'
})
export class FileDropDirective {
    private dragCounter = 0;

    @Input()
    public allowedFiles: string[];

    @Input()
    public onlyImages: boolean;

    @Input()
    public noDrop: boolean;

    @Input('sqxDropDisabled')
    public disabled = false;

    @Output('sqxDropFile')
    public drop = new EventEmitter<File[]>();

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer2
    ) {
    }

    @HostListener('paste', ['$event'])
    public onPaste(event: ClipboardEvent) {
        if (this.noDrop) {
            return;
        }

        const result: File[] = [];

        if (event.clipboardData) {
            for (let i = 0; i < event.clipboardData.items.length; i++) {
                const file = event.clipboardData.items[i].getAsFile();

                if (this.isAllowedFile(file)) {
                    result.push(file!);
                }
            }
        }

        if (result.length > 0 && !this.disabled) {
            this.drop.emit(result);
        }

        this.stopEvent(event);
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
            const result: File[] = [];

            for (let i = 0; i < event.dataTransfer.files.length; i++) {
                const file = event.dataTransfer.files.item(i);

                if (this.isAllowedFile(file)) {
                    result.push(file!);
                }
            }

            if (result.length > 0) {
                this.drop.emit(result);
            }

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

        if (this.dragCounter === 1 && !this.disabled) {
            this.renderer.addClass(this.element.nativeElement, 'drag');
        }
    }

    private dragEnd(number?: number ) {
        this.dragCounter = number || this.dragCounter - 1;

        if (this.dragCounter === 0 && !this.disabled) {
            this.renderer.removeClass(this.element.nativeElement, 'drag');
        }
    }

    private isAllowedFile(file: File | null) {
        return file && (!this.allowedFiles || this.allowedFiles.indexOf(file.type) >= 0) && (!this.onlyImages || ImageTypes.indexOf(file.type) >= 0);
    }

    private hasFiles(types: any): boolean {
        if (!types) {
            return false;
        }

        if (Types.isFunction(types.indexOf)) {
            return types.indexOf('Files') !== -1;
        } else if (Types.isFunction(types.contains)) {
            return types.contains('Files');
        } else {
            return false;
        }
    }
}

interface DragDropEvent extends MouseEvent {
    readonly dataTransfer: DataTransfer;
}