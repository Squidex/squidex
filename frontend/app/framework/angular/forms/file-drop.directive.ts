/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: prefer-for-of
// tslint:disable: readonly-array

import { Directive, ElementRef, EventEmitter, HostListener, Input, Output, Renderer2 } from '@angular/core';
import { Types } from '@app/framework/internal';

const ImageTypes: ReadonlyArray<string> = [
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
    public allowedFiles: ReadonlyArray<string>;

    @Input('sqxDropOnlyImages')
    public onlyImages: boolean;

    @Input('sqxDropNoPaste')
    public noPaste: boolean;

    @Input('sqxDropDisabled')
    public disabled = false;

    @Output('sqxDropFile')
    public drop = new EventEmitter<ReadonlyArray<File>>();

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer2
    ) {
    }

    @HostListener('paste', ['$event'])
    public async onPaste(event: ClipboardEvent) {
        if (!this.noPaste) {
            this.stopEvent(event);

            const files = await this.getAllowedFiles(event.clipboardData);

            if (files && !this.disabled) {
                this.drop.emit(files);
            }
        }
    }

    @HostListener('dragend', ['$event'])
    @HostListener('dragleave', ['$event'])
    public onDragEnd(event: DragDropEvent) {
        const hasFile = this.hasAllowedFile(event.dataTransfer);

        if (hasFile) {
            this.dragEnd();
        }
    }

    @HostListener('dragenter', ['$event'])
    public onDragEnter(event: DragDropEvent) {
        const hasFile = this.hasAllowedFile(event.dataTransfer);

        if (hasFile) {
            this.dragStart();
        }
    }

    @HostListener('dragover', ['$event'])
    public onDragOver(event: DragDropEvent) {
        const isFiles = hasFiles(event.dataTransfer);

        if (isFiles) {
            this.stopEvent(event);
        }
    }

    @HostListener('drop', ['$event'])
    public async onDrop(event: DragDropEvent) {
        if (hasFiles(event.dataTransfer)) {
            this.stopDrag(event);

            const files = await this.getAllowedFiles(event.dataTransfer);

            if (files && !this.disabled) {
                this.drop.emit(files);
            }
        }
    }

    private stopEvent(event: Event) {
        event.preventDefault();
        event.stopPropagation();
    }

    private stopDrag(event: DragDropEvent) {
        this.dragEnd(0);
        this.stopEvent(event);
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

    private async getAllowedFiles(dataTransfer: DataTransfer | null) {
        if (!dataTransfer || !hasFiles(dataTransfer)) {
            return null;
        }

        let files: File[] = [];

        for (let i = 0; i < dataTransfer.items.length; i++) {
            const item = dataTransfer.items[i];

            await transferFileTree(item, files);
        }

        files = files.filter(f => this.isAllowedFile(f));

        return files.length > 0 ? files : null;
    }

    private hasAllowedFile(dataTransfer: DataTransfer | null) {
        if (!dataTransfer || !hasFiles(dataTransfer)) {
            return null;
        }

        for (let i = 0; i < dataTransfer.files.length; i++) {
            const file = dataTransfer.files.item(i);

            if (file && this.isAllowedFile(file)) {
                return true;
            }
        }

        for (let i = 0; i < dataTransfer.items.length; i++) {
            const file = dataTransfer.items[i];

            if (file && this.isAllowedFile(file)) {
                return true;
            }
        }

        return false;
    }

    private isAllowedFile(file: { type: string }) {
        return this.isAllowed(file) && this.isImage(file);
    }

    private isAllowed(file: { type: string }) {
        return !this.allowedFiles || this.allowedFiles.indexOf(file.type) >= 0;
    }

    private isImage(file: { type: string }) {
        return !this.onlyImages || ImageTypes.indexOf(file.type) >= 0;
    }
}

function hasFiles(dataTransfer: DataTransfer): boolean {
    if (!dataTransfer || !dataTransfer.types) {
        return false;
    }

    const types: any = dataTransfer.types;

    if (Types.isFunction(types.indexOf)) {
        return types.indexOf('Files') !== -1;
    } else if (Types.isFunction(types.contains)) {
        return types.contains('Files');
    } else {
        return false;
    }
}

async function transferWebkitTree(item: any, files: File[]) {
    if (item.isFile) {
        const file = await getFilePromise(item);

        files.push(file);
    } else if (item.isDirectory) {
        const entries = await getFilesPromise(item);

        for (const entry of entries) {
            await transferWebkitTree(entry, files);
        }
    }
}

async function transferFileTree(item: DataTransferItem, files: File[]) {
    if (Types.isFunction(item['webkitGetAsEntry'])) {
        const webkitEntry = item.webkitGetAsEntry();

        if (webkitEntry) {
            await transferWebkitTree(webkitEntry, files);

            return;
        }
    }

    if (Types.isFunction(item['getAsFile'])) {
        const fileItem = item.getAsFile();

        if (fileItem) {
            files.push(fileItem);
        }
    }
}

function getFilesPromise(item: any): Promise<ReadonlyArray<any>> {
    return new Promise((resolve, reject) => {
        try {
            const reader = item.createReader();

            reader.readEntries(resolve);
        } catch (ex) {
            reject(ex);
        }
    });
}

function getFilePromise(item: any): Promise<File> {
    return new Promise((resolve, reject) => {
        try {
            item.file(resolve);
        } catch (ex) {
            reject(ex);
        }
    });
}

interface DragDropEvent extends MouseEvent {
    readonly dataTransfer: DataTransfer;
}