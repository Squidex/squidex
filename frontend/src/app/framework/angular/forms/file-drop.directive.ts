/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-input-rename */

import { booleanAttribute, Directive, ElementRef, inject, input, output, Renderer2 } from '@angular/core';
import { Types } from '@app/framework/internal';

@Directive({
    selector: '[sqxDropFile]',
    host: {
        '(paste)': 'onPaste($event)',
        '(dragend)': 'onDragEnd($event)',
        '(dragleave)': 'onDragEnd($event)',
        '(dragenter)': 'onDragEnter($event)',
        '(dragover)': 'onDragOver($event)',
        '(drop)': 'onDrop($event)',
    },
})
export class FileDropDirective {
    private readonly element = inject(ElementRef);
    private readonly renderer = inject(Renderer2);
    private dragCounter = 0;

    /** The MIME types of the files that are accepted. All files are accepted if not defined. */
    public readonly allowedFiles = input<ReadonlyArray<string>>();

    /** Only accepts image files. */
    public readonly onlyImages = input(false, { alias: 'sqxDropOnlyImages', transform: booleanAttribute });

    /** Does not accept files that are pasted from the clipboard. */
    public readonly noPaste = input(false, { alias: 'sqxDropNoPaste', transform: booleanAttribute });

    /** Ignores dropped and pasted files. */
    public readonly disabled = input(false, { alias: 'sqxDropDisabled', transform: booleanAttribute });

    /** Emits the accepted files when they are dropped or pasted. */
    public readonly fileDrop = output<ReadonlyArray<File>>({ alias: 'sqxDropFile' });

    protected async onPaste(event: ClipboardEvent) {
        if (!this.noPaste()) {
            this.stopEvent(event);

            const files = await this.getAllowedFiles(event.clipboardData);

            if (files && !this.disabled()) {
                this.fileDrop.emit(files);
            }
        }
    }

    protected onDragEnd(event: DragEvent) {
        const hasFile = this.hasAllowedFile(event.dataTransfer);

        if (hasFile) {
            this.dragEnd();
        }
    }

    protected onDragEnter(event: DragEvent) {
        const hasFile = this.hasAllowedFile(event.dataTransfer);

        if (hasFile) {
            this.dragStart();
        }
    }

    protected onDragOver(event: DragEvent) {
        const isFiles = hasFiles(event.dataTransfer);

        if (isFiles) {
            this.stopEvent(event);
        }
    }

    protected async onDrop(event: DragEvent) {
        if (hasFiles(event.dataTransfer)) {
            this.stopDrag(event);

            const files = await this.getAllowedFiles(event.dataTransfer);

            if (files && !this.disabled()) {
                this.fileDrop.emit(files);
            }
        }
    }

    private stopEvent(event: Event) {
        event.preventDefault();
        event.stopPropagation();
    }

    private stopDrag(event: DragEvent) {
        this.dragEnd(0);
        this.stopEvent(event);
    }

    private dragStart() {
        this.dragCounter++;

        if (this.dragCounter === 1 && !this.disabled()) {
            this.renderer.addClass(this.element.nativeElement, 'drag');
        }
    }

    private dragEnd(number?: number) {
        this.dragCounter = number || this.dragCounter - 1;

        if (this.dragCounter === 0 && !this.disabled()) {
            this.renderer.removeClass(this.element.nativeElement, 'drag');
        }
    }

    private async getAllowedFiles(dataTransfer: DataTransfer | null) {
        if (!dataTransfer || !hasFiles(dataTransfer)) {
            return null;
        }

        const files: File[] = [];
        const items = getItems(dataTransfer);

        // Loop over files first, otherwise Chromes deletes them in the async call.
        for (const item of items) {
            const file = item.getAsFile();

            if (file && this.isAllowedFile(file)) {
                files.push(file);
            }
        }

        for (const item of items) {
            if (Types.isFunction(item['webkitGetAsEntry'])) {
                const webkitEntry = item.webkitGetAsEntry();

                if (webkitEntry && webkitEntry.isDirectory) {

                    await this.traverseWebkitTree(webkitEntry, files);
                }
            }
        }

        if (files.length === 0) {
            return null;
        }

        return files;
    }

    private async traverseWebkitTree(item: any, files: File[]) {
        if (item.isFile) {
            const file = await getFilePromise(item);

            if (file && this.isAllowedFile(file)) {
                files.push(file);
            }
        } else if (item.isDirectory) {
            const entries = await getFilesPromise(item);

            for (const entry of entries) {

                await this.traverseWebkitTree(entry, files);
            }
        }
    }

    private hasAllowedFile(dataTransfer: DataTransfer | null) {
        if (!dataTransfer || !hasFiles(dataTransfer)) {
            return null;
        }

        for (const file of getFiles(dataTransfer)) {
            if (this.isAllowedFile(file)) {
                return true;
            }
        }

        for (const item of getItems(dataTransfer)) {
            if (this.isAllowedFile(item)) {
                return true;
            }
        }

        return false;
    }

    private isAllowedFile(file: { type: string }) {
        return this.isAllowed(file) && this.isImage(file);
    }

    private isAllowed(file: { type: string }) {
        const allowedFiles = this.allowedFiles();

        return !allowedFiles || allowedFiles.includes(file.type);
    }

    private isImage(file: { type: string }) {
        return !this.onlyImages() || file.type.indexOf('image/') === 0;
    }
}

function hasFiles(dataTransfer: DataTransfer | null): boolean {
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

function getItems(dataTransfer: DataTransfer) {
    const result: DataTransferItem[] = [];

    if (dataTransfer.files) {
        for (let i = 0; i < dataTransfer.items.length; i++) {
            const item = dataTransfer.items[i];

            if (item) {
                result.push(item);
            }
        }
    }

    return result;
}

function getFiles(dataTransfer: DataTransfer) {
    const result: File[] = [];

    if (dataTransfer.files) {
        for (let i = 0; i < dataTransfer.files.length; i++) {
            const file = dataTransfer.files[i];

            if (file) {
                result.push(file);
            }
        }
    }

    return result;
}